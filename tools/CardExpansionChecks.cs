using System;
using System.Linq;
using System.Collections.Generic;
public static class CardExpansionChecks {
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static RogueRun Run(string card){return new RogueRun{state="combat",enemy="combat",hp=100,maxHp=100,enemyHp=200,enemyMax=200,question=new RogueQuestion(),cardBattle=new CardBattleState{version=1,turn=1,energy=10,energyCapacity=10,monster="精英",hand=new List<string>{card},draw=new List<string>{"guide","barrier"}}};}
 static RogueRun Play(string id){var r=Run(id);Check(CardBattle.Play(r,0),"play "+id);return r;}
 public static int Main(){try{
  var timing=Run("guide");var health=new CombatHealthSnapshot(timing);timing.lastDamage=33;timing.lastReceived=8;timing.cardBattle.burnDamage=7;timing.enemyHp=160;timing.hp=92;
  Check(health.EnemyAt(timing,0)==200&&health.EnemyAt(timing,15)==200,"HP unchanged before projectile impact");
  Check(health.EnemyAt(timing,16)==167&&health.EnemyAt(timing,43)==167&&health.EnemyAt(timing,44)==160,"impact damage precedes burn damage");
  Check(health.HeroAt(timing,35)==100&&health.HeroAt(timing,36)==92,"hero damage waits for enemy impact");
  timing.lastDamage=250;timing.enemyHp=0;timing.cardBattle.burnDamage=0;Check(health.EnemyAt(timing,15)==200&&health.EnemyAt(timing,16)==0,"lethal impact waits and clamps health");
  Check(CardBattle.Cards.Length==32&&CardBattle.Cards.Select(c=>c.id).Distinct().Count()==32,"32 distinct cards");
  var p=new RogueProfile();var starter=Run("guide");starter.cardBattle=null;CardBattle.Start(starter,p);var deck=starter.cardBattle.deck;
  Check(deck.Count==8&&deck.Count(x=>x=="duet")==1&&deck.Count(x=>x=="mark")==1&&deck.Count(x=>x=="guide")==3&&deck.Count(x=>x=="barrier")==3,"starter counts");
  Check(starter.cardBattle.hand.Count==4&&starter.cardBattle.draw.Count==4,"initial draw");starter.cardBattle.deck.Add("spark");CardBattle.Start(starter,p);Check(starter.cardBattle.deck.Count==9,"preserve acquired deck");
  var mark=Run("mark");mark.cardBattle.energy=0;Check(CardBattle.Play(mark,0)&&mark.cardBattle.energy==0&&mark.cardBattle.vulnerable==1&&mark.cardBattle.percent==0,"zero-cost vulnerable mark");
  foreach(var id in new[]{"mark","scan","focus","coolant","rethink","rescue","finale"}){
   var consumed=Run(id);consumed.cardBattle.deck=new List<string>{id,"guide"};consumed.cardBattle.draw.Clear();consumed.cardBattle.hand.Add("guide");Check(CardBattle.Play(consumed,0),"consume "+id);
   if(consumed.cardBattle.overflow.Count>0)CardBattle.DiscardChoice(consumed,0);
   for(int turn=0;turn<8;turn++)CardBattle.Begin(consumed,false);
   var state=consumed.cardBattle;Check(state.exhausted.Contains(id)&&!state.draw.Contains(id)&&!state.hand.Contains(id)&&!state.discard.Contains(id)&&state.deck.Contains(id),"exhaust remains isolated "+id);
   var restored=Engine.Json.Deserialize<RogueRun>(Engine.Json.Serialize(consumed));CardBattle.Start(restored,p);Check(restored.cardBattle.exhausted.Count==0&&restored.cardBattle.hand.Contains(id),"next battle restores "+id);
  }
  Check(Play("scan").cardBattle.vulnerable==1,"scan");Check(Play("quick").enemyHp==195,"quick");Check(Play("double").cardBattle.attackChain==2,"double chain");Check(Play("retreat").cardBattle.shield==7,"retreat");
  var r=Run("pursuit");r.cardBattle.vulnerable=2;CardBattle.Play(r,0);Check(r.enemyHp==191&&r.cardBattle.energy==10,"pursuit refund / vulnerability");r.cardBattle.hand.Add("pursuit");CardBattle.Play(r,0);Check(r.cardBattle.energy==9,"refund limited per turn");
  Check(Play("judgement").cardBattle.judgement==2,"judgement");r=Run("expose");r.cardBattle.vulnerable=1;CardBattle.Play(r,0);Check(r.cardBattle.vulnerable==3&&r.enemyHp==193,"expose");
  r=Run("execution");r.cardBattle.attackChain=3;CardBattle.Play(r,0);Check(r.enemyHp==178,"chain finisher");Check(Play("spark").cardBattle.burning==3,"spark");Check(Play("fuel").cardBattle.burning==5,"fuel");
  r=Run("heatguard");r.cardBattle.burning=60;CardBattle.Play(r,0);Check(r.cardBattle.shield==11,"shield cap");r=Run("hottrack");r.cardBattle.burning=1;CardBattle.Play(r,0);Check(r.cardBattle.hand.Count==1,"hot tracking draw");
  r=Play("overjet");Check(r.cardBattle.burning==9&&r.cardBattle.energy==8,"overload cost");r.cardBattle.hand.Add("overjet");CardBattle.Play(r,0);Check(r.cardBattle.burning==13&&r.cardBattle.energy==7,"overload once");r=Run("overjet");r.cardBattle.energy=1;CardBattle.Play(r,0);Check(r.cardBattle.burning==4,"normal jet");
  r=Play("coolant");Check(r.cardBattle.exhausted.Contains("coolant")&&r.cardBattle.shield==3,"coolant exhaust");r=Play("catalyst");CardBattle.Attack(r,10,true);Check(r.cardBattle.burning==5,"clean word catalyst");r=Play("catalyst");CardBattle.Attack(r,10,false);Check(r.cardBattle.burning==0,"assisted catalyst blocked");
  r=Run("detonate");r.cardBattle.burning=10;r.cardBattle.vulnerable=1;CardBattle.Play(r,0);Check(r.enemyHp==175&&r.cardBattle.burning==0,"detonation");
  r=Run("guide");r.cardBattle.burning=7;r.cardBattle.vulnerable=2;r.cardBattle.enemyShield=50;CardBattle.Enemy(r,false,false,0);Check(r.enemyHp==193&&r.cardBattle.burning==5&&r.cardBattle.enemyShield==50&&r.cardBattle.vulnerable==1,"burn bypasses shield / decays");
  r.cardBattle.attackChain=4;r.cardBattle.overchargeUsed=true;CardBattle.Begin(r,false);Check(r.cardBattle.attackChain==0&&!r.cardBattle.overchargeUsed&&r.cardBattle.burning==5,"turn reset retains statuses");
  r=Run("quick");r.cardBattle.draw.Clear();r.cardBattle.discard.Add("barrier");CardBattle.Play(r,0);Check(r.cardBattle.hand.Count==1&&r.cardBattle.draw.Count==1&&r.cardBattle.discard.Count==0,"reshuffle card conservation");
  var saved=Engine.Json.Deserialize<RogueRun>(Engine.Json.Serialize(r));Check(saved.cardBattle.hand.SequenceEqual(r.cardBattle.hand)&&saved.cardBattle.draw.SequenceEqual(r.cardBattle.draw),"save pile roundtrip");
  var rewards=new HashSet<string>();for(int i=0;i<200;i++){r=Run("guide");r.seed=i;r.enemy="elite";p=new RogueProfile{run=r};CardBattle.Reward(p,r);Check(r.cardBattle.rewards.Count==3&&r.cardBattle.rewards.Distinct().Count()==3,"three choices");foreach(var id in r.cardBattle.rewards)rewards.Add(id);}Check(CardBattle.Cards.Skip(16).All(c=>rewards.Contains(c.id)),"all expansion cards offered");
  Console.WriteLine("PASS: starter deck, 16 new cards, energy limits, statuses, learning triggers, wash / save, rewards and deck persistence.");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
