using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using System.Drawing;using System.Windows.Forms;
class WordExampleChecks {
 static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Check(bool value,string label){if(!value)throw new Exception(label);}
 static object Call(Game game,string method,params object[] args){return typeof(Game).GetMethod(method,Flags).Invoke(game,args);}
 static IEnumerable<Control> All(Control root){yield return root;foreach(Control child in root.Controls)foreach(var item in All(child))yield return item;}
 static string Pages(Form popup){string result="";var text=All(popup).OfType<OutlinedLabel>().First(x=>x.Dock==DockStyle.Fill);var next=All(popup).OfType<VNButton>().First(x=>x.Text=="下一页");for(int i=0;i<15;i++){string before=text.Text;result+=before+"\n";next.PerformClick();Application.DoEvents();if(before==text.Text)break;}return result;}
 [STAThread]static void Main(string[] args){try{
  var rows=Engine.Json.Deserialize<Dictionary<string,List<BilingualWordExample>>>(File.ReadAllText(Path.Combine(args[0],"assets/word-examples.json")));
  Check(rows.Count>5000,"examples loaded");Check(rows.All(r=>r.Value.Count>=1&&r.Value.Count<=2&&r.Value.All(e=>!String.IsNullOrWhiteSpace(e.english)&&!String.IsNullOrWhiteSpace(e.chinese))),"bilingual pairs complete");
  Check(rows.All(r=>r.Value.Select(e=>e.english.ToLowerInvariant()).Distinct().Count()==r.Value.Count),"English examples deduplicated");
  using(var game=new Game()){
   game.Show();Application.DoEvents();var save=(Save)typeof(Game).GetField("save",Flags).GetValue(game);int wordCount=save.words.Count,coins=save.rogue.coins;
   Check(((List<RogueEntry>)Call(game,"TrainingPool","四级训练")).Count==4371,"CET4 training unchanged");Check(((List<RogueEntry>)Call(game,"TrainingPool","六级训练")).Count==2610,"CET6 training unchanged");
   Check(((List<BilingualWordExample>)Call(game,"ImportedWordExamples","ABILITY")).Count>0,"case-insensitive lookup");
   string meaning="保留原有释义",context="Keep this original contextual example.";
   Call(game,"ShowSpellWord","abandon",meaning,context,"",false);Application.DoEvents();var popup=Application.OpenForms.Cast<Form>().First(f=>f!=game&&f.Text.StartsWith("单词释义"));
   Check(!All(popup).OfType<ScrollableControl>().Any(c=>c.AutoScroll),"popup avoids native scrollbars");var definition=All(popup).OfType<OutlinedLabel>().First(x=>x.Dock==DockStyle.Fill);var footer=All(popup).OfType<FlowLayoutPanel>().First(x=>x.Dock==DockStyle.Bottom);Check(!definition.Bounds.IntersectsWith(footer.Bounds),"definition does not cover pagination buttons");using(var preview=new Bitmap(popup.Width,popup.Height)){popup.DrawToBitmap(preview,new Rectangle(0,0,popup.Width,popup.Height));preview.Save(Path.Combine(args[0],".validation/word-examples-popup.png"));}
   string content=Pages(popup);Check(content.Contains(meaning)&&content.Contains(context),"meaning and contextual example retained");
   foreach(var pair in rows["abandon"])Check(content.Contains(pair.english)&&content.Contains(pair.chinese),"all bilingual examples reachable by paging");
   popup.Size=new Size(520,430);Application.DoEvents();Check(footer.Controls.Cast<Control>().All(c=>footer.ClientRectangle.Contains(c.Bounds)),"all footer buttons fit narrow popup");All(popup).OfType<VNButton>().First(x=>x.Text=="上一页").PerformClick();All(popup).OfType<VNButton>().First(x=>x.Text=="上一页").PerformClick();
   content=Pages(popup);foreach(var pair in rows["abandon"])Check(content.Contains(pair.english)&&content.Contains(pair.chinese),"resized pages retain examples");
   popup.DrawToBitmap(new Bitmap(popup.Width,popup.Height),new Rectangle(0,0,popup.Width,popup.Height));popup.Close();
   Call(game,"AddWord","ability","");Application.DoEvents();popup=Application.OpenForms.Cast<Form>().First(f=>f!=game&&f.Text.StartsWith("单词释义"));content=Pages(popup);Check(content.Contains(rows["ability"][0].english)&&content.Contains(rows["ability"][0].chinese),"story lookup shares bilingual popup");popup.Close();
   var run=new RogueRun{question=new RogueQuestion{entry=new RogueEntry{word="ability",meaning="能力",example="",exampleZh=""}},feedback="回答正确\nability · 能力\n"};
   var originalEntry=Engine.Json.Serialize(run.question.entry);content=(string)Call(game,"RogueFeedbackText",run);Check(content.Contains(rows["ability"][0].english)&&content.Contains(rows["ability"][0].chinese),"feedback includes imported bilingual example");Check(originalEntry==Engine.Json.Serialize(run.question.entry),"feedback does not mutate question entry");
   run.question.entry.example="Original {ability} example.";run.question.entry.exampleZh="原有剧情例句翻译";run.feedback="回答正确\nability · 能力\nOriginal ability example.";content=(string)Call(game,"RogueFeedbackText",run);Check(content.Contains("原有剧情例句翻译")&&!content.Contains(rows["ability"][0].english),"existing training context retains priority");
   Call(game,"ShowSpellWord","zzzmissingexample","仍然可以查词","","",false);Application.DoEvents();popup=Application.OpenForms.Cast<Form>().First(f=>f!=game&&f.Text.StartsWith("单词释义"));Check(Pages(popup).Contains("仍然可以查词"),"unmatched word remains usable");popup.Close();
   Check(save.words.Count==wordCount&&save.rogue.coins==coins,"lookup and feedback preserve saves");game.Close();
  }
  Console.WriteLine("PASS: bilingual data, unchanged training, contextual priority, feedback, saved progress, popup pagination and resize.");
 }catch(Exception error){Console.WriteLine(error);Environment.Exit(1);}}
}
