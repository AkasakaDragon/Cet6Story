using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public static class MonsterCombat {
 public static int Profile(RogueRun run){if(run.enemy=="boss")return 9;if(run.enemy=="elite")return 6+Math.Max(0,Math.Min(2,run.theme));int index=Array.IndexOf(CardBattle.Monsters,run.cardBattle==null?null:run.cardBattle.monster);return index>=0?index:Math.Max(0,Math.Min(2,run.theme))*2;}
 public static string PlannedAction(RogueRun run){
  int phase=run.cardBattle==null?3:Math.Max(0,run.cardBattle.turn-1)%4;
  if(Profile(run)==2){if(phase==0)return "skill-a";if(phase==1)return "skill-b";return phase==2?"none":"normal";}
  if(Profile(run)==4&&phase==3)return "none";
  return phase==0?"skill-a":phase==2?"skill-b":"normal";
 }
 public static string Action(RogueRun run){return run.cardBattle!=null&&!String.IsNullOrEmpty(run.cardBattle.lastMonsterAction)?run.cardBattle.lastMonsterAction:run.enemyHp<=0?"none":PlannedAction(run);}
 public static string SoundKey(RogueRun run,string action){return MonsterAudio.Profiles[Profile(run)]+"/"+action;}
 static int Family(int profile){return profile<6?profile:profile==6?0:profile==7?2:profile==8?5:3;}
 static Color Palette(int profile){Color[] colors={Color.FromArgb(148,173,112),Color.FromArgb(168,132,176),Color.FromArgb(169,184,184),Color.FromArgb(125,178,204),Color.FromArgb(210,144,91),Color.FromArgb(193,152,114)};return profile==9?Color.FromArgb(189,174,114):colors[Family(profile)];}
 public static Rectangle Pose(RogueRun run,Rectangle rect,int frame){
  if(run.lastDamage>0&&frame>=16&&frame<23)rect.Offset((int)(Math.Sin((frame-16)*2.7)*5),0);
  if(Action(run)=="none"||frame<22||frame>47)return rect;
  float t=(frame-22)/25f,wave=(float)Math.Sin(t*Math.PI);string action=Action(run);
  if(action=="normal"){rect.Offset(-(int)(wave*(Profile(run)==2?60:115)),-(int)(wave*12));}
  else{int stretch=(int)(wave*rect.Height*(action=="skill-a"?.035:.055));rect.Y-=stretch;rect.Height+=stretch;rect.Inflate((int)(wave*3),0);}
  return rect;
 }
 public static void Draw(Graphics g,RogueRun run,Rectangle enemy,Rectangle hero,int frame){PixelBattleEffects.Monster(g,run,enemy,hero,frame);}
}
