using System;using System.Drawing;using System.Drawing.Drawing2D;
public static class HallFurniture {
 // Hip anchors taken from the green cushions in hall-empty.png.
 public static readonly PointF[] Cushions={new PointF(529,588),new PointF(466,721),new PointF(766,689),new PointF(736,823)};
 public static PointF SeatedFeet(int table){var p=Cushions[table];return new PointF(p.X-8,p.Y+27);}
 public static HallDiningSeat Seat(int table){return new HallDiningSeat(Cushions[table],new PointF(Cushions[table].X+55,Cushions[table].Y-40));}
 public static readonly PointF OwnerFeet=new PointF(935,581);
 public static PointF[] BarFront{get{return new[]{new PointF(651,453),new PointF(1110,587),new PointF(1110,655),new PointF(651,521)};}}
}
public sealed class HallDiningSeat {
 public readonly PointF Hip,Table;
 public HallDiningSeat(PointF hip,PointF table){Hip=hip;Table=table;}
 public bool Mirror{get{return Table.X<Hip.X;}}
 public HallWalkDirection Facing{get{return HallWalking.Direction(Table.X-Hip.X,Table.Y-Hip.Y);}}
}
public sealed partial class TavernHallCanvas {
 HallDiningSeat DiningSeat(int seat){var hip=HallSeats.Hip(seat,Panorama);var table=HallSeats.Table(seat,Panorama);return new HallDiningSeat(Panorama?WorldToHall(hip):hip,Panorama?WorldToHall(table):table);}
 PointF SeatFeet(int seat){var p=DiningSeat(seat).Hip;return new PointF(p.X,p.Y+27);}

