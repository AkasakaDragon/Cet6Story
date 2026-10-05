using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;
public class RogueIcon:Button {
 public string Kind;public bool Selected;public int Count;bool over;
 public RogueIcon(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);Width=44;Height=44;Margin=new Padding(3,6,3,3);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;BackColor=Color.FromArgb(22,38,45);Cursor=Cursors.Hand;DoubleBuffered=true;}
 protected override void OnMouseEnter(EventArgs e){over=true;Invalidate();base.OnMouseEnter(e);}
 protected override void OnMouseLeave(EventArgs e){over=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnPaintBackground(PaintEventArgs e){e.Graphics.Clear(Color.FromArgb(22,38,45));}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(Color.FromArgb(22,38,45));g.SmoothingMode=SmoothingMode.AntiAlias;float cx=Width/2f,cy=Height/2f;float scale=Math.Min(Width,Height)/44f;var saved=g.Save();g.TranslateTransform(cx,cy);g.ScaleTransform(scale,scale);Color color=!Enabled?Color.FromArgb(103,118,121):Kind=="star"?(Selected?Color.FromArgb(255,209,111):Color.FromArgb(200,181,231)):Kind=="bulb"?Color.FromArgb(247,211,135):Color.FromArgb(150,225,206);using(var bg=new SolidBrush(Color.FromArgb(over?210:155,22,38,45)))g.FillEllipse(bg,-20,-20,40,40);using(var border=new Pen(over||Focused?Color.FromArgb(239,211,150):Color.FromArgb(90,150,166,162),1))g.DrawEllipse(border,-20,-20,40,40);using(var pen=new Pen(color,2.2f))using(var brush=new SolidBrush(color)){
 if(Kind=="speaker"){g.TranslateTransform(-2,0);g.ScaleTransform(.9f,.9f);g.FillPolygon(brush,new[]{new PointF(-12,-4),new PointF(-7,-4),new PointF(0,-10),new PointF(0,10),new PointF(-7,4),new PointF(-12,4)});g.DrawArc(pen,-4,-8,15,16,-60,120);g.DrawArc(pen,-7,-13,25,26,-55,110);}
 else if(Kind=="star"){var points=new PointF[10];for(int i=0;i<10;i++){double angle=-Math.PI/2+i*Math.PI/5;float radius=i%2==0?12:5.3f;points[i]=new PointF((float)Math.Cos(angle)*radius,(float)Math.Sin(angle)*radius);}if(Selected)g.FillPolygon(brush,points);else g.DrawPolygon(pen,points);}
 else{g.DrawEllipse(pen,-7,-12,14,16);g.DrawLine(pen,-4,5,4,5);g.DrawLine(pen,-4,8,4,8);g.DrawLine(pen,-2,11,2,11);g.DrawLine(pen,-2,-4,0,3);g.DrawLine(pen,2,-4,0,3);}}
 g.Restore(saved);if(Kind=="bulb")using(var font=GameTheme.Body(8))GameTheme.DrawText(g,Count.ToString(),font,new Rectangle(Width-15,Height-15,14,14),color,TextFormatFlags.Right|TextFormatFlags.NoPadding);}
}

