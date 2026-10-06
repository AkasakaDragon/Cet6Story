using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

public static class TavernStory {
 public const string Id="tavern-prologue";
 public const string BattleFlag="tavern-first-battle";
 public const string OpeningFlag="tavern-opening-seen";
 public const string TransferFlag="tavern-goddess-transfer-seen";
 public const int TransferLine=12;
 public const string EntranceFlag="tavern-knight-entrance-seen";
 public const int EntranceLine=17;
 public const int BattleLine=24;
 public const string ScriptFlag="tavern-script-without-dorm-v2";
 public const string WaystationFlag="waystation-prologue-v3";
 static void MigrateWaystation(Save s){
  if(s==null||s.storyFlags==null||s.storyFlags.Contains(WaystationFlag))return;
  if(s.positions.ContainsKey(Id)&&s.positions[Id]>=12)s.positions[Id]=12;
  s.heardLines.RemoveAll(k=>{int n;return k.StartsWith(Id+"/line/")&&int.TryParse(k.Substring((Id+"/line/").Length),out n)&&n>=12;});
  SectionAttempt a;if(s.sectionAttempts.TryGetValue(Id,out a)&&a.replayAfterLine>=12)a.replayAfterLine=-1;
  s.storyFlags.Add(WaystationFlag);
 }
 public static void Migrate(Save s){
  if(s==null||s.storyFlags==null)return;
  if(s.storyFlags.Contains(ScriptFlag)){MigrateWaystation(s);return;}
  if(s.positions.ContainsKey(Id))s.positions[Id]=Math.Max(0,s.positions[Id]-8);
  string prefix=Id+"/line/";
  var heard=s.heardLines.Where(k=>k.StartsWith(prefix)).ToList();s.heardLines.RemoveAll(k=>k.StartsWith(prefix));
  foreach(var k in heard){int old;if(int.TryParse(k.Substring(prefix.Length),out old)&&old>=8)s.heardLines.Add(prefix+(old-8));}
  Shift(s.quizAnswers);Shift(s.answerStarted);Shift(s.answerTimely);
  SectionAttempt attempt;if(s.sectionAttempts.TryGetValue(Id,out attempt)&&attempt.replayAfterLine>=0)attempt.replayAfterLine=attempt.replayAfterLine>=8?attempt.replayAfterLine-8:-1;
  s.storyFlags.Add(ScriptFlag);
  MigrateWaystation(s);
 }
 static void Shift<T>(Dictionary<string,T> values){
  string prefix=Id+"/q/";var old=values.Where(k=>k.Key.StartsWith(prefix)).ToList();
  foreach(var pair in old)values.Remove(pair.Key);
  foreach(var pair in old){int line;if(int.TryParse(pair.Key.Substring(prefix.Length),out line)&&line>=8)values[prefix+(line-8)]=pair.Value;}
 }
 public static bool Is(Chapter c){return c!=null&&c.id==Id;}
 public static RogueRun NewBattle(RogueProfile profile){
  var r=new RogueRun{id="tavern-tutorial",seed=60105,state="combat",enemy="combat",theme=0,hp=70,maxHp=70,attack=20,enemyHp=60,enemyMax=60,pool=new List<RogueEntry>()};
  CardBattle.Start(r,profile);r.cardBattle.monster="荆棘孢子兽";
  r.cardBattle.deck=new List<string>{"barrier","guide","barrier","guide"};r.cardBattle.hand=new List<string>{"barrier","guide"};r.cardBattle.draw=new List<string>{"barrier","guide"};
  SetQuestion(r);return r;
 }
 public static void SetQuestion(RogueRun r){
  string[] words={"protect","strengthen","restore"};string[] meanings={"保护","增强","修复"};int i=r.answered%3;
  string[] translations={"我们必须保护封印。","我们必须加固封印。","我们必须修复封印。"};
  r.question=new RogueQuestion{entry=new RogueEntry{word=words[i],meaning=meanings[i],example="We must {"+words[i]+"} the seal.",exampleZh=translations[i]},kind=0,options=new List<string>{"保护","增强","修复","放弃"},answer=i};
 }
 public static bool Answer(RogueRun r,int selected){
  if(r==null||r.state!="combat"||r.question.answered)return false;
  if(r.cardBattle.turn==1&&(!r.cardBattle.discard.Contains("barrier")||!r.cardBattle.discard.Contains("guide")))return false;
  if(!r.cardBattle.answering&&!CardBattle.EndTurn(r))return false;bool correct=selected==r.question.answer;r.question.answered=true;r.answered++;
  int hit=correct?CardBattle.Attack(r,r.attack,true):0;int received=CardBattle.Enemy(r,correct,correct,hit);
  r.lastDamage=hit;r.lastReceived=received;
  r.feedback=(correct?"契约完成 · 艾莉娅斩击造成 "+hit+" 伤害":"契约未同步 · 本题正确释义："+r.question.entry.meaning)+"\n受到 "+received+" 伤害 · 护盾抵挡 "+r.cardBattle.lastBlocked;
  r.feedback+="\n"+r.question.entry.word+" · "+r.question.entry.meaning+"\n"+r.question.entry.example.Replace("{"+r.question.entry.word+"}",r.question.entry.word);
  r.state=r.enemyHp<=0?"won":r.hp<=0?"lost":"feedback";return true;
 }
 public static void NextTurn(RogueRun r){
  r.state="combat";CardBattle.Begin(r,false);SetQuestion(r);
 }
}