 Bitmap chairWoodArt;
 Bitmap ChairWoodArt(){if(chairWoodArt!=null)return chairWoodArt;chairWoodArt=new Bitmap(Art.Width,Art.Height,System.Drawing.Imaging.PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(chairWoodArt))g.DrawImageUnscaled(Art,0,0);var locked=chairWoodArt.LockBits(new Rectangle(0,0,chairWoodArt.Width,chairWoodArt.Height),System.Drawing.Imaging.ImageLockMode.ReadWrite,System.Drawing.Imaging.PixelFormat.Format32bppArgb);var pixels=new byte[locked.Stride*locked.Height];System.Runtime.InteropServices.Marshal.Copy(locked.Scan0,pixels,0,pixels.Length);for(int i=0;i<pixels.Length;i+=4)if(pixels[i+2]<pixels[i+1]*1.16f||pixels[i+2]<pixels[i]*1.2f)pixels[i+3]=0;System.Runtime.InteropServices.Marshal.Copy(pixels,0,locked.Scan0,pixels.Length);chairWoodArt.UnlockBits(locked);return chairWoodArt;}
 void FurniturePatch(Graphics g,PointF[] outline,bool woodOnly=false){var saved=g.Save();using(var path=new GraphicsPath()){path.AddPolygon(outline);g.SetClip(path,CombineMode.Intersect);PaintFurnitureImage(g,woodOnly?ChairWoodArt():null);}g.Restore(saved);}
 // Only the three wooden back uprights and top rail belong in front of the guest.
 // The green cushion and seat rim stay in the background, underneath the pelvis.
 void ChairFront(Graphics g,int table){
  if(table>=16||HallSeats.Front(table))return;
  if(table>=12){var seatHip=HallSeats.Hip(table,Panorama);var origin=new PointF(seatHip.X+(Panorama?2:-2),seatHip.Y-(Panorama?22:29));
   var parts=Panorama?new[]{new[]{new PointF(0,0),new PointF(7,0),new PointF(7,41),new PointF(0,41)},new[]{new PointF(23,-9),new PointF(29,-9),new PointF(29,31),new PointF(23,31)},new[]{new PointF(0,0),new PointF(29,-11),new PointF(29,-5),new PointF(0,6)},new[]{new PointF(15,5),new PointF(19,3),new PointF(19,19),new PointF(15,21)}}:new[]{new[]{new PointF(0,0),new PointF(11,0),new PointF(11,65),new PointF(0,65)},new[]{new PointF(26,-13),new PointF(36,-13),new PointF(36,51),new PointF(26,51)},new[]{new PointF(0,0),new PointF(36,-14),new PointF(36,-4),new PointF(0,11)},new[]{new PointF(17,13),new PointF(24,10),new PointF(24,29),new PointF(17,32)}};
   foreach(var part in parts){if(Panorama)PanoramaChairWood(g,origin,part);else ChairWood(g,origin,part);}return;}
  if(Panorama){var worldOrigins=new[]{new PointF(473,478),new PointF(433,558),new PointF(617,535),new PointF(596,615)};var worldOrigin=worldOrigins[table];
   PanoramaChairWood(g,worldOrigin,new[]{new PointF(0,0),new PointF(7,2),new PointF(8,36),new PointF(2,40)});
   PanoramaChairWood(g,worldOrigin,new[]{new PointF(16,9),new PointF(23,11),new PointF(25,45),new PointF(18,47)});
   PanoramaChairWood(g,worldOrigin,new[]{new PointF(0,0),new PointF(23,10),new PointF(23,16),new PointF(0,6)});
   PanoramaChairWood(g,worldOrigin,new[]{new PointF(10,12),new PointF(14,14),new PointF(15,26),new PointF(11,24)});return;}
  var origins=new[]{new PointF(490,550),new PointF(426,683),new PointF(722,649),new PointF(686,784)};
  var p=origins[table];
  ChairWood(g,p,new[]{new PointF(0,0),new PointF(11,3),new PointF(13,61),new PointF(2,65)});
  ChairWood(g,p,new[]{new PointF(28,13),new PointF(39,17),new PointF(40,75),new PointF(29,78)});
  ChairWood(g,p,new[]{new PointF(17,18),new PointF(24,21),new PointF(24,39),new PointF(17,36)});
  ChairWood(g,p,new[]{new PointF(0,0),new PointF(39,16),new PointF(39,25),new PointF(0,9)});
 }
 void ChairWood(Graphics g,PointF origin,PointF[] points){for(int i=0;i<points.Length;i++)points[i]=new PointF(points[i].X+origin.X,points[i].Y+origin.Y);FurniturePatch(g,points,true);}
 void PanoramaChairWood(Graphics g,PointF origin,PointF[] points){for(int i=0;i<points.Length;i++)points[i]=WorldToHall(new PointF(points[i].X+origin.X,points[i].Y+origin.Y));FurniturePatch(g,points,true);}
 void TableFront(Graphics g,int seat){
  if(seat>=16){var p=HallSeats.Hip(seat,Panorama);float rx=Panorama?30:45;var points=new[]{new PointF(p.X-rx,p.Y-35),new PointF(p.X+rx,p.Y-20),new PointF(p.X+rx,p.Y-6),new PointF(p.X-rx,p.Y-21)};if(Panorama)for(int i=0;i<points.Length;i++)points[i]=WorldToHall(points[i]);FurniturePatch(g,points);return;}
  var center=HallSeats.Table(seat,Panorama);float x=Panorama?43:62,y=Panorama?18:26,lip=Panorama?8:12;
  var outline=new[]{new PointF(center.X-x,center.Y),new PointF(center.X-x*.7f,center.Y-y*.8f),new PointF(center.X,center.Y-y),new PointF(center.X+x*.7f,center.Y-y*.8f),new PointF(center.X+x,center.Y),new PointF(center.X+x*.7f,center.Y+y+lip*.5f),new PointF(center.X,center.Y+y+lip),new PointF(center.X-x*.7f,center.Y+y+lip*.5f)};
  if(Panorama)for(int i=0;i<outline.Length;i++)outline[i]=WorldToHall(outline[i]);FurniturePatch(g,outline);
 }

 void BarFront(Graphics g){FurniturePatch(g,Panorama?new[]{WorldToHall(new PointF(563,430)),WorldToHall(new PointF(810,493)),WorldToHall(new PointF(810,530)),WorldToHall(new PointF(563,467))}:HallFurniture.BarFront);}
}
