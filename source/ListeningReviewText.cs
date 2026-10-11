using System;
using System.Linq;
using System.Text.RegularExpressions;

public static class ListeningReviewText {
 public static string Format(RealListeningQuestion question,int index,int count,bool review){
  string header="第 "+question.number+" 题 · "+(index+1)+" / "+count+"\n";
  string options=String.Join("\n",question.options.Select((text,i)=>((char)(65+i))+". "+text));
  if(!review)return header+options;
  // The source explanation already supplies the translated question and detailed answer.
  string explanation=Regex.Replace(question.explanation??"",@"^\s*【题目】[^\r\n]*(?:\r?\n\s*)?","").Trim();
  string answer=explanation.Contains("【答案】")?"":"正确答案："+(char)(65+question.answer)+"\n";
  return header+question.prompt+"\n"+options+"\n"+answer+explanation;
 }
}
