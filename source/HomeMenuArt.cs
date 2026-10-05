using System;
using System.Drawing;
using System.Windows.Forms;

// Home-only chrome, kept separate from combat and story controls.
public static class HomeMenuArt {
 public static void Button(Graphics g,Rectangle bounds,string text,Font font,bool hover,bool enabled){
  int p=Math.Max(2,bounds.Height/18);var r=new Rectangle(p,p,bounds.Width-p*2,bounds.Height-p*3);
  Color edge=Color.FromArgb(72,104,86),paper=enabled?(hover?Color.FromArgb(252,245,207):Color.FromArgb(239,234,207)):Color.FromArgb(208,215,197);
  using(var shadow=new SolidBrush(Color.FromArgb(95,35,62,52)))g.FillRectangle(shadow,r.X+p,r.Y+p*2,r.Width,r.Height);
  using(var b=new SolidBrush(edge))g.FillRectangle(b,r);
  using(var b=new SolidBrush(paper))g.FillRectangle(b,r.X+p,r.Y+p,r.Width-p*2,r.Height-p*2);
  using(var b=new SolidBrush(Color.FromArgb(255,251,227)))g.FillRectangle(b,r.X+p,r.Y+p,r.Width-p*2,p);
  using(var b=new SolidBrush(hover?Color.FromArgb(183,144,67):Color.FromArgb(128,151,104))){g.FillRectangle(b,r.X+p*3,r.Y+p*3,p,r.Height-p*6);g.FillRectangle(b,r.Right-p*4,r.Y+p*3,p,r.Height-p*6);}
  int cx=r.X+p*7,cy=r.Y+r.Height/2;
  using(var b=new SolidBrush(enabled?edge:Color.FromArgb(140,150,134))){g.FillRectangle(b,cx-p,cy-p,p*2,p*2);if(hover)g.FillRectangle(b,cx,cy-p*2,p,p*4);}
  TextRenderer.DrawText(g,text,font,r,enabled?Color.FromArgb(44,73,61):Color.FromArgb(130,143,127),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding|TextFormatFlags.NoPrefix);
  if(hover)using(var pen=new Pen(Color.FromArgb(203,173,101),p))g.DrawRectangle(pen,r.X+p,r.Y+p,r.Width-p*2,r.Height-p*2);
 }
}
