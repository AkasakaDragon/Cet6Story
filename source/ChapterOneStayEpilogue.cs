using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public partial class Game {
 void ShowChapterOneStayEpilogue(){
  StopAudio();ClearPage();page="waystation-stay-epilogue";
  string[] speakers={"旁白","陆川","艾莉娅","陆川","艾莉娅","陆川","艾莉娅","旁白"};
  string[] zh={
   "打烊之后，陆川收起账本。艾莉娅却仍站在矿道记录旁，目光停在那枚修补过的晶核上。",
   "今天多亏你了。不过，你明天是不是也该继续赶路了？",
   "矿道里的事还没查清。那块碎片为什么会在那里，晶核又为什么会回应它……我想留下弄明白。",
   "所以，是为了调查？",
   "还有，你今天没有逞强。该配合的时候配合，该撤退的时候也听得进去。和这样的人一起做事，我比较放心。",
   "那明天，还是给你留一份早饭？",
   "留吧。桌子你擦，别又全推给我。",
   "陆川拿起抹布，艾莉娅把记录折好。留下来的理由有了答案：未解的线索，还有一个值得信任的搭档。门边的灯，仍然亮着。"};
  string[] en={
   "After closing, Lu Chuan puts away the ledger. Aelia remains beside the mine notes, studying the repaired crystal.",
   "Thanks for everything today. But shouldn't you be continuing your journey tomorrow?",
   "We still haven't solved the mystery in the mine. Why was that fragment there, and why did the crystal respond to it? I want to stay and find out.",
   "So you're staying to investigate?",
   "And you didn't try to be a hero today. You worked with us and listened when it was time to retreat. I feel safer working with someone like that.",
   "Then shall I save you some breakfast tomorrow?",
   "Please do. You're wiping the tables, though. Don't leave them all to me again.",
   "Lu Chuan picks up the cloth as Aelia folds the notes. Unanswered questions and a trusted partner give her a reason to stay. The lamp by the door is still burning."};
  stayOriginalChapter=current;stayOriginalIndex=index;
  stayDialogueChapter=new Chapter{id=current.id,title="第一章第六节 · 打烊对话",timeOfDay="night",pixelArt=true,inlineQuestions=false,
   background="art/tavern/chapter-one-stay-closeup.png",audio="audio/tavern/chapter-six/stay.wav",audioLabel="离线固定角色英文配音",questions=new System.Collections.Generic.List<QuizQuestion>(),decisions=new System.Collections.Generic.List<Decision>(),
   actors=new System.Collections.Generic.List<Actor>{new Actor{id="luchuan",gender="male"},new Actor{id="aelia",gender="female"}},lines=new System.Collections.Generic.List<Line>()};
  var timing=Engine.Json.Deserialize<System.Collections.Generic.List<Line>>(File.ReadAllText(Engine.SafePath(folders[current.id],"audio/tavern/chapter-six/stay.json")));
  for(int i=0;i<zh.Length;i++)stayDialogueChapter.lines.Add(new Line{speaker=speakers[i],actor=speakers[i]=="陆川"?"luchuan":speakers[i]=="艾莉娅"?"aelia":"",text=en[i],translation=zh[i],start=timing[i].start,end=timing[i].end,scene=stayDialogueChapter.background,sceneSingle=true,timeOfDay="night"});
  current=stayDialogueChapter;index=0;ShowStory();
 }
 Chapter stayDialogueChapter,stayOriginalChapter;int stayOriginalIndex;
 bool IsStayDialogue(){return current!=null&&Object.ReferenceEquals(current,stayDialogueChapter);}
 void CompleteStayDialogue(){StopAudio();current=stayOriginalChapter;index=stayOriginalIndex;stayDialogueChapter=null;Attempt().endingShown=true;Persist();CompleteChapterOne();}
}
