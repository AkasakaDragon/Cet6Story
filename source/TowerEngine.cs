using System;
using System.Linq;
using System.Collections.Generic;

public class TowerNode {
 public string id {get;set;} public int row {get;set;} public int lane {get;set;} public string kind {get;set;} public List<string> next {get;set;} public bool visited {get;set;}
 public TowerNode(){next=new List<string>();}
}

// The generated graph is stored with the run, so reopening never rerolls encounters.
public static class TowerEngine {
 public const int Floors=8;
 public static bool IsTower(RogueRun r){return r!=null&&(r.mapVersion==2||r.mapVersion==3);}
 public static void Initialize(RogueRun r){
  r.mapVersion=3;r.nodes=new List<TowerNode>();var rng=new Random(r.seed);
  string[][] rows={new[]{"combat"},new[]{"combat","event","shop"},new[]{"elite","combat","rest"},new[]{"chest","chest","chest"},new[]{"combat","event","shop"},new[]{"elite","combat","event"},new[]{"rest","rest","rest"},new[]{"boss"}};
  for(int row=0;row<Floors;row++){var kinds=rows[row].OrderBy(x=>rng.Next()).ToArray();for(int i=0;i<kinds.Length;i++)r.nodes.Add(new TowerNode{id="n"+row+"-"+i,row=row,lane=kinds.Length==1?1:i,kind=kinds[i]});}
  foreach(var node in r.nodes){foreach(var to in r.nodes.Where(n=>n.row==node.row+1&&Math.Abs(n.lane-node.lane)<=1))node.next.Add(to.id);}
  r.lastNode=null;r.currentNode=null;Routes(r);
 }
 public static void Migrate(RogueProfile p){
  var r=p.ActiveRun;if(r==null||r.mode=="本节词汇准备"||IsTower(r)||r.state=="ended")return;
  string state=r.state,enemy=r.enemy;int depth=r.depth;Initialize(r);r.depth=Math.Min(Floors-2,depth);
  foreach(var node in r.nodes.Where(n=>n.row<r.depth&&n.lane==1))node.visited=true;
  r.lastNode=r.nodes.Where(n=>n.row==r.depth-1&&n.lane==1).Select(n=>n.id).FirstOrDefault();
  r.currentNode=r.nodes.First(n=>n.row==r.depth&&n.lane==1).id;
  if(state!="map"){r.nodes.First(n=>n.id==r.currentNode).kind=state=="combat"||state=="feedback"||state=="reward"?enemy:state=="rest"?"rest":state=="shop"?"shop":"event";r.state=state;r.theme=RunTheme(r,r.depth);}
  else{r.currentNode=null;Routes(r);}if(r.gold>0){p.coins+=r.gold;r.earnedCoins+=r.gold;r.gold=0;}if(state=="reward")r.battlePaid=true;
 }
 public static int FloorCount(RogueRun r){return r!=null&&r.nodes!=null&&r.nodes.Count>0?r.nodes.Max(n=>n.row)+1:Floors;}
 public static int RunTheme(RogueRun r,int depth){return r.mapVersion>=3?Math.Min(2,depth/3):Theme(depth);}
 public static int EnemyHealth(RogueRun r,string kind){return r.mapVersion>=3?(kind=="boss"?340:kind=="elite"?190+r.depth*4:110+r.depth*4):(kind=="boss"?165:kind=="elite"?Math.Min(100,75+r.depth*3):Math.Min(55,35+r.depth*2));}
 public static int Theme(int depth){return Math.Min(2,depth/4);}
 public static TowerNode Node(RogueRun r){return r.nodes==null?null:r.nodes.FirstOrDefault(n=>n.id==r.currentNode);}
 public static List<TowerNode> Available(RogueRun r){
  if(r.nodes==null||r.state!="map")return new List<TowerNode>();
  var last=r.nodes.FirstOrDefault(n=>n.id==r.lastNode);return r.nodes.Where(n=>!n.visited&&(last==null?n.row==0:last.next.Contains(n.id))).ToList();
 }
 public static void Routes(RogueRun r){r.state="map";r.routes=Available(r).Select(n=>n.id).ToList();}
 public static void Choose(RogueRun r,string id,RogueProfile p){
  var node=Available(r).FirstOrDefault(n=>n.id==id);if(node==null)return;
  r.currentNode=id;r.theme=RunTheme(r,node.row);r.eventBattle=false;r.battleAssists=0;r.battlePaid=false;r.battleCoins=0;r.guardUsed=false;
  switch(node.kind){case "combat":case "elite":StartBattle(r,p,node.kind);break;case "boss":r.state="boss-intro";r.enemy="boss";r.enemyMax=EnemyHealth(r,"boss");r.enemyHp=r.enemyMax;break;case "rest":r.state="rest";break;case "shop":r.shopBought.Clear();r.state="shop";break;case "chest":r.chestKind=RogueEngine.RandomFor(r).Next(3);r.state="chest";break;default:r.eventKind=RogueEngine.RandomFor(r).Next(5);r.state="event";break;}
 }
 public static void StartBattle(RogueRun r,RogueProfile p,string kind){r.enemy=kind;r.enemyMax=EnemyHealth(r,kind);r.enemyHp=r.enemyMax;r.combo=0;r.guardUsed=false;r.state="combat";CardBattle.Start(r,p);RogueEngine.NextQuestion(r,p);}
 public static int EnemyDamage(RogueRun r){return Math.Max(1,(r.enemy=="boss"?18:r.enemy=="elite"?12:8)+r.depth/4-r.armor);}
 public static string EnemyName(RogueRun r){if(CardBattle.Name(r)!=null)return CardBattle.Name(r);if(r.enemy=="boss")return "BOSS · 古界守门者";string[] normal={"苔藓木灵","石像哨兵","余烬蜥蜴"},elite={"古树守卫","封印骑士","熔岩巨魔"};return r.enemy=="elite"?"精英 · "+elite[Math.Min(2,r.theme)]:normal[Math.Min(2,r.theme)];}
 public static string KindName(string kind){switch(kind){case "combat":return "小怪";case "elite":return "精英";case "rest":return "火堆";case "event":return "问号事件";case "chest":return "宝箱";case "shop":return "商店";default:return "BOSS";}}
 public static string KindDescription(string kind){switch(kind){case "combat":return "战胜后获得 8–12 金币";case "elite":return "战胜后获得 20–30 金币与三选一赋能";case "rest":return "恢复 30% 生命上限，或攻击 +3";case "event":return "选择风险与回报，遭遇五种随机事件";case "chest":return "金币宝箱，稀有宝箱另有三选一赋能";case "shop":return "用永久金币购买本局恢复与赋能";default:return "最终挑战 · 40–50 金币";}}
 public static void Coins(RogueProfile p,RogueRun r,int amount){p.coins+=amount;r.earnedCoins+=amount;}
 public static void Continue(RogueProfile p){
  var r=p.ActiveRun;if(r==null||r.state!="feedback")return;if(r.hp<=0){RogueEngine.Finish(p,false);return;}
  if(r.enemyHp>0){if(r.cardBattle!=null)CardBattle.Begin(r,false);RogueEngine.NextQuestion(r,p);return;}
  if(!r.battlePaid){var rng=RogueEngine.RandomFor(r);int reward=r.eventBattle?12:r.enemy=="boss"?rng.Next(40,51):r.enemy=="elite"?rng.Next(20,31):rng.Next(8,13);reward+=r.fortune;if(r.battleAssists>0)reward=Math.Max(1,reward/2);Coins(p,r,reward);r.battleCoins=reward;r.battlePaid=true;r.hp=Math.Min(r.maxHp,r.hp+r.heal);}
  if(r.enemy=="boss"){r.depth=FloorCount(r);var node=Node(r);if(node!=null)node.visited=true;RogueEngine.Finish(p,true);return;}
  if(r.cardBattle!=null){CardBattle.Reward(p,r);return;}if(r.enemy=="elite"&&!r.eventBattle){Offer(r);return;}r.feedback="战斗胜利 · 金币 +"+r.battleCoins+(r.battleAssists>0?"（本场使用过提示，战利品减半）":"")+(r.heal>0?"\n胜利恢复 "+r.heal+" 生命":"");r.state="loot";
 }
 public static void Offer(RogueRun r){var rng=RogueEngine.RandomFor(r);r.rewards=RogueEngine.Relics.OrderBy(x=>rng.Next()).Take(3).ToList();r.state="reward";}
 public static bool PreparedQuestion(RogueRun r,RogueProfile p){
  if(!IsTower(r)||String.IsNullOrEmpty(r.vocabularyChapter)||PreparationEngine.Complete(p,r.vocabularyChapter,r.pool))return false;
  var previous=p.prepSession;try{p.prepSession=r;PreparationEngine.Next(p,r.vocabularyChapter,r.pool);}finally{p.prepSession=previous;}return true;
 }
 public static void Learn(RogueProfile p,RogueRun r,RogueQuestion q,bool correct){
  if(!IsTower(r)||String.IsNullOrEmpty(r.vocabularyChapter)||!correct||q.assisted)return;
  var progress=PreparationEngine.Progress(p,r.vocabularyChapter);progress.masks[q.entry.word]=PreparationEngine.Mask(progress,q.entry.word)|(1<<q.kind);
  if(PreparationEngine.Complete(p,r.vocabularyChapter,r.pool)&&!progress.rewarded){progress.rewarded=true;Coins(p,r,60);r.feedback+="\n本节词汇准备完成 · 首次奖励 +60 金币";}
 }
 public static void Complete(RogueProfile p){var r=p.ActiveRun;if(r.state!="loot"&&r.state!="reward"&&r.state!="shop")return;var node=Node(r);if(node!=null){node.visited=true;r.lastNode=node.id;}r.depth++;p.bestDepth=Math.Max(p.bestDepth,r.depth);r.currentNode=null;r.question=null;Routes(r);}
 public static void Rest(RogueProfile p,bool heal){var r=p.ActiveRun;if(r.state!="rest")return;if(heal)r.hp=Math.Min(r.maxHp,r.hp+(int)Math.Ceiling(r.maxHp*.3));else r.attack+=3;r.feedback=heal?"火堆休息 · 已恢复生命":"火堆磨练 · 本局攻击 +3";r.state="loot";}
 public static void Chest(RogueProfile p){var r=p.ActiveRun;if(r.state!="chest")return;int coins=r.chestKind==2?20:15;Coins(p,r,coins);r.battleCoins=coins;r.feedback="开启宝箱 · 金币 +"+coins;if(r.chestKind==2){Offer(r);return;}r.state="loot";}
 public static string EventTitle(int id){return new[]{"献祭石碑","旅行者的药箱","流浪商人的秘藏","被困的木灵","遗忘的词匣"}[id];}
 public static string EventDescription(int id){return new[]{"石碑承诺一份古老的力量，代价是 12 生命。也可以带走散落的金币。","一个遗落的药箱仍有可用的药剂。付费恢复，或免费做一次小幅包扎。","商人拿出一枚锋刃符石。支付 45 金币购买，或平静离开。","一只木灵被猎手围困。加入一场战斗解救它，或留下补给继续前进。","词匣中封着一只小怪。接受词汇战斗可赚金币，也可以带走一枚免费提示。"}[id];}
 public static bool Event(RogueProfile p,bool risk){
  var r=p.ActiveRun;if(r.state!="event")return false;switch(r.eventKind){
   case 0:if(risk){if(r.hp<=12)return false;r.hp-=12;RogueEngine.ApplyRelic(r,RogueEngine.Relics[RogueEngine.RandomFor(r).Next(RogueEngine.Relics.Length)]);r.feedback="石碑赐福 · 消耗 12 生命，获得随机赋能";}else{Coins(p,r,8);r.feedback="拾起散落金币 · 金币 +8";}break;
   case 1:if(risk){if(p.coins<15||r.hp==r.maxHp)return false;p.coins-=15;r.hp=Math.Min(r.maxHp,r.hp+35);r.feedback="治疗药剂 · 消耗 15 金币，恢复 35 生命";}else{r.hp=Math.Min(r.maxHp,r.hp+10);r.feedback="简单包扎 · 恢复 10 生命";}break;
   case 2:if(risk){if(p.coins<45)return false;p.coins-=45;RogueEngine.ApplyRelic(r,"blade");r.feedback="购买锋刃符石 · 攻击 +4";}else r.feedback="你向商人告别，继续前行。";break;
   case 3:case 4:if(risk){r.eventBattle=true;r.battlePaid=false;r.battleAssists=0;StartBattle(r,p,"combat");return true;}if(r.eventKind==3){Coins(p,r,6);r.feedback="留下祝福 · 金币 +6";}else{r.hints++;r.feedback="词匣的回声 · 提示次数 +1";}break;
  }r.state="loot";return true;
 }
 public static int SupplyPrice(string id){return id=="potion"?35:id=="hint"?20:80;}
 public static bool Buy(RogueProfile p,string id){var r=p.ActiveRun;if(r==null||r.state!="shop"||r.shopBought.Contains(id)||!new[]{"potion","shield","blade","hint"}.Contains(id))return false;int price=SupplyPrice(id);if(p.coins<price||(id=="potion"&&r.hp==r.maxHp))return false;p.coins-=price;r.shopBought.Add(id);if(id=="potion")r.hp=Math.Min(r.maxHp,r.hp+30);else if(id=="hint")r.hints++;else RogueEngine.ApplyRelic(r,id);return true;}
}

