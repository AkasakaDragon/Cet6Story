using System;using System.IO;using System.Linq;using System.Collections.Generic;
class DictionaryFormChecks {
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
 static void Main(string[] args){try{
  string root=args[0];var lex=Lexicon.Load(Path.Combine(root,"assets/dictionary.tsv.gz"));var forms=Engine.Json.Deserialize<Dictionary<string,DictionaryWordForm>>(File.ReadAllText(Path.Combine(root,"assets/dictionary-forms.json")));
  foreach(var pair in forms){var found=lex.Find(pair.Key);Check(found!=null&&found.Corrected,"supplemented form not found: "+pair.Key);LexEntry own;if(lex.Entries.TryGetValue(pair.Key,out own)){Check(found.Phonetic==own.Phonetic,"inflection pronunciation changed: "+pair.Key);Check(found.Meaning.Contains(own.Meaning),"own meaning lost: "+pair.Key);if(own.Corrected)Check(found.Meaning==own.Meaning,"reviewed correction overwritten: "+pair.Key);else Check(found.Meaning.Contains(lex.Entries[pair.Value.lemma].Meaning),"base meaning missing: "+pair.Key);}}
  foreach(var pair in new[]{new[]{"fried","煎"},new[]{"seasoned","调味"},new[]{"whipped","搅打"},new[]{"scrambled","炒鸡蛋"},new[]{"went","go"},new[]{"were","be"},new[]{"colored","color"}})Check(lex.Find(pair[0]).Meaning.Contains(pair[1]),"regression: "+pair[0]);
  foreach(string word in new[]{"news","james","weed","inexperienced"}){LexEntry own;if(lex.Entries.TryGetValue(word,out own))Check(lex.Find(word).Meaning==own.Meaning,"unsupported or misleading stem: "+word);}
  Check(lex.Find("SEASONED").Meaning==lex.Find("seasoned").Meaning,"case-insensitive morphology");var inherited=lex.Find("seasoned");lex.Entries["season"].Meaning="调味的已修正原形释义";Check(lex.Find("seasoned").Meaning.Contains("已修正原形释义"),"base corrections do not reach inflections");Check(lex.Find("nonesuchzzzz")==null,"invented fallback");
  Console.WriteLine("PASS: "+forms.Count+" forms preserve own meanings, exact phonetics and corrections; cooking, irregular verbs, ambiguous roots and corrected-base inheritance.");
 }catch(Exception e){Console.WriteLine(e.Message);Environment.Exit(1);}}
}
