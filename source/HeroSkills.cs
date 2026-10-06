using System;
using System.Linq;
using System.Collections.Generic;

public class HeroSkill {
 public string Id,Name,Description,Icon,Target;public int Index,Cooldown,Uses,Damage,Shield;public bool Group;
}
public static class HeroSkills {
 public static readonly HeroSkill[] Knight={
  S("slash","骑士斩击","单体100%伤害；消耗自身决意，每层增伤15%。","sword","enemy",0,100),
  S("bleed","裂伤剑锋","单体70%伤害；流血3回合，每回合4点。","bleed","enemy",0,70),
  S("bash","盾击破阵","单体50%伤害；削减敌盾12点，获得8点护盾。","shield","enemy",1,50,8),
  S("guard","守护誓言","获得16点护盾，替一名队友承受下一次单体攻击。","guard","ally",2,0,16),
  S("sweep","横扫剑弧","敌方前两位各65%伤害；群攻只答一次。","sweep","enemy",1,65,0,true),
  S("execute","决意斩杀","单体120%伤害；生命低于30%的敌人受到180%伤害。","sword","enemy",2,120),
  S("advance","挺身突进","向前移动一位，单体90%伤害，获得10点护盾。","move","enemy",1,90,10),
  S("counter","架盾反击","获得18点护盾，接下来两次受击各反击50%伤害。","counter","self",2,0,18),
  S("break","破甲重斩","单体80%伤害；破甲2回合，自身攻击额外增伤25%。","break","enemy",2,80),
  S("recover","坚韧整备","恢复自身15%最大生命，解除流血，获得12点护盾。","heal","self",0,0,12,false,2),
  S("stand","不退之阵","获得30点护盾，抵抗位移至下回合结束。","shield","self",3,0,30),
  S("pursuit","血痕追击","单体110%伤害；流血目标受到150%伤害，流血延长1回合。","bleed","enemy",2,110)
 };
 public static readonly HeroSkill[] Mage={
  S("bolt","言契冲击","单体85%伤害；施加1层符印，最多3层。","bolt","enemy",0,85),
  S("inscribe","铭刻符印","单体30%伤害；施加2层符印，持续3回合。","rune","enemy",0,30),
  S("burn","封印灼痕","单体50%伤害；灼烧3回合，每回合4点，并施加1层符印。","fire","enemy",1,50),
  S("wave","符文震荡","敌方后两位各60%伤害，各施加1层符印。","wave","enemy",2,60,0,true),
  S("bandage","应急包扎","治疗一名队友20%最大生命，并解除流血。","heal","ally",0,0,0,false,2),
  S("screen","共鸣屏障","自身获得14点护盾；下一次施加符印额外增加1层。","shield","self",2,0,14),
  S("detonate","符印引爆","单体60%伤害；每消耗1层符印增加40%，最多180%。","burst","enemy",1,60),
  S("silence","禁言封咒","单体50%伤害；消耗1层符印，阻止敌人下一次治疗或增益。","rune","enemy",2,50),
  S("embers","余烬回响","单体70%伤害；灼烧目标立即跳伤一次，并获得1层符印。","fire","enemy",2,70),
  S("spread","符印扩散","单体40%伤害；将符印复制到相邻敌人，原目标保留。","wave","enemy",2,40),
  S("retreat","折跃退避","向后移动一位，获得8点护盾，符印引爆冷却减少1回合。","move","self",2,0,8),
  S("collapse","言契崩解","全体40%伤害；消耗各目标符印，每层增加30%伤害。","burst","enemy",0,40,0,true,1)
 };
 static HeroSkill S(string id,string name,string desc,string icon,string target,int cd,int damage,int shield=0,bool group=false,int uses=0){return new HeroSkill{Id=id,Name=name,Description=desc,Icon=icon,Target=target,Cooldown=cd,Damage=damage,Shield=shield,Group=group,Uses=uses};}
 static HeroSkills(){for(int i=0;i<12;i++){Knight[i].Index=i;Mage[i].Index=i;}}
 public static HeroSkill[] For(string hero){return hero=="aelia"?Knight:Mage;}
 public static HeroSkill Get(string hero,string id){return For(hero).FirstOrDefault(s=>s.Id==id);}
 public static bool Unlocked(Save save,string hero,int i){if(i<5)return true;int level=Engine.Level(save.xp);switch(i){case 5:return level>=2;case 6:return save.storyFlags.Contains(hero+"-sidequest-1");case 7:return level>=4;case 8:return save.storyFlags.Contains(hero+"-sidequest-2");case 9:return level>=6;case 10:return save.storyFlags.Contains(hero+"-advanced");default:return save.storyFlags.Contains(hero+"-story-complete");}}
 public static string UnlockText(int i){return new[]{"初始技能","初始技能","初始技能","初始技能","初始技能","达到2级","完成首次角色支线","达到4级","完成第二次角色支线","达到6级","完成角色进阶任务","完成角色关键剧情"}[i];}
}
