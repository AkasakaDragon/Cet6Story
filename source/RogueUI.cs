using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

public partial class Game {
 string rogueSpokenQuestion=""; List<RogueEntry> rogueBank;FlowLayoutPanel rogueBody;RogueArena rogueArena;
 List<RogueEntry> cet4Bank,cet6Bank;
 List<RogueEntry> TrainingPool(string mode){if(mode=="基础训练")return RogueBank().Where(w=>w.tier==1).ToList();if(mode=="四级训练"||mode=="进阶训练"){if(cet4Bank==null)cet4Bank=Engine.Json.Deserialize<List<RogueEntry>>(File.ReadAllText(Path.Combine(root,"assets","cet4-words.json")));return cet4Bank;}if(cet6Bank==null)cet6Bank=Engine.Json.Deserialize<List<RogueEntry>>(File.ReadAllText(Path.Combine(root,"assets","cet6-words.json")));return cet6Bank;}
 List<RogueEntry> RogueBank(){if(rogueBank==null)rogueBank=RogueEngine.LoadBank(Path.Combine(root,"assets","rogue-words.tsv"));return rogueBank;}
 VNButton RogueButton(string text,Action action,int width=240,int height=55){var b=new RogueChoice{Text=text,PixelStyle=true,Width=width,Height=height,Margin=new Padding(6),Font=GameTheme.Body(11),ForeColor=TextColor};b.Click+=(sender,e)=>action();return b;}
 void RoguePage(string title){ClearPage();page="rogue";rogueArena=null;var sceneArt=QuietScene(title.Contains("商店")||title.Contains("图鉴")?1:save.rogue.ActiveRun==null?0:save.rogue.ActiveRun.theme);content.BackgroundImage=sceneArt;content.BackgroundImageLayout=ImageLayout.Stretch;var header=new FlowLayoutPanel{Dock=DockStyle.Top,Height=64,Padding=new Padding(16,8,0,0),BackColor=Color.Transparent,WrapContents=false,AutoScroll=true};content.Controls.Add(header);if(save.rogue.preparationActive)header.Controls.Add(RogueButton("返回本节剧情",ShowStory,160,42));header.Controls.Add(RogueButton("远征大厅",ShowRogueHome,135,42));header.Controls.Add(RogueButton("生词本",ShowWords,110,42));header.Controls.Add(RogueButton("系统商店",ShowSystemShop,135,42));header.Controls.Add(RogueButton("返回主界面",ShowMain,150,42));rogueBody=new ExpeditionSurface{Art=sceneArt,Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(22,15,22,24),BackColor=Color.Transparent};content.Controls.Add(rogueBody);rogueBody.BringToFront();var h=Lab(title,23,Gold);rogueBody.Controls.Add(h);rogueBody.Resize+=(s,e)=>RogueLayout();}
 void RogueLayout(){if(rogueBody==null||rogueBody.IsDisposed)return;int width=Math.Max(650,rogueBody.ClientSize.Width-65);foreach(Control c in rogueBody.Controls){if(c is RogueCard||c is RogueArena)c.Width=width;else if(c is Label)c.MaximumSize=new Size(width,0);if(c is RogueCard){var card=(RogueCard)c;card.MaximumSize=new Size(width,0);card.MinimumSize=new Size(width,0);foreach(Control child in card.Controls){if(child is VNButton)child.Width=width-45;else if(child is TableLayoutPanel)child.Width=width-45;else if(child is Label)child.MaximumSize=new Size(width-45,0);else if(child is FlowLayoutPanel)child.MaximumSize=new Size(width-45,0);}}}rogueBody.PerformLayout();}
 RogueCard RogueCard(string title,string description){var card=new RogueCard{Width=Math.Max(650,content.Width-85),AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(18),Margin=new Padding(6,7,6,7),BackColor=Color.Transparent};card.Controls.Add(Lab(title,16,Gold));var label=Lab(description,11,Muted);label.MaximumSize=new Size(Math.Max(600,content.Width-140),0);card.Controls.Add(label);rogueBody.Controls.Add(card);card.Resize+=(s,e)=>{foreach(var l in card.Controls.OfType<Label>())l.MaximumSize=new Size(Math.Max(580,card.Width-50),0);};return card;}
 void RogueActions(RogueCard card,params Control[] buttons){var row=new FlowLayoutPanel{AutoSize=true,MaximumSize=new Size(Math.Max(600,card.Width-40),0),WrapContents=true,Margin=new Padding(0)};row.Controls.AddRange(buttons);card.Controls.Add(row);}
 Image RogueHero(){return CachedImage(Path.Combine(root,"chapters","art","neon","xingyao.png"));}
 List<RogueEntry> OwnRoguePool(){
  if(sectionVocabulary==null)sectionVocabulary=Engine.Json.Deserialize<Dictionary<string,List<RogueEntry>>>(File.ReadAllText(Path.Combine(root,"assets","section-vocabulary.json")));
  var examples=RogueBank().Concat(sectionVocabulary.Values.SelectMany(x=>x)).ToLookup(e=>e.word,StringComparer.OrdinalIgnoreCase);
  return save.words.Where(w=>w!=null&&!String.IsNullOrWhiteSpace(w.text)&&!String.IsNullOrWhiteSpace(w.meaning)).Select(w=>{string meaning=w.meaning.Split('\n')[0].Trim();var bank=examples[w.text].FirstOrDefault(e=>e.meaning==meaning&&!String.IsNullOrWhiteSpace(e.example)&&e.example.Contains("{"+w.text+"}"));return new RogueEntry{word=w.text,meaning=meaning,tier=bank==null?2:bank.tier,exampleZh=bank==null?"":bank.exampleZh,example=bank==null?"":bank.example};}).Where(e=>!String.IsNullOrWhiteSpace(e.meaning)).GroupBy(e=>e.word,StringComparer.OrdinalIgnoreCase).Select(g=>g.First()).ToList();
 }

