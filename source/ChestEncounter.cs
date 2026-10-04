using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public sealed class ChestEncounter:Control {
 public Image BackgroundArt,ChestArt;
 public bool Rare;
 public int Coins,Balance;
 public Func<bool> Claim;
 public Action Continue;
 readonly Timer timer=new Timer{Interval=25};readonly Stopwatch watch=new Stopwatch();
 bool opened,hover,continuing;Rectangle chestBounds,continueBounds;
 public bool Opened{get{return opened;}}
 public ChestEncounter(){DoubleBuffered=true;ResizeRedraw=true;TabStop=true;BackColor=GameTheme.Navy;AccessibleRole=AccessibleRole.PushButton;AccessibleName="宝箱，点击或按回车打开";timer.Tick+=(s,e)=>{Invalidate();if(watch.Elapsed.TotalSeconds>1.3)timer.Stop();};}
 void LayoutScene(){int top=(int)(Height*.22),h=Math.Max(100,Math.Min((int)(Height*.57),Height-top-140)),w=Math.Min((int)(Width*.65),h);chestBounds=new Rectangle((Width-w)/2,top,w,h);continueBounds=new Rectangle(Math.Max(12,Width-258),Height-66,230,44);}
 public void Open(){if(opened||Claim==null||!Claim())return;opened=true;AccessibleName="宝箱已打开，按回车继续";watch.Restart();timer.Start();Invalidate();}
 void Proceed(){if(!opened||watch.Elapsed.TotalSeconds<.85||continuing)return;continuing=true;if(Continue!=null)Continue();}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);LayoutScene();bool next=(!opened&&chestBounds.Contains(e.Location))||(opened&&continueBounds.Contains(e.Location));if(next!=hover){hover=next;Invalidate();}Cursor=next?Cursors.Hand:Cursors.Default;}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hover=false;Invalidate();}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button!=MouseButtons.Left)return;LayoutScene();if(!opened&&chestBounds.Contains(e.Location))Open();else if(opened&&continueBounds.Contains(e.Location))Proceed();}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){e.Handled=true;e.SuppressKeyPress=true;if(opened)Proceed();else Open();}}
 protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
 void TextAt(Graphics g,string text,Rectangle rect,float size,Color color){using(var f=GameTheme.Body(size,FontStyle.Bold)){GameTheme.DrawText(g,text,f,new Rectangle(rect.X+1,rect.Y+2,rect.Width,rect.Height),Color.FromArgb(12,19,22),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak);GameTheme.DrawText(g,text,f,rect,color,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak);}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);LayoutScene();var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;ExpeditionVisuals.Background(g,BackgroundArt,ClientRectangle,150);
  float age=opened?(float)watch.Elapsed.TotalSeconds:0,progress=Math.Min(1,age/.85f);
  // Keep the chest anchored to the ground while the light expands behind it.
  var shadow=new Rectangle(chestBounds.X+chestBounds.Width/8,chestBounds.Bottom-45,chestBounds.Width*3/4,45);using(var b=new SolidBrush(Color.FromArgb(140,0,0,0)))g.FillEllipse(b,shadow);
  var glow=new Rectangle(chestBounds.X-30,chestBounds.Y-25,chestBounds.Width+60,chestBounds.Height+65);using(var path=new GraphicsPath()){path.AddEllipse(glow);using(var light=new PathGradientBrush(path)){light.CenterColor=Color.FromArgb(opened?150:hover?85:55,255,190,60);light.SurroundColors=new[]{Color.Transparent};g.FillPath(light,path);}}
  if(ChestArt!=null){int half=ChestArt.Width/2;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;if(!opened||progress>=1)g.DrawImage(ChestArt,chestBounds,new Rectangle(opened?half:0,0,half,ChestArt.Height),GraphicsUnit.Pixel);else{for(int frame=0;frame<2;frame++)using(var attributes=new System.Drawing.Imaging.ImageAttributes()){var matrix=new System.Drawing.Imaging.ColorMatrix();matrix.Matrix33=frame==0?1-progress:progress;attributes.SetColorMatrix(matrix);g.DrawImage(ChestArt,chestBounds,frame*half,0,half,ChestArt.Height,GraphicsUnit.Pixel,attributes);}}}
  if(opened){for(int i=0;i<19;i++){double angle=i*2.399;float spread=progress*(.65f+i%5*.11f),x=Width/2f+(float)Math.Cos(angle)*chestBounds.Width*.47f*spread,y=chestBounds.Y+chestBounds.Height*.43f+(float)Math.Sin(angle)*chestBounds.Height*.45f*spread-age*12;int size=8+i%3*3;using(var b=new SolidBrush(Color.FromArgb((int)(220*(1-Math.Min(.65,age*.4))),255,201,68)))g.FillEllipse(b,x,y,size,size);using(var p=new Pen(Color.FromArgb(180,126,78,20),2))g.DrawEllipse(p,x,y,size,size);}}
  int bw=Math.Min(530,Width-70),bh=58,bx=(Width-bw)/2,by=Math.Max(16,Height/12);Point[] ribbon={new Point(bx,by+7),new Point(bx+25,by+7),new Point(bx+32,by),new Point(bx+bw-32,by),new Point(bx+bw-25,by+7),new Point(bx+bw,by+7),new Point(bx+bw-10,by+bh/2),new Point(bx+bw,by+bh-4),new Point(bx+bw-32,by+bh-7),new Point(bx+32,by+bh-7),new Point(bx,by+bh-4),new Point(bx+10,by+bh/2)};
  using(var paper=new LinearGradientBrush(new Rectangle(bx,by,bw,bh),Color.FromArgb(207,180,127),Color.FromArgb(147,119,77),90))g.FillPolygon(paper,ribbon);using(var edge=new Pen(Color.FromArgb(102,79,49),2))g.DrawPolygon(edge,ribbon);TextAt(g,"里面有什么？",new Rectangle(bx+25,by+4,bw-50,bh-10),Width<900?19:24,Color.FromArgb(255,241,207));
  int textY=Math.Min(Height-116,chestBounds.Bottom+12);TextAt(g,opened?"金币 +"+Coins+"  ·  当前金币 "+Balance:(Rare?"稀有宝箱":"遗迹宝箱")+" · 点击宝箱打开",new Rectangle(20,textY,Width-40,34),Width<900?14:18,GameTheme.Gold);
  if(opened){TextAt(g,Rare?"还获得一次赋能选择机会":"金币已加入余额",new Rectangle(20,textY+35,Width-40,26),11,GameTheme.Muted);if(age>=.85)using(var font=GameTheme.Body(12,FontStyle.Bold))ExpeditionVisuals.Button(g,continueBounds,Rare?"选择赋能 →":"继续探索 →",font,hover,true);}
 }
}

public partial class Game {
 void RenderChestEncounter(RogueRun r){
  ClearPage();page="rogue";rogueBody=null;rogueArena=null;
  var scene=new ChestEncounter{Dock=DockStyle.Fill,BackgroundArt=TowerBackground(r),ChestArt=CachedImage(System.IO.Path.Combine(root,"assets","rogue","tower","chest-states.png")),Rare=r.chestKind==2};
  scene.Claim=()=>{if(save.rogue.ActiveRun!=r||r.state!="chest")return false;TowerEngine.Chest(save.rogue);scene.Coins=r.battleCoins;scene.Balance=save.rogue.coins;Persist();return true;};
  scene.Continue=RenderRogue;content.Controls.Add(scene);scene.Focus();
 }
}
