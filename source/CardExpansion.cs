using System;
public static partial class CardBattle {
 static void DirectHit(RogueRun r,int amount){var b=r.cardBattle;int damage=(int)Math.Ceiling(amount*(b.vulnerable>0?1.25:1));int blocked=Math.Min(damage,b.enemyShield);b.enemyShield-=blocked;if(blocked>0&&b.enemyShield==0)b.broken=true;r.enemyHp=Math.Max(0,r.enemyHp-damage+blocked);}
 static void ExpansionPlay(RogueRun r,string id){var b=r.cardBattle;switch(id){
  case "scan":b.vulnerable++;break;
  case "quick":DirectHit(r,5);b.attackChain++;Draw(r,1);break;
  case "double":DirectHit(r,3);DirectHit(r,3);b.attackChain+=2;break;
  case "retreat":b.shield+=7;if(b.attackChain>0)Draw(r,1);break;
  case "pursuit":DirectHit(r,7);b.attackChain++;if(b.vulnerable>0&&!b.pursuitUsed){b.energy++;b.pursuitUsed=true;}break;
  case "judgement":b.judgement+=2;break;
  case "expose":if(b.vulnerable>0){DirectHit(r,5);b.attackChain++;}b.vulnerable+=2;break;
  case "execution":DirectHit(r,10+b.attackChain*4);b.attackChain++;break;
  case "spark":DirectHit(r,4);b.attackChain++;b.burning+=3;break;
  case "fuel":b.burning+=5;break;
  case "heatguard":b.shield+=6+Math.Min(5,b.burning/3);break;
  case "hottrack":DirectHit(r,6);b.attackChain++;if(b.burning>0)Draw(r,1);break;
  case "overjet":if(!b.overchargeUsed&&b.energy>0){b.energy--;b.burning+=9;b.overchargeUsed=true;}else b.burning+=4;break;
  case "coolant":Draw(r,1);b.shield+=3;break;
  case "catalyst":b.catalyst+=5;break;
  case "detonate":DirectHit(r,b.burning*2);b.burning=0;b.attackChain++;break;
 }}
 public static string Status(CardBattleState b){return b==null?"":"易伤 "+b.vulnerable+" 回合 · 燃烧 "+b.burning+" · 攻击连击 "+b.attackChain;}
}
