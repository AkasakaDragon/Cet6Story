using System;using System.Drawing;using System.Linq;using System.Windows.Forms;
public partial class Game {
 const string SubtitleStarRule="全答对：★；全答对且未开中文：★★；全答对且未开中英：★★★。未全答对无星。";
 bool ChooseSectionSubtitles(){
  var attempt=Attempt();if(attempt.review)return true;if(attempt.subtitlesChosen){if(attempt.chineseSubtitles.HasValue)save.chinese=translating=attempt.chineseSubtitles.Value;if(attempt.englishSubtitles.HasValue)save.english=englishVisible=attempt.englishSubtitles.Value;return true;}
  using(var dialog=new GuildWordDialog{Text="本节字幕与挑战模式",Width=700,Height=420}){
   dialog.Location=new Point(Left+(Width-dialog.Width)/2,Top+(Height-dialog.Height)/2);
   var body=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(20),BackColor=dialog.BackColor};dialog.Controls.Add(body);
   var heading=Lab(current.title,18,Gold);heading.MaximumSize=new Size(620,0);body.Controls.Add(heading);var rules=Lab(SubtitleStarRule+"\n本节中途开启字幕也会计入评价；重听不扣星。",11);rules.MaximumSize=new Size(620,0);body.Controls.Add(rules);
   bool chosen=false;Action<bool,bool> select=(zh,en)=>{
    // Legacy unfinished attempts have no reliable historical subtitle record.
    bool legacy=attempt.started>0||save.heardLines.Any(k=>k.StartsWith(current.id+"/line/"))||save.quizAnswers.Keys.Any(k=>k.StartsWith(current.id+"/q/"));
    attempt.usedChineseSubtitles|=legacy||zh;attempt.usedEnglishSubtitles|=legacy||en;attempt.subtitlesChosen=true;attempt.chineseSubtitles=zh;attempt.englishSubtitles=en;
    save.chinese=translating=zh;save.english=englishVisible=en;chosen=true;Persist();dialog.Close();
   };
   body.Controls.Add(Btn("中英文字幕 · 全对可得一星",()=>select(true,true),true));
   body.Controls.Add(Btn("仅英文字幕 · 全对可得两星",()=>select(false,true)));
   body.Controls.Add(Btn("关闭中英文字幕 · 全对可得三星",()=>select(false,false)));
   if(attempt.started>0){var old=Lab("旧版未完成进度缺少字幕记录：本次最高一星。重新挑战可争取三星。",10,Muted);old.MaximumSize=new Size(620,0);body.Controls.Add(old);}
   dialog.ShowDialog(this);if(!chosen){ShowMain();return false;}return true;
  }
 }
 void RecordSectionSubtitles(){if(current==null)return;var a=Attempt();if(a.review)return;a.chineseSubtitles=translating;a.englishSubtitles=englishVisible;a.usedChineseSubtitles|=translating;a.usedEnglishSubtitles|=englishVisible;}
 int SectionStars(int correct,int total){RecordSectionSubtitles();var a=Attempt();return SectionRules.Stars(correct,total,a.usedChineseSubtitles,a.usedEnglishSubtitles);}
}
