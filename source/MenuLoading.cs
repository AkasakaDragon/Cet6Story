using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

public sealed class MenuLoadingScreen:Control {
 readonly Timer timer=new Timer{Interval=25};readonly Stopwatch time=new Stopwatch();bool prepared,finished;double readyAt;
 public Action Prepare,Completed;public Action<Exception> Failed;public string Destination,Word,Meaning;
 public MenuLoadingScreen(){Dock=DockStyle.Fill;BackColor=Color.FromArgb(25,53,55);DoubleBuffered=true;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Opaque,true);TabStop=true;AccessibleName="正在加载";timer.Tick+=(s,e)=>TickFrame();}
 public void Start(){time.Restart();timer.Start();Focus();}
 void TickFrame(){if(finished)return;if(!prepared&&time.Elapsed.TotalSeconds>=.25){timer.Stop();try{if(Prepare!=null)Prepare();}catch(Exception ex){finished=true;if(Failed!=null)Failed(ex);return;}if(IsDisposed)return;prepared=true;readyAt=time.Elapsed.TotalSeconds;timer.Start();}
  Invalidate();if(prepared&&time.Elapsed.TotalSeconds>=Math.Max(1.35,readyAt+.55)){finished=true;timer.Stop();if(Completed!=null)Completed();}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);double age=time.Elapsed.TotalSeconds;double progress=prepared?.88+.12*Math.Min(1,(age-readyAt)/.5):Math.Min(.28,age*.8);DrawFrame(e.Graphics,ClientSize,age,progress,Destination,Word,Meaning);}
 public static void DrawFrame(Graphics g,Size size,double age,double progress,string destination,string word,string meaning){
  if(loadingArt==null){string path=System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"assets","menu","loading-town.png");if(System.IO.File.Exists(path))loadingArt=Image.FromFile(path);}
  g.Clear(Color.FromArgb(25,53,55));g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
  if(loadingArt!=null)g.DrawImage(loadingArt,new Rectangle(Point.Empty,size));
  float scale=Math.Max(.5f,Math.Min(1.4f,size.Width/1672f));int width=Math.Min(size.Width-40,(int)(440*scale)),height=(int)(76*scale),cx=size.Width/2,top=(int)(size.Height*.54);
  var plate=new Rectangle(cx-width/2,top,width,height);GuildChrome.Draw(g,plate);
  using(var font=GameTheme.Body(17*scale,FontStyle.Bold))using(var ink=new SolidBrush(GuildChrome.Ivory))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})GameTheme.DrawPixelString(g,"正在加载 · "+destination,font,ink,plate,format);
  var bar=new Rectangle(cx-width/2,plate.Bottom+(int)(24*scale),width,Math.Max(14,(int)(28*scale)));GuildChrome.Draw(g,bar);
  using(var gold=new SolidBrush(Color.FromArgb(242,194,91)))g.FillRectangle(gold,bar.Left+10,bar.Top+7,Math.Max(0,(int)((bar.Width-20)*Math.Max(0,Math.Min(1,progress)))),Math.Max(1,bar.Height-14));
  using(var font=GameTheme.Body(13*scale))GameTheme.DrawText(g,((int)(progress*100))+"%",font,new Rectangle(bar.Left,bar.Bottom+6,bar.Width,Math.Max(24,(int)(32*scale))),GuildChrome.Ivory,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
  int margin=Math.Max(22,size.Width/28);using(var font=GameTheme.Body(14*scale))GameTheme.DrawText(g,(word??"resonance")+"  ·  "+(meaning??"n. 共鸣；共振"),font,new Rectangle(margin,size.Height-margin-55,size.Width-margin*2,55),GuildChrome.Ivory,TextFormatFlags.WordBreak|TextFormatFlags.VerticalCenter);
 }
 static Image loadingArt;
 protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
}

public partial class Game {
 MenuLoadingScreen menuLoading;int loadingWordIndex;bool menuExitPending;
 void NavigateMenu(Action action,string destination,bool exit,bool woodenSound=false,Action ready=null){
  if(menuExitPending||menuLoading!=null&&!menuLoading.IsDisposed)return;if(woodenSound)PlayWoodMenuSound(true);else PlayMenuClick();
  // Main-menu navigation already owns an overlay; unwrap these page entries
  // so every other entry point can use the same transition without nesting it.
  if(action==(Action)ShowWords){action=ShowWordsPage;destination="生词本";}
  else if(action==(Action)ShowCardCollection){action=ShowCardCollectionPage;destination="卡牌图鉴";}
  if(exit){menuExitPending=true;var closeDelay=new Timer{Interval=170};closeDelay.Tick+=(s,e)=>{closeDelay.Dispose();if(!IsDisposed)action();};closeDelay.Start();return;}
  if(action==(Action)ShowWordsPage){action();content.Refresh();content.Focus();UpdateRogueAudio();return;}
  if(destination=="开始新游戏"&&save.hasGame){action();return;}
  string word="resonance",meaning="n. 共鸣；共振";
  var chapter=chapters.FirstOrDefault(c=>c.id==save.lastChapter)??chapters.FirstOrDefault();
  if(chapter!=null){var entries=PreparationWords(chapter);var pending=entries.Where(e=>!save.words.Any(w=>String.Equals(w.text,e.word,StringComparison.OrdinalIgnoreCase)&&w.box>=3)).ToList();if(pending.Count==0)pending=entries;if(pending.Count>0){var entry=pending[loadingWordIndex++%pending.Count];word=entry.word;meaning=entry.meaning;}}
  var screen=new MenuLoadingScreen{Destination=destination,Word=word,Meaning=meaning};menuLoading=screen;Controls.Add(screen);screen.BringToFront();
  screen.Prepare=()=>{using(var redraw=new BattleRedrawScope(content)){action();content.PerformLayout();if(content.Width>0&&content.Height>0)using(var firstFrame=new Bitmap(content.Width,content.Height))content.DrawToBitmap(firstFrame,new Rectangle(Point.Empty,firstFrame.Size));}screen.BringToFront();};
  Action release=()=>{if(menuLoading==screen)menuLoading=null;using(var redraw=new BattleRedrawScope(this)){content.PerformLayout();content.Refresh();Controls.Remove(screen);screen.Dispose();}if(!IsDisposed){Refresh();content.Focus();UpdateRogueAudio();}};
  screen.Completed=()=>{release();if(!IsDisposed&&ready!=null)ready();};screen.Failed=ex=>{release();GameMessage.Show(this,"页面加载失败："+ex.Message,"加载提示");};screen.Start();
 }
}
