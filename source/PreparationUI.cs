using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public partial class Game {
 Dictionary<string,List<RogueEntry>> sectionVocabulary;
 List<RogueEntry> PreparationWords(Chapter chapter=null){chapter=chapter??current;if(sectionVocabulary==null)sectionVocabulary=Engine.Json.Deserialize<Dictionary<string,List<RogueEntry>>>(File.ReadAllText(Path.Combine(root,"assets","section-vocabulary.json")));List<RogueEntry> words;return sectionVocabulary.TryGetValue(chapter.id,out words)?words:new List<RogueEntry>();}
 bool PreparationReady(Chapter chapter){var words=PreparationWords(chapter);return words.Count==0?!StoryRoutes.Enhanced(chapter.id):PreparationEngine.Complete(save.rogue,chapter.id,words);}


 void ShowPreparationHome(){StartPreparation();}
 void StartPreparation(){
  var p=save.rogue;p.Normalize();var words=PreparationWords();if(words.Count<4){GameMessage.Show(this,"本节词库不足四个词，无法开始远征。","本节词汇准备");return;}
  if(p.prepSession!=null&&!String.IsNullOrEmpty(p.prepChapter)&&TowerEngine.IsTower(p.prepSession))p.preparationRuns[p.prepChapter]=p.prepSession;
  RogueRun run;p.preparationRuns.TryGetValue(current.id,out run);
  if(run==null||run.state=="ended"){
   var ordinary=p.run;try{run=RogueEngine.NewRun(p,words,"本节词汇准备",Environment.TickCount);run.vocabularyChapter=current.id;}finally{p.run=ordinary;}
   p.preparationRuns[current.id]=run;
  }
  p.prepChapter=current.id;p.prepSession=run;p.preparationActive=true;save.lastChapter=current.id;save.hasGame=true;Persist();RenderRogue();
 }
 void RenderPreparation(){RenderRogue();}
 void ContinuePreparation(){RogueEngine.Continue(save.rogue);SaveRogue();}
 void AnswerPreparation(int selected){AnswerRogue(selected,null);}
}
