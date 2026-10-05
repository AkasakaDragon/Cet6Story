using System;
using System.Drawing;
using System.Drawing.Drawing2D;

// Shared pixel plaque construction for the training lobby, matching the town menu.
public static class GuildChrome {
 public static readonly Color Gold=Color.FromArgb(213,173,101),Ivory=Color.FromArgb(246,228,185),Muted=Color.FromArgb(184,200,187);
 public static void Draw(Graphics g,Rectangle bounds,bool active=false,bool enabled=true){
  g.SmoothingMode=SmoothingMode.None;int w=bounds.Width,h=bounds.Height;
  if(w<18||h<18)return;
  var r=Rectangle.Inflate(bounds,-3,-3);int cut=Math.Min(12,Math.Max(5,h/7));
  using(var shadow=new SolidBrush(Color.FromArgb(125,9,18,23))){var sr=r;sr.Offset(2,3);g.FillPolygon(shadow,GameTheme.Outline(sr,cut));}
  using(var fill=new SolidBrush(enabled?Color.FromArgb(245,25,53,55):Color.FromArgb(245,34,45,47)))g.FillPolygon(fill,GameTheme.Outline(r,cut));
  using(var pattern=new SolidBrush(Color.FromArgb(15,154,174,143)))for(int y=14;y<h-12;y+=11)for(int x=14+(y%3)*4;x<w-12;x+=19)g.FillRectangle(pattern,x,y,2,1);
  using(var edge=new Pen(active?Color.FromArgb(255,217,142):Color.FromArgb(164,121,65),2))g.DrawPolygon(edge,GameTheme.Outline(r,cut));
  var inner=Rectangle.Inflate(r,-4,-4);
  using(var line=new Pen(active?Color.FromArgb(248,221,162):Color.FromArgb(104,91,61),1))g.DrawPolygon(line,GameTheme.Outline(inner,Math.Max(3,cut-3)));
  using(var gold=new SolidBrush(enabled?Gold:Color.FromArgb(126,120,100))){
   foreach(int x in new[]{r.Left+cut,r.Right-cut-3})foreach(int y in new[]{r.Top,r.Bottom-3})g.FillRectangle(gold,x,y,3,3);
   foreach(int x in new[]{r.Left,r.Right-3})foreach(int y in new[]{r.Top+cut,r.Bottom-cut-3})g.FillRectangle(gold,x,y,3,3);
  }
  if(active)using(var glow=new Pen(Color.FromArgb(105,255,201,111),2))g.DrawPolygon(glow,GameTheme.Outline(Rectangle.Inflate(r,2,2),cut+1));
 }
 public static void Emblem(Graphics g,Rectangle r,Color ink){
  Draw(g,r);int cx=r.X+r.Width/2;
  using(var pen=new Pen(ink,1)){g.DrawLine(pen,r.Left+8,r.Top+10,r.Left+8,r.Bottom-10);g.DrawLine(pen,r.Right-8,r.Top+10,r.Right-8,r.Bottom-10);}
  using(var brush=new SolidBrush(ink))g.FillPolygon(brush,new[]{new Point(cx,r.Top-3),new Point(cx+4,r.Top+1),new Point(cx,r.Top+5),new Point(cx-4,r.Top+1)});
 }
}
