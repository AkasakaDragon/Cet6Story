using System;
public partial class Game {
 const string SubtitleStarRule="全答对：★★★。未全答对无星；中英文字幕可自由开关，不影响评价。";
 bool ChooseSectionSubtitles(){
  // Subtitle preferences apply directly, including when resuming an older attempt.
  translating=save.chinese;englishVisible=save.english;
  if(current!=null&&!IsSpellPractice())RecordSectionSubtitles();
  return true;
 }
 bool SetStorySubtitle(bool chinese,bool enabled){
  if(chinese)save.chinese=translating=enabled;else save.english=englishVisible=enabled;
  RecordSectionSubtitles();if(page=="story")UpdateLine();Persist();return true;
 }
 void RecordSectionSubtitles(){if(current==null||IsSpellPractice())return;var a=Attempt();a.subtitlesChosen=true;a.chineseSubtitles=translating;a.englishSubtitles=englishVisible;}
 int SectionStars(int correct,int total){return SectionRules.Stars(correct,total,false,false);}
}
