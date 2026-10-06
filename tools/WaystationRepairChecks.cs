using System;using System.IO;using System.Linq;using System.Drawing;using System.Reflection;using System.Windows.Forms;
class WaystationRepairChecks {
 static BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static object Get(Game g,string name){return typeof(Game).GetField(name,flags).GetValue(g);}
 static void Set(Game g,string name,object value){typeof(Game).GetField(name,flags).SetValue(g,value);}
 static object Call(Game g,string name,params object[] args){return typeof(Game).GetMethod(name,flags).Invoke(g,args);}
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static void Pump(int milliseconds){var end=DateTime.UtcNow.AddMilliseconds(milliseconds);while(DateTime.UtcNow<end){Application.DoEvents();System.Threading.Thread.Sleep(10);}}
 [STAThread] static void Main(){Application.EnableVisualStyles();using(var g=new Game()){
 g.Opacity=0;g.Show();var chapter=((System.Collections.Generic.List<Chapter>)Get(g,"chapters")).Single(TavernStory.Is);Engine.Validate(chapter,Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"chapters"));Check(chapter.lines.Count==45,"45 lines");
 var s=new Save();foreach(var flag in new[]{TavernStory.ScriptFlag,TavernStory.WaystationFlag,TavernStory.OpeningFlag,TavernStory.TransferFlag,TavernStory.BattleFlag})s.storyFlags.Add(flag);s.hasGame=true;s.lastChapter=chapter.id;s.positions[chapter.id]=24;Set(g,"save",s);Set(g,"current",chapter);Set(g,"index",24);
 s.tavernBattle=TavernStory.NewBattle(s.rogue);s.tavernBattle.state="won";Call(g,"ContinueTavernBattle");Call(g,"StopAudio");Check((string)Get(g,"page")=="story"&&(int)Get(g,"index")==25&&!s.completed.Contains(chapter.id),"victory continues dialogue before completion");
 for(int i=25;i<45;i++){Set(g,"index",i);Call(g,"UpdateLine");Pump(30);Check(chapter.lines[i].voiceRole==chapter.lines[i].speaker,"fixed voice roles");Check(chapter.lines[i].scene==chapter.lines[25+((i-25)/5)*5].scene,"five lines per background");if((i-25)%5==0){Pump(550);var stage=(ArtPanel)Get(g,"stage");using(var frame=new Bitmap(g.Width,g.Height)){g.DrawToBitmap(frame,new Rectangle(Point.Empty,frame.Size));frame.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"repair-"+((i-25)/5)+".png"));}Check(!stage.Controls.OfType<WhiteSceneReveal>().Any(),"transition releases overlay");}}
 s.positions[chapter.id]=35;Call(g,"ContinueGame");Call(g,"StopAudio");Check((int)Get(g,"index")==35,"repair resumes saved progress");
 Set(g,"index",44);s.heardLines.Add(chapter.id+"/line/44");Call(g,"Next");Check((string)Get(g,"page")=="tavern-ending"&&s.storyFlags.Contains("waystation-basic-repair-complete"),"last dialogue completes basic repair");int xp=s.xp;Call(g,"CompleteTavern");Check(s.xp==xp,"no duplicate completion reward");Console.WriteLine("PASS: victory, 20 voiced lines, four repair stages, fades, resume, final completion and reward deduplication.");g.Close();}
 }
}
