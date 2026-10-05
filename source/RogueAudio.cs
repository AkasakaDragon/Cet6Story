using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Windows.Forms;
using System.Threading;

// Mix music and overlapping effects locally; never change the system volume.
public sealed class RogueAudioMixer:IDisposable {
 [StructLayout(LayoutKind.Sequential,Pack=2)] struct Format{public ushort tag,channels;public uint rate,bytes;public ushort align,bits,extra;}
 [StructLayout(LayoutKind.Sequential)] struct Header{public IntPtr data;public uint length,recorded;public UIntPtr user;public uint flags,loops;public IntPtr next;public UIntPtr reserved;}
 [DllImport("winmm.dll")] static extern uint waveOutOpen(out IntPtr handle,uint device,ref Format format,IntPtr callback,IntPtr instance,uint flags);
 [DllImport("winmm.dll")] static extern uint waveOutPrepareHeader(IntPtr handle,IntPtr header,uint size);
 [DllImport("winmm.dll")] static extern uint waveOutUnprepareHeader(IntPtr handle,IntPtr header,uint size);
 [DllImport("winmm.dll")] static extern uint waveOutWrite(IntPtr handle,IntPtr header,uint size);
 [DllImport("winmm.dll")] static extern uint waveOutReset(IntPtr handle);
 [DllImport("winmm.dll")] static extern uint waveOutClose(IntPtr handle);
 class Buffer{public IntPtr data,header;public bool prepared,queued;public readonly short[] output=new short[Samples];}
 class Voice{public short[] clip;public int position;}
 readonly short[][] castingEffects={CastingAudio.Build(0),CastingAudio.Build(1),CastingAudio.Build(2)};
 readonly Dictionary<string,short[]> monsterEffects=new Dictionary<string,short[]>();IntPtr device;readonly List<Buffer> buffers=new List<Buffer>();readonly List<Voice> voices=new List<Voice>();readonly short[] music,menuMusic,attack,hurt,purchaseSuccess,purchaseFailed,coinGain,cardHover,cardPlay,menuClick;int position;int musicTrack;double gain;volatile bool active,disposed;const int Samples=2048;readonly object audioGate=new object();Thread worker;volatile string audioError;long submittedBuffers;
 public volatile int MasterVolume=100,MusicVolume=70,EffectsVolume=85;public volatile bool Ducked;public bool Active{get{return active;}}public int ActiveEffects{get{lock(audioGate)return voices.Count;}}public string Error{get{return audioError;}}public long SubmittedBuffers{get{return Interlocked.Read(ref submittedBuffers);}}
 public RogueAudioMixer(string folder){
  music=Read(Path.Combine(folder,"forest-loop.wav"));menuMusic=Read(Path.Combine(folder,"menu-orbit.wav"));attack=Read(Path.Combine(folder,"attack.wav"));hurt=Read(Path.Combine(folder,"hurt.wav"));purchaseSuccess=Read(Path.Combine(folder,"purchase-success.wav"));purchaseFailed=Read(Path.Combine(folder,"purchase-failed.wav"));
  for(int profile=0;profile<MonsterAudio.Profiles.Length;profile++)for(int effect=0;effect<MonsterAudio.Events.Length;effect++){
   string key=MonsterAudio.Profiles[profile]+"/"+MonsterAudio.Events[effect],path=Path.Combine(folder,"monsters",MonsterAudio.Profiles[profile]+"-"+MonsterAudio.Events[effect]+".wav");
   monsterEffects[key]=File.Exists(path)?Read(path):MonsterAudio.Build(profile,effect);
  }
  menuClick=MenuClickAudio.Build();cardHover=CardInteractionAudio.Build(false);cardPlay=CardInteractionAudio.Build(true);
  string coinPath=Path.Combine(folder,"coin-gain.wav");coinGain=File.Exists(coinPath)?Read(coinPath):CurrencyAudio.Build();
  var format=new Format{tag=1,channels=2,rate=44100,bytes=176400,align=4,bits=16};
  if(waveOutOpen(out device,UInt32.MaxValue,ref format,IntPtr.Zero,IntPtr.Zero,0)!=0)throw new Exception("无法打开音频输出设备。");
  try{for(int i=0;i<4;i++){var b=new Buffer{data=Marshal.AllocHGlobal(Samples*2),header=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Header)))};buffers.Add(b);Marshal.StructureToPtr(new Header{data=b.data,length=Samples*2},b.header,false);if(waveOutPrepareHeader(device,b.header,(uint)Marshal.SizeOf(typeof(Header)))!=0)throw new Exception("无法准备音频缓冲区。");b.prepared=true;}}catch{Dispose();throw;}
  worker=new Thread(FeedAudio){IsBackground=true,Name="Rogue audio feed",Priority=ThreadPriority.AboveNormal};worker.Start();
 }
 public static short[] Read(string path){using(var reader=new BinaryReader(File.OpenRead(path))){if(new string(reader.ReadChars(4))!="RIFF")throw new Exception("音频不是 WAV 文件。");reader.ReadUInt32();if(new string(reader.ReadChars(4))!="WAVE")throw new Exception("音频格式无效。");bool valid=false;while(reader.BaseStream.Position+8<=reader.BaseStream.Length){string tag=new string(reader.ReadChars(4));uint size=reader.ReadUInt32();long end=reader.BaseStream.Position+size;if(end>reader.BaseStream.Length)throw new Exception("音频文件不完整。");if(tag=="fmt "&&size>=16){valid=reader.ReadUInt16()==1&&reader.ReadUInt16()==2&&reader.ReadUInt32()==44100;reader.ReadUInt32();reader.ReadUInt16();valid=reader.ReadUInt16()==16&&valid;}if(tag=="data"){if(!valid||size==0||size%4!=0)throw new Exception("需要 44.1 kHz 双声道 16 位 PCM 音频。");byte[] bytes=reader.ReadBytes((int)size);short[] result=new short[bytes.Length/2];System.Buffer.BlockCopy(bytes,0,result,0,bytes.Length);return result;}reader.BaseStream.Position=end+size%2;}throw new Exception("音频缺少数据。");}}
 // Feed the device independently of WinForms painting, resizing and modal dialogs.
 void FeedAudio(){while(!disposed){try{Pump();}catch(Exception ex){audioError=ex.Message;return;}Thread.Sleep(8);}}
 public void SetMenuMusic(bool value){SetMusicTrack(value?1:0);}
 public void SetMusicTrack(int value){lock(audioGate){if(musicTrack==value)return;musicTrack=value;position=0;gain=0;if(active){waveOutReset(device);foreach(var b in buffers)b.queued=false;}}}
 public void SetActive(bool value){lock(audioGate){if(disposed||active==value)return;active=value;if(!value){waveOutReset(device);foreach(var b in buffers)b.queued=false;voices.Clear();Ducked=false;gain=0;}}}
 public void Effect(bool damageToPlayer){lock(audioGate){if(!disposed&&active&&EffectsVolume>0&&MasterVolume>0){if(voices.Count>=8)voices.RemoveAt(0);voices.Add(new Voice{clip=damageToPlayer?hurt:attack});}}}

 public void MonsterEffect(string key){lock(audioGate){short[] clip;if(!disposed&&active&&EffectsVolume>0&&MasterVolume>0&&monsterEffects.TryGetValue(key,out clip)){if(voices.Count>=8)voices.RemoveAt(0);voices.Add(new Voice{clip=clip});}}}
 public void CastingEffect(int style){lock(audioGate){if(!disposed&&active&&EffectsVolume>0&&MasterVolume>0){if(voices.Count>=8)voices.RemoveAt(0);voices.Add(new Voice{clip=castingEffects[Math.Max(0,Math.Min(2,style))]});}}}
 public void HandEffect(bool play){lock(audioGate){if(!disposed&&active&&EffectsVolume>0&&MasterVolume>0){if(voices.Count>=8)voices.RemoveAt(0);voices.Add(new Voice{clip=play?cardPlay:cardHover});}}}
 public void MenuClickEffect(){lock(audioGate){if(!disposed&&active&&EffectsVolume>0&&MasterVolume>0){if(voices.Count>=8)voices.RemoveAt(0);voices.Add(new Voice{clip=menuClick});}}}
 public void CoinEffect(){lock(audioGate){if(!disposed&&active&&EffectsVolume>0&&MasterVolume>0){if(voices.Count>=8)voices.RemoveAt(0);voices.Add(new Voice{clip=coinGain});}}}
 public void ShopEffect(bool success){lock(audioGate){if(!disposed&&active&&EffectsVolume>0&&MasterVolume>0){if(voices.Count>=8)voices.RemoveAt(0);voices.Add(new Voice{clip=success?purchaseSuccess:purchaseFailed});}}}
 public void Pump(){lock(audioGate)PumpLocked();}
 void PumpLocked(){if(disposed||!active)return;foreach(var b in buffers){var h=(Header)Marshal.PtrToStructure(b.header,typeof(Header));if(b.queued&&(h.flags&1)==0)continue;short[] output=b.output;short[] track=musicTrack==1?menuMusic:music;double target=Math.Max(0,Math.Min(100,MusicVolume))/100.0*Math.Max(0,Math.Min(100,MasterVolume))/100.0*(Ducked?.2:1),fx=Math.Max(0,Math.Min(100,EffectsVolume))/100.0*Math.Max(0,Math.Min(100,MasterVolume))/100.0;
   if(target==0)gain=0;for(int i=0;i<Samples;i+=2){gain+=(target-gain)*(musicTrack==1?.000012:.00004);for(int channel=0;channel<2;channel++){double value=track[(position+channel)%track.Length]*gain;foreach(var voice in voices)if(voice.position+channel<voice.clip.Length)value+=voice.clip[voice.position+channel]*fx;output[i+channel]=(short)Math.Max(short.MinValue,Math.Min(short.MaxValue,value));}position=(position+2)%track.Length;foreach(var voice in voices)voice.position+=2;voices.RemoveAll(v=>v.position>=v.clip.Length);}
   Marshal.Copy(output,0,b.data,output.Length);if(waveOutWrite(device,b.header,(uint)Marshal.SizeOf(typeof(Header)))!=0)throw new Exception("音频播放失败。");b.queued=true;Interlocked.Increment(ref submittedBuffers);
  }}
 public void Dispose(){lock(audioGate){if(disposed)return;disposed=true;active=false;}if(worker!=null&&worker!=Thread.CurrentThread)worker.Join();lock(audioGate){if(device!=IntPtr.Zero)waveOutReset(device);foreach(var b in buffers){if(b.prepared)waveOutUnprepareHeader(device,b.header,(uint)Marshal.SizeOf(typeof(Header)));if(b.header!=IntPtr.Zero)Marshal.FreeHGlobal(b.header);if(b.data!=IntPtr.Zero)Marshal.FreeHGlobal(b.data);}buffers.Clear();voices.Clear();if(device!=IntPtr.Zero)waveOutClose(device);device=IntPtr.Zero;}}

}

