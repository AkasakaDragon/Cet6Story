using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

public sealed class MenuLoadingScreen:Control {
 readonly Timer timer=new Timer{Interval=25};readonly Stopwatch time=new Stopwatch();bool prepared,finished;double readyAt;
 public Action Prepare,Completed;public Action<Exception> Failed;public string Destination,Word,Meaning;
 public MenuLoadingScreen(){Dock=DockStyle.Fill;BackColor=Color.Black;DoubleBuffered=true;TabStop=true;AccessibleName="正在加载";timer.Tick+=(s,e)=>TickFrame();}
 public void Start(){time.Restart();timer.Start();Focus();}
 void TickFrame(){if(finished)return;if(!prepared&&time.Elapsed.TotalSeconds>=.25){timer.Stop();try{if(Prepare!=null)Prepare();}catch(Exception ex){finished=true;if(Failed!=null)Failed(ex);return;}if(IsDisposed)return;prepared=true;readyAt=time.Elapsed.TotalSeconds;timer.Start();}
  Invalidate();if(prepared&&time.Elapsed.TotalSeconds>=Math.Max(1.35,readyAt+.55)){finished=true;timer.Stop();if(Completed!=null)Completed();}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);double age=time.Elapsed.TotalSeconds;double progress=prepared?.88+.12*Math.Min(1,(age-readyAt)/.5):Math.Min(.28,age*.8);DrawFrame(e.Graphics,ClientSize,age,progress,Destination,Word,Meaning);}
 public static void DrawFrame(Graphics g,Size size,double age,double progress,string destination,string word,string meaning){
  g.Clear(Color.Black);g.SmoothingMode=SmoothingMode.AntiAlias;float cx=size.Width/2f,cy=size.Height/2f-30,scale=Math.Max(.7f,Math.Min(1.3f,size.Width/1000f));float radius=29*scale;
  for(int i=0;i<12;i++){double angle=(i*30+age*135)*Math.PI/180;int alpha=45+(11-i)*18;float x=cx+(float)Math.Cos(angle)*radius,y=cy+(float)Math.Sin(angle)*radius;using(var b=new SolidBrush(Color.FromArgb(alpha,79,221,232)))g.FillRectangle(b,x-3*scale,y-3*scale,6*scale,6*scale);}
  using(var p=new Pen(Color.FromArgb(42,94,182,204),1))g.DrawEllipse(p,cx-radius-10,cy-radius-10,(radius+10)*2,(radius+10)*2);
  var diamond=new[]{new PointF(cx,cy-8*scale),new PointF(cx+8*scale,cy),new PointF(cx,cy+8*scale),new PointF(cx-8*scale,cy)};using(var p=new Pen(Color.FromArgb(150,84,225,229),1.5f))g.DrawPolygon(p,diamond);
  using(var font=GameTheme.Body(10))using(var b=new SolidBrush(Color.FromArgb(141,160,177))){string text="正在加载 · "+destination;SizeF measure=g.MeasureString(text,font);g.DrawString(text,font,b,cx-measure.Width/2,cy+radius+24);}
  float width=Math.Min(230,size.Width*.44f),top=cy+radius+55;using(var b=new SolidBrush(Color.FromArgb(20,38,45)))g.FillRectangle(b,cx-width/2,top,width,3);using(var b=new SolidBrush(Color.FromArgb(65,204,220)))g.FillRectangle(b,cx-width/2,top,(float)(width*Math.Max(0,Math.Min(1,progress))),3);
  using(var font=GameTheme.Body(9))using(var b=new SolidBrush(Color.FromArgb(103,130,144))){string text=((int)(progress*100))+"%";g.DrawString(text,font,b,cx-g.MeasureString(text,font).Width/2,top+10);}
  int margin=Math.Max(22,size.Width/35);using(var font=GameTheme.Body(12))using(var b=new SolidBrush(Color.FromArgb(146,197,205)))g.DrawString((word??"resonance")+"  ·  "+(meaning??"n. 共鸣；共振"),font,b,new RectangleF(margin,size.Height-margin-64,Math.Max(1,size.Width-margin*2),60),new StringFormat{Trimming=StringTrimming.EllipsisCharacter});
 }
 protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
}

public partial class Game {
 MenuLoadingScreen menuLoading;int loadingWordIndex;bool menuExitPending;
 void NavigateMenu(Action action,string destination,bool exit){
  if(menuExitPending||menuLoading!=null&&!menuLoading.IsDisposed)return;PlayMenuClick();
  if(exit){menuExitPending=true;var closeDelay=new Timer{Interval=170};closeDelay.Tick+=(s,e)=>{closeDelay.Dispose();if(!IsDisposed)action();};closeDelay.Start();return;}
  if(destination=="开始新游戏"&&save.hasGame){action();return;}
  string word="resonance",meaning="n. 共鸣；共振";
  var chapter=chapters.FirstOrDefault(c=>c.id==save.lastChapter)??chapters.FirstOrDefault();
  if(chapter!=null){var entries=PreparationWords(chapter);var pending=entries.Where(e=>!save.words.Any(w=>String.Equals(w.text,e.word,StringComparison.OrdinalIgnoreCase)&&w.box>=3)).ToList();if(pending.Count==0)pending=entries;if(pending.Count>0){var entry=pending[loadingWordIndex++%pending.Count];word=entry.word;meaning=entry.meaning;}}
  var screen=new MenuLoadingScreen{Destination=destination,Word=word,Meaning=meaning};menuLoading=screen;Controls.Add(screen);screen.BringToFront();
  screen.Prepare=action;Action release=()=>{if(menuLoading==screen)menuLoading=null;Controls.Remove(screen);screen.Dispose();if(!IsDisposed){content.Focus();UpdateRogueAudio();}};
  screen.Completed=release;screen.Failed=ex=>{release();GameMessage.Show(this,"页面加载失败："+ex.Message,"加载提示");};screen.Start();
 }
}
