using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;
public class WordDefinitionPopup:Form {
 public static readonly Color Paper=Color.Black,Ink=Color.FromArgb(235,237,232),SoftInk=Color.FromArgb(160,173,170);
 readonly Button close;
 public WordDefinitionPopup(){DoubleBuffered=true;FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;StartPosition=FormStartPosition.Manual;BackColor=Paper;ForeColor=Ink;Padding=new Padding(24,66,24,24);
  close=new Button{Text="×",Size=new Size(34,34),FlatStyle=FlatStyle.Flat,BackColor=Paper,ForeColor=SoftInk,Font=GameTheme.Body(18),Cursor=Cursors.Hand,AccessibleName="关闭释义"};close.FlatAppearance.BorderSize=0;close.FlatAppearance.MouseOverBackColor=Color.FromArgb(28,32,32);close.Click+=(s,e)=>Close();Controls.Add(close);Resize+=(s,e)=>{close.Location=new Point(ClientSize.Width-52,16);close.BringToFront();ClipFrame();};ClipFrame();
 }
 void ClipFrame(){
  if(ClientSize.Width<24||ClientSize.Height<24)return;
  using(var path=new GraphicsPath()){
   path.AddPolygon(GameTheme.Outline(Rectangle.Inflate(ClientRectangle,-1,-1),9));
   var old=Region;Region=new Region(path);if(old!=null)old.Dispose();
  }
 }
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.None;
  using(var edge=new Pen(GuildChrome.Gold,2))g.DrawPolygon(edge,GameTheme.Outline(Rectangle.Inflate(ClientRectangle,-2,-2),9));
  using(var line=new Pen(Color.FromArgb(45,52,50)))g.DrawLine(line,28,57,Width-28,57);
  using(var font=GameTheme.Body(12))TextRenderer.DrawText(g,"单词释义",font,new Rectangle(30,20,Width-95,26),SoftInk,TextFormatFlags.NoPadding);
 }
 protected override void WndProc(ref Message m){if(GameTheme.HitTest(this,ref m,true,false))return;base.WndProc(ref m);}
}
public class WordDefinitionIcon:RogueIcon {
 protected override void OnPaintBackground(PaintEventArgs e){e.Graphics.Clear(WordDefinitionPopup.Paper);}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(WordDefinitionPopup.Paper);g.SmoothingMode=SmoothingMode.AntiAlias;var ink=Enabled?WordDefinitionPopup.Ink:WordDefinitionPopup.SoftInk;
  using(var pen=new Pen(ink,1.8f)){
   if(Kind=="speaker"){var points=new[]{new Point(10,18),new Point(16,18),new Point(23,12),new Point(23,32),new Point(16,26),new Point(10,26)};g.DrawPolygon(pen,points);g.DrawArc(pen,19,14,13,16,-65,130);g.DrawArc(pen,19,9,20,26,-65,130);}
   else{var points=new PointF[10];for(int i=0;i<10;i++){double angle=-Math.PI/2+i*Math.PI/5;double radius=i%2==0?14:6;points[i]=new PointF(22+(float)(Math.Cos(angle)*radius),22+(float)(Math.Sin(angle)*radius));}if(Selected)using(var fill=new SolidBrush(Color.FromArgb(53,80,73)))g.FillPolygon(fill,points);g.DrawPolygon(pen,points);}
  }
  if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(3,3,Width-6,Height-6),ink,WordDefinitionPopup.Paper);
 }
}
