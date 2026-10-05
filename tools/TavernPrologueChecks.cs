using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;

public static class TavernPrologueChecks {
 static object Get(Game g,string key){return typeof(Game).GetField(key,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(g);}
 static void Set(Game g,string key,object value){typeof(Game).GetField(key,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(g,value);}
 static object Call(Game g,string name,params object[] args){return typeof(Game).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(g,args);}
 static void Require(bool value,string message){if(!value)throw new Exception(message);}
 static void Capture(Game g,string name){Call(g,"StopAudio");Application.DoEvents();using(var image=new Bitmap(g.Width,g.Height)){g.DrawToBitmap(image,new Rectangle(Point.Empty,image.Size));image.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,name+".png"));}}
 [STAThread] public static void Main(){
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  string root=AppDomain.CurrentDomain.BaseDirectory;
  var c=Engine.Json.Deserialize<Chapter>(File.ReadAllText(Path.Combine(root,"chapters","10-tavern-prologue.json")));
  Engine.Validate(c,Path.Combine(root,"chapters"));Require(c.lines.Count==44&&c.questions.Count==4,"complete script");
  Require(c.questions.Select(q=>q.afterLine).SequenceEqual(new[]{7,19,29,43}),"segment question positions");
  Require(c.lines.All(l=>!String.IsNullOrWhiteSpace(l.translation)),"Chinese translation coverage");
  var fresh=new Save();var legacy=new Chapter{id="legacy",inlineQuestions=true};var next=new Chapter{id="next",inlineQuestions=true};
  var mixed=new List<Chapter>{legacy,c,next};Require(SectionRules.Unlocked(c,mixed,fresh),"new campaign independently unlocked");
  Require(!SectionRules.Unlocked(next,mixed,fresh),"legacy progression remains gated");fresh.sectionStars[legacy.id]=2;Require(SectionRules.Unlocked(next,mixed,fresh),"tavern does not become legacy prerequisite");
  var r=TavernStory.NewBattle(fresh.rogue);Require(!TavernStory.Answer(r,r.question.answer),"first-turn card tutorial gate");
  Require(CardBattle.Play(r,0)&&CardBattle.Play(r,0),"shield and guide usable");Require(r.cardBattle.shield==8&&r.cardBattle.bonus==6&&r.cardBattle.energy==1,"existing support effects");
  r=Engine.Json.Deserialize<RogueRun>(Engine.Json.Serialize(r));Require(r.cardBattle.shield==8&&r.cardBattle.hand.Count==0,"battle resume saves used cards");
  Require(TavernStory.Answer(r,r.question.answer),"correct attack");Require(!TavernStory.Answer(r,0),"no duplicate answer");
  while(r.state=="feedback"){TavernStory.NextTurn(r);TavernStory.Answer(r,r.question.answer);}
  Require(r.state=="won"&&r.hp>0,"tutorial winnable");
  var losing=TavernStory.NewBattle(fresh.rogue);CardBattle.Play(losing,0);CardBattle.Play(losing,0);
  for(int i=0;i<60&&losing.state!="lost";i++){TavernStory.Answer(losing,3);if(losing.state=="feedback")TavernStory.NextTurn(losing);}
  Require(losing.state=="lost"&&TavernStory.NewBattle(fresh.rogue).hp==70,"loss and retry");
  using(var game=new Game()){
   game.Opacity=0;game.Show();game.Bounds=new Rectangle(0,0,1280,780);Set(game,"save",new Save{openingCgPending=true});
   Call(game,"ShowMain");var stage=(ArtPanel)Get(game,"stage");Require(stage.Controls.OfType<VNButton>().Count()==7,"seven home actions");Capture(game,"home");
   Call(game,"EnterTavern");Application.DoEvents();Call(game,"StopAudio");Require((string)Get(game,"page")=="story","skip old CG and preparation");
   Require((bool)Get(game,"englishVisible")&&(bool)Get(game,"translating"),"first visit bilingual default");Capture(game,"dorm");
   ((VNButton)Get(game,"englishButton")).PerformClick();((VNButton)Get(game,"translationButton")).PerformClick();Require(!((Control)Get(game,"englishText")).Visible&&((Control)Get(game,"translationLabel")).Text=="","both subtitles hidden");Capture(game,"no-subtitles");
   ((VNButton)Get(game,"englishButton")).PerformClick();((VNButton)Get(game,"translationButton")).PerformClick();
   Set(game,"index",8);Call(game,"UpdateLine");Capture(game,"goddess");Set(game,"index",20);Call(game,"UpdateLine");Capture(game,"ruins");
   Set(game,"index",30);Call(game,"UpdateLine");Capture(game,"encounter");
   Set(game,"index",35);Call(game,"UpdateLine");var save=(Save)Get(game,"save");
   for(int i=0;i<=35;i++)save.heardLines.Add(c.id+"/line/"+i);
   Call(game,"PlayAll");Require((long)Get(game,"storyEndMs")== (long)(c.lines[35].end*1000),"continuous playback stops before tutorial");Call(game,"StopAudio");
   Call(game,"Next");Require((string)Get(game,"page")=="tavern-battle","story transitions into playable tutorial");Capture(game,"battle");
   game.Size=new Size(800,500);Application.DoEvents();Capture(game,"battle-small");game.Size=new Size(1280,780);
   save.tavernBattle=r;Call(game,"ShowTavernBattle");Capture(game,"battle-won");
   StoryRoutes.Flag(save,TavernStory.BattleFlag);save.tavernBattle=null;Set(game,"index",36);Call(game,"ShowStory");Capture(game,"after-battle");
   Set(game,"index",43);foreach(var q in c.questions)save.quizAnswers[c.id+"/q/"+q.afterLine]=(q.answer+1)%4;
   Call(game,"Complete");Require((string)Get(game,"page")=="tavern-ending"&&save.completed.Contains(c.id),"wrong answers still complete story");int xp=save.xp;Call(game,"Complete");Require(save.xp==xp,"completion reward once");Capture(game,"ending");
   var reloaded=Engine.Json.Deserialize<Save>(File.ReadAllText(Path.Combine(root,"save.json")));Require(reloaded.completed.Contains(c.id)&&reloaded.storyFlags.Contains(TavernStory.BattleFlag),"checkpoint persistence");
   game.Close();
  }
  Console.WriteLine("PASS: resources, independent entry/progression, free subtitle toggles, four listening segments, audio gate, real support effects, battle resume/win/loss, ending despite errors, reward deduplication and persistence.");
 }
}
