using System;using System.Drawing;using System.IO;using System.Collections.Generic;
public sealed class HallActionSheet:IDisposable {
 public readonly Image Art;public readonly int Rows;public HallActionSheet(string file,int rows){Art=Image.FromFile(file);Rows=rows;}
 public void Draw(Graphics g,PointF feet,float height,int row,int frame,bool mirror){float cw=Art.Width/4f,ch=Art.Height/(float)Rows;var saved=g.Save();g.TranslateTransform(feet.X,feet.Y);if(mirror)g.ScaleTransform(-1,1);float width=height*cw/ch;g.DrawImage(Art,new RectangleF(-width/2,-height,width,height),new RectangleF((frame%4)*cw,row*ch,cw,ch),GraphicsUnit.Pixel);g.Restore(saved);}
 public void Dispose(){Art.Dispose();}
}
public static class HallActionRows {
 public static int Server(string action,double progress){return action=="收桌"?(progress<6?2:3):action=="备饮"?4:action.Contains("送餐")?1:action=="接待点单"||action=="结账"?0:5;}
 public static int Chef(string action){return action=="拿取食材"?0:action.Contains("切配")?1:action=="烹饪"?2:action=="装盘"?3:action=="补柴"?5:4;}
 public static int Guest(HallGuest guest){return guest.stage==4?(guest.recipe==3?2:1):guest.stage==5?3:0;}
}
public sealed partial class TavernHallCanvas {
 Dictionary<string,HallActionSheet> actions=new Dictionary<string,HallActionSheet>();public bool Guiding;
 void LoadActions(string folder){foreach(var spec in new[]{"aelia:6","lyse:6","guest:4","luchuan:2","rest:2","delivery:4"}){var parts=spec.Split(':');string file=Path.Combine(folder,"actions",parts[0]=="delivery"?"delivery-walk.png":parts[0]+"-actions.png");if(File.Exists(file))actions[parts[0]]=new HallActionSheet(file,Int32.Parse(parts[1]));}}
 bool DrawAction(Graphics g,string key,PointF feet,float height,int row,int frame,bool mirror){HallActionSheet sheet;if(!actions.TryGetValue(key,out sheet))return false;sheet.Draw(g,feet,height,row,frame,mirror);return true;}
 void GuestAction(Graphics g,HallGuest guest,PointF feet){int row=HallActionRows.Guest(guest);int frame=(int)(Animation*(row==0?1.5:3))%4;if(!DrawAction(g,"guest",feet,94,row,frame,false))Sprite(g,Guest,feet,94,"待命");}
 void OwnerAction(Graphics g){if(!DrawAction(g,"luchuan",HallFurniture.OwnerFeet,130,Guiding?1:0,(int)(Animation*(Guiding?2:1))%4,false))Sprite(g,Owner,HallFurniture.OwnerFeet,130,"待命");}
 bool EmployeeAction(Graphics g,string key,PointF feet,float height,string action){if(action=="待命")return DrawAction(g,"rest",feet,height,key=="aelia"?0:1,(int)Animation%2,false);if(action=="自动休息")return DrawAction(g,"rest",feet,height,key=="aelia"?0:1,(int)(Animation*1.5)%4,false);int row=key=="aelia"?HallActionRows.Server(action,State.server.progress):HallActionRows.Chef(action);int frame=key=="aelia"?new[]{0,1,1,0}[(int)(Animation*3)%4]:(int)(Animation*3)%4;return DrawAction(g,key,feet,height,row,frame,key=="aelia"&&State.server.job>=0&&action!="备饮");}
 bool DeliveryWalk(Graphics g,HallWalkPose pose,float height){bool back=(int)pose.direction<2,mirror=pose.direction==HallWalkDirection.UpperRight||pose.direction==HallWalkDirection.LowerRight;int row=(back?0:2)+(State.server.batch.Count>1?1:0);return DrawAction(g,"delivery",pose.position,height,row,pose.Frame,mirror);}
}
