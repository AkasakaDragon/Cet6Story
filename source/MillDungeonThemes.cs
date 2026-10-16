using System;using System.Drawing;using System.Linq;using System.Collections.Generic;
public static partial class MillDungeonGenerator {
 // Additive, idempotent upgrade: v3 geometry, old IDs, opened/broken states and inventory survive.
 public static void UpgradeVisuals(MillTrialState state){UpgradeDoors(state);for(int level=0;level<state.floors.Count;level++){var f=state.floors[level];if(f.visualRevision>=4)continue;int first=f.props.Count;
  foreach(var r in f.rooms){foreach(var p in f.props.Where(p=>r.Contains(p.x,p.y))){p.group=r.type+":original";
   if(p.kind=="deco"&&p.name=="粮袋"&&r.type!="grain"&&r.type!="entry"&&r.type!="mill"){
    if(r.type=="ledger"){p.art="04-furniture-32";p.name="账册卷宗";}else if(r.type=="cellar"){p.art="03-containers-04";p.name="封存酒桶";}else {p.art="03-containers-10";p.name="维修物料箱";}}
   if(p.kind=="ground"&&p.name=="散落谷物"&&r.type!="grain"&&r.type!="entry"&&r.type!="mill"){p.art=r.type=="ledger"?"04-furniture-45":"06-effects-34";p.name=r.type=="ledger"?"散落账纸":"废木屑";}
  }}
  bool[] reserve=ThemeReserve(f,state.floor==level?new Point(state.playerX,state.playerY):new Point(f.startX,f.startY));
  foreach(var r in f.rooms){int start=f.props.Count;
   LootCorner(f,r,reserve,r.x+r.w-2,r.y+r.h-2);
   reserve=ThemeReserve(f,state.floor==level?new Point(state.playerX,state.playerY):new Point(f.startX,f.startY));
   switch(r.type){
    case "grain":
     ThemeStore(f,r,reserve,r.x,r.y+5,0);ThemeStore(f,r,reserve,r.x+r.w-2,r.y+2,0);ThemeStore(f,r,reserve,r.x+r.w-2,r.y+r.h-2,1);
     VisualPack(f,r,reserve,"长储粮架","04-furniture-16",2,1,r.x+2,r.y+1);ThemeLamp(f,r,r.x+2,r.y+4);ThemeLamp(f,r,r.x+r.w-3,r.y+r.h-3);break;
    case "tools":
     ThemeWorkshop(f,r,reserve,r.x+r.w-3,r.y+r.h-2);ThemeStore(f,r,reserve,r.x,r.y+3,2);VisualPack(f,r,reserve,"武器备件架","04-furniture-21",2,1,r.x+r.w-2,r.y+2);
     ThemeLamp(f,r,r.x+2,r.y);ThemeLamp(f,r,r.x+r.w-2,r.y+r.h-2);r.name="工具维修与军需间";break;
    case "mill":
     ThemeProduction(f,r,reserve,16,3);ThemeStore(f,r,reserve,24,14,0);VisualPack(f,r,reserve,"工作平台栏杆","02-structure-44",2,1,23,15);VisualPack(f,r,reserve,"筛粮工具","05-machinery-37",1,1,14,11);
     Ground(f,r,reserve,"装袋面粉带","03-containers-40",18,10);Ground(f,r,reserve,"石磨粉尘","03-containers-39",21,12);Ground(f,r,reserve,"粮斗洒谷","03-containers-38",24,9);ThemeLamp(f,r,17,7);ThemeLamp(f,r,25,13);break;
    case "wheel":case "terminal":case "drain":
     ThemeWorkshop(f,r,reserve,r.x,r.y+r.h-2);ThemeStore(f,r,reserve,r.x+r.w-3,r.y+4,2);VisualPack(f,r,reserve,"备用齿轮","05-machinery-21",1,1,r.x+2,r.y+3);
     VisualPack(f,r,reserve,"传动支撑架","05-machinery-24",1,1,r.x+r.w-4,r.y+4);ThemeLamp(f,r,r.x+2,r.y+3);ThemeLamp(f,r,r.x+r.w-4,r.y+r.h-3);break;
    case "ledger":
     ThemeOffice(f,r,reserve,r.x+r.w-3,r.y+r.h-2);VisualPack(f,r,reserve,"卷宗书架","04-furniture-14",2,1,r.x,r.y+2);VisualPack(f,r,reserve,"账房小柜","04-furniture-25",1,1,r.x+r.w-2,r.y);ThemeLamp(f,r,r.x+2,r.y);r.name="账房与小型工作室";break;
    case "stairs":
     ThemeOffice(f,r,reserve,r.x,r.y+r.h-3);ThemeStore(f,r,reserve,r.x+r.w-2,r.y+1,1);VisualPack(f,r,reserve,"值守置物架","04-furniture-18",2,1,r.x+r.w-2,r.y+r.h-2);ThemeLamp(f,r,r.x+2,r.y+2);if(level==0)r.name="值守储物与下行楼梯";break;
    case "guard":
     VisualPack(f,r,reserve,"个人储物架","04-furniture-14",2,1,r.x+r.w-2,r.y);VisualPack(f,r,reserve,"饮水桶","03-containers-03",1,1,r.x,r.y+r.h-2);ThemeLamp(f,r,r.x+1,r.y);break;
    case "entry":
     ThemeStore(f,r,reserve,r.x,r.y+r.h-2,1);VisualPack(f,r,reserve,"接货货架","04-furniture-17",2,1,r.x+r.w-2,r.y+1);ThemeLamp(f,r,r.x+2,r.y);break;
    case "cellar":
     ThemeStore(f,r,reserve,r.x,r.y+3,3);ThemeStore(f,r,reserve,r.x+r.w-2,r.y+r.h-2,3);ThemeLamp(f,r,r.x+1,r.y+2);break;
    case "cache":
     ThemeOffice(f,r,reserve,r.x,r.y+1);ThemeStore(f,r,reserve,r.x+r.w-2,r.y+r.h-2,1);ThemeLamp(f,r,r.x+2,r.y);break;
   }
   foreach(var p in f.props.Skip(start))if(String.IsNullOrEmpty(p.group))p.group=r.type+":wall-set";
  }
  for(int i=first;i<f.props.Count;i++)f.props[i].id="f"+level+"visual4-"+i;
  if(!MaintainsAccess(f,new Point(f.startX,f.startY)))throw new Exception("Themed dressing blocked access");f.visualRevision=4;
 }}
 static bool[] ThemeReserve(MillFloor f,Point player){var s=new bool[f.tiles.Length];var start=new Point(f.startX,f.startY);var targets=f.rooms.Select(r=>new Point(r.CenterX,r.CenterY)).ToList();if(f.CanWalk(player.X,player.Y,true))targets.Add(player);
  var reach=Flood(f);foreach(var p in f.props.Where(p=>p.interactive)){Point? best=null;int distance=Int32.MaxValue;foreach(var n in reach){int dx=Math.Max(p.x-n.X,Math.Max(0,n.X-(p.x+p.w-1))),dy=Math.Max(p.y-n.Y,Math.Max(0,n.Y-(p.y+p.h-1)));if(dx+dy==1){int d=Math.Abs(n.X-f.startX)+Math.Abs(n.Y-f.startY);if(d<distance){best=n;distance=d;}}}if(best.HasValue)targets.Add(best.Value);}
  foreach(var t in targets){var path=f.Path(start,t,true);if(path!=null)foreach(var n in path){s[n.Y*f.width+n.X]=true;foreach(var d in new[]{new Point(1,0),new Point(0,1)})if(f.Inside(n.X+d.X,n.Y+d.Y)&&f.CanWalk(n.X+d.X,n.Y+d.Y,true))s[(n.Y+d.Y)*f.width+n.X+d.X]=true;}}
  foreach(var r in f.rooms)for(int y=r.CenterY-1;y<=r.CenterY+1;y++)for(int x=r.CenterX-1;x<=r.CenterX+1;x++)if(f.Inside(x,y))s[y*f.width+x]=true;
  s[start.Y*f.width+start.X]=true;if(f.Inside(player.X,player.Y))s[player.Y*f.width+player.X]=true;return s;
 }
 static void Stamp(MillFloor f,int start,string group){if(!MaintainsAccess(f,new Point(f.startX,f.startY))){f.props.RemoveRange(start,f.props.Count-start);return;}foreach(var p in f.props.Skip(start))p.group=group;}
 static void VisualPack(MillFloor f,MillRoom r,bool[] s,string name,string art,int w,int h,int ox,int oy){int first=f.props.Count;Pack(f,r,s,name,art,w,h,ox,oy);Stamp(f,first,r.type+":wall-cabinet");}
 static void ThemeStore(MillFloor f,MillRoom r,bool[] s,int ox,int oy,int type){var place=Find(f,r,s,ox,oy,2,2);if(!place.HasValue)return;int x=place.Value.X,y=place.Value.Y,first=f.props.Count;
  string a=type==0?"03-containers-35":type==3?"03-containers-04":type==2?"03-containers-27":"03-containers-13";
  Solid(f,r,s,type==0?"堆叠粮袋":type==3?"大酒桶":type==2?"封装器械":"箱堆",a,x,y);
  Solid(f,r,s,type==0?"面粉袋":type==2?"工具桶":"小桶",type==0?"03-containers-33":type==2?"03-containers-03":"03-containers-05",x+1,y);
  Place(f,r,s,"push","大木桶","03-containers-04","",x,y+1,1,1,null);Container(f,r,s,"break","成组木箱","03-containers-12","03-containers-16",x+1,y+1,"木料");
  Ground(f,r,s,type==0?"洒落面粉":"木屑",type==0?"03-containers-39":"06-effects-33",x,y+1);Stamp(f,first,r.type+":storage-corner");
 }
 static void LootCorner(MillFloor f,MillRoom r,bool[] s,int ox,int oy){var place=Find(f,r,s,ox,oy,2,2);int first=f.props.Count;if(place.HasValue){int x=place.Value.X,y=place.Value.Y;Container(f,r,s,"chest","角落补给箱","03-containers-30","03-containers-31",x,y,"干粮");Container(f,r,s,"break","角落陶罐","03-containers-20","03-containers-22",x+1,y,"陶片");Solid(f,r,s,"小桶","03-containers-05",x,y+1);Container(f,r,s,"break","小物资箱","03-containers-14","03-containers-16",x+1,y+1,"木料");}else {place=Find(f,r,s,ox,oy,1,2);if(place.HasValue){Container(f,r,s,"chest","壁边补给箱","03-containers-30","03-containers-31",place.Value.X,place.Value.Y,"干粮");Container(f,r,s,"break","陶罐","03-containers-20","03-containers-22",place.Value.X,place.Value.Y+1,"陶片");}}
  Stamp(f,first,r.type+":loot-corner");
 }
 static void ThemeWorkshop(MillFloor f,MillRoom r,bool[] s,int ox,int oy){var place=Find(f,r,s,ox,oy,3,2);if(!place.HasValue){VisualPack(f,r,s,"维修工具架","04-furniture-20",2,1,ox,oy);return;}int x=place.Value.X,y=place.Value.Y,first=f.props.Count;
  Place(f,r,s,"search","维修工具台","04-furniture-36","04-furniture-36",x,y,2,1,"修理零件");Solid(f,r,s,"靠墙工具","04-furniture-19",x+2,y);Solid(f,r,s,"木凳","04-furniture-42",x,y+1);Container(f,r,s,"cabinet","武器备件箱","03-containers-27","03-containers-29",x+1,y+1,"修理零件");Solid(f,r,s,"堆叠木料","06-effects-15",x+2,y+1);Ground(f,r,s,"锯屑","06-effects-34",x+2,y+1);Stamp(f,first,r.type+":repair-bench");
 }
 static void ThemeOffice(MillFloor f,MillRoom r,bool[] s,int ox,int oy){int first=f.props.Count;Desk(f,r,s,ox,oy,"04-furniture-34","带账册长桌");VisualPack(f,r,s,"账本书架","04-furniture-13",2,1,ox+2,oy);Stamp(f,first,r.type+":desk-and-shelf");}
 static void ThemeProduction(MillFloor f,MillRoom r,bool[] s,int ox,int oy){var p=Find(f,r,s,ox,oy,4,2);if(!p.HasValue)return;int x=p.Value.X,y=p.Value.Y,first=f.props.Count;
  Solid(f,r,s,"上料长台","04-furniture-41",x,y,2,1);Solid(f,r,s,"谷物筐","05-machinery-41",x+2,y);Solid(f,r,s,"筛子","05-machinery-38",x+3,y);Solid(f,r,s,"面粉袋堆","03-containers-35",x,y+1,2,1);Solid(f,r,s,"木铲","05-machinery-39",x+2,y+1);Ground(f,r,s,"连续面粉痕迹","03-containers-40",x+3,y+1);Stamp(f,first,"mill:processing-line");
 }
 static void ThemeLamp(MillFloor f,MillRoom r,int ox,int oy){var reachable=Flood(f);var point=Cells(r).Where(p=>reachable.Contains(p)&&f.Tile(p.X,p.Y-1)==0&&!f.props.Any(q=>q.kind=="lamp"&&Math.Abs(q.x-p.X)+Math.Abs(q.y-p.Y)<3)).OrderBy(p=>Math.Abs(p.X-ox)+Math.Abs(p.Y-oy)).Select(p=>(Point?)p).FirstOrDefault();if(!point.HasValue)return;var n=point.Value;var lamp=Prop(f,"lamp","工作壁灯","06-effects-09","06-effects-10",n.X,n.Y,1,1,false,true,null);lamp.group=r.type+":light";}
}

