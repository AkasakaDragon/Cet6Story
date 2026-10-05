using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

public static class TavernStory {
 public const string Id="tavern-prologue";
 public const string BattleFlag="tavern-first-battle";
 public static bool Is(Chapter c){return c!=null&&c.id==Id;}
 public static RogueRun NewBattle(RogueProfile profile){
  var r=new RogueRun{id="tavern-tutorial",seed=60105,state="combat",enemy="combat",theme=1,hp=70,maxHp=70,attack=20,enemyHp=60,enemyMax=60,pool=new List<RogueEntry>()};
  CardBattle.Start(r,profile);r.cardBattle.monster="回响灯灵";
  r.cardBattle.deck=new List<string>{"barrier","guide","barrier","guide"};r.cardBattle.hand=new List<string>{"barrier","guide"};r.cardBattle.draw=new List<string>{"barrier","guide"};
  SetQuestion(r);return r;
 }
 public static void SetQuestion(RogueRun r){
  string[] words={"protect","strengthen","restore"};string[] meanings={"保护","增强","修复"};int i=r.answered%3;
  r.question=new RogueQuestion{entry=new RogueEntry{word=words[i],meaning=meanings[i],example="We must {"+words[i]+"} the seal.",exampleZh="我们必须维护封印。"},kind=0,options=new List<string>{"保护","增强","修复","放弃"},answer=i};
 }
 public static bool Answer(RogueRun r,int selected){
  if(r==null||r.state!="combat"||r.question.answered)return false;
  if(r.cardBattle.turn==1&&(!r.cardBattle.discard.Contains("barrier")||!r.cardBattle.discard.Contains("guide")))return false;
  if(!CardBattle.EndTurn(r))return false;bool correct=selected==r.question.answer;r.question.answered=true;r.answered++;
  int hit=correct?CardBattle.Attack(r,r.attack,true):0;int received=CardBattle.Enemy(r,correct,correct,hit);
  r.feedback=(correct?"契约完成 · 艾莉娅斩击造成 "+hit+" 伤害":"契约未同步 · 本题正确释义："+r.question.entry.meaning)+"\n受到 "+received+" 伤害 · 护盾抵挡 "+r.cardBattle.lastBlocked;
  r.state=r.enemyHp<=0?"won":r.hp<=0?"lost":"feedback";return true;
 }
 public static void NextTurn(RogueRun r){
  // A small teaching deck: unused cards go to discard between rounds.
  r.cardBattle.discard.AddRange(r.cardBattle.hand);r.cardBattle.hand.Clear();
  r.state="combat";CardBattle.Begin(r,false);SetQuestion(r);
 }
}

