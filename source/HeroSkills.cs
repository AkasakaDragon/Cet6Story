using System;using System.Linq;using System.Collections.Generic;
public class HeroSkill {public string Id,Name,Description,Icon,Target;public int Index,Cooldown,Uses,Damage,Shield;public bool Group;public int[] CastRanks,TargetRanks;}
public static class HeroSkills {
 static int[] R(params int[] a){return a;}
 static HeroSkill S(string id,string name,string desc,string icon,string target,int damage,int cd,int[] cast,int[] targets,bool group=false,int uses=0){return new HeroSkill{Id=id,Name=name,Description=desc,Icon=icon,Target=target,Damage=damage,Cooldown=cd,CastRanks=cast,TargetRanks=targets,Group=group,Uses=uses};}
 public static readonly HeroSkill[] Knight={
 S("slash","骑士斩击","单体100%；消耗1符印无视闪避。","sword","enemy",100,0,R(1,2,3),R(1,2)),
 S("bleed","裂伤剑锋","单体50%；流血3点×3次行动。","bleed","enemy",50,0,R(1,2),R(1,2)),
 S("bash","盾击破阵","单体35%；先移除1格挡，命中打断准备；符印额外移除1格挡。","shield","enemy",35,1,R(1,2),R(1,2,3)),
 S("guard","守护誓言","守护另一名队友下一次单体攻击，自身获得1格挡。","guard","ally",0,1,R(1,2,3,4),R(1,2,3,4)),
 S("advance","挺身突进","单体75%，前移1格；消耗1决意获得不退。","move","enemy",75,1,R(2,3,4),R(1,2)),
 S("counter","架盾反击","获得2次50%反击，至下次自身行动结束。","counter","self",0,2,R(1,2),R(1,2)),
 S("execute","决意斩杀","需2决意；100%+每决意25%，全部消耗；敌生命≤30%再加25%。","sword","enemy",100,2,R(1,2),R(1,2)),
 S("recover","坚韧整备","自身生命≤40%；治疗20%，清除流血和菌毒；每战2次。","heal","self",0,0,R(1,2,3,4),R(1,2,3,4),false,2),
 S("sweep","横扫剑弧","敌1、2各55%伤害，一次答题。","sweep","enemy",55,1,R(1,2),R(1,2),true),
 S("inspire","誓约激励","消耗1决意，给予另一队友1强力：下次攻击+50%。","guard","ally",0,2,R(1,2,3,4),R(1,2,3,4))};
 public static readonly HeroSkill[] Mage={
 S("bolt","言契冲击","单体80%；消耗1符印无视闪避。","bolt","enemy",80,0,R(2,3,4),R(1,2,3,4)),
 S("inscribe","铭刻符印","施加2符印，无伤害；不受闪避影响。","rune","enemy",0,0,R(2,3),R(1,2,3,4)),
 S("pull","言契牵引","单体25%，拉近1格；消耗符印，位移成功附易伤。","move","enemy",25,1,R(2,3),R(3,4)),
 S("screen","共鸣屏障","给予一名友军1格挡，清除1虚弱。","shield","ally",0,1,R(2,3),R(1,2,3,4)),
 S("bandage","应急包扎","目标生命≤50%；治疗20%，清除流血和一份菌毒；每战2次。","heal","ally",0,0,R(1,2,3),R(1,2,3,4),false,2),
 S("detonate","符印引爆","需2符印；50%+每符印30%，全部消耗，无视闪避。","burst","enemy",50,1,R(3,4),R(1,2,3,4)),
 S("wave","符文震荡","敌3、4各35%；命中各施加1符印。","wave","enemy",35,1,R(4),R(3,4),true),
 S("silence","禁言封咒","消耗1符印，封咒并取消法术准备，不阻止物理装填。","rune","enemy",0,2,R(3,4),R(2,3,4)),
 S("burn","灼印余烬","单体30%；灼烧3点×3次行动。","fire","enemy",30,1,R(3,4),R(1,2,3,4)),
 S("retreat","折跃退避","后移1格并获得1闪避。","move","self",0,1,R(1,2),R(1,2))};
 static HeroSkills(){for(int i=0;i<Knight.Length;i++)Knight[i].Index=i;for(int i=0;i<Mage.Length;i++)Mage[i].Index=i;}
 public static HeroSkill[] For(string hero){return hero=="aelia"?Knight:Mage;}
 public static HeroSkill Get(string hero,string id){return For(hero).FirstOrDefault(s=>s.Id==id);}
 public static bool Unlocked(Save save,string hero,int i){return i>=0&&i<For(hero).Length;}
 public static string UnlockText(int i){return "已开放 · 驿站装配";}
 public static List<string> Normalize(string hero,IEnumerable<string> ids){var list=(ids??new string[0]).Where(id=>Get(hero,id)!=null).Distinct().Take(5).ToList();foreach(var s in For(hero).Take(5))if(list.Count<5&&!list.Contains(s.Id))list.Add(s.Id);return list;}
}
