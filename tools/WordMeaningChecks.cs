using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using System.Windows.Forms;
class WordMeaningChecks {
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
 static IEnumerable<Control> All(Control root){yield return root;foreach(Control child in root.Controls)foreach(var item in All(child))yield return item;}
 [STAThread]static void Main(string[] args){try{
  string root=args[0];var lex=Lexicon.Load(Path.Combine(root,"assets/dictionary.tsv.gz"));
  foreach(var sample in new[]{new[]{"weeks","周"},new[]{"ideas","想法"},new[]{"cars","汽车"},new[]{"pets","宠物"},new[]{"gets","得到"},new[]{"minutes","分钟"},new[]{"seconds","秒"},new[]{"gdp","国内生产总值"},new[]{"affordable","负担得起"},new[]{"faring","进展"}}){var entry=lex.Find(sample[0]);Check(entry!=null&&entry.Corrected&&entry.Meaning.Contains(sample[1]),sample[0]+" resolves its common meaning");}
  Check(lex.Find("WEEKS").Meaning==lex.Find("weeks").Meaning,"case-insensitive corrected lookup");
  Check(lex.Find("wilson").Meaning.Contains("威尔逊")&&!lex.Find("wilson").Corrected,"real proper names retain their meanings");
  using(var game=new Game()){
   game.Show();Application.DoEvents();var flags=BindingFlags.Instance|BindingFlags.NonPublic;var save=(Save)typeof(Game).GetField("save",flags).GetValue(game);save.words.Add(new Word{text="weeks",meaning="n. 威克斯（姓氏）",example="two weeks"});
   typeof(Game).GetMethod("HallWord",flags).Invoke(game,new object[]{"weeks","two weeks"});Application.DoEvents();
   var popup=Application.OpenForms.Cast<Form>().First(f=>f!=game&&f.Text.StartsWith("单词释义"));
   Check(All(popup).Any(c=>c.Text.StartsWith("n. 周、星期")),"saved stale surname meaning cannot obscure corrected lookup");
   popup.Close();game.Close();
  }
  Console.WriteLine("PASS: common plural/verb meanings, time units, GDP, genuine names, case-insensitive lookup and old-favorite popup.");
 }catch(Exception e){Console.WriteLine(e);Environment.Exit(1);}}
}
