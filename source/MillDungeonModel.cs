using System;using System.Collections.Generic;using System.Drawing;using System.Linq;

// No story completion, party state, enemy or combat data is touched by this standalone exploration trial.
public sealed class MillProp {
 public string group{get;set;}
 public string id{get;set;} public string kind{get;set;} public string name{get;set;}
 public string art{get;set;} public string alternate{get;set;}
 public int x{get;set;} public int y{get;set;} public int w{get;set;} public int h{get;set;}
 public int state{get;set;} public bool blocking{get;set;} public bool interactive{get;set;}
 public string loot{get;set;}
 public bool Contains(int px,int py){return px>=x&&py>=y&&px<x+w&&py<y+h;}
}
public sealed class MillRoom {
 public string[] mask{get;set;}public int anchorX{get;set;}public int anchorY{get;set;}
 public string type{get;set;}public List<int> neighbors{get;set;}
 public int id{get;set;}public int x{get;set;}public int y{get;set;}public int w{get;set;}public int h{get;set;}public string name{get;set;}
 public int CenterX{get{return anchorX;}}public int CenterY{get{return anchorY;}}
 public bool Contains(int px,int py){return px>=x&&py>=y&&px<x+w&&py<y+h&&mask!=null&&mask[py-y][px-x]=='1';}
}
public sealed class MillFloor {
 public int visualRevision{get;set;}
 public int[] walls{get;set;}public int Wall(int x,int y){return Inside(x,y)&&walls!=null?walls[y*width+x]:0;}
 public List<MillPortal> portals{get;set;}
 public int width{get;set;}public int height{get;set;}public int[] tiles{get;set;}public bool[] seen{get;set;}
 public List<MillRoom> rooms{get;set;}public List<MillProp> props{get;set;}
 public int startX{get;set;}public int startY{get;set;}public int endX{get;set;}public int endY{get;set;}
 public bool Inside(int x,int y){return x>=0&&y>=0&&x<width&&y<height;}
 public int Tile(int x,int y){return Inside(x,y)?tiles[y*width+x]:0;}
 public bool CanWalk(int x,int y,bool ignoreDoors){
  int tile=Tile(x,y);if(tile==0||tile==3)return false;
  foreach(var p in props)if(p.blocking&&p.Contains(x,y)&&!(p.kind=="door"&&(p.state==1||ignoreDoors))&&!(p.kind=="break"&&p.state==1))return false;
  return true;
 }
 public void Reveal(int x,int y){
  for(int yy=y-8;yy<=y+8;yy++)for(int xx=x-8;xx<=x+8;xx++)if(Inside(xx,yy)&&(xx-x)*(xx-x)+(yy-y)*(yy-y)<=64&&VisibleFrom(x,y,xx,yy))seen[yy*width+xx]=true;
 }
 bool VisibleFrom(int x,int y,int tx,int ty){int dx=Math.Abs(tx-x),dy=-Math.Abs(ty-y),sx=x<tx?1:-1,sy=y<ty?1:-1,err=dx+dy;while(x!=tx||y!=ty){if(Tile(x,y)==0||props.Any(p=>p.kind=="door"&&p.state==0&&p.Contains(x,y)))return false;int e2=2*err;if(e2>=dy){err+=dy;x+=sx;}if(e2<=dx){err+=dx;y+=sy;}}return true;}
 public List<Point> Path(Point start,Point end,bool ignoreDoors){
  if(!Inside(end.X,end.Y)||!CanWalk(end.X,end.Y,ignoreDoors))return null;
  int[] prev=new int[width*height];for(int i=0;i<prev.Length;i++)prev[i]=-1;
  int s=start.Y*width+start.X,t=end.Y*width+end.X;if(s<0||s>=prev.Length)return null;
  var q=new Queue<int>();q.Enqueue(s);prev[s]=s;int[] dx={0,1,0,-1},dy={-1,0,1,0};
  while(q.Count>0&&prev[t]<0){int n=q.Dequeue();int x=n%width,y=n/width;for(int i=0;i<4;i++){int xx=x+dx[i],yy=y+dy[i];if(!Inside(xx,yy)||!CanWalk(xx,yy,ignoreDoors))continue;int v=yy*width+xx;if(prev[v]>=0)continue;prev[v]=n;q.Enqueue(v);}}
  if(prev[t]<0)return null;var path=new List<Point>();for(int n=t;n!=s;n=prev[n])path.Add(new Point(n%width,n/width));path.Reverse();return path;
 }
}
public sealed class MillTrialState {
 public int version{get;set;}public int seed{get;set;}public int floor{get;set;}public int playerX{get;set;}public int playerY{get;set;}
 public bool waterOn{get;set;}public bool reachedEnd{get;set;}public List<MillFloor> floors{get;set;}public Dictionary<string,int> inventory{get;set;}
}
public sealed class MillPortal {
 public int a{get;set;}public int b{get;set;}public int x{get;set;}public int y{get;set;}public int w{get;set;}public int h{get;set;}
 public bool optional{get;set;}
}
// Version 3: room membership is an authored union mask. Bounds are broad phase only.
// Corridors are independent stepped footprints, not center-to-center rectangle links.
public static partial class MillDungeonGenerator {
 static Rectangle R(int x,int y,int w,int h){return new Rectangle(x,y,w,h);}
 static void Paint(MillFloor f,Rectangle r,int tile){for(int y=r.Top;y<r.Bottom;y++)for(int x=r.Left;x<r.Right;x++){if(!f.Inside(x,y))throw new Exception("Geometry exceeds map");f.tiles[y*f.width+x]=tile;}}
 static MillRoom Region(MillFloor f,string type,string name,int ax,int ay,int material,params Rectangle[] pieces){
  var b=pieces[0];foreach(var p in pieces)b=Rectangle.Union(b,p);var rows=new string[b.Height];for(int y=0;y<b.Height;y++){char[] row=new char[b.Width];for(int x=0;x<b.Width;x++)row[x]=pieces.Any(p=>p.Contains(x+b.X,y+b.Y))?'1':'0';rows[y]=new string(row);}
  var r=new MillRoom{id=f.rooms.Count,type=type,name=name,x=b.X,y=b.Y,w=b.Width,h=b.Height,mask=rows,anchorX=ax,anchorY=ay,neighbors=new List<int>()};if(!r.Contains(ax,ay))throw new Exception("Anchor outside "+name);foreach(var p in pieces)Paint(f,p,material);f.rooms.Add(r);return r;
 }
 static MillProp Prop(MillFloor f,string kind,string name,string art,string alt,int x,int y,int w,int h,bool blocking,bool use,string loot){var p=new MillProp{id="p"+f.props.Count,kind=kind,name=name,art=art,alternate=alt,x=x,y=y,w=w,h=h,blocking=blocking,interactive=use,loot=loot};f.props.Add(p);return p;}
 static void Passage(MillFloor f,int a,int b,bool optional,params Rectangle[] bends){foreach(var r in bends)Paint(f,r,1);f.rooms[a].neighbors.Add(b);f.rooms[b].neighbors.Add(a);var p=bends[0];f.portals.Add(new MillPortal{a=a,b=b,x=p.X,y=p.Y,w=p.Width,h=p.Height,optional=optional});}
 public static void UpgradeDoors(MillTrialState state){for(int i=0;i<state.floors.Count;i++){var f=state.floors[i];NarrowDoors(f,i==state.floor?new Point?(new Point(state.playerX,state.playerY)):null);BuildWalls(f);}}
 static void NarrowDoors(MillFloor f,Point? player){foreach(var p in f.props.Where(p=>p.kind=="door"&&(p.w>1||p.h>1))){int oldW=p.w,oldH=p.h;int keepX=p.x,keepY=p.y;if(player.HasValue&&p.Contains(player.Value.X,player.Value.Y)){keepX=player.Value.X;keepY=player.Value.Y;}for(int y=p.y;y<p.y+oldH;y++)for(int x=p.x;x<p.x+oldW;x++)if(x!=keepX||y!=keepY){f.tiles[y*f.width+x]=0;foreach(var r in f.rooms.Where(r=>r.Contains(x,y))){var row=r.mask[y-r.y].ToCharArray();row[x-r.x]='0';r.mask[y-r.y]=new string(row);}}p.x=keepX;p.y=keepY;p.w=1;p.h=1;}}
 static void Door(MillFloor f,int x,int y,int w,int h){Prop(f,"door","木门",h>1?"02-structure-25":"02-structure-23",h>1?"02-structure-26":"02-structure-24",x,y,w,h,true,true,null);}
 public static MillTrialState CreateStructure(int seed){return CreateInternal(seed,false);}
 public static MillTrialState Create(int seed){return CreateInternal(seed,true);}
 static MillTrialState CreateInternal(int seed,bool furniture){var s=new MillTrialState{version=3,seed=seed,floor=0,waterOn=true,floors=new List<MillFloor>(),inventory=new Dictionary<string,int>()};for(int i=0;i<2;i++)s.floors.Add(Generate(seed,i,furniture));if(furniture)UpgradeVisuals(s);s.playerX=s.floors[0].startX;s.playerY=s.floors[0].startY;s.floors[0].Reveal(s.playerX,s.playerY);return s;}
 static MillFloor Generate(int seed,int level,bool furniture){
  var f=new MillFloor{width=48,height=50,tiles=new int[48*50],walls=new int[48*50],seen=new bool[48*50],rooms=new List<MillRoom>(),props=new List<MillProp>(),portals=new List<MillPortal>()};
  if(level==0){
   Region(f,"entry","折角接货前厅",10,40,1,R(5,36,7,6),R(10,38,6,7));
   Region(f,"guard","值守室",6,31,2,R(3,28,6,6));
   Region(f,"grain","L形储粮仓",8,23,2,R(3,15,7,11),R(8,21,8,6),R(2,23,3,2));
   Region(f,"ledger","扩建账房",23,37,2,R(19,34,8,5),R(21,38,7,4));
   Region(f,"tools","侧延维修间",32,26,1,R(28,23,8,6),R(33,27,7,5));
   Region(f,"mill","T形主磨坊",21,10,1,R(13,3,15,5),R(17,6,10,10),R(13,10,5,5),R(27,9,3,3));
   Region(f,"wheel","错台水车机械间",36,14,1,R(34,6,7,7),R(32,11,10,6),R(35,16,8,6));
   Region(f,"stairs","石砌下行楼梯间",7,10,1,R(5,4,7,5),R(3,8,9,5));
   Passage(f,0,1,false,R(6,34,2,2));Door(f,6,34,2,1);
   Passage(f,1,2,true,R(5,26,2,2));Door(f,5,27,2,1);
   // Loading route swells into a short landing, turns twice, and narrows beside a load-bearing pier.
   Passage(f,0,5,false,R(10,31,6,5),R(16,31,5,2),R(19,28,3,4),R(21,25,6,3),R(20,18,2,8),R(19,16,4,3));Door(f,10,35,2,1);Door(f,20,19,2,1);
   Passage(f,2,0,false,R(14,27,2,5));Door(f,14,27,2,1);
   Passage(f,0,3,false,R(21,31,4,2),R(23,33,2,1));Door(f,23,33,2,1);
   Passage(f,5,4,false,R(27,25,1,2));Door(f,27,25,1,2);
   Passage(f,5,6,false,R(30,10,4,2));Door(f,31,10,1,2);
   Passage(f,5,7,false,R(12,10,1,2));Door(f,12,10,1,2);
   if((seed&1)!=0){Passage(f,4,3,true,R(29,29,2,8),R(27,35,3,2));Door(f,29,29,2,1);}
   // A two-stage canal follows the mill's later eastern extension; bridges remain real floor cells.
   Paint(f,R(39,7,2,9),3);Paint(f,R(40,16,2,5),3);Paint(f,R(39,13,2,2),4);Paint(f,R(40,18,2,2),4);
   PaintInside(f,f.rooms[5],R(17,7,8,8),2);PaintInside(f,f.rooms[5],R(13,10,5,5),2);
   PaintInside(f,f.rooms[4],R(28,23,7,2),2);
  }else{
   Region(f,"stairs","上行石阶与拱廊",7,10,1,R(5,4,7,5),R(3,8,9,5));
   Region(f,"cellar","阶梯酒桶地窖",8,21,1,R(3,17,7,6),R(6,21,8,7),R(11,25,4,4));
   Region(f,"grain","深凹干储库",22,17,2,R(16,13,10,5),R(20,17,7,6));
   Region(f,"tools","加固检修间",34,10,1,R(29,5,8,6),R(33,9,7,6));
   Region(f,"ledger","旧水务账房",35,22,2,R(32,19,7,6),R(30,21,3,3));
   Region(f,"drain","阶梯排水平台",29,30,1,R(21,27,11,5),R(25,30,9,6),R(31,33,6,4));
   Region(f,"terminal","深部水轮检修厅",36,43,1,R(34,39,7,5),R(29,42,12,5),R(38,43,5,5));
   Region(f,"cache","封堵后的侧藏室",11,37,1,R(5,34,9,4),R(9,37,9,5));
   Passage(f,0,1,false,R(5,13,2,4),R(5,16,4,2));Door(f,5,14,2,1);
   Passage(f,0,2,false,R(11,11,3,2),R(13,12,2,4),R(14,14,2,2));Door(f,14,14,1,2);
   Passage(f,2,3,false,R(25,12,5,2),R(28,9,2,4));Door(f,28,11,2,1);
   Passage(f,3,4,false,R(35,15,2,4));Door(f,35,17,2,1);
   Passage(f,2,5,false,R(22,23,2,4),R(22,26,4,2));Door(f,22,24,2,1);
   Passage(f,1,5,true,R(15,27,6,2));Door(f,17,27,1,2);
   Passage(f,1,7,false,R(11,29,2,5));Door(f,11,31,2,1);
   Passage(f,5,6,false,R(34,37,2,2));Door(f,34,38,2,1);
   Paint(f,R(31,28,1,5),3);Paint(f,R(32,32,1,3),3);Paint(f,R(31,30,1,2),4);Paint(f,R(39,40,1,6),3);Paint(f,R(39,43,1,2),4);
  }
  NarrowDoors(f,null);BuildWalls(f);f.startX=f.rooms[0].CenterX;f.startY=f.rooms[0].CenterY;int endId=level==0?7:6;f.endX=f.rooms[endId].CenterX;f.endY=f.rooms[endId].CenterY;
  Prop(f,level==0?"exit":"up",level==0?"返回麦溪村":"返回磨坊上层","02-structure-35","",f.startX,f.startY+1,1,1,false,true,null);
  Prop(f,level==0?"down":"goal",level==0?"进入下层检修渠":"深部水轮检修台","02-structure-36","",f.endX,f.endY,1,1,false,true,null);
  foreach(var r in f.rooms)if(f.Path(new Point(f.startX,f.startY),new Point(r.CenterX,r.CenterY),true)==null)throw new Exception("Geometry disconnected: "+r.name);
  if(furniture)Furnish(f,seed,level);
  for(int i=0;i<f.props.Count;i++)f.props[i].id="f"+level+"p"+i;
  return f;
 }
 static void PaintInside(MillFloor f,MillRoom r,Rectangle box,int tile){for(int y=box.Top;y<box.Bottom;y++)for(int x=box.Left;x<box.Right;x++)if(r.Contains(x,y))f.tiles[y*f.width+x]=tile;}
 static void BuildWalls(MillFloor f){int[] dx={0,1,0,-1,1,1,-1,-1},dy={-1,0,1,0,-1,1,1,-1};for(int y=0;y<f.height;y++)for(int x=0;x<f.width;x++)if(f.Tile(x,y)==0){int bits=0;for(int d=0;d<8;d++)if(f.Tile(x+dx[d],y+dy[d])>0)bits|=1<<d;f.walls[y*f.width+x]=bits;}}
 static void Furnish(MillFloor f,int seed,int level){
  var reserve=new bool[f.tiles.Length];var rng=new Random(unchecked(seed*733+level*19));
  foreach(var r in f.rooms){var path=f.Path(new Point(f.startX,f.startY),new Point(r.CenterX,r.CenterY),true);foreach(var p in path)reserve[p.Y*f.width+p.X]=true;reserve[r.CenterY*f.width+r.CenterX]=true;}
  foreach(var door in f.props.Where(p=>p.kind=="door")){var path=f.Path(new Point(f.startX,f.startY),new Point(door.x,door.y),true);if(path==null)throw new Exception("Door outside walk mask");foreach(var p in path)reserve[p.Y*f.width+p.X]=true;}
  // Preserve both ends of optional passages as well as the shortest critical route.
  for(int y=0;y<f.height;y++)for(int x=0;x<f.width;x++)if(f.Tile(x,y)>0&&!f.rooms.Any(r=>r.Contains(x,y)))foreach(var d in new[]{new Point(0,0),new Point(0,-1),new Point(1,0),new Point(0,1),new Point(-1,0)})if(f.Inside(x+d.X,y+d.Y))reserve[(y+d.Y)*f.width+x+d.X]=true;
  foreach(var target in level==0?new[]{new Point(3,24),new Point(26,41),new Point(38,30)}:new[]{new Point(25,21),new Point(16,40),new Point(37,46)}){var path=f.Path(new Point(f.startX,f.startY),target,true);if(path==null)throw new Exception("Nook route disconnected");foreach(var p in path)reserve[p.Y*f.width+p.X]=true;}
  reserve[f.startY*f.width+f.startX]=true;reserve[(f.startY+1)*f.width+f.startX]=true;
  foreach(var p in f.props)for(int y=p.y-(p.h==1?1:0);y<p.y+p.h+(p.h==1?1:0);y++)for(int x=p.x-(p.h>1?1:0);x<p.x+p.w+(p.h>1?1:0);x++)if(f.Inside(x,y))reserve[y*f.width+x]=true;
  foreach(var r in f.rooms){
   var north=Cells(r).Where(p=>f.Tile(p.X,p.Y-1)==0).OrderBy(p=>Math.Abs(p.X-r.CenterX)+Math.Abs(p.Y-r.CenterY)).First();Place(f,r,reserve,"lamp","壁灯","06-effects-09","06-effects-10",north.X,north.Y,1,1,null);
   switch(r.type){
    case "entry":
     Desk(f,r,reserve,r.x,r.y+1,"04-furniture-33","接货桌");Storage(f,r,reserve,r.x+r.w-2,r.y+r.h-2,rng);Storage(f,r,reserve,r.x,r.y+r.h-2,rng);
     Solid(f,r,reserve,"门边木梁","02-structure-33",5,36);Ground(f,r,reserve,"泥水","09-extra-mill-structure-light-61",11,42);break;
    case "guard":
     Place(f,r,reserve,"deco","值守木床","04-furniture-27","",3,29,1,2,null);Desk(f,r,reserve,7,28,"04-furniture-33","用餐桌");Container(f,r,reserve,"cabinet","个人柜","04-furniture-23","04-furniture-24",3,33,"旧钥匙");Storage(f,r,reserve,7,32,rng);break;
    case "grain":
     if(level==0){Solid(f,r,reserve,"内凹墙储物架","04-furniture-18",10,21,2,1);Solid(f,r,reserve,"折角箱堆","03-containers-13",10,24,2,1);}
     if(level==0)Container(f,r,reserve,"chest","储粮壁龛藏箱","03-containers-23","03-containers-24",2,24,"干粮");else Container(f,r,reserve,"chest","凹室物资箱","03-containers-23","03-containers-24",26,21,"干粮");
     Pack(f,r,reserve,"储粮货架","04-furniture-16",2,1,r.x,r.y);Pack(f,r,reserve,"装袋货架","04-furniture-18",2,1,r.x+r.w-2,r.y);
     for(int i=0;i<4;i++)Storage(f,r,reserve,i%2==0?r.x:r.x+r.w-2,i<2?r.y+3:r.y+r.h-2,rng);
     Pack(f,r,reserve,"粮袋堆","03-containers-35",2,1,r.x+3,r.y);Pack(f,r,reserve,"粮袋堆","03-containers-36",2,1,r.x+r.w-3,r.y+r.h-2);
     break;
    case "ledger":
     Container(f,r,reserve,"chest","旧宝箱","03-containers-25","03-containers-26",level==0?27:30,level==0?41:23,"银饰");
     Desk(f,r,reserve,r.x,r.y,"04-furniture-34","账本桌");Pack(f,r,reserve,"账册书架","04-furniture-13",2,1,r.x+r.w-2,r.y);Pack(f,r,reserve,"抽屉柜","04-furniture-25",1,1,r.x,r.y+r.h-1);Storage(f,r,reserve,r.x+r.w-2,r.y+r.h-2,rng);
     break;
    case "tools":
     Desk(f,r,reserve,r.x,r.y,"04-furniture-06","维修台");Pack(f,r,reserve,"工具挂架","04-furniture-19",2,1,r.x+r.w-2,r.y);Pack(f,r,reserve,"铁砧","04-furniture-38",1,1,r.x,r.y+2);
     Storage(f,r,reserve,r.x,r.y+r.h-2,rng);Pack(f,r,reserve,"器械架","04-furniture-22",2,1,r.x+r.w-2,r.y+r.h-1);
     Container(f,r,reserve,"cabinet","狭角工具箱","07-extra-containers-22","07-extra-containers-23",level==0?39:39,level==0?30:14,"修理零件");Pack(f,r,reserve,"备用齿轮","05-machinery-19",1,1,r.x+r.w-3,r.y+3);break;
    case "mill":
     Place(f,r,reserve,"machine","西侧石磨","05-machinery-10","",17,7,3,3,null);Place(f,r,reserve,"machine","东侧石磨","05-machinery-10","",23,12,2,2,null);
     Place(f,r,reserve,"machine","粮食漏斗","05-machinery-13","",17,6,2,1,null);Place(f,r,reserve,"machine","侧向粮斗","05-machinery-14","",24,7,2,1,null);
     Solid(f,r,reserve,"传动轴","05-machinery-16",21,5,3,1);Solid(f,r,reserve,"轴承座","05-machinery-23",26,9);Solid(f,r,reserve,"主齿轮","05-machinery-22",28,9);
     Desk(f,r,reserve,13,12,"09-extra-mill-structure-light-11","装卸操作台");Storage(f,r,reserve,13,14,rng);Storage(f,r,reserve,25,14,rng);Storage(f,r,reserve,13,3,rng);
     Pack(f,r,reserve,"筛粮架","09-extra-mill-structure-light-12",2,1,25,3);Pack(f,r,reserve,"粮筐","05-machinery-41",1,1,19,8);Ground(f,r,reserve,"面粉","03-containers-39",19,12);Ground(f,r,reserve,"散粮","03-containers-37",15,12);break;
    case "wheel":
     Place(f,r,reserve,"wheel","水车主轮","05-machinery-01","",39,7,2,2,null);Solid(f,r,reserve,"齿轮组","05-machinery-22",35,7,2,1);Solid(f,r,reserve,"传动轴","05-machinery-16",37,10,2,1);
     Desk(f,r,reserve,32,15,"04-furniture-36","检修平台工作台");Storage(f,r,reserve,35,20,rng);Solid(f,r,reserve,"工具架","04-furniture-20",35,17,2,1);
     Place(f,r,reserve,"lever","水闸控制杆","05-machinery-27","05-machinery-28",38,18,1,1,null);Ground(f,r,reserve,"盘链","05-machinery-35",36,15);break;
    case "stairs":
     Pack(f,r,reserve,"木梁支撑","02-structure-33",1,1,r.x,r.y+r.h-1);Pack(f,r,reserve,"木梁支撑","02-structure-33",1,1,r.x+r.w-1,r.y);
     Storage(f,r,reserve,r.x+r.w-2,r.y,rng);Pack(f,r,reserve,"旧陶罐","03-containers-46",1,1,r.x,r.y+r.h-2);Ground(f,r,reserve,"墙根苔藓","06-effects-49",5,9);break;
    case "cellar":
     for(int i=0;i<4;i++)Storage(f,r,reserve,i%2==0?r.x:r.x+r.w-2,i<2?r.y:r.y+r.h-2,rng);Pack(f,r,reserve,"大酒桶","03-containers-04",1,1,3,18);Pack(f,r,reserve,"倒桶","03-containers-02",2,1,12,27);break;
    case "drain":
     Solid(f,r,reserve,"加固木柱","02-structure-33",23,27);Solid(f,r,reserve,"加固木柱","02-structure-33",30,32);Desk(f,r,reserve,25,34,"04-furniture-36","排水维修台");Storage(f,r,reserve,21,28,rng);Storage(f,r,reserve,35,35,rng);Ground(f,r,reserve,"潮湿碎石","06-effects-34",29,32);break;
    case "terminal":
     Place(f,r,reserve,"wheel","水轮主轴","05-machinery-01","",39,40,2,2,null);Solid(f,r,reserve,"主轴齿轮","05-machinery-22",34,40,2,1);Desk(f,r,reserve,29,44,"04-furniture-36","终端检修台");Storage(f,r,reserve,40,46,rng);Storage(f,r,reserve,30,45,rng);Place(f,r,reserve,"lever","排水控制杆","05-machinery-27","05-machinery-28",38,46,1,1,null);break;
    case "cache":
     Storage(f,r,reserve,5,34,rng);Storage(f,r,reserve,9,40,rng);Pack(f,r,reserve,"封堵木梁","02-structure-48",2,1,5,37);Container(f,r,reserve,"chest","深凹藏箱","03-containers-23","03-containers-24",17,40,"补给");break;
   }
   var pot=Find(f,r,reserve,r.x+r.w-1,r.y+r.h-1,1,1);if(pot.HasValue)Container(f,r,reserve,"break","陶罐","03-containers-20","03-containers-22",pot.Value.X,pot.Value.Y,"陶片");
   // Small ground details stay attached to a real inside corner, never to the bounding rectangle's void.
   var corner=Cells(r).Where(p=>f.Tile(p.X,p.Y-1)==0&&f.Tile(p.X-1,p.Y)==0).FirstOrDefault();if(r.Contains(corner.X,corner.Y))Ground(f,r,reserve,"蛛网","06-effects-48",corner.X,corner.Y);
  }
  // A decorated nook is allowed only when its interaction side remains reachable.
  bool changed;do{changed=false;var reach=Flood(f);foreach(var p in f.props.Where(p=>p.interactive&&p.kind!="door"&&p.kind!="up"&&p.kind!="down"&&p.kind!="exit"&&p.kind!="goal").ToArray())if(!Adjacent(reach,p)){f.props.Remove(p);changed=true;}}while(changed);
  foreach(var r in f.rooms)if(f.Path(new Point(f.startX,f.startY),new Point(r.CenterX,r.CenterY),true)==null)throw new Exception("Furniture blocked "+r.name);
 }
 static IEnumerable<Point> Cells(MillRoom r){for(int y=r.y;y<r.y+r.h;y++)for(int x=r.x;x<r.x+r.w;x++)if(r.Contains(x,y))yield return new Point(x,y);}
 static HashSet<Point> Flood(MillFloor f){var found=new HashSet<Point>();var queue=new Queue<Point>();var a=new Point(f.startX,f.startY);found.Add(a);queue.Enqueue(a);while(queue.Count>0){var p=queue.Dequeue();foreach(var d in new[]{new Point(0,-1),new Point(1,0),new Point(0,1),new Point(-1,0)}){var n=new Point(p.X+d.X,p.Y+d.Y);if(f.CanWalk(n.X,n.Y,true)&&found.Add(n))queue.Enqueue(n);}}return found;}
 static bool Adjacent(HashSet<Point> reach,MillProp p){for(int y=p.y-1;y<=p.y+p.h;y++)for(int x=p.x-1;x<=p.x+p.w;x++)if(!p.Contains(x,y)&&reach.Contains(new Point(x,y))&&(x>=p.x&&x<p.x+p.w||y>=p.y&&y<p.y+p.h))return true;return false;}
 public static bool MaintainsAccess(MillFloor f,Point player){var reach=Flood(f);return reach.Contains(player)&&f.rooms.All(r=>reach.Contains(new Point(r.CenterX,r.CenterY)))&&f.props.Where(p=>p.interactive).All(p=>Adjacent(reach,p));}
 static bool Fits(MillFloor f,MillRoom r,bool[] reserve,int x,int y,int w,int h,bool ground,bool water){for(int yy=y;yy<y+h;yy++)for(int xx=x;xx<x+w;xx++)if(!r.Contains(xx,yy)||(!water&&f.Tile(xx,yy)==3)||(!ground&&(reserve[yy*f.width+xx]||f.props.Any(p=>p.blocking&&p.Contains(xx,yy)))))return false;return true;}
 static bool Place(MillFloor f,MillRoom r,bool[] reserve,string kind,string name,string art,string alt,int x,int y,int w,int h,string loot){bool ground=kind=="ground"||kind=="lamp";if(!Fits(f,r,reserve,x,y,w,h,ground,kind=="wheel"))return false;Prop(f,kind,name,art,alt,x,y,w,h,!ground,kind!="deco"&&kind!="ground",loot);return true;}
 static void Solid(MillFloor f,MillRoom r,bool[] s,string name,string art,int x,int y,int w=1,int h=1){Place(f,r,s,"deco",name,art,"",x,y,w,h,null);}
 static void Ground(MillFloor f,MillRoom r,bool[] s,string name,string art,int x,int y){Place(f,r,s,"ground",name,art,"",x,y,1,1,null);}
 static void Container(MillFloor f,MillRoom r,bool[] s,string kind,string name,string art,string alt,int x,int y,string loot){Place(f,r,s,kind,name,art,alt,x,y,1,1,loot);}
 static Point? Find(MillFloor f,MillRoom r,bool[] s,int ox,int oy,int w,int h){foreach(var p in Cells(r).OrderBy(p=>Math.Abs(p.X-ox)+Math.Abs(p.Y-oy))){if(!Fits(f,r,s,p.X,p.Y,w,h,false,false))continue;bool edge=f.Tile(p.X,p.Y-1)==0||f.Tile(p.X-1,p.Y)==0||f.Tile(p.X+w,p.Y)==0||f.Tile(p.X,p.Y+h)==0;if(edge)return p;}return null;}
 static void Pack(MillFloor f,MillRoom r,bool[] s,string name,string art,int w,int h,int ox,int oy){var p=Find(f,r,s,ox,oy,w,h);if(p.HasValue)Solid(f,r,s,name,art,p.Value.X,p.Value.Y,w,h);}
 static void Storage(MillFloor f,MillRoom r,bool[] s,int ox,int oy,Random rng){var p=Find(f,r,s,ox,oy,2,2);if(!p.HasValue)return;int x=p.Value.X,y=p.Value.Y;Solid(f,r,s,"粮袋",rng.Next(2)==0?"03-containers-32":"03-containers-33",x,y);Solid(f,r,s,"粮袋","03-containers-32",x+1,y);Place(f,r,s,"push","木桶","03-containers-01","",x,y+1,1,1,null);Container(f,r,s,"break","堆叠木箱",rng.Next(2)==0?"03-containers-12":"03-containers-14","03-containers-16",x+1,y+1,"木料");Ground(f,r,s,"散落谷物","03-containers-37",x,y+2);}
 static void Desk(MillFloor f,MillRoom r,bool[] s,int ox,int oy,string art,string name){var p=Find(f,r,s,ox,oy,2,2);if(!p.HasValue)return;int x=p.Value.X,y=p.Value.Y;Place(f,r,s,"search",name,art,art,x,y,2,1,"旧账页");Solid(f,r,s,"木凳","04-furniture-42",x,y+1);Ground(f,r,s,"杂物","04-furniture-45",x+1,y+1);}
}

