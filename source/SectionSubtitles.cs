using System;using System.Drawing;using System.Linq;using System.Windows.Forms;
public partial class Game {
 const string SubtitleStarRule="全答对：★；全答对且未开中文：★★；全答对且未开中英：★★★。未全答对无星。";
 bool ChooseSectionSubtitles(){
  var attempt=Attempt();if(attempt.review)return true;if(attempt.subtitlesChosen){if(attempt.chineseSubtitles.HasValue)save.chinese=translating=attempt.chineseSubtitles.Value;if(attempt.englishSubtitles.HasValue)save.english=englishVisible=attempt.englishSubtitles.Value;return true;}
  using(var dialog=new GuildWordDialog{QuestStyle=true,BackColor=Color.FromArgb(6,29,29),Padding=new Padding(30,58,30,30),Text="本节字幕与挑战模式",Width=700,Height=450}){
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
   foreach(var button in body.Controls.OfType<Button>()){button.AutoSize=false;button.Size=new Size(590,46);button.Margin=new Padding(0,6,0,6);}
   Action alignOptions=()=>{int width=Math.Max(1,body.ClientSize.Width-body.Padding.Horizontal);foreach(var button in body.Controls.OfType<Button>())button.Width=width;heading.MaximumSize=new Size(width,0);rules.MaximumSize=new Size(width,0);};body.Resize+=(s,e)=>alignOptions();alignOptions();
   if(attempt.started>0){var old=Lab("旧版未完成进度缺少字幕记录：本次最高一星。重新挑战可争取三星。",10,Muted);old.MaximumSize=new Size(620,0);body.Controls.Add(old);}
   dialog.ShowDialog(this);if(!chosen){page="subtitle-cancelled";StopAudio();var cancelledChapter=current;if(WaystationChapterOne.Dialogue(current)){Action returnToList=()=>{if(IsDisposed||page!="subtitle-cancelled"||current!=cancelledChapter)return;bool chapterOne=WaystationChapterOne.Is(cancelledChapter);ShowWaystationWorldMap();if(chapterOne)ShowTavernMainQuest();};if(menuLoading!=null&&!menuLoading.IsDisposed){var wait=new Timer{Interval=50};wait.Tick+=(s,e)=>{if(IsDisposed){wait.Dispose();return;}if(menuLoading!=null&&!menuLoading.IsDisposed)return;wait.Stop();wait.Dispose();returnToList();};wait.Start();}else returnToList();}else ShowChapters();return false;}return true;
  }
 }
 bool SetStorySubtitle(bool chinese,bool enabled){
  bool duringSection=!IsSpellPractice()&&current!=null&&(page=="story"||page=="waystation-chapter-cg"||page=="tavern-opening"||page=="goddess-transfer"||page=="knight-entrance");
  if(duringSection&&enabled){var a=Attempt();int before=SectionRules.Stars(1,1,a.usedChineseSubtitles,a.usedEnglishSubtitles),after=SectionRules.Stars(1,1,a.usedChineseSubtitles||chinese,a.usedEnglishSubtitles||!chinese);
   if(!a.review&&after<before){bool resume=!pausedAudio&&(originalPlaying||speech!=null&&speech.State==System.Speech.Synthesis.SynthesizerState.Speaking);if(resume)TogglePlay();DialogResult answer;try{answer=GameMessage.Show(inGameSettings!=null&&!inGameSettings.IsDisposed?(IWin32Window)inGameSettings:this,"开启"+(chinese?"中文字幕":"英文字幕")+"后，本节全答对的最高评价将从"+before+"星降为"+after+"星。\n之后关闭字幕也不会恢复本次星级；重新挑战可重新争取。\n是否仍要开启？","开启字幕确认",MessageBoxButtons.YesNo,MessageBoxIcon.Warning);}finally{if(resume&&page=="story"&&pausedAudio)TogglePlay();}if(answer!=DialogResult.Yes)return false;}
  }
  if(chinese)save.chinese=translating=enabled;else save.english=englishVisible=enabled;if(duringSection)RecordSectionSubtitles();if(page=="story")UpdateLine();Persist();return true;
 }
 void RecordSectionSubtitles(){if(current==null||IsSpellPractice())return;var a=Attempt();if(a.review)return;a.chineseSubtitles=translating;a.englishSubtitles=englishVisible;a.usedChineseSubtitles|=translating;a.usedEnglishSubtitles|=englishVisible;}
 int SectionStars(int correct,int total){RecordSectionSubtitles();var a=Attempt();return SectionRules.Stars(correct,total,a.usedChineseSubtitles,a.usedEnglishSubtitles);}
}
