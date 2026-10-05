using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class RogueOrnateSprite {
 static readonly Dictionary<Image,Rectangle> visible=new Dictionary<Image,Rectangle>();
 static readonly Dictionary<Image,Dictionary<string,Bitmap>> rendered=new Dictionary<Image,Dictionary<string,Bitmap>>();
 public static void ClearCache(){foreach(var sizes in rendered.Values)foreach(var image in sizes.Values)image.Dispose();rendered.Clear();visible.Clear();}
 static readonly ImageAttributes muted=MutedAttributes();
 public static void Prepare(params Image[] images){foreach(var image in images)if(image!=null)VisibleBounds(image);}
 static ImageAttributes MutedAttributes(){
  const float s=.42f,b=.86f,lr=.2126f,lg=.7152f,lb=.0722f;
  var matrix=new ColorMatrix(new[]{
   new[]{b*(lr*(1-s)+s),b*lr*(1-s),b*lr*(1-s),0f,0f},
   new[]{b*lg*(1-s),b*(lg*(1-s)+s),b*lg*(1-s),0f,0f},
   new[]{b*lb*(1-s),b*lb*(1-s),b*(lb*(1-s)+s),0f,0f},
   new[]{0f,0f,0f,1f,0f},new[]{0f,0f,0f,0f,1f}
  });var attributes=new ImageAttributes();attributes.SetColorMatrix(matrix);return attributes;
 }
 static Rectangle VisibleBounds(Image image){
  Rectangle result;if(visible.TryGetValue(image,out result))return result;
  var bitmap=image as Bitmap;if(bitmap==null)return new Rectangle(0,0,image.Width,image.Height);
  int left=bitmap.Width,top=bitmap.Height,right=0,bottom=0;
  using(var pixels=bitmap.Clone(new Rectangle(0,0,bitmap.Width,bitmap.Height),PixelFormat.Format32bppArgb)){
   var data=pixels.LockBits(new Rectangle(0,0,pixels.Width,pixels.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
   try{var row=new byte[Math.Abs(data.Stride)];for(int y=0;y<pixels.Height;y+=2){Marshal.Copy(IntPtr.Add(data.Scan0,y*data.Stride),row,0,row.Length);for(int x=0;x<pixels.Width;x+=2)if(row[x*4+3]>12){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}}}
   finally{pixels.UnlockBits(data);}
  }
  result=left>right?new Rectangle(0,0,image.Width,image.Height):Rectangle.FromLTRB(Math.Max(0,left-3),Math.Max(0,top-3),Math.Min(bitmap.Width,right+4),Math.Min(bitmap.Height,bottom+4));visible[image]=result;return result;
 }
 static void DrawPart(Graphics graphics,Image image,Rectangle destination,Rectangle source,bool harmonize){
  if(destination.Width<=0||destination.Height<=0||source.Width<=0||source.Height<=0)return;
  if(harmonize)graphics.DrawImage(image,destination,source.X,source.Y,source.Width,source.Height,GraphicsUnit.Pixel,muted);
  else graphics.DrawImage(image,destination,source,GraphicsUnit.Pixel);
 }
 static Bitmap Cached(Image image,string key,int width,int height,Action<Graphics> paint){
  Dictionary<string,Bitmap> sizes;if(!rendered.TryGetValue(image,out sizes)){sizes=new Dictionary<string,Bitmap>();rendered[image]=sizes;}Bitmap result;if(sizes.TryGetValue(key,out result))return result;
  if(sizes.Count>=12){foreach(var old in sizes.Values)old.Dispose();sizes.Clear();}
  result=new Bitmap(width,height,PixelFormat.Format32bppPArgb);using(var graphics=Graphics.FromImage(result)){graphics.Clear(Color.Transparent);graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;graphics.PixelOffsetMode=PixelOffsetMode.HighQuality;paint(graphics);}sizes[key]=result;return result;
 }
 public static void Draw(Graphics graphics,Image image,Rectangle bounds,bool cropTransparency=true,bool harmonize=false){
  if(image==null||bounds.Width<=0||bounds.Height<=0)return;
  var source=cropTransparency?VisibleBounds(image):new Rectangle(0,0,image.Width,image.Height);string key="plain:"+bounds.Width+":"+bounds.Height+":"+cropTransparency+":"+harmonize;
  var result=Cached(image,key,bounds.Width,bounds.Height,g=>DrawPart(g,image,new Rectangle(0,0,bounds.Width,bounds.Height),source,harmonize));graphics.DrawImageUnscaled(result,bounds.Location);
 }
 public static void DrawWide(Graphics graphics,Image image,Rectangle bounds,bool harmonize=true){
  if(image==null||bounds.Width<=0||bounds.Height<=0)return;
  Rectangle source=VisibleBounds(image);int side=Math.Min(source.Width/5,source.Height/2),destSide=Math.Min(bounds.Width/4,Math.Max(20,bounds.Height*side/source.Height));
  string key="wide:"+bounds.Width+":"+bounds.Height+":"+harmonize;
  var result=Cached(image,key,bounds.Width,bounds.Height,g=>{
   DrawPart(g,image,new Rectangle(0,0,destSide,bounds.Height),new Rectangle(source.X,source.Y,side,source.Height),harmonize);
   DrawPart(g,image,new Rectangle(destSide,0,bounds.Width-2*destSide,bounds.Height),new Rectangle(source.X+side,source.Y,source.Width-2*side,source.Height),harmonize);
   DrawPart(g,image,new Rectangle(bounds.Width-destSide,0,destSide,bounds.Height),new Rectangle(source.Right-side,source.Y,side,source.Height),harmonize);
  });graphics.DrawImageUnscaled(result,bounds.Location);
 }
}
