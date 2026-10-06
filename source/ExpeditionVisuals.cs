using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;using System.IO;

public static class ExpeditionVisuals {
 public static GraphicsPath Rounded(RectangleF r,float radius){var p=new GraphicsPath();float d=radius*2;p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
 public static void Background(Graphics g,Image art,Rectangle bounds,int shade,bool pixelArt=false){g.Clear(Color.FromArgb(23,32,37));if(art!=null){float scale=Math.Max(bounds.Width/(float)art.Width,bounds.Height/(float)art.Height);float w=art.Width*scale,h=art.Height*scale;g.InterpolationMode=pixelArt?InterpolationMode.NearestNeighbor:InterpolationMode.HighQualityBicubic;if(pixelArt)g.PixelOffsetMode=PixelOffsetMode.Half;g.DrawImage(art,(bounds.Width-w)/2,(bounds.Height-h)/2,w,h);}using(var brush=new SolidBrush(Color.FromArgb(shade,10,19,25)))g.FillRectangle(brush,bounds);}
 public static void Button(Graphics g,Rectangle r,string text,Font font,bool hover,bool enabled){CyberChrome.Button(g,r,text,font,false,hover,enabled);}
}
public class ExpeditionSurface:FlowLayoutPanel {
 public bool FullScrollRedraw,PixelArt,AlignToParentBackground;public CardRewardSurface BackgroundSurface;public int ShadeAlpha=55;public Image Art;Bitmap backdrop;Size backdropSize;Rectangle backdropBounds;
 public ExpeditionSurface(){DoubleBuffered=true;ResizeRedraw=true;SetStyle(ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;}
 [System.Runtime.InteropServices.DllImport("user32.dll")]static extern bool RedrawWindow(IntPtr window,IntPtr rect,IntPtr region,uint flags);
 protected override CreateParams CreateParams{get{var value=base.CreateParams;if(FullScrollRedraw)value.ExStyle|=0x02000000;return value;}}
 protected override void OnScroll(ScrollEventArgs e){base.OnScroll(e);if(FullScrollRedraw)Invalidate(true);}
 protected override void WndProc(ref Message message){
  int kind=message.Msg;base.WndProc(ref message);
  // Native scrolling copies old pixels before moving child windows. Repaint the
  // entire child tree after that move, so translucent plaques never retain them.
  if(FullScrollRedraw&&(kind==0x0114||kind==0x0115||kind==0x020A||kind==0x020E)&&!IsDisposed&&IsHandleCreated)
   RedrawWindow(Handle,IntPtr.Zero,IntPtr.Zero,0x0185);
 }
 protected override void OnPaintBackground(PaintEventArgs e){
  if(BackgroundSurface!=null){var origin=BackgroundSurface.PointToClient(PointToScreen(Point.Empty));BackgroundSurface.DrawBackdrop(e.Graphics,new Rectangle(origin,ClientSize));return;}
  var imageBounds=AlignToParentBackground&&Parent!=null?new Rectangle(-Left,-Top,Parent.ClientSize.Width,Parent.ClientSize.Height):ClientRectangle;
  if(backdrop==null||backdropSize!=ClientSize||backdropBounds!=imageBounds){
   if(backdrop!=null)backdrop.Dispose();backdrop=new Bitmap(Math.Max(1,Width),Math.Max(1,Height));
   using(var g=Graphics.FromImage(backdrop)){
    if(AlignToParentBackground){g.Clear(Color.FromArgb(23,32,37));g.InterpolationMode=PixelArt?InterpolationMode.NearestNeighbor:InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=PixelOffsetMode.Half;if(Art!=null)g.DrawImage(Art,imageBounds);}
    else ExpeditionVisuals.Background(g,Art,ClientRectangle,ShadeAlpha,PixelArt);
   }
   backdropSize=ClientSize;backdropBounds=imageBounds;
  }
  e.Graphics.DrawImageUnscaled(backdrop,0,0);
 }
 protected override void Dispose(bool disposing){if(disposing&&backdrop!=null)backdrop.Dispose();base.Dispose(disposing);}
}
public partial class Game {
 Image QuietScene(int theme){string path=Path.Combine(root,"assets","rogue","cards","quiet-scenes.png");if(!File.Exists(path))return TowerArt(new[]{"forest","temple","lava"}[Math.Max(0,Math.Min(2,theme))]);string key="quiet-scene:"+theme;Image cached;if(imageCache.TryGetValue(key,out cached))return cached;var atlas=CachedImage(path);int height=atlas.Height/3;var image=new Bitmap(atlas.Width,height);using(var g=Graphics.FromImage(image))g.DrawImage(atlas,new Rectangle(0,0,image.Width,image.Height),new Rectangle(0,Math.Max(0,Math.Min(2,theme))*height,atlas.Width,height),GraphicsUnit.Pixel);imageCache[key]=image;return image;}
 Image CardFrameAtlas(){string path=Path.Combine(root,"assets","rogue","cards","ornate-frames.png");return File.Exists(path)?CachedImage(path):null;}
}
