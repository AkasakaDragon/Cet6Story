using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// Shared holographic chrome for ordinary buttons, choices, dialogs and combat HUDs.
public static class CyberChrome {
 public static readonly Color Neon=Color.FromArgb(105,207,221),Amber=Color.FromArgb(211,182,123),Magenta=Color.FromArgb(171,119,182);
 public static void Panel(Graphics g,Rectangle bounds,Color accent,bool capsule=false,bool active=false,bool enabled=true){
  if(bounds.Width<8||bounds.Height<8)return;var state=g.Save();g.SmoothingMode=SmoothingMode.AntiAlias;
  var rect=new RectangleF(bounds.X+2,bounds.Y+2,bounds.Width-4,bounds.Height-4);float radius=capsule?rect.Height/2:Math.Min(12,rect.Height/4);
  Color edge=enabled?accent:Color.FromArgb(90,111,121);
  using(var path=ExpeditionVisuals.Rounded(rect,radius)){
   using(var fill=new LinearGradientBrush(rect,Color.FromArgb(active?240:225,active?28:16,active?49:30,active?67:43),Color.FromArgb(235,9,17,28),90))g.FillPath(fill,path);
   for(int i=3;i>=1;i--)using(var glow=new Pen(Color.FromArgb(active?22:12,edge),i*2+1))g.DrawPath(glow,path);
   using(var rim=new Pen(Color.FromArgb(enabled?185:90,edge),1.4f))g.DrawPath(rim,path);
   var inner=RectangleF.Inflate(rect,-4,-4);if(inner.Width>12&&inner.Height>10)using(var inset=ExpeditionVisuals.Rounded(inner,Math.Max(2,radius-4)))using(var pen=new Pen(Color.FromArgb(60,edge)))g.DrawPath(pen,inset);
  }
  float cx=rect.Left+rect.Width/2,notch=Math.Min(32,rect.Width*.14f);
  using(var fine=new Pen(Color.FromArgb(150,enabled?Amber:edge),1)){
   g.DrawLines(fine,new[]{new PointF(cx-notch,rect.Top+2),new PointF(cx-8,rect.Top+2),new PointF(cx,rect.Top+6),new PointF(cx+8,rect.Top+2),new PointF(cx+notch,rect.Top+2)});
   g.DrawLines(fine,new[]{new PointF(cx-notch,rect.Bottom-2),new PointF(cx-7,rect.Bottom-2),new PointF(cx,rect.Bottom-6),new PointF(cx+7,rect.Bottom-2),new PointF(cx+notch,rect.Bottom-2)});
  }
  if(!capsule&&rect.Width>50&&rect.Height>34){using(var pen=new Pen(Color.FromArgb(170,enabled?Magenta:edge),2)){g.DrawLine(pen,rect.Left+9,rect.Top+1,rect.Left+26,rect.Top+1);g.DrawLine(pen,rect.Right-26,rect.Bottom-1,rect.Right-9,rect.Bottom-1);}using(var pen=new Pen(Color.FromArgb(165,edge),2)){g.DrawLine(pen,rect.Left+1,rect.Top+13,rect.Left+1,rect.Top+26);g.DrawLine(pen,rect.Right-1,rect.Bottom-26,rect.Right-1,rect.Bottom-13);}}
  g.Restore(state);
 }
 public static void Button(Graphics g,Rectangle bounds,string text,Font font,bool primary,bool hover,bool enabled,bool left=false){
  bool compact=bounds.Height<38||bounds.Width<110;Color accent=primary?Amber:Neon;Panel(g,Rectangle.Inflate(bounds,-2,-3),accent,true,hover,enabled);
  var textBounds=Rectangle.Inflate(bounds,-(compact?8:22),-6);var flags=TextFormatFlags.NoPadding|TextFormatFlags.VerticalCenter|(compact?TextFormatFlags.SingleLine:TextFormatFlags.WordBreak)|(left?TextFormatFlags.Left:TextFormatFlags.HorizontalCenter);
  Color ink=!enabled?Color.FromArgb(107,127,139):primary?Color.FromArgb(239,218,172):Color.FromArgb(218,239,239);
  var shadow=textBounds;shadow.Offset(0,1);GameTheme.DrawText(g,text,font,shadow,Color.FromArgb(5,12,20),flags);GameTheme.DrawText(g,text,font,textBounds,ink,flags);
  if(hover&&enabled&&bounds.Width>120){using(var pen=new Pen(accent,1.5f)){float y=bounds.Top+bounds.Height/2f;g.DrawLines(pen,new[]{new PointF(bounds.Left+13,y-4),new PointF(bounds.Left+17,y),new PointF(bounds.Left+13,y+4)});g.DrawLines(pen,new[]{new PointF(bounds.Right-13,y-4),new PointF(bounds.Right-17,y),new PointF(bounds.Right-13,y+4)});}}
 }
}
