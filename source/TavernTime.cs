using System;using System.Drawing;using System.Drawing.Drawing2D;using System.IO;using System.Windows.Forms;

public class TavernTimeState {
 public int day{get;set;}public string phase{get;set;}public string weather{get;set;}
 public TavernTimeState(){day=1;phase=StoryTime.Morning;weather="clear";}
}
public static class TavernTime {
 public static TavernTimeState State(Save save){if(save.tavernTime==null)save.tavernTime=new TavernTimeState();var t=save.tavernTime;if(t.day<1)t.day=1;if(t.phase!=StoryTime.Morning&&t.phase!=StoryTime.Dusk&&t.phase!=StoryTime.Night)t.phase=StoryTime.Morning;if(t.weather!="clear"&&t.weather!="cloudy"&&t.weather!="rain")t.weather="clear";return t;}
 public static string Weather(string value){return value=="rain"?"雨天":value=="cloudy"?"多云":"晴天";}
 public static void Advance(Save save,Random random){var t=State(save);if(t.phase==StoryTime.Morning)t.phase=StoryTime.Dusk;else if(t.phase==StoryTime.Dusk)t.phase=StoryTime.Night;else{t.phase=StoryTime.Morning;t.day++;}t.weather=new[]{"clear","cloudy","rain"}[random.Next(3)];}
 // Relight the existing room and clip weather to its windows and exterior doorway.
 public static Bitmap Backdrop(Image original,TavernTimeState t){var b=new Bitmap(original.Width,original.Height);using(var g=Graphics.FromImage(b)){
  g.DrawImageUnscaled(original,0,0);Color tint=t.phase==StoryTime.Night?Color.FromArgb(92,10,24,57):t.phase==StoryTime.Dusk?Color.FromArgb(40,82,40,49):Color.Transparent;using(var brush=new SolidBrush(tint))g.FillRectangle(brush,0,0,b.Width,b.Height);
 }return b;}
}
public sealed class TavernClock:Control {
 public Image Art;public TavernTimeState Time;
 readonly Timer animation=new Timer{Interval=16};readonly System.Diagnostics.Stopwatch elapsed=new System.Diagnostics.Stopwatch();
 double animatedAngle;Action finished;public bool Animating{get{return animation.Enabled;}}public double DisplayAngle{get{return Animating?animatedAngle:Angle(Time==null?StoryTime.Morning:Time.phase);}}
 static double Angle(string phase){return phase==StoryTime.Morning?-90:phase==StoryTime.Dusk?30:150;}
 public void AnimateNext(Action complete){if(Animating)return;double start=Angle(Time.phase);animatedAngle=start;finished=complete;elapsed.Restart();animation.Tick+=(AnimationTick);animation.Start();Invalidate();}
 void AnimationTick(object sender,EventArgs e){double p=Math.Min(1,elapsed.Elapsed.TotalMilliseconds/1400);double eased=p*p*(3-2*p);animatedAngle=Angle(Time.phase)+120*eased;Invalidate();if(p>=1){animation.Stop();animation.Tick-=AnimationTick;elapsed.Stop();var done=finished;finished=null;if(done!=null)done();}}
 protected override void Dispose(bool disposing){if(disposing){animation.Stop();animation.Dispose();finished=null;}base.Dispose(disposing);}
 public TavernClock(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer,true);BackColor=Color.Transparent;Cursor=Cursors.Hand;TabStop=true;AccessibleRole=AccessibleRole.PushButton;AccessibleName="时钟：跳过当前时间节点";}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.SmoothingMode=SmoothingMode.None;int side=Math.Min(Width,Height-42);if(Art!=null)g.DrawImage(Art,new Rectangle((Width-side)/2,0,side,side));if(Time!=null){using(var font=GameTheme.Body(10))GameTheme.DrawText(g,"第"+Time.day+"天 · "+StoryTime.Caption(Time.phase)+" · "+TavernTime.Weather(Time.weather),font,new Rectangle(0,side,Width,42),GameTheme.Gold,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);var state=g.Save();g.TranslateTransform(Width/2f,side/2f);g.RotateTransform((float)DisplayAngle);float length=side*.32f,thickness=Math.Max(2,side*.016f);var points=new[]{new PointF(-side*.055f,0),new PointF(length*.68f,-thickness),new PointF(length*.72f,-thickness*2.8f),new PointF(length,0),new PointF(length*.72f,thickness*2.8f),new PointF(length*.68f,thickness)};using(var brush=new SolidBrush(GameTheme.Gold))using(var edge=new Pen(Color.FromArgb(65,43,25),2)){g.FillPolygon(brush,points);g.DrawPolygon(edge,points);}g.Restore(state);using(var brush=new SolidBrush(GameTheme.Gold))g.FillEllipse(brush,Width/2f-4,side/2f-4,8,8);}if(Focused)ControlPaint.DrawFocusRectangle(g,ClientRectangle);}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button==MouseButtons.Left&&ClientRectangle.Contains(e.Location)){Focus();OnClick(EventArgs.Empty);}}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){OnClick(EventArgs.Empty);e.Handled=true;}}
}
public partial class Game {
 readonly Random tavernWeatherRandom=new Random();
 void SkipTavernTime(TavernClock clock){if(clock.Animating)return;var t=TavernTime.State(save);string next=t.phase==StoryTime.Morning?"黄昏":t.phase==StoryTime.Dusk?"晚上":"次日早上";if(GameMessage.Show(this,"当前：第"+t.day+"天 · "+StoryTime.Caption(t.phase)+"。\n是否跳过这个时间节点，前往"+next+"？\n屋外天气会重新随机变化。","时间流逝",MessageBoxButtons.YesNo)!=DialogResult.Yes)return;foreach(Control entry in clock.Parent.Controls)entry.Enabled=false;clock.AnimateNext(()=>{TavernTime.Advance(save,tavernWeatherRandom);Persist();ShowTavernHub();});}
}


