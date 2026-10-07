using System;using System.Collections.Generic;using System.Linq;using System.Drawing;using System.Drawing.Drawing2D;
public partial class PartyBattleCanvas {
 bool monsterCasting;Image monsterActions;readonly HashSet<string> monsterHits=new HashSet<string>();readonly List<string> monsterTargets=new List<string>();
 public void SetMonsterHits(IEnumerable<string> ids){monsterHits.Clear();foreach(var id in ids)monsterHits.Add(id);}
 public void PlayMonsterCast(string id,IEnumerable<string> targets,Action finished){StopCast();monsterCasting=true;castHero=id;monsterTargets.Clear();monsterTargets.AddRange(targets);castFinished=finished;castClock.Restart();castTimer.Tick+=CastTick;castTimer.Start();Invalidate();}
 int MonsterRow(string kind){switch(kind){case "beast":case "spore":return 0;case "crab":return 1;case "moth":return 2;case "bard":return 3;case "cannon":return 4;default:return -1;}}
 bool DrawMonsterAction(Graphics g,PartyUnit unit,Rectangle normal){
  if(!IsCasting)return false;bool attack=monsterCasting&&unit.id==castHero,hit=monsterHits.Contains(unit.id)&&CastVisualAge>=CastHitTime;if(!attack&&!hit)return false;
  int row=MonsterRow(unit.kind);if(row<0)return false;if(monsterActions==null){string path=System.IO.Path.Combine(ArtRoot,"assets","monsters","actions","monster-actions-v2.png");if(!System.IO.File.Exists(path))return false;monsterActions=Image.FromFile(path);}
  float t=CastVisualAge;if(hit&&melee!=null){float recoil=Math.Max(0,1-(t-melee.HitTiming)/180);normal.Offset((int)(6*recoil),0);}if(melee!=null&&attack)normal.Offset((int)(melee.Position(t)-melee.OriginalPosition),0);int frame=attack?(t<180?0:t<750?1:t<1070?2:t<1350?3:0):(t<880?4:5);
  if(melee!=null){if(attack)frame=t<melee.WindupDuration+melee.DashDuration?1:t<melee.HitTiming+melee.HitStopDuration?2:t<melee.HitTiming+melee.HitStopDuration+melee.RecoveryDuration?3:0;else frame=t<melee.HitTiming+170?4:5;}var cell=MonsterAnimationArt.Cell(monsterActions,row,frame);int left=cell.Left,right=cell.Right,top=cell.Top,bottom=cell.Bottom;
  // All six frames use the same cell dimensions, scale and ground anchor.
  float scale=normal.Height/(float)(bottom-top);int w=(int)((right-left)*scale);var bounds=new Rectangle(normal.Left+normal.Width/2-w/2,normal.Bottom-(int)((bottom-top)*scale),w,normal.Height);
  if(attack)DrawMeleeTrail(g,monsterActions,cell,bounds);DrawIsolatedAction(g,monsterActions,new Rectangle(left,top,right-left,bottom-top),bounds,"monster:"+row+":"+frame,true);return true;
 }
 void DrawMonsterEffects(Graphics g,int floor,float height){
  var actor=Battle.enemies.FirstOrDefault(u=>u.id==castHero);if(actor==null)return;float age=TimedVfxAge(CastVisualAge);if(age<440||age>1020)return;
  float x=Width*(.595f+PositionSlot(actor,true)*.11f)-height*.3f,y=floor-height*.58f;var targets=Battle.heroes.Where(u=>monsterTargets.Contains(u.id)).ToList();foreach(var target in targets){float end=Width*(.075f+PositionSlot(target,false)*.11f),t=Math.Max(0,Math.Min(1,(age-440)/300)),head=x+(end-x)*t;Color color=actor.kind=="moth"?Color.FromArgb(151,133,185):actor.kind=="cannon"?Color.FromArgb(199,158,92):Color.FromArgb(156,178,123);
   using(var b=new SolidBrush(Color.FromArgb(age>780?(int)Math.Max(0,220*(1020-age)/240):220,color))){for(int i=0;i<12;i++){float px=Math.Min(x,head+i*8);int size=i<3?6:3;g.FillRectangle(b,(int)px/3*3,(int)(y+Math.Sin(i+age/80)*5)/3*3,size,size);}if(age>740)for(int i=0;i<12;i++){double a=i*2.399;float r=(age-740)/8;g.FillRectangle(b,(int)(end+Math.Cos(a)*r)/3*3,(int)(y+Math.Sin(a)*r)/3*3,6,3);}}
  }
 }
}
