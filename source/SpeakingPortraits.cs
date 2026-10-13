using System;
using System.IO;
using System.Linq;

public static class DialogueExpressions {
 public static string For(Line line){
  if(line==null)return "neutral";
  string[] valid={"neutral","angry","confused","surprised","sad","serious","happy"};
  if(valid.Contains(line.expression))return line.expression;
  string t=line.translation??"";
  if(new[]{"住手","不许","混蛋","滚开"}.Any(t.Contains))return "angry";
  if(new[]{"怎么会","不可能","天哪","吓"}.Any(t.Contains))return "surprised";
  if(new[]{"难过","失去","遗憾","哭"}.Any(t.Contains))return "sad";
  if(new[]{"谢谢","太好了","哈哈","高兴","欢迎"}.Any(t.Contains))return "happy";
  if(t.Contains("？"))return "confused";
  return "serious";
 }
}
public partial class Game {
 void UpdateSpeakingPortraits(Line line){
  if(stage==null)return;
  bool changed=false;
  foreach(var actor in stage.Actors){
   bool visible=!line.hidePortraits&&!String.IsNullOrEmpty(line.actor)&&actor.Id==line.actor&&(!PrologueV2.Is(current)||PrologueV2.Visible(line,actor.Id));
   if(actor.Visible!=visible){actor.Visible=visible;changed=true;}
   string id=actor.Id=="noah"?"noya":actor.Id=="lyra"?"dagger":actor.Id;
   string file=Path.Combine(root,"assets","characters","dialogue-v2",id,DialogueExpressions.For(line)+".png");
   if(File.Exists(file)){var picture=CachedImage(file);if(actor.Image!=picture){actor.Image=picture;changed=true;}}
  }
  if(changed)stage.Snap();
 }
}
