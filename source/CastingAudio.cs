using System;

public static class CastingAudio {
 public static short[] Build(int style){const int rate=44100;int length=(int)(rate*.76);var pcm=new short[length*2];for(int i=0;i<length;i++){double t=i/(double)rate,attack=Math.Min(1,t/.04),tail=Math.Max(0,1-t/.76),release=t<.33?0:Math.Exp(-(t-.33)*13);double f=style==0?360+t*480:style==1?190+t*260:520-t*240;double wave=Math.Sin(2*Math.PI*(f*t))* .22+Math.Sin(2*Math.PI*f*t*1.5)*.10;wave*=attack*tail;wave+=release*Math.Sin(2*Math.PI*(style==0?1100:style==1?620:1450)*t)*.23;double shimmer=Math.Sin(2*Math.PI*(1700+style*240)*t)*Math.Sin(2*Math.PI*13*t)*.04*tail;short sample=(short)(Math.Max(-1,Math.Min(1,wave+shimmer))*14000);pcm[i*2]=pcm[i*2+1]=sample;}return pcm;}
}
