using System;
public static class MenuClickAudio {
 public static short[] Build(){const int rate=44100;int count=(int)(rate*.14);var clip=new short[count*2];for(int i=0;i<count;i++){double t=i/(double)rate;double envelope=(1-Math.Exp(-t*900))*Math.Exp(-t*40)*Math.Min(1,(count-i)/(rate*.025));double v=(Math.Sin(2*Math.PI*(880*t-900*t*t))*.22+Math.Sin(2*Math.PI*1760*t)*.06)*envelope;clip[i*2]=(short)(v*23000);clip[i*2+1]=(short)(v*21000);}return clip;}
}
