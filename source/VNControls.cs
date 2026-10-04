using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class VNButton:Control {
 public bool PixelStyle=false;public string Icon="";public bool MenuStyle=false;public bool Active=false;bool hover=false;
 public VNButton(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);SetStyle(ControlStyles.StandardClick,false);BackColor=Color.Transparent;ForeColor=Color.FromArgb(255,230,201);Font=new Font(GameTheme.BodyName,11);TabStop=true;Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.PushButton;Size=new Size(40,40);}
 protected override void OnTextChanged(EventArgs e){base.OnTextChanged(e);Invalidate();}
 public void PerformClick(){if(Enabled)OnClick(EventArgs.Empty);}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}
 protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnEnabledChanged(EventArgs e){base.OnEnabledChanged(e);Invalidate();}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}
 protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(Enabled&&e.Button==MouseButtons.Left&&ClientRectangle.Contains(e.Location))OnClick(EventArgs.Empty);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(Enabled&&(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space)){OnClick(EventArgs.Empty);e.Handled=true;}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);e.Graphics.SmoothingMode=PixelStyle?SmoothingMode.None:SmoothingMode.AntiAlias;Color c=!Enabled?Color.FromArgb(130,150,154):(hover||Focused||Active)?Color.FromArgb(255,196,132):ForeColor;using(var pen=new Pen(c,2.2f))using(var brush=new SolidBrush(c)){
 if(Icon==""){CyberChrome.Button(e.Graphics,ClientRectangle,Text,Font,Active,hover||Focused,Enabled);return;}
 if(PixelStyle&&!MenuStyle)CyberChrome.Panel(e.Graphics,Rectangle.Inflate(ClientRectangle,-1,-1),CyberChrome.Neon);
 if(MenuStyle){GameTheme.Button(e.Graphics,ClientRectangle,Text,Font,false,hover||Focused,Enabled);if(hover||Focused){using(var gold=new SolidBrush(GameTheme.Gold))e.Graphics.FillRectangle(gold,15,Height/2-3,6,6);}return;}

 if(Icon==""){TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,c,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);return;}
 float x=Width/2f,y=Height/2f,r=8;
 if(Icon=="play")e.Graphics.FillPolygon(brush,new[]{new PointF(x-5,y-r),new PointF(x+8,y),new PointF(x-5,y+r)});
 else if(Icon=="pause"){e.Graphics.FillRectangle(brush,x-7,y-r,4,16);e.Graphics.FillRectangle(brush,x+3,y-r,4,16);}
 else if(Icon=="prev"||Icon=="next"){float direction=Icon=="next"?1:-1;e.Graphics.FillPolygon(brush,new[]{new PointF(x-direction*5,y-r),new PointF(x+direction*6,y),new PointF(x-direction*5,y+r)});e.Graphics.DrawLine(pen,x+direction*10,y-r,x+direction*10,y+r);}
 else if(Icon=="auto"){e.Graphics.FillPolygon(brush,new[]{new PointF(x-9,y-r),new PointF(x-1,y),new PointF(x-9,y+r)});e.Graphics.FillPolygon(brush,new[]{new PointF(x+1,y-r),new PointF(x+9,y),new PointF(x+1,y+r)});}
 else if(Icon=="stop")e.Graphics.FillRectangle(brush,x-6,y-6,12,12);
 else if(Icon=="home"){e.Graphics.DrawLines(pen,new[]{new PointF(x-10,y),new PointF(x,y-9),new PointF(x+10,y)});e.Graphics.DrawLines(pen,new[]{new PointF(x-7,y-1),new PointF(x-7,y+9),new PointF(x+7,y+9),new PointF(x+7,y-1)});e.Graphics.DrawRectangle(pen,x-2,y+3,4,6);}
 else if(Icon=="book"){e.Graphics.DrawRectangle(pen,x-9,y-9,18,18);e.Graphics.DrawLine(pen,x,y-9,x,y+9);e.Graphics.DrawLine(pen,x-6,y-4,x-3,y-4);e.Graphics.DrawLine(pen,x+3,y-4,x+6,y-4);}
 else if(Icon=="log"){e.Graphics.DrawRectangle(pen,x-8,y-9,16,18);for(int i=-4;i<=4;i+=4)e.Graphics.DrawLine(pen,x-4,y+i,x+4,y+i);}
 else if(Icon=="settings"){e.Graphics.DrawEllipse(pen,x-5,y-5,10,10);for(int i=0;i<8;i++){double a=i*Math.PI/4;e.Graphics.DrawLine(pen,x+(float)Math.Cos(a)*7,y+(float)Math.Sin(a)*7,x+(float)Math.Cos(a)*11,y+(float)Math.Sin(a)*11);}}
 else if(Icon=="words"){TextRenderer.DrawText(e.Graphics,"Aa",new Font(GameTheme.LatinName,13,FontStyle.Bold),ClientRectangle,c,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}
 if(Focused)ControlPaint.DrawFocusRectangle(e.Graphics,new Rectangle(3,3,Width-6,Height-6));}}
}

public class OutlinedLabel:Control {
 public bool PixelText=false;
 public OutlinedLabel(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);BackColor=Color.Transparent;ForeColor=Color.White;Font=new Font(GameTheme.BodyName,17);}
 protected override void OnTextChanged(EventArgs e){base.OnTextChanged(e);Invalidate();}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(String.IsNullOrEmpty(Text))return;if(PixelText){var flags=TextFormatFlags.NoPadding|TextFormatFlags.WordBreak;var r=new Rectangle(2,3,Math.Max(1,Width-5),Math.Max(1,Height-4));var shadow=r;shadow.Offset(2,2);TextRenderer.DrawText(e.Graphics,Text,Font,shadow,Color.FromArgb(10,10,24),flags);TextRenderer.DrawText(e.Graphics,Text,Font,r,ForeColor,flags);return;}e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var path=new GraphicsPath())using(var format=new StringFormat(StringFormat.GenericTypographic)){format.Trimming=StringTrimming.EllipsisCharacter;path.AddString(Text,Font.FontFamily,(int)Font.Style,e.Graphics.DpiY*Font.Size/72f,new RectangleF(2,3,Width-5,Height-4),format);using(var shadow=new Pen(Color.FromArgb(200,12,10,24),3.5f)){shadow.LineJoin=LineJoin.Round;e.Graphics.DrawPath(shadow,path);}using(var brush=new SolidBrush(ForeColor))e.Graphics.FillPath(brush,path);}}
}


