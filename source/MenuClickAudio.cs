using System;
public static class MenuClickAudio {
 // A short impulse excites several non-harmonic wooden resonances, with no sustained tone.
 public static short[] BuildWood(bool click){
  const int rate=44100;double duration=click?.16:.10;int count=(int)(rate*duration);var clip=new short[count*2];var random=new Random(click?731:419);double previousNoise=0;
  for(int i=0;i<count;i++){
   double t=i/(double)rate,noise=random.NextDouble()*2-1,attack=1-Math.Exp(-t*7000);
   double fundamental=click?760:1260;
   double body=Math.Sin(2*Math.PI*fundamental*t)*Math.Exp(-t/(click?.018:.010))*.65
    +Math.Sin(2*Math.PI*fundamental*1.83*t)*Math.Exp(-t/.008)*.26
    +Math.Sin(2*Math.PI*fundamental*3.17*t)*Math.Exp(-t/.004)*.13;
   double contact=(noise-previousNoise)*Math.Exp(-t/.0018)*.22;previousNoise=noise;
   double signal=(body+contact)*attack*(click?.68:.42);
   short value=(short)(Math.Max(-1,Math.Min(1,signal))*32767);clip[i*2]=value;clip[i*2+1]=value;
  }
  return clip;
 }
 public static short[] Build(){const int rate=44100;int count=(int)(rate*.14);var clip=new short[count*2];for(int i=0;i<count;i++){double t=i/(double)rate;double envelope=(1-Math.Exp(-t*900))*Math.Exp(-t*40)*Math.Min(1,(count-i)/(rate*.025));double v=(Math.Sin(2*Math.PI*(880*t-900*t*t))*.22+Math.Sin(2*Math.PI*1760*t)*.06)*envelope;clip[i*2]=(short)(v*23000);clip[i*2+1]=(short)(v*21000);}return clip;}
}
