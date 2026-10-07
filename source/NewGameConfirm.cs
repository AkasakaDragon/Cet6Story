using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public partial class Game {
 NewGameConfirmOverlay newGameConfirm;
 void ConfirmNewGame(){
  if(newGameConfirm!=null&&!newGameConfirm.IsDisposed)return;
  var snapshot=new Bitmap(Math.Max(1,content.Width),Math.Max(1,content.Height));
  content.DrawToBitmap(snapshot,new Rectangle(Point.Empty,snapshot.Size));
  var overlay=new NewGameConfirmOverlay(snapshot,Path.Combine(root,"assets","ui","new-game-confirm.png")){Dock=DockStyle.Fill};
  newGameConfirm=overlay;
  overlay.Confirmed=()=>{newGameConfirm=null;overlay.Dispose();NavigateMenu(StartNewGame,"进入新游戏",false);};
  overlay.Cancelled=()=>{newGameConfirm=null;overlay.Dispose();content.Focus();};
  content.Controls.Add(overlay);overlay.BringToFront();overlay.Focus();
 }
}

// A compact artwork-backed modal drawn inside the existing game window.
public sealed class NewGameConfirmOverlay:Control {
 readonly Bitmap backdrop;readonly Image skin;
 readonly Rectangle source=new Rectangle(366,239,941,438);
 Rectangle panel,cancel,confirm,close;int hovered=-1,selected=0;
 public Action Confirmed,Cancelled;
 public NewGameConfirmOverlay(Bitmap background,string path){
  backdrop=background;skin=Image.FromFile(path);DoubleBuffered=true;ResizeRedraw=true;TabStop=true;
  SetStyle(ControlStyles.Selectable,true);AccessibleName="开启新旅程：要从第一章重新开始吗？";AccessibleRole=AccessibleRole.Dialog;
 }
 void LayoutArt(){
  int width=Math.Min(600,Math.Max(1,ClientSize.Width-40));int height=width*438/941;
  panel=new Rectangle((Width-width)/2,(Height-height)/2,width,height);
  cancel=Part(75,292,386,106);confirm=Part(484,292,386,106);close=Part(792,49,86,87);
 }
 Rectangle Part(int x,int y,int w,int h){return new Rectangle(panel.X+x*panel.Width/941,panel.Y+y*panel.Height/438,w*panel.Width/941,h*panel.Height/438);}
 protected override void OnPaint(PaintEventArgs e){
  LayoutArt();var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
  g.DrawImage(backdrop,ClientRectangle);using(var dim=new SolidBrush(Color.FromArgb(160,0,0,12)))g.FillRectangle(dim,ClientRectangle);
  g.DrawImage(skin,panel,source,GraphicsUnit.Pixel);
  int target=hovered>=0?hovered:selected;Rectangle hit=target==0?cancel:target==1?confirm:close;
  using(var pen=new Pen(target==1?Color.FromArgb(255,230,149):Color.FromArgb(129,214,239),2))g.DrawRectangle(pen,Rectangle.Inflate(hit,-4,-4));
 }
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);LayoutArt();hovered=cancel.Contains(e.Location)?0:confirm.Contains(e.Location)?1:close.Contains(e.Location)?2:-1;Cursor=hovered>=0?Cursors.Hand:Cursors.Default;Invalidate();}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hovered=-1;Invalidate();}
 protected override void OnMouseClick(MouseEventArgs e){base.OnMouseClick(e);if(e.Button!=MouseButtons.Left)return;LayoutArt();if(confirm.Contains(e.Location))Accept();else if(cancel.Contains(e.Location)||close.Contains(e.Location))Cancel();}
 public void Accept(){if(Confirmed!=null)Confirmed();}
 public void Cancel(){if(Cancelled!=null)Cancelled();}
 public void ActivateSelection(){if(selected==1)Accept();else Cancel();}
 protected override bool ProcessDialogKey(Keys keyData){Keys key=keyData&Keys.KeyCode;if(key==Keys.Tab||key==Keys.Left||key==Keys.Right){selected=1-selected;Invalidate();return true;}if(key==Keys.Enter||key==Keys.Space){ActivateSelection();return true;}return base.ProcessDialogKey(keyData);}
 protected override void Dispose(bool disposing){if(disposing){skin.Dispose();backdrop.Dispose();}base.Dispose(disposing);}
}
