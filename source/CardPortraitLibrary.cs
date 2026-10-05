using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class CardPortraitLibrary {
 static readonly Dictionary<string,Bitmap> portraits=new Dictionary<string,Bitmap>();
 public static Image Get(string id){Bitmap cached;if(portraits.TryGetValue(id,out cached))return cached;
  string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"assets","rogue","cards","portraits",id+".png");if(!File.Exists(path))return null;
  // Decode the full asset once, retain only a small runtime portrait. All screens
  // share the same image; hovering and scrolling never reload full-size artwork.
  using(var source=Image.FromFile(path)){cached=new Bitmap(320,344,PixelFormat.Format32bppPArgb);using(var g=Graphics.FromImage(cached)){g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;float cropWidth=Math.Min(source.Width,source.Height*320f/344),cropHeight=Math.Min(source.Height,source.Width*344f/320);g.DrawImage(source,new Rectangle(0,0,320,344),new RectangleF((source.Width-cropWidth)/2,(source.Height-cropHeight)/2,cropWidth,cropHeight),GraphicsUnit.Pixel);}}
  portraits[id]=cached;return cached;
 }
}
