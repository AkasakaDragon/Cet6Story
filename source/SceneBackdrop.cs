using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Drawing.Imaging;
public partial class ArtPanel {
 Bitmap sceneBackdrop;Image sceneBackdropArt;bool sceneBackdropHome,sceneBackdropPixel,sceneBackdropArchive;int sceneBackdropDialogHeight;
 void ClearSceneBackdrop(){if(sceneBackdrop!=null){sceneBackdrop.Dispose();sceneBackdrop=null;}}
 void DrawSceneBackdrop(Graphics g){
  if(sceneBackdrop==null||sceneBackdrop.Width!=Width||sceneBackdrop.Height!=Height||sceneBackdropArt!=Art||sceneBackdropHome!=Home||sceneBackdropPixel!=PixelArt||sceneBackdropArchive!=ArchiveScene||sceneBackdropDialogHeight!=DialogHeight){
   ClearSceneBackdrop();sceneBackdrop=new Bitmap(Math.Max(1,Width),Math.Max(1,Height),PixelFormat.Format32bppPArgb);
   using(var drawing=Graphics.FromImage(sceneBackdrop)){
    drawing.InterpolationMode=PixelArt?InterpolationMode.NearestNeighbor:InterpolationMode.HighQualityBicubic;drawing.PixelOffsetMode=PixelArt?PixelOffsetMode.Half:PixelOffsetMode.Default;
    if(Art!=null){if(Home||ArchiveScene)drawing.Clear(ArchiveScene?Color.FromArgb(37,43,32):Color.FromArgb(12,24,29));var bounds=SceneArtBounds();if(ArchiveScene){int strip=Math.Min(24,Art.Width),tile=Math.Max(1,(int)Math.Round(strip*bounds.Height/(double)Art.Height));for(int x=bounds.Left-tile;x+tile>0;x-=tile)drawing.DrawImage(Art,new Rectangle(x,bounds.Top,tile,bounds.Height),new Rectangle(0,0,strip,Art.Height),GraphicsUnit.Pixel);for(int x=bounds.Right;x<Width;x+=tile)drawing.DrawImage(Art,new Rectangle(x,bounds.Top,tile,bounds.Height),new Rectangle(Art.Width-strip,0,strip,Art.Height),GraphicsUnit.Pixel);}drawing.DrawImage(Art,bounds);}
    else using(var brush=new LinearGradientBrush(ClientRectangle,Color.FromArgb(25,57,78),Color.FromArgb(13,20,38),45))drawing.FillRectangle(brush,ClientRectangle);
    using(var shade=new SolidBrush(Color.FromArgb(Home?0:28,5,15,30)))drawing.FillRectangle(shade,ClientRectangle);
   }
   sceneBackdropArt=Art;sceneBackdropHome=Home;sceneBackdropPixel=PixelArt;sceneBackdropArchive=ArchiveScene;sceneBackdropDialogHeight=DialogHeight;
  }
  g.DrawImageUnscaled(sceneBackdrop,0,0);
 }
}
