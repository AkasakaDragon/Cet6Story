using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public partial class Game {
 void ShowEmbeddedRules(){
  ClearPage();page="rules";var canvas=new RulesImageCanvas{Dock=DockStyle.Fill,Art=CachedImage(System.IO.Path.Combine(root,"assets","prologue","rules.png")),BackColor=Bg};content.Controls.Add(canvas);
  var close=new RulesCloseButton{Anchor=AnchorStyles.Top|AnchorStyles.Right};canvas.Controls.Add(close);Action place=()=>close.Location=new Point(Math.Max(8,canvas.ClientSize.Width-close.Width-20),20);canvas.Resize+=(sender,e)=>place();place();close.BringToFront();tips.SetToolTip(close,"关闭规则，进入第一节");
  close.Click+=(sender,e)=>{if(page!="rules")return;Attempt().rulesShown=true;Persist();index=0;ShowStory();};
 }

}
public class RulesImageCanvas:Control {
 public Image Art;
 public RulesImageCanvas(){DoubleBuffered=true;ResizeRedraw=true;Cursor=Cursors.Default;AccessibleName="游戏规则，点击右上角关闭按钮进入第一节";}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(Art==null)return;float scale=Math.Min((float)Width/Art.Width,(float)Height/Art.Height);var rect=new RectangleF((Width-Art.Width*scale)/2,(Height-Art.Height*scale)/2,Art.Width*scale,Art.Height*scale);e.Graphics.InterpolationMode=InterpolationMode.NearestNeighbor;e.Graphics.PixelOffsetMode=PixelOffsetMode.Half;e.Graphics.DrawImage(Art,rect);}
}

public class RulesCloseButton:Control {
 bool over;
 public RulesCloseButton(){Size=new Size(76,76);Cursor=Cursors.Hand;TabStop=true;AccessibleName="关闭游戏规则，进入第一节";AccessibleRole=AccessibleRole.PushButton;DoubleBuffered=true;}
 protected override void OnMouseEnter(EventArgs e){over=true;Invalidate();base.OnMouseEnter(e);}
 protected override void OnMouseLeave(EventArgs e){over=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){e.Handled=true;OnClick(EventArgs.Empty);}base.OnKeyDown(e);}
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;using(var shadow=new SolidBrush(Color.FromArgb(8,10,24)))g.FillRectangle(shadow,6,6,Width-6,Height-6);
  using(var fill=new SolidBrush(over?Color.FromArgb(58,48,84):Color.FromArgb(24,25,47)))g.FillRectangle(fill,0,0,Width-6,Height-6);
  using(var edge=new SolidBrush(over?Color.FromArgb(126,231,197):Color.FromArgb(173,158,233))){g.FillRectangle(edge,4,0,Width-14,4);g.FillRectangle(edge,4,Height-10,Width-14,4);g.FillRectangle(edge,0,4,4,Height-14);g.FillRectangle(edge,Width-10,4,4,Height-14);}
  using(var ink=new SolidBrush(over?Color.White:Color.FromArgb(255,220,169)))for(int i=0;i<7;i++){g.FillRectangle(ink,14+i*6,14+i*6,6,6);g.FillRectangle(ink,14+(6-i)*6,14+i*6,6,6);}
  if(Focused)ControlPaint.DrawFocusRectangle(g,new Rectangle(8,8,Width-24,Height-24));
 }
}
