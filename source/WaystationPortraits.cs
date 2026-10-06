using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

public partial class Game {
 void UpdateWaystationPortraits(){
  if(!TavernStory.Is(current)||stage==null)return;
  stage.WaystationPortraits=true;
  foreach(var a in stage.Actors){a.Visible=index>=12&&(a.Id!="aelia"||index>=17);a.Mirror=a.Id=="aelia";a.Scale=a.Id=="luchuan"?1.1f:1f;}
  stage.Snap();
 }
}

public partial class ArtPanel {
 void DrawWaystationPortraits(Graphics g){
  float baseHeight=Math.Min(Math.Max(150,Height-65)*.95f,Width*.29f*1.33f)*.8f;
  foreach(var a in Actors.Where(a=>a.Visible).OrderBy(a=>a.Id==ActiveActor?1:0)){
   float h=baseHeight*a.Scale,w=h*a.Image.Width/a.Image.Height;
   float center=Width*(a.Side=="right"?.82f:.18f);
   var bounds=new Rectangle((int)(center-w/2),(int)(Height-52-h),(int)w,(int)h);
   bool speaking=a.Id==ActiveActor;
   using(var attr=new ImageAttributes()){
    if(!speaking)attr.SetColorMatrix(new ColorMatrix(new float[][]{
     new[]{.2126f,.2126f,.2126f,0f,0f},new[]{.7152f,.7152f,.7152f,0f,0f},new[]{.0722f,.0722f,.0722f,0f,0f},new[]{0f,0f,0f,1f,0f},new[]{0f,0f,0f,0f,1f}}));
    var state=g.Save();
    if(a.Mirror){g.TranslateTransform(bounds.Left+bounds.Right,0);g.ScaleTransform(-1,1);}
    g.DrawImage(a.Image,bounds,0,0,a.Image.Width,a.Image.Height,GraphicsUnit.Pixel,attr);
    g.Restore(state);
   }
  }
 }
}
