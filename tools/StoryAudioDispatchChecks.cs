using System;using System.Linq;using System.Reflection;using System.Windows.Forms;using System.Collections.Generic;using System.Text;using System.Runtime.InteropServices;
class StoryAudioDispatchChecks {
 [DllImport("winmm.dll",CharSet=CharSet.Auto)]static extern int mciSendString(string command,StringBuilder result,int size,IntPtr window);
 static BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static object Get(Game g,string n){return typeof(Game).GetField(n,flags).GetValue(g);}static void Set(Game g,string n,object value){typeof(Game).GetField(n,flags).SetValue(g,value);}static object Call(Game g,string n,params object[] args){return typeof(Game).GetMethod(n,flags).Invoke(g,args);}
 static Game testGame;static string Status(string name){var result=new StringBuilder(100);Call(testGame,"mciSendString","status storyaudio "+name,result,100,IntPtr.Zero);return result.ToString().Trim();}
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 [STAThread]static void Main(){try{Run();}catch(Exception ex){Console.WriteLine("FAIL: "+ex.GetBaseException().Message);Environment.ExitCode=1;}}static void Run(){Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);Application.EnableVisualStyles();using(var g=new Game()){testGame=g;g.Opacity=0;g.Show();Application.DoEvents();var chapter=((List<Chapter>)Get(g,"chapters")).First(c=>c.id=="tavern-01-01");var save=new Save();save.completed.Add(TavernStory.Id);save.sectionStars[TavernStory.Id]=3;Set(g,"save",save);Set(g,"current",chapter);Set(g,"index",0);var attempt=(SectionAttempt)Call(g,"Attempt");attempt.subtitlesChosen=true;attempt.introShown=true;Call(g,"ShowStory");Call(g,"PrepareAudio");
var watch=System.Diagnostics.Stopwatch.StartNew();Call(g,"PlayCurrent");long launch=watch.ElapsedMilliseconds;
Check(launch<60,"warm playback blocks UI: "+launch);Call(g,"FlushStoryAudio");Check(Status("mode")=="playing","queued playback did not start");
Call(g,"StopAudio");Call(g,"FlushStoryAudio");Check(Status("mode")=="stopped","queued stop failed");
for(int i=0;i<6;i++){Call(g,"PlayCurrent");Call(g,"StopAudio");}Call(g,"FlushStoryAudio");Application.DoEvents();Check(!(bool)Get(g,"originalPlaying")&&Status("mode")=="stopped","cancelled callback restarted playback");
Call(g,"PlayCurrent");Call(g,"TogglePlay");Call(g,"FlushStoryAudio");Application.DoEvents();Console.WriteLine("paused state="+Status("mode"));Check(Status("mode")=="paused"&&(bool)Get(g,"pausedAudio"),"pause while start is pending failed");
Call(g,"TogglePlay");Call(g,"FlushStoryAudio");Check(Status("mode")=="playing","resume failed");
Call(g,"ShowWaystationWorldMap");Call(g,"FlushStoryAudio");Application.DoEvents();Check(!(bool)Get(g,"originalPlaying")&&Status("mode")=="stopped","map exit did not stop playback");
Console.WriteLine("Warm launch: "+launch+" ms");g.Close();}Console.WriteLine("PASS: nonblocking playback, queued stop, rapid cancellations, pending pause/resume and map exit.");}
}
