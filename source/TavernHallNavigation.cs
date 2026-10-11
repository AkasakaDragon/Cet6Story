using System;using System.Drawing;using System.Collections.Generic;using System.Linq;
// Feet travel on the floor; table footprints include their chairs and a walking margin.
public static class HallNavigation {
 public static readonly PointF Entry=new PointF(487,462),Counter=new PointF(720,550);
 public static PointF CounterFor(int layout){return layout==0?Counter:new PointF(925,635);}
 public static readonly PointF[] Seats={HallFurniture.SeatedFeet(0),HallFurniture.SeatedFeet(1),HallFurniture.SeatedFeet(2),HallFurniture.SeatedFeet(3)};
 public static readonly PointF[] Approaches={new PointF(465,590),new PointF(413,710),new PointF(700,690),new PointF(670,835)};
 static readonly PointF[] floor={new PointF(463,440),new PointF(1050,630),new PointF(960,803),new PointF(817,918),new PointF(655,918),new PointF(240,735),new PointF(420,550)};
 static readonly PointF[] tables={new PointF(578,580),new PointF(529,714),new PointF(816,694),new PointF(784,838)};
 static readonly Dictionary<string,List<PointF>> cache=new Dictionary<string,List<PointF>>();
 public static bool Clear(PointF p){bool inside=false;for(int i=0,j=floor.Length-1;i<floor.Length;j=i++){var a=floor[i];var b=floor[j];if((a.Y>p.Y)!=(b.Y>p.Y)&&p.X<(b.X-a.X)*(p.Y-a.Y)/(b.Y-a.Y)+a.X)inside=!inside;}if(!inside)return false;foreach(var t in tables)if(Math.Pow((p.X-t.X)/107,2)+Math.Pow((p.Y-t.Y)/62,2)<1)return false;return true;}
 static PointF World(int u,int v){return new PointF(487+(u-v)*18,462+(u+v)*9);}
 static int Key(int u,int v){return (u+70)*141+v+70;}static PointF World(int key){return World(key/141-70,key%141-70);}
 static double Distance(PointF a,PointF b){return Math.Sqrt(Math.Pow(a.X-b.X,2)+Math.Pow(a.Y-b.Y,2));}
 static int Nearest(PointF p){int best=-1;double d=Double.MaxValue;for(int u=-65;u<=65;u++)for(int v=-65;v<=65;v++){var w=World(u,v);double n=Distance(p,w);if(n<d&&Clear(w)){d=n;best=Key(u,v);}}return best;}
 public static List<PointF> Path(PointF from,PointF to){string name=from.X+","+from.Y+":"+to.X+","+to.Y;List<PointF> result;if(cache.TryGetValue(name,out result))return result;int start=Nearest(from),end=Nearest(to);var open=new List<int>{start};var closed=new HashSet<int>();var costs=new Dictionary<int,double>{{start,0}};var previous=new Dictionary<int,int>();while(open.Count>0){int node=open.OrderBy(n=>costs[n]+Distance(World(n),World(end))).First();open.Remove(node);if(node==end)break;closed.Add(node);int u=node/141-70,v=node%141-70;foreach(var next in new[]{Key(u+1,v),Key(u-1,v),Key(u,v+1),Key(u,v-1)}){if(next<0||next>=141*141||closed.Contains(next)||!Clear(World(next))||!Clear(new PointF((World(next).X+World(node).X)/2,(World(next).Y+World(node).Y)/2)))continue;double cost=costs[node]+Distance(World(node),World(next));if(!costs.ContainsKey(next)||cost<costs[next]){costs[next]=cost;previous[next]=node;if(!open.Contains(next))open.Add(next);}}}result=new List<PointF>();if(start==end||previous.ContainsKey(end)){for(int n=end;;n=previous[n]){result.Insert(0,World(n));if(n==start)break;}}else result.Add(World(start));cache[name]=result;return result;}
 public static PointF At(List<PointF> path,double t){if(path.Count==0)return Entry;if(path.Count==1)return path[0];double total=0;for(int i=1;i<path.Count;i++)total+=Distance(path[i-1],path[i]);double left=Math.Max(0,Math.Min(1,t))*total;for(int i=1;i<path.Count;i++){double d=Distance(path[i-1],path[i]);if(left<=d)return new PointF(path[i-1].X+(path[i].X-path[i-1].X)*(float)(left/d),path[i-1].Y+(path[i].Y-path[i-1].Y)*(float)(left/d));left-=d;}return path[path.Count-1];}
 public static PointF Route(PointF from,PointF to,double t){return At(Path(from,to),t);}
}
