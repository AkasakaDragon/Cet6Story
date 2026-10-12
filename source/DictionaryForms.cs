using System;using System.IO;using System.Collections.Generic;

public class DictionaryWordForm {public string lemma{get;set;}public List<string> types{get;set;}}
public partial class Lexicon {
 Dictionary<string,DictionaryWordForm> forms=new Dictionary<string,DictionaryWordForm>(StringComparer.OrdinalIgnoreCase);
 void LoadStudyMeanings(string dictionaryPath){
  string path=Path.Combine(Path.GetDirectoryName(dictionaryPath),"word-meanings.json");
  if(!File.Exists(path))return;
  foreach(var pair in Engine.Json.Deserialize<Dictionary<string,string>>(File.ReadAllText(path))){LexEntry existing;Entries.TryGetValue(pair.Key,out existing);Entries[pair.Key]=new LexEntry{Meaning=pair.Value,Phonetic=existing==null?"":existing.Phonetic,Corrected=true};}
 }
 void LoadForms(string dictionaryPath){
  string path=Path.Combine(Path.GetDirectoryName(dictionaryPath),"dictionary-forms.json");
  if(File.Exists(path))forms=new Dictionary<string,DictionaryWordForm>(Engine.Json.Deserialize<Dictionary<string,DictionaryWordForm>>(File.ReadAllText(path)),StringComparer.OrdinalIgnoreCase);
 }
 LexEntry WithBaseMeaning(string word,LexEntry entry){
  // Explicit reviewed corrections already contain the appropriate common senses.
  if(entry!=null&&entry.Corrected)return entry;
  DictionaryWordForm form;LexEntry original;
  if(!forms.TryGetValue(word,out form)||String.IsNullOrWhiteSpace(form.lemma)||!Entries.TryGetValue(form.lemma,out original)||String.IsNullOrWhiteSpace(original.Meaning))return entry;
  string own=entry==null?"":entry.Meaning;
  string label=word.ToLowerInvariant()+" 是 "+form.lemma+" 的"+String.Join(" / ",form.types??new List<string>())+"。";
  return new LexEntry{Meaning=label+"\n【原形释义】\n"+original.Meaning+(String.IsNullOrWhiteSpace(own)?"":"\n\n【变形词补充释义】\n"+own),Phonetic=entry==null?"":entry.Phonetic,Corrected=true};
 }
}

