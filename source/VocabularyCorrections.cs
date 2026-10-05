using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class VocabularyCorrection {
 public string word {get;set;} public string meaning {get;set;} public List<string> previous {get;set;}
}
public static class VocabularyCorrections {
 static Dictionary<string,VocabularyCorrection> corrections;
 static Dictionary<string,VocabularyCorrection> Entries {
  get {
   if(corrections==null){
    string path=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"assets","vocabulary-corrections.json");
    var list=File.Exists(path)?Engine.Json.Deserialize<List<VocabularyCorrection>>(File.ReadAllText(path)):new List<VocabularyCorrection>();
    corrections=list.ToDictionary(x=>x.word,StringComparer.OrdinalIgnoreCase);
   }
   return corrections;
  }
 }
 static string Correct(string word,string meaning){VocabularyCorrection c;return word!=null&&Entries.TryGetValue(word,out c)&&c.previous!=null&&c.previous.Contains(meaning)?c.meaning:meaning;}
 public static bool Apply(Save save){
  bool changed=false;
  foreach(var w in save.words){string value=Correct(w.text,w.meaning);if(value!=w.meaning){w.meaning=value;changed=true;}}
  var p=save.rogue;if(p==null)return changed;
  var runs=new List<RogueRun>{p.run,p.prepSession};if(p.preparationRuns!=null)runs.AddRange(p.preparationRuns.Values);
  foreach(var r in runs.Where(x=>x!=null).Distinct()){
   var q=r.question;
   // Meaning options must migrate with the entry, without changing the answer index.
   if(q!=null&&q.kind==0&&q.options!=null){
    for(int i=0;i<q.options.Count;i++){
     string old=q.options[i],value=old;
     if(r.pool!=null){var entry=r.pool.FirstOrDefault(x=>x.meaning==old);if(entry!=null)value=Correct(entry.word,old);}
     if(i==q.answer&&q.entry!=null)value=Correct(q.entry.word,old);
     if(value!=old){q.options[i]=value;changed=true;}
    }
   }
   var entries=r.pool==null?new List<RogueEntry>():new List<RogueEntry>(r.pool);if(q!=null&&q.entry!=null)entries.Add(q.entry);
   foreach(var e in entries){string old=e.meaning,value=Correct(e.word,old);if(value!=old){e.meaning=value;changed=true;if(r.feedback!=null)r.feedback=r.feedback.Replace(e.word+" · "+old,e.word+" · "+value);}}
  }
  return changed;
 }
}
