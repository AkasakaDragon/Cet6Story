using System;using System.Collections.Generic;using System.Reflection;using System.Linq;
class StoryQuizSettingsChecks {
 static void Check(bool condition,string name){if(!condition)throw new Exception(name);}
 [STAThread]static void Main(){
  var old=Engine.Json.Deserialize<Save>("{}");Check(old.storyQuizEnabled??true,"old saves default to quizzes on");
  var save=new Save{storyQuizEnabled=false};var restored=Engine.Json.Deserialize<Save>(Engine.Json.Serialize(save));Check(restored.storyQuizEnabled==false,"switch persists");
  StoryQuizRules.MarkRead(restored,"neon-01-01");StoryQuizRules.MarkRead(restored,"neon-01-01");Check(restored.storyReadCompleted.Count==1&&restored.quizAnswers.Count==0&&restored.sectionStars.Count==0,"reading completion does not invent answers or stars");
  var first=new Chapter{id="neon-01-01",inlineQuestions=true};var next=new Chapter{id="neon-01-02",inlineQuestions=true};
  Check(SectionRules.Unlocked(next,new List<Chapter>{first,next},restored),"reading completion unlocks the next section");restored.storyQuizEnabled=true;Check(SectionRules.Unlocked(next,new List<Chapter>{first,next},restored),"reenabling quizzes preserves story access");
  restored.storyQuizEnabled=false;Engine.StartNewStory(restored,"first");Check(restored.storyQuizEnabled==false&&restored.storyReadCompleted.Count==0,"new game keeps setting and clears reading progress");
  using(var game=new Game()){
   var flags=BindingFlags.NonPublic|BindingFlags.Instance;typeof(Game).GetField("save",flags).SetValue(game,save);
   var question=new QuizQuestion{afterLine=0,prompt="test",options=new List<string>{"A","B","C","D"},answer=0};
   var chapter=new Chapter{id="test-story",inlineQuestions=true,questions=new List<QuizQuestion>{question}};
   typeof(Game).GetField("current",flags).SetValue(game,chapter);typeof(Game).GetField("index",flags).SetValue(game,0);
   var pending=typeof(Game).GetMethod("PendingInline",flags);Check(pending.Invoke(game,null)==null,"disabled quiz does not block next line");
   Check((bool)typeof(Game).GetMethod("ResolveInlineQuestion",flags).Invoke(game,null),"disabled quiz advances without a dialog");
   save.storyQuizEnabled=true;Check(pending.Invoke(game,null)==question,"reenabled quiz returns the real unanswered question");
   bool captured=false;var timer=new System.Windows.Forms.Timer{Interval=250};timer.Tick+=(sender,e)=>{
    var dialog=System.Windows.Forms.Application.OpenForms.Cast<System.Windows.Forms.Form>().FirstOrDefault(f=>f.Text=="游戏设置");if(dialog==null)return;
    timer.Stop();var controls=Children(dialog).ToList();controls.OfType<System.Windows.Forms.Button>().First(b=>b.Text=="剧情与听力").PerformClick();
    var toggle=controls.OfType<GoldCheckBox>().First(c=>c.AccessibleName=="主线剧情选择题");Check(toggle.Checked,"setting displays enabled state");toggle.Checked=false;Check(save.storyQuizEnabled==false,"settings control updates saved choice");
    using(var image=new System.Drawing.Bitmap(dialog.Width,dialog.Height)){dialog.DrawToBitmap(image,new System.Drawing.Rectangle(0,0,dialog.Width,dialog.Height));image.Save(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"story-quiz-settings.png"));}
    captured=true;dialog.Close();
   };timer.Start();typeof(Game).GetMethod("ShowInGameSettings",flags).Invoke(game,new object[]{game});timer.Dispose();Check(captured,"settings page renders");
  }
  Console.WriteLine("PASS default compatibility, persistence, quiz gating, story access, no fabricated scores, new game settings");
 }
 static IEnumerable<System.Windows.Forms.Control> Children(System.Windows.Forms.Control parent){foreach(System.Windows.Forms.Control child in parent.Controls){yield return child;foreach(var descendant in Children(child))yield return descendant;}}
}
