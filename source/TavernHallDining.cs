using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

// Ground coordinates may be affine, but upright sprites must retain their aspect ratio.
public static class HallSpriteTransform {
 public static void KeepAspect(Graphics g){
  using(var transform=g.Transform){var m=transform.Elements;float determinant=m[0]*m[3]-m[1]*m[2];
   if(Math.Abs(determinant)<.000001f)return;
   float scale=(float)Math.Sqrt(m[2]*m[2]+m[3]*m[3]);
   using(var correction=new Matrix(scale*m[3]/determinant,-scale*m[1]/determinant,-scale*m[2]/determinant,scale*m[0]/determinant,0,0))g.MultiplyTransform(correction);
  }
 }
}

// A seated sprite is positioned by its pelvis, rather than by its walking feet.
// Read the actual separated silhouettes: generated artwork need not land on exact cells.
// Registration uses the unchanged seated body, not the moving spoon/mug's bounding centre.
public sealed class HallDiningSheet : IDisposable {
 public float HipFraction=.30f;
 public readonly Bitmap Art;
 public readonly Rectangle Bounds;
 public readonly int CellWidth,CellHeight;
 public readonly Rectangle[,] Frames;
 public HallDiningSheet(string file):this(file,3){}
 public HallDiningSheet(string file,int expectedRows):this(new Bitmap(file),expectedRows){}
 public HallDiningSheet(Image image,int expectedRows):this(new Bitmap(image),expectedRows){}
 HallDiningSheet(Bitmap bitmap,int expectedRows){
  Frames=new Rectangle[expectedRows,4];
  Art=bitmap;CellWidth=Art.Width/4;CellHeight=Art.Height/expectedRows;
  var locked=Art.LockBits(new Rectangle(0,0,Art.Width,Art.Height),ImageLockMode.ReadOnly,PixelFormat.Format32bppArgb);
  int stride=locked.Stride;var pixels=new byte[stride*Art.Height];
  try{Marshal.Copy(locked.Scan0,pixels,0,pixels.Length);}finally{Art.UnlockBits(locked);}
  var rows=new Rectangle[expectedRows];int rowStart=-1,rowCount=0;
  for(int y=0;y<=Art.Height;y++){
   int solid=0;if(y<Art.Height)for(int x=0;x<Art.Width;x++)if(pixels[y*stride+x*4+3]>200)solid++;
   if(solid>10&&rowStart<0)rowStart=y;
   if(solid<=10&&rowStart>=0){
    if(rowCount>=expectedRows)throw new ArgumentException("More than three dining rows");
    rows[rowCount++]=Rectangle.FromLTRB(0,Math.Max(0,rowStart-3),Art.Width,Math.Min(Art.Height,y+3));rowStart=-1;
   }
  }
  if(rowCount!=expectedRows)throw new ArgumentException("Dining sheet needs three separated action rows");
  for(int row=0;row<expectedRows;row++){
   int y0=rows[row].Top,y1=rows[row].Bottom,start=-1,frame=0;
   for(int x=0;x<=Art.Width;x++){
    int solid=0;if(x<Art.Width)for(int y=y0;y<y1;y++)if(pixels[y*stride+x*4+3]>200)solid++;
    // Small transparent gaps inside a dish or hand are not separate characters.
    if(solid<=5&&start>=0)for(int next=x+1;next<Math.Min(Art.Width,x+11);next++){
     int nearby=0;for(int y=y0;y<y1;y++)if(pixels[y*stride+next*4+3]>200)nearby++;
     if(nearby>5){solid=6;break;}
    }
    if(solid>5&&start<0)start=x;
    if(solid<=5&&start>=0){
     if(frame>=4)throw new ArgumentException("More than four dining silhouettes");
     int left=start,top=y1,right=x,bottom=y0;
     for(int sx=start;sx<x;sx++)for(int y=y0;y<y1;y++)if(pixels[y*stride+sx*4+3]>200){top=Math.Min(top,y);bottom=Math.Max(bottom,y+1);}
     Frames[row,frame++]=Rectangle.FromLTRB(Math.Max(0,left-3),Math.Max(y0,top-3),Math.Min(Art.Width,right+3),Math.Min(y1,bottom+3));start=-1;
    }
   }
   if(frame!=4)throw new ArgumentException("Dining sheet needs four separated sprites per row");
  }
  Bounds=Frames[0,0];
 }
 public void Draw(Graphics g,PointF hip,float height,int row,int frame,bool mirror){DrawLayer(g,hip,height,row,frame,mirror,false);}
 public void DrawUpper(Graphics g,PointF hip,float height,int row,int frame,bool mirror){DrawLayer(g,hip,height,row,frame,mirror,true);}
 void DrawLayer(Graphics g,PointF hip,float height,int row,int frame,bool mirror,bool upper){
  var source=Frames[row,frame%4];float scale=height/source.Height,width=source.Width*scale;
  var saved=g.Save();g.TranslateTransform(hip.X,hip.Y);HallSpriteTransform.KeepAspect(g);if(mirror)g.ScaleTransform(-1,1);
  if(upper)g.SetClip(new RectangleF(-height*2,-height*2,height*4,height*1.88f),CombineMode.Intersect);
  g.DrawImage(Art,new RectangleF(-height*HipFraction,-height*.76f,width,height),source,GraphicsUnit.Pixel);
  g.Restore(saved);
 }
 public void Dispose(){Art.Dispose();}
}
public static class HallDiningMotion {
 public static int Row(HallGuest guest){return guest.stage==4?(guest.recipe==3?2:1):0;}
 // Hold the reach, raise, mouth and return poses for different durations.
 // Offset by customer id, and restart from their actual meal progress.
 public static int Frame(HallGuest guest,double animation){
  if(guest.stage!=4)return 0;
  double time=Math.Max(0,guest.progress)+guest.id*.19;
  double cycle=time%2.4;
  return cycle<.65?0:cycle<1.05?1:cycle<1.8?2:3;
 }
}
