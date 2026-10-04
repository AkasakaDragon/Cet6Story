using System;
using System.IO;

// Original procedural sounds: no external samples, stereo PCM compatible with the local mixer.
public static class MonsterAudio {
 public static readonly string[] Profiles={"wood","spore","stone","echo","ember","armor","ancient-tree","sealed-knight","lava-troll","gatekeeper"};
 public static readonly string[] Events={"normal","hurt","skill-a","skill-b"};
 public static short[] Build(int profile,int action){
  if(profile<0||profile>=Profiles.Length||action<0||action>=Events.Length)throw new ArgumentOutOfRangeException();
  int family=profile<6?profile:profile==6?0:profile==7?2:profile==8?5:3;
  double duration=action==1?.30:action==0?.52:action==2?.78:1.02;
  int count=(int)(44100*duration);var result=new short[count*2];var random=new Random(7919+profile*101+action*977);
  double filtered=0,phase=0;double[] frequencies={145,310,76,460,110,185};
  double pitch=frequencies[family]*(profile>=6?.68:1)*(1+profile*.017);
  for(int i=0;i<count;i++){
   double t=i/44100.0,u=t/duration,noise=random.NextDouble()*2-1;filtered=filtered*.83+noise*.17;
   double envelope=Math.Min(1,t/.012)*Math.Pow(1-u,action==1?2.7:1.5);
   double sweep=action==2?.70+u*.75:action==3?1.6-u*.8:1.25-u*.6;phase+=2*Math.PI*pitch*sweep/44100;
   double pulse=1;
   if(family==1)pulse=.35+.65*Math.Pow(Math.Max(0,Math.Sin(t*(action==3?64:40))),4);
   if(action==3&&family!=1)pulse=.45+.55*Math.Pow(Math.Sin(t*(profile+11)),2);
   double signal;
   switch(family){
    case 0:signal=.42*filtered+.22*noise*Math.Exp(-t*22)+.33*Math.Sin(phase)*Math.Exp(-t*4)+.12*Math.Sin(phase*2.13);break;
    case 1:signal=.36*Math.Sin(phase)+.29*noise*pulse+.22*Math.Sin(phase*3.27)*pulse;break;
    case 2:signal=.43*Math.Sin(phase)*Math.Exp(-t*5)+.40*filtered+.19*noise*Math.Exp(-t*35);break;
    case 3:signal=.31*Math.Sin(phase)+.22*Math.Sin(phase*1.498)+.13*Math.Sin(phase*2.007)+.10*filtered;break;
    case 4:signal=.53*filtered+.27*noise*(.5+.5*Math.Sin(t*80))+.17*Math.Sin(phase);break;
    default:signal=.30*Math.Sin(phase)+.23*Math.Sin(phase*2.71)*Math.Exp(-t*5)+.17*Math.Sin(phase*4.13)+.25*filtered;break;
   }
   if(action==1)signal=signal*.7+.2*noise*Math.Exp(-t*30);
   if(action==2)signal*=.65+.35*u;
   if(action==3)signal=signal*pulse+.14*Math.Sin(phase*.5);
   if(profile==9)signal+=.12*Math.Sin(phase*.251)*Math.Sin(t*19);
   double gain=envelope*.62;
   result[i*2]=(short)(Math.Max(-.88,Math.Min(.88,signal*gain))*32767);
   result[i*2+1]=(short)(Math.Max(-.88,Math.Min(.88,(signal*.94+filtered*.045*Math.Sin(t*31))*gain))*32767);
  }
  return result;
 }
 public static void Write(string path,short[] samples){using(var w=new BinaryWriter(File.Create(path))){int bytes=samples.Length*2;w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+bytes);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)2);w.Write(44100);w.Write(176400);w.Write((short)4);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(bytes);foreach(short sample in samples)w.Write(sample);}}
}
