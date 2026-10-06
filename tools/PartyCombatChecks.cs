using System;
using System.Linq;
using System.Collections.Generic;
public static class PartyCombatChecks {
 static void Check(bool b,string message){if(!b)throw new Exception(message);}
 static PartyCombatState New(){return PartyCombat.Create(new RogueRun{enemyHp=300,enemyMax=300,seed=1,cardBattle=new CardBattleState{monster="感染兽"}},new Save());}
 static void Use(PartyCombatState b,int h,string id,string target,bool correct){Check(PartyCombat.BeginSkill(b,h,id,target),"begin "+id);PartyCombat.AttackAnswer(b,correct);}
 static void Defense(PartyCombatState b,string hero,int damage,bool correct){b.phase="defense-question";b.enemyCursor=0;b.defenseTargets=new List<string>{hero};b.defenseCursor=0;b.enemyDamage=damage;PartyCombat.DefenseAnswer(b,correct);}
 public static void Main(){
  Check(HeroSkills.Knight.Length==12&&HeroSkills.Mage.Length==12,"12 skills each");var b=New();Check(b.heroes.All(h=>h.skills.Count==5),"5 equipped skills");
  Use(b,0,"bleed","enemy-0",false);Check(b.enemies[0].hp==293&&b.enemies[0].bleed==0,"wrong answer half damage, no bleed");int hp=b.enemies[0].hp;PartyCombat.AttackAnswer(b,true);Check(b.enemies[0].hp==hp,"no duplicate resolution");
  b=New();Use(b,0,"bleed","enemy-0",true);Check(b.enemies[0].hp==285&&b.enemies[0].bleed==3,"correct damage and bleed");
  b=New();Use(b,0,"bash","enemy-0",false);Check(b.heroes[0].shield==0,"wrong attacking skill gives no shield");
  b=New();Use(b,0,"guard","luchuan",false);Check(b.heroes[0].shield==8&&b.heroes[0].guard==null,"wrong support gives half shield without guard");
  b=New();var knight=b.heroes[0];PartyCombat.AddShield(knight,16);Defense(b,"aelia",22,true);Check(knight.hp==100&&knight.shield==5&&knight.resolve==1,"defense half before shield, gain resolve");
  b=New();knight=b.heroes[0];PartyCombat.AddShield(knight,16);Defense(b,"aelia",22,false);Check(knight.hp==94&&knight.shield==0,"wrong defense full damage spills to HP");PartyCombat.AddShield(knight,999);Check(knight.shield==40,"shield cap 40 percent");
  b=New();b.enemies.Add(new PartyUnit{id="enemy-1",name="兽2",hp=100,maxHp=100,rank=2,speed=3,attack=20});Use(b,0,"sweep","enemy-0",true);Check(b.enemies[0].hp<300&&b.enemies[1].hp<100&&b.questions==1,"friendly AoE one question");
  b=New();b.round=3;PartyCombat.EndHeroes(b);Check(b.phase=="defense-question"&&b.defenseTargets.Count==2,"enemy AoE queues each hero");PartyCombat.DefenseAnswer(b,true);Check(b.questions==1&&b.heroes[0].hp==90&&b.heroes[1].hp==75,"first target only");PartyCombat.Continue(b);PartyCombat.DefenseAnswer(b,false);Check(b.questions==2&&b.heroes[1].hp==55,"second target separate answer");PartyCombat.Continue(b);Check(b.round==4&&b.heroes[0].hp==86&&b.heroes[1].hp==51,"DoT bypasses shield, no question");
  b=New();Use(b,1,"inscribe","enemy-0",true);Check(b.enemies[0].marks==2,"mage marks");PartyCombat.Continue(b);Use(b,0,"slash","enemy-0",true);Check(b.enemies[0].marks==2,"knight never consumes mage marks");
  b=New();b.heroes[1].skills[0]="detonate";b.enemies[0].marks=3;Use(b,1,"detonate","enemy-0",false);Check(b.enemies[0].marks==3,"wrong detonation preserves marks");
  b=New();Use(b,0,"bash","enemy-0",true);Check(b.heroes[0].cooldowns["bash"]==2,"cooldown starts");PartyCombat.Continue(b);Use(b,1,"bolt","enemy-0",true);PartyCombat.Continue(b);PartyCombat.DefenseAnswer(b,true);PartyCombat.Continue(b);Check(!PartyCombat.CanUse(b,b.heroes[0],HeroSkills.Get("aelia","bash")),"cooldown blocks next round");
  b=Engine.Json.Deserialize<PartyCombatState>(Engine.Json.Serialize(b));Check(b.heroes[0].shield>=0&&b.round==2,"save serialization");
  b=New();b.round=2;PartyCombat.EndHeroes(b);Check(b.phase=="enemy-feedback"&&b.enemies[0].shield==12,"enemy shield turn");PartyCombat.Continue(b);Check(b.phase=="heroes"&&b.round==3,"enemy buff advances without fake question");
  var save=new Save();Check(!HeroSkills.Unlocked(save,"aelia",5),"locked initial skill");save.xp=100;Check(HeroSkills.Unlocked(save,"aelia",5)&&!HeroSkills.Unlocked(save,"aelia",6),"level unlock separate from quest");
  b=New();int steps=0;while(String.IsNullOrEmpty(b.outcome)&&steps++<150){if(b.phase=="heroes"){int i=b.heroes.FindIndex(h=>h.hp>0&&!h.acted);if(i<0){PartyCombat.EndHeroes(b);continue;}var fighter=b.heroes[i];string id=fighter.id=="aelia"?"slash":"bolt";Use(b,i,id,"enemy-0",true);}else if(b.phase=="defense-question")PartyCombat.DefenseAnswer(b,true);else PartyCombat.Continue(b);}Check(b.outcome=="won"&&b.heroes.Any(h=>h.hp>0),"complete combat winnable");
  b=New();b.round=3;PartyCombat.EndHeroes(b);PartyCombat.DefenseAnswer(b,true);b=Engine.Json.Deserialize<PartyCombatState>(Engine.Json.Serialize(b));PartyCombat.Continue(b);Check(b.phase=="defense-question"&&b.defenseCursor==1,"save resumes second AoE target without repeating first");
  b=New();b.heroes.Add(new PartyUnit{id="ally-3",name="队员3",hp=80,maxHp=80,rank=3});b.heroes.Add(new PartyUnit{id="ally-4",name="队员4",hp=80,maxHp=80,rank=4});b.round=3;PartyCombat.EndHeroes(b);Check(b.defenseTargets.Count==4,"four-person enemy AoE");for(int i=0;i<4;i++){PartyCombat.DefenseAnswer(b,i%2==0);if(i<3)PartyCombat.Continue(b);}Check(b.questions==4&&b.heroes[2].hp==70&&b.heroes[3].hp==60,"four independent defense answers");
  Console.WriteLine("PASS: 12 skills / 5 slots, answer rules, shield cap, own combos, AoE queue, cooldowns, persistence, unlocks");
 }
}
