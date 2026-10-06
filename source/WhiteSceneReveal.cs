using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

// Let the final white CG frame clear into the destination without a hard cut.
public class WhiteSceneReveal:Control {
 readonly Stopwatch clock=new Stopwatch();readonly Timer timer=new Timer{Interval=20};
 public WhiteSceneReveal(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);BackColor=Color.Transparent;timer.Tick+=(s,e)=>{if(clock.Elapsed.TotalMilliseconds>=800)Dispose();else Invalidate();};}
 public void Start(){clock.Start();timer.Start();}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);int alpha=(int)Math.Round(255*Math.Max(0,1-clock.Elapsed.TotalMilliseconds/800));using(var brush=new SolidBrush(Color.FromArgb(alpha,Color.White)))e.Graphics.FillRectangle(brush,ClientRectangle);}
 protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
}
