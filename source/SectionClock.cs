using System;
using System.Diagnostics;
using System.Windows.Forms;

public partial class Game {
 readonly Timer sectionClock=new Timer{Interval=1000};
 readonly Stopwatch sectionWatch=new Stopwatch();
 SectionAttempt timedAttempt;
 void InitSectionClock(){sectionClock.Tick+=(s,e)=>{if(timedAttempt==null)return;CommitSectionTiming();Persist();UpdateNeonHud();};}
 void CommitSectionTiming(){if(timedAttempt==null||!sectionWatch.IsRunning)return;timedAttempt.elapsedTicks=Math.Max(0,timedAttempt.elapsedTicks??0)+sectionWatch.Elapsed.Ticks;sectionWatch.Restart();}
 void PauseSectionTiming(){CommitSectionTiming();sectionWatch.Reset();sectionClock.Stop();if(timedAttempt!=null)Persist();timedAttempt=null;}
 void ResumeSectionTiming(){
  if(page!="story"||current==null||!current.inlineQuestions)return;
  var attempt=Attempt();if(attempt.started==0||attempt.finished>0||attempt.review)return;
  if(timedAttempt==attempt&&sectionWatch.IsRunning)return;
  PauseSectionTiming();timedAttempt=attempt;if(!attempt.elapsedTicks.HasValue)attempt.elapsedTicks=0;sectionWatch.Restart();sectionClock.Start();
 }
 void FinishSectionTiming(){PauseSectionTiming();Attempt().finished=DateTime.UtcNow.Ticks;}
}
