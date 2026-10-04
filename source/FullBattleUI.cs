using System;using System.Linq;using System.Drawing;using System.Windows.Forms;

public partial class Game {
 Action battleLayoutCleanup;
 void RenderFullBattle(RogueRun r){
  if(battleLayoutCleanup!=null){battleLayoutCleanup();battleLayoutCleanup=null;}
  bool reuse=rogueArena!=null&&!rogueArena.IsDisposed&&rogueArena.Parent==content&&rogueArena.Integrated;
  using(var redraw=new BattleRedrawScope(reuse?(Control)rogueArena:content)){
  if(r.state=="combat"||r.state=="feedback")CardBattle.Ensure(r,save.rogue);RogueEngine.EnsureFirstQuestion(r,save.rogue);
  if(reuse){while(rogueArena.Controls.Count>0)rogueArena.Controls[0].Dispose();}else{ClearPage();rogueArena=new RogueArena{Dock=DockStyle.Fill,Integrated=true};content.Controls.Add(rogueArena);}
  page="rogue";rogueBody=null;var arena=rogueArena;
  arena.AnimationCompleted=null;arena.SupportFrames=new[]{SupportSprite(0),SupportSprite(1),SupportSprite(2),SupportSprite(3)};arena.Art=TowerBackground(r);arena.Hero=RogueHero();arena.Support=SupportSprite(r.cardBattle!=null&&r.cardBattle.lastCard!=null?(new[]{"barrier","echo","cover","rescue"}.Contains(r.cardBattle.lastCard)?2:1):0);arena.EnemyArt=TowerEnemy(r);arena.PistolFrames=CombatFrames("pistol",false);arena.ReactionFrames=CombatFrames("reactions",false);arena.Effects=CombatFrames("effects",true);arena.Run=r;arena.Mode=r.state;arena.ShowDrone=true;arena.HitSound=PlayRogueHit;arena.MonsterSound=PlayMonsterSound;arena.AnimateHit=pendingTowerEffect;pendingTowerEffect=false;arena.RestartAnimation();
  var hud=new OutlinedLabel{BackColor=Color.Transparent,ForeColor=Gold,Font=GameTheme.Body(11),Location=new Point(110,12),Size=new Size(520,90)};hud.Text="词域远征 · "+r.mode+"\n攻击 "+r.attack+" · 护甲 "+r.armor+" · 金币 "+save.rogue.coins+" · 连击 "+r.combo;if(!String.IsNullOrEmpty(r.vocabularyChapter))hud.Text+="\n词汇准备 "+PreparationEngine.Count(save.rogue,r.vocabularyChapter,r.pool)+" / "+r.pool.Count;if(r.cardBattle!=null)hud.Text+="\n护盾 "+r.cardBattle.shield+" · 敌盾 "+r.cardBattle.enemyShield+" · 增伤 +"+r.cardBattle.bonus+" / "+r.cardBattle.percent+"%";arena.Controls.Add(hud);
  var menu=RogueButton("菜单",()=>{},80,40);menu.Location=new Point(12,12);arena.Controls.Add(menu);var dropdown=new FlowLayoutPanel{Visible=false,BackColor=GameTheme.Navy,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoSize=true,Padding=new Padding(6),Location=new Point(12,60)};arena.Controls.Add(dropdown);if(save.rogue.preparationActive)dropdown.Controls.Add(RogueButton("返回本节剧情",ShowStory,180,40));dropdown.Controls.Add(RogueButton("远征大厅",ShowRogueHome,180,40));dropdown.Controls.Add(RogueButton("卡牌图鉴",ShowCardCollection,180,40));dropdown.Controls.Add(RogueButton("生词本",ShowWords,180,40));dropdown.Controls.Add(RogueButton("成长商店",ShowRogueShop,180,40));dropdown.Controls.Add(RogueButton("系统商店",ShowSystemShop,180,40));dropdown.Controls.Add(RogueButton("返回主界面",ShowMain,180,40));menu.Click+=(s,e)=>{dropdown.Visible=!dropdown.Visible;dropdown.BringToFront();};
  var glass=new BattleGlassPanel{Anchor=AnchorStyles.Left|AnchorStyles.Right|AnchorStyles.Bottom,Padding=new Padding(18,10,18,8)};arena.Controls.Add(glass);
  var footer=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=48,BackColor=Color.Transparent,FlowDirection=FlowDirection.RightToLeft,WrapContents=false};glass.Controls.Add(footer);
  var body=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Color.Transparent,Padding=new Padding(0)};glass.Controls.Add(body);body.BringToFront();
  var row=new FlowLayoutPanel{AutoSize=true,WrapContents=true,BackColor=Color.Transparent,Margin=new Padding(0)};body.Controls.Add(row);TableLayoutPanel choices=null;bool compact=content.Height<560;
  if(r.state=="combat"&&r.cardBattle.answering){
   var q=r.question;string type=q.kind==0?"选择词义":q.kind==1?"根据词义选词":q.kind==2?"语境填空":"拼写单词";string text=q.kind==0?q.entry.word:q.kind==2?q.entry.example.Replace("{"+q.entry.word+"}","______"):q.entry.meaning;var prompt=Lab(text,q.kind==0?(compact?18:22):(compact?11:14),TextColor);prompt.BackColor=Color.Transparent;prompt.Margin=new Padding(3,9,12,4);row.Controls.Add(prompt);
   if(q.kind==0||q.kind==3){var speaker=new RogueIcon{Kind="speaker",BackColor=Color.Transparent};speaker.Click+=(s,e)=>SpeakWord(q.entry.word);tips.SetToolTip(speaker,"播放单词发音");row.Controls.Add(speaker);}row.Controls.Add(RogueFeedbackIcon(q.entry,"star"));var hint=new RogueIcon{Kind="bulb",Count=r.hints,Enabled=r.hints>0&&!q.assisted,BackColor=Color.Transparent};hint.Click+=(s,e)=>{if(RogueEngine.Hint(r))SaveRogue();};tips.SetToolTip(hint,"提示 · 剩余 "+r.hints+" 次");row.Controls.Add(hint);
   var typeLabel=Lab(type+(q.kind==3?" · Enter 确认":" · 按 1–4 选择")+(q.assisted?" · 提示："+q.entry.word+" · "+q.entry.meaning:""),10,Gold);typeLabel.BackColor=Color.Transparent;body.Controls.Add(typeLabel);
   if(q.kind==3){var input=new TextBox{Width=350,Font=GameTheme.Body(16),BackColor=GameTheme.Navy,ForeColor=TextColor,MaxLength=80};string spellingKey=r.id+":"+r.answered;if(cardSpellingKey==spellingKey)input.Text=cardSpellingText;input.TextChanged+=(s,e)=>{cardSpellingKey=spellingKey;cardSpellingText=input.Text;};body.Controls.Add(input);Action submit=()=>{if(!String.IsNullOrWhiteSpace(input.Text))AnswerRogue(-1,input.Text);};footer.Controls.Add(RogueButton("确认拼写",submit,160,40));input.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;submit();}};arena.BeginInvoke((Action)(()=>{if(!input.IsDisposed)input.Focus();}));}
   else{choices=new TableLayoutPanel{Height=100,ColumnCount=2,RowCount=2,Margin=new Padding(0),BackColor=Color.Transparent};choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));choices.RowStyles.Add(new RowStyle(SizeType.Percent,50));choices.RowStyles.Add(new RowStyle(SizeType.Percent,50));if(compact){choices.RowCount=1;choices.ColumnCount=4;choices.Height=48;choices.ColumnStyles.Clear();choices.RowStyles.Clear();for(int col=0;col<4;col++)choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,25));choices.RowStyles.Add(new RowStyle(SizeType.Percent,100));}for(int i=0;i<q.options.Count;i++){int answer=i;var button=RogueButton(((char)('A'+i))+". "+q.options[i],()=>AnswerRogue(answer,null),220,44);button.Dock=DockStyle.Fill;button.Margin=new Padding(3);if(compact)button.Font=GameTheme.Body(9);choices.Controls.Add(button,compact?i:i%2,compact?0:i/2);}body.Controls.Add(choices);footer.Visible=false;}
   string key=r.id+":"+r.answered;if((q.kind==0||q.kind==3)&&rogueSpokenQuestion!=key){rogueSpokenQuestion=key;arena.BeginInvoke((Action)(()=>{if(!arena.IsDisposed&&save.rogue.ActiveRun==r&&r.state=="combat"&&r.question==q)SpeakWord(q.entry.word);}));}
  }else if(r.state=="combat"){glass.Visible=false;}else if(r.state=="feedback"){
   row.Controls.Add(Lab("本回合反馈",16,Gold));row.Controls.Add(RogueFeedbackIcon(r.question.entry,"speaker"));row.Controls.Add(RogueFeedbackIcon(r.question.entry,"star"));var feedback=Lab(RogueFeedbackText(r),12,TextColor);feedback.BackColor=Color.Transparent;body.Controls.Add(feedback);footer.Controls.Add(RogueButton(r.hp<=0?"查看结果":r.enemyHp<=0?"领取战利品":"下一回合 · Enter",()=>{RogueEngine.Continue(save.rogue);SaveRogue();},200,40));
  }else{row.Controls.Add(Lab("BOSS · 古界守门者",18,Gold));var intro=Lab("守门者已经苏醒。答题发动攻击，护甲与本局赋能继续生效。",12,TextColor);intro.BackColor=Color.Transparent;body.Controls.Add(intro);footer.Controls.Add(RogueButton("挑战守门者",()=>{if(r.state!="boss-intro")return;TowerEngine.StartBattle(r,save.rogue,"boss");SaveRogue();},200,40));}
  if(r.state=="feedback"&&save.rogue.preparationActive&&current!=null&&PreparationReady(current)){footer.Controls.Add(RogueButton("词汇准备完成 · 进入剧情",ShowStory,240,40));footer.Height=48;}
  AddSupportHand(r,glass,arena);body.BringToFront();AddRelicHud(r,arena);
  Action layout=()=>{if(compact){hud.Height=36;hud.Text="攻击 "+r.attack+" · 护甲 "+r.armor+" · 金币 "+save.rogue.coins+"\n连击 "+r.combo+(r.cardBattle==null?"":" · 护盾 "+r.cardBattle.shield+" · 敌盾 "+r.cardBattle.enemyShield);}int h=r.state=="combat"?Math.Min(compact?220:330,Math.Max(180,arena.Height-100)):Math.Min(270,Math.Max(210,arena.Height*34/100));int popupWidth=Math.Min(760,Math.Max(100,arena.Width-48));glass.Bounds=new Rectangle((arena.Width-popupWidth)/2,Math.Max(65,(arena.Height-h)/2),popupWidth,h);arena.OverlayHeight=0;hud.Width=Math.Max(200,arena.Width-410);int width=Math.Max(180,glass.ClientSize.Width-55);row.MaximumSize=new Size(width,0);foreach(Control child in body.Controls){if(child is Label)((Label)child).MaximumSize=new Size(width,0);}foreach(Control child in row.Controls)if(child is Label)((Label)child).MaximumSize=new Size(Math.Max(140,width-180),0);if(choices!=null)choices.Width=width;if(glass.Visible){glass.PerformLayout();row.PerformLayout();body.PerformLayout();int used=body.Controls.Cast<Control>().Where(c=>c.Visible).Select(c=>c.Bottom+c.Margin.Bottom).DefaultIfEmpty(0).Max();int fitted=Math.Min(Math.Max(140,arena.Height-130),used+glass.Padding.Vertical+(footer.Visible?footer.Height:0)+10);glass.Bounds=new Rectangle((arena.Width-popupWidth)/2,Math.Max(65,(arena.Height-fitted)/2),popupWidth,fitted);glass.PerformLayout();body.PerformLayout();}dropdown.BringToFront();arena.Invalidate();};EventHandler resized=(s,e)=>layout();arena.Resize+=resized;var handCleanup=battleLayoutCleanup;battleLayoutCleanup=()=>{arena.Resize-=resized;if(handCleanup!=null)handCleanup();};layout();
  if(r.state=="feedback"&&arena.AnimateHit){
   glass.Visible=false;
   arena.AnimationCompleted=()=>{if(!glass.IsDisposed){glass.Visible=true;glass.BringToFront();dropdown.BringToFront();}};
  }
  }
 }
}
public class BattleGlassPanel:Panel {
 public BattleGlassPanel(){DoubleBuffered=true;BackColor=Color.Transparent;}
 protected override void OnPaintBackground(PaintEventArgs e){base.OnPaintBackground(e);CyberChrome.Panel(e.Graphics,new Rectangle(1,1,Width-3,Height-3),CyberChrome.Neon);}
}
















