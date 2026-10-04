using System;using System.IO;using System.Linq;using System.Collections.Generic;
public partial class Game {
 // Refresh only sentence content, preserving question type, answer choices and learning progress.
 bool RefreshStoryExamples(RogueRun run){
  if(run==null||String.IsNullOrEmpty(run.vocabularyChapter))return false;
  if(sectionVocabulary==null)sectionVocabulary=Engine.Json.Deserialize<Dictionary<string,List<RogueEntry>>>(File.ReadAllText(Path.Combine(root,"assets","section-vocabulary.json")));
  List<RogueEntry> entries;if(!sectionVocabulary.TryGetValue(run.vocabularyChapter,out entries))return false;
  var currentEntries=entries.ToDictionary(e=>e.word,StringComparer.OrdinalIgnoreCase);bool changed=false;
  var savedEntries=(run.pool??new List<RogueEntry>()).ToList();if(run.question!=null&&run.question.entry!=null)savedEntries.Add(run.question.entry);
  foreach(var old in savedEntries){RogueEntry updated;if(old==null||!currentEntries.TryGetValue(old.word,out updated)||old.example==updated.example&&old.exampleZh==updated.exampleZh)continue;
   if(run.question!=null&&Object.ReferenceEquals(old,run.question.entry)&&!String.IsNullOrEmpty(run.feedback)&&!String.IsNullOrEmpty(old.example))run.feedback=run.feedback.Replace(old.example.Replace("{"+old.word+"}",old.word),updated.example.Replace("{"+updated.word+"}",updated.word));
   old.example=updated.example;old.exampleZh=updated.exampleZh;changed=true;
  }
  foreach(var word in save.words){RogueEntry updated;if(word!=null&&(word.example??"").StartsWith("Practice the word")&&currentEntries.TryGetValue(word.text,out updated)){word.example=updated.example.Replace("{"+updated.word+"}",updated.word);changed=true;}}
  return changed;
 }
}
