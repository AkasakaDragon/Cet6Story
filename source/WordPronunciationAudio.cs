using System;using System.IO;using System.Text;using System.Security.Cryptography;using System.Speech.Synthesis;

public static class WordPronunciationAudio {
 public static string Prepare(string clip,string text,string voice,string root){
  string folder=Path.Combine(root,".audio-cache");Directory.CreateDirectory(folder);
  string identity="word-louder-v1|"+text+"|"+voice;
  if(File.Exists(clip)){var info=new FileInfo(clip);identity+="|"+clip+"|"+info.Length+"|"+info.LastWriteTimeUtc.Ticks;}
  string key;using(var hash=SHA256.Create())key=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(identity))).Replace("-","");
  string target=Path.Combine(folder,"word-"+key+".wav");if(File.Exists(target))return target;
  byte[] data;
  if(File.Exists(clip))data=File.ReadAllBytes(clip);
  else using(var stream=new MemoryStream())using(var synth=new SpeechSynthesizer()){
   synth.SelectVoice(voice);synth.Volume=100;synth.Rate=-1;synth.SetOutputToWaveStream(stream);synth.Speak(text);data=stream.ToArray();
  }
  using(var stream=new MemoryStream(data))using(var reader=new BinaryReader(stream)){
   if(new string(reader.ReadChars(4))!="RIFF")throw new InvalidDataException();reader.ReadInt32();if(new string(reader.ReadChars(4))!="WAVE")throw new InvalidDataException();
   int format=0,bits=0;while(stream.Position+8<=stream.Length){string chunk=new string(reader.ReadChars(4));int size=reader.ReadInt32();long start=stream.Position;if(size<0||start+size>stream.Length)throw new InvalidDataException();
    if(chunk=="fmt "&&size>=16){format=reader.ReadUInt16();stream.Position+=12;bits=reader.ReadUInt16();}
    if(chunk=="data"){
     if(format!=1||bits!=16)throw new InvalidDataException();int peak=0;
     for(long i=start;i+1<start+size;i+=2)peak=Math.Max(peak,Math.Abs((int)(short)(data[i]|data[i+1]<<8)));
     double gain=peak==0?1:Math.Min(3.0,29490.0/peak);gain=Math.Max(1,gain);
     for(long i=start;i+1<start+size;i+=2){int sample=(int)Math.Round((short)(data[i]|data[i+1]<<8)*gain);sample=Math.Max(-32768,Math.Min(32767,sample));data[i]=(byte)sample;data[i+1]=(byte)(sample>>8);}
     break;
    }
    stream.Position=start+size+(size%2);
   }
  }
  File.WriteAllBytes(target,data);return target;
 }
}
