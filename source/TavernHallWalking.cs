using System;using System.Drawing;using System.Drawing.Drawing2D;using System.IO;using System.Collections.Generic;
public enum HallWalkDirection {UpperLeft=0,UpperRight=1,LowerLeft=2,LowerRight=3}
public static class HallWalking {
 public static HallWalkDirection Direction(float dx,float dy){return dy<0?(dx<0?HallWalkDirection.UpperLeft:HallWalkDirection.UpperRight):(dx<0?HallWalkDirection.LowerLeft:HallWalkDirection.LowerRight);}
 public static PointF Corner(PointF a,PointF b){float x=(b.X-a.X+2*(b.Y-a.Y))/2;return new PointF(a.X+x,a.Y+x/2);}
 static double Distance(PointF a,PointF b){return Math.Sqrt((b.X-a.X)*(b.X-a.X)+(b.Y-a.Y)*(b.Y-a.Y));}
 public static PointF Route(PointF a,PointF b,double t){var mid=Corner(a,b);double first=Distance(a,mid),second=Distance(mid,b),distance=(first+second)*Math.Max(0,Math.Min(1,t));if(distance<=first&&first>0)return Lerp(a,mid,distance/first);return second>0?Lerp(mid,b,(distance-first)/second):b;}
 public static PointF Via(PointF a,PointF via,PointF b,double t){double first=Distance(a,Corner(a,via))+Distance(Corner(a,via),via),second=Distance(via,Corner(via,b))+Distance(Corner(via,b),b);double d=(first+second)*Math.Max(0,Math.Min(1,t));return d<first&&first>0?Route(a,via,d/first):Route(via,b,second>0?(d-first)/second:1);}
 static PointF Lerp(PointF a,PointF b,double t){return new PointF(a.X+(b.X-a.X)*(float)t,a.Y+(b.Y-a.Y)*(float)t);}
}
public sealed class HallWalkSheet {
 public readonly Image Art;public HallWalkSheet(Image art){Art=art;}
 public void Draw(Graphics g,PointF feet,float height,HallWalkDirection direction,int frame){int cw=Art.Width/4,ch=Art.Height/4;bool back=(int)direction<2,right=direction==HallWalkDirection.UpperRight||direction==HallWalkDirection.LowerRight;int row=back?0:2;var source=new Rectangle(new[]{0,1,2,1}[frame%4]*cw,row*ch,cw,ch);float width=height*cw/ch;var saved=g.Save();g.TranslateTransform(feet.X,feet.Y);if(right)g.ScaleTransform(-1,1);g.DrawImage(Art,new RectangleF(-width/2,-height,width,height),source,GraphicsUnit.Pixel);g.Restore(saved);}
}
public sealed class HallWalkPose {
 public PointF position;public HallWalkDirection direction=HallWalkDirection.LowerLeft;public bool moving;public double time=-1,phase;public PointF goal;PointF start,corner;double length,travel;public bool Floor;public double Speed=150;List<PointF> path;
 public void Update(PointF desired,double now,bool smooth){if(time<0){position=desired;goal=desired;time=now;return;}double dt=Math.Max(0,now-time);if(dt==0)return;time=now;var old=position;if(!smooth)position=desired;else {if(Math.Abs(goal.X-desired.X)>3||Math.Abs(goal.Y-desired.Y)>3){goal=desired;start=position;corner=HallWalking.Corner(start,goal);length=Math.Sqrt(Math.Pow(corner.X-start.X,2)+Math.Pow(corner.Y-start.Y,2))+Math.Sqrt(Math.Pow(goal.X-corner.X,2)+Math.Pow(goal.Y-corner.Y,2));if(Floor){path=HallNavigation.Path(start,goal);length=0;for(int i=1;i<path.Count;i++)length+=Math.Sqrt(Math.Pow(path[i].X-path[i-1].X,2)+Math.Pow(path[i].Y-path[i-1].Y,2));}travel=0;}travel+=dt*Speed;position=Floor?(path!=null?HallNavigation.At(path,length>0?travel/length:1):position):HallWalking.Route(start,goal,length>0?travel/length:1);}float dx=position.X-old.X,dy=position.Y-old.Y;moving=Math.Abs(dx)+Math.Abs(dy)>.05;if(moving){direction=HallWalking.Direction(dx,dy);phase+=Math.Sqrt(dx*dx+dy*dy)/18;}}
 public int Frame{get{return (int)phase%4;}}
}
public sealed partial class TavernHallCanvas {
 Dictionary<string,HallWalkSheet> sheets=new Dictionary<string,HallWalkSheet>();Dictionary<string,HallWalkPose> poses=new Dictionary<string,HallWalkPose>();
 public void LoadWalking(string folder){LoadActions(folder);foreach(var key in new[]{"aelia","lyse","luchuan","townsperson"}){string file=Path.Combine(folder,"walking",key+"-walk.png");if(File.Exists(file))sheets[key]=new HallWalkSheet(Image.FromFile(file));}}
 HallWalkPose Pose(string id,PointF desired,bool smooth){HallWalkPose pose;if(!poses.TryGetValue(id,out pose)){pose=new HallWalkPose{Floor=id=="server"};poses[id]=pose;}if(id=="server")pose.Speed=State.server.resting?0:150*(1+State.server.Efficiency*.018+State.upgrade*.18)*(State.server.fatigue>State.server.Stamina*.75?.8:1);pose.Update(desired,Animation,smooth);return pose;}
 void Walker(Graphics g,string key,Image fallback,PointF feet,float height,string action,string id,bool smooth){var pose=Pose(id,feet,smooth);HallWalkSheet sheet;if(key=="aelia"&&pose.moving&&action.Contains("送餐")&&DeliveryWalk(g,pose,height))return;if(sheets.TryGetValue(key,out sheet)&&pose.moving){sheet.Draw(g,pose.position,height,pose.direction,pose.moving?pose.Frame:1);}else if(key=="aelia"||key=="lyse"){if(!EmployeeAction(g,key,pose.position,height,action))Sprite(g,fallback,pose.position,height,action);}else Sprite(g,fallback,pose.position,height,action);}
 protected override void Dispose(bool disposing){if(disposing){foreach(var s in sheets.Values)s.Art.Dispose();sheets.Clear();foreach(var a in actions.Values)a.Dispose();actions.Clear();}base.Dispose(disposing);}
}

