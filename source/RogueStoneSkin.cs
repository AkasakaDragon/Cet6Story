using System;
using System.Drawing;
using System.Drawing.Drawing2D;

// Shared carved-stone treatment for the adventure lobby and expedition result.
public static class RogueStoneSkin {
 static readonly Bitmap Texture=CreateTexture();
 static Bitmap CreateTexture(){
  var image=new Bitmap(126,96);var random=new Random(4173);
  using(var g=Graphics.FromImage(image)){
   g.Clear(Color.FromArgb(25,33,36));
   for(int row=0;row<3;row++)for(int x=row%2==0?0:-21;x<126;x+=42){
    int y=row*32,shade=random.Next(-6,8);var slab=new Rectangle(x+2,y+2,38,27);
    using(var stone=new SolidBrush(Color.FromArgb(58+shade,65+shade,64+shade)))g.FillRectangle(stone,slab);
    using(var light=new Pen(Color.FromArgb(84+shade,90+shade,85+shade)))g.DrawLine(light,slab.X+1,slab.Y+1,slab.Right-2,slab.Y+1);
    using(var dark=new Pen(Color.FromArgb(31,39,41)))g.DrawLine(dark,slab.X+1,slab.Bottom-1,slab.Right-2,slab.Bottom-1);
    if(random.Next(3)==0)using(var crack=new Pen(Color.FromArgb(37,46,47)))g.DrawLines(crack,new[]{new Point(x+12,y+8),new Point(x+17,y+10),new Point(x+20,y+15)});
   }
   for(int i=0;i<80;i++){
    int x=random.Next(image.Width),y=random.Next(image.Height),shade=random.Next(2)==0?29:87;
    using(var grain=new SolidBrush(Color.FromArgb(shade,shade+6,shade+4)))g.FillRectangle(grain,x,y,random.Next(1,4),1);
   }
  }return image;
 }
 public static void Draw(Graphics g,Rectangle bounds,bool raised=false,bool active=false){
  if(bounds.Width<12||bounds.Height<12)return;
  var state=g.Save();g.SetClip(bounds);g.SmoothingMode=SmoothingMode.None;
  using(var brush=new TextureBrush(Texture,WrapMode.Tile))g.FillRectangle(brush,bounds);
  using(var shade=new SolidBrush(Color.FromArgb(raised?18:42,8,15,19)))g.FillRectangle(shade,bounds);
  using(var upper=new SolidBrush(Color.FromArgb(active?192:128,127,139,133)))g.FillRectangle(upper,bounds.X+8,bounds.Y+2,bounds.Width-16,3);
  using(var shadow=new SolidBrush(Color.FromArgb(150,13,20,23)))g.FillRectangle(shadow,bounds.X+8,bounds.Bottom-5,bounds.Width-16,3);
  using(var light=new Pen(active?Color.FromArgb(187,184,169):Color.FromArgb(117,128,126),2))using(var dark=new Pen(Color.FromArgb(22,30,33),3)){
   g.DrawRectangle(dark,bounds.X+1,bounds.Y+1,bounds.Width-3,bounds.Height-3);
   g.DrawLine(light,bounds.X+7,bounds.Y+5,bounds.Right-8,bounds.Y+5);
   g.DrawLine(light,bounds.X+5,bounds.Y+7,bounds.X+5,bounds.Bottom-8);
   g.DrawLine(dark,bounds.X+7,bounds.Bottom-6,bounds.Right-8,bounds.Bottom-6);
   g.DrawLine(dark,bounds.Right-6,bounds.Y+7,bounds.Right-6,bounds.Bottom-8);
  }
  using(var notch=new SolidBrush(Color.FromArgb(23,30,32)))using(var fleck=new SolidBrush(Color.FromArgb(172,148,104))){
   foreach(int x in new[]{bounds.X+2,bounds.Right-9})foreach(int y in new[]{bounds.Y+2,bounds.Bottom-9}){
    g.FillRectangle(notch,x,y,7,7);g.FillRectangle(fleck,x+2,y+2,2,2);
   }
  }
  g.Restore(state);
 }
}
