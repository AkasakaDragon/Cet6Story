using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;
public sealed partial class TavernHallCanvas {
 public bool Panorama; public float CameraX,CameraY; bool cameraReady; Timer cameraTimer;
 float CameraScale{get{return Math.Max(Math.Max(1,Width-RightPaneWidth)/760f,Math.Max(1,Height)/650f);}}
 public PointF HallToWorld(PointF p){return new PointF(.68247f*p.X-.05655f*p.Y+175,.58342f*p.Y-.01092f*p.X+146);}
 public PointF WorldToHall(PointF p){float x=p.X-175,y=p.Y-146;float d=.68247f*.58342f-.05655f*.01092f;return new PointF((.58342f*x+.05655f*y)/d,(.01092f*x+.68247f*y)/d);}
 void EnsureCamera(){if(!cameraReady){CameraX=620-Math.Max(1,Width-RightPaneWidth)/CameraScale/2;CameraY=445-Height/CameraScale/2;cameraReady=true;}MoveCamera(0,0);}
 public void MoveCamera(float dx,float dy){if(Art==null)return;CameraX=Math.Max(0,Math.Min(Math.Max(0,Art.Width-Math.Max(1,Width-RightPaneWidth)/CameraScale),CameraX+dx));CameraY=Math.Max(0,Math.Min(Math.Max(0,Art.Height-Height/CameraScale),CameraY+dy));Invalidate();}
 public PointF ScreenToHall(Point p){if(Panorama){EnsureCamera();return WorldToHall(new PointF(CameraX+p.X/CameraScale,CameraY+p.Y/CameraScale));}var v=View;return new PointF((p.X-v.X)*1400/v.Width,(p.Y-v.Y)*1132/v.Height);}
 void PaintSceneBackground(Graphics g){if(!Panorama){var v=View;g.TranslateTransform(v.X,v.Y);g.ScaleTransform(v.Width/1400,v.Height/1132);g.DrawImage(Art,0,0,1400,1132);return;}EnsureCamera();g.ScaleTransform(CameraScale,CameraScale);g.TranslateTransform(-CameraX,-CameraY);g.DrawImage(Art,0,0,Art.Width,Art.Height);using(var map=new Matrix(.68247f,-.01092f,-.05655f,.58342f,175,146))g.MultiplyTransform(map);}
 void PaintFurnitureImage(Graphics g,Image layer=null){var image=layer??Art;if(!Panorama){g.DrawImage(image,0,0,1400,1132);return;}using(var map=new Matrix(.68247f,-.01092f,-.05655f,.58342f,175,146)){map.Invert();g.MultiplyTransform(map);g.DrawImage(image,0,0,image.Width,image.Height);}}
 public void StartCamera(){if(cameraTimer!=null)return;cameraTimer=new Timer{Interval=25};cameraTimer.Tick+=(s,e)=>{if(!Panorama||!Visible||FindForm()==null||Form.ActiveForm!=FindForm())return;var p=PointToClient(Cursor.Position);int w=Width-RightPaneWidth;if(p.Y<0||p.Y>Height||p.X<0||p.X>Width)return;float dx=p.X<24?-1:p.X>w-24&&p.X<w?1:p.X>Width-24?1:0;float dy=p.Y<24?-1:p.Y>Height-24?1:0;if(dx!=0||dy!=0)MoveCamera(dx*8/CameraScale,dy*8/CameraScale);};cameraTimer.Start();}
}
