using System;using System.IO;using System.Drawing;using System.Drawing.Imaging;using System.Drawing.Drawing2D;using System.Collections.Generic;using System.Diagnostics;
public static class HeroIdleAnimation {
 static readonly Dictionary<string,Image[]> cache=new Dictionary<string,Image[]>();
 static readonly Stopwatch clock=Stopwatch.StartNew();
 public static int FrameIndex {get{return (int)(clock.ElapsedMilliseconds*32/1000%96);}}
 public static Image[] Load(string path){Image[] frames;if(cache.TryGetValue(path,out frames))return frames;if(!File.Exists(path))return null;
  using(var gif=Image.FromFile(path)){int count=gif.GetFrameCount(FrameDimension.Time);frames=new Image[count];try{for(int i=0;i<count;i++){gif.SelectActiveFrame(FrameDimension.Time,i);var frame=new Bitmap(355,468,PixelFormat.Format32bppPArgb);using(var g=Graphics.FromImage(frame)){g.CompositingMode=CompositingMode.SourceCopy;g.InterpolationMode=InterpolationMode.HighQualityBilinear;g.DrawImage(gif,new Rectangle(0,0,355,468));}frames[i]=frame;}}catch{foreach(var frame in frames)if(frame!=null)frame.Dispose();throw;}}
  cache[path]=frames;return frames;
 }
}
