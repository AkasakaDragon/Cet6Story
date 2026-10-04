using System;
public static class CurrencyAudio {
 public static short[] Build(){const int rate=44100;int count=(int)(rate*.72);var result=new short[count*2];double[] starts={0,.09,.19,.30},notes={1046.5,1318.5,1568,2093};for(int i=0;i<count;i++){double t=i/(double)rate,value=0;for(int j=0;j<notes.Length;j++){double time=t-starts[j];if(time<0)continue;double envelope=Math.Min(1,time/.004)*Math.Exp(-time*13);double phase=2*Math.PI*notes[j]*time;value+=envelope*(Math.Sin(phase)+.28*Math.Sin(phase*2.76)+.12*Math.Sin(phase*4.08))*.17;}double fade=Math.Min(1,(count-i)/(rate*.025));short sample=(short)(Math.Max(-.8,Math.Min(.8,value*fade))*32767);result[i*2]=sample;result[i*2+1]=(short)(sample*.94);}return result;}
}
