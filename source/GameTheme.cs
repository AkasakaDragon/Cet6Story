using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

public static class GameTheme {
 public static readonly Color Navy=Color.FromArgb(14,19,36),Card=Color.FromArgb(24,31,51),Gold=Color.FromArgb(246,201,119),Ink=Color.FromArgb(239,235,222),Muted=Color.FromArgb(174,151,131),Cyan=Color.FromArgb(119,199,215),Violet=Color.FromArgb(130,126,177);
 // GDI registration is private to this process; GDI+ collection lives for the game lifetime.
 static readonly PrivateFontCollection PixelFonts=new PrivateFontCollection();
 static readonly FontFamily PixelFamily=LoadPixelFont();
 public static readonly string BodyName=PixelFamily==null?"Microsoft YaHei UI":PixelFamily.Name,LatinName=BodyName;
 [DllImport("gdi32.dll",CharSet=CharSet.Unicode)]static extern int AddFontResourceEx(string name,uint flags,IntPtr reserved);
 static FontFamily LoadPixelFont(){string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"assets","fonts","fusion-pixel-12px-proportional-zh_hans.ttf");if(!File.Exists(path))return null;try{AddFontResourceEx(path,0x10,IntPtr.Zero);PixelFonts.AddFontFile(path);return PixelFonts.Families[0];}catch{return null;}}
 public static Font Body(float size,FontStyle style=FontStyle.Regular,GraphicsUnit unit=GraphicsUnit.Point){return PixelFamily==null?new Font(BodyName,size,style,unit):new Font(PixelFamily,size,style,unit);}
 public static Font Latin(float size,FontStyle style=FontStyle.Regular,GraphicsUnit unit=GraphicsUnit.Point){return Body(size,style,unit);}
 public static void DrawPixelString(Graphics g,string text,Font font,Brush ink,RectangleF bounds,StringFormat format){
  // GDI+ honours the expedition panels' scaling; GDI TextRenderer does not.
  var hint=g.TextRenderingHint;g.TextRenderingHint=TextRenderingHint.SingleBitPerPixelGridFit;
  try{var shadow=bounds;shadow.Offset(1,2);using(var dark=new SolidBrush(Color.FromArgb(12,12,18)))g.DrawString(text,font,dark,shadow,format);g.DrawString(text,font,ink,bounds,format);}finally{g.TextRenderingHint=hint;}
 }
 public static void DrawText(IDeviceContext g,string text,Font font,Rectangle bounds,Color ink,TextFormatFlags flags=TextFormatFlags.Default){flags|=TextFormatFlags.NoPrefix;var shadow=bounds;shadow.Offset(1,2);TextRenderer.DrawText(g,text,font,shadow,Color.FromArgb(12,12,18),flags);TextRenderer.DrawText(g,text,font,bounds,ink,flags);}
 public static Point[] Outline(Rectangle r,int cut){return new[]{new Point(r.Left+cut,r.Top),new Point(r.Right-cut,r.Top),new Point(r.Right-cut,r.Top+cut),new Point(r.Right,r.Top+cut),new Point(r.Right,r.Bottom-cut),new Point(r.Right-cut,r.Bottom-cut),new Point(r.Right-cut,r.Bottom),new Point(r.Left+cut,r.Bottom),new Point(r.Left+cut,r.Bottom-cut),new Point(r.Left,r.Bottom-cut),new Point(r.Left,r.Top+cut),new Point(r.Left+cut,r.Top+cut)};}
 public static void Frame(Graphics g,Rectangle r,Color fill,Color edge,bool corners=true){CyberChrome.Panel(g,r,edge);}
 public static void Button(Graphics g,Rectangle r,string text,Font font,bool primary,bool hover,bool enabled,bool left=false){CyberChrome.Button(g,r,text,font,primary,hover,enabled,left);}
 [DllImport("user32.dll")]static extern bool ReleaseCapture();
 [DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr h,int msg,IntPtr w,IntPtr l);
 public static void Drag(Form f){ReleaseCapture();SendMessage(f.Handle,0xA1,new IntPtr(2),IntPtr.Zero);}
 public static bool HitTest(Form f,ref Message m,bool resize,bool drag){
  // A modal dialog disables its owner. DefWindowProc otherwise beeps on clicks outside it.
  if(m.Msg==0x20&&!f.Enabled){m.Result=new IntPtr(1);return true;}
  if(m.Msg!=0x84)return false;long pos=m.LParam.ToInt64();Point pt=f.PointToClient(new Point((short)(pos&0xffff),(short)((pos>>16)&0xffff)));int w=f.ClientSize.Width,h=f.ClientSize.Height;bool left=pt.X<8,right=pt.X>=w-8,top=pt.Y<8,bottom=pt.Y>=h-8;int hit=1;if(resize){if(top&&left)hit=13;else if(top&&right)hit=14;else if(bottom&&left)hit=16;else if(bottom&&right)hit=17;else if(left)hit=10;else if(right)hit=11;else if(top)hit=12;else if(bottom)hit=15;}if(hit==1&&drag&&pt.Y<32&&pt.X<w-132)hit=2;m.Result=new IntPtr(hit);return hit!=1;}
}

public class GameButton:Button {
 public bool Primary;public bool SelectionOnly;bool over;
 public GameButton(){FlatStyle=FlatStyle.Flat;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);FlatAppearance.BorderSize=0;Font=GameTheme.Body(12,FontStyle.Bold);Cursor=Cursors.Hand;}
 protected override void OnMouseEnter(EventArgs e){over=true;Invalidate();base.OnMouseEnter(e);}
 protected override void OnMouseLeave(EventArgs e){over=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(Parent==null?GameTheme.Navy:Parent.BackColor);GameTheme.Button(e.Graphics,ClientRectangle,Text,Font,Primary,!SelectionOnly&&(over||Focused),Enabled);}
}

