using System;

public sealed class CombatHealthSnapshot {
 public int Hero,Enemy;
 public CombatHealthSnapshot(RogueRun run){Hero=run.hp;Enemy=run.enemyHp;}
 public int EnemyAt(RogueRun run,int frame){if(frame<16)return Enemy;if(frame<36)return Math.Max(0,Enemy-run.lastDamage);if(frame<44)return run.enemyHp+(run.cardBattle==null?0:run.cardBattle.burnDamage);return run.enemyHp;}
 public int HeroAt(RogueRun run,int frame){return frame<36?Hero:run.hp;}
}
public partial class Game {CombatHealthSnapshot pendingHealthPresentation;}
public partial class RogueArena {
 public CombatHealthSnapshot HealthPresentation;
 int PresentedEnemyHp{get{return AnimateHit&&Mode=="feedback"&&HealthPresentation!=null?HealthPresentation.EnemyAt(Run,frame):Run.enemyHp;}}
 int PresentedHeroHp{get{return AnimateHit&&Mode=="feedback"&&HealthPresentation!=null?HealthPresentation.HeroAt(Run,frame):Run.hp;}}
}
