using System;using System.Drawing;using System.Drawing.Drawing2D;
public partial class PartyBattleCanvas {
 bool DrawMaleRig(Graphics g,float center,int floor,float height,out Rectangle bounds){
  var frames=MaleIdle;var art=frames!=null&&frames.Length>0?frames[HeroIdleAnimation.FrameIndex%frames.Length]:Male;
  bounds=Rectangle.Empty;if(art==null)return false;
  bounds=frames!=null&&frames.Length>0?Sprite(art,center,floor+height*50/395f,height*468/395f):Sprite(art,center,floor,height);
  float t=MaleRigProgress();
  if(t<=0){g.DrawImage(art,bounds);return true;}
  // Animate cutout limbs from the SAME idle bitmap. Head, torso, palette,
  // coat, legs, alpha and the original idle transform never change.
  PointF[] right={new PointF(.65f,.32f),new PointF(.73f,.33f),new PointF(.80f,.52f),new PointF(.78f,.59f),new PointF(.70f,.59f),new PointF(.64f,.42f)};
  PointF[] left={new PointF(.32f,.27f),new PointF(.44f,.30f),new PointF(.46f,.42f),new PointF(.38f,.46f),new PointF(.23f,.42f),new PointF(.23f,.35f)};
  using(var arm=RigPath(right,bounds))using(var other=RigPath(left,bounds)){
   var state=g.Save();using(var body=new Region(bounds)){body.Exclude(arm);if(castRow==1)body.Exclude(other);g.SetClip(body,CombineMode.Intersect);g.DrawImage(art,bounds);}g.Restore(state);
   DrawRigArm(g,art,bounds,arm,new PointF(bounds.X+bounds.Width*.665f,bounds.Y+bounds.Height*.335f),-62*t);
   if(castRow==1)DrawRigArm(g,art,bounds,other,new PointF(bounds.X+bounds.Width*.385f,bounds.Y+bounds.Height*.31f),-24*t);
  }return true;
 }
 float MaleRigProgress(){float age=CastVisualAge,t=age<920?Math.Max(0,Math.Min(1,(age-220)/700)):age<1130?1:Math.Max(0,1-(age-1130)/520);return t*t*(3-2*t);}
 PointF MaleRigPalm(float center,int floor,float height){var frames=MaleIdle;var art=frames!=null&&frames.Length>0?frames[HeroIdleAnimation.FrameIndex%frames.Length]:Male;if(art==null)return new PointF(center+height*.3f,floor-height*.52f);var r=frames!=null&&frames.Length>0?Sprite(art,center,floor+height*50/395f,height*468/395f):Sprite(art,center,floor,height);double a=-62*MaleRigProgress()*Math.PI/180;float dx=r.Width*.095f,dy=r.Height*.215f;return new PointF(r.X+r.Width*.665f+(float)(dx*Math.Cos(a)-dy*Math.Sin(a))+4,r.Y+r.Height*.335f+(float)(dx*Math.Sin(a)+dy*Math.Cos(a)));}
 GraphicsPath RigPath(PointF[] points,Rectangle bounds){var p=new PointF[points.Length];for(int i=0;i<p.Length;i++)p[i]=new PointF(bounds.X+points[i].X*bounds.Width,bounds.Y+points[i].Y*bounds.Height);var path=new GraphicsPath();path.AddPolygon(p);return path;}
 void DrawRigArm(Graphics g,Image art,Rectangle bounds,GraphicsPath limb,PointF pivot,float angle){var state=g.Save();g.TranslateTransform(pivot.X,pivot.Y);g.RotateTransform(angle);g.TranslateTransform(-pivot.X,-pivot.Y);g.SetClip(limb,CombineMode.Intersect);g.DrawImage(art,bounds);g.Restore(state);}
}

