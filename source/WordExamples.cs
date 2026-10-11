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
  int number=0;foreach(var example in imported){if(usableContext&&example.english==context)continue;parts.Add("【例句 "+(++number)+"】\n"+example.english+"\n"+example.chinese);}
  return String.Join("\n\n",parts);
 }
 string ImportedExampleFeedback(string word){var example=ImportedWordExamples(word).FirstOrDefault();return example==null?"":"例句："+example.english+"\n例句译文："+example.chinese;}
}
