using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Drawing.Imaging;
public partial class ArtPanel {
 Bitmap sceneBackdrop;Image sceneBackdropArt;bool sceneBackdropHome,sceneBackdropPixel;
 void ClearSceneBackdrop(){if(sceneBackdrop!=null){sceneBackdrop.Dispose();sceneBackdrop=null;}}
 void DrawSceneBackdrop(Graphics g){
  if(sceneBackdrop==null||sceneBackdrop.Width!=Width||sceneBackdrop.Height!=Height||sceneBackdropArt!=Art||sceneBackdropHome!=Home||sceneBackdropPixel!=PixelArt){
   ClearSceneBackdrop();sceneBackdrop=new Bitmap(Math.Max(1,Width),Math.Max(1,Height),PixelFormat.Format32bppPArgb);
   using(var drawing=Graphics.FromImage(sceneBackdrop)){
    drawing.InterpolationMode=PixelArt?InterpolationMode.NearestNeighbor:InterpolationMode.HighQualityBicubic;drawing.PixelOffsetMode=PixelArt?PixelOffsetMode.Half:PixelOffsetMode.Default;
    if(Art!=null){if(Home)drawing.Clear(Color.FromArgb(12,24,29));drawing.DrawImage(Art,SceneArtBounds());}
    else using(var brush=new LinearGradientBrush(ClientRectangle,Color.FromArgb(25,57,78),Color.FromArgb(13,20,38),45))drawing.FillRectangle(brush,ClientRectangle);
    using(var shade=new SolidBrush(Color.FromArgb(Home?0:28,5,15,30)))drawing.FillRectangle(shade,ClientRectangle);
   }
   sceneBackdropArt=Art;sceneBackdropHome=Home;sceneBackdropPixel=PixelArt;
  }
  g.DrawImageUnscaled(sceneBackdrop,0,0);
 }
}
