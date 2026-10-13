using System;
public static class StoryCgNavigation {
 public static string PreviousCg(Chapter chapter,int line){
  if(WaystationChapterOne.Is(chapter)&&line==0)return "chapter-intro";
  if(PrologueV2.Is(chapter))return line==0?"v2-cg01":String.IsNullOrEmpty(chapter.lines[line].cgBefore)?null:"v2-"+chapter.lines[line].cgBefore;
  if(TavernStory.Is(chapter)){
   if(line==0)return "tavern-opening";
   if(line==TavernStory.TransferLine)return "goddess-transfer";
   if(line==TavernStory.EntranceLine+1)return "knight-entrance";
  }return null;
 }
}
public partial class Game {
 bool TryReplayPreviousCg(){
  string previousCg=StoryCgNavigation.PreviousCg(current,index);if(previousCg!=null&&previousCg.StartsWith("v2-")){ShowNewPrologueCg(previousCg.Substring(3),index);return true;}
  switch(previousCg){
   case "chapter-intro":PlayChapterOneCg(true);return true;
   case "tavern-opening":ShowTavernOpening();return true;
   case "goddess-transfer":ShowGoddessTransfer();return true;
   case "knight-entrance":ShowKnightEntrance();return true;
   default:return false;
  }
 }
}