public partial class Game {
 void EnterTavern(){
  var chapter=chapters.FirstOrDefault(TavernStory.Is);if(chapter==null){GameMessage.Show(this,"新序幕资源尚未加载，请保留完整 chapters 文件夹。","封印酒馆");return;}
  current=chapter;index=save.positions.ContainsKey(chapter.id)?Math.Max(0,Math.Min(chapter.lines.Count-1,save.positions[chapter.id])):0;
  if(!save.positions.ContainsKey(chapter.id)){englishVisible=save.english=true;translating=save.chinese=true;}
  ShowStory();BeginInvoke((Action)(()=>{if(page=="story"&&TavernStory.Is(current))PlayCurrent();}));
 }
 void ShowTavernHub(){
  ClearPage();page="tavern-hub";LoadStage(current,false);stage.Novel=true;stage.Art=CachedImage(Path.Combine(root,"chapters","art","tavern","tavern-ruins.png"));stage.Snap();
  var panel=new FlowLayoutPanel{FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,BackColor=Color.FromArgb(235,18,29,33),Size=new Size(460,420),Padding=new Padding(20)};stage.Controls.Add(panel);
  panel.Controls.Add(Lab("封印酒馆 · 新主线",23,Gold));panel.Controls.Add(Lab("序幕：第一盏灯",17));panel.Controls.Add(Lab("逐句英文配音 · 四道分段理解题\n中英文字幕分别开关；点击英文单词可查词。\n首次观看也可开中文，答错不影响剧情。",11));
  panel.Controls.Add(Btn("继续序幕",EnterTavern,true));panel.Controls.Add(Btn("从头重看序幕",()=>{current=chapters.First(TavernStory.Is);ResetSection();save.tavernBattle=null;save.storyFlags.Remove(TavernStory.BattleFlag);ShowStory();PlayCurrent();}));
  panel.Controls.Add(Lab("第一章：今天开始营业 · 后续开放",12,Muted));panel.Controls.Add(Btn("返回主界面",ShowMain));
  Action place=()=>{panel.Location=new Point(Math.Max(16,(stage.Width-panel.Width)/2),Math.Max(16,(stage.Height-panel.Height)/2));panel.Height=Math.Min(420,stage.Height-32);};stage.Resize+=(s,e)=>place();place();
 }
 bool TryTavernBattle(){
  if(!TavernStory.Is(current)||index!=35||save.storyFlags.Contains(TavernStory.BattleFlag))return false;
  if(!LineHeard()){PlayCurrent();return true;}ShowTavernBattle();return true;
 }
 void ShowTavernBattle(){
  ClearPage();page="tavern-battle";if(save.tavernBattle==null)save.tavernBattle=TavernStory.NewBattle(save.rogue);var r=save.tavernBattle;Persist();
  LoadStage(current,false);stage.Novel=true;stage.Art=CachedImage(Path.Combine(root,"chapters","art","tavern","tavern-defense.png"));stage.Snap();
  var hud=new OutlinedLabel{Dock=DockStyle.Top,Height=106,Font=GameTheme.Body(11),ForeColor=Color.White,Text="圣骑士艾莉娅  "+r.hp+" / "+r.maxHp+"    回响灯灵  "+r.enemyHp+" / "+r.enemyMax+"\n能量 "+r.cardBattle.energy+" / 3 · 敌人预告："+CardBattle.Intent(r),Padding=new Padding(22,14,22,0)};stage.Controls.Add(hud);
  var box=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=350,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(18,6,18,4),BackColor=Color.FromArgb(245,18,26,35)};stage.Controls.Add(box);
  stage.Resize+=(s,e)=>{if(!box.IsDisposed)box.Height=Math.Min(350,Math.Max(280,stage.Height-110));};
  if(r.state=="won"){
   box.Controls.Add(Lab("支援成功 · 艾莉娅击败了魔物",20,Gold));box.Controls.Add(Lab(r.feedback,12));box.Controls.Add(Btn("继续序幕",()=>{StoryRoutes.Flag(save,TavernStory.BattleFlag);save.tavernBattle=null;index=36;Persist();ShowStory();PlayCurrent();},true));
  }else if(r.state=="lost"){
   box.Controls.Add(Lab("暂时撤退 · 可以重新尝试",20,Gold));box.Controls.Add(Lab("答错不会改变男主设定或角色关系。查看释义后再试一次。",12));box.Controls.Add(Btn("重试教学战",()=>{save.tavernBattle=null;ShowTavernBattle();},true));
  }else if(r.state=="feedback"){
   box.Controls.Add(Lab(r.feedback,17,Gold));box.Controls.Add(Btn("下一回合",()=>{TavernStory.NextTurn(r);ShowTavernBattle();},true));
  }else{
   box.Controls.Add(Lab(r.cardBattle.turn==1?"先点击「守护屏障」和「战术指引」，再作答为艾莉娅提供支援。":"按需出牌，再作答发动攻击。教学战每回合抽两张，未用牌在回合结束时弃置。",12,Gold));
   var row=new FlowLayoutPanel{Height=132,Width=700,WrapContents=false};box.Controls.Add(row);
   for(int i=0;i<r.cardBattle.hand.Count;i++){int slot=i;var card=CardBattle.Get(r.cardBattle.hand[i]);var view=new SupportCardView{Card=card,Atlas=SupportAtlas(),FrameAtlas=CardFrameAtlas(),Size=new Size(84,126),Playable=card.cost<=r.cardBattle.energy,AccessibleName=card.name};view.Click+=(s,e)=>{if(CardBattle.Play(r,slot))ShowTavernBattle();};tips.SetToolTip(view,card.name+" · "+card.text);row.Controls.Add(view);}
   box.Controls.Add(Lab("“"+r.question.entry.word+"” 的含义？",14));
   var task=new FlowLayoutPanel{AutoSize=true,WrapContents=false,MaximumSize=new Size(740,0)};box.Controls.Add(task);
   for(int i=0;i<r.question.options.Count;i++){int answer=i;task.Controls.Add(Btn(r.question.options[i],()=>{if(!TavernStory.Answer(r,answer)){GameMessage.Show(this,"先使用守护屏障与战术指引，再回答本题。","卡牌支援教学");return;}if(answer!=r.question.answer)CollectRogue(r.question.entry);ShowTavernBattle();}));}
  }
  box.Controls.Add(Btn("保存并返回主界面",ShowMain));
 }
 void CompleteTavern(){
  if(Attempt().finished==0)FinishSectionTiming();int correct=current.questions.Count(q=>save.quizAnswers.ContainsKey(InlineKey(q))&&save.quizAnswers[InlineKey(q)]==q.answer);
  int stars=SectionRules.Stars(correct,current.questions.Count,SectionSeconds()<=SectionLimit());save.sectionStars[current.id]=Math.Max(stars,save.sectionStars.ContainsKey(current.id)?save.sectionStars[current.id]:0);
  bool fresh=!save.completed.Contains(current.id);if(fresh){save.completed.Add(current.id);save.xp+=60;}Persist();
  ClearPage();page="tavern-ending";var p=PageFlow();p.Controls.Add(Lab("序幕完成 · 第一盏灯",26,Gold));p.Controls.Add(Lab(current.ending,13));p.Controls.Add(Lab("听力 "+correct+" / 4 · "+new string('★',stars)+new string('☆',3-stars)+(fresh?" · 首次完成 +60 XP":" · 完成奖励已领取"),16));p.Controls.Add(Lab("听力成绩用于复习反馈，答错也能完成序幕。酒馆经营与下一章尚未开放。",11,Muted));
  foreach(var q in current.questions){int selected;string answer=save.quizAnswers.TryGetValue(InlineKey(q),out selected)?((char)('A'+selected)).ToString():"未作答";var text=Lab(q.prompt+"\n你的答案 "+answer+" · 正确答案 "+(char)('A'+q.answer)+"\n"+q.explanation,12);text.MaximumSize=new Size(900,0);p.Controls.Add(text);}
  p.Controls.Add(Btn("整节连续重听",()=>{Attempt().review=true;index=0;ShowStory();ReviewSection();},true));p.Controls.Add(Btn("新主线章节",ShowTavernHub));
 }
}
