using System;using System.Drawing;using System.Drawing.Drawing2D;using System.IO;using System.Windows.Forms;
public partial class Game {
 void ShowWaystationWorldMap(){
  ClearPage();page="waystation-map";
  var map=new WaystationWorldMap(CachedImage(Path.Combine(root,"assets","world-map","waystation-map.png")));
  map.Dock=DockStyle.Fill;map.Return=ShowTavernHub;
  map.SelectChapter=i=>{if(i==0)ShowTavernMainQuest();else GameMessage.Show(this,"本章尚未开放，请先在驿站准备下一段旅程。",WaystationWorldMap.ChapterNames[i]);};
  content.Controls.Add(map);map.Focus();
 }
}
public sealed class WaystationWorldMap:Control {
 public static readonly string[] ChapterNames={"序幕 · 第一盏灯","第一章 · 今天开始营业","第二章 · 住在楼上的炼金师","第三章 · 王冠之下","第四章 · 失去名字的守护者","第五章 · 最后一次开门营业","终章 · 归灯之夜"};
 readonly Image art;public Action Return;public Action<int> SelectChapter;public bool ChaptersVisible{get;private set;}
 RectangleF view;Rectangle toggle;int hover=-1;
 public WaystationWorldMap(Image image){art=image;ChaptersVisible=true;DoubleBuffered=true;ResizeRedraw=true;BackColor=Color.FromArgb(8,24,39);TabStop=true;AccessibleName="世界地图；按 Tab 收起或展开章节，按 Escape 返回驿站";}
 protected override bool IsInputKey(Keys keyData){return keyData==Keys.Tab||base.IsInputKey(keyData);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Tab){Toggle();e.Handled=true;}else if(e.KeyCode==Keys.Escape&&Return!=null){Return();e.Handled=true;}}
 public void Toggle(){ChaptersVisible=!ChaptersVisible;hover=-1;Invalidate();}
 void LayoutMap(){float sourceWidth=ChaptersVisible?1672:1278;float scale=Math.Min(Width/sourceWidth,Height/941f);view=new RectangleF((Width-sourceWidth*scale)/2,(Height-941*scale)/2,sourceWidth*scale,941*scale);toggle=ChaptersVisible?Transform(new RectangleF(1247,389,44,94)):new Rectangle(Math.Max(0,Width-48),(Height-82)/2,44,82);}
 Rectangle Transform(RectangleF r){float scale=view.Height/941f;return Rectangle.Round(new RectangleF(view.X+r.X*scale,view.Y+r.Y*scale,r.Width*scale,r.Height*scale));}
 int Hit(Point p){LayoutMap();if(toggle.Contains(p))return 8;if(Transform(new RectangleF(27,24,183,57)).Contains(p))return 7;if(ChaptersVisible)for(int i=0;i<7;i++)if(Transform(new RectangleF(1297,150+i*107,360,96)).Contains(p))return i;return -1;}
 void FillFog(Graphics g,Rectangle bounds){if(bounds.Width<=0||bounds.Height<=0)return;float sx=art.Width/1672f,sy=art.Height/941f;using(var brush=new TextureBrush(art,WrapMode.TileFlipXY,new RectangleF(50*sx,480*sy,120*sx,120*sy))){float scale=Math.Max(.25f,view.Height/941f);brush.ScaleTransform(scale/sx,scale/sy);g.FillRectangle(brush,bounds);}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);LayoutMap();var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
  int left=(int)Math.Ceiling(view.Left),right=(int)Math.Floor(view.Right),top=(int)Math.Ceiling(view.Top),bottom=(int)Math.Floor(view.Bottom);
  FillFog(g,new Rectangle(0,0,left,Height));FillFog(g,new Rectangle(right,0,Width-right,Height));FillFog(g,new Rectangle(left,0,right-left,top));FillFog(g,new Rectangle(left,bottom,right-left,Height-bottom));
  g.DrawImage(art,view,new RectangleF(0,0,ChaptersVisible?art.Width:1278f*art.Width/1672f,art.Height),GraphicsUnit.Pixel);
  if(!ChaptersVisible)FillFog(g,Transform(new RectangleF(1244,388,34,97)));
  if(!ChaptersVisible){GuildChrome.Draw(g,toggle,true);using(var f=GameTheme.Body(20,FontStyle.Bold))GameTheme.DrawText(g,"‹",f,toggle,GuildChrome.Gold,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}
  if(hover>=0){Rectangle r=hover==8?toggle:hover==7?Transform(new RectangleF(27,24,183,57)):Transform(new RectangleF(1297,150+hover*107,360,96));using(var pen=new Pen(Color.FromArgb(210,255,224,140),2))g.DrawRectangle(pen,Rectangle.Inflate(r,-2,-2));}
 }
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);int h=Hit(e.Location);if(h!=hover){hover=h;Cursor=h<0?Cursors.Default:Cursors.Hand;Invalidate();}}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hover=-1;Invalidate();}
 protected override void OnMouseClick(MouseEventArgs e){base.OnMouseClick(e);if(e.Button!=MouseButtons.Left)return;Focus();int h=Hit(e.Location);if(h==8)Toggle();else if(h==7&&Return!=null)Return();else if(h>=0&&h<7&&SelectChapter!=null)SelectChapter(h);}
}
