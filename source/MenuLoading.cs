using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

public sealed class MenuLoadingScreen:Control {
 readonly Timer timer=new Timer{Interval=25};readonly Stopwatch time=new Stopwatch();bool prepared,finished;double readyAt;int emblem;static readonly Random emblemRandom=new Random();static int lastEmblem=-1;
 public Action Prepare,Completed;public Action<Exception> Failed;public string Destination,Word,Meaning;
 public MenuLoadingScreen(){Dock=DockStyle.Fill;BackColor=Color.Black;DoubleBuffered=true;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Opaque,true);TabStop=true;AccessibleName="正在加载";timer.Tick+=(s,e)=>TickFrame();}
 public void Start(){lock(emblemRandom){emblem=emblemRandom.Next(lastEmblem<0?5:4);if(lastEmblem>=0&&emblem>=lastEmblem)emblem++;lastEmblem=emblem;}time.Restart();timer.Start();Focus();}
 void TickFrame(){if(finished)return;if(!prepared&&time.Elapsed.TotalSeconds>=.25){timer.Stop();try{if(Prepare!=null)Prepare();}catch(Exception ex){finished=true;if(Failed!=null)Failed(ex);return;}if(IsDisposed)return;prepared=true;readyAt=time.Elapsed.TotalSeconds;timer.Start();}
  Invalidate();if(prepared&&time.Elapsed.TotalSeconds>=Math.Max(1.35,readyAt+.55)){finished=true;timer.Stop();if(Completed!=null)Completed();}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);double age=time.Elapsed.TotalSeconds;double progress=prepared?.88+.12*Math.Min(1,(age-readyAt)/.5):Math.Min(.28,age*.8);DrawFrame(e.Graphics,ClientSize,age,progress,Destination,Word,Meaning,emblem);}
 public static void DrawFrame(Graphics g,Size size,double age,double progress,string destination,string word,string meaning,int emblem=0){
  g.Clear(Color.Black);g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
  EnsureEmblems();float scale=Math.Max(.5f,Math.Min(1.3f,size.Width/1280f));int cx=size.Width/2,cy=size.Height/2,side=(int)(125*scale);var art=emblems[Math.Max(0,Math.Min(4,emblem))];if(art!=null){float k=side/(float)Math.Max(art.Width,art.Height);int w=(int)(art.Width*k),h=(int)(art.Height*k);g.DrawImage(art,new Rectangle(cx-w/2,cy-h/2-25,w,h));}
  string text=(destination??"").Contains("战斗")?"正在进入战斗":"正在加载";
  using(var font=GameTheme.Body(12*scale))GameTheme.DrawText(g,text,font,new Rectangle(cx-200,cy+side/2+5,400,35),Color.FromArgb(216,207,184),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
  int dot=Math.Max(4,(int)(6*scale)),gap=dot*3,lit=(int)(age*3)%3;for(int i=0;i<3;i++)using(var ink=new SolidBrush(i==lit?GuildChrome.Gold:Color.FromArgb(67,55,33)))g.FillRectangle(ink,cx-gap+i*gap-dot/2,cy+side/2+52,dot,dot);
 }
 static readonly Bitmap[] emblems=new Bitmap[5];static bool emblemsLoaded;
 static void EnsureEmblems(){if(emblemsLoaded)return;emblemsLoaded=true;string dir=System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"assets","menu","loading");string sheet=System.IO.Path.Combine(dir,"emblems-v1.png"),shield=System.IO.Path.Combine(dir,"shield-preview-v1.png");if(System.IO.File.Exists(sheet))using(var art=new Bitmap(sheet)){int w=art.Width/2,h=art.Height/2;for(int i=0;i<4;i++)emblems[i+1]=TrimEmblem(art,new Rectangle(i%2*w,i/2*h,w,h));}if(System.IO.File.Exists(shield))using(var art=new Bitmap(shield))emblems[0]=TrimEmblem(art,new Rectangle((int)(art.Width*.37),(int)(art.Height*.29),(int)(art.Width*.26),(int)(art.Height*.27)));}
 static Bitmap TrimEmblem(Bitmap image,Rectangle cell){int left=cell.Right,top=cell.Bottom,right=cell.Left,bottom=cell.Top;for(int y=cell.Top;y<cell.Bottom;y++)for(int x=cell.Left;x<cell.Right;x++){var c=image.GetPixel(x,y);if(c.R<30&&c.G<30&&c.B<30)continue;left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}if(right<left)return null;return image.Clone(Rectangle.FromLTRB(left,top,right+1,bottom+1),System.Drawing.Imaging.PixelFormat.Format32bppArgb);}
 protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
}

public partial class Game {
 MenuLoadingScreen menuLoading;int loadingWordIndex;bool menuExitPending;
 void NavigateMenu(Action action,string destination,bool exit,bool woodenSound=false,Action ready=null){
  if(menuExitPending||menuLoading!=null&&!menuLoading.IsDisposed)return;if(woodenSound)PlayWoodMenuSound(true);else PlayMenuClick();
  if(action==(Action)ShowSettings||action==(Action)ShowAchievements){action();return;}
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
