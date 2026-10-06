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
  Engine.Validate(c,Path.Combine(root,"chapters"));Require(c.lines.Count==36&&c.questions.Count==4,"complete script");
  Require(c.questions.Select(q=>q.afterLine).SequenceEqual(new[]{1,11,21,35}),"segment question positions after CG");
  Require(c.lines.All(l=>!String.IsNullOrWhiteSpace(l.translation)),"Chinese translation coverage");
  Require(c.lines.First().speaker=="伊瑟雅"&&c.lines.All(l=>l.scene!="art/tavern/dorm-midnight.png"),"dorm dialogue removed");
  var migrated=new Save();migrated.positions[c.id]=14;migrated.heardLines.Add(c.id+"/line/14");migrated.heardLines.Add(c.id+"/line/3");migrated.quizAnswers[c.id+"/q/19"]=3;migrated.answerStarted[c.id+"/q/19"]=123;migrated.answerTimely[c.id+"/q/19"]=true;
  TavernStory.Migrate(migrated);TavernStory.Migrate(migrated);Require(migrated.positions[c.id]==6&&migrated.heardLines.SequenceEqual(new[]{c.id+"/line/6"})&&migrated.quizAnswers[c.id+"/q/11"]==3&&migrated.answerStarted[c.id+"/q/11"]==123&&migrated.answerTimely[c.id+"/q/11"],"old progress remapped once");
  var fresh=new Save();var legacy=new Chapter{id="legacy",inlineQuestions=true};var next=new Chapter{id="next",inlineQuestions=true};
  var mixed=new List<Chapter>{legacy,c,next};Require(SectionRules.Unlocked(c,mixed,fresh),"new campaign independently unlocked");
  Require(!SectionRules.Unlocked(next,mixed,fresh),"legacy progression remains gated");fresh.sectionStars[legacy.id]=2;Require(SectionRules.Unlocked(next,mixed,fresh),"tavern does not become legacy prerequisite");
  var r=TavernStory.NewBattle(fresh.rogue);Require(!TavernStory.Answer(r,r.question.answer),"first-turn card tutorial gate");
  Require(r.cardBattle.monster=="荆棘孢子兽","existing requested monster used in tutorial");
  Require(CardBattle.Play(r,0)&&CardBattle.Play(r,0),"shield and guide usable");Require(r.cardBattle.shield==8&&r.cardBattle.bonus==6&&r.cardBattle.energy==1,"existing support effects");
  r=Engine.Json.Deserialize<RogueRun>(Engine.Json.Serialize(r));Require(r.cardBattle.shield==8&&r.cardBattle.hand.Count==0,"battle resume saves used cards");
  Require(TavernStory.Answer(r,r.question.answer),"correct attack");Require(!TavernStory.Answer(r,0),"no duplicate answer");
  while(r.state=="feedback"){TavernStory.NextTurn(r);TavernStory.Answer(r,r.question.answer);}
  Require(r.state=="won"&&r.hp>0,"tutorial winnable");
  var losing=TavernStory.NewBattle(fresh.rogue);CardBattle.Play(losing,0);CardBattle.Play(losing,0);
  for(int i=0;i<60&&losing.state!="lost";i++){TavernStory.Answer(losing,3);if(losing.state=="feedback")TavernStory.NextTurn(losing);}
  Require(losing.state=="lost"&&TavernStory.NewBattle(fresh.rogue).hp==70,"loss and retry");
  var retaining=TavernStory.NewBattle(fresh.rogue);CardBattle.Play(retaining,0);CardBattle.Play(retaining,0);TavernStory.Answer(retaining,3);TavernStory.NextTurn(retaining);
  var unplayed=retaining.cardBattle.hand.ToList();Require(unplayed.Count==2,"next turn draws two cards");TavernStory.Answer(retaining,3);retaining=Engine.Json.Deserialize<RogueRun>(Engine.Json.Serialize(retaining));TavernStory.NextTurn(retaining);
  Require(retaining.cardBattle.hand.Take(unplayed.Count).SequenceEqual(unplayed)&&retaining.cardBattle.hand.Count==4&&retaining.cardBattle.energy==3,"unplayed cards survive save and next round alongside new draws");
  using(var game=new Game()){
   game.Opacity=0;game.Show();game.Bounds=new Rectangle(0,0,1280,780);Set(game,"save",new Save{openingCgPending=true});
   Call(game,"ShowMain");var stage=(ArtPanel)Get(game,"stage");Require(stage.Controls.OfType<VNButton>().Count()==6,"main menu replaces separate tavern entry");Capture(game,"home");
   Call(game,"StartNewGame");Application.DoEvents();Require((string)Get(game,"page")=="tavern-opening"&&((Chapter)Get(game,"current")).id==TavernStory.Id,"default new game starts tavern CG");
   var content=(Control)Get(game,"content");var cg=content.Controls.OfType<OpeningCgCanvas>().Single();
   var deadline=DateTime.UtcNow.AddSeconds(3);while(cg.FramesShown==0&&DateTime.UtcNow<deadline){Application.DoEvents();System.Threading.Thread.Sleep(20);}
   Require(cg.FramesShown>0&&cg.AudioStarted,"CG video and audio start");Capture(game,"opening");
   cg.Controls.OfType<VNButton>().Single().PerformClick();Application.DoEvents();Call(game,"StopAudio");Require((string)Get(game,"page")=="story"&&(int)Get(game,"index")==0,"skip enters goddess directly");
   Require(((Save)Get(game,"save")).storyFlags.Contains(TavernStory.OpeningFlag),"CG checkpoint persisted");
   var continued=(Save)Get(game,"save");continued.lastChapter="neon-01-01";continued.positions["neon-01-01"]=3;int retainedXp=continued.xp;
   Call(game,"ContinueGame");Application.DoEvents();Call(game,"StopAudio");Require((string)Get(game,"page")=="story"&&((Chapter)Get(game,"current")).id==TavernStory.Id&&continued.xp==retainedXp&&continued.positions["neon-01-01"]==3,"legacy continue enters tavern and retains saved learning progress");
   var openingSave=(Save)Get(game,"save");openingSave.storyFlags.Remove(TavernStory.OpeningFlag);openingSave.positions[c.id]=0;Call(game,"EnterTavern");
   deadline=DateTime.UtcNow.AddSeconds(23);while((string)Get(game,"page")=="tavern-opening"&&DateTime.UtcNow<deadline){Application.DoEvents();System.Threading.Thread.Sleep(20);}
   Call(game,"StopAudio");Require((string)Get(game,"page")=="story"&&(int)Get(game,"index")==0,"natural CG ending enters goddess");
   Require((bool)Get(game,"englishVisible")&&(bool)Get(game,"translating"),"first visit bilingual default");Capture(game,"dorm");
   ((VNButton)Get(game,"englishButton")).PerformClick();((VNButton)Get(game,"translationButton")).PerformClick();Require(!((Control)Get(game,"englishText")).Visible&&((Control)Get(game,"translationLabel")).Text=="","both subtitles hidden");Capture(game,"no-subtitles");
   ((VNButton)Get(game,"englishButton")).PerformClick();((VNButton)Get(game,"translationButton")).PerformClick();
   foreach(var size in new[]{new Size(1280,780),new Size(800,500),new Size(1920,1080)}){
    game.Size=size;Application.DoEvents();
    for(int line=0;line<c.lines.Count;line++){
     Set(game,"index",line);Call(game,"UpdateLine");Application.DoEvents();
     var english=(SentenceView)Get(game,"englishText");var chinese=(Control)Get(game,"translationLabel");
     Require(!typeof(ScrollableControl).IsAssignableFrom(english.GetType()),"dialogue cannot create scrollbars");
     Require(english.Height>=english.WrappedLineCount(Math.Max(30,english.ClientSize.Width-22))*(int)Math.Ceiling(english.Font.Height*1.55)+12,"complete English lines fit: "+size+" line "+line);
     Require(chinese.Height>=TextRenderer.MeasureText(chinese.Text,chinese.Font,new Size(Math.Max(30,chinese.Width-5),int.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height+4,"Chinese translation fits");
     if(line==6)Capture(game,"long-subtitle-"+size.Width);
    }
   }
   game.Size=new Size(1280,780);Application.DoEvents();
   Set(game,"index",0);Call(game,"UpdateLine");Capture(game,"goddess");
   Set(game,"index",11);var transferSave=(Save)Get(game,"save");transferSave.heardLines.Add(c.id+"/line/11");transferSave.quizAnswers[c.id+"/q/11"]=c.questions.First(q=>q.afterLine==11).answer;Call(game,"Next");
   Require((string)Get(game,"page")=="goddess-transfer","next enters transfer after goddess dialogue");
   var filling=((Control)Get(game,"content")).Controls.OfType<OpeningCgCanvas>().Single();Require(filling.FillFrame,"transfer fills window without white letterbox edges");
   deadline=DateTime.UtcNow.AddSeconds(3);while(filling.FramesShown==0&&DateTime.UtcNow<deadline){Application.DoEvents();System.Threading.Thread.Sleep(20);}
   foreach(var size in new[]{new Size(800,500),new Size(1920,1080)}){game.Size=size;Application.DoEvents();Capture(game,"transfer-fill-"+size.Width);using(var bitmap=new Bitmap(filling.Width,filling.Height)){filling.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));Require(bitmap.GetPixel(0,bitmap.Height/2).ToArgb()!=Color.White.ToArgb()&&bitmap.GetPixel(bitmap.Width-1,bitmap.Height/2).ToArgb()!=Color.White.ToArgb(),"no exposed white side borders");}}
   game.Size=new Size(1280,780);
   deadline=DateTime.UtcNow.AddSeconds(9);while((string)Get(game,"page")=="goddess-transfer"&&DateTime.UtcNow<deadline){Application.DoEvents();System.Threading.Thread.Sleep(20);}
   Call(game,"StopAudio");Require((string)Get(game,"page")=="story"&&(int)Get(game,"index")==12&&transferSave.storyFlags.Contains(TavernStory.TransferFlag),"transfer completes into tavern");
   Call(game,"ShowStory");Require((string)Get(game,"page")=="story","transfer does not repeat on resume");
   transferSave.storyFlags.Remove(TavernStory.TransferFlag);Set(game,"index",11);Call(game,"ContinueAuto");Require((string)Get(game,"page")=="goddess-transfer","continuous playback enters transfer");
   var transferCanvas=((Control)Get(game,"content")).Controls.OfType<OpeningCgCanvas>().Single();transferCanvas.Completed();Call(game,"StopAudio");
   Set(game,"index",12);Call(game,"UpdateLine");Capture(game,"ruins");
   Set(game,"index",16);Call(game,"UpdateLine");transferSave.heardLines.Add(c.id+"/line/16");Call(game,"PlayAll");Require((long)Get(game,"storyEndMs")== (long)(c.lines[16].end*1000),"continuous audio stops before knight CG");Call(game,"StopAudio");Call(game,"Next");
   Require((string)Get(game,"page")=="knight-entrance","knight CG starts before intrusion dialogue");
   deadline=DateTime.UtcNow.AddSeconds(15);while((string)Get(game,"page")=="knight-entrance"&&DateTime.UtcNow<deadline){Application.DoEvents();System.Threading.Thread.Sleep(20);}
   Call(game,"StopAudio");Require((string)Get(game,"page")=="story"&&(int)Get(game,"index")==18&&transferSave.heardLines.Contains(c.id+"/line/17"),"CG narration heard and dialogue resumes with knight");Capture(game,"knight-after-cg");
   Set(game,"index",22);Call(game,"UpdateLine");Capture(game,"encounter");
   Set(game,"index",27);Call(game,"UpdateLine");var save=(Save)Get(game,"save");
   for(int i=0;i<=27;i++)save.heardLines.Add(c.id+"/line/"+i);
   Call(game,"PlayAll");Require((long)Get(game,"storyEndMs")== (long)(c.lines[27].end*1000),"continuous playback stops before tutorial");Call(game,"StopAudio");
   Call(game,"Next");var battleLoading=Get(game,"menuLoading");Require(battleLoading is MenuLoadingScreen,"story reuses existing battle loading animation");Capture(game,"battle-loading");
   Call(game,"Next");Require(Object.ReferenceEquals(battleLoading,Get(game,"menuLoading")),"repeated advance keeps one loading transition");
   deadline=DateTime.UtcNow.AddSeconds(10);while(Get(game,"menuLoading")!=null&&DateTime.UtcNow<deadline){Application.DoEvents();System.Threading.Thread.Sleep(20);}
   Require(Get(game,"menuLoading")==null&&(string)Get(game,"page")=="tavern-battle","loading completes into playable tutorial");Capture(game,"battle");
   Call(game,"UpdateRogueAudio");var mixer=(RogueAudioMixer)Get(game,"rogueAudio");Require((bool)Call(game,"RogueAudioPage")&&mixer!=null&&mixer.Active&&mixer.Error==null,"tavern battle activates music mixer");
   mixer.MasterVolume=100;mixer.EffectsVolume=85;Call(game,"PlayCastingSound",0);Call(game,"PlayRogueHit",false);Call(game,"PlayMonsterSound","spore/normal");Call(game,"PlayHandSound",true);Require(mixer.ActiveEffects>0,"battle effects reach active audio mixer");
   game.Size=new Size(800,500);Application.DoEvents();Capture(game,"battle-small");game.Size=new Size(1280,780);
   var tutorial=save.tavernBattle;var expedition=save.rogue.run;var preparation=save.rogue.prepSession;int coins=save.rogue.coins;
   Require(((Control)Get(game,"content")).Controls.OfType<RogueArena>().Single().Integrated,"same integrated combat interface");
   Require(CardBattle.Play(tutorial,0)&&CardBattle.Play(tutorial,0),"UI tutorial cards usable");Require((bool)Call(game,"EndTavernTurn",tutorial),"end turn enters question");Call(game,"SaveRogue");
   Require((string)Get(game,"page")=="tavern-battle"&&tutorial.cardBattle.answering,"UI rerender keeps independent battle");Capture(game,"battle-question");
   Call(game,"AnswerRogue",tutorial.question.answer,null);Require(tutorial.state=="feedback","UI answer routes to tutorial engine");
   string review=(string)Call(game,"RogueFeedbackText",tutorial);Require(review.Contains(tutorial.question.entry.word+" · "+tutorial.question.entry.meaning)&&review.Contains(tutorial.question.entry.example.Replace("{"+tutorial.question.entry.word+"}",tutorial.question.entry.word)),"tutorial feedback includes word meaning and English example");
   while(tutorial.state=="feedback"){Call(game,"ContinueTavernBattle");Call(game,"EndTavernTurn",tutorial);Call(game,"AnswerRogue",tutorial.question.answer,null);}
   Require(tutorial.state=="won"&&Object.ReferenceEquals(expedition,save.rogue.run)&&Object.ReferenceEquals(preparation,save.rogue.prepSession)&&save.rogue.coins==coins,"tutorial preserves expedition, preparation and economy");
   var finalArena=(RogueArena)Get(game,"rogueArena");Require(finalArena.Mode=="feedback"&&finalArena.AnimateHit&&finalArena.AnimationCompleted!=null,"final blow plays combat animation before feedback");
   deadline=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<deadline){Application.DoEvents();System.Threading.Thread.Sleep(20);}Capture(game,"battle-final-feedback");
   Call(game,"ContinueTavernBattle");Call(game,"StopAudio");Require((string)Get(game,"page")=="story"&&(int)Get(game,"index")==28&&save.tavernBattle==null,"UI victory continues prologue");
   save.tavernBattle=r;Call(game,"ShowTavernBattle");Capture(game,"battle-won");
   StoryRoutes.Flag(save,TavernStory.BattleFlag);save.tavernBattle=null;Set(game,"index",28);Call(game,"ShowStory");Capture(game,"after-battle");
   Set(game,"index",35);foreach(var q in c.questions)save.quizAnswers[c.id+"/q/"+q.afterLine]=(q.answer+1)%4;
   Call(game,"Complete");Require((string)Get(game,"page")=="tavern-ending"&&save.completed.Contains(c.id),"wrong answers still complete story");int xp=save.xp;Call(game,"Complete");Require(save.xp==xp,"completion reward once");Capture(game,"ending");
   var reloaded=Engine.Json.Deserialize<Save>(File.ReadAllText(Path.Combine(root,"save.json")));Require(reloaded.completed.Contains(c.id)&&reloaded.storyFlags.Contains(TavernStory.BattleFlag),"checkpoint persistence");
   game.Close();
  }
  Console.WriteLine("PASS: resources, independent entry/progression, free subtitle toggles, four listening segments, audio gate, real support effects, battle resume/win/loss, ending despite errors, reward deduplication and persistence.");
 }
}
