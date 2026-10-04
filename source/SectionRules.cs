using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

public static class SectionRules {
 public const int AnswerSeconds=30;
 public static int Stars(int correct,int total,bool timely){return total>0&&correct==total?(timely?3:2):1;}
 public static bool Unlocked(Chapter chapter,IList<Chapter> chapters,Save save){
  if(!chapter.inlineQuestions)return Engine.Level(save.xp)>=Math.Max(1,chapter.unlockLevel);
  StoryRoutes.Normalize(save);
  if(chapter.id=="neon-01-A")return save.storyFlags.Contains("clinic-discovered")&&save.sectionStars.ContainsKey("neon-01-01")&&save.sectionStars["neon-01-01"]>=2;
  var list=chapters.Where(c=>c.inlineQuestions&&!StoryRoutes.Hidden(c.id)).ToList();int i=list.IndexOf(chapter);
  return i<=0||(save.sectionStars.ContainsKey(list[i-1].id)&&save.sectionStars[list[i-1].id]>=2);

 }
}

public class SectionAttempt { public bool introShown {get;set;} public bool endingShown {get;set;}
 public long? elapsedTicks {get;set;}
 public bool rulesShown {get;set;} public long started {get;set;} public long finished {get;set;} public bool usedReplay {get;set;} public bool review {get;set;} public int replayAfterLine {get;set;}
 public SectionAttempt(){replayAfterLine=-1;}
}
public partial class Game {
 int listeningFrom=-1,listeningTo=-1;string listeningChapter="";int requestedReplay=-1;
 SectionAttempt Attempt(){SectionAttempt a;if(!save.sectionAttempts.TryGetValue(current.id,out a)){a=new SectionAttempt();save.sectionAttempts[current.id]=a;}return a;}
 int SectionLimit(){return current.timeLimitSeconds>0?current.timeLimitSeconds:300;}
 double SectionSeconds(){var a=Attempt();return TimeSpan.FromTicks(Math.Max(0,(a.elapsedTicks??0)+(timedAttempt==a&&sectionWatch.IsRunning?sectionWatch.Elapsed.Ticks:0))).TotalSeconds;}
 string TimingText(){var a=Attempt();return "整节用时 "+TimeSpan.FromSeconds(SectionSeconds()).ToString(@"mm\:ss")+" / "+TimeSpan.FromSeconds(SectionLimit()).ToString(@"mm\:ss")+" · 全对且限时内完成得三星";}
 void BeginSectionTiming(){if(!current.inlineQuestions||Attempt().review)return;var a=Attempt();if(a.started==0){a.started=DateTime.UtcNow.Ticks;a.elapsedTicks=0;Persist();}ResumeSectionTiming();}
 string HeardKey(int i){return current.id+"/line/"+i;}
 QuizQuestion SegmentQuestion(){return current.inlineQuestions?current.questions.OrderBy(q=>q.afterLine).FirstOrDefault(q=>q.afterLine>=index):null;}
 int SegmentFirst(QuizQuestion q){var prev=current.questions.Where(x=>x.afterLine<q.afterLine).OrderByDescending(x=>x.afterLine).FirstOrDefault();return prev==null?0:prev.afterLine+1;}
 bool SegmentHeard(QuizQuestion q){return Enumerable.Range(SegmentFirst(q),q.afterLine-SegmentFirst(q)+1).All(i=>save.heardLines.Contains(HeardKey(i)));}
 bool SubtitleAllowed(){return true;}
 bool ChineseAllowed(){return !current.inlineQuestions||Attempt().review;}
 bool LineHeard(){return !current.inlineQuestions||Attempt().review||save.heardLines.Contains(HeardKey(index));}
 // Save each completed sentence during continuous playback, rather than waiting
 // for the entire question segment to finish. Unfinished sentences stay locked.
 void RecordListeningProgress(long milliseconds){
  if(current==null||!current.inlineQuestions||listeningChapter!=current.id||listeningFrom<0)return;
  bool changed=false;while(listeningFrom<=listeningTo&&listeningFrom<current.lines.Count&&current.lines[listeningFrom].end*1000<=milliseconds+1){string key=HeardKey(listeningFrom++);if(!save.heardLines.Contains(key)){save.heardLines.Add(key);changed=true;}}
  if(changed)Persist();
 }
 void RefreshListeningProgress(){if(!originalPlaying||current==null)return;var position=new System.Text.StringBuilder(64);mciSendString("status storyaudio position",position,position.Capacity,IntPtr.Zero);long milliseconds;if(long.TryParse(position.ToString(),out milliseconds))RecordListeningProgress((long)Math.Round(milliseconds*save.storySpeed));}
 void RecordListening(){if(listeningChapter!=current.id||listeningFrom<0)return;for(int i=listeningFrom;i<=listeningTo;i++)if(!save.heardLines.Contains(HeardKey(i)))save.heardLines.Add(HeardKey(i));if(allPlaying)index=listeningTo;listeningFrom=-1;listeningTo=-1;Persist();UpdateLine();}
 void ResetSection(){StoryRoutes.Normalize(save);save.storyRoutes.Remove(current.id);string prefix=current.id+"/q/";foreach(var k in save.quizAnswers.Keys.Where(k=>k.StartsWith(prefix)).ToList())save.quizAnswers.Remove(k);foreach(var k in save.answerStarted.Keys.Where(k=>k.StartsWith(prefix)).ToList())save.answerStarted.Remove(k);foreach(var k in save.answerTimely.Keys.Where(k=>k.StartsWith(prefix)).ToList())save.answerTimely.Remove(k);save.heardLines.RemoveAll(k=>k.StartsWith(current.id+"/line/"));save.sectionAttempts[current.id]=new SectionAttempt();index=0;save.positions[current.id]=0;Persist();}
 void ClearAnsweredReplay(){if(!current.inlineQuestions)return;var a=Attempt();if(a.replayAfterLine==index&&save.quizAnswers.ContainsKey(current.id+"/q/"+index)){a.replayAfterLine=-1;Persist();}}
 void ReplayQuestion(QuizQuestion q){StopAudio();var a=Attempt();a.usedReplay=true;a.replayAfterLine=q.afterLine;index=SegmentFirst(q);englishVisible=true;Persist();UpdateLine();allPlaying=true;StartLine(true);}
 void ReviewSection(){StopAudio();var a=Attempt();a.review=true;a.replayAfterLine=-1;index=0;englishVisible=true;Persist();UpdateLine();allPlaying=true;StartLine(true);}
}