 void StartRogue(string mode){if(save.rogue.ActiveRun!=null&&save.rogue.ActiveRun.state!="ended"){SetStatus("已有未结束的远征，请先继续或结束本局。");GameMessage.Show(this,"已有未结束的远征，请先继续或结束本局。","词域远征");return;}var pool=mode=="我的生词本"?OwnRoguePool():TrainingPool(mode);try{RogueEngine.NewRun(save.rogue,pool,mode,BitConverter.ToInt32(Guid.NewGuid().ToByteArray(),0));Persist();RenderRogue();}catch(Exception ex){GameMessage.Show(this,ex.Message,"无法开局");}}
 void SaveRogue(){using(var redraw=new BattleRedrawScope(content)){Persist();if(page=="tavern-battle")ShowTavernBattle();else RenderRogue();}}
 void RenderRogueQuestion(RogueRun r){RogueEngine.EnsureFirstQuestion(r,save.rogue);var q=r.question;string type=q.kind==0?"选择词义":q.kind==1?"根据词义选词":q.kind==2?"语境填空":"拼写单词";string prompt=q.kind==0?q.entry.word:q.kind==1||q.kind==3?q.entry.meaning:q.entry.example.Replace("{"+q.entry.word+"}","______");var card=RogueCard(type+(q.kind!=3?" · 按 1–4 选择":"")+"   ·   连击 "+r.combo,prompt);card.Controls[1].Font=GameTheme.Latin(q.kind==0?25:17);card.Controls[1].ForeColor=TextColor;
  if(q.assisted)card.Controls.Add(Lab("提示："+q.entry.word+" · "+q.entry.meaning+"（本题不发金币）",12,Gold));
  if(q.kind==3){var input=new TextBox{Width=400,Font=GameTheme.Latin(20),BackColor=Bg,ForeColor=TextColor,BorderStyle=BorderStyle.FixedSingle,Margin=new Padding(8),MaxLength=80};card.Controls.Add(input);Action submit=()=>{if(String.IsNullOrWhiteSpace(input.Text))return;AnswerRogue(-1,input.Text);};card.Controls.Add(RogueButton("确认拼写 · Enter",submit,320));input.KeyDown+=(sender,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;submit();}};input.Focus();}
  else {var grid=new TableLayoutPanel{Width=Math.Max(600,content.Width-145),Height=128,ColumnCount=2,RowCount=2,Margin=new Padding(0)};grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));grid.RowStyles.Add(new RowStyle(SizeType.Percent,50));grid.RowStyles.Add(new RowStyle(SizeType.Percent,50));for(int i=0;i<q.options.Count;i++){int selected=i;var option=RogueButton(((char)('A'+i))+". "+q.options[i],()=>AnswerRogue(selected,null),280,54);option.Dock=DockStyle.Fill;grid.Controls.Add(option,i%2,i/2);}card.Controls.Add(grid);card.Resize+=(sender,e)=>grid.Width=Math.Max(570,card.Width-44);}

  var promptLabel=card.Controls[1];card.Controls.Remove(promptLabel);var wordRow=new FlowLayoutPanel{AutoSize=true,WrapContents=true,Margin=new Padding(0),MaximumSize=new Size(Math.Max(600,card.Width-45),0)};promptLabel.Margin=new Padding(4,8,12,4);promptLabel.MaximumSize=new Size(Math.Max(300,card.Width-240),0);wordRow.Controls.Add(promptLabel);
  if(q.kind==0||q.kind==3){var hear=new RogueIcon{Kind="speaker",AccessibleName="听单词"};hear.Click+=(sender,e)=>{if(r.state=="combat"&&r.question==q)SpeakWord(q.entry.word);};tips.SetToolTip(hear,"播放单词发音");wordRow.Controls.Add(hear);}
  var star=new RogueIcon{Kind="star",Selected=save.words.Any(w=>w.text.Equals(q.entry.word,StringComparison.OrdinalIgnoreCase)),AccessibleName="收藏单词"};star.Click+=(sender,e)=>{ToggleRogueFavorite(q.entry,star);};tips.SetToolTip(star,star.Selected?"已收藏到生词本":"收藏到生词本");wordRow.Controls.Add(star);
  var hint=new RogueIcon{Kind="bulb",Count=r.hints,Enabled=r.hints>0&&!q.assisted,AccessibleName="提示"};hint.Click+=(sender,e)=>{if(RogueEngine.Hint(r))SaveRogue();};tips.SetToolTip(hint,q.assisted?"本题已使用提示":"提示 · 剩余 "+r.hints+" 次");wordRow.Controls.Add(hint);card.Controls.Add(wordRow);card.Controls.SetChildIndex(wordRow,1);
  string key=r.id+":"+r.answered;if((q.kind==0||q.kind==3)&&rogueSpokenQuestion!=key){rogueSpokenQuestion=key;BeginInvoke((Action)(()=>{if(!wordRow.IsDisposed&&(page=="rogue"||page=="prep-combat")&&r.state=="combat"&&r.question==q)SpeakWord(q.entry.word);}));}}



 string RogueFeedbackText(RogueRun r){
  var entry=r.question.entry;string zh=entry.exampleZh;
  if(String.IsNullOrWhiteSpace(zh)){
   var original=RogueBank().FirstOrDefault(w=>w.word.Equals(entry.word,StringComparison.OrdinalIgnoreCase)&&w.example==entry.example);
   if(original!=null)zh=original.exampleZh;
   if(String.IsNullOrWhiteSpace(zh)){
    if(sectionVocabulary==null)sectionVocabulary=Engine.Json.Deserialize<Dictionary<string,List<RogueEntry>>>(File.ReadAllText(Path.Combine(root,"assets","section-vocabulary.json")));
    original=sectionVocabulary.Values.SelectMany(words=>words).FirstOrDefault(w=>w.word.Equals(entry.word,StringComparison.OrdinalIgnoreCase)&&w.example==entry.example);
    if(original!=null)zh=original.exampleZh;
   }
   if(String.IsNullOrWhiteSpace(zh)&&entry.example.StartsWith("Practice the word"))zh="继续之前，请练习单词“"+entry.word+"”。";
  }
  string feedback=r.feedback??"";
  if(r.cardBattle!=null&&!String.IsNullOrEmpty(r.cardBattle.log))feedback=feedback.Replace("\n"+r.cardBattle.log,"");
  var lines=feedback.Replace("\r","").Split('\n').ToList();
  if(r.cardBattle!=null&&lines.Count>0&&!lines[0].Contains("护盾抵挡")){
   int blocked=r.cardBattle.lastBlocked;var match=System.Text.RegularExpressions.Regex.Match(r.cardBattle.log??"",@"护盾抵挡\s+(\d+)");if(match.Success)int.TryParse(match.Groups[1].Value,out blocked);
   lines[0]+=" · 护盾抵挡 "+blocked;
  }
  if(r.cardBattle!=null&&r.cardBattle.burnDamage>0)lines.Insert(Math.Min(1,lines.Count),"燃烧结算 · 造成 "+r.cardBattle.burnDamage+" 伤害");
  if(!String.IsNullOrWhiteSpace(zh))lines.Insert(Object.ReferenceEquals(r,save.tavernBattle)?lines.Count:Math.Min(3,lines.Count),"例句译文："+zh);
  return String.Join("\n",lines);
 }
 Control RogueFeedbackIcon(RogueEntry entry,string kind){
  var icon=new RogueIcon{Kind=kind,Selected=kind=="star"&&save.words.Any(w=>w.text.Equals(entry.word,StringComparison.OrdinalIgnoreCase)),AccessibleName=kind=="speaker"?"播放单词语音":"收藏单词"};
  tips.SetToolTip(icon,kind=="speaker"?"播放单词语音":icon.Selected?"已收藏到生词本":"收藏到生词本");
  icon.Click+=(sender,e)=>{if(kind=="speaker")SpeakWord(entry.word);else{ToggleRogueFavorite(entry,icon);}};return icon;
 }
 void AnswerRogue(int selected,string spelling){if(page=="tavern-battle"){
 var battle=save.tavernBattle;var snapshot=new CombatHealthSnapshot(battle);if(TavernStory.Answer(battle,selected)){pendingTowerEffect=true;pendingHealthPresentation=snapshot;if(selected!=battle.question.answer)CollectRogue(battle.question.entry);SaveRogue();}return;}
 var r=save.rogue.ActiveRun;var before=r==null?null:new CombatHealthSnapshot(r);if(RogueEngine.Answer(save.rogue,selected,spelling,DateTime.Now)){pendingTowerEffect=true;pendingHealthPresentation=before;if(r.question.answered&&r.wrongWords.Contains(r.question.entry.word))CollectRogue(r.question.entry);SaveRogue();}}
 void ToggleRogueFavorite(RogueEntry entry,RogueIcon icon){bool owned=save.words.Any(w=>w.text.Equals(entry.word,StringComparison.OrdinalIgnoreCase));if(owned){save.words.RemoveAll(w=>w.text.Equals(entry.word,StringComparison.OrdinalIgnoreCase));if(reviewWord!=null&&reviewWord.text.Equals(entry.word,StringComparison.OrdinalIgnoreCase))reviewWord=null;}else CollectRogue(entry);Persist();UpdateStats();icon.Selected=!owned;icon.Invalidate();tips.SetToolTip(icon,icon.Selected?"已收藏 · 再次点击取消":"收藏到生词本");}
 void CollectRogue(RogueEntry entry){if(!save.words.Any(w=>w.text.Equals(entry.word,StringComparison.OrdinalIgnoreCase)))save.words.Add(new Word{text=entry.word,meaning=entry.meaning,example=entry.example.Replace("{"+entry.word+"}",entry.word),box=0,due=DateTime.Today.ToString("yyyy-MM-dd")});}
}

public class RogueChoice:VNButton {
 bool over;
 protected override void OnMouseEnter(EventArgs e){over=true;base.OnMouseEnter(e);}
 protected override void OnMouseLeave(EventArgs e){over=false;base.OnMouseLeave(e);}
 protected override void OnPaint(PaintEventArgs e){ExpeditionVisuals.Button(e.Graphics,ClientRectangle,Text,Font,over||Focused,Enabled);}

}
public class RogueCard:FlowLayoutPanel {
 public RogueCard(){DoubleBuffered=true;SetStyle(ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;}
 protected override void OnPaintBackground(PaintEventArgs e){base.OnPaintBackground(e);CyberChrome.Panel(e.Graphics,new Rectangle(1,1,Width-3,Height-3),CyberChrome.Neon);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);}

}





