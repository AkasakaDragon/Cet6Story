using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

// Let the final white CG frame clear into the destination without a hard cut.
public class WhiteSceneReveal:Control {
 readonly Stopwatch clock=new Stopwatch();readonly Timer timer=new Timer{Interval=20};Bitmap scene;
 public Action Completed;
 public int DurationMs=800;public int HoldMs=0;public Color FadeColor=Color.White;
 public WhiteSceneReveal(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Opaque,true);BackColor=Color.White;timer.Tick+=(s,e)=>{if(clock.Elapsed.TotalMilliseconds>=HoldMs+DurationMs){var completed=Completed;Dispose();if(completed!=null)completed();}else Invalidate();};}
 public void CaptureScene(Control destination){scene=new Bitmap(Math.Max(1,destination.Width),Math.Max(1,destination.Height));destination.DrawToBitmap(scene,new Rectangle(Point.Empty,scene.Size));}
 public void Start(){clock.Start();timer.Start();}
 protected override void OnPaintBackground(PaintEventArgs e){}
 protected override void OnPaint(PaintEventArgs e){if(scene!=null&&scene.Size==ClientSize)e.Graphics.DrawImageUnscaled(scene,0,0);else if(scene!=null)e.Graphics.DrawImage(scene,ClientRectangle);else e.Graphics.Clear(FadeColor);int alpha=(int)Math.Round(255*Math.Min(1,Math.Max(0,1-(clock.Elapsed.TotalMilliseconds-HoldMs)/DurationMs)));using(var brush=new SolidBrush(Color.FromArgb(alpha,FadeColor)))e.Graphics.FillRectangle(brush,ClientRectangle);}
 protected override void Dispose(bool disposing){if(disposing){timer.Dispose();if(scene!=null){scene.Dispose();scene=null;}}base.Dispose(disposing);}
}
