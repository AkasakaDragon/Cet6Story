using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

public partial class Game {
 PartyBattleCanvas partyCanvas;RogueRun partyRun;Action partySelectionRefresh;
 void RefreshPartySelection(RogueRun r){if(Object.ReferenceEquals(r,partyRun)&&partyCanvas!=null&&!partyCanvas.IsDisposed&&partyCanvas.Parent==content&&partySelectionRefresh!=null&&r.partyBattle.phase=="heroes")partySelectionRefresh();else RenderPartyBattle(r);}
 void RenderPartyBattle(RogueRun r){
  if(r.partyBattle!=null)PartyCombat.Ensure(r.partyBattle);var existing=partyCanvas;bool reuse=Object.ReferenceEquals(partyRun,r)&&existing!=null&&!existing.IsDisposed&&existing.Parent==content&&Object.ReferenceEquals(existing.Battle,r.partyBattle);
  if(reuse)existing.BeginInterfaceUpdate();
  try{RenderPartyBattleCore(r,reuse);}finally{if(reuse&&!existing.IsDisposed)existing.EndInterfaceUpdate();}
 }
 void RenderPartyBattleCore(RogueRun r,bool reuse){
  var maleIdle=HeroIdleAnimation.Load(Path.Combine(root,"assets","characters","animations","luchuan-idle.gif"));var femaleIdle=HeroIdleAnimation.Load(Path.Combine(root,"assets","characters","animations","aelia-idle.gif"));
  if(!reuse){if(battleLayoutCleanup!=null){battleLayoutCleanup();battleLayoutCleanup=null;}ClearPage();}else partyCanvas.ClearInterface();partyRun=r;bool tavern=Object.ReferenceEquals(r,save.tavernBattle);page=r.id==ToxicWoodland.Id?"toxic-battle":tavern?"tavern-battle":"rogue";rogueArena=null;
  if(r.partyBattle==null)r.partyBattle=PartyCombat.Create(r,save);var b=r.partyBattle;PartyCombat.Sync(r);
  b.selectedHero=Math.Max(0,Math.Min(b.heroes.Count-1,b.selectedHero));var hero=b.heroes[b.selectedHero];var skill=HeroSkills.Get(hero.id,b.selectedSkill);if(skill==null||!hero.skills.Contains(skill.Id)){b.selectedSkill=hero.skills[0];skill=HeroSkills.Get(hero.id,b.selectedSkill);}
  if(b.phase=="attack-question")skill=HeroSkills.Get(b.pendingHero,b.pendingSkill);
  var arena=reuse?partyCanvas:new PartyBattleCanvas{Dock=DockStyle.Fill,MaleIdle=maleIdle,FemaleIdle=femaleIdle,Battle=b,Scene=r.id==ToxicWoodland.Id?ToxicBattleBackground():TowerBackground(r),Male=BattleCharacter("hero-male"),Female=BattleCharacter("hero-female"),Enemy=TowerEnemy(r),Skill=skill,Banner=r.id==ToxicWoodland.Id?"剧毒林地 · 第"+save.toxicWoodland.steps+" / 10 节点":tavern?"破败驿站 · 初遇感染兽":"词域远征 · 角色技能战斗"};arena.ArtRoot=root;arena.MaleActions=CachedImage(Path.Combine(root,"assets","characters","actions","luchuan-actions-v2.png"));arena.FemaleActions=CachedImage(Path.Combine(root,"assets","characters","actions","aelia-actions.png"));partyCanvas=arena;if(!reuse){arena.PrepareHeroCastRows();content.Controls.Add(arena);}arena.Skill=skill;
  arena.HeroSelected=i=>{if(b.phase!="heroes"||b.heroes[i].id!=b.active)return;b.selectedHero=i;b.selectedSkill=null;b.target=null;RefreshPartySelection(r);};arena.TargetSelected=id=>{if(b.phase!="heroes")return;b.target=id;RefreshPartySelection(r);};
  var menu=PartyButton("菜单",()=>ShowPartyBattleMenu(r),85,38);menu.Location=new Point(15,14);arena.Controls.Add(menu);
  var skills=new List<HeroSkillButton>();for(int i=0;i<5;i++){var s=HeroSkills.Get(hero.id,hero.skills[i]);int cd=hero.cooldowns.ContainsKey(s.Id)?hero.cooldowns[s.Id]:0;int used=hero.used.ContainsKey(s.Id)?hero.used[s.Id]:0;var button=new HeroSkillButton{Art=HeroSkillImage(hero.id,s.Id),Skill=s,Slot=i+1,Remaining=cd,UsesLeft=s.Uses==0?-1:s.Uses-used,Active=s.Id==b.selectedSkill,CanRelease=PartyCombat.CanUse(b,hero,s),Enabled=b.phase=="heroes",AccessibleName=s.Name+" · "+s.Description,Size=new Size(70,78)};button.Click+=(sender,e)=>{b.selectedSkill=button.Skill.Id;b.target=null;RefreshPartySelection(r);};arena.Controls.Add(button);skills.Add(button);}
  var hint=new OutlinedLabel{ForeColor=GuildChrome.Ivory,Font=GameTheme.Body(10),BackColor=Color.Transparent};arena.Controls.Add(hint);
  var valid=PartyCombat.Targets(b,hero,skill);if(valid.Count>0&&!valid.Any(u=>u.id==b.target))b.target=valid[0].id;
  var target=PartyButton(valid.Count==0?"无可用目标":"目标 · "+valid.First(u=>u.id==b.target).name,()=>{if(b.phase!="heroes"||valid.Count==0)return;int i=valid.FindIndex(u=>u.id==b.target);b.target=valid[(i+1)%valid.Count].id;RefreshPartySelection(r);},170,34);target.AccessibleName="选择技能目标";target.Enabled=b.phase=="heroes"&&valid.Count>0;tips.SetToolTip(target,"点击切换目标，也可以直接点击场上的人物或怪物。");arena.Controls.Add(target);
  var cast=PartyButton("施放技能",()=>{if(PartyCombat.BeginSkill(b,b.selectedHero,b.selectedSkill,b.target)){NextPartyQuestion(r);PartyCombat.Sync(r);RenderPartyBattle(r);}},170,48);cast.Enabled=PartyCombat.CanUse(b,hero,skill)&&valid.Count>0;arena.Controls.Add(cast);
  var end=PartyButton("等待 / 跳过行动",()=>{PartyCombat.EndHeroes(b);PreparePartyQuestion(r);PartyCombat.Sync(r);RenderPartyBattle(r);},170,38);end.Enabled=b.phase=="heroes";arena.Controls.Add(end);
  hint.Text=PartyCombat.Unavailable(b,hero,skill)+"\n"+(hero.acted?"本回合已行动":"选择技能，再点击目标或使用右侧选择框。")+"\n"+(skill.Group?"群攻 · 一次答题":"每次行动 · 一次答题");
  var footerLog=new OutlinedLabel{ForeColor=GuildChrome.Muted,Font=GameTheme.Body(9),BackColor=Color.Transparent};arena.Controls.Add(footerLog);footerLog.Text="护盾先吸收伤害 · 答错攻击减半且无附加效果 · 防御答对伤害减半";
  var selector=new FlowLayoutPanel{BackColor=Color.Transparent,WrapContents=false,Height=34};arena.Controls.Add(selector);foreach(var h in b.heroes){int i=b.heroes.IndexOf(h);var choose=PartyButton(h.name+(h.acted?" · 已行动":""),()=>{b.selectedHero=i;b.selectedSkill=null;b.target=null;RefreshPartySelection(r);},125,30);choose.Active=b.selectedHero==i;choose.Enabled=b.phase=="heroes"&&h.id==b.active;selector.Controls.Add(choose);}
  var forward=PartyButton("前移",()=>{if(PartyCombat.MoveAction(b,-1)){PartyCombat.Sync(r);RenderPartyBattle(r);}},65,30);var backward=PartyButton("后移",()=>{if(PartyCombat.MoveAction(b,1)){PartyCombat.Sync(r);RenderPartyBattle(r);}},65,30);forward.Enabled=b.phase=="heroes"&&hero.id==b.active&&hero.rank>1;backward.Enabled=b.phase=="heroes"&&hero.id==b.active&&hero.rank<4;arena.Controls.Add(forward);arena.Controls.Add(backward);var tutorial=tavern?AddTavernBattleGuide(arena,b):null;
  partySelectionRefresh=()=>{
   hero=b.heroes[b.selectedHero];skill=HeroSkills.Get(hero.id,b.selectedSkill);
   if(skill==null||!hero.skills.Contains(skill.Id)){b.selectedSkill=hero.skills[0];skill=HeroSkills.Get(hero.id,b.selectedSkill);}arena.Skill=skill;
   for(int i=0;i<skills.Count;i++){
    var button=skills[i];var definition=HeroSkills.Get(hero.id,hero.skills[i]);button.Skill=definition;button.Art=HeroSkillImage(hero.id,definition.Id);
    button.Remaining=hero.cooldowns.ContainsKey(definition.Id)?hero.cooldowns[definition.Id]:0;
    int used=hero.used.ContainsKey(definition.Id)?hero.used[definition.Id]:0;button.UsesLeft=definition.Uses==0?-1:definition.Uses-used;
    button.CanRelease=PartyCombat.CanUse(b,hero,definition);button.Enabled=b.phase=="heroes";button.AccessibleName=definition.Name+" · "+definition.Description;

   }
   for(int i=0;i<selector.Controls.Count;i++){var choose=(VNButton)selector.Controls[i];choose.Active=b.selectedHero==i;choose.Invalidate();}
   valid=PartyCombat.Targets(b,hero,skill);if(valid.Count>0&&!valid.Any(u=>u.id==b.target))b.target=valid[0].id;
   foreach(var button in skills){button.Active=button.Skill.Id==b.selectedSkill;button.Invalidate();}
   target.Text=valid.Count==0?"无可用目标":"目标 · "+valid.First(u=>u.id==b.target).name;target.Enabled=valid.Count>0;
   cast.Enabled=PartyCombat.CanUse(b,hero,skill)&&valid.Count>0;
   hint.Text=PartyCombat.Unavailable(b,hero,skill)+"\n"+(hero.acted?"本回合已行动":"选择技能，再点击目标或使用右侧选择框。")+"\n"+(skill.Group?"群攻 · 一次答题":"每次行动 · 一次答题");
   if(tutorial!=null) tutorial.Invalidate();
   arena.Invalidate();
  };

  Action layout=()=>{
   if(!arena.HasBattleViewport)return;
   if(tutorial!=null){int guideWidth=Math.Min(460,Math.Max(320,arena.Width*42/100));tutorial.Bounds=new Rectangle((arena.Width-guideWidth)/2,64,guideWidth,tutorial.Collapsed?44:154);tutorial.Invalidate();}
   float scale=Math.Min(1.3f,arena.Width/1280f);int h=(int)(238*scale);arena.FooterHeight=h;int top=arena.Height-h;int logicalWidth=(int)(arena.Width/scale);
   int actionWidth=170,actionX=logicalWidth-actionWidth-22;int iconX=logicalWidth*48/100;int iconEnd=actionX-18;int size=Math.Min(78,(iconEnd-iconX-24)/5);
   Func<int,int,int,int,Rectangle> rect=(x,y,w,height)=>new Rectangle((int)(x*scale),top+(int)(y*scale),(int)(w*scale),(int)(height*scale));
   for(int i=0;i<5;i++){skills[i].Bounds=rect(iconX+i*(size+6),61,size,82);skills[i].Font=GameTheme.Body(9*scale);}
   selector.Bounds=rect(iconX,18,iconEnd-iconX,34);foreach(Control c in selector.Controls){c.Width=(int)(125*scale);c.Height=(int)(30*scale);c.Margin=new Padding((int)(3*scale));c.Font=GameTheme.Body(11*scale);}
   forward.Bounds=rect(192,207,65,26);backward.Bounds=rect(265,207,65,26);hint.Bounds=rect(iconX,153,iconEnd-iconX,43);hint.Font=GameTheme.Body(10*scale);
   target.Bounds=rect(actionX,38,actionWidth,30);target.Font=GameTheme.Body(11*scale);cast.Bounds=rect(actionX,84,actionWidth,48);end.Bounds=rect(actionX,145,actionWidth,38);cast.Font=GameTheme.Body(11*scale);end.Font=GameTheme.Body(11*scale);
   footerLog.Bounds=rect(iconX,204,logicalWidth-iconX-20,26);footerLog.Font=GameTheme.Body(9*scale);
   if(tutorial!=null) tutorial.Invalidate();
   arena.Invalidate();
  };EventHandler resize=(s,e)=>layout();arena.Resize+=resize;arena.InterfaceCleanup=()=>arena.Resize-=resize;layout();
  if(b.phase!="heroes")ShowPartyOverlay(r,arena);Persist();UpdateRogueAudio();
 }
 VNButton PartyButton(string text,Action click,int width,int height){var button=new VNButton{Text=text,Size=new Size(width,height),Font=GameTheme.Body(11),PixelStyle=true};button.Click+=(s,e)=>click();return button;}
 void NextPartyQuestion(RogueRun r){
  if(r.id==ToxicWoodland.Id){r.pool=null;r.question=ExpeditionVocabulary.Next(save.rogue,ExpeditionBank(),DateTime.Today);return;}
  var bank=r.pool!=null&&r.pool.Count>=4?r.pool:RogueBank();var distinct=bank.Where(w=>!String.IsNullOrEmpty(w.meaning)).GroupBy(w=>w.meaning).Select(g=>g.First()).ToList();
  int n=r.partyBattle.questions;var entry=distinct[(int)(((long)(r.seed&0x7fffffff)+n*17L)%distinct.Count)];var options=distinct.Where(x=>x.meaning!=entry.meaning).Skip(n%Math.Max(1,distinct.Count-4)).Take(3).Select(x=>x.meaning).ToList();if(options.Count<3)options=distinct.Where(x=>x.meaning!=entry.meaning).Take(3).Select(x=>x.meaning).ToList();int answer=n%4;options.Insert(answer,entry.meaning);r.question=new RogueQuestion{entry=entry,kind=0,options=options,answer=answer};RogueEngine.RecordWordAppearance(save.rogue,entry.word);
 }
 void PreparePartyQuestion(RogueRun r){if(r.partyBattle.phase=="defense-question"||r.partyBattle.phase=="attack-question")NextPartyQuestion(r);}
 TextBox partySpellingInput;
 void AnswerPartyQuestion(int selected,string spelling=null){
  var r=partyRun;if(r==null||r.question==null||r.question.answered||partyCanvas!=null&&partyCanvas.IsCasting)return;var b=r.partyBattle;if(r.question.kind==3&&String.IsNullOrWhiteSpace(spelling))return;
  bool hero=b.phase=="attack-question";bool close=hero?PartyBattleCanvas.IsMelee(b.pendingHero,b.pendingSkill):b.phase=="defense-question"&&PartyBattleCanvas.IsMonsterMelee(b.enemySkill);
  if(partyCanvas==null){FinishPartyQuestion(selected,spelling);return;}
  var canvas=partyCanvas;string actor=hero?b.pendingHero:b.active,target=hero?b.pendingTarget:b.defenseTargets.FirstOrDefault();
  Action impact=()=>{canvas.DiscardCastOverlays();FinishPartyQuestion(selected,spelling);};
  if(close)canvas.PlayMeleeAttackSequence(actor,target,hero?b.pendingSkill:b.enemySkill,!hero,ExpeditionVocabulary.Correct(r.question,selected,spelling),impact,null);
  else{if(hero)canvas.PlayCast(actor,b.pendingSkill,target,ExpeditionVocabulary.Correct(r.question,selected,spelling),null);else canvas.PlayMonsterCast(actor,b.defenseTargets,null);canvas.SetCastImpact(impact);}
  foreach(var panel in canvas.Controls.OfType<BattleGlassPanel>().ToList())canvas.HideCastOverlay(panel);
 }
 void FinishPartyQuestion(int selected,string spelling=null){
  var r=partyRun;if(r==null||r.partyBattle==null||r.question==null||r.question.answered)return;var b=r.partyBattle;if(b.phase!="attack-question"&&b.phase!="defense-question")return;
  var beforeHp=b.enemies.ToDictionary(u=>u.id,u=>u.hp);string enemyCaster=b.active;var enemyTargets=b.defenseTargets.ToList();bool heroCast=b.phase=="attack-question";string caster=b.pendingHero,castSkill=b.pendingSkill,castTarget=b.pendingTarget;bool correct=ExpeditionVocabulary.Correct(r.question,selected,spelling);r.question.answered=true;r.answered++;if(correct)r.correct++;else{r.mistakes++;CollectRogue(r.question.entry);}
  string learningFeedback=null;if(r.id==ToxicWoodland.Id)learningFeedback=ExpeditionVocabulary.Answer(save.rogue,r.question,correct,DateTime.Today);
  if(r.question.kind>=0){save.rogue.answers++;if(correct)save.rogue.correct++;RogueMemory memory;if(!save.rogue.memory.TryGetValue(r.question.entry.word,out memory)){memory=new RogueMemory();save.rogue.memory[r.question.entry.word]=memory;}memory.last=DateTime.Today.ToString("yyyy-MM-dd");if(correct){memory.correct++;memory.mask|=1<<r.question.kind;r.wrongWords.Remove(r.question.entry.word);}else{memory.wrong++;if(!r.wrongWords.Contains(r.question.entry.word))r.wrongWords.Add(r.question.entry.word);}TowerEngine.Learn(save.rogue,r,r.question,correct);}
  if(b.phase=="attack-question")PartyCombat.AttackAnswer(b,correct);else PartyCombat.DefenseAnswer(b,correct);if(learningFeedback!=null)b.log+="\n"+learningFeedback;PartyCombat.Sync(r);var activeCanvas=partyCanvas;if(activeCanvas!=null){activeCanvas.SetMonsterHits(b.enemies.Where(u=>beforeHp.ContainsKey(u.id)&&u.hp<beforeHp[u.id]).Select(u=>u.id));activeCanvas.ShowImpact(correct?"答对 · 完整效果":"答错 · 查看本次结算");}PlayRogueHit(!correct);Action complete=()=>{if(activeCanvas==null||!activeCanvas.IsDisposed&&Object.ReferenceEquals(partyRun,r))RenderPartyBattle(r);else Persist();};if(activeCanvas!=null&&activeCanvas.IsCasting)activeCanvas.AfterCurrentCast(complete);else complete();
 }
 void ContinuePartyBattle(){
  var r=partyRun;if(r==null||partyCanvas!=null&&partyCanvas.IsCasting)return;var b=r.partyBattle;
  if(b.outcome=="won"||b.outcome=="lost"){
   if(r.id==ToxicWoodland.Id){FinishToxicBattle();return;}
   if(Object.ReferenceEquals(r,save.tavernBattle)){if(b.outcome=="lost"){save.tavernBattle=null;ShowTavernBattle();}else ContinueTavernBattle();return;}
   r.partyRoster=b.heroes;r.state="feedback";r.cardBattle=null;r.partyBattle=null;RogueEngine.Continue(save.rogue);Persist();RenderRogue();return;
  }
  PartyCombat.Continue(b);PreparePartyQuestion(r);PartyCombat.Sync(r);RenderPartyBattle(r);
 }
 void ShowPartyOverlay(RogueRun r,PartyBattleCanvas arena){
  var b=r.partyBattle;bool question=b.phase=="attack-question"||b.phase=="defense-question";var panel=new BattleGlassPanel{Padding=new Padding(18),AccessibleName=question?"战斗答题":"战斗结算"};arena.Controls.Add(panel);
  var title=new OutlinedLabel{Font=GameTheme.Body(15),ForeColor=GameTheme.Gold,BackColor=Color.Transparent};panel.Controls.Add(title);
  var text=new OutlinedLabel{Font=GameTheme.Body(12),ForeColor=GuildChrome.Ivory,BackColor=Color.Transparent};panel.Controls.Add(text);
  var word=new OutlinedLabel{Font=GameTheme.Latin(21),ForeColor=GuildChrome.Ivory,BackColor=Color.Transparent};panel.Controls.Add(word);
  var verdict=new OutlinedLabel{Font=GameTheme.Body(13,FontStyle.Bold),BackColor=Color.Transparent,Visible=false};panel.Controls.Add(verdict);
  Panel logArea=null;OutlinedLabel translation=null;
  partySpellingInput=null;TextBox spellingInput=null;VNButton spellingSubmit=null;var choices=new List<VNButton>();VNButton next=null;
  if(question){if(r.question==null||r.question.answered)NextPartyQuestion(r);var q=r.question;
   title.Text=(b.phase=="attack-question"?"施放技能":"抵御攻击")+(q.kind==3?" · 拼写单词":q.kind==1?" · 选择英文":" · 选择词义");
   string who=b.phase=="attack-question"?b.heroes.First(x=>x.id==b.pendingHero).name+" · "+HeroSkills.Get(b.pendingHero,b.pendingSkill).Name:b.heroes.First(x=>x.id==b.defenseTargets[b.defenseCursor]).name+" · "+(b.enemyGroup?"群体攻击 · 一题保护全部目标":"单体攻击");
   text.Text=who+(b.phase=="defense-question"?" · "+PartyCombat.EnemyActionName(b.enemySkill):"");if(r.id==ToxicWoodland.Id)text.Text+=" · "+ExpeditionVocabulary.Status(save.rogue,q,DateTime.Today);word.Text=q.kind==1||q.kind==3?ExpeditionVocabulary.Meaning(q.entry):q.entry.word;if(q.kind==1||q.kind==3)word.Font=GameTheme.Body(17);if(q.kind==3){spellingInput=new TextBox{Text=q.spellingDraft??"",Font=GameTheme.Latin(18),BackColor=Color.FromArgb(25,53,55),ForeColor=GuildChrome.Ivory,BorderStyle=BorderStyle.FixedSingle,MaxLength=100,ImeMode=ImeMode.Disable,AccessibleName="输入英文拼写"};partySpellingInput=spellingInput;panel.Controls.Add(spellingInput);spellingInput.TextChanged+=(sender,e)=>q.spellingDraft=spellingInput.Text;spellingSubmit=PartyButton("确认拼写 · Enter",()=>AnswerPartyQuestion(-1,spellingInput.Text),200,44);panel.Controls.Add(spellingSubmit);spellingInput.KeyDown+=(sender,e)=>{if(e.KeyCode==Keys.Enter){e.Handled=e.SuppressKeyPress=true;AnswerPartyQuestion(-1,spellingInput.Text);}};arena.BeginInvoke((Action)(()=>{if(!spellingInput.IsDisposed&&Object.ReferenceEquals(partyRun,r)&&!q.answered)spellingInput.Focus();}));}else for(int i=0;i<4;i++){int answer=i;var choice=PartyButton(((char)('A'+i))+". "+q.options[i],()=>AnswerPartyQuestion(answer),200,44);choice.Font=GameTheme.Body(q.kind==1?14:12);panel.Controls.Add(choice);choices.Add(choice);}
  }else{title.Text=b.outcome=="won"?"战斗胜利":b.outcome=="lost"?"队伍倒下":b.phase=="enemy-feedback"?"怪物行动":"行动结算";text.Font=GameTheme.Body(14);text.Text=b.log;bool answered=b.lastAnswerCorrect.HasValue&&r.question!=null&&r.question.answered;verdict.Visible=answered;if(answered){verdict.Text=b.lastAnswerCorrect.Value?"回答正确":"回答错误";verdict.ForeColor=b.lastAnswerCorrect.Value?Color.FromArgb(112,225,155):Color.FromArgb(249,151,126);word.Font=GameTheme.Latin(16);word.Text=r.question.entry.word;translation=new OutlinedLabel{Text=r.question.entry.meaning,Font=GameTheme.Body(12),ForeColor=GuildChrome.Ivory};panel.Controls.Add(translation);}
   logArea=new Panel{AutoScroll=false,BackColor=Color.FromArgb(25,53,55)};panel.Controls.Add(logArea);panel.Controls.Remove(text);logArea.Controls.Add(text);
   next=PartyButton(b.outcome=="won"?"继续旅程":b.outcome=="lost"?(r.id==ToxicWoodland.Id?"结束远征":"重新挑战"):"继续 · Enter",ContinuePartyBattle,180,40);panel.Controls.Add(next);}
  RogueIcon hear=null,star=null;var entry=question?r.question.entry:b.lastAnswerCorrect.HasValue&&r.question!=null&&r.question.answered?r.question.entry:null;
  if(entry!=null){if(!question||r.question.kind==0||r.question.kind==3){hear=new RogueIcon{Kind="speaker",AccessibleName="朗读单词",Size=new Size(32,32)};hear.Click+=(s,e)=>SpeakWord(entry.word);tips.SetToolTip(hear,"朗读单词");panel.Controls.Add(hear);}star=new RogueIcon{Kind="star",AccessibleName="收藏单词",Size=new Size(32,32),Selected=save.words.Any(w=>w.text.Equals(entry.word,StringComparison.OrdinalIgnoreCase))};star.Click+=(s,e)=>ToggleRogueFavorite(entry,star);tips.SetToolTip(star,star.Selected?"已收藏 · 再次点击取消":"收藏到生词本");panel.Controls.Add(star);}

  Action layout=()=>{if(!arena.HasBattleViewport)return;int width=Math.Min(640,arena.Width-50);int logHeight=TextRenderer.MeasureText(text.Text,text.Font,new Size(width-64,10000),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height+8;int wordWidth=Math.Min(width-150,TextRenderer.MeasureText(word.Text,word.Font,Size.Empty,TextFormatFlags.NoPadding|TextFormatFlags.SingleLine).Width+6);int translationHeight=translation==null?0:TextRenderer.MeasureText(translation.Text,translation.Font,new Size(width-49,10000),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height+8;int wordHeight=Math.Max(question?48:32,TextRenderer.MeasureText(word.Text,word.Font,new Size(Math.Max(1,wordWidth-5),10000),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height+8);int height=Math.Min(question?285+wordHeight-48:Math.Max(260,logHeight+translationHeight+180),Math.Max(180,arena.Height-90));panel.Bounds=new Rectangle((arena.Width-width)/2,Math.Max(66,(arena.Height-arena.FooterHeight-height)/2),width,height);title.Bounds=new Rectangle(22,17,width-44,28);verdict.Bounds=new Rectangle(22,50,width-44,28);int wordY=question?91:height-65-translationHeight-wordHeight;word.Visible=entry!=null;word.Bounds=new Rectangle(22,wordY,wordWidth,wordHeight);if(hear!=null)hear.Location=new Point(word.Right+2,wordY+2);if(star!=null)star.Location=new Point(hear!=null?hear.Right+6:word.Right+2,wordY+2);
   if(question){text.Bounds=new Rectangle(22,52,width-44,36);if(spellingInput!=null){spellingInput.Bounds=new Rectangle(22,height-106,width-44,38);spellingSubmit.Bounds=new Rectangle(width-222,height-56,200,40);}for(int i=0;i<choices.Count;i++){choices[i].Bounds=new Rectangle(18+(i%2)*(width-36)/2,height-112+(i/2)*49,(width-44)/2,44);float optionSize=choices[i].Font.Size;while(optionSize>8&&TextRenderer.MeasureText(choices[i].Text,choices[i].Font,new Size(choices[i].Width-20,1000),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height>choices[i].Height-8){optionSize-=.5f;var previousFont=choices[i].Font;choices[i].Font=GameTheme.Body(optionSize);previousFont.Dispose();}}}else{if(translation!=null)translation.Bounds=new Rectangle(22,wordY+wordHeight,width-44,translationHeight);logArea.Bounds=new Rectangle(22,verdict.Visible?84:52,width-44,Math.Max(24,wordY-(verdict.Visible?84:52)-8));float logSize=text.Font.Size;while(logSize>8&&TextRenderer.MeasureText(text.Text,text.Font,new Size(width-64,10000),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height+8>logArea.Height){logSize-=0.5f;var oldFont=text.Font;text.Font=GameTheme.Body(logSize);oldFont.Dispose();}text.Bounds=new Rectangle(0,0,width-64,logArea.Height);next.Bounds=new Rectangle(width-205,height-53,180,40);}panel.BringToFront();};EventHandler resize=(s,e)=>layout();arena.Resize+=resize;var previousCleanup=arena.InterfaceCleanup;arena.InterfaceCleanup=()=>{arena.Resize-=resize;if(previousCleanup!=null)previousCleanup();};layout();
 }
 void ShowPartyBattleMenu(RogueRun r){
  using(var f=new PartyBattleMenuFrame{Text="战斗菜单",ClientSize=new Size(430,300),Font=Font}){
   var body=new Panel{Dock=DockStyle.Fill,BackColor=Color.FromArgb(25,53,55)};f.Controls.Add(body);
   var buttons=new[]{PartyButton("角色技能图鉴",()=>ShowHeroSkillBook(false),320,42),PartyButton("保存并返回主界面",()=>{Persist();f.Close();if(r.id==ToxicWoodland.Id)ToxicTransition(ShowMain);else ShowMain();},320,42),PartyButton("继续战斗",()=>f.Close(),320,42)};
   foreach(var button in buttons)body.Controls.Add(button);

  Action layout=()=>{int width=Math.Min(320,Math.Max(1,body.ClientSize.Width-32));int height=42,gap=12;int top=(body.ClientSize.Height-(height*buttons.Length+gap*(buttons.Length-1)))/2;for(int i=0;i<buttons.Length;i++)buttons[i].Bounds=new Rectangle((body.ClientSize.Width-width)/2,top+i*(height+gap),width,height);};
   body.Resize+=(sender,e)=>layout();layout();f.ShowDialog(this);
  }
 }
 void ShowHeroSkillBook(bool editable){
  using(var f=new GuildWordDialog{QuestStyle=true,Text="角色技能 · 每人携带五个",Width=1080,Height=790,Padding=new Padding(30,62,30,30),BackColor=Color.FromArgb(6,29,29),StartPosition=FormStartPosition.CenterParent}){
   var divider=typeof(PixelFrame).GetField("ShowTitleDivider");if(divider!=null)divider.SetValue(f,false);
   var view=new HeroLoadoutView{Dock=DockStyle.Fill,SaveData=save,Editable=editable,SkillArt=HeroSkillImage,Portrait=id=>BattleCharacter(id=="aelia"?"hero-female":"hero-male")};view.Initialize();f.Controls.Add(view);
   f.FormClosing+=(sender,e)=>{if(editable&&!view.Complete){e.Cancel=true;view.Notice="请为艾莉娅和陆川各携带五个技能后再关闭。";view.Invalidate();}else if(editable){if(save.heroLoadouts==null)save.heroLoadouts=new Dictionary<string,List<string>>();foreach(var item in view.Loadouts)save.heroLoadouts[item.Key]=item.Value.ToList();}};
   f.ShowDialog(this);if(editable)Persist();
  }
 }
 void InitPartyBattleKeys(){KeyDown+=(sender,e)=>{if(woodlandChanging||e.Handled||e.Control||e.Alt||partyCanvas==null||partyCanvas.IsDisposed||partyCanvas.Parent!=content||partyRun==null)return;var b=partyRun.partyBattle;if(b==null)return;
  if((b.phase=="attack-question"||b.phase=="defense-question")&&partyRun.question!=null&&partyRun.question.kind==3){if(e.KeyCode==Keys.Enter){e.Handled=e.SuppressKeyPress=true;if(partySpellingInput!=null&&!partySpellingInput.IsDisposed)AnswerPartyQuestion(-1,partySpellingInput.Text);}return;}
  if((b.phase=="attack-question"||b.phase=="defense-question")&&e.KeyCode>=Keys.D1&&e.KeyCode<=Keys.D4){AnswerPartyQuestion((int)e.KeyCode-(int)Keys.D1);e.Handled=e.SuppressKeyPress=true;}
  else if((b.phase=="feedback"||b.phase=="enemy-feedback")&&e.KeyCode==Keys.Enter){ContinuePartyBattle();e.Handled=e.SuppressKeyPress=true;}
  else if(b.phase=="heroes"&&e.KeyCode>=Keys.D1&&e.KeyCode<=Keys.D5){var h=b.heroes[b.selectedHero];b.selectedSkill=h.skills[(int)e.KeyCode-(int)Keys.D1];b.target=null;RefreshPartySelection(partyRun);e.Handled=e.SuppressKeyPress=true;}
 };}
}
