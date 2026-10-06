using System;
using System.IO;
using System.Linq;

public static class MenuWoodAudioChecks {
 static double Energy(short[] samples,int start,int end){double sum=0;for(int i=start;i<end;i++)sum+=(double)samples[i]*samples[i];return Math.Sqrt(sum/Math.Max(1,end-start));}
 static void Check(short[] samples){
  if(samples.Length%2!=0||samples.Length<4410)throw new Exception("Invalid stereo duration");
  int peak=samples.Max(s=>Math.Abs((int)s));if(peak<1000||peak>=32767)throw new Exception("Silent or clipped effect");
  for(int i=0;i<samples.Length;i+=2)if(samples[i]!=samples[i+1])throw new Exception("Stereo imbalance");
  if(Energy(samples,0,samples.Length/3)<Energy(samples,samples.Length*2/3,samples.Length)*10)throw new Exception("Wood strike must decay quickly");
 }
 static void Write(string path,short[] samples){using(var w=new BinaryWriter(File.Create(path))){int bytes=samples.Length*2;w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+bytes);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)2);w.Write(44100);w.Write(176400);w.Write((short)4);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(bytes);foreach(short value in samples)w.Write(value);}}
 public static void Main(string[] args){var hover=MenuClickAudio.BuildWood(false);var click=MenuClickAudio.BuildWood(true);Check(hover);Check(click);if(hover.SequenceEqual(click)||Energy(click,0,4410)<=Energy(hover,0,4410))throw new Exception("Click must be distinct and stronger than hover");Directory.CreateDirectory(args[0]);Write(Path.Combine(args[0],"wood-hover.wav"),hover);Write(Path.Combine(args[0],"wood-click.wav"),click);var paper=PageTurnAudio.Build();if(paper.Length<44100/2||paper.Max(s=>Math.Abs((int)s))<1000||paper.Any(s=>Math.Abs((int)s)>=32767)||paper.SequenceEqual(click))throw new Exception("Invalid paper page turn");if(Energy(paper,0,882)<Energy(paper,paper.Length/3,paper.Length*2/3)/10&&Energy(paper,paper.Length-882,paper.Length)<Energy(paper,paper.Length/3,paper.Length*2/3)/10){}else throw new Exception("Page rustle must fade at both ends");Write(Path.Combine(args[0],"page-turn.wav"),paper);Console.WriteLine("PASS: distinct wood taps and paper page rustle, faded ends, no silence or clipping.");}
}
