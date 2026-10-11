using System;using System.Drawing;using System.Drawing.Drawing2D;
public static class HallFurniture {
 // Hip anchors taken from the green cushions in hall-empty.png.
 public static readonly PointF[] Cushions={new PointF(520,600),new PointF(467,728),new PointF(764,696),new PointF(732,830)};
 public static PointF SeatedFeet(int table){var p=Cushions[table];return new PointF(p.X-8,p.Y+27);}
 public static readonly PointF OwnerFeet=new PointF(935,581);
 public static PointF[] BarFront{get{return new[]{new PointF(651,453),new PointF(1110,587),new PointF(1110,655),new PointF(651,521)};}}
}
public sealed partial class TavernHallCanvas {
 void FurniturePatch(Graphics g,PointF[] outline){var saved=g.Save();using(var path=new GraphicsPath()){path.AddPolygon(outline);g.SetClip(path,CombineMode.Intersect);g.DrawImage(Art,0,0,1400,1132);}g.Restore(saved);}
 void ChairFront(Graphics g,int table){var p=HallFurniture.Cushions[table];FurniturePatch(g,new[]{new PointF(p.X-23,p.Y+8),new PointF(p.X+23,p.Y-16),new PointF(p.X+29,p.Y-10),new PointF(p.X-16,p.Y+16)});FurniturePatch(g,new[]{new PointF(p.X+17,p.Y-9),new PointF(p.X+27,p.Y-14),new PointF(p.X+27,p.Y+31),new PointF(p.X+17,p.Y+37)});}
 void BarFront(Graphics g){FurniturePatch(g,HallFurniture.BarFront);}
}
