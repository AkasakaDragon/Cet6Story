using System;

// Original slow modal choir and orbital pads; no sampled music or borrowed melody.
public static class MenuMusic {
 public static short[] Build(){
  const int rate=44100,seconds=64;int count=rate*seconds;var dry=new double[count];
  int[][] chords={new[]{45,52,57,60},new[]{41,48,57,60},new[]{48,55,59,62},new[]{43,50,57,62},new[]{45,52,57,64},new[]{41,48,55,60},new[]{43,50,59,62},new[]{45,52,57,60}};
  for(int section=0;section<8;section++)foreach(int note in chords[section]){
   double hz=440*Math.Pow(2,(note-69)/12.0);int start=section*8*rate,length=12*rate;
   for(int j=0;j<length;j++){double t=j/(double)rate,envelope=Math.Pow(Math.Sin(Math.PI*j/length),2);double phase=2*Math.PI*hz*t+.017*Math.Sin(2*Math.PI*.31*t);double voice=0;
    for(int harmonic=1;harmonic<=9;harmonic++){double f=hz*harmonic;double formant=.16+Math.Exp(-Math.Pow((f-650)/240,2))+.4*Math.Exp(-Math.Pow((f-1150)/320,2));voice+=Math.Sin(phase*harmonic)*formant/(harmonic*3.5);}
    double pad=.14*Math.Sin(phase*.5)+.07*Math.Sin(phase*1.003);dry[(start+j)%count]+=(voice+pad)*envelope*.22;
   }
  }
  var result=new short[count*2];double peak=0;
  for(int i=0;i<count;i++)for(int c=0;c<2;c++){double value=dry[i]*.7;for(int echo=1;echo<=6;echo++){int delay=(int)((.173*echo+.047*c)*rate);value+=dry[(i-delay+count)%count]*Math.Pow(.64,echo)*.28;}peak=Math.Max(peak,Math.Abs(value));result[i*2+c]=(short)Math.Max(-32700,Math.Min(32700,value*23000));}
  return result;
 }
}
