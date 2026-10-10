using System;using System.Linq;using System.Collections.Generic;
public static class MineStoryBattle {
 public const string ChapterId="tavern-01-05",RunId="mine-first-encounter",MigrationFlag="mine-story-battle-v1";
 public const int Line=6;
 public static bool Is(Chapter c){return c!=null&&c.id==ChapterId;}
 public static int Remap(int old){return old<7?old:old<11?6:old-4;}
 public static void Migrate(Save s){
  if(s.storyFlags.Contains(MigrationFlag))return;
  if(s.positions.ContainsKey(ChapterId))s.positions[ChapterId]=Remap(s.positions[ChapterId]);
  string prefix=ChapterId+"/line/";var heard=s.heardLines.Where(k=>k.StartsWith(prefix)).ToList();s.heardLines.RemoveAll(k=>k.StartsWith(prefix));
  foreach(var k in heard){int n;if(int.TryParse(k.Substring(prefix.Length),out n)&&(n<7||n>=11))s.heardLines.Add(prefix+Remap(n));}
  Shift(s.quizAnswers);Shift(s.answerStarted);Shift(s.answerTimely);
  SectionAttempt a;if(s.sectionAttempts.TryGetValue(ChapterId,out a)){if(a.replayAfterLine>=0)a.replayAfterLine=a.replayAfterLine==11?-1:Remap(a.replayAfterLine);a.mineBattleWon=s.positions.ContainsKey(ChapterId)&&s.positions[ChapterId]>Line;}
  s.storyFlags.Add(MigrationFlag);
 }
 static void Shift<T>(Dictionary<string,T> values){string prefix=ChapterId+"/q/";var old=values.Where(k=>k.Key.StartsWith(prefix)).ToList();foreach(var p in old)values.Remove(p.Key);foreach(var p in old){int n;if(int.TryParse(p.Key.Substring(prefix.Length),out n)&&n!=11)values[prefix+Remap(n)]=p.Value;}}
}
public partial class Game {
 bool TryMineStoryBattle(){
  if(!MineStoryBattle.Is(current)||index!=MineStoryBattle.Line||Attempt().review||Attempt().mineBattleWon)return false;
  if(menuLoading!=null&&!menuLoading.IsDisposed)return true;
  StopAudio();NavigateMenu(ShowMineStoryBattle,"旧矿道战斗",false);return true;
 }
 void ShowMineStoryBattle(){
  if(save.tavernBattle==null||save.tavernBattle.id!=MineStoryBattle.RunId){
   var r=new RogueRun{id=MineStoryBattle.RunId,seed=60110,state="combat",enemy="combat",theme=0,hp=175,maxHp=175,attack=20,enemyHp=110,enemyMax=110,pool=PreparationWords(current).ToList()};
   r.partyBattle=PartyCombat.CreateEncounter(r,save,new[]{"beast"},Engine.Level(save.xp),0);r.partyBattle.enemies[0].name="矿道守卫兽";save.tavernBattle=r;
  }
  Persist();RenderFullBattle(save.tavernBattle);
 }
 void ContinueMineStoryBattle(){
  if(save.tavernBattle.state=="lost"){save.tavernBattle=null;ShowMineStoryBattle();return;}
  if(save.tavernBattle.state!="won")return;
  Attempt().mineBattleWon=true;save.tavernBattle=null;index=MineStoryBattle.Line+1;save.positions[current.id]=index;Persist();ShowStory();PlayCurrent();
 }
}
