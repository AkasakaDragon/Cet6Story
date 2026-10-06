using System;

// Soft broadband friction and a short landing rustle, distinct from wooden taps.
public static class PageTurnAudio {
 public static short[] Build(){
  const int rate=44100;int count=(int)(rate*.38);var samples=new short[count*2];var random=new Random(8927);double low=0,previous=0;
  for(int i=0;i<count;i++){
   double t=i/(double)rate,noise=random.NextDouble()*2-1;low=low*.76+noise*.24;
   double friction=(noise-low)*.64+(low-previous)*1.2;previous=low;
   double sweep=Math.Pow(Math.Sin(Math.PI*t/.38),2)*(.7+.3*Math.Sin(t*35));
   double landing=Math.Exp(-Math.Pow((t-.29)/.027,2))*.55;
   double envelope=(sweep+landing)*Math.Min(1,t/.008)*Math.Min(1,(.38-t)/.018);
   short value=(short)(Math.Max(-1,Math.Min(1,friction*envelope*.31))*32767);
   samples[i*2]=value;samples[i*2+1]=value;
  }
  return samples;
 }
}
