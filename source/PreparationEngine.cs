using System;
using System.Linq;
using System.Collections.Generic;

public class SectionPreparation {
 public Dictionary<string,int> masks {get;set;} public List<string> introduced {get;set;} public bool rewarded {get;set;} public bool cleared {get;set;}
 public SectionPreparation(){masks=new Dictionary<string,int>();introduced=new List<string>();}
 public void Normalize(){if(masks==null)masks=new Dictionary<string,int>();if(introduced==null)introduced=new List<string>();}
}
public static class PreparationEngine {
 public static SectionPreparation Progress(RogueProfile p,string chapter){p.Normalize();SectionPreparation progress;if(!p.preparations.TryGetValue(chapter,out progress)){progress=new SectionPreparation();p.preparations[chapter]=progress;}progress.Normalize();return progress;}
 public static int Mask(SectionPreparation p,string word){int value;return p.masks.TryGetValue(word,out value)?value:0;}
 public static bool Mastered(int mask){return mask!=0&&(mask&(mask-1))!=0;}
 public static int Count(RogueProfile p,string chapter,List<RogueEntry> words){var progress=Progress(p,chapter);return words.Count(w=>Mastered(Mask(progress,w.word)));}
 public static bool Complete(RogueProfile p,string chapter,List<RogueEntry> words){var progress=Progress(p,chapter);RogueRun run;if(p.preparationRuns.TryGetValue(chapter,out run)&&run!=null&&run.won&&run.vocabularyChapter==chapter)progress.cleared=true;if(p.prepSession!=null&&p.prepSession.won&&p.prepSession.vocabularyChapter==chapter)progress.cleared=true;return words.Count>=4&&(progress.cleared||Count(p,chapter,words)==words.Count);}
 public static void Next(RogueProfile p,string chapter,List<RogueEntry> words){
  var r=p.prepSession;var progress=Progress(p,chapter);var rng=new Random(r.seed+(++r.serial)*7919);
  var candidates=words.Where(w=>!Mastered(Mask(progress,w.word))).OrderBy(w=>Mask(progress,w.word)==0?0:1).ThenBy(w=>rng.Next()).ToList();if(candidates.Count==0)return;
  var entry=candidates.FirstOrDefault(w=>!r.recent.Contains(w.word))??candidates[0];int mask=Mask(progress,entry.word);int kind=mask==0?0:((mask&2)==0?1:2);
  if(mask!=0&&rng.Next(2)==0&&!entry.example.StartsWith("Practice the word"))kind=2;if((mask&(1<<kind))!=0)kind=(mask&2)==0?1:0;
  if(!RogueEngine.SeenWord(p,entry.word))kind=0;var q=new RogueQuestion{entry=entry,kind=kind,options=new List<string>()};q.options=words.Where(w=>w.word!=entry.word&&(kind!=0||w.meaning!=entry.meaning)).OrderBy(w=>rng.Next()).Select(w=>kind==0?w.meaning:w.word).Distinct().Take(3).ToList();q.options.Add(kind==0?entry.meaning:entry.word);q.options=q.options.OrderBy(w=>rng.Next()).ToList();q.answer=q.options.IndexOf(kind==0?entry.meaning:entry.word);RogueEngine.RecordWordAppearance(p,entry.word);r.question=q;r.state="combat";r.recent.Add(entry.word);while(r.recent.Count>3)r.recent.RemoveAt(0);
 }
 public static bool Answer(RogueProfile p,string chapter,List<RogueEntry> words,int selected,DateTime now){
  var r=p.prepSession;if(r==null||r.state!="combat"||r.question.answered)return false;var q=r.question;var old=p.run;bool answered;
  try{p.run=r;answered=RogueEngine.Answer(p,selected,null,now);}finally{p.run=old;}
  if(!answered)return false;var progress=Progress(p,chapter);
  if(selected==q.answer&&!q.assisted){progress.masks[q.entry.word]=Mask(progress,q.entry.word)|(1<<q.kind);if(Mastered(progress.masks[q.entry.word]))r.hp=Math.Min(r.maxHp,r.hp+8);}
  if(Complete(p,chapter,words)&&!progress.rewarded){progress.rewarded=true;p.coins+=60;r.earnedCoins+=60;r.feedback+="\n本节词汇准备完成 · 首次奖励 +60 金币";}
  return true;
 }
}

