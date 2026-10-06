using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

public partial class Game {
 string wordbookSearch="";int wordbookPage;
 void StartWordbookTraining(){if(save.rogue.run!=null&&save.rogue.run.mode=="我的生词本"&&save.rogue.run.state!="ended"){save.rogue.preparationActive=false;RenderRogue();return;}var pool=OwnRoguePool();if(pool.Count<4||pool.Select(w=>w.meaning).Distinct().Count()<4){GameMessage.Show(this,"生词本训练至少需要 4 个有释义的单词，并且有 4 种不同释义。","生词本训练");return;}save.rogue.preparationActive=false;StartRogue("我的生词本");}
 bool StoreManualWord(string text,string meaning,out string message){text=(text??"").Trim().ToLowerInvariant();meaning=(meaning??"").Trim();if(String.IsNullOrWhiteSpace(text)||String.IsNullOrWhiteSpace(meaning)){message="请填写英文单词和中文释义。";return false;}if(text.Length>80||meaning.Length>1000||!System.Text.RegularExpressions.Regex.IsMatch(text,@"^[a-z]+(?:[ '\-][a-z]+)*$")){message="请输入英文单词或短语；单词最长 80 字符，释义最长 1000 字符。";return false;}if(save.words.Any(w=>w.text.Equals(text,StringComparison.OrdinalIgnoreCase))){message="这个单词已经在生词本里了。";return false;}save.words.Add(new Word{text=text,meaning=meaning,example="",box=0,due=DateTime.Today.ToString("yyyy-MM-dd")});message="已添加："+text;return true;}
 void ShowManualWordDialog(){
  var f=DialogForm("添加新词",580,390);var panel=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(20)};f.Controls.Add(panel);
  var word=new TextBox{Width=480,MaxLength=80,Font=GameTheme.Body(12),BackColor=Bg,ForeColor=TextColor,Margin=new Padding(6)};var meaning=new TextBox{Width=480,MaxLength=1000,Multiline=true,Height=80,Font=GameTheme.Body(12),BackColor=Bg,ForeColor=TextColor,Margin=new Padding(6)};
  panel.Controls.Add(Lab("英文单词 / 短语",11,Muted));panel.Controls.Add(word);panel.Controls.Add(Lab("中文释义",11,Muted));panel.Controls.Add(meaning);var notice=Lab("",11,Gold);notice.MaximumSize=new Size(480,0);
  var actions=new FlowLayoutPanel{AutoSize=true,WrapContents=false};actions.Controls.Add(RogueButton("查找释义",()=>{string text=word.Text.Trim();var found=TrainingPool("四级训练").Concat(TrainingPool("六级挑战")).Concat(RogueBank()).FirstOrDefault(w=>w.word.Equals(text,StringComparison.OrdinalIgnoreCase));var lex=lexicon.Find(text);meaning.Text=found!=null?found.meaning:lex!=null?lex.Meaning:"";notice.Text=String.IsNullOrWhiteSpace(meaning.Text)?"没有查到释义，请手动填写。":"已填入释义，可修改后保存。";},145));actions.Controls.Add(RogueButton("保存单词",()=>{string message;if(!StoreManualWord(word.Text,meaning.Text,out message)){notice.Text=message;return;}Persist();UpdateStats();wordbookSearch="";wordbookPage=0;f.Close();if(page=="words")RenderWords();},145));actions.Controls.Add(RogueButton("取消",()=>f.Close(),100));panel.Controls.Add(actions);panel.Controls.Add(notice);
  var dismiss=new WordPopupDismissFilter(f);Application.AddMessageFilter(dismiss);f.FormClosed+=(sender,e)=>{Application.RemoveMessageFilter(dismiss);f.Dispose();};f.Show(this);f.Location=new Point(Left+(Width-f.Width)/2,Top+(Height-f.Height)/2);word.Focus();
 }
 void RenderWords(){
  // Keep one opaque copy of the current page above the control replacement.
  // This is the existing picture, with no loading screen or timed transition.
  Bitmap previous=null;PictureBox cover=null;
  try{
   if(content.Visible&&content.Width>0&&content.Height>0){
    previous=new Bitmap(content.Width,content.Height);content.DrawToBitmap(previous,new Rectangle(Point.Empty,previous.Size));
    cover=new PictureBox{Image=previous,Bounds=content.Bounds,SizeMode=PictureBoxSizeMode.StretchImage,TabStop=false};
    content.Parent.Controls.Add(cover);cover.BringToFront();cover.Update();
   }
   using(var redraw=new BattleRedrawScope(content)){
    BuildWordbookPage();
    content.ResumeLayout(true);content.PerformLayout();content.SuspendLayout();
    if(content.Width>0&&content.Height>0)using(var frame=new Bitmap(content.Width,content.Height))content.DrawToBitmap(frame,new Rectangle(Point.Empty,frame.Size));
   }
  }finally{
   if(cover!=null){cover.Parent.Controls.Remove(cover);cover.Dispose();}
   if(previous!=null)previous.Dispose();
   content.Refresh();
  }
 }
 void BuildWordbookPage(){RoguePage("生词本");page="words";var heading=RogueCard("生词本",save.words.Count+" 个词 · 远征金币 "+save.rogue.coins);heading.Controls[0].Font=GameTheme.Body(23);
  var tools=RogueCard("生词本训练与管理","使用收藏词开启肉鸽远征；至少需要 4 个单词和 4 种释义。可按英文或中文查找收藏词。");var own=OwnRoguePool();int mastered=own.Count(w=>{RogueMemory m;return save.rogue.memory.TryGetValue(w.word,out m)&&(m.mask&3)==3;});tools.Controls.Add(Lab("可训练 "+own.Count+" 词 · 已掌握 "+mastered+" / "+own.Count,12,Accent));
  RogueActions(tools,RogueButton(save.rogue.run!=null&&save.rogue.run.mode=="我的生词本"&&save.rogue.run.state!="ended"?"继续生词本远征":"开始生词本远征",StartWordbookTraining,250),RogueButton("添加新词",ShowManualWordDialog,180));
  tools.Controls.Add(Lab("查找收藏词",11,Muted));var input=new TextBox{Text=wordbookSearch,Width=360,Font=GameTheme.Body(12),BackColor=Bg,ForeColor=TextColor,Margin=new Padding(6)};tools.Controls.Add(input);Action search=()=>{wordbookSearch=input.Text.Trim();wordbookPage=0;RenderWords();};input.KeyDown+=(sender,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;search();}};RogueActions(tools,RogueButton("查找",search,130),RogueButton("显示全部",()=>{wordbookSearch="";wordbookPage=0;RenderWords();},160));
  if(reviewWord!=null){var card=RogueCard(reviewWord.text,revealed?reviewWord.meaning:"先回忆这个单词的含义，再翻开词义。");if(revealed&&!String.IsNullOrWhiteSpace(reviewWord.example))card.Controls.Add(Lab(reviewWord.example,11,Muted));if(!revealed)RogueActions(card,RogueButton("显示词义",()=>{revealed=true;RenderWords();},230),RogueButton("听单词",()=>SpeakWord(reviewWord.text),180));else RogueActions(card,RogueButton("认识 · 延长复习间隔",()=>ReviewAnswer(true),270),RogueButton("忘了 · 今天再次复习",()=>ReviewAnswer(false),270));}
  if(save.words.Count==0)RogueCard("生词本还是空的","剧情中可以点击英文单词收藏；远征答错的词会自动收录。点击上方“添加新词”按钮也可以收录单词。");
  else {var list=save.words.Where(w=>String.IsNullOrWhiteSpace(wordbookSearch)||w.text.IndexOf(wordbookSearch,StringComparison.OrdinalIgnoreCase)>=0||(w.meaning??"").IndexOf(wordbookSearch,StringComparison.OrdinalIgnoreCase)>=0).OrderBy(w=>w.text).ToList();int pages=Math.Max(1,(list.Count+11)/12);wordbookPage=Math.Max(0,Math.Min(pages-1,wordbookPage));var result=RogueCard("筛选结果 "+list.Count+" 个 · 第 "+(wordbookPage+1)+" / "+pages+" 页","");result.Controls[0].Font=GameTheme.Body(11);result.Controls[1].Visible=false;foreach(var word in list.Skip(wordbookPage*12).Take(12)){var chosen=word;RogueMemory memory;string stats=save.rogue.memory.TryGetValue(word.text,out memory)?" · 远征无提示答对 "+memory.correct+" / 答错 "+memory.wrong:"";var card=RogueCard(word.text,word.meaning+"\n下次复习 "+word.due+stats);RogueActions(card,RogueButton("听单词",()=>SpeakWord(chosen.text),150),RogueButton("复习这个词",()=>{reviewWord=chosen;revealed=false;RenderWords();},190),RogueButton("移除收藏",()=>{if(GameMessage.Show(this,"从生词本移除 "+chosen.text+"？远征学习统计会保留。","移除收藏",MessageBoxButtons.YesNo)==DialogResult.Yes){save.words.Remove(chosen);if(reviewWord==chosen)reviewWord=null;Persist();RenderWords();}},170));}var nav=RogueCard("翻页","");var previous=RogueButton("上一页",()=>{wordbookPage--;RenderWords();},170);previous.Enabled=wordbookPage>0;var next=RogueButton("下一页",()=>{wordbookPage++;RenderWords();},170);next.Enabled=wordbookPage<pages-1;RogueActions(nav,previous,next);}
  RogueLayout();
 }
}
