using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;using System.IO;

public static class ExpeditionVisuals {
 public static GraphicsPath Rounded(RectangleF r,float radius){var p=new GraphicsPath();float d=radius*2;p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
 public static void Background(Graphics g,Image art,Rectangle bounds,int shade){g.Clear(Color.FromArgb(23,32,37));if(art!=null){float scale=Math.Max(bounds.Width/(float)art.Width,bounds.Height/(float)art.Height);float w=art.Width*scale,h=art.Height*scale;g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.DrawImage(art,(bounds.Width-w)/2,(bounds.Height-h)/2,w,h);}using(var brush=new SolidBrush(Color.FromArgb(shade,10,19,25)))g.FillRectangle(brush,bounds);}
 public static void Button(Graphics g,Rectangle r,string text,Font font,bool hover,bool enabled){g.SmoothingMode=SmoothingMode.AntiAlias;var rect=new RectangleF(3,3,r.Width-7,r.Height-7);using(var path=Rounded(rect,Math.Min(12,r.Height/3)))using(var fill=new LinearGradientBrush(rect,hover?Color.FromArgb(72,91,96):Color.FromArgb(42,60,67),Color.FromArgb(17,29,37),90))using(var edge=new Pen(enabled?(hover?Color.FromArgb(255,216,136):Color.FromArgb(152,140,108)):Color.FromArgb(74,85,89),1.5f)){g.FillPath(fill,path);g.DrawPath(edge,path);using(var shine=new Pen(Color.FromArgb(65,244,227,171),1))g.DrawLine(shine,rect.X+14,rect.Y+2,rect.Right-14,rect.Y+2);TextRenderer.DrawText(g,text,font,r,enabled?Color.FromArgb(247,233,202):Color.FromArgb(129,140,143),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}}
}
public class ExpeditionSurface:FlowLayoutPanel {
 public Image Art;Bitmap backdrop;Size backdropSize;
 public ExpeditionSurface(){DoubleBuffered=true;ResizeRedraw=true;SetStyle(ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;}
 protected override void OnPaintBackground(PaintEventArgs e){if(backdrop==null||backdropSize!=ClientSize){if(backdrop!=null)backdrop.Dispose();backdrop=new Bitmap(Math.Max(1,Width),Math.Max(1,Height));using(var g=Graphics.FromImage(backdrop))ExpeditionVisuals.Background(g,Art,ClientRectangle,55);backdropSize=ClientSize;}e.Graphics.DrawImageUnscaled(backdrop,0,0);}
 protected override void Dispose(bool disposing){if(disposing&&backdrop!=null)backdrop.Dispose();base.Dispose(disposing);}
}
public partial class Game {
 Image QuietScene(int theme){string path=Path.Combine(root,"assets","rogue","cards","quiet-scenes.png");if(!File.Exists(path))return TowerArt(new[]{"forest","temple","lava"}[Math.Max(0,Math.Min(2,theme))]);string key="quiet-scene:"+theme;Image cached;if(imageCache.TryGetValue(key,out cached))return cached;var atlas=CachedImage(path);int height=atlas.Height/3;var image=new Bitmap(atlas.Width,height);using(var g=Graphics.FromImage(image))g.DrawImage(atlas,new Rectangle(0,0,image.Width,image.Height),new Rectangle(0,Math.Max(0,Math.Min(2,theme))*height,atlas.Width,height),GraphicsUnit.Pixel);imageCache[key]=image;return image;}
 Image CardFrameAtlas(){string path=Path.Combine(root,"assets","rogue","cards","ornate-frames.png");return File.Exists(path)?CachedImage(path):null;}
}
