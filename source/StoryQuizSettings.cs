using System;using System.Linq;using System.Collections.Generic;
public static class StoryQuizRules {
 public static bool Passed(Save save,string chapter){return save.sectionStars.ContainsKey(chapter)&&save.sectionStars[chapter]>=2||save.storyReadCompleted!=null&&save.storyReadCompleted.Contains(chapter);}
 public static void MarkRead(Save save,string chapter){if(save.storyReadCompleted==null)save.storyReadCompleted=new List<string>();if(!save.storyReadCompleted.Contains(chapter))save.storyReadCompleted.Add(chapter);}
}
public partial class Game {
 bool StoryQuestionsEnabled{get{return IsSpellPractice()||(save.storyQuizEnabled??true);}}
 void CompleteReadOnlyStory(){
  if(Attempt().finished==0)FinishSectionTiming();StoryQuizRules.MarkRead(save,current.id);
  bool fresh=!save.completed.Contains(current.id);if(fresh){save.completed.Add(current.id);save.xp+=60;}Persist();UpdateStats();
  GameMessage.Show(this,current.ending+"\n\n主线选择题已关闭，本次不评听力星级。"+(fresh?"\n首次剧情通关 +60 XP":""),"本节剧情完成");ShowStoryMap();
 }
}
