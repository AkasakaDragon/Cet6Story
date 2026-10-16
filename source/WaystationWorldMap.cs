using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Drawing.Imaging;using System.IO;using System.Windows.Forms;
public partial class Game {
 void ShowWaystationWorldMap(){
  ClearPage();page="waystation-map";
  var map=new WaystationWorldMap(CachedImage(Path.Combine(root,"assets","world-map","aeserlan-sketch.png"))){Dock=DockStyle.Fill};
  map.ChapterOneOpen=save.completed.Contains(TavernStory.Id);map.ChapterOneCompleted=save.completed.Contains(WaystationChapterOne.Ids[5]);map.Return=ShowTavernHub;
  map.SelectPlace=i=>{if(WaystationWorldMap.PlaceNames[i]=="麦溪村")ShowMaixiVillage();else GameMessage.Show(this,"暂未开放",WaystationWorldMap.PlaceNames[i]);};
  content.Controls.Add(map);map.Focus();
 }
 void ShowMaixiVillage(){
  ClearPage();page="maixi-village";
  var scene=MaixiVillageScene.FromWalkGif(
   CachedImage(Path.Combine(root,"assets","world-map","maixi-village.png")),
   CachedImage(Path.Combine(root,"assets","characters","exploration","luchuan-maixi-idle.png")),
   Path.Combine(root,"assets","characters","exploration","luchuan-walk.gif")
  );
  scene.Dock=DockStyle.Fill;
  scene.PlacePlayer(millVillageReturnX);
  scene.OpenMill=()=>{millVillageReturnX=scene.PlayerX;ShowMillDungeon();};
  scene.Return=ShowWaystationWorldMap;
  scene.OpenQuest=ShowTavernMainQuest;
  content.Controls.Add(scene);scene.Focus();
 }
}
public sealed class WaystationWorldMap : Control {
 public static readonly string[] PlaceNames={"枯冠根庭","森林入口","钟铜城镇","白岚城堡","驿站","麦溪村","镜潮码头","无声之庭（蚀冠堡垒）","烬星陨坑","陨坑营地","沉脊矿洞"};
 const float MapWidth=1402f, MapHeight=1122f, HoverScale=1.09f;
 // Original image coordinates. Paths follow landmark groups and exclude their lettering.
 static PointF[] Points(params int[] xy){var p=new PointF[xy.Length/2];for(int i=0;i<p.Length;i++)p[i]=new PointF(xy[i*2],xy[i*2+1]);return p;}
 static readonly PointF[][] Landmarks={
  Points(156,208,180,200,202,207,227,205,249,222,253,247,228,258,219,292,177,294,173,273,156,252),
  Points(334,477,336,447,348,426,358,416,370,423,387,443,391,477),
  Points(133,701,143,681,157,662,182,662,196,682,219,681,219,636,234,595,248,636,251,670,269,646,288,671,286,700,268,707,161,707),
  Points(417,590,432,565,446,552,457,551,465,531,476,507,488,531,490,552,509,550,528,568,535,590),
  Points(620,821,620,803,630,801,641,781,653,784,670,804,670,824,693,825,702,808,717,819,722,838,691,839,677,829,642,825),
  Points(827,932,829,906,838,881,855,878,873,892,884,919,897,917,912,907,937,925,945,949,907,955,867,946,841,944),
  Points(1073,756,1076,734,1100,721,1100,705,1114,688,1126,701,1139,695,1151,709,1183,710,1185,675,1196,634,1205,675,1207,721,1220,733,1236,750,1232,776,1213,770,1207,757,1173,748,1147,745,1115,756,1101,771,1084,764),
  Points(726,423,731,403,742,388,752,401,755,380,766,363,777,380,779,400,792,390,805,404,810,424),
  Points(1006,282,1021,261,1049,244,1080,237,1125,238,1164,248,1200,266,1237,284,1236,301,1218,314,1177,321,1129,326,1086,321,1042,307),
  Points(1147,409,1154,395,1168,374,1176,388,1187,373,1197,392,1208,401,1217,407),
  Points(768,193,770,170,781,156,819,156,832,173,833,194)
 };
 // Text is display-only, including when an enlarged landmark approaches it.
 static readonly RectangleF[] TextAreas={
  new RectangleF(231,260,103,27),new RectangleF(395,437,99,32),
  new RectangleF(134,709,127,31),new RectangleF(440,598,110,34),
  new RectangleF(674,768,56,31),new RectangleF(906,868,78,34),
  new RectangleF(1230,689,111,34),new RectangleF(723,435,116,55),
  new RectangleF(1078,331,107,31),new RectangleF(1172,417,104,33),
  new RectangleF(743,120,107,33)
 };
 public bool ChapterOneOpen{get;set;} public bool ChapterOneCompleted{get;set;}
 readonly Image art;readonly GraphicsPath[] paths;
 public Action Return;public Action<int> SelectPlace;
 RectangleF view;Rectangle returnButton;int hover=-1,selected=5;bool keyboardSelection;
 public WaystationWorldMap(Image image){
  if(image==null)throw new ArgumentNullException("image");art=image;
  paths=new GraphicsPath[Landmarks.Length];for(int i=0;i<paths.Length;i++){paths[i]=new GraphicsPath();paths[i].AddPolygon(Landmarks[i]);}
  DoubleBuffered=true;ResizeRedraw=true;BackColor=Color.FromArgb(24,22,19);TabStop=true;
  AccessibleName="艾瑟兰大陆地图；选择建筑进入地区；麦溪村可进入，其余地点暂未开放";
 }
 void LayoutMap(){
  returnButton=new Rectangle(18,Math.Max(18,Height-66),Math.Min(180,Math.Max(80,Width/5)),48);
  if(Width<=0||Height<=0){view=RectangleF.Empty;return;}
  float k=Math.Min(Width/MapWidth,Height/MapHeight);view=new RectangleF((Width-MapWidth*k)/2,(Height-MapHeight*k)/2,MapWidth*k,MapHeight*k);
 }
 PointF LandmarkAnchor(int i){var b=paths[i].GetBounds();return new PointF(b.Left+b.Width/2,b.Bottom);}
 bool IsText(PointF p){foreach(var r in TextAreas)if(r.Contains(p))return true;return false;}
 bool ContainsLandmark(int i,PointF p,bool enlarged){
  if(IsText(p))return false;
  if(enlarged){var a=LandmarkAnchor(i);p=new PointF(a.X+(p.X-a.X)/HoverScale,a.Y+(p.Y-a.Y)/HoverScale);}
  return !IsText(p)&&paths[i].IsVisible(p);
 }
 public Rectangle PlaceBounds(int i){
  if(i<0||i>=paths.Length)throw new ArgumentOutOfRangeException("i");LayoutMap();var r=paths[i].GetBounds();float k=view.Width/MapWidth;
  return Rectangle.Round(new RectangleF(view.X+r.X*k,view.Y+r.Y*k,r.Width*k,r.Height*k));
 }
 public int HitPlace(Point p){
  LayoutMap();if(returnButton.Contains(p))return PlaceNames.Length;if(view.Width<=0||view.Height<=0)return -1;
  var q=new PointF((p.X-view.X)*MapWidth/view.Width,(p.Y-view.Y)*MapHeight/view.Height);
  if(IsText(q))return -1;
  // Keep the enlarged silhouette interactive until the pointer leaves it.
  if(hover>=0&&hover<paths.Length&&ContainsLandmark(hover,q,true))return hover;
  for(int i=0;i<paths.Length;i++)if(ContainsLandmark(i,q,false))return i;
  return -1;
 }
 void DrawLandmarkHover(Graphics g,int i){
  var state=g.Save();float k=view.Width/MapWidth;g.TranslateTransform(view.X,view.Y);g.ScaleTransform(k,k);
  var a=LandmarkAnchor(i);
  using(var enlarged=(GraphicsPath)paths[i].Clone())using(var matrix=new Matrix()){
   matrix.Translate(a.X,a.Y);matrix.Scale(HoverScale,HoverScale);matrix.Translate(-a.X,-a.Y);enlarged.Transform(matrix);
   g.SetClip(enlarged,CombineMode.Intersect);foreach(var r in TextAreas)g.ExcludeClip(Rectangle.Round(r));
   g.DrawImage(art,new RectangleF(a.X*(1-HoverScale),a.Y*(1-HoverScale),MapWidth*HoverScale,MapHeight*HoverScale),new RectangleF(0,0,art.Width,art.Height),GraphicsUnit.Pixel);
  }g.Restore(state);
 }
 protected override void OnPaint(PaintEventArgs e){
  base.OnPaint(e);LayoutMap();var g=e.Graphics;g.Clear(BackColor);g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=PixelOffsetMode.HighQuality;
  g.DrawImage(art,view);
  int active=hover>=0?hover:Focused&&keyboardSelection?selected:-1;
  if(active>=0&&active<paths.Length)DrawLandmarkHover(g,active);
  GuildChrome.Draw(g,returnButton,active==PlaceNames.Length);using(var f=GameTheme.Body(13,FontStyle.Bold))GameTheme.DrawText(g,"返回驿站",f,returnButton,GuildChrome.Ivory,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
 }
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);bool wasKeyboard=keyboardSelection;keyboardSelection=false;int hit=HitPlace(e.Location);if(hit!=hover||wasKeyboard){hover=hit;Cursor=hit<0?Cursors.Default:Cursors.Hand;Invalidate();}}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hover=-1;Cursor=Cursors.Default;Invalidate();}
 void Activate(int hit){if(hit==PlaceNames.Length){if(Return!=null)Return();}else if(hit>=0&&hit<PlaceNames.Length){selected=hit;if(SelectPlace!=null)SelectPlace(hit);}}
 protected override void OnMouseClick(MouseEventArgs e){base.OnMouseClick(e);if(e.Button!=MouseButtons.Left)return;Focus();Activate(HitPlace(e.Location));}
 protected override bool IsInputKey(Keys key){return key==Keys.Left||key==Keys.Right||key==Keys.Up||key==Keys.Down||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);keyboardSelection=true;hover=-1;Cursor=Cursors.Default;if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Up){selected=(selected+PlaceNames.Length)%(PlaceNames.Length+1);e.Handled=true;Invalidate();}else if(e.KeyCode==Keys.Right||e.KeyCode==Keys.Down){selected=(selected+1)%(PlaceNames.Length+1);e.Handled=true;Invalidate();}else if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){Activate(selected);e.Handled=true;}}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}
 protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);hover=-1;Cursor=Cursors.Default;Invalidate();}
 protected override void OnResize(EventArgs e){base.OnResize(e);hover=-1;Invalidate();}
 protected override void Dispose(bool disposing){if(disposing){foreach(var p in paths)p.Dispose();Return=null;SelectPlace=null;}base.Dispose(disposing);}
}


