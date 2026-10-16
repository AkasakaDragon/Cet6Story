using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Drawing.Imaging;using System.Collections.Generic;using System.Linq;using System.IO;using System.Diagnostics;using System.Windows.Forms;using System.Web.Script.Serialization;

public sealed partial class MillDungeonScene:Control {
 public Action Return;public Action<string> Sound;public Action<bool> FlowChanged;
 readonly string root,savePath;readonly Dictionary<string,Bitmap> images=new Dictionary<string,Bitmap>();readonly List<Bitmap> walk=new List<Bitmap>();
 readonly Timer timer;readonly HashSet<Keys> keys=new HashSet<Keys>();readonly Stopwatch clock=Stopwatch.StartNew();Form owner;
 public MillTrialState State{get;private set;}public string PersistenceError{get;private set;}
 MillFloor Floor{get{return State.floors[State.floor];}}
 float visualX,visualY,fromX,fromY,travel=1;double elapsed,animation,saveElapsed;bool left,disposed,bag,fullMap,labels,newPrompt,walking,keyRepeat;
 List<Point> route=new List<Point>();string pending,hover;string notice="单人探索试玩：WASD／方向键或点击移动，E互动，B背包，M地图。";double noticeUntil=9;int bagPage;
 Rectangle exitButton,bagButton,mapButton,newButton,yesButton,noButton,bagPrev,bagNext;
 const int TileSize=32;float zoom=2,camX,camY;Rectangle viewport;
 public MillDungeonScene(string gameRoot){
  root=gameRoot;savePath=Path.Combine(root,"mill-exploration-save.json");State=LoadOrCreate();visualX=State.playerX;visualY=State.playerY;
  using(var gif=Image.FromFile(Path.Combine(root,"assets/characters/exploration/luchuan-walk.gif"))){for(int i=0;i<gif.GetFrameCount(FrameDimension.Time);i++){gif.SelectActiveFrame(FrameDimension.Time,i);walk.Add(new Bitmap(gif));}}
  images["hero"]=new Bitmap(Path.Combine(root,"assets/characters/exploration/luchuan-maixi-idle.png"));
  // Resolve all images up front. Missing resources are a visible setup failure, never blank furniture.
  foreach(var f in State.floors)foreach(var p in f.props){ImageFor(p.art);if(!String.IsNullOrEmpty(p.alternate))ImageFor(p.alternate);}
  foreach(string id in new[]{"01-ground-01","01-ground-02","01-ground-03","01-ground-04","01-ground-05","01-ground-06","01-ground-07","01-ground-08","01-ground-33","02-structure-01","06-effects-17","06-effects-18","06-effects-19","06-effects-20","06-effects-21","06-effects-22","05-machinery-01"})ImageFor(id);
  SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.Selectable,true);
  BackColor=Color.FromArgb(12,17,19);TabStop=true;AccessibleName="麦溪村水车磨坊单人探索试玩；WASD移动、E互动、B背包、M地图";
  timer=new Timer{Interval=16};timer.Tick+=(s,e)=>TickScene();
 }
 Bitmap ImageFor(string id){Bitmap b;if(images.TryGetValue(id,out b))return b;string p=Path.Combine(root,"assets/dungeon/mill/tiles",id+".png");if(!File.Exists(p))throw new FileNotFoundException("缺少磨坊素材："+id,p);b=new Bitmap(p);images.Add(id,b);return b;}
 MillTrialState LoadOrCreate(){
  if(File.Exists(savePath)){try{var s=new JavaScriptSerializer{MaxJsonLength=4000000}.Deserialize<MillTrialState>(File.ReadAllText(savePath));if(Valid(s)){MillDungeonGenerator.UpgradeVisuals(s);return s;}if(s!=null&&(s.version==1||s.version==2)&&s.inventory!=null){File.Copy(savePath,savePath+".layout-v"+s.version+"-"+DateTime.UtcNow.ToString("yyyyMMddHHmmss"),true);var rebuilt=MillDungeonGenerator.Create(s.seed);rebuilt.inventory=s.inventory;notice="旧布局已备份，物资保留；从重建后的入口前厅开始。";return rebuilt;}throw new InvalidDataException("磨坊试玩存档版本或结构无效");}catch(Exception ex){PersistenceError=ex.Message;try{File.Copy(savePath,savePath+".invalid-"+DateTime.UtcNow.ToString("yyyyMMddHHmmss"),true);}catch{} }}
  return MillDungeonGenerator.Create(unchecked(Environment.TickCount&Int32.MaxValue));
 }
 static bool Valid(MillTrialState s){return s!=null&&s.version==3&&s.floor>=0&&s.floor<2&&s.floors!=null&&s.floors.Count==2&&s.inventory!=null&&s.floors.All(f=>f!=null&&f.width==48&&f.height==50&&f.tiles!=null&&f.tiles.Length==48*50&&f.seen!=null&&f.seen.Length==48*50&&f.props!=null&&f.props.All(p=>p!=null&&p.w>0&&p.h>0&&p.w<=6&&p.h<=6&&f.Inside(p.x,p.y))&&f.rooms!=null&&f.rooms.All(r=>r.mask!=null&&r.mask.Length==r.h&&r.mask.All(row=>row!=null&&row.Length==r.w))&&f.walls!=null&&f.walls.Length==48*50)&&s.floors[s.floor].CanWalk(s.playerX,s.playerY,false);}
 public void SaveTrial(){if(State==null)return;try{string text=new JavaScriptSerializer{MaxJsonLength=4000000}.Serialize(State);string temp=savePath+".tmp";File.WriteAllText(temp,text);if(File.Exists(savePath))File.Replace(temp,savePath,savePath+".bak");else File.Move(temp,savePath);PersistenceError=null;}catch(Exception ex){PersistenceError=ex.Message;Notify("保存失败："+ex.Message);}}
 void Notify(string text){notice=text;noticeUntil=elapsed+5;Invalidate();}
 void Play(string sound){if(Sound!=null)Sound(sound);}
 void Bind(){var next=FindForm();if(next==owner)return;if(owner!=null){owner.Deactivate-=Deactivated;owner.EnabledChanged-=EnabledOwner;owner.Activated-=Activated;}owner=next;if(owner!=null){owner.Deactivate+=Deactivated;owner.EnabledChanged+=EnabledOwner;owner.Activated+=Activated;}}
 void Deactivated(object s,EventArgs e){StopInput();}void EnabledOwner(object s,EventArgs e){if(owner!=null&&!owner.Enabled)StopInput();}void Activated(object s,EventArgs e){if(Visible&&Enabled)Focus();clock.Restart();}
 bool ActiveInput{get{return !disposed&&Visible&&Enabled&&ContainsFocus&&(owner==null||(owner.Enabled&&Form.ActiveForm==owner));}}
 void StopInput(){keys.Clear();route.Clear();pending=null;hover=null;walking=false;}
 void TickScene(){double dt=Math.Min(.05,clock.Elapsed.TotalSeconds);clock.Restart();AdvanceScene(dt,ActiveInput);}
 internal void AdvanceScene(double dt,bool active){
  if(disposed)return;if(!active){StopInput();Invalidate();return;}elapsed+=dt;animation+=dt;saveElapsed+=dt;
  if(bag||fullMap||newPrompt){walking=false;Invalidate();return;}
  if(travel<1){travel=Math.Min(1,travel+(float)(dt/.14));visualX=fromX+(State.playerX-fromX)*travel;visualY=fromY+(State.playerY-fromY)*travel;walking=true;}
  else {walking=false;int dx=0,dy=0;if(keys.Contains(Keys.A)||keys.Contains(Keys.Left))dx=-1;else if(keys.Contains(Keys.D)||keys.Contains(Keys.Right))dx=1;else if(keys.Contains(Keys.W)||keys.Contains(Keys.Up))dy=-1;else if(keys.Contains(Keys.S)||keys.Contains(Keys.Down))dy=1;
   if(dx!=0||dy!=0){route.Clear();pending=null;TryStep(State.playerX+dx,State.playerY+dy);}
   else if(route.Count>0){var p=route[0];route.RemoveAt(0);if(!TryStep(p.X,p.Y)){route.Clear();pending=null;}}
   else if(pending!=null){string id=pending;pending=null;var prop=Floor.props.FirstOrDefault(p=>p.id==id);if(prop!=null&&CanUse(prop))Interact(prop);}
  }
  if(saveElapsed>2){saveElapsed=0;SaveTrial();}Invalidate();
 }
 internal bool TryStep(int x,int y){if(Math.Abs(x-State.playerX)+Math.Abs(y-State.playerY)!=1||!Floor.CanWalk(x,y,false))return false;fromX=visualX;fromY=visualY;State.playerX=x;State.playerY=y;travel=0;walking=true;if(x!=fromX)left=x<fromX;Floor.Reveal(x,y);Play(Floor.Tile(x,y)==2||Floor.Tile(x,y)==4?"step-wood":"step-stone");return true;}
 int Distance(MillProp p,int x,int y){return Math.Abs(x-Math.Max(p.x,Math.Min(p.x+p.w-1,x)))+Math.Abs(y-Math.Max(p.y,Math.Min(p.y+p.h-1,y)));}
 internal bool CanUse(MillProp p){if(Distance(p,State.playerX,State.playerY)>2)return false;var path=Approach(p);return path!=null&&path.Count<=2;}
 List<Point> Approach(MillProp p){List<Point> best=null;for(int y=p.y-1;y<=p.y+p.h;y++)for(int x=p.x-1;x<=p.x+p.w;x++){if(Distance(p,x,y)>1||(!Floor.CanWalk(x,y,false)))continue;var route0=Floor.Path(new Point(State.playerX,State.playerY),new Point(x,y),false);if(route0!=null&&(best==null||route0.Count<best.Count))best=route0;}return best;}
 internal void UseProp(MillProp p){if(travel<1)return;if(CanUse(p)){route.Clear();pending=null;Interact(p);}else{var path=Approach(p);if(path==null){Notify("暂时无法到达，请先开门或换一条路。");return;}route=path;pending=p.id;}}
 internal void Interact(MillProp p){
  if(!p.interactive||!CanUse(p))return;
  if(p.kind=="door"){
   if(p.state==1&&p.Contains(State.playerX,State.playerY)){Notify("请先离开门口再关门。");return;}p.state=1-p.state;Play("door");Notify(p.state==1?"木门已打开。":"木门已关闭。");
  }else if(p.kind=="break"){
   if(p.state==1){Notify("只剩破碎残骸。");return;}p.state=1;Play(p.name.Contains("罐")?"ceramic":"wood");AddLoot(p.loot);Notify(p.name+"已打碎，拾取少量"+p.loot+"。");
  }else if(p.kind=="chest"||p.kind=="cabinet"||p.kind=="search"){
   if(p.state==1){Notify("这里已经搜过了。");return;}p.state=1;AddLoot(p.loot);Play("loot");Notify("获得："+p.loot+" ×1（试玩物资）。");
  }else if(p.kind=="push"){
   int dx=Math.Sign(p.x-State.playerX),dy=Math.Sign(p.y-State.playerY);if(dx!=0&&dy!=0){Notify("请站到木桶的正侧面推动。");return;}if(dx==0&&dy==0)return;int nx=p.x+dx,ny=p.y+dy;
   var r=Floor.rooms.FirstOrDefault(rr=>rr.Contains(p.x,p.y));bool safe=r!=null&&r.Contains(nx,ny)&&!Floor.rooms.Any(q=>q.CenterX==nx&&q.CenterY==ny)&&!Floor.props.Any(q=>q.kind=="door"&&Distance(q,nx,ny)<=1);
   if(!safe||!Floor.CanWalk(nx,ny,false)||Floor.props.Any(q=>q!=p&&q.Contains(nx,ny))){Notify("这里是通道或有障碍，请换一侧推动。");return;}p.x=nx;p.y=ny;if(!MillDungeonGenerator.MaintainsAccess(Floor,new Point(State.playerX,State.playerY))){p.x-=dx;p.y-=dy;Notify("木桶会堵住通路或互动位置，请换一个方向。");return;}Play("wood");Notify("木桶向前移动了一格。");
  }else if(p.kind=="lever"){
   State.waterOn=!State.waterOn;foreach(var f in State.floors)foreach(var lever in f.props.Where(q=>q.kind=="lever"))lever.state=State.waterOn?0:1;Play("lever");if(FlowChanged!=null)FlowChanged(State.waterOn);Notify(State.waterOn?"水闸开启，水流恢复，水车开始转动。":"水闸关闭，水流渐缓，检修平台保持可通行。");
  }else if(p.kind=="lamp"){
   p.state=1-p.state;Play("switch");Notify(p.state==0?"壁灯已点亮。":"壁灯已熄灭。");
  }else if(p.kind=="down"||p.kind=="up"){
   State.floor=p.kind=="down"?1:0;var f=Floor;State.playerX=p.kind=="down"?f.startX:f.endX;State.playerY=p.kind=="down"?f.startY:f.endY+1;visualX=State.playerX;visualY=State.playerY;travel=1;StopInput();Floor.Reveal(State.playerX,State.playerY);Play("door");Notify(State.floor==0?"返回磨坊上层。":"进入下层检修渠，深部水轮间等待探索。");
  }else if(p.kind=="exit"){
   SaveTrial();Play("door");if(Return!=null)Return();return;
  }else if(p.kind=="goal"||p.kind=="wheel"){
   State.reachedEnd=true;Play("lever");Notify("已抵达深部水轮间！本版只体验单人探索，终点不放置敌人。");
  }else {Play("switch");Notify("磨盘、粮斗与传动轴保留在原位，等待后续完整检修玩法。");}
  SaveTrial();Invalidate();
 }
 void AddLoot(string name){if(String.IsNullOrEmpty(name))return;int n;State.inventory.TryGetValue(name,out n);State.inventory[name]=n+1;}
 void Nearest(){var p=Floor.props.Where(q=>q.interactive&&CanUse(q)).OrderBy(q=>Distance(q,State.playerX,State.playerY)).ThenBy(q=>q.kind=="lamp"?1:0).FirstOrDefault();if(p==null)Notify("附近没有可互动的物件。");else UseProp(p);}
 void LayoutView(){
  viewport=new Rectangle(0,70,Width,Math.Max(1,Height-140));zoom=Math.Max(2,(int)Math.Ceiling(Math.Max(Width/(32f*15),viewport.Height/(32f*11))));EnsureVisuals();
  camX=Math.Max(0,Math.Min(Floor.width*32-Width/zoom,(visualX+.5f)*32-Width/(2*zoom)));
  camY=Math.Max(0,Math.Min(Floor.height*32-viewport.Height/zoom,(visualY+.5f)*32-viewport.Height/(2*zoom)-24));
  exitButton=new Rectangle(12,12,Math.Min(160,Width/4),44);bagButton=new Rectangle(Math.Max(180,Width-342),12,100,44);mapButton=new Rectangle(Math.Max(285,Width-234),12,100,44);newButton=new Rectangle(Math.Max(390,Width-126),12,114,44);
  yesButton=new Rectangle(Width/2-150,Height/2+55,140,44);noButton=new Rectangle(Width/2+10,Height/2+55,140,44);bagPrev=new Rectangle(Width/2-150,Height-140,140,36);bagNext=new Rectangle(Width/2+10,Height-140,140,36);
 }
 Point ScreenCell(Point p){return new Point((int)Math.Floor((p.X/zoom+camX)/32),(int)Math.Floor(((p.Y-viewport.Y)/zoom+camY)/32));}
 bool Known(int x,int y){return Floor.Inside(x,y)&&Floor.seen[y*Floor.width+x];}
 MillProp HoverProp(Point p){if(!viewport.Contains(p))return null;var world=new PointF(p.X/zoom+camX,(p.Y-viewport.Y)/zoom+camY);return Floor.props.Where(q=>q.interactive&&PropInSight(q)&&PropBounds(q).Contains(world)).OrderByDescending(q=>q.y+q.h).ThenBy(q=>q.kind=="lamp"?1:0).FirstOrDefault();}
 protected override void OnPaint(PaintEventArgs e){
  base.OnPaint(e);LayoutView();var g=e.Graphics;g.Clear(BackColor);g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.SmoothingMode=SmoothingMode.None;
  var state=g.Save();g.SetClip(viewport);g.TranslateTransform(-camX*zoom,viewport.Y-camY*zoom);g.ScaleTransform(zoom,zoom);
  int x0=Math.Max(0,(int)(camX/32)-2),x1=Math.Min(Floor.width-1,(int)((camX+Width/zoom)/32)+2),y0=Math.Max(0,(int)(camY/32)-3),y1=Math.Min(Floor.height-1,(int)((camY+viewport.Height/zoom)/32)+3);
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)if(InSight(x,y)&&Floor.Tile(x,y)>0)DrawFloor(g,x,y);
  foreach(var p in Floor.props.Where(p=>p.kind=="ground"&&PropInSight(p)&&p.x>=x0-3&&p.x<=x1+3&&p.y>=y0-3&&p.y<=y1+3))DrawProp(g,p);
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)if(InSight(x,y)&&WallAt(x,y)&&Floor.Tile(x,y-1)==0)DrawWall(g,x,y);
  var order=Floor.props.Where(p=>p.kind!="ground"&&PropInSight(p)&&p.x>=x0-4&&p.x<=x1+3&&p.y>=y0-4&&p.y<=y1+3).OrderBy(p=>p.y+p.h).ToList();bool heroDrawn=false;
  foreach(var p in order){if(!heroDrawn&&p.y+p.h>visualY+1){DrawHero(g);heroDrawn=true;}DrawProp(g,p);}if(!heroDrawn)DrawHero(g);
  if(route.Count>0)using(var pen=new Pen(Color.FromArgb(130,203,185,119),1))foreach(var p in route.Take(18))g.DrawRectangle(pen,p.X*32+11,p.Y*32+11,10,10);
  // South-facing foreground walls are drawn after feet-sorted furniture and actor.
  for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)if(InSight(x,y)&&WallAt(x,y)&&Floor.Tile(x,y-1)>0)DrawWall(g,x,y);
  DrawIllumination(g);g.Restore(state);DrawHud(g);if(bag)DrawBag(g);if(fullMap)DrawMap(g,true);if(newPrompt)DrawNewPrompt(g);
 }
 bool WallAt(int x,int y){return Floor.Wall(x,y)>0;}
 void DrawFloor(Graphics g,int x,int y){
  int t=Floor.Tile(x,y);uint hash=unchecked((uint)(x*73856093^y*19349663^State.seed));int variant=(int)(hash%4);var r=Floor.rooms.FirstOrDefault(q=>q.Contains(x,y));bool damp=r!=null&&(r.type=="wheel"||r.type=="drain"||r.type=="terminal"||r.type=="cellar");
  string art=t==2||t==4?"01-ground-"+(hash%7==0?6+variant%3:5).ToString("00"):damp?"01-ground-"+(25+variant).ToString("00"):hash%13==0?"01-ground-"+(29+variant).ToString("00"):"01-ground-"+(1+(hash%9==0?variant:0)).ToString("00");
  g.DrawImage(ImageFor("01-ground-01"),new Rectangle(x*32,y*32,32,32));g.DrawImage(ImageFor(art),new Rectangle(x*32,y*32,32,32));if(t==2&&hash%(r!=null&&(r.type=="guard"||r.type=="ledger")?31:9)==0)g.DrawImage(ImageFor("09-extra-mill-structure-light-45"),new Rectangle(x*32,y*32,32,32),new Rectangle(10,10,44,44),GraphicsUnit.Pixel);if(t==3)DrawWater(g,x,y);
  DrawFloorDetail(g,x,y,r==null?"":r.type,hash);
  using(var shade=new SolidBrush(Color.FromArgb(90,8,10,16))){if(Floor.Tile(x,y-1)==0)g.FillRectangle(shade,x*32,y*32,32,10);if(Floor.Tile(x-1,y)==0)g.FillRectangle(shade,x*32,y*32,5,32);if(Floor.Tile(x+1,y)==0)g.FillRectangle(shade,x*32+28,y*32,4,32);if(Floor.Tile(x,y+1)==0)g.FillRectangle(shade,x*32,y*32+28,32,4);}
  if(t==2||t==4)using(var edge=new Pen(Color.FromArgb(105,73,49),2)){if(Floor.Tile(x,y-1)==1)g.DrawLine(edge,x*32,y*32+1,x*32+32,y*32+1);if(Floor.Tile(x-1,y)==1)g.DrawLine(edge,x*32+1,y*32,x*32+1,y*32+32);}
 }
 void DrawWall(Graphics g,int x,int y){
  var wall=ImageFor("02-structure-01");int px=x*32,py=y*32;bool front=Floor.Tile(x,y-1)>0;int top=front?py-7:py;
  g.DrawImage(wall,new Rectangle(px,top-5,32,44),new Rectangle(16,23,32,30),GraphicsUnit.Pixel);
  using(var dark=new SolidBrush(Color.FromArgb(92,8,10,14)))g.FillRectangle(dark,px,top+25,32,14);
  // Use the stone cap from the existing module continuously; do not repeat its end posts per cell.
  if(!WallAt(x,y-1)||front){g.DrawImage(wall,new Rectangle(px,top-6,32,12),new Rectangle(16,18,32,7),GraphicsUnit.Pixel);using(var shadow=new Pen(Color.FromArgb(114,8,10,15),2))g.DrawLine(shadow,px,top+7,px+32,top+7);}
  int boundary=Floor.Wall(x,y);if((boundary&2)!=0)DrawSideCap(g,wall,px+32,top);if((boundary&8)!=0)DrawSideCap(g,wall,px+6,top);
  using(var edge=new Pen(Color.FromArgb(135,20,23,26),1)){if(Floor.Tile(x+1,y)>0)g.DrawLine(edge,px+31,top,px+31,top+38);if(Floor.Tile(x-1,y)>0)g.DrawLine(edge,px,top,px,top+38);}
 }
 void DrawSideCap(Graphics g,Bitmap wall,int x,int y){var state=g.Save();g.TranslateTransform(x,y);g.RotateTransform(90);g.DrawImage(wall,new Rectangle(0,0,39,6),new Rectangle(16,18,32,7),GraphicsUnit.Pixel);g.Restore(state);}
 void DrawWater(Graphics g,int x,int y){
  using(var ink=new SolidBrush(Color.FromArgb(29,58,71)))g.FillRectangle(ink,x*32,y*32,32,32);
  for(int j=0;j<4;j++)using(var ink=new SolidBrush(Color.FromArgb(18+j*4,52,111,124)))g.FillRectangle(ink,x*32+j*2,y*32,32-j*4,32);
  int shift=State.waterOn?(int)(animation*13)%16:0;for(int i=0;i<3;i++){int yy=y*32+((i*11+shift+x*3)%32);using(var pen=new Pen(Color.FromArgb(120,108,158,164),1))g.DrawLine(pen,x*32+3+(i*5)%9,yy,x*32+15+(i*7)%12,yy);}
  using(var pen=new Pen(Color.FromArgb(90,19,37,43),1))g.DrawLine(pen,x*32,y*32,x*32,y*32+32);
  using(var bank=new SolidBrush(Color.FromArgb(60,70,68)))using(var dark=new SolidBrush(Color.FromArgb(115,10,23,28))){if(Floor.Tile(x-1,y)!=3){g.FillRectangle(bank,x*32,y*32,3,32);g.FillRectangle(dark,x*32+3,y*32,3,32);}if(Floor.Tile(x+1,y)!=3){g.FillRectangle(bank,x*32+29,y*32,3,32);g.FillRectangle(dark,x*32+26,y*32,3,32);}if(Floor.Tile(x,y-1)!=3)g.FillRectangle(bank,x*32,y*32,32,3);if(Floor.Tile(x,y+1)!=3)g.FillRectangle(bank,x*32,y*32+29,32,3);}
 }
 void DrawHero(Graphics g){float px=(visualX+.5f)*32,py=(visualY+1)*32;using(var shade=new SolidBrush(Color.FromArgb(90,0,0,0)))g.FillEllipse(shade,px-9,py-5,18,7);var s=g.Save();g.TranslateTransform(px,py);g.ScaleTransform(left?-1:1,1);
  if(walking&&walk.Count>0){float k=48f/136;g.DrawImage(walk[(int)(animation*10)%walk.Count],new RectangleF(-70*k,-146*k,136*k,152*k));}
  else {var hero=images["hero"];g.DrawImage(hero,new RectangleF(-11,-48,22,48));}g.Restore(s);
 }
 RectangleF PropBounds(MillProp p){
  float w=Math.Max(40,p.w*32+8),h=Math.Max(w,p.h*32+10);float x=p.x*32+p.w*16-w/2,y=(p.y+p.h)*32-h;
  if(p.kind=="ground"){w=p.w*32;h=p.h*32;x=p.x*32;y=p.y*32;}
  if(p.kind=="door"){w=32;h=p.art=="02-structure-25"?44:56;x=p.x*32;y=(p.y+p.h)*32-h;}
  if(p.kind=="wheel"){w=96;h=96;x=p.x*32-16;y=(p.y+p.h)*32-h;}
  return new RectangleF(x,y,w,h);
 }
 void DrawProp(Graphics g,MillProp p){
  string art=p.state==1&&!String.IsNullOrEmpty(p.alternate)?p.alternate:p.art;
  var bounds=PropBounds(p);float x=bounds.X,y=bounds.Y,w=bounds.Width,h=bounds.Height;
  DrawFurnitureShadow(g,p);if(p.kind=="door")DrawDoorFrame(g,p);
  var img=ImageFor(art);g.DrawImage(img,new RectangleF(x,y,w,h));
  if(p.kind=="machine"&&p.name.Contains("石磨"))DrawMillStoneTop(g,p,bounds);
  if(p.kind=="wheel")DrawWheelSpray(g,p,bounds);
  if(p.kind=="lamp"&&p.state==0){
   
   string fire="06-effects-"+(17+(int)(animation*8+p.x)%6).ToString("00");g.DrawImage(ImageFor(fire),new RectangleF(x+w*.35f,y+h*.13f,12,16));
  }
  if(p.kind=="wheel"&&State.waterOn){float cx=x+w*.48f,cy=y+h*.45f;using(var pen=new Pen(Color.FromArgb(130,143,106,64),2)){for(int i=0;i<8;i++){double a=animation*.7+i*Math.PI/4;g.DrawLine(pen,cx,cy,cx+(float)Math.Cos(a)*30,cy+(float)Math.Sin(a)*30);}}}
  if((hover==p.id||labels)&&p.interactive){using(var pen=new Pen(hover==p.id?Color.FromArgb(236,204,120):Color.FromArgb(112,165,160,117),1))g.DrawRectangle(pen,p.x*32,p.y*32,p.w*32,p.h*32);}
 }
 new void Text(Graphics g,string text,Rectangle r,int size,Color color,TextFormatFlags extra=TextFormatFlags.Default){using(var f=GameTheme.Body(size))GameTheme.DrawText(g,text,f,r,color,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis|extra);}
 void Button(Graphics g,Rectangle r,string text,bool active){GuildChrome.Draw(g,r,active);Text(g,text,r,11,GuildChrome.Ivory,TextFormatFlags.HorizontalCenter);}
 void DrawHud(Graphics g){
  using(var shade=new SolidBrush(Color.FromArgb(246,17,35,31))){g.FillRectangle(shade,0,0,Width,70);g.FillRectangle(shade,0,Height-70,Width,70);}
  Button(g,exitButton,"返回麦溪村",false);Button(g,bagButton,"背包 B",bag);Button(g,mapButton,"地图 M",fullMap);Button(g,newButton,"更换布局 N",newPrompt);
  int available=bagButton.Left-exitButton.Right-16;if(available>100)Text(g,"水车磨坊 · "+(State.floor==0?"上层":"下层"),new Rectangle(exitButton.Right+8,12,available,44),12,GuildChrome.Ivory);
  string room=Floor.rooms.FirstOrDefault(r=>r.Contains(State.playerX,State.playerY))==null?"连接通道":Floor.rooms.First(r=>r.Contains(State.playerX,State.playerY)).name;
  string line=elapsed<noticeUntil?notice:"WASD / 点击移动 · E互动 · B背包 · M地图 · Tab显示物件";
  var target=Floor.props.FirstOrDefault(p=>p.id==hover);if(target!=null&&!bag&&!fullMap&&!newPrompt)line=target.name+" · "+(target.state==1&&(target.kind=="chest"||target.kind=="break"||target.kind=="search"||target.kind=="cabinet")?"已处理":CanUse(target)?"E 或点击互动":"点击走近互动");
  Text(g,line,new Rectangle(16,Height-68,Width-32,34),11,GuildChrome.Ivory);Text(g,room+"  ·  布局 "+State.seed+"  ·  仅男主探索"+(State.reachedEnd?"  ·  已抵达终点":""),new Rectangle(16,Height-34,Width-32,28),9,Color.FromArgb(163,172,145));
 }
 void Panel(Graphics g,Rectangle r){GuildChrome.Draw(g,r);}
 void DrawBag(Graphics g){var r=new Rectangle(Math.Max(12,Width/2-260),95,Math.Min(520,Width-24),Math.Max(250,Height-190));Panel(g,r);Text(g,"探索物资 · 试玩独立背包（B关闭）",new Rectangle(r.X+20,r.Y+12,r.Width-40,40),14,GuildChrome.Ivory);var items=State.inventory.OrderBy(p=>p.Key).ToArray();int pages=Math.Max(1,(items.Length+5)/6);bagPage=Math.Max(0,Math.Min(pages-1,bagPage));if(items.Length==0)Text(g,"尚未搜集物资。点击箱柜或靠近后按 E。",new Rectangle(r.X+20,r.Y+70,r.Width-40,60),11,GuildChrome.Ivory);for(int i=0;i<6&&bagPage*6+i<items.Length;i++){var item=items[bagPage*6+i];Text(g,item.Key+" × "+item.Value,new Rectangle(r.X+26,r.Y+64+i*38,r.Width-52,35),12,GuildChrome.Ivory);}if(items.Length>6){Button(g,bagPrev,"上一页",false);Button(g,bagNext,"下一页",false);}}
 void DrawMap(Graphics g,bool large){DrawExplorationAtlas(g);}
 void DrawNewPrompt(Graphics g){var r=new Rectangle(Math.Max(10,Width/2-250),Height/2-120,Math.Min(500,Width-20),240);Panel(g,r);Text(g,"重新生成试验布局？",new Rectangle(r.X+20,r.Y+15,r.Width-40,38),15,GuildChrome.Ivory);Text(g,"将重置本次房间、容器和试玩背包。\n不影响主线进度、金币或角色装备。",new Rectangle(r.X+20,r.Y+58,r.Width-40,100),12,GuildChrome.Ivory,TextFormatFlags.WordBreak);Button(g,yesButton,"确认新布局",true);Button(g,noButton,"保留当前",false);}
 void Toggle(ref bool value){StopInput();value=!value;Play("switch");Invalidate();}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);LayoutView();hover=bag||fullMap||newPrompt?null:HoverProp(e.Location)==null?null:HoverProp(e.Location).id;Cursor=hover!=null||exitButton.Contains(e.Location)||bagButton.Contains(e.Location)||mapButton.Contains(e.Location)||newButton.Contains(e.Location)?Cursors.Hand:Cursors.Default;Invalidate();}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hover=null;Cursor=Cursors.Default;Invalidate();}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left)return;Focus();LayoutView();
  if(newPrompt){if(yesButton.Contains(e.Location)){State=MillDungeonGenerator.Create(unchecked(Environment.TickCount&Int32.MaxValue));visualX=State.playerX;visualY=State.playerY;travel=1;newPrompt=false;StopInput();if(FlowChanged!=null)FlowChanged(true);SaveTrial();Notify("已更换成组摆设与可选支路。建筑功能与房间尺度保持不变。");}else if(noButton.Contains(e.Location)){newPrompt=false;Invalidate();}return;}
  if(exitButton.Contains(e.Location)){StopInput();SaveTrial();if(Return!=null)Return();return;}if(bagButton.Contains(e.Location)){fullMap=false;Toggle(ref bag);return;}if(mapButton.Contains(e.Location)){bag=false;Toggle(ref fullMap);return;}if(newButton.Contains(e.Location)){StopInput();newPrompt=true;bag=fullMap=false;Invalidate();return;}
  if(bag){if(bagPrev.Contains(e.Location))bagPage--;if(bagNext.Contains(e.Location))bagPage++;Invalidate();return;}if(fullMap)return;
  var p=HoverProp(e.Location);if(p!=null){UseProp(p);return;}if(!viewport.Contains(e.Location))return;var cell=ScreenCell(e.Location);if(!InSight(cell.X,cell.Y)){Notify("先沿已看见的通路探索。");return;}var path=Floor.Path(new Point(State.playerX,State.playerY),cell,false);if(path!=null&&path.All(p0=>InSight(p0.X,p0.Y))){keys.Clear();route=path;pending=null;}else Notify("这里有墙、深水或关闭的门。");
 }
 protected override bool IsInputKey(Keys k){var key=k&Keys.KeyCode;return key==Keys.Left||key==Keys.Right||key==Keys.Up||key==Keys.Down||key==Keys.Tab||base.IsInputKey(k);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.Control||e.Alt||!ActiveInput)return;if(keyRepeat&&!keys.Contains(e.KeyCode)){e.Handled=e.SuppressKeyPress=true;return;}bool repeat=keys.Contains(e.KeyCode);keys.Add(e.KeyCode);if(e.KeyCode==Keys.B&&!repeat){fullMap=false;Toggle(ref bag);}else if(e.KeyCode==Keys.M&&!repeat){bag=false;Toggle(ref fullMap);}else if(e.KeyCode==Keys.N&&!repeat){StopInput();newPrompt=true;bag=fullMap=false;Invalidate();}else if(e.KeyCode==Keys.E&&!repeat&&!bag&&!fullMap&&!newPrompt)Nearest();else if(e.KeyCode==Keys.Tab){labels=true;Invalidate();}if(e.KeyCode==Keys.W||e.KeyCode==Keys.A||e.KeyCode==Keys.S||e.KeyCode==Keys.D||e.KeyCode==Keys.Left||e.KeyCode==Keys.Right||e.KeyCode==Keys.Up||e.KeyCode==Keys.Down||e.KeyCode==Keys.B||e.KeyCode==Keys.M||e.KeyCode==Keys.N||e.KeyCode==Keys.E||e.KeyCode==Keys.Tab)e.Handled=e.SuppressKeyPress=true;}
 protected override bool ProcessKeyEventArgs(ref Message m){bool old=keyRepeat;keyRepeat=(m.Msg==0x100||m.Msg==0x104)&&(m.LParam.ToInt64()&(1L<<30))!=0;try{return base.ProcessKeyEventArgs(ref m);}finally{keyRepeat=old;}}
 protected override void OnKeyUp(KeyEventArgs e){base.OnKeyUp(e);keys.Remove(e.KeyCode);if(e.KeyCode==Keys.Tab){labels=false;Invalidate();}}
 protected override void OnLostFocus(EventArgs e){StopInput();base.OnLostFocus(e);}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);clock.Restart();}
 protected override void OnParentChanged(EventArgs e){base.OnParentChanged(e);Bind();}
 protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);Bind();clock.Restart();timer.Start();}
 protected override void OnHandleDestroyed(EventArgs e){timer.Stop();StopInput();base.OnHandleDestroyed(e);}
 protected override void Dispose(bool disposing){if(disposing&&!disposed){SaveTrial();disposed=true;timer.Stop();timer.Dispose();if(owner!=null){owner.Deactivate-=Deactivated;owner.EnabledChanged-=EnabledOwner;owner.Activated-=Activated;}DisposeVisuals();foreach(var b in images.Values)b.Dispose();foreach(var b in walk)b.Dispose();Return=null;Sound=null;FlowChanged=null;}base.Dispose(disposing);}
}


