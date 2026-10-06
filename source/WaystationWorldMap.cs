using System;using System.Drawing;using System.Drawing.Drawing2D;using System.IO;using System.Windows.Forms;
public partial class Game {
 void ShowWaystationWorldMap(){
  ClearPage();page="waystation-map";
  var map=new WaystationWorldMap(CachedImage(Path.Combine(root,"assets","world-map","waystation-map.png")),CachedImage(Path.Combine(root,"assets","world-map","waystation-map-wide.png")));
  map.Dock=DockStyle.Fill;map.Return=ShowTavernHub;
  map.SelectChapter=i=>{if(i==0)ShowTavernMainQuest();else GameMessage.Show(this,"本章尚未开放，请先在驿站准备下一段旅程。",WaystationWorldMap.ChapterNames[i]);};
  content.Controls.Add(map);map.Focus();
 }
}
public sealed class WaystationWorldMap:Control {
 public static readonly string[] ChapterNames={"序幕 · 第一盏灯","第一章 · 今天开始营业","第二章 · 住在楼上的炼金师","第三章 · 王冠之下","第四章 · 失去名字的守护者","第五章 · 最后一次开门营业","终章 · 归灯之夜"};
 readonly Image art,backdrop;public Action Return;public Action<int> SelectChapter;public bool ChaptersVisible{get;private set;}
 RectangleF view;Rectangle toggle,returnButton,panel;float uiScale;int hover=-1;
 public WaystationWorldMap(Image image,Image mapImage=null){art=image;backdrop=mapImage??image;ChaptersVisible=true;DoubleBuffered=true;ResizeRedraw=true;BackColor=Color.FromArgb(8,24,39);TabStop=true;AccessibleName="世界地图；按 Tab 收起或展开章节，按 Escape 返回驿站";}
 protected override bool IsInputKey(Keys keyData){return keyData==Keys.Tab||base.IsInputKey(keyData);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Tab){Toggle();e.Handled=true;}else if(e.KeyCode==Keys.Escape&&Return!=null){Return();e.Handled=true;}}
 public void Toggle(){ChaptersVisible=!ChaptersVisible;hover=-1;Invalidate();}
 void LayoutMap(){
  uiScale=Math.Min(Width/900f,Height/941f);int pw=(int)Math.Round(394*uiScale);panel=new Rectangle(Width-pw,0,pw,Height);
  float k=Math.Max(Width/(float)backdrop.Width,Height/(float)backdrop.Height);view=new RectangleF((Width-backdrop.Width*k)/2,(Height-backdrop.Height*k)/2,backdrop.Width*k,backdrop.Height*k);
  returnButton=new Rectangle((int)(27*uiScale),(int)(24*uiScale),(int)(183*uiScale),(int)(57*uiScale));
  toggle=new Rectangle(ChaptersVisible?Math.Max(0,panel.Left-(int)(36*uiScale)):Math.Max(0,Width-(int)(44*uiScale)),(Height-(int)(94*uiScale))/2,Math.Max(25,(int)(44*uiScale)),Math.Max(55,(int)(94*uiScale)));
 }
 Rectangle ChapterBounds(int i){float k=panel.Height/941f;return Rectangle.Round(new RectangleF(panel.Left+19*panel.Width/394f,(150+i*107)*k,360*panel.Width/394f,96*k));}
 int Hit(Point p){LayoutMap();if(toggle.Contains(p))return 8;if(returnButton.Contains(p))return 7;if(ChaptersVisible)for(int i=0;i<7;i++)if(ChapterBounds(i).Contains(p))return i;return -1;}
 void DrawOriginal(Graphics g,Rectangle destination,RectangleF source){g.DrawImage(art,destination,new RectangleF(source.X*art.Width/1672f,source.Y*art.Height/941f,source.Width*art.Width/1672f,source.Height*art.Height/941f),GraphicsUnit.Pixel);} void DrawMapLabel(Graphics g,float x,float y,RectangleF source){float k=view.Height/941f;var r=new Rectangle((int)(view.X+x*view.Width-source.Width*k/2),(int)(view.Y+y*view.Height-source.Height*k/2),(int)(source.Width*k),(int)(source.Height*k));DrawOriginal(g,r,source);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);LayoutMap();var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
  g.DrawImage(backdrop,view);
  DrawMapLabel(g,.43f,.415f,new RectangleF(604,374,137,40));DrawMapLabel(g,.43f,.46f,new RectangleF(620,414,123,26));DrawMapLabel(g,.59f,.395f,new RectangleF(908,369,130,40));
  DrawOriginal(g,returnButton,new RectangleF(27,24,183,57));
  if(ChaptersVisible)DrawOriginal(g,panel,new RectangleF(1278,0,394,941));
  GuildChrome.Draw(g,toggle,true);using(var f=GameTheme.Body(Math.Max(12,20*uiScale),FontStyle.Bold))GameTheme.DrawText(g,ChaptersVisible?"›":"‹",f,toggle,GuildChrome.Gold,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
  if(hover>=0){Rectangle r=hover==8?toggle:hover==7?returnButton:ChapterBounds(hover);using(var pen=new Pen(Color.FromArgb(210,255,224,140),2))g.DrawRectangle(pen,Rectangle.Inflate(r,-2,-2));}
 } protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);int h=Hit(e.Location);if(h!=hover){hover=h;Cursor=h<0?Cursors.Default:Cursors.Hand;Invalidate();}}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hover=-1;Invalidate();}
 protected override void OnMouseClick(MouseEventArgs e){base.OnMouseClick(e);if(e.Button!=MouseButtons.Left)return;Focus();int h=Hit(e.Location);if(h==8)Toggle();else if(h==7&&Return!=null)Return();else if(h>=0&&h<7&&SelectChapter!=null)SelectChapter(h);}
}
