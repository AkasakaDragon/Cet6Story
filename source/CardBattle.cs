using System;using System.Linq;using System.Collections.Generic;

public class SupportCard {
 public string id,name,text;public int cost,rarity,art;
 public SupportCard(string i,string n,int c,int r,string t,int a){id=i;name=n;cost=c;rarity=r;text=t;art=a;}
 public string Archetype{get{return new[]{"scan","quick","double","retreat","pursuit","judgement","expose","execution"}.Contains(id)?"破绽连击":new[]{"spark","fuel","heatguard","hottrack","overjet","coolant","catalyst","detonate"}.Contains(id)?"燃烧过载":"支援";}}
 public string Quality{get{return new[]{"普通","稀有","史诗"}[rarity];}}
}
public class CardBattleState {
 public int version {get;set;} public string monster {get;set;} public int turn {get;set;} public int energy {get;set;} public int energyCapacity {get;set;}
 public List<string> deck {get;set;} public List<string> draw {get;set;} public List<string> hand {get;set;} public List<string> discard {get;set;} public List<string> exhausted {get;set;} public List<string> overflow {get;set;} public List<string> rewards {get;set;}
 public string lastMonsterAction {get;set;} public bool lastEvaded {get;set;} public int lastBlocked {get;set;} public int shield {get;set;} public int carryShield {get;set;} public int enemyShield {get;set;} public int spores {get;set;} public int heat {get;set;} public int noise {get;set;} public bool broken {get;set;}
 public int bonus {get;set;} public int percent {get;set;} public int nextBonus {get;set;} public int nextEnergy {get;set;} public int nextDraw {get;set;} public bool ignoreArmor {get;set;} public bool emergency {get;set;} public bool rescue {get;set;} public bool harmony {get;set;} public bool finisher {get;set;}
 public int vulnerable {get;set;} public int burning {get;set;} public int attackChain {get;set;} public int judgement {get;set;} public int catalyst {get;set;} public bool pursuitUsed {get;set;} public bool overchargeUsed {get;set;} public int burnDamage {get;set;} public bool answering {get;set;} public bool paid {get;set;} public string lastCard {get;set;} public string log {get;set;} public string unlockNotice {get;set;}
 public CardBattleState(){deck=new List<string>();draw=new List<string>();hand=new List<string>();discard=new List<string>();exhausted=new List<string>();overflow=new List<string>();rewards=new List<string>();}
}
public static partial class CardBattle {
 public static readonly SupportCard[] Cards={
  new SupportCard("guide","战术指引",1,0,"本回合答对：伤害 +6",0),new SupportCard("barrier","守护屏障",1,0,"本回合获得 8 护盾",1),new SupportCard("mark","弱点标记",0,0,"施加 1 回合易伤。消耗",2),new SupportCard("duet","默契一击",2,0,"本回合答对：伤害 +12",3),
  new SupportCard("focus","凝神",0,0,"抽 1 张牌。消耗",4),new SupportCard("cleanse","净化微光",1,0,"移除 1 孢子、1 热量，解除杂音",5),new SupportCard("break","破甲指令",1,0,"削减敌盾 12；本回合伤害 +4",6),new SupportCard("cover","应急掩护",1,0,"护盾 +5；本题答错再获 7 护盾",7),
  new SupportCard("charge","蓄势待发",1,1,"下一回合答对：伤害 +12",8),new SupportCard("pierce","精准破绽",2,1,"本回合攻击无视敌盾，伤害 +8",9),new SupportCard("echo","共鸣屏障",2,1,"获得 18 护盾，保留至下回合结束",10),new SupportCard("rethink","战术重整",0,1,"选择弃 1 张手牌，抽 2 张。消耗",11),new SupportCard("harmony","心意相通",1,1,"答对后下回合能量 +1、抽牌 +1；不叠加",12),
  new SupportCard("stars","双星协奏",3,2,"本回合伤害 +24，护盾 +12",13),new SupportCard("rescue","逆转之光",2,2,"本回合免于一次致死并获 8 护盾。消耗",14),new SupportCard("finale","终幕指令",3,2,"本回合伤害 +60%；击杀回复 10。消耗",15),
  new SupportCard("scan","弱点扫描",0,0,"施加 1 回合易伤。消耗",2),
  new SupportCard("quick","快速射击",1,0,"造成 5 伤害，抽 1 张牌",3),
  new SupportCard("double","双重突袭",1,0,"造成两次 3 伤害，连击 +2",3),
  new SupportCard("retreat","战术撤步",1,0,"护盾 +7；已有连击时抽 1 张",7),
  new SupportCard("pursuit","追击指令",1,1,"造成 7 伤害；易伤时返还 1 能量，每回合一次",9),
  new SupportCard("judgement","精准判断",1,1,"本回合无提示答对：下回合多抽 2 张",4),
  new SupportCard("expose","破绽扩大",1,1,"施加 2 回合易伤；已有易伤时造成 5 伤害",6),
  new SupportCard("execution","双星终结",2,2,"造成 10 伤害；此前每次攻击连击额外 +4",13),
  new SupportCard("spark","火花弹",1,0,"造成 4 伤害，施加 3 层燃烧",3),
  new SupportCard("fuel","燃料注入",1,0,"施加 5 层燃烧",8),
  new SupportCard("heatguard","热能护罩",1,0,"护盾 +6；每 3 层燃烧额外 +1，上限 +5",10),
  new SupportCard("hottrack","炽热追踪",1,0,"造成 6 伤害；敌人燃烧时抽 1 张",9),
  new SupportCard("overjet","过载喷射",1,1,"燃烧 +4；每回合首次额外消耗 1 能量，改为 +9",15),
  new SupportCard("coolant","冷却循环",0,0,"抽 1 张牌，护盾 +3。消耗",5),
  new SupportCard("catalyst","词义催化",1,1,"本回合无提示答对：燃烧 +5",12),
  new SupportCard("detonate","引爆核心",2,2,"移除燃烧，造成其层数两倍伤害，可受易伤加成",15)};
 public static readonly string[] Monsters={"苔藓木灵","荆棘孢子兽","石像哨兵","回响灯灵","余烬蜥蜴","熔甲幼兽"};
 public static SupportCard Get(string id){return Cards.First(c=>c.id==id);}
 public static void Profile(RogueProfile p){if(p.unlockedCards==null)p.unlockedCards=new List<string>();if(p.monsterKills==null)p.monsterKills=new Dictionary<string,int>();foreach(var c in Cards.Where((c,i)=>i<4||i>=16))if(!p.unlockedCards.Contains(c.id))p.unlockedCards.Add(c.id);}
 public static void Ensure(RogueRun r,RogueProfile p){Profile(p);if(r.cardBattle!=null&&r.cardBattle.version==1)return;Start(r,p);}
 public static void Start(RogueRun r,RogueProfile p){Profile(p);var old=r.cardBattle;var b=new CardBattleState{version=1,monster=r.enemy=="combat"?Monsters[Math.Min(2,r.theme)*2+RogueEngine.RandomFor(r).Next(2)]:r.enemy=="elite"?"精英": "首领"};b.deck=old!=null&&old.deck.Count>0?old.deck.ToList():new List<string>{"guide","guide","guide","barrier","barrier","barrier","mark","duet"};b.draw=b.deck.OrderBy(x=>RogueEngine.RandomFor(r).Next()).ToList();r.cardBattle=b;Begin(r,true);}
 static void Draw(RogueRun r,int count){var b=r.cardBattle;for(int i=0;i<count;i++){if(b.draw.Count==0){b.draw=b.discard.OrderBy(x=>RogueEngine.RandomFor(r).Next()).ToList();b.discard.Clear();}if(b.draw.Count==0)break;string id=b.draw[0];b.draw.RemoveAt(0);if(b.hand.Count<8)b.hand.Add(id);else b.overflow.Add(id);}}
 public static void Begin(RogueRun r,bool first){var b=r.cardBattle;b.answering=false;b.turn++;b.energy=3+b.nextEnergy;b.energyCapacity=b.energy;b.nextEnergy=0;b.bonus=b.nextBonus;b.nextBonus=0;b.percent=0;b.ignoreArmor=b.emergency=b.rescue=b.harmony=b.finisher=false;b.shield=b.carryShield;b.carryShield=0;b.attackChain=b.judgement=b.catalyst=b.burnDamage=0;b.pursuitUsed=b.overchargeUsed=false;b.lastCard=null;b.log="";Draw(r,first?4:2+b.nextDraw);b.nextDraw=0;}
 public static bool Play(RogueRun r,int index){var b=r.cardBattle;if(b==null||r.state!="combat"||r.question==null||r.question.answered||b.answering||b.overflow.Count>0||index<0||index>=b.hand.Count)return false;var c=Get(b.hand[index]);if(c.cost>b.energy||c.id=="rethink"&&b.hand.Count<2)return false;b.energy-=c.cost;b.hand.RemoveAt(index);bool exhaust=c.id=="mark"||c.id=="focus"||c.id=="rethink"||c.id=="rescue"||c.id=="finale"||c.id=="scan"||c.id=="coolant";if(exhaust)b.exhausted.Add(c.id);else b.discard.Add(c.id);b.lastCard=c.id;
  switch(c.id){case "guide":b.bonus+=6;break;case "barrier":b.shield+=8;break;case "mark":b.vulnerable++;break;case "duet":b.bonus+=12;break;case "focus":Draw(r,1);break;case "cleanse":b.spores=Math.Max(0,b.spores-1);b.heat=Math.Max(0,b.heat-1);b.noise=0;break;case "break":int before=b.enemyShield;b.enemyShield=Math.Max(0,b.enemyShield-12);if(before>0&&b.enemyShield==0)b.broken=true;b.bonus+=4;break;case "cover":b.shield+=5;b.emergency=true;break;case "charge":b.nextBonus+=12;break;case "pierce":b.ignoreArmor=true;b.bonus+=8;break;case "echo":b.shield+=18;b.carryShield=b.shield;break;case "rethink":b.overflow.Add("@rethink");break;case "harmony":b.harmony=true;break;case "stars":b.bonus+=24;b.shield+=12;break;case "rescue":b.rescue=true;break;case "finale":b.percent+=60;b.finisher=true;break;default:ExpansionPlay(r,c.id);break;}return true;
 }
 public static bool EndTurn(RogueRun r){var b=r.cardBattle;if(b==null||r.state!="combat"||b.answering||b.overflow.Count>0)return false;b.answering=true;return true;}
 public static bool DiscardChoice(RogueRun r,int index){var b=r.cardBattle;if(b==null||r.state!="combat"||b.overflow.Count==0||index<0||index>=b.hand.Count)return false;b.discard.Add(b.hand[index]);b.hand.RemoveAt(index);string pending=b.overflow[0];b.overflow.RemoveAt(0);if(pending=="@rethink")Draw(r,2);else b.hand.Add(pending);return true;}
 public static string Name(RogueRun r){return r.cardBattle!=null&&r.enemy=="combat"?r.cardBattle.monster:null;}
 public static string Intent(RogueRun r){var b=r.cardBattle;if(b==null)return "准备战斗";int phase=(b.turn-1)%4;string[] actions;
  switch(b.monster){case "苔藓木灵":actions=new[]{"扎根 · 获得 12 护盾","藤鞭 · 8 伤害","汲取生机 · 6 伤害并吸血；破盾阻止吸血","藤鞭 · 8 伤害"};break;case "荆棘孢子兽":actions=new[]{"孢子播种 · 2 层；无提示答对净化 1 层","棘刺 · 7 伤害","孢子引爆 · "+(4+b.spores*4)+" 伤害；答对先净化","棘刺 · 7 伤害"};break;case "石像哨兵":actions=new[]{"符文蓄力 · 准备重击","石槌重击 · 18 伤害；无提示答对伤害 ≥20 可打断","冷却 · 不攻击，受到伤害 +25%","石拳 · 8 伤害"};break;case "回响灯灵":actions=new[]{"杂音结界 · 两题连击增伤减半；答对解除","灵火 · 7 伤害","回响冲击 · "+(10+(b.noise>0?4:0))+" 伤害","灵火 · 7 伤害"};break;case "余烬蜥蜴":actions=new[]{"吞火 · 获得 1 热量；无提示答对降温","灼热撕咬 · "+(8+b.heat*2)+" 伤害","喷焰 · "+(10+b.heat*4)+" 伤害并清空热量","散热 · 不攻击，受到伤害 +25%"};break;case "熔甲幼兽":actions=new[]{"熔甲凝固 · 获得 16 护盾","冲撞 · 9 伤害","碎甲震荡 · "+(6+b.enemyShield/2)+" 伤害；提前破盾可取消","爪击 · 7 伤害"};break;default:actions=new[]{"蓄力 · 不攻击","普通攻击 · "+(r.enemy=="boss"?18:12)+" 伤害","符文重击 · "+(r.enemy=="boss"?24:16)+" 伤害","普通攻击 · "+(r.enemy=="boss"?18:12)+" 伤害"};break;}
  string detail=actions[phase];bool normal=phase==1||phase==3;if(b.monster=="石像哨兵")normal=phase==3;if(detail.Contains("伤害")&&!detail.Contains("不攻击"))detail+=normal?" · 答对可闪避":" · 答对伤害减半";return "第 "+b.turn+" 回合 · "+detail;
 }
 public static int Attack(RogueRun r,int baseDamage,bool clean){var b=r.cardBattle;int phase=(b.turn-1)%4;if(clean){b.spores=Math.Max(0,b.spores-1);b.heat=Math.Max(0,b.heat-1);b.noise=0;}if(clean){b.nextDraw+=b.judgement;b.burning+=b.catalyst;}int damage=(int)Math.Ceiling((baseDamage+b.bonus)*(100+b.percent)/100.0*(b.vulnerable>0?1.25:1));if(b.monster=="石像哨兵"&&phase==2||b.monster=="余烬蜥蜴"&&phase==3||b.monster=="熔甲幼兽"&&phase==2&&b.broken)damage=(int)Math.Ceiling(damage*1.25);if(!b.ignoreArmor){int blocked=Math.Min(damage,b.enemyShield);b.enemyShield-=blocked;damage-=blocked;if(blocked>0&&b.enemyShield==0)b.broken=true;}r.enemyHp=Math.Max(0,r.enemyHp-damage);if(r.enemyHp==0&&b.finisher)r.hp=Math.Min(r.maxHp,r.hp+10);if(b.harmony){b.nextEnergy=1;b.nextDraw=1;}return damage;}
 public static int Enemy(RogueRun r,bool correct,bool clean,int attack){var b=r.cardBattle;int p=(b.turn-1)%4;int raw=0;bool normal=p==1||p==3;string action=Intent(r);b.lastMonsterAction=r.enemyHp<=0?"none":MonsterCombat.PlannedAction(r);
  if(r.enemyHp>0)switch(b.monster){case "苔藓木灵":if(p==0){b.enemyShield=12;b.broken=false;}else raw=p==2?6:8;break;case "荆棘孢子兽":if(p==0)b.spores=2;else if(p==2){raw=4+b.spores*4;b.spores=0;}else raw=7;break;case "石像哨兵":normal=p==3;if(p==1){if(clean&&attack>=20){action="重击被打断 · 石像失衡";b.lastMonsterAction="none";}else raw=18;}else if(p==3)raw=8;break;case "回响灯灵":if(p==0)b.noise=2;else raw=p==2?10+(b.noise>0?4:0):7;break;case "余烬蜥蜴":if(p==0)b.heat=Math.Min(2,b.heat+1);else if(p==1)raw=8+b.heat*2;else if(p==2){raw=10+b.heat*4;b.heat=0;}break;case "熔甲幼兽":if(p==0){b.enemyShield=16;b.broken=false;}else if(p==2){if(!b.broken)raw=6+b.enemyShield/2;else{action="甲壳破裂 · 震荡取消";b.lastMonsterAction="none";}b.enemyShield=0;b.broken=false;}else raw=p==1?9:7;break;default:raw=p==0?0:p==2?(r.enemy=="boss"?24:16):(r.enemy=="boss"?18:12);break;}
  b.lastEvaded=correct&&normal&&raw>0;if(!correct&&b.emergency)b.shield+=7;if(raw>0){if(correct)raw=normal?0:(int)Math.Ceiling(raw/2.0);if(raw>0)raw=Math.Max(0,raw+r.depth/4-r.armor);if(!correct&&r.guard>0&&!r.guardUsed&&raw>0){raw=(int)Math.Ceiling(raw/Math.Pow(2,Math.Min(6,r.guard)));r.guardUsed=true;}}
  int blocked=Math.Min(b.shield,raw);b.lastBlocked=blocked;b.shield-=blocked;int received=raw-blocked;if(received>=r.hp&&b.rescue){received=Math.Max(0,r.hp-1);b.shield+=8;b.rescue=false;action+=" · 逆转之光救援";}r.hp=Math.Max(0,r.hp-received);if(r.enemyHp>0&&b.monster=="苔藓木灵"&&p==2&&!b.broken)r.enemyHp=Math.Min(r.enemyMax,r.enemyHp+received);b.burnDamage=r.enemyHp>0?Math.Min(r.enemyHp,b.burning):0;r.enemyHp=Math.Max(0,r.enemyHp-b.burnDamage);b.burning=Math.Max(0,b.burning-2);b.vulnerable=Math.Max(0,b.vulnerable-1);b.log=r.enemyHp<=0?(b.burnDamage>0?"敌方行动结束后被燃烧击败":"敌人已被击败，行动取消"):action+"\n护盾抵挡 "+blocked+" · 承受 "+received;if(b.burnDamage>0)b.log+="\n燃烧造成 "+b.burnDamage+" 伤害";if(b.noise>0&&p!=0)b.noise--;b.carryShield=Math.Min(b.carryShield,b.shield);return received;
 }
 public static void Reward(RogueProfile p,RogueRun r){var b=r.cardBattle;if(b.paid)return;b.paid=true;Profile(p);var fresh=new List<string>();int kills;p.monsterKills.TryGetValue(b.monster,out kills);p.monsterKills[b.monster]=kills+1;p.smallKills+=r.enemy=="combat"?1:0;if(r.enemy=="elite")p.eliteKills++;
  if(r.enemy=="combat"&&kills==0){int i=Array.IndexOf(Monsters,b.monster);if(i>=0)fresh.Add(new[]{"focus","cleanse","charge","rethink","cover","break"}[i]);}
  if(r.enemy=="elite"){fresh.Add(new[]{"pierce","echo","harmony"}[(p.eliteKills-1)%3]);if(p.eliteKills>=3)fresh.Add("stars");if(p.eliteKills>=6)fresh.Add("rescue");if(p.eliteKills>=9)fresh.Add("finale");}
  if(p.smallKills>=4)fresh.Add("focus");if(p.smallKills>=8)fresh.Add("cleanse");if(p.smallKills>=12)fresh.Add("charge");if(p.smallKills>=16)fresh.Add("rethink");fresh=fresh.Distinct().Where(x=>!p.unlockedCards.Contains(x)).ToList();p.unlockedCards.AddRange(fresh);b.unlockNotice=String.Join("、",fresh.Select(x=>Get(x).name));var rng=RogueEngine.RandomFor(r);b.rewards=fresh.Take(3).ToList();if(r.enemy=="elite"&&!b.rewards.Any(x=>Get(x).rarity>=1)){var rare=Cards.Where(c=>c.rarity==1&&p.unlockedCards.Contains(c.id)).ToList();if(rare.Count>0)b.rewards.Add(rare[rng.Next(rare.Count)].id);}while(b.rewards.Count<3){int roll=rng.Next(100);int rarity=r.enemy=="elite"?(roll<20?0:roll<85?1:2):(roll<80?0:1);var pool=Cards.Where(c=>c.rarity==rarity&&p.unlockedCards.Contains(c.id)&&!b.rewards.Contains(c.id)).ToList();if(pool.Count==0)pool=Cards.Where(c=>p.unlockedCards.Contains(c.id)&&!b.rewards.Contains(c.id)).ToList();if(pool.Count==0)break;b.rewards.Add(pool[rng.Next(pool.Count)].id);}r.state="card-reward";
 }
 public static bool Pick(RogueProfile p,string id){var r=p.ActiveRun;if(r==null||r.state!="card-reward")return false;var b=r.cardBattle;if(id!=null&&!b.rewards.Contains(id))return false;if(id!=null)b.deck.Add(id);b.rewards.Clear();if(r.enemy=="elite"&&!r.eventBattle)TowerEngine.Offer(r);else{r.feedback="战斗胜利 · 金币 +"+r.battleCoins;r.state="loot";}return true;}
}





