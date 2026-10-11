using System.Drawing;
using System.Linq;
public partial class PartyBattleCanvas {
 bool HasPixelAelia{get{return FemalePixelAttack!=null&&FemalePixelAttack.Length==AeliaPixelAnimation.FrameCount;}}
 bool IsPixelAeliaSkillOne{get{return !monsterCasting&&castHero=="aelia"&&castSkill=="slash"&&FemaleSkillOneAttack!=null&&FemaleSkillOneAttack.Length==AeliaSkillOneAnimation.FrameCount;}}
 bool IsPixelAeliaAttack{get{return HasPixelAelia&&!monsterCasting&&castHero=="aelia"&&castRow==0;}}
 bool DrawPixelAelia(Graphics g,PartyUnit hero,float center,int floor,float height,out Rectangle bounds) {
  bounds=Rectangle.Empty;if(!IsCasting||hero.id!=castHero)return false;
  if(IsPixelAeliaSkillOne){int index=AeliaSkillOneAnimation.FrameAt(CastVisualAge);var target=Battle.enemies.FirstOrDefault(u=>u.id==castTarget);if(target!=null)center=AeliaSkillOneAnimation.PlacementCenter(center,Width*(.595f+PositionSlot(target,true)*.11f),height,index);bounds=AeliaSkillOneAnimation.Bounds(center,floor,height,index);g.DrawImage(FemaleSkillOneAttack[index],bounds);return true;}
  if(melee!=null)center=melee.Position(CastVisualAge);
  int frame=castRow==0?AeliaPixelAnimation.FrameAt(CastVisualAge):0;
  bounds=AeliaPixelAnimation.Bounds(center,floor,height);
  g.DrawImage(FemalePixelAttack[frame],bounds);return true;
 }
}
