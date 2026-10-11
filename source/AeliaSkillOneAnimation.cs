using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

public static class AeliaSkillOneAnimation {
 public const int FrameCount=60, FrameMilliseconds=120, Duration=5680, ImpactTime=4440;
 public static readonly int[] StrikeTimes={240,2440,3000,3480,3960,4440};
 static readonly Dictionary<string,Image[]> cache=new Dictionary<string,Image[]>();
 public static Image[] Load(string root){
  string folder=Path.Combine(root,"assets","characters","aelia-pixel","skill-one");
  Image[] frames;if(cache.TryGetValue(folder,out frames))return frames;
  if(!File.Exists(Path.Combine(folder,"frame-001.png")))return null;
  frames=new Image[FrameCount];
  try{for(int i=0;i<FrameCount;i++){frames[i]=new Bitmap(Path.Combine(folder,"frame-"+(i+1).ToString("000")+".png"));if(frames[i].Size!=new Size(1080,600))throw new InvalidDataException("骑士斩击动画画布不一致。");}}
  catch{foreach(var f in frames)if(f!=null)f.Dispose();throw;}
  cache[folder]=frames;return frames;
 }
 public static int FrameLength(int frame){return frame>=17&&frame<55?80:120;}
 public static int FrameStart(int frame){int time=0;for(int i=0;i<frame;i++)time+=FrameLength(i);return time;}
 public static int FrameAt(float age){int time=0;for(int i=0;i<FrameCount;i++){time+=FrameLength(i);if(age<time)return i;}return FrameCount-1;}
 public static float PlacementCenter(float original,float target,float height,int frame){
  // Zero-based 16 is the invisible seventeenth frame; the eighteenth is the first teleport.
  if(frame<17||frame>=55)return original;
  bool behind=frame<27||(frame>=32&&frame<39);
  return target+(behind?1:-1)*height*.35f;
 }
 public static int SourceRootX(int frame){int originalFrame=frame+2;return originalFrame<=19||originalFrame>=57?420:originalFrame<=28||(originalFrame>=34&&originalFrame<=40)?800:620;}
 public static Rectangle Bounds(float center,int floor,float height,int frame){int scale=Math.Max(1,(int)Math.Round(height/150));return new Rectangle((int)Math.Round(center)-SourceRootX(frame)*scale,floor-420*scale,1080*scale,600*scale);}
}
