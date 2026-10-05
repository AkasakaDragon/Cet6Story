using System;

// Original pastoral cue: curious arrival in another world, gentle 90 BPM.
// Piano/harp arpeggios, breathy flute and restrained bell answers, no borrowed melody.
public static class MenuMusic {
 const int Rate=44100;const double Beat=2.0/3.0;
 static void Note(double[] mix,int pitch,double start,double length,double gain,int voice){
  double hz=440*Math.Pow(2,(pitch-69)/12.0);int begin=(int)(start*Rate),count=(int)(length*Rate);
  for(int i=0;i<count;i++){
   double t=i/(double)Rate,u=t/length,phase=2*Math.PI*hz*t;
   double env=Math.Min(1,t/(voice==1?.09:.012))*Math.Min(1,(length-t)/.14),sound;
   if(voice==1){env*=Math.Pow(Math.Sin(Math.PI*u),.45);sound=Math.Sin(phase+.006*Math.Sin(t*29))+.16*Math.Sin(2*phase)+.035*Math.Sin(3*phase);}
   else if(voice==2){env*=Math.Exp(-t*3.5);sound=Math.Sin(phase)+.28*Math.Sin(phase*2.76)+.08*Math.Sin(phase*5.4);}
   else if(voice==3){env*=Math.Pow(Math.Sin(Math.PI*u),.6);sound=.65*Math.Sin(phase)+.2*Math.Sin(phase*1.002)+.1*Math.Sin(phase*2);}
   else{env*=Math.Exp(-t*2.2);sound=Math.Sin(phase)+.25*Math.Sin(2*phase)+.08*Math.Sin(3*phase);}
   mix[(begin+i)%mix.Length]+=sound*env*gain;
  }
 }
 public static short[] Build(){
  int count=64*Rate;var dry=new double[count];
  int[][] chords={new[]{48,55,60,64},new[]{55,62,67,71},new[]{57,64,69,72},new[]{53,60,65,69},new[]{48,55,60,64},new[]{52,59,64,67},new[]{53,60,65,69},new[]{55,62,67,71}};
  int[][] melody={new[]{76,-1,79,81,79,-1,76,74},new[]{74,-1,76,79,74,-1,71,-1},new[]{72,76,-1,79,76,-1,72,71},new[]{69,-1,72,74,72,-1,69,-1},new[]{76,-1,79,83,81,79,76,-1},new[]{74,76,-1,79,76,74,71,-1},new[]{72,-1,74,76,74,-1,72,69},new[]{71,-1,74,79,74,71,72,-1}};
  for(int bar=0;bar<24;bar++){
   int phrase=bar%8;double start=bar*4*Beat;
   foreach(int pitch in chords[phrase])Note(dry,pitch,start,4.5*Beat,.032,3);
   for(int step=0;step<8;step++)Note(dry,chords[phrase][new[]{0,1,2,3,1,2,3,2}[step]]+12,start+step*Beat*.5,1.6*Beat,.075,0);
   Note(dry,chords[phrase][0]-12,start,2.4*Beat,.085,0);
   if(bar>=4&&bar<20)for(int step=0;step<8;step++)if(melody[phrase][step]>=0)Note(dry,melody[phrase][step],start+step*.5*Beat,.83*Beat,.082,1);
   if(bar%2==1){Note(dry,chords[phrase][2]+24,start+2.5*Beat,1.1*Beat,.035,2);Note(dry,chords[phrase][3]+24,start+3*Beat,.9*Beat,.026,2);}
  }
  var stereo=new double[count*2];double peak=0;
  for(int i=0;i<count;i++)for(int channel=0;channel<2;channel++){
   double value=dry[i];for(int echo=1;echo<=4;echo++){int delay=(int)((.117*echo+.023*channel)*Rate);value+=dry[(i-delay+count)%count]*Math.Pow(.45,echo)*.24;}
   stereo[i*2+channel]=value;peak=Math.Max(peak,Math.Abs(value));
  }
  var result=new short[stereo.Length];double scale=peak>0?18500/peak:0;
  for(int i=0;i<result.Length;i++)result[i]=(short)(stereo[i]*scale);
  return result;
 }
}
