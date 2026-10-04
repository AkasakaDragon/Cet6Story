using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Drawing;
using System.Windows.Forms;

public static class StoryAudioSpeed {
 public static double Normalize(double value){return value==.5||value==1.25||value==1.5?value:1.0;}
 public static string Prepare(string source,double speed,string root){
  speed=Normalize(speed);if(speed==1)return source;
  var info=new FileInfo(source);string key;using(var hash=SHA256.Create())key=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(source+"|"+info.Length+"|"+info.LastWriteTimeUtc.Ticks+"|tempo|"+speed.ToString(CultureInfo.InvariantCulture)))).Replace("-","").Substring(0,12).ToLowerInvariant();
  var folder=Path.Combine(root,".audio-cache");Directory.CreateDirectory(folder);var target=Path.Combine(folder,"s"+key+".wav");if(File.Exists(target))return target;
  string temporary=target+".tmp.wav";
  var start=new ProcessStartInfo(Path.Combine(root,"tools","ffmpeg","ffmpeg.exe"),"-nostdin -hide_banner -loglevel error -y -i \""+source+"\" -vn -af atempo="+speed.ToString(CultureInfo.InvariantCulture)+" -c:a pcm_s16le \""+temporary+"\""){UseShellExecute=false,CreateNoWindow=true,RedirectStandardError=true};
  using(var process=Process.Start(start)){string error=process.StandardError.ReadToEnd();process.WaitForExit();if(process.ExitCode!=0){if(File.Exists(temporary))File.Delete(temporary);throw new IOException("倍速音频生成失败："+error);}}
  File.Move(temporary,target);return target;
 }
}

public partial class Game {
 long storyEndMs; VNButton speedButton;
 long SpeedTime(double seconds){return (long)Math.Round(seconds*1000/save.storySpeed);}
 string StoryAudioFile(){return String.IsNullOrWhiteSpace(current.audio)?"":AudioVolume.Prepare(StoryAudioSpeed.Prepare(Engine.SafePath(folders[current.id],current.audio),save.storySpeed,root),StorySoundVolume(),root);}
 void AddSpeedControl(FlowLayoutPanel bar){
  speedButton=Mini(save.storySpeed.ToString("0.0#",CultureInfo.InvariantCulture)+"x","","点击切换下一个倍速：0.5x → 1.0x → 1.25x → 1.5x",()=>{double[] rates={.5,1.0,1.25,1.5};int position=Array.IndexOf(rates,save.storySpeed);ChangeStorySpeed(rates[(position+1)%rates.Length]);});speedButton.Width=66;bar.Controls.Add(speedButton);
 }
 void ChangeStorySpeed(double rate){
  rate=StoryAudioSpeed.Normalize(rate);if(rate==save.storySpeed)return;
  bool playing=originalPlaying,paused=pausedAudio;double sourceMs=current.lines[index].start*1000;var position=new StringBuilder(64);long ms;
  if(playing&&mciSendString("status storyaudio position",position,64,IntPtr.Zero)==0&&long.TryParse(position.ToString(),out ms))sourceMs=ms*save.storySpeed;
  double previous=save.storySpeed;string prepared;
  try{Cursor=Cursors.WaitCursor;prepared=String.IsNullOrWhiteSpace(current.audio)?"":AudioVolume.Prepare(StoryAudioSpeed.Prepare(Engine.SafePath(folders[current.id],current.audio),rate,root),StorySoundVolume(),root);}
  catch(Exception ex){GameMessage.Show(this,ex.Message,"播放倍速");return;}finally{Cursor=Cursors.Default;}
  // Read the latest position after preparing an imported audio file.
  position.Clear();if(playing&&mciSendString("status storyaudio position",position,64,IntPtr.Zero)==0&&long.TryParse(position.ToString(),out ms))sourceMs=ms*previous;
  save.storySpeed=rate;Persist();speedButton.Text=rate.ToString("0.0#",CultureInfo.InvariantCulture)+"x";speedButton.Invalidate();
  if(audioPath!=""){audioTimer.Stop();mciSendString("close storyaudio",null,0,IntPtr.Zero);audioReady=false;PrepareAudio();if(playing&&audioReady){int error=mciSendString("play storyaudio from "+(long)(sourceMs/rate)+" to "+(long)(storyEndMs/rate),null,0,IntPtr.Zero);if(error==0){if(paused)mciSendString("pause storyaudio",null,0,IntPtr.Zero);else audioTimer.Start();}else{StopAudio();SetStatus("倍速切换失败，请重新播放当前句。");}}}
  if(speech!=null)speech.Rate=(int)Math.Round(Math.Log(rate,2)*5)-1;
 }
}
