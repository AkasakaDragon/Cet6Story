using System;using System.Linq;using System.Collections.Generic;
public static class KitchenVocabulary {
 public static readonly string[] Modes={"四级词汇","六级词汇","四六级混合"};
 public static bool ValidMode(string mode){return Modes.Contains(mode);}
 public static List<RogueEntry> Batch(List<RogueEntry> bank,IEnumerable<string> recent,int count,Random random){var usable=bank.Where(w=>!String.IsNullOrWhiteSpace(w.meaning)&&!String.IsNullOrWhiteSpace(w.word)&&w.word.Length>=3&&w.word.All(c=>c>='a'&&c<='z')).GroupBy(w=>w.word).Select(g=>g.First()).ToList();var old=new HashSet<string>(recent??new string[0]);return usable.OrderBy(w=>old.Contains(w.word)?1:0).ThenBy(w=>random.Next()).Take(count).ToList();}
}
public partial class Game {
 List<RogueEntry> KitchenBank(){string mode=KitchenState().wordMode;if(mode=="四级词汇")return TrainingPool("四级训练");if(mode=="六级词汇")return TrainingPool("六级训练");if(mixedExpeditionBank==null)mixedExpeditionBank=TrainingPool("四级训练").Concat(TrainingPool("六级训练")).GroupBy(w=>w.word,StringComparer.OrdinalIgnoreCase).Select(g=>g.First()).ToList();return mixedExpeditionBank;}
 void PrepareKitchenWords(string kind){var state=KitchenState();var words=state.Words(kind);if(words!=null&&words.Count==3)return;if(state.recentWords==null)state.recentWords=new List<string>();words=KitchenVocabulary.Batch(KitchenBank(),state.recentWords,3,new Random(Guid.NewGuid().GetHashCode()));if(words.Count<3)throw new InvalidOperationException("四六级词库不完整。");if(kind=="cut")state.cutWords=words;else state.cookWords=words;state.recentWords.AddRange(words.Select(w=>w.word));while(state.recentWords.Count>30)state.recentWords.RemoveAt(0);Persist();}
}
