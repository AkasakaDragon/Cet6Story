using System;using System.Drawing;using System.Drawing.Drawing2D;using System.IO;using System.Linq;
class TavernDiningChecks {
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);}
 [STAThread]static void Main(){try{Run();}catch(Exception e){Console.WriteLine(e);Environment.Exit(1);}}
 static void Run(){
  string root=AppDomain.CurrentDomain.BaseDirectory,folder=Path.Combine(root,"assets/tavern/management");
  var occupied=new System.Collections.Generic.List<HallGuest>();
  var walk=new HallWalkSheet(Path.Combine(folder,"walking/townsperson-walk.png"));
  try{using(var preview=new Bitmap(640,480))using(var g=Graphics.FromImage(preview)){
   g.Clear(Color.FromArgb(38,55,48));g.InterpolationMode=InterpolationMode.NearestNeighbor;
   for(int direction=0;direction<4;direction++)for(int frame=0;frame<4;frame++)using(var sprite=new Bitmap(160,140)){
    using(var sg=Graphics.FromImage(sprite))walk.Draw(sg,new PointF(80,130),104,(HallWalkDirection)direction,frame);
    int top=140,bottom=0;for(int y=0;y<140;y++)for(int x=0;x<160;x++)if(sprite.GetPixel(x,y).A>200){top=Math.Min(top,y);bottom=Math.Max(bottom,y);}
    Check(bottom-top>=99&&bottom-top<=104,"walking body height excludes transparent padding");Check(bottom>=126&&bottom<=130,"walking boots remain on the route anchor");
    g.DrawImageUnscaled(sprite,frame*160,direction*120-10);
   }
   preview.Save(Path.Combine(root,"guest-walking-size.png"));
  }}finally{walk.Art.Dispose();}
  for(int id=0;id<HallSeats.Count;id++){
   int seat=HallSeats.NextFree(occupied,id);Check(seat>=0&&!occupied.Any(c=>c.table==seat),"seat allocation is unique");occupied.Add(new HallGuest{id=id,table=seat});
   var path=HallNavigation.Path(HallNavigation.Entry,HallNavigation.Approaches[seat]);Check(path.Count>=2,"every chair and bar stool is reachable");
   for(int segment=1;segment<path.Count;segment++){
    var a=path[segment-1];var b=path[segment];Check(Math.Abs(Math.Abs(b.X-a.X)-2*Math.Abs(b.Y-a.Y))<.01,"route follows one isometric direction per segment");
    for(int step=0;step<=20;step++){var point=new PointF(a.X+(b.X-a.X)*step/20,a.Y+(b.Y-a.Y)*step/20);Check(HallNavigation.Clear(point),"route stays outside table footprints");}
    if(segment>=2){var before=path[segment-2];Check(Math.Abs((a.X-before.X)*(b.Y-a.Y)-(a.Y-before.Y)*(b.X-a.X))>.01,"straight walking runs are merged");}
   }
   Console.WriteLine("Seat "+seat+": "+(path.Count-1)+" straight runs");
  }
  Check(HallSeats.NextFree(occupied,0)==-1,"full hall does not assign an occupied seat");
  using(var bitmap=new Bitmap(10,10))using(var g=Graphics.FromImage(bitmap)){
   using(var ground=new Matrix(.68247f,-.01092f,-.05655f,.58342f,175,146))g.Transform=ground;
   g.TranslateTransform(529,588);var origin=new[]{PointF.Empty};using(var before=g.Transform)before.TransformPoints(origin);
   HallSpriteTransform.KeepAspect(g);
   var points=new[]{PointF.Empty,new PointF(100,0),new PointF(0,100)};using(var after=g.Transform)after.TransformPoints(points);
   float dx=points[1].X-points[0].X,dy=points[2].Y-points[0].Y;
   Check(Math.Abs(dx-dy)<.001&&Math.Abs(points[1].Y-points[0].Y)<.001&&Math.Abs(points[2].X-points[0].X)<.001,"upright sprites use uniform scaling without ground shear");
   Check(Math.Abs(points[0].X-origin[0].X)<.001&&Math.Abs(points[0].Y-origin[0].Y)<.001,"aspect correction preserves the seat anchor");
  }
  for(int i=0;i<4;i++){
   var seat=HallFurniture.Seat(i);Check(seat.Facing==HallWalkDirection.UpperRight,"guest faces this table");
   Check(Math.Abs(seat.Hip.Y-HallNavigation.Seats[i].Y+27)<.01,"seat and walking feet differ");
  }
  var guest=new HallGuest{stage=4,id=0};int[] expected={0,1,2,3};double[] times={.2,.8,1.4,2.1};
  for(int i=0;i<4;i++){guest.progress=times[i];Check(HallDiningMotion.Frame(guest,0)==expected[i],"spoon cycle");}
  guest.stage=1;guest.progress=50;Check(HallDiningMotion.Frame(guest,5)==0,"waiting remains seated without eating");
  guest.stage=5;guest.recipe=3;Check(HallDiningMotion.Row(guest)==0,"checkout stays seated and stops drinking");
  using(var sheet=new HallDiningSheet(Path.Combine(folder,"actions/guest-dining-ne.png"))){
   Check(sheet.Art.GetPixel(0,0).A==0,"sprite sheet has transparency");
   for(int row=0;row<3;row++)for(int frame=0;frame<4;frame++)using(var bitmap=new Bitmap(180,180)){
    using(var g=Graphics.FromImage(bitmap)){g.InterpolationMode=InterpolationMode.NearestNeighbor;sheet.Draw(g,new PointF(90,110),94,row,frame,false);}
    int visible=0,bottom=0;for(int y=0;y<180;y++)for(int x=0;x<180;x++)if(bitmap.GetPixel(x,y).A>128){visible++;bottom=Math.Max(bottom,y);}
    Check(visible>1500,"all twelve frame crops contain the seated customer");
    Check(bottom>=128&&bottom<=136,"boots keep their height below the seated hip in every pose");
   }
  }
  foreach(var spec in new[]{"aelia:6","lyse:6","guest:4","luchuan:2","rest:2","delivery:4"}){
   var parts=spec.Split(':');int rows=Int32.Parse(parts[1]);string name=parts[0];
   using(var sheet=new HallActionSheet(Path.Combine(folder,"actions",name=="delivery"?"delivery-walk.png":name+"-actions.png"),rows))using(var preview=new Bitmap(640,rows*150)){
    using(var g=Graphics.FromImage(preview)){
     g.Clear(Color.FromArgb(38,55,48));g.InterpolationMode=InterpolationMode.NearestNeighbor;
     for(int row=0;row<rows;row++)for(int frame=0;frame<4;frame++){
      var crop=sheet.Frame(row,frame);Check(crop.Width>100&&crop.Height>150,"each action crop contains a complete character");
      if(name=="aelia"&&row==2)Check(crop.Bottom<=735,"wipe frames exclude the next row's red hair");
      if(name=="aelia"&&row==3)Check(crop.Bottom<=985,"dish collection excludes drink preparation hair");
      sheet.Draw(g,new PointF(frame*160+80,row*150+140),125,row,frame,frame<2);
     }
    }
    preview.Save(Path.Combine(root,name+"-action-crops.png"));
   }
  }
  using(var art=Image.FromFile(Path.Combine(folder,"hall-empty.png")))using(var canvas=new TavernHallCanvas{Width=1400,Height=1132,Art=art,State=TavernHall.State(new Save())}){
   canvas.LoadWalking(folder);
   // Verify the actual foreground alpha, rather than only the character animation.
   var chairMethod=typeof(TavernHallCanvas).GetMethod("ChairFront",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
   Point[] cushionPixels={new Point(541,587),new Point(478,720),new Point(778,688),new Point(748,822)};
   Point[] woodPixels={new Point(495,560),new Point(431,693),new Point(727,659),new Point(691,794)};
   for(int table=0;table<4;table++)using(var mask=new Bitmap(1400,1132)){
    using(var g=Graphics.FromImage(mask))chairMethod.Invoke(canvas,new object[]{g,table});
    Check(mask.GetPixel(cushionPixels[table].X,cushionPixels[table].Y).A==0,"cushion must not overwrite the guest");
    Check(mask.GetPixel(woodPixels[table].X,woodPixels[table].Y).A>200,"wooden back must occlude the guest");
    mask.Save(Path.Combine(root,"chair-front-"+table+".png"));
   }
   for(int i=0;i<HallSeats.Count;i++)canvas.State.guests.Add(new HallGuest{id=i,table=i,stage=4,recipe=i%4});
   for(int frame=0;frame<4;frame++){
    canvas.Animation=times[frame];foreach(var c in canvas.State.guests)c.progress=times[frame];
    using(var bitmap=new Bitmap(1400,1132)){canvas.DrawToBitmap(bitmap,new Rectangle(0,0,1400,1132));bitmap.Save(Path.Combine(root,"hall-dining-"+frame+".png"));
     using(var detail=new Bitmap(1140,860))using(var g=Graphics.FromImage(detail)){g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.DrawImage(bitmap,new Rectangle(0,0,1140,860),new Rectangle(350,450,570,430),GraphicsUnit.Pixel);detail.Save(Path.Combine(root,"dining-detail-"+frame+".png"));}
    }
   }
   foreach(var c in canvas.State.guests)c.stage=1;
   using(var bitmap=new Bitmap(1400,1132)){canvas.DrawToBitmap(bitmap,new Rectangle(0,0,1400,1132));bitmap.Save(Path.Combine(root,"hall-waiting.png"));}
  }
  using(var art=Image.FromFile(Path.Combine(folder,"hall-forest-panorama.png")))using(var canvas=new TavernHallCanvas{Width=1200,Height=850,Art=art,Panorama=true,State=TavernHall.State(new Save())}){
   canvas.LoadWalking(folder);for(int i=0;i<HallSeats.Count;i++)canvas.State.guests.Add(new HallGuest{id=i,table=i,stage=4,recipe=i%4,progress=1.4});
   canvas.State.server.action="收桌";canvas.State.server.progress=7;canvas.State.server.job=0;
   using(var bitmap=new Bitmap(1200,850)){canvas.DrawToBitmap(bitmap,new Rectangle(0,0,1200,850));bitmap.Save(Path.Combine(root,"panorama-dining.png"));}
  }
  Console.WriteLine("PASS 19 occupied seats,  four chair foreground masks exclude cushions, seat directions, hip anchors, timed meal states, 12 sprite crops, hall and panorama renders");
 }
}