public partial class Game {
 int observedCoins=-1;RogueAudioMixer rogueAudio;System.Windows.Forms.Timer rogueAudioClock;bool rogueAudioFailed;DateTime wordDuckUntil;
 bool RogueAudioPage(){return page=="rogue"||page=="prep-home"||page=="prep-intro"||page=="prep-combat";}
 void InitRogueAudio(){observedCoins=save.rogue.coins;rogueAudioClock=new System.Windows.Forms.Timer{Interval=20};rogueAudioClock.Tick+=(s,e)=>UpdateRogueAudio();rogueAudioClock.Start();}
 void UpdateRogueAudio(){if(rogueAudioFailed)return;try{bool menu=page=="home"||page=="settings";bool enabled=menu||RogueAudioPage()||page=="system-shop";if(rogueAudio==null&&enabled)rogueAudio=new RogueAudioMixer(Path.Combine(root,"assets","rogue","audio"));if(rogueAudio==null)return;if(rogueAudio.Error!=null)throw new Exception(rogueAudio.Error);rogueAudio.SetMenuMusic(menu);rogueAudio.MasterVolume=MasterSoundVolume();rogueAudio.MusicVolume=page=="system-shop"?0:save.rogueMusicVolume;rogueAudio.EffectsVolume=save.rogueEffectsVolume;rogueAudio.SetActive(enabled);if(enabled){var mode=new StringBuilder(32);mciSendString("status wordaudio mode",mode,mode.Capacity,IntPtr.Zero);rogueAudio.Ducked=DateTime.UtcNow<wordDuckUntil||mode.ToString().Trim()=="playing"||(speech!=null&&speech.State==SynthesizerState.Speaking);}}catch(Exception ex){rogueAudioFailed=true;if(rogueAudio!=null){rogueAudio.Dispose();rogueAudio=null;}SetStatus("游戏音频不可用："+ex.Message);}}
 void PlayMenuClick(){UpdateRogueAudio();if(rogueAudio!=null)rogueAudio.MenuClickEffect();}
 void PlayShopPurchase(bool success){if(page!="system-shop")return;UpdateRogueAudio();if(rogueAudio!=null)rogueAudio.ShopEffect(success);}
 void PlayRogueHit(bool hurt){if(!RogueAudioPage())return;UpdateRogueAudio();if(rogueAudio!=null)rogueAudio.Effect(hurt);}
 void ObserveCoinGain(){int currentCoins=save.rogue.coins;bool gained=observedCoins>=0&&currentCoins>observedCoins;observedCoins=currentCoins;if(gained){UpdateRogueAudio();if(rogueAudio!=null)rogueAudio.CoinEffect();}}
 void PlayCastingSound(int style){if(!RogueAudioPage())return;UpdateRogueAudio();if(rogueAudio!=null)rogueAudio.CastingEffect(style);}
 void PlayHandSound(bool play){if(!RogueAudioPage())return;UpdateRogueAudio();if(rogueAudio!=null)rogueAudio.HandEffect(play);}
 void PlayMonsterSound(string key){if(!RogueAudioPage())return;UpdateRogueAudio();if(rogueAudio!=null)rogueAudio.MonsterEffect(key);}
 void CloseRogueAudio(){if(rogueAudioClock!=null)rogueAudioClock.Dispose();if(rogueAudio!=null){rogueAudio.Dispose();rogueAudio=null;}}
}