public class WindowGlyph:Control {
 public string Kind="close";public bool DialogStyle;bool over;
 public WindowGlyph(){DoubleBuffered=true;SetStyle(ControlStyles.StandardClick,false);Cursor=Cursors.Hand;Size=new Size(32,30);TabStop=true;BackColor=GameTheme.Navy;AccessibleRole=AccessibleRole.PushButton;}
 protected override void OnMouseEnter(EventArgs e){over=true;Invalidate();base.OnMouseEnter(e);}
 protected override void OnMouseLeave(EventArgs e){over=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button==MouseButtons.Left&&ClientRectangle.Contains(e.Location))OnClick(EventArgs.Empty);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){OnClick(EventArgs.Empty);e.Handled=true;}}
 protected override void OnPaint(PaintEventArgs e){if(DialogStyle){var g=e.Graphics;g.Clear(BackColor);GuildChrome.Draw(g,ClientRectangle,over,true);var ink=over?GuildChrome.Ivory:Color.FromArgb(190,157,94);using(var b=new SolidBrush(ink)){int cx=Width/2,cy=Height/2;for(int i=-4;i<=4;i+=2){g.FillRectangle(b,cx+i-1,cy+i-1,3,3);g.FillRectangle(b,cx+i-1,cy-i-1,3,3);}}return;}using(var b=new SolidBrush(over?GameTheme.Card:GameTheme.Navy))e.Graphics.FillRectangle(b,ClientRectangle);using(var p=new Pen(over?GameTheme.Gold:GameTheme.Cyan,3)){if(Kind=="close"){e.Graphics.DrawLine(p,10,8,22,20);e.Graphics.DrawLine(p,22,8,10,20);}else if(Kind=="min")e.Graphics.DrawLine(p,9,20,23,20);else e.Graphics.DrawRectangle(p,9,8,14,13);}}
}

public sealed class GameLogo:Control {
 readonly Image image;
 public GameLogo(string path){image=Image.FromFile(path);SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer,true);BackColor=Color.Transparent;AccessibleName="六级物语";}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);e.Graphics.InterpolationMode=InterpolationMode.NearestNeighbor;e.Graphics.PixelOffsetMode=PixelOffsetMode.Half;float scale=Math.Min(Width/(float)image.Width,Height/(float)image.Height);int w=(int)(image.Width*scale),h=(int)(image.Height*scale);e.Graphics.DrawImage(image,new Rectangle((Width-w)/2,(Height-h)/2,w,h));}
 protected override void Dispose(bool disposing){if(disposing)image.Dispose();base.Dispose(disposing);}
}

public static class GameMessage {
 public static DialogResult Show(string text,string title){return Show(null,text,title,MessageBoxButtons.OK,MessageBoxIcon.None);}
 public static DialogResult Show(string text){return Show(null,text,"提示",MessageBoxButtons.OK,MessageBoxIcon.None);}
 public static DialogResult Show(IWin32Window owner,string text){return Show(owner,text,"提示",MessageBoxButtons.OK,MessageBoxIcon.None);}
 public static DialogResult Show(IWin32Window owner,string text,string title){return Show(owner,text,title,MessageBoxButtons.OK,MessageBoxIcon.None);}
 public static DialogResult Show(IWin32Window owner,string text,string title,MessageBoxButtons buttons){return Show(owner,text,title,buttons,MessageBoxIcon.None);}
 public static DialogResult Show(IWin32Window owner,string text,string title,MessageBoxButtons buttons,MessageBoxIcon icon){using(var f=new PixelFrame{Text=title,Size=new Size(620,330),MinimumSize=new Size(430,260),StartPosition=FormStartPosition.CenterParent}){var body=new RetroLabel{Text=text,Font=GameTheme.Body(13),ForeColor=GameTheme.Ink,Dock=DockStyle.Fill,Padding=new Padding(20),AutoSize=false};f.Controls.Add(body);var row=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=62,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(12,4,12,4)};f.Controls.Add(row);bool yesno=buttons==MessageBoxButtons.YesNo||buttons==MessageBoxButtons.YesNoCancel;var ok=new GameButton{Text=yesno?"确认":"知道了",Primary=true,Size=new Size(140,46)};ok.Click+=(s,e)=>{f.DialogResult=yesno?DialogResult.Yes:DialogResult.OK;f.Close();};row.Controls.Add(ok);if(yesno||buttons==MessageBoxButtons.OKCancel){var cancel=new GameButton{Text="取消",Size=new Size(140,46)};cancel.Click+=(s,e)=>{f.DialogResult=yesno?DialogResult.No:DialogResult.Cancel;f.Close();};row.Controls.Add(cancel);f.CancelButton=cancel;f.AcceptButton=cancel;}else f.AcceptButton=ok;return f.ShowDialog(owner);}}
}

public partial class Game {
 void InitWindowControls(){Padding=Padding.Empty;}

 protected override void WndProc(ref Message m){if(GameTheme.HitTest(this,ref m,!windowFullscreenApplied,false))return;base.WndProc(ref m);}
}

public class MainMenuPanel:Panel {
 protected override CreateParams CreateParams{get{var parameters=base.CreateParams;parameters.ExStyle|=0x02000000;return parameters;}}
 public MainMenuPanel(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer,true);BackColor=Color.Transparent;}
 protected override void OnPaintBackground(PaintEventArgs e){base.OnPaintBackground(e);}
}
