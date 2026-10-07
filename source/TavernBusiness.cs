using System;using System.Collections.Generic;using System.Linq;
public class TavernBusinessState {
 public int day{get;set;}public bool introduced{get;set;}public string mode{get;set;}public bool settled{get;set;}public int baseCoins{get;set;}public int bonusCoins{get;set;}public int groups{get;set;}public int wordCursor{get;set;}public int level{get;set;}public List<string> rewarded{get;set;}
 public TavernBusinessState(){day=1;level=4;rewarded=new List<string>();}
}
public static class TavernBusiness {
 public static bool Unlocked(Save s){return WaystationChapterOne.Ids.All(id=>s.completed.Contains(id));}
 public static TavernBusinessState State(Save s){if(s.tavernBusiness==null)s.tavernBusiness=new TavernBusinessState();if(s.tavernBusiness.rewarded==null)s.tavernBusiness.rewarded=new List<string>();return s.tavernBusiness;}
 public static bool Begin(Save s,string mode){var t=State(s);if(!Unlocked(s)||t.settled||(mode!="words"&&mode!="listening")||t.mode!=null&&t.mode!=mode)return false;t.mode=mode;if(t.baseCoins==0){s.rogue.coins+=100;t.baseCoins=100;}return true;}
 public static int Reward(Save s,string key,bool correct){var t=State(s);if(!correct||t.settled||t.mode==null||t.bonusCoins>=50||t.rewarded.Contains(key))return 0;t.rewarded.Add(key);int coins=Math.Min(5,50-t.bonusCoins);t.bonusCoins+=coins;s.rogue.coins+=coins;return coins;}
 public static bool Skip(Save s){var t=State(s);if(!Unlocked(s)||t.settled||t.mode!=null)return false;s.rogue.coins+=50;t.baseCoins=50;t.settled=true;return true;}
 public static bool Settle(Save s){var t=State(s);if(t.settled||t.mode==null)return false;t.settled=true;return true;}
 public static bool NextDay(Save s){var t=State(s);if(!t.settled)return false;s.tavernBusiness=new TavernBusinessState{day=t.day+1,introduced=t.introduced,level=t.level};return true;}
}
public class TavernListeningGroup {public string id{get;set;}public int level{get;set;}public string title{get;set;}public string source{get;set;}public string audio{get;set;}public string transcript{get;set;}public List<TavernListeningQuestion> questions{get;set;}}
public class TavernListeningQuestion {public string[] options{get;set;}public int answer{get;set;}public string explanation{get;set;}}
