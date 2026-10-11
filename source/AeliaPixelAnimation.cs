using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

// Preserve the authored 150px canvas: never trim or rescale individual poses.
public static class AeliaPixelAnimation {
 public const int FrameCount=38, FrameMilliseconds=40, Duration=1570, ImpactTime=800;
 public const int CanvasSize=150, AnchorX=94, GroundY=132, StandingHeight=91;
 static readonly Dictionary<string,Image[]> cache=new Dictionary<string,Image[]>();
 public static Image[] Load(string root) {
  string path=Path.Combine(root,"assets","characters","aelia-pixel","attack.png");
  Image[] frames;if(cache.TryGetValue(path,out frames))return frames;if(!File.Exists(path))return null;
  using(var sheet=new Bitmap(path)) {
   if(sheet.Width!=1200||sheet.Height!=750)throw new InvalidDataException("女骑士动作表必须为38个150×150帧。");
   frames=new Image[FrameCount];
   try{for(int i=0;i<frames.Length;i++)frames[i]=sheet.Clone(new Rectangle(i%8*150,i/8*150,150,150),PixelFormat.Format32bppArgb);}
   catch{foreach(var frame in frames)if(frame!=null)frame.Dispose();throw;}
  }
  cache[path]=frames;return frames;
 }
 static int FrameLength(int i){return i==0?80:i==19?60:i==37?100:i>=20&&i<30?30:i>=30&&i<33?50:40;}
 public static int FrameStart(int frame){int time=0;for(int i=0;i<Math.Min(FrameCount,Math.Max(0,frame));i++)time+=FrameLength(i);return time;}
 public static int FrameAt(float milliseconds){int end=0;for(int i=0;i<FrameCount;i++){end+=FrameLength(i);if(milliseconds<end)return i;}return FrameCount-1;}
 public static Rectangle Bounds(float center,int floor,float bodyHeight) {
  int scale=Math.Max(1,(int)Math.Round(bodyHeight/StandingHeight));
  return new Rectangle((int)Math.Round(center)-AnchorX*scale,floor-GroundY*scale,CanvasSize*scale,CanvasSize*scale);
 }
}
