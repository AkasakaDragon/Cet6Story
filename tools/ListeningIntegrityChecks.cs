using System;using System.IO;using System.Linq;using System.Drawing;using System.Collections.Generic;using System.Windows.Forms;
class ListeningIntegrityChecks {
 static void Check(bool ok,string reason){if(!ok)throw new Exception(reason);}
 [STAThread]static void Main(string[] args){try{Run(args[0]);}catch(Exception e){Console.WriteLine(e);Environment.Exit(1);}}
 static void Run(string root){
  var source=new List<Line>{new Line{text="3.",translation="3 个",start=10,end=10.5},new Line{text="What does the speaker suggest?",translation="说话者提出了什么建议？",start=11,end=14}};
  var merged=ListeningCaptionIntegrity.Normalize(source,new List<RealListeningGroup>{new RealListeningGroup{start=0,end=20}});
  Check(merged.Count==1&&merged[0].text=="3. What does the speaker suggest?","number and prompt share one replay unit");
  Check(merged[0].translation=="第 3 题：说话者提出了什么建议？"&&merged[0].start==10&&merged[0].end==14,"translation and spoken number timing retained");
  Check(source[0].text=="3.","source rows are not mutated");
  Check(ListeningCaptionIntegrity.Normalize(source,new List<RealListeningGroup>{new RealListeningGroup{start=0,end=10.8},new RealListeningGroup{start=11,end=20}}).Count==1,"number cannot attach across question groups");
  int exams=0,questions=0;
  foreach(var file in Directory.GetFiles(Path.Combine(root,"assets/tavern/listening/real-exams/cettong"),"*.resources.json",SearchOption.AllDirectories)){
   var exam=Engine.Json.Deserialize<RealListeningExam>(File.ReadAllText(file));var captions=Engine.Json.Deserialize<RealListeningExam>(File.ReadAllText(file.Replace(".resources.json",".captions.json")));
   var lines=ListeningCaptionIntegrity.Normalize(captions.lines,exam.groups);Check(!lines.Any(l=>ListeningCaptionIntegrity.IsNumber(l.text)),exam.id+" standalone number");
   exam.lines=lines;Check(exam.groups.All(g=>ListeningCaptionIntegrity.HasCompleteQuestions(exam,g)),exam.id+" every playable group has all spoken questions");
   foreach(var q in exam.questions){
    string text=q.number+". "+q.prompt;var line=lines.Single(l=>l.text==text);var group=exam.groups.Single(g=>g.id==q.group_id);
    Check(line.start>=group.start-.2&&line.end<=group.end+.001,exam.id+" question outside playable group "+q.number);
    var playback=lines.Where(l=>l.start>=group.start-.2&&l.start<group.end).ToList();int at=playback.IndexOf(line);
    Check(at>=0&&Math.Min(SpellPracticeAudio.SentenceEndMs(playback,at),(long)(group.end*1000))>=(long)(line.end*1000),"replay includes complete question");
    foreach(int width in new[]{760,1240})using(var subtitle=new HallSentencePanel{Width=width}){
     subtitle.Height=subtitle.DialogueHeight(line.text,line.translation,true,true)-60;subtitle.SetSentence(line.text,line.translation,true,true);
     Check(subtitle.PageCount==1&&subtitle.English.Text==text,"question must not paginate its number separately: "+exam.id+"/"+q.number);
     Check(subtitle.English.Height>=8+subtitle.English.WrappedLineCount(subtitle.English.Width-22)*(int)Math.Ceiling(subtitle.English.Font.Height*1.55),"question English fits");
     if(exam.id=="cet4-2025_12_2"&&q.number==3&&width==1240)using(var bitmap=new Bitmap(subtitle.Width,subtitle.Height)){subtitle.DrawToBitmap(bitmap,subtitle.ClientRectangle);bitmap.Save(Path.Combine(root,".validation/listening-question-fixed.png"));}
    }
    questions++;
   }
   exams++;
  }
  Console.WriteLine("PASS: "+exams+" exams, "+questions+" complete question replay units; no isolated numbers, no cross-group merging; subtitles fit 800/1280 layouts.");
 }
}
