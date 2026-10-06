using System;
// Original 80-second, 72 BPM triple-time waystation cue. Circular note tails and reverb permit looping.
public static class WaystationMusic {
 const int Rate=44100;const double Beat=5.0/6;
 static void Note(double[] mix,int pitch,double start,double length,double gain,int voice){
  double hz=440*Math.Pow(2,(pitch-69)/12.0);int begin=(int)(start*Rate),count=(int)(length*Rate);
  for(int i=0;i<count;i++){double t=i/(double)Rate,p=2*Math.PI*hz*t;
   double env=Math.Min(1,t/(voice==1?.12:.008))*Math.Min(1,(length-t)/.16);
   double sound;
   if(voice==1){env*=Math.Pow(Math.Sin(Math.PI*t/length),.5);sound=Math.Sin(p+.018*Math.Sin(2*Math.PI*4.8*t))+.1*Math.Sin(p*2);}
   else if(voice==2){env*=.65+.35*Math.Sin(Math.PI*t/length);sound=.75*Math.Sin(p)+.18*Math.Sin(p*2);}
   else{env*=Math.Exp(-t*3);sound=Math.Sin(p)+.38*Math.Sin(p*2)*Math.Exp(-t*2)+.14*Math.Sin(p*3)*Math.Exp(-t*5);}
   mix[(begin+i)%mix.Length]+=sound*env*gain;
  }
 }
 public static short[] Build(){int count=80*Rate;var dry=new double[count];
 int[][] chords={new[]{50,57,62,66},new[]{47,54,59,62},new[]{43,50,55,59},new[]{45,52,57,61},new[]{50,57,62,66},new[]{54,57,62,66},new[]{43,50,55,59},new[]{45,52,57,61}};
 int[][] tune={new[]{74,78,81,78,76,74},new[]{71,74,78,-1,76,74},new[]{71,74,79,78,76,74},new[]{73,76,81,-1,78,76},new[]{78,81,83,81,78,74},new[]{78,76,74,-1,73,74},new[]{71,74,76,79,78,76},new[]{73,76,78,76,73,69}};
 for(int bar=0;bar<32;bar++){int q=bar%8;double start=bar*3*Beat;
  Note(dry,chords[q][0]-12,start,2.8*Beat,.07,0);
  for(int step=0;step<6;step++)Note(dry,chords[q][new[]{0,2,1,3,2,1}[step]]+12,start+step*.5*Beat,1.9*Beat,.068,0);
  if(bar>=8&&bar<28)for(int step=0;step<6;step++)if(tune[q][step]>=0)Note(dry,tune[q][step],start+step*.5*Beat,(step==5?1.2:.85)*Beat,.062,1);
  if(bar%2==0)foreach(int pitch in chords[q])Note(dry,pitch,start,5.8*Beat,.016,2);
 }
 var stereo=new double[count*2];double peak=0;
 for(int i=0;i<count;i++)for(int c=0;c<2;c++){double v=dry[i];for(int e=1;e<=5;e++){int delay=(int)((.137*e+.019*c)*Rate);v+=dry[(i-delay+count)%count]*Math.Pow(.48,e)*.27;}stereo[i*2+c]=v;peak=Math.Max(peak,Math.Abs(v));}
 var result=new short[stereo.Length];for(int i=0;i<result.Length;i++)result[i]=(short)(stereo[i]*17000/peak);return result;
 }
}
