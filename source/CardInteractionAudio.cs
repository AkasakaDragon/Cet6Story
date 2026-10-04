using System;
public static class CardInteractionAudio {
 public static short[] Build(bool play){const int rate=44100;double duration=play?.34:.13;int count=(int)(rate*duration);var samples=new short[count*2];var random=new Random(play?4171:829);double filtered=0;for(int i=0;i<count;i++){double t=i/(double)rate,u=t/duration,noise=random.NextDouble()*2-1;filtered=filtered*.56+noise*.44;double envelope=Math.Sin(Math.PI*u)*Math.Pow(1-u,.6);double signal=filtered*(play?.30:.20);if(play){signal+=Math.Sin(2*Math.PI*(660*t+600*t*t))*.10*Math.Exp(-t*10);signal+=Math.Sin(2*Math.PI*1320*t)*.05*Math.Exp(-t*14);}double pan=.35+u*.3;short left=(short)(signal*envelope*32767*(1-pan)),right=(short)(signal*envelope*32767*pan);samples[i*2]=left;samples[i*2+1]=right;}return samples;}
}
