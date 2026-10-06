using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

public static class TrainingLobbyChecks {
 [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]struct WindowRect{public int Left,Top,Right,Bottom;}
 [System.Runtime.InteropServices.DllImport("dwmapi.dll")]static extern int DwmGetWindowAttribute(IntPtr window,int attribute,out WindowRect rect,int size);
 [System.Runtime.InteropServices.DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr window,int message,IntPtr wParam,IntPtr lParam);
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 static object Call(Game game,string name,params object[] args){return typeof(Game).GetMethod(name,Private).Invoke(game,args);}
 static CardRewardSurface Scene(Game game){return ((Panel)typeof(Game).GetField("content",Private).GetValue(game)).Controls.OfType<CardRewardSurface>().Single();}
 static void Capture(Game game,string path,bool refresh=true){if(refresh)game.Refresh();Application.DoEvents();WindowRect rect;if(DwmGetWindowAttribute(game.Handle,9,out rect,16)!=0)throw new Exception("Window capture bounds");using(var image=new Bitmap(rect.Right-rect.Left,rect.Bottom-rect.Top))using(var g=Graphics.FromImage(image)){g.CopyFromScreen(new Point(rect.Left,rect.Top),Point.Empty,image.Size);image.Save(path);}}
 static void CheckLibraryPainting(CardRewardSurface scene){
  foreach(var button in scene.Controls.OfType<RogueShowcaseButton>().Where(b=>b.Visible)){
   using(var actual=new Bitmap(button.Width,button.Height))using(var expected=new Bitmap(button.Width,button.Height)){
    button.DrawToBitmap(actual,new Rectangle(Point.Empty,actual.Size));
    using(var g=Graphics.FromImage(expected))scene.DrawBackdrop(g,button.Bounds);
    // Cut corners must reproduce the scene in both states, with no black blocks.
    foreach(int x in new[]{0,actual.Width-1})foreach(int y in new[]{0,actual.Height-1})if(actual.GetPixel(x,y).ToArgb()!=expected.GetPixel(x,y).ToArgb())throw new Exception("Plaque background corner: "+button.Text);
    int lightText=0;for(int y=actual.Height/3;y<actual.Height*2/3;y++)for(int x=actual.Width/4;x<actual.Width*3/4;x++)if(actual.GetPixel(x,y).ToArgb()==GuildChrome.Ivory.ToArgb())lightText++;
    if(lightText<15)throw new Exception("Library text is faded: "+button.Text);
   }
  }
 }
 static void EnterTraining(Game game){Call(game,"ShowRogueHome");Scene(game).Controls.OfType<RogueShowcaseButton>().Single(b=>b.Visible&&b.Text=="词域远征").PerformClick();Application.DoEvents();}
 static void LayoutCheck(CardRewardSurface scene){
  var cards=scene.Controls.OfType<RogueAdventureCard>().ToArray();if(cards.Length!=3||cards.Any(c=>!c.GuildStyle||c.Total<4))throw new Exception("Training cards");
  foreach(var c in scene.Controls.Cast<Control>().Where(c=>c.Visible))if(!scene.ClientRectangle.Contains(c.Bounds))throw new Exception("Clipped control: "+c.Text);
  for(int i=0;i<cards.Length;i++)for(int j=i+1;j<cards.Length;j++)if(cards[i].Bounds.IntersectsWith(cards[j].Bounds))throw new Exception("Overlapping cards");
  var nav=scene.Controls.OfType<RogueShowcaseButton>().ToArray();if(nav.Length!=5||nav.Any(b=>!b.GuildStyle))throw new Exception("Navigation style");
  if(nav.Count(b=>b.Visible)!=1||!nav.Single(b=>b.Visible).Text.Equals("返回图书馆"))throw new Exception("Utility entries must be hidden and retained");
  if(Math.Abs((cards[1].Left-cards[0].Right)-(cards[2].Left-cards[1].Right))>1)throw new Exception("Training gaps must be equal");
 }
 [STAThread]public static void Main(string[] args){try{Run(args);}catch(Exception error){Console.WriteLine("FAIL: "+error.GetType().Name+": "+error.Message);Environment.ExitCode=1;}}
 static void Run(string[] args){
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Directory.CreateDirectory(args[0]);
  using(var game=new Game()){
   // All test mutations persist inside a separate sandbox, never in the player save.
   var saveField=typeof(Game).GetField("save",Private);var original=(Save)saveField.GetValue(game);
   var test=Engine.Json.Deserialize<Save>(Engine.Json.Serialize(original));test.rogue.run=null;test.rogue.preparationActive=false;
   saveField.SetValue(game,test);typeof(Game).GetField("root",Private).SetValue(game,Path.Combine(args[0],"sandbox"));
   game.TopMost=true;game.Show();game.Bounds=new Rectangle(0,0,1280,780);game.Activate();Call(game,"ShowRogueHome");Application.DoEvents();
   var library=Scene(game);
   var visible=library.Controls.Cast<Control>().Where(c=>c.Visible).ToArray();
   if(visible.Length!=4||visible.Any(c=>!(c is RogueShowcaseButton)))throw new Exception("Library must display only four entries");
   if(library.Controls.OfType<RogueAdventureCard>().Count()!=3||library.Controls.OfType<RogueAdventureCard>().Any(c=>c.Visible))throw new Exception("Training controls must be retained and hidden");
   CheckLibraryPainting(library);Capture(game,Path.Combine(args[0],"library-wide.png"));
   foreach(var control in visible)typeof(Control).GetMethod("OnMouseEnter",Private).Invoke(control,new object[]{EventArgs.Empty});
   CheckLibraryPainting(library);Capture(game,Path.Combine(args[0],"library-hover.png"));
   foreach(var control in visible)typeof(Control).GetMethod("OnMouseLeave",Private).Invoke(control,new object[]{EventArgs.Empty});
   CheckLibraryPainting(library);Capture(game,Path.Combine(args[0],"library-after-hover.png"));
   game.Size=new Size(800,500);Application.DoEvents();
   foreach(var control in visible)if(!library.ClientRectangle.Contains(control.Bounds))throw new Exception("Library entry clipped");
   Capture(game,Path.Combine(args[0],"library-small.png"));
   game.Size=new Size(1280,780);EnterTraining(game);
   var scene=Scene(game);LayoutCheck(scene);Capture(game,Path.Combine(args[0],"training-wide.png"));
   CheckLibraryPainting(scene);
   var homeButton=scene.Controls.OfType<RogueShowcaseButton>().Single(b=>b.Visible);
   foreach(var entry in scene.Controls.OfType<RogueAdventureCard>()){
    if(!entry.HoverFeedback)throw new Exception("Training hover feedback missing");
    typeof(Control).GetMethod("OnMouseEnter",Private).Invoke(entry,new object[]{EventArgs.Empty});
   }
   typeof(Control).GetMethod("OnMouseEnter",Private).Invoke(homeButton,new object[]{EventArgs.Empty});CheckLibraryPainting(scene);Capture(game,Path.Combine(args[0],"training-home-hover.png"));
   typeof(Control).GetMethod("OnMouseLeave",Private).Invoke(homeButton,new object[]{EventArgs.Empty});CheckLibraryPainting(scene);
   foreach(var entry in scene.Controls.OfType<RogueAdventureCard>())typeof(Control).GetMethod("OnMouseLeave",Private).Invoke(entry,new object[]{EventArgs.Empty});
   if(scene.Controls.OfType<RogueShowcasePanel>().Any(p=>p.Visible&&p.Controls.Cast<Control>().Any(c=>c.Text.StartsWith("无提示答对"))))throw new Exception("Hint strip must be hidden");
   var cards=scene.Controls.OfType<RogueAdventureCard>().ToArray();var before=Engine.Json.Serialize(test.rogue.memory);
   game.Size=new Size(800,500);Application.DoEvents();LayoutCheck(scene);Capture(game,Path.Combine(args[0],"training-small.png"));
   if(Engine.Json.Serialize(test.rogue.memory)!=before)throw new Exception("Rendering changed learning memory");
   // Exercise each real training card; the original handler must start that mode.
   foreach(string mode in new[]{"基础训练","四级训练","六级挑战"}){
    test.rogue.run=null;EnterTraining(game);scene=Scene(game);
    var card=scene.Controls.OfType<RogueAdventureCard>().Single(c=>c.Mode==mode);
    typeof(Control).GetMethod("OnClick",Private).Invoke(card,new object[]{EventArgs.Empty});
    if(test.rogue.ActiveRun==null||test.rogue.ActiveRun.mode!=mode)throw new Exception("Training entry failed: "+mode);
   }
   game.Size=new Size(1280,780);EnterTraining(game);scene=Scene(game);LayoutCheck(scene);
   if(scene.Controls.OfType<RogueAdventureCard>().Any(c=>c.Enabled))throw new Exception("Busy run allows new training");
   var resume=scene.Controls.OfType<RogueShowcasePanel>().Single(p=>p.Visible&&p.Controls.OfType<RogueShowcaseButton>().Any());
   if(!resume.Controls.OfType<RogueShowcaseButton>().Any(b=>b.Text=="继续本局"))throw new Exception("Resume entry missing");
   Capture(game,Path.Combine(args[0],"training-busy.png"));
   scene.Controls.OfType<RogueShowcaseButton>().Single(b=>b.Visible&&b.Text=="返回图书馆").PerformClick();Application.DoEvents();
   var returned=Scene(game);if(returned.Controls.OfType<RogueAdventureCard>().Any(c=>c.Visible)||returned.Controls.OfType<RogueShowcaseButton>().Count(b=>b.Visible)!=4)throw new Exception("Return library navigation");
   returned.Controls.OfType<RogueShowcaseButton>().Single(b=>b.Visible&&b.Text=="主界面").PerformClick();Application.DoEvents();
   if((string)typeof(Game).GetField("page",Private).GetValue(game)!="home")throw new Exception("Library home navigation");
   Call(game,"ShowRogueHome");Scene(game).Controls.OfType<RogueShowcaseButton>().Single(b=>b.Visible&&b.Text=="生词本").PerformClick();
   if((string)typeof(Game).GetField("page",Private).GetValue(game)!="words"||typeof(Game).GetField("menuLoading",Private).GetValue(game)!=null)throw new Exception("Wordbook must open immediately without loading overlay");
   string firstWordbook=Path.Combine(args[0],"wordbook-first-frame.png"),readyWordbook=Path.Combine(args[0],"wordbook-first-frame-refreshed.png");
   Capture(game,firstWordbook,false);Capture(game,readyWordbook);
   using(var first=new Bitmap(firstWordbook))using(var ready=new Bitmap(readyWordbook)){
    int different=0,total=0;for(int y=60;y<first.Height-30;y+=3)for(int x=30;x<first.Width-40;x+=3){total++;if(first.GetPixel(x,y).ToArgb()!=ready.GetPixel(x,y).ToArgb())different++;}
    if(different>total/1000)throw new Exception("Wordbook first frame was incomplete: "+different+" / "+total);
   }
   Application.DoEvents();
   var wordsBody=(ExpeditionSurface)typeof(Game).GetField("rogueBody",Private).GetValue(game);
   if(wordsBody.Art!=(Image)Call(game,"RogueUiArt","wordbook-counter-morning")||!wordsBody.PixelArt||wordsBody.ShadeAlpha!=0)throw new Exception("Wordbook morning background");
   var wordsScene=Scene(game);var back=wordsScene.Controls.OfType<RogueShowcaseButton>().Single();
   if(back.Text!="返回图书馆"||!back.GuildStyle||Math.Abs(back.Left*2+back.Width-wordsScene.Width)>1)throw new Exception("Wordbook centered return entry");
   if(!back.LibraryStyle)throw new Exception("Wordbook return hover effect missing");
   CheckLibraryPainting(wordsScene);
   typeof(Control).GetMethod("OnMouseEnter",Private).Invoke(back,new object[]{EventArgs.Empty});
   CheckLibraryPainting(wordsScene);Capture(game,Path.Combine(args[0],"wordbook-return-hover.png"));
   typeof(Control).GetMethod("OnMouseLeave",Private).Invoke(back,new object[]{EventArgs.Empty});CheckLibraryPainting(wordsScene);
   if(!wordsScene.CompositeChildren||wordsBody.BackgroundSurface!=wordsScene)throw new Exception("Wordbook requires one composed background");
   if(wordsBody.Parent.Width>=wordsBody.Width||wordsBody.Parent.Right!=wordsScene.ClientSize.Width)throw new Exception("Wordbook scrollbar must be clipped outside viewport");
   var heading=wordsBody.Controls.OfType<RogueCard>().First();
   if(heading.Controls[0].Text!="生词本"||!heading.GuildStyle||!heading.Controls[1].Text.Contains("远征金币"))throw new Exception("Wordbook heading and statistics frame");
   if(wordsBody.Controls.OfType<RogueCard>().Any(c=>!c.GuildStyle))throw new Exception("Wordbook panel frames");
   Capture(game,Path.Combine(args[0],"wordbook-morning.png"));
   for(int step=0;step<36;step++){SendMessage(wordsBody.Handle,0x0115,new IntPtr(1),IntPtr.Zero);Application.DoEvents();}
   for(int step=0;step<12;step++){SendMessage(wordsBody.Handle,0x020A,new IntPtr(120<<16),IntPtr.Zero);Application.DoEvents();}
   for(int step=0;step<4;step++){SendMessage(wordsBody.Handle,0x020A,new IntPtr(-120<<16),IntPtr.Zero);Application.DoEvents();}
   if(wordsBody.AutoScrollPosition.Y>=0)throw new Exception("Native scroll test did not move the word list");
   string nativeScroll=Path.Combine(args[0],"wordbook-scrolled.png"),freshScroll=Path.Combine(args[0],"wordbook-scrolled-refreshed.png");
   Capture(game,nativeScroll,false);Capture(game,freshScroll);
   using(var actual=new Bitmap(nativeScroll))using(var expected=new Bitmap(freshScroll)){
    int different=0,total=0;for(int y=240;y<actual.Height-30;y+=3)for(int x=30;x<actual.Width-40;x+=3){total++;if(actual.GetPixel(x,y).ToArgb()!=expected.GetPixel(x,y).ToArgb())different++;}
    if(different>total/1000)throw new Exception("Native scrolling left stale pixels: "+different+" / "+total);
   }
   back.PerformClick();Application.DoEvents();
   if(Scene(game).Controls.OfType<RogueShowcaseButton>().Count(b=>b.Visible)!=4)throw new Exception("Wordbook return library navigation");
   Call(game,"ShowWords");if((string)typeof(Game).GetField("page",Private).GetValue(game)!="words"||typeof(Game).GetField("menuLoading",Private).GetValue(game)!=null)throw new Exception("Shared wordbook entry still shows loading");
   typeof(Game).GetField("wordbookSearch",Private).SetValue(game,"test");Call(game,"RenderWords");Application.DoEvents();
   wordsBody=(ExpeditionSurface)typeof(Game).GetField("rogueBody",Private).GetValue(game);
   if(wordsBody.Art!=(Image)Call(game,"RogueUiArt","wordbook-counter-morning"))throw new Exception("Wordbook background lost after filtering");
   Call(game,"ShowCardCollectionPage");Application.DoEvents();
   var collectionContent=(Panel)typeof(Game).GetField("content",Private).GetValue(game);
   var collection=collectionContent.Controls.OfType<CardCollectionCanvas>().Single();
   if(collection.SceneArt!=(Image)Call(game,"RogueUiArt","card-collection-morning")||collectionContent.Controls.Count!=1)throw new Exception("Collection background or old navigation remains");
   if(collection.PageCount!=(CardBattle.Cards.Length+7)/8||collection.PageIndex!=0)throw new Exception("Album spread count");
   var cached=(System.Collections.Generic.List<Bitmap>)typeof(CardCollectionCanvas).GetField("cards",Private).GetValue(collection);
   foreach(var bitmap in cached)if(bitmap.GetPixel(0,0).A!=0||bitmap.GetPixel(bitmap.Width-1,bitmap.Height-1).A!=0)throw new Exception("Card black backing remains");
   for(int slot=0;slot<8;slot++){
    var box=collection.GetCardBounds(slot);if(!collection.BookBounds.Contains(box))throw new Exception("Card outside book");
    if(slot<4&&box.Right>collection.BookBounds.Left+collection.BookBounds.Width/2||slot>=4&&box.Left<collection.BookBounds.Left+collection.BookBounds.Width/2)throw new Exception("Four cards per leaf");
    for(int other=slot+1;other<8;other++)if(box.IntersectsWith(collection.GetCardBounds(other)))throw new Exception("Overlapping album cards");
   }
   Capture(game,Path.Combine(args[0],"card-collection-morning.png"));
   int sounds=0;var originalSound=collection.PageTurnSound;collection.PageTurnSound=()=>{sounds++;originalSound();};
   collection.TurnPage(-1);if(sounds!=0||collection.IsTurning)throw new Exception("First leaf limit");
   collection.TurnPage(1);if(collection.PageIndex!=1||!collection.IsTurning||sounds!=1)throw new Exception("Animated album page turn");
   collection.TurnPage(1);if(sounds!=1)throw new Exception("Repeated turn during animation");
   var until=DateTime.UtcNow.AddMilliseconds(180);while(DateTime.UtcNow<until){Application.DoEvents();System.Threading.Thread.Sleep(8);}
   Capture(game,Path.Combine(args[0],"card-collection-turning.png"));
   until=DateTime.UtcNow.AddMilliseconds(260);while(DateTime.UtcNow<until){Application.DoEvents();System.Threading.Thread.Sleep(8);}
   if(collection.IsTurning)throw new Exception("Page turn did not finish");
   Capture(game,Path.Combine(args[0],"card-collection-second-spread.png"));
   collection.SetOffset(0);var cardBox=collection.GetCardBounds(0);
   typeof(Control).GetMethod("OnMouseDown",Private).Invoke(collection,new object[]{new MouseEventArgs(MouseButtons.Left,1,cardBox.Left+cardBox.Width/2,cardBox.Top+cardBox.Height/2,0)});Application.DoEvents();
   if(typeof(CardCollectionCanvas).GetField("enlarged",Private).GetValue(collection)==null)throw new Exception("Collection enlargement failed");
   Capture(game,Path.Combine(args[0],"card-collection-enlarged.png"));
   collection.TurnPage(1);if(collection.PageIndex!=0)throw new Exception("Turn behind enlarged card");
   typeof(Control).GetMethod("OnMouseDown",Private).Invoke(collection,new object[]{new MouseEventArgs(MouseButtons.Left,1,50,160,0)});Application.DoEvents();
   if(typeof(CardCollectionCanvas).GetField("enlarged",Private).GetValue(collection)!=null)throw new Exception("Collection dismissal failed");
   game.Size=new Size(800,500);Application.DoEvents();
   for(int slot=0;slot<8;slot++)if(!collection.BookBounds.Contains(collection.GetCardBounds(slot)))throw new Exception("Small album layout");
   Capture(game,Path.Combine(args[0],"card-collection-small.png"));game.Size=new Size(1280,780);Application.DoEvents();
   typeof(Control).GetMethod("OnMouseWheel",Private).Invoke(collection,new object[]{new MouseEventArgs(MouseButtons.None,0,0,0,-120)});if(collection.PageIndex!=1)throw new Exception("Wheel page turn");
   collection.SetOffset(int.MaxValue);if(collection.PageIndex!=collection.PageCount-1)throw new Exception("Final album spread");
   collection.TurnPage(1);if(collection.IsTurning)throw new Exception("Last leaf limit");
   collection.ReturnToLibrary();Application.DoEvents();if(Scene(game).Controls.OfType<RogueShowcaseButton>().Count(b=>b.Visible)!=4)throw new Exception("Album return to library");
   test.rogue.run=null;Call(game,"StartRogue","基础训练");Application.DoEvents();
   var battleRun=test.rogue.ActiveRun;TowerEngine.StartBattle(battleRun,test.rogue,"normal");Call(game,"RenderFullBattle",battleRun);Application.DoEvents();string battleBefore=Engine.Json.Serialize(battleRun);
   Call(game,"ShowBattleCardCollection",battleRun);
   var deadline=DateTime.UtcNow.AddSeconds(8);
   while((string)typeof(Game).GetField("page",Private).GetValue(game)!="card-collection"&&DateTime.UtcNow<deadline){Application.DoEvents();System.Threading.Thread.Sleep(15);}
   collection=collectionContent.Controls.OfType<CardCollectionCanvas>().Single();
   if(collection.ReturnText!="返回战斗")throw new Exception("Battle album return label");
   collection.ReturnToLibrary();Application.DoEvents();
   if((string)typeof(Game).GetField("page",Private).GetValue(game)!="rogue"||!Object.ReferenceEquals(test.rogue.ActiveRun,battleRun)||Engine.Json.Serialize(battleRun)!=battleBefore)throw new Exception("Album return changed battle state");
   Call(game,"ShowCardCollectionPage");Application.DoEvents();
   if(collectionContent.Controls.OfType<CardCollectionCanvas>().Single().ReturnText!="返回图书馆")throw new Exception("Battle return leaked into library album");
   Call(game,"CloseRogueAudio");
  }
  Console.WriteLine("PASS: responsive lobby, live progress unchanged, all three training actions, busy/resume state, navigation home. Player save untouched.");
 }
}
