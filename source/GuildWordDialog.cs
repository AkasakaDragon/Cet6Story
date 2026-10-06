using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public sealed class GuildWordDialog:Form {
 readonly Button close;
 public GuildWordDialog(){
  DoubleBuffered=true;FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;
  StartPosition=FormStartPosition.Manual;BackColor=Color.FromArgb(25,53,55);ForeColor=GuildChrome.Ivory;
  Font=GameTheme.Body(11);Padding=new Padding(14,58,14,14);
  close=new Button{Text="×",Size=new Size(32,32),FlatStyle=FlatStyle.Flat,BackColor=BackColor,ForeColor=GuildChrome.Ivory,Font=GameTheme.Body(18),Cursor=Cursors.Hand,AccessibleName="关闭窗口"};
  close.FlatAppearance.BorderSize=0;close.FlatAppearance.MouseOverBackColor=Color.FromArgb(40,75,70);
  close.Click+=(sender,e)=>Close();Controls.Add(close);Resize+=(sender,e)=>LayoutFrame();
 }
 void LayoutFrame(){
  close.Location=new Point(Math.Max(0,ClientSize.Width-50),16);close.BringToFront();
  using(var path=new GraphicsPath()){path.AddPolygon(GameTheme.Outline(Rectangle.Inflate(ClientRectangle,-3,-3),12));var previous=Region;Region=new Region(path);if(previous!=null)previous.Dispose();}Invalidate();
 }
 protected override void OnPaint(PaintEventArgs e){
  base.OnPaint(e);GuildChrome.Draw(e.Graphics,ClientRectangle);
  using(var pen=new Pen(GuildChrome.Gold))e.Graphics.DrawLine(pen,24,50,ClientSize.Width-24,50);
  using(var font=GameTheme.Body(13,FontStyle.Bold))using(var ink=new SolidBrush(GuildChrome.Ivory))using(var format=new StringFormat{LineAlignment=StringAlignment.Center})GameTheme.DrawPixelString(e.Graphics,Text,font,ink,new RectangleF(26,16,Math.Max(1,ClientSize.Width-90),30),format);
 }
 protected override void WndProc(ref Message m){if(GameTheme.HitTest(this,ref m,true,false))return;base.WndProc(ref m);}
}
