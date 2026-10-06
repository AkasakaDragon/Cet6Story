using System;
using System.Drawing;
using System.Windows.Forms;

// Home-only chrome, kept separate from combat and story controls.
public static class HomeMenuArt {
 public static void Button(Graphics g,Rectangle bounds,string text,Font font,bool hover,bool enabled){
  int p=Math.Max(2,bounds.Height/18);var r=new Rectangle(p,p,bounds.Width-p*2,bounds.Height-p*3);
  Color edge=Color.FromArgb(213,156,52),paper=hover?Color.FromArgb(23,78,75):Color.FromArgb(15,58,59);
  using(var shadow=new SolidBrush(Color.FromArgb(95,35,62,52)))g.FillRectangle(shadow,r.X+p,r.Y+p*2,r.Width,r.Height);
  int corner=Math.Max(4,p*3);
  using(var b=new SolidBrush(edge))g.FillPolygon(b,GameTheme.Outline(r,corner));
  var inside=Rectangle.Inflate(r,-p,-p);
  using(var b=new SolidBrush(paper))g.FillPolygon(b,GameTheme.Outline(inside,Math.Max(2,corner-p)));
  using(var pen=new Pen(hover?Color.FromArgb(255,228,148):Color.FromArgb(125,101,53),Math.Max(1,p/2)))g.DrawPolygon(pen,GameTheme.Outline(Rectangle.Inflate(inside,-p,-p),Math.Max(2,corner-p)));
  int cx=r.X+p*7,cy=r.Y+r.Height/2;
  if(hover)using(var b=new SolidBrush(edge))g.FillPolygon(b,new[]{new Point(cx-p*2,cy),new Point(cx,cy-p*2),new Point(cx+p*2,cy),new Point(cx,cy+p*2)});
  TextRenderer.DrawText(g,text,font,r,enabled?Color.FromArgb(255,227,154):Color.FromArgb(145,151,133),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding|TextFormatFlags.NoPrefix);
  if(hover)using(var pen=new Pen(Color.FromArgb(255,220,124),p))g.DrawPolygon(pen,GameTheme.Outline(r,corner));
 }
}
