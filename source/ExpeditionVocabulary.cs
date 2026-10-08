using System;using System.Linq;using System.Collections.Generic;
public class ExpeditionWordProgress {
 public int streak{get;set;} public int stage{get;set;} public string due{get;set;} public int lastQuestion{get;set;} public bool failed{get;set;}
}
public class ExpeditionLearning {
 public string mode{get;set;} public int serial{get;set;} public Dictionary<string,ExpeditionWordProgress> words{get;set;} public List<string> active{get;set;}
 public int reviewQuestionSerial{get;set;} public int learned{get;set;} public int reviewed{get;set;}
 public ExpeditionLearning(){mode="四六级混合";words=new Dictionary<string,ExpeditionWordProgress>();active=new List<string>();}
 public void Normalize(){if(words==null)words=new Dictionary<string,ExpeditionWordProgress>();if(active==null)active=new List<string>();if(String.IsNullOrEmpty(mode))mode="四六级混合";}
}
public static class ExpeditionVocabulary {
 public static bool Correct(RogueQuestion q,int selected,string spelling){return q.kind<0||(q.kind==3?String.Equals((spelling??"").Trim(),q.entry.word,StringComparison.OrdinalIgnoreCase):selected==q.answer);}
 public static string Meaning(RogueEntry e){var first=(e.meaning??"").Split(new[]{';','；'})[0];return String.Join("，",first.Split(new[]{',','，','、'}).Take(2).ToArray());}
 static readonly int[] Days={1,3,7,14,30};
 public static ExpeditionLearning Profile(RogueProfile p){if(p.expeditionLearning==null)p.expeditionLearning=new ExpeditionLearning();p.expeditionLearning.Normalize();return p.expeditionLearning;}
 public static ExpeditionWordProgress Progress(RogueProfile p,string word,DateTime today){var l=Profile(p);ExpeditionWordProgress m;if(!l.words.TryGetValue(word,out m)){m=new ExpeditionWordProgress{lastQuestion=-100};RogueMemory old;if(p.memory.TryGetValue(word,out old)&&old!=null&&old.correct>=3){m.stage=1;m.streak=3;m.due=today.ToString("yyyy-MM-dd");}l.words[word]=m;}return m;}
 public static bool Due(ExpeditionWordProgress m,DateTime today){DateTime date;return m.stage>0&&m.stage<=5&&(!DateTime.TryParse(m.due,out date)||date.Date<=today.Date);}
 public static RogueQuestion Next(RogueProfile p,List<RogueEntry> bank,DateTime today){
  var l=Profile(p);var rng=new Random(Guid.NewGuid().GetHashCode());var entries=bank.Where(w=>!String.IsNullOrWhiteSpace(w.word)&&!String.IsNullOrWhiteSpace(w.meaning)).GroupBy(w=>w.word,StringComparer.OrdinalIgnoreCase).Select(g=>g.First()).ToList();
  foreach(var key in l.words.Where(pair=>pair.Value.stage==0&&pair.Value.streak==0&&!pair.Value.failed&&pair.Value.lastQuestion==-100).Select(pair=>pair.Key).ToList())l.words.Remove(key);
  var states=new Dictionary<string,ExpeditionWordProgress>();foreach(var w in entries){ExpeditionWordProgress value;RogueMemory old;if(!l.words.TryGetValue(w.word,out value))value=p.memory.TryGetValue(w.word,out old)&&old!=null&&old.correct>=3?Progress(p,w.word,today):new ExpeditionWordProgress{lastQuestion=-100};states[w.word]=value;}
  var availableWords=new HashSet<string>(entries.Select(w=>w.word));
  l.active=l.active.Where(w=>availableWords.Contains(w)&&states[w].stage==0).Distinct().ToList();
  // Keep a small rolling learning batch; due reviews are selected independently.
  var fresh=entries.Where(w=>states[w.word].stage==0&&!l.active.Contains(w.word)).OrderBy(w=>states[w.word].failed?0:1).ThenBy(w=>rng.Next()).Take(Math.Max(0,20-l.active.Count));l.active.AddRange(fresh.Select(w=>w.word));
  var eligible=entries.Where(w=>{var m=states[w.word];return (l.active.Contains(w.word)||m.failed||Due(m,today))&&l.serial-m.lastQuestion>=(m.failed?6:9);}).ToList();
  var entry=eligible.OrderBy(w=>states[w.word].failed?0:(l.serial%3==2?states[w.word].stage==0:Due(states[w.word],today))?1:2).ThenBy(w=>states[w.word].lastQuestion).ThenBy(w=>rng.Next()).FirstOrDefault();
  if(entry==null)return new RogueQuestion{entry=new RogueEntry{word="今日学习完成",meaning="当前没有到期词汇，继续战斗"},kind=-1,options=new List<string>{"继续施放 / 抵御","继续施放 / 抵御","继续施放 / 抵御","继续施放 / 抵御"},answer=0};
  var memory=Progress(p,entry.word,today);int kind=memory.stage>0?new[]{0,1,3}[l.reviewQuestionSerial++%3]:(memory.streak>=2?3:memory.streak%2);memory.lastQuestion=l.serial;if(kind==3){RogueEngine.RecordWordAppearance(p,entry.word);return new RogueQuestion{entry=entry,kind=3,options=new List<string>(),answer=-1};}var options=entries.Where(w=>w.word!=entry.word&&(kind==0?Meaning(w)!=Meaning(entry):true)).OrderBy(w=>rng.Next()).Select(w=>kind==0?Meaning(w):w.word).Distinct().Take(3).ToList();options.Add(kind==0?Meaning(entry):entry.word);options=options.OrderBy(w=>rng.Next()).ToList();memory.lastQuestion=l.serial;RogueEngine.RecordWordAppearance(p,entry.word);return new RogueQuestion{entry=entry,kind=kind,options=options,answer=options.IndexOf(kind==0?Meaning(entry):entry.word)};
 }
 public static string Answer(RogueProfile p,RogueQuestion q,bool correct,DateTime today){var l=Profile(p);l.serial++;if(q.kind<0)return "今日暂无待学词 · 战斗继续";var m=Progress(p,q.entry.word,today);if(!correct){m.streak=0;m.stage=0;m.due=null;m.failed=true;return "待巩固 · 隔5道其他题后重试";}m.failed=false;if(m.stage>0){l.reviewed++;m.stage++;if(m.stage>5){m.due=null;return "已长期掌握 · 退出自动出题";}m.due=today.AddDays(Days[m.stage-1]).ToString("yyyy-MM-dd");return "复习通过 · 下次 "+m.due;}m.streak++;if(m.streak<3)return "本轮掌握 "+m.streak+" / 3";m.stage=1;m.due=today.AddDays(1).ToString("yyyy-MM-dd");l.active.Remove(q.entry.word);l.learned++;return "今日已学会 · 明日复习";}
 public static string Status(RogueProfile p,RogueQuestion q,DateTime today){if(q.kind<0)return "暂无到期词 · 继续战斗";var m=Progress(p,q.entry.word,today);return m.stage>0?"到期复习 · 第 "+m.stage+" / 5 阶段":"本轮掌握 "+m.streak+" / 3";}
}
public partial class Game {
 List<RogueEntry> mixedExpeditionBank;
 List<RogueEntry> ExpeditionBank(){var mode=ExpeditionVocabulary.Profile(save.rogue).mode;if(mode=="四级词汇")return TrainingPool("四级训练");if(mode=="六级词汇")return TrainingPool("六级训练");if(mixedExpeditionBank==null)mixedExpeditionBank=TrainingPool("四级训练").Concat(TrainingPool("六级训练")).GroupBy(w=>w.word,StringComparer.OrdinalIgnoreCase).Select(g=>g.First()).ToList();return mixedExpeditionBank;}
}
