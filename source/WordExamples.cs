using System;using System.IO;using System.Linq;using System.Collections.Generic;

public class BilingualWordExample {
 public string english{get;set;}public string chinese{get;set;}
}
public partial class Game {
 Dictionary<string,List<BilingualWordExample>> wordExamples;
 List<BilingualWordExample> ImportedWordExamples(string word){
  if(wordExamples==null){
   var path=Path.Combine(root,"assets","word-examples.json");
   wordExamples=File.Exists(path)?Engine.Json.Deserialize<Dictionary<string,List<BilingualWordExample>>>(File.ReadAllText(path)):new Dictionary<string,List<BilingualWordExample>>();
  }
  List<BilingualWordExample> result;
  return wordExamples.TryGetValue((word??"").Trim().ToLowerInvariant(),out result)?result:new List<BilingualWordExample>();
 }
 string PlainWordExample(string word,string example){return (example??"").Replace("{"+word+"}",word).Trim();}
 string ContextWordExampleTranslation(string word,string example){
  var imported=ImportedWordExamples(word).FirstOrDefault(e=>e.english==example);if(imported!=null)return imported.chinese;
  if(current!=null&&current.lines!=null){var line=current.lines.FirstOrDefault(l=>l.text==example);if(line!=null&&ChineseAllowed())return line.translation??"";}
  return "";
 }
 string WordExamplesText(string word,string context){
  var parts=new List<string>();context=PlainWordExample(word,context);
  bool usableContext=!String.IsNullOrWhiteSpace(context)&&!context.StartsWith("Practice the word");
  var imported=ImportedWordExamples(word);
  if(usableContext){string zh=ContextWordExampleTranslation(word,context);parts.Add("【当前例句】\n"+context+(String.IsNullOrWhiteSpace(zh)?"":"\n"+zh));}
  if(!usableContext){var example=imported.OrderBy(e=>e.english.Length+e.chinese.Length).FirstOrDefault();if(example!=null)parts.Add("【例句】\n"+example.english+"\n"+example.chinese);}
  return String.Join("\n\n",parts);
 }
 string ImportedExampleFeedback(string word){var example=ImportedWordExamples(word).FirstOrDefault();return example==null?"":"例句："+example.english+"\n例句译文："+example.chinese;}
 string SavedWordMeaning(Word word){var found=lexicon.Find(word.text);return found!=null&&found.Corrected?found.Meaning:word.meaning;}
 string StudyPromptMeaning(RogueEntry entry){var found=lexicon==null?null:lexicon.Find(entry.word);return ExpeditionVocabulary.Meaning(found!=null&&found.Corrected?new RogueEntry{meaning=found.Meaning}:entry);}
 string WordPhonetic(string word){var entry=lexicon==null?null:lexicon.Find(word);return entry==null||String.IsNullOrWhiteSpace(entry.Phonetic)?"":"/"+entry.Phonetic.Trim().Trim('/')+"/";}
 string VocabularySettlementText(RogueEntry entry){string sound=WordPhonetic(entry.word);return (sound.Length==0?"":sound+"\n\n")+VocabularySettlementDetails(entry);}
 string VocabularySettlementDetails(RogueEntry entry){
  var corrected=lexicon==null?null:lexicon.Find(entry.word);
  string text=(corrected!=null&&corrected.Corrected?corrected.Meaning:entry.meaning)??"";
  text=text.Replace("\\n","\n");
  if(!String.IsNullOrWhiteSpace(entry.example)&&!entry.example.StartsWith("Practice the word")){
   text+="\n\n例句："+PlainWordExample(entry.word,entry.example);if(!String.IsNullOrWhiteSpace(entry.exampleZh))text+="\n例句译文："+entry.exampleZh;
  }else{string example=ImportedExampleFeedback(entry.word);if(!String.IsNullOrWhiteSpace(example))text+="\n\n"+example;}
  return text;
 }
}