public partial class Game {
 void PreloadTransferAssets(){
  var paths=current.actors.Select(a=>Engine.SafePath(folders[current.id],a.image)).Concat(new[]{Engine.SafePath(folders[current.id],current.lines[TavernStory.TransferLine].scene)}).Where(p=>!imageCache.ContainsKey(p)).Distinct().ToArray();
  string audio=Engine.SafePath(folders[current.id],current.audio);double speed=save.storySpeed;int volume=StorySoundVolume();
  System.Threading.ThreadPool.QueueUserWorkItem(_=>{
   var loaded=new Dictionary<string,Image>();
   try{foreach(var path in paths)using(var source=Image.FromFile(path))loaded[path]=new Bitmap(source);
    AudioVolume.Prepare(StoryAudioSpeed.Prepare(audio,speed,root),volume,root);
    if(IsDisposed||!IsHandleCreated){foreach(var image in loaded.Values)image.Dispose();return;}
    BeginInvoke((Action)(()=>{if(IsDisposed){foreach(var image in loaded.Values)image.Dispose();return;}foreach(var pair in loaded){if(imageCache.ContainsKey(pair.Key))pair.Value.Dispose();else imageCache[pair.Key]=pair.Value;}}));
   }catch{foreach(var image in loaded.Values)image.Dispose();}
  });
 }
 void QueueRepairReveal(){
  if(!TavernStory.Is(current)||index<25)return;
  var destination=stage;int line=index;
  BeginInvoke((Action)(()=>{if(page!="story"||stage!=destination||destination.IsDisposed||index!=line)return;
   foreach(var old in content.Controls.OfType<WhiteSceneReveal>().ToList())old.Dispose();
   var reveal=new WhiteSceneReveal{Dock=DockStyle.Fill,DurationMs=line==40?500:300,FadeColor=Color.Black};reveal.CaptureScene(destination);content.Controls.Add(reveal);reveal.BringToFront();reveal.Start();
  }));
 }
 bool TryTavernTransition(){return TryTavernTransfer();}
 bool TryKnightEntrance(){if(!TavernStory.Is(current)||index!=TavernStory.EntranceLine||save.storyFlags.Contains(TavernStory.EntranceFlag))return false;ShowKnightEntrance();return true;}
 void ShowKnightEntrance(){
  ClearPage();page="knight-entrance";save.positions[current.id]=index;Persist();bool finished=false;
  Action finish=()=>{if(finished||page!="knight-entrance")return;finished=true;StoryRoutes.Flag(save,TavernStory.EntranceFlag);string heard=current.id+"/line/"+TavernStory.EntranceLine;if(!save.heardLines.Contains(heard))save.heardLines.Add(heard);index=TavernStory.EntranceLine+1;save.positions[current.id]=index;Persist();ShowStory();PlayCurrent();};
  try{
   var folder=Path.Combine(root,"assets","opening","tavern");var canvas=new OpeningCgCanvas(Path.Combine(root,"tools","ffmpeg","ffmpeg.exe"),Path.Combine(folder,"knight-entrance.mp4"),AudioDevicePath.Relative(AudioVolume.Prepare(Path.Combine(folder,"knight-entrance.wav"),EffectSoundVolume(),root)),12){Dock=DockStyle.Fill,FillFrame=true};content.Controls.Add(canvas);canvas.Completed=finish;canvas.Failed=message=>{if(page=="knight-entrance"){GameMessage.Show(this,"骑士闯入 CG 未能播放："+message,"播放提示");finish();}};canvas.Start();
  }catch(Exception ex){GameMessage.Show(this,"骑士闯入 CG 未能播放："+ex.Message,"播放提示");finish();}
 }
 bool TryTavernTransfer(){if(!TavernStory.Is(current)||index!=TavernStory.TransferLine||save.storyFlags.Contains(TavernStory.TransferFlag))return false;ShowGoddessTransfer();return true;}
 void ShowGoddessTransfer(){
  ClearPage();page="goddess-transfer";save.positions[current.id]=index;Persist();bool finished=false;
  PreloadTransferAssets();
  Action finish=()=>{if(finished||page!="goddess-transfer")return;finished=true;StoryRoutes.Flag(save,TavernStory.TransferFlag);index=TavernStory.TransferLine;save.positions[current.id]=index;Persist();ShowStory();var reveal=new WhiteSceneReveal{Dock=DockStyle.Fill};content.PerformLayout();stage.PerformLayout();reveal.CaptureScene(stage);reveal.Completed=()=>{if(page=="story"&&index==TavernStory.TransferLine)PlayCurrent();};content.Controls.Add(reveal);reveal.BringToFront();reveal.Start();};
  try{
   var folder=Path.Combine(root,"assets","opening","tavern");var canvas=new OpeningCgCanvas(Path.Combine(root,"tools","ffmpeg","ffmpeg.exe"),Path.Combine(folder,"goddess-transfer.mp4"),AudioDevicePath.Relative(AudioVolume.Prepare(Path.Combine(folder,"goddess-transfer.wav"),EffectSoundVolume(),root)),6){Dock=DockStyle.Fill,FillFrame=true};content.Controls.Add(canvas);canvas.Completed=finish;canvas.Failed=message=>{if(page=="goddess-transfer"){GameMessage.Show(this,"传送 CG 未能播放："+message,"播放提示");finish();}};canvas.Start();
  }catch(Exception ex){GameMessage.Show(this,"传送 CG 未能播放："+ex.Message,"播放提示");finish();}
 }
 void ShowTavernOpening(){
  ClearPage();page="tavern-opening";
  bool finished=false;
  Action finish=()=>{if(finished||page!="tavern-opening")return;finished=true;StoryRoutes.Flag(save,TavernStory.OpeningFlag);index=0;save.positions[current.id]=index;save.lastChapter=current.id;Persist();ShowStory();PlayCurrent();};
  try{
   var folder=Path.Combine(root,"assets","opening","tavern");
   var canvas=new OpeningCgCanvas(Path.Combine(root,"tools","ffmpeg","ffmpeg.exe"),Path.Combine(folder,"opening.mp4"),AudioDevicePath.Relative(AudioVolume.Prepare(Path.Combine(folder,"opening.wav"),EffectSoundVolume(),root)),18){Dock=DockStyle.Fill};content.Controls.Add(canvas);
   canvas.Completed=finish;canvas.Failed=message=>{if(page!="tavern-opening")return;GameMessage.Show(this,"序幕 CG 未能播放："+message,"播放提示");finish();};
   var skip=new VNButton{Text="跳过 CG",PixelStyle=true,Size=new Size(125,46),Font=GameTheme.Body(12),AccessibleName="跳过酒馆序幕 CG"};canvas.Controls.Add(skip);
   Action place=()=>skip.Location=new Point(Math.Max(8,canvas.Width-skip.Width-20),20);canvas.Resize+=(s,e)=>place();place();skip.Click+=(s,e)=>finish();tips.SetToolTip(skip,"跳过后直接进入女神对话");canvas.Start();
  }catch(Exception ex){GameMessage.Show(this,"序幕 CG 未能播放："+ex.Message,"播放提示");finish();}
 }
 void EnterTavern(){
  TavernStory.Migrate(save);
  var chapter=chapters.FirstOrDefault(TavernStory.Is);if(chapter==null){GameMessage.Show(this,"新序幕资源尚未加载，请保留完整 chapters 文件夹。","封印酒馆");return;}
  current=chapter;index=save.positions.ContainsKey(chapter.id)?Math.Max(0,Math.Min(chapter.lines.Count-1,save.positions[chapter.id])):0;
  if(index==TavernStory.BattleLine&&save.storyFlags.Contains(TavernStory.BattleFlag)&&chapter.lines.Count>TavernStory.BattleLine+1)index=TavernStory.BattleLine+1;
  if(!save.positions.ContainsKey(chapter.id)){englishVisible=save.english=true;translating=save.chinese=true;}
  save.hasGame=true;save.lastChapter=chapter.id;save.positions[chapter.id]=index;save.openingCgPending=false;Persist();
  if(save.tavernBattle!=null&&!save.storyFlags.Contains(TavernStory.BattleFlag)){ShowTavernBattle();return;}
  ShowStory();BeginInvoke((Action)(()=>{if(page=="story"&&TavernStory.Is(current))PlayCurrent();}));
 }
 bool TryTavernBattle(){
  if(!TavernStory.Is(current)||index!=TavernStory.BattleLine||save.storyFlags.Contains(TavernStory.BattleFlag))return false;
  if(menuLoading!=null&&!menuLoading.IsDisposed)return true;
  StopAudio();NavigateMenu(ShowTavernBattle,"战斗",false);return true;
 }
 void ShowTavernBattle(){
  if(save.tavernBattle==null)save.tavernBattle=TavernStory.NewBattle(save.rogue);
  var r=save.tavernBattle;r.cardBattle.monster="荆棘孢子兽";r.mode="破败驿站 · 角色技能战斗";r.theme=0;Persist();RenderFullBattle(r);
 }
 bool EndTavernTurn(RogueRun r){
  if(r.cardBattle.turn==1&&(!r.cardBattle.discard.Contains("barrier")||!r.cardBattle.discard.Contains("guide"))){GameMessage.Show(this,"先使用守护屏障与战术指引，再结束回合。","卡牌支援教学");return false;}
  return CardBattle.EndTurn(r);
 }
 void ContinueTavernBattle(){
  var r=save.tavernBattle;if(r.state=="won"){
   foreach(var line in current.lines.Skip(TavernStory.BattleLine+1))if(!String.IsNullOrEmpty(line.scene))CachedImage(Engine.SafePath(folders[current.id],line.scene));
   StoryRoutes.Flag(save,TavernStory.BattleFlag);save.tavernBattle=null;index=TavernStory.BattleLine+1;save.positions[current.id]=index;Persist();ShowStory();PlayCurrent();}
  else if(r.state=="lost"){save.tavernBattle=null;ShowTavernBattle();}
  else if(r.state=="feedback"){TavernStory.NextTurn(r);ShowTavernBattle();}
 }
 void CompleteTavern(){
  StoryRoutes.Flag(save,"waystation-basic-repair-complete");
  if(Attempt().finished==0)FinishSectionTiming();int correct=current.questions.Count(q=>save.quizAnswers.ContainsKey(InlineKey(q))&&save.quizAnswers[InlineKey(q)]==q.answer);
  int stars=SectionRules.Stars(correct,current.questions.Count,SectionSeconds()<=SectionLimit());save.sectionStars[current.id]=Math.Max(stars,save.sectionStars.ContainsKey(current.id)?save.sectionStars[current.id]:0);
  bool fresh=!save.completed.Contains(current.id);if(fresh){save.completed.Add(current.id);save.xp+=60;}Persist();
  ClearPage();page="tavern-ending";var ending=new PrologueCompletion{Dock=DockStyle.Fill,BackgroundArt=CachedImage(Path.Combine(root,"assets","menu","prologue-completion-town.png")),Chapter=current,Save=save,Correct=correct,Stars=stars,FirstCompletion=fresh,ReturnHome=ShowMain,Chapters=ShowTavernHub,Replay=()=>{Attempt().review=true;index=0;ShowStory();ReviewSection();}};content.Controls.Add(ending);
 }
}
