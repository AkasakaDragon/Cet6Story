using System;
using System.Linq;
using System.Collections.Generic;

public class PartyUnit {
 public string id {get;set;} public string name {get;set;} public int hp {get;set;} public int maxHp {get;set;} public int attack {get;set;} public int shield {get;set;} public int rank {get;set;} public int speed {get;set;}
 public int resolve {get;set;} public int riposte {get;set;} public int runeBoost {get;set;} public int marks {get;set;} public int markTurns {get;set;} public int bleed {get;set;} public int burn {get;set;} public int broken {get;set;} public int silence {get;set;} public int anchored {get;set;} public string guard {get;set;} public bool acted {get;set;}
 public List<string> skills {get;set;} public Dictionary<string,int> cooldowns {get;set;} public Dictionary<string,int> used {get;set;}
 public PartyUnit(){skills=new List<string>();cooldowns=new Dictionary<string,int>();used=new Dictionary<string,int>();}
}
public class PartyCombatState {
 public List<PartyUnit> heroes {get;set;} public List<PartyUnit> enemies {get;set;} public int round {get;set;} public string phase {get;set;} public int selectedHero {get;set;} public string selectedSkill {get;set;} public string target {get;set;}
 public string pendingSkill {get;set;} public string pendingHero {get;set;} public string pendingTarget {get;set;} public int enemyCursor {get;set;} public List<string> defenseTargets {get;set;} public int defenseCursor {get;set;} public int enemyDamage {get;set;} public bool enemyGroup {get;set;} public string log {get;set;} public string outcome {get;set;} public int questions {get;set;}
 public PartyCombatState(){heroes=new List<PartyUnit>();enemies=new List<PartyUnit>();defenseTargets=new List<string>();round=1;phase="heroes";log="点击角色，选择技能与目标。每位角色每回合行动一次。";}
}
public static class PartyCombat {
 public static PartyCombatState Create(RogueRun r,Save save){
  var b=new PartyCombatState();int level=Engine.Level(save.xp);
  b.heroes.Add(Hero("aelia","艾莉娅",100,22,1,level,save));b.heroes.Add(Hero("luchuan","陆川",75,24,2,level,save));
  if(r.partyRoster!=null&&r.partyRoster.Count>0){
   foreach(var h in b.heroes){var prior=r.partyRoster.FirstOrDefault(x=>x.id==h.id);if(prior!=null){h.hp=Math.Max(0,Math.Min(h.maxHp,prior.hp+Math.Max(0,h.maxHp-prior.maxHp)));h.rank=prior.rank;}}
   int recovery=Math.Max(0,r.hp-r.partyRoster.Sum(x=>x.hp));foreach(var h in b.heroes.Where(x=>x.hp>0)){int heal=Math.Min(recovery,h.maxHp-h.hp);h.hp+=heal;recovery-=heal;}
  }
  b.enemies.Add(new PartyUnit{id="enemy-0",name=r.cardBattle!=null?r.cardBattle.monster:"感染兽",maxHp=Math.Max(110,r.enemyMax),hp=Math.Max(1,r.enemyHp==r.enemyMax?Math.Max(110,r.enemyMax):r.enemyHp),attack=20,speed=4,rank=1});
  return b;
 }
 static PartyUnit Hero(string id,string name,int hp,int attack,int rank,int level,Save save){
  var h=new PartyUnit{id=id,name=name,maxHp=hp+(level-1)*8,hp=hp+(level-1)*8,attack=attack+(level-1)*2,rank=rank,speed=id=="aelia"?3:5};
  List<string> loadout=null;if(save.heroLoadouts!=null)save.heroLoadouts.TryGetValue(id,out loadout);
  h.skills=(loadout??HeroSkills.For(id).Take(5).Select(s=>s.Id).ToList()).Where(k=>HeroSkills.Get(id,k)!=null&&HeroSkills.Unlocked(save,id,HeroSkills.Get(id,k).Index)).Distinct().Take(5).ToList();
  foreach(var s in HeroSkills.For(id).Take(5))if(h.skills.Count<5&&!h.skills.Contains(s.Id))h.skills.Add(s.Id);return h;
 }
 public static bool CanUse(PartyCombatState b,PartyUnit h,HeroSkill s){return b.phase=="heroes"&&String.IsNullOrEmpty(b.outcome)&&h.hp>0&&!h.acted&&s!=null&&h.skills.Contains(s.Id)&&(!h.cooldowns.ContainsKey(s.Id)||h.cooldowns[s.Id]==0)&&(s.Uses==0||!h.used.ContainsKey(s.Id)||h.used[s.Id]<s.Uses);}
 public static List<PartyUnit> Targets(PartyCombatState b,PartyUnit h,HeroSkill s){if(s.Target=="self")return new List<PartyUnit>{h};if(s.Target=="ally")return b.heroes.Where(x=>x.hp>0&&(s.Id!="guard"||x!=h)).ToList();var enemies=b.enemies.Where(x=>x.hp>0).OrderBy(x=>x.rank).ToList();return h.id=="aelia"?enemies.Take(2).ToList():enemies;}
 public static bool BeginSkill(PartyCombatState b,int hero,string skill,string target){if(hero<0||hero>=b.heroes.Count)return false;var h=b.heroes[hero];var s=HeroSkills.Get(h.id,skill);if(!CanUse(b,h,s))return false;var targets=Targets(b,h,s);if(!targets.Any(x=>x.id==target))return false;b.pendingHero=h.id;b.pendingSkill=skill;b.pendingTarget=target;b.phase="attack-question";return true;}
 public static int AddShield(PartyUnit h,int amount){int before=h.shield;h.shield=Math.Min(h.maxHp*40/100,h.shield+Math.Max(0,amount));return h.shield-before;}
 static int ShieldAmount(PartyUnit h,HeroSkill s){return s.Shield*h.maxHp/(h.id=="aelia"?100:75);}
 public static void AttackAnswer(PartyCombatState b,bool correct){
  if(b.phase!="attack-question")return;var h=b.heroes.First(x=>x.id==b.pendingHero);var s=HeroSkills.Get(h.id,b.pendingSkill);var t=b.heroes.Concat(b.enemies).First(x=>x.id==b.pendingTarget);
  h.acted=true;h.cooldowns[s.Id]=s.Cooldown+1;if(!h.used.ContainsKey(s.Id))h.used[s.Id]=0;h.used[s.Id]++;b.questions++;
  var log=new List<string>{h.name+" · "+s.Name+" · "+(correct?"答对，完整效果":"答错，基础效果减半")};
  var targets=new List<PartyUnit>{t};if(s.Group){var live=b.enemies.Where(x=>x.hp>0).OrderBy(x=>x.rank);targets=(s.Id=="sweep"?live.Take(2):s.Id=="wave"?live.Reverse().Take(2):live).ToList();}
  if(s.Damage>0){int resolve=h.resolve;
   foreach(var enemy in targets){int percent=s.Damage;if(correct){if(s.Id=="execute"&&enemy.hp*100<enemy.maxHp*30)percent=180;if(s.Id=="pursuit"&&enemy.bleed>0)percent=150;if(s.Id=="detonate")percent+=40*enemy.marks;if(s.Id=="collapse")percent+=30*enemy.marks;}
    double boost=correct?(1+resolve*.15)*(h.id=="aelia"&&enemy.broken>0?1.25:1):1;int hit=Math.Max(1,(int)Math.Floor(h.attack*percent/100.0*boost*(correct?1:.5)));int absorbed=Math.Min(enemy.shield,hit);enemy.shield-=absorbed;enemy.hp=Math.Max(0,enemy.hp-hit+absorbed);log.Add(enemy.name+" −"+(hit-absorbed)+"生命"+(absorbed>0?"（护盾吸收"+absorbed+"）":""));
    if(correct){switch(s.Id){case "bleed":enemy.bleed=3;break;case "bash":enemy.shield=Math.Max(0,enemy.shield-12);break;case "break":enemy.broken=3;break;case "pursuit":if(enemy.bleed>0)enemy.bleed++;break;case "bolt":case "wave":Mark(enemy,1+h.runeBoost);break;case "inscribe":Mark(enemy,2+h.runeBoost);break;case "burn":enemy.burn=3;Mark(enemy,1+h.runeBoost);break;case "detonate":case "collapse":enemy.marks=0;enemy.markTurns=0;break;case "silence":if(enemy.marks>0){enemy.marks--;enemy.silence=1;}break;case "embers":if(enemy.burn>0){enemy.hp=Math.Max(0,enemy.hp-4);Mark(enemy,1+h.runeBoost);}break;case "spread":var other=b.enemies.Where(x=>x.hp>0&&x!=enemy).OrderBy(x=>Math.Abs(x.rank-enemy.rank)).FirstOrDefault();if(other!=null&&enemy.marks>0)Mark(other,enemy.marks);break;}}
   }
   if(correct){h.resolve=0;if(new[]{"bolt","wave","inscribe","burn","embers"}.Contains(s.Id))h.runeBoost=0;if(s.Shield>0)log.Add("获得"+AddShield(h,ShieldAmount(h,s))+"点护盾");if(s.Id=="advance")Move(b.heroes,h,-1);}
  }else{
   if(s.Shield>0)log.Add("获得"+AddShield(h,ShieldAmount(h,s)/(correct?1:2))+"点护盾");
   if(s.Id=="recover"||s.Id=="bandage"){var patient=s.Id=="recover"?h:t;int heal=patient.maxHp*(s.Id=="recover"?15:20)/100/(correct?1:2);int actual=Math.Min(patient.maxHp-patient.hp,heal);patient.hp+=actual;if(correct)patient.bleed=0;log.Add(patient.name+"恢复"+actual+"生命");}
   if(correct){switch(s.Id){case "guard":h.guard=t.id;break;case "counter":h.riposte=2;break;case "stand":h.anchored=2;break;case "screen":h.runeBoost=1;break;case "retreat":Move(b.heroes,h,1);if(h.cooldowns.ContainsKey("detonate"))h.cooldowns["detonate"]=Math.Max(0,h.cooldowns["detonate"]-1);break;}}
  }
  b.log=String.Join("\n",log);Outcome(b);b.phase="feedback";
 }
 static void Mark(PartyUnit e,int n){e.marks=Math.Min(3,e.marks+n);e.markTurns=3;}
 static void Move(List<PartyUnit> units,PartyUnit h,int direction){var next=units.Where(x=>x.hp>0&&(direction<0?x.rank<h.rank:x.rank>h.rank)).OrderBy(x=>Math.Abs(x.rank-h.rank)).FirstOrDefault();if(next!=null&&h.anchored==0){int r=h.rank;h.rank=next.rank;next.rank=r;}}
 public static void Continue(PartyCombatState b){if(!String.IsNullOrEmpty(b.outcome))return;if(b.phase=="enemy-feedback"){StartEnemy(b);return;}if(b.phase!="feedback")return;if(b.defenseTargets.Count>0){b.defenseCursor++;while(b.defenseCursor<b.defenseTargets.Count&&b.heroes.First(x=>x.id==b.defenseTargets[b.defenseCursor]).hp<=0)b.defenseCursor++;if(b.defenseCursor<b.defenseTargets.Count){b.phase="defense-question";return;}b.defenseTargets.Clear();b.enemyCursor++;StartEnemy(b);return;}if(b.heroes.Any(h=>h.hp>0&&!h.acted)){b.phase="heroes";b.selectedHero=b.heroes.FindIndex(h=>h.hp>0&&!h.acted);return;}b.enemyCursor=0;StartEnemy(b);}
 public static void EndHeroes(PartyCombatState b){if(b.phase!="heroes"||!String.IsNullOrEmpty(b.outcome))return;foreach(var h in b.heroes)h.acted=true;b.enemyCursor=0;StartEnemy(b);}
 public static List<PartyUnit> EnemyOrder(PartyCombatState b){return b.enemies.Where(x=>x.hp>0).OrderByDescending(x=>x.speed).ThenBy(x=>x.rank).ToList();}
 static List<PartyUnit> Sequence(PartyCombatState b){return b.enemies.OrderByDescending(x=>x.speed).ThenBy(x=>x.rank).ToList();}
 static void StartEnemy(PartyCombatState b){
  var order=Sequence(b);while(b.enemyCursor<order.Count&&order[b.enemyCursor].hp<=0)b.enemyCursor++;if(b.enemyCursor>=order.Count){NewRound(b);return;}var e=order[b.enemyCursor];var live=b.heroes.Where(x=>x.hp>0).OrderBy(x=>x.rank).ToList();
  if((b.round+b.enemyCursor)%3==2&&e.silence==0){AddShield(e,12);e.acted=true;b.enemyCursor++;b.log=e.name+"扎根，获得12点护盾。";b.phase="enemy-feedback";return;}
  e.silence=0;b.enemyGroup=b.round%3==0;b.enemyDamage=e.attack;b.defenseTargets=b.enemyGroup?live.Select(x=>x.id).ToList():new List<string>{live[(b.round+b.enemyCursor-1)%live.Count].id};
  if(!b.enemyGroup){var guarded=b.heroes.FirstOrDefault(x=>x.hp>0&&x.guard==b.defenseTargets[0]);if(guarded!=null){b.defenseTargets[0]=guarded.id;guarded.guard=null;}}
  b.defenseCursor=0;b.phase="defense-question";
 }
 public static void DefenseAnswer(PartyCombatState b,bool correct){
  if(b.phase!="defense-question")return;var h=b.heroes.First(x=>x.id==b.defenseTargets[b.defenseCursor]);var e=Sequence(b)[b.enemyCursor];int damage=b.enemyDamage/(correct?2:1),absorbed=Math.Min(h.shield,damage);h.shield-=absorbed;h.hp=Math.Max(0,h.hp-damage+absorbed);if(absorbed>0&&h.id=="aelia")h.resolve=Math.Min(3,h.resolve+1);
  if(b.enemyGroup)h.burn=Math.Max(h.burn,2);string counter="";if(h.riposte>0){h.riposte--;int hit=h.attack/2,blocked=Math.Min(e.shield,hit);e.shield-=blocked;e.hp=Math.Max(0,e.hp-hit+blocked);counter="\n反击造成"+(hit-blocked)+"伤害。";}
  b.questions++;b.log=h.name+" · "+(correct?"答对，伤害减半":"答错，承受完整伤害")+"\n护盾吸收"+absorbed+"，生命减少"+(damage-absorbed)+counter+(b.enemyGroup?"\n附带灼烧2回合。":"");Outcome(b);b.phase="feedback";
 }
 static void NewRound(PartyCombatState b){b.round++;foreach(var u in b.heroes.Concat(b.enemies)){if(u.hp<=0)continue;int dot=(u.bleed>0?4:0)+(u.burn>0?4:0);u.hp=Math.Max(0,u.hp-dot);u.bleed=Math.Max(0,u.bleed-1);u.burn=Math.Max(0,u.burn-1);u.broken=Math.Max(0,u.broken-1);u.anchored=Math.Max(0,u.anchored-1);u.markTurns=Math.Max(0,u.markTurns-1);if(u.markTurns==0)u.marks=0;u.acted=false;foreach(var key in u.cooldowns.Keys.ToList())u.cooldowns[key]=Math.Max(0,u.cooldowns[key]-1);}Outcome(b);b.phase=String.IsNullOrEmpty(b.outcome)?"heroes":"feedback";b.selectedHero=Math.Max(0,b.heroes.FindIndex(h=>h.hp>0));b.log="第"+b.round+"回合 · 持续伤害已结算，护盾保留。";}
 static void Outcome(PartyCombatState b){if(b.enemies.All(x=>x.hp<=0))b.outcome="won";else if(b.heroes.All(x=>x.hp<=0))b.outcome="lost";}
 public static void Sync(RogueRun r){var b=r.partyBattle;r.hp=b.heroes.Sum(x=>x.hp);r.maxHp=b.heroes.Sum(x=>x.maxHp);r.enemyHp=b.enemies.Sum(x=>x.hp);r.enemyMax=b.enemies.Sum(x=>x.maxHp);r.feedback=b.log;r.state=String.IsNullOrEmpty(b.outcome)?"combat":b.outcome;r.won=b.outcome=="won";}
}
