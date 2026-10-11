using System;using System.Collections.Generic;using System.Linq;using System.Text.RegularExpressions;

// Keep a spoken question number and its text on the same subtitle and replay unit.
public static class ListeningCaptionIntegrity {
 public static bool HasCompleteQuestions(RealListeningExam exam,RealListeningGroup group){
  if(exam.lines==null||exam.questions==null)return false;
  var questions=exam.questions.Where(q=>q.group_id==group.id).ToList();
  return questions.Count==group.last-group.first+1&&questions.All(q=>exam.lines.Any(l=>l.text==q.number+". "+q.prompt&&l.start>=group.start-.2&&l.end<=group.end+.001&&l.end>l.start));
 }
 public static bool IsNumber(string text){return Regex.IsMatch((text??"").Trim(),@"^(?:Question\s*)?\d+[.:]?$",RegexOptions.IgnoreCase);}
 public static List<Line> Normalize(List<Line> source,List<RealListeningGroup> groups){
  var result=new List<Line>();if(source==null)return result;
  foreach(var line in source){
   if(result.Count>0&&IsNumber(result[result.Count-1].text)){
    var previous=result[result.Count-1];var group=groups==null?null:groups.FirstOrDefault(g=>g.start-.5<=previous.start&&previous.start<g.end);
    if(line.start>=previous.end-.001&&line.start-previous.end<5&&(group==null||line.start<group.end)){
     previous.text=previous.text.Trim()+" "+line.text;previous.translation="第 "+Regex.Match(previous.text,@"\d+").Value+" 题："+line.translation;previous.end=line.end;continue;
    }
    result.RemoveAt(result.Count-1);
   }
   result.Add(new Line{speaker=line.speaker,actor=line.actor,text=line.text,translation=line.translation,start=line.start,end=line.end});
  }
  if(result.Count>0&&IsNumber(result[result.Count-1].text))result.RemoveAt(result.Count-1);
  return result;
 }
}
