using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

public partial class Game {
 void UpdateWaystationPortraits(){
  if(!WaystationChapterOne.Dialogue(current)||stage==null)return;
  bool changed=!stage.WaystationPortraits;stage.WaystationPortraits=true;
  foreach(var a in stage.Actors){bool visible=WaystationChapterOne.Second(current)?(a.Id!="lyse"||(index>=6&&index<34)):WaystationChapterOne.Is(current)?(a.Id!="aelia"||index<28):index>=12&&(a.Id!="aelia"||index>=17),mirror=a.Id=="aelia"||a.Id=="lyse";float scale=a.Id=="luchuan"?1.1f:1f;
   if(TavernStory.Is(current)){visible=index<12?(a.Id=="luchuan"||a.Id=="iserya"):a.Id!="iserya"&&(a.Id!="aelia"||index>=17);}
   if(a.Visible!=visible||a.Mirror!=mirror||a.Scale!=scale)changed=true;
   if(WaystationChapterOne.Number(current)>=2)visible=true;
   a.Visible=visible;a.Mirror=mirror;a.Scale=scale;
  }
  if(changed)stage.Snap();
 }
}

public partial class ArtPanel {
 readonly System.Collections.Generic.Dictionary<string,Bitmap> portraitFrames=new System.Collections.Generic.Dictionary<string,Bitmap>();
 void ClearPortraitFrames(){foreach(var frame in portraitFrames.Values)frame.Dispose();portraitFrames.Clear();}
 void DrawWaystationPortraits(Graphics g){
  // Match the large dialogue portraits across aspect ratios; height drives their scale.
  float baseHeight=Math.Min(Math.Max(150,Height-52)*.74f,Width*.46f*1.33f);
  foreach(var a in Actors.Where(a=>a.Visible).OrderBy(a=>a.Id==ActiveActor?1:0)){
   float h=baseHeight*a.Scale,w=h*a.Image.Width/a.Image.Height;
   float center=Width*(a.Side=="right"?.82f:.18f);
   var bounds=new Rectangle((int)(center-w/2),(int)(Height-52-h),(int)w,(int)h);
   bool speaking=a.Id==ActiveActor;
   string key=System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(a.Image)+":"+bounds.Width+":"+bounds.Height+":"+speaking;
   Bitmap portrait;
   if(!portraitFrames.TryGetValue(key,out portrait)){
    portrait=new Bitmap(Math.Max(1,bounds.Width),Math.Max(1,bounds.Height),PixelFormat.Format32bppPArgb);
    using(var drawing=Graphics.FromImage(portrait))using(var attr=new ImageAttributes()){
     drawing.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;drawing.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.Half;
     if(!speaking)attr.SetColorMatrix(new ColorMatrix(new float[][]{
      new[]{.2126f,.2126f,.2126f,0f,0f},new[]{.7152f,.7152f,.7152f,0f,0f},new[]{.0722f,.0722f,.0722f,0f,0f},new[]{0f,0f,0f,1f,0f},new[]{0f,0f,0f,0f,1f}}));
     drawing.DrawImage(a.Image,new Rectangle(Point.Empty,portrait.Size),0,0,a.Image.Width,a.Image.Height,GraphicsUnit.Pixel,attr);
    }
    portraitFrames[key]=portrait;
   }
   {
    var state=g.Save();
    if(a.Mirror){g.TranslateTransform(bounds.Left+bounds.Right,0);g.ScaleTransform(-1,1);}
    g.DrawImageUnscaled(portrait,bounds.Location);
    g.Restore(state);
   }
  }
 }
}
