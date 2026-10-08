using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;

class StoryTimeChecks {
 [STAThread] static void Main(){try{Run();}catch(Exception error){Console.WriteLine("FAIL "+error.GetType().Name+": "+error.Message);if(error.InnerException!=null)Console.WriteLine("INNER "+error.InnerException.GetType().Name+": "+error.InnerException.Message);Environment.ExitCode=1;}}
 static void Run(){
  var json=new JavaScriptSerializer();
  string root=AppDomain.CurrentDomain.BaseDirectory;
  string[] phases={StoryTime.Morning,StoryTime.Morning,StoryTime.Dusk,StoryTime.Dusk,StoryTime.Night,StoryTime.Night};
  for(int n=1;n<=6;n++){
   var chapter=json.Deserialize<Chapter>(File.ReadAllText(Path.Combine(root,"chapters",(10+n)+"-tavern-01-0"+n+".json")));
   if(chapter.timeOfDay!=phases[n-1])throw new Exception("chapter phase "+n);
   for(int i=0;i<chapter.lines.Count;i++){
    string expected=n==4&&i>=18?StoryTime.Night:phases[n-1];
    if(StoryTime.Phase(chapter,chapter.lines[i])!=expected)throw new Exception("line phase "+n+"/"+i);
    if(!File.Exists(Path.Combine(root,"chapters",chapter.lines[i].scene)))throw new Exception("missing scene");
   }
   if(n==4){
    if(StoryTime.Phase(chapter,chapter.lines[18])!=StoryTime.Night||StoryTime.Phase(chapter,chapter.lines[17])!=StoryTime.Dusk)throw new Exception("previous line cannot restore dusk");
   }
   using(var panel=new Panel()){
    StoryTime.AddBadge(panel,phases[n-1]);
    if(panel.Controls.Count!=1||!panel.Controls[0].Text.Contains(StoryTime.Caption(phases[n-1])))throw new Exception("CG badge");
   }
  }
  if(StoryTime.Phase(new Chapter(),new Line())!=null)throw new Exception("unrelated chapter phase changed");
  var flags=BindingFlags.Instance|BindingFlags.NonPublic;
  using(var game=new Game()){
   game.Show();var type=typeof(Game);
   var chapters=(System.Collections.Generic.List<Chapter>)type.GetField("chapters",flags).GetValue(game);
   var save=(Save)type.GetField("save",flags).GetValue(game);
   var chapter=chapters.Single(c=>c.id=="tavern-01-04");
   save.completed.Add("tavern-01-03");
   save.sectionAttempts[chapter.id]=new SectionAttempt{review=true,introShown=true};
   type.GetField("current",flags).SetValue(game,chapter);type.GetField("index",flags).SetValue(game,17);
   type.GetMethod("ShowStory",flags).Invoke(game,null);
   foreach(int index in new[]{17,18,17}){
    type.GetField("index",flags).SetValue(game,index);type.GetMethod("UpdateLine",flags).Invoke(game,null);
    var clock=Stopwatch.StartNew();while(clock.ElapsedMilliseconds<400){Application.DoEvents();System.Threading.Thread.Sleep(8);}
    var hud=(Control)type.GetField("neonHud",flags).GetValue(game);
    if(!hud.Text.Contains(index==18?"第一天 · 晚上":"第一天 · 黄昏"))throw new Exception("runtime HUD phase");
    var panel=(Panel)type.GetField("content",flags).GetValue(game);
    if(panel.Controls.OfType<WhiteSceneReveal>().Any())throw new Exception("black transition leak");
    using(var frame=new Bitmap(panel.Width,panel.Height)){
     panel.DrawToBitmap(frame,panel.ClientRectangle);
     frame.Save(Path.Combine(root,index==18?"time-night-preview.png":"time-dusk-preview.png"));
    }
   }
   type.GetMethod("StopAudio",flags).Invoke(game,null);
  }
  Console.WriteLine("PASS six sections, every line, previous-line dusk restoration, scene files and CG time badges");
  Console.WriteLine("PASS runtime dusk/night/dusk HUD and black transition, isolated preview captures");
 }
}
