using System;using System.Linq;using System.Drawing;using System.Drawing.Imaging;
public sealed class MeleeSequence {
 public int WindupDuration=420,DashDuration=110,AttackDuration=300,HitStopDuration=70,RecoveryDuration=180,ReturnDuration=140;public float AttackDistance=100;public bool AfterimageEnabled=true;public int ScreenShakeStrength;
 public float OriginalPosition,Destination;public Action Hit;public bool Resolved;
 public int HitTiming{get{return WindupDuration+DashDuration+AttackDuration;}}public int Duration{get{return HitTiming+HitStopDuration+RecoveryDuration+ReturnDuration;}}
 public float Position(float ms){if(ms<WindupDuration)return OriginalPosition- Math.Sign(Destination-OriginalPosition)*3*(ms/WindupDuration);if(ms<WindupDuration+DashDuration){float t=(ms-WindupDuration)/DashDuration;return OriginalPosition+(Destination-OriginalPosition)*(1-(float)Math.Pow(1-t,3));}int back=HitTiming+HitStopDuration+RecoveryDuration;if(ms<back)return Destination;if(ms>=Duration)return OriginalPosition;float u=(ms-back)/ReturnDuration;return Destination+(OriginalPosition-Destination)*(1-(float)Math.Pow(1-u,3));}
 public int HeroFrame(float ms){if(ms<WindupDuration)return ms<140?0:ms<280?1:2;float attack=ms-WindupDuration-DashDuration;if(attack<0)return 2;if(attack<60)return 3;if(attack<180)return 4;if(ms<HitTiming+HitStopDuration)return 5;if(ms<HitTiming+HitStopDuration+RecoveryDuration)return 6;return 7;}
}
public partial class PartyBattleCanvas {
 MeleeSequence melee;
 public static bool IsMelee(string hero,string skill){var s=HeroSkills.Get(hero,skill);return hero=="aelia"&&s!=null&&s.Damage>0;}
 public static bool IsMonsterMelee(string skill){return skill=="bite"||skill=="claw";}
 public void PlayMeleeAttackSequence(string attacker,string target,string skill,bool enemy,bool correct,Action hit,Action finished){
  if(enemy)PlayMonsterCast(attacker,new[]{target},finished);else PlayCast(attacker,skill,target,correct,finished);
  var a=Battle.heroes.Concat(Battle.enemies).FirstOrDefault(u=>u.id==attacker);var t=Battle.heroes.Concat(Battle.enemies).FirstOrDefault(u=>u.id==target);if(a==null||t==null)return;
  float original=Width*((enemy?.595f:.075f)+PositionSlot(a,enemy)*.11f),goal=Width*((enemy?.075f:.595f)+PositionSlot(t,!enemy)*.11f);float h=Math.Min(Math.Max(80,Height-FooterHeight-185),Math.Min(340,Width*.25f))*.8f;
  melee=new MeleeSequence{OriginalPosition=original,Destination=goal-Math.Sign(goal-original)*h*.72f,AttackDistance=h*.72f,Hit=hit,ScreenShakeStrength=skill=="execute"||skill=="bash"?2:0};
 }
 void DrawMeleeTrail(Graphics g,Image atlas,Rectangle cell,Rectangle bounds){if(melee==null||!melee.AfterimageEnabled)return;float age=castClock.ElapsedMilliseconds;bool outward=age>=melee.WindupDuration&&age<melee.WindupDuration+melee.DashDuration,back=age>=melee.HitTiming+melee.HitStopDuration+melee.RecoveryDuration&&age<melee.Duration;if(!outward&&!back)return;for(int i=2;i>=1;i--){var r=bounds;r.Offset((int)(melee.Position(Math.Max(0,age-i*23))-melee.Position(age)),0);using(var attr=new ImageAttributes()){var matrix=new ColorMatrix();matrix.Matrix33=back?.12f:.18f;attr.SetColorMatrix(matrix);var sprite=ActionFrame(atlas,cell,"trail:"+castHero+":"+cell.X+":"+cell.Y,false);g.DrawImage(sprite,r,0,0,sprite.Width,sprite.Height,GraphicsUnit.Pixel,attr);}}}
 void DrawMeleeEffects(Graphics g,int floor,float height){float age=castClock.ElapsedMilliseconds;if(age<melee.HitTiming||age>melee.HitTiming+melee.HitStopDuration+melee.RecoveryDuration)return;var target=Battle.heroes.Concat(Battle.enemies).FirstOrDefault(u=>u.id==castTarget||monsterTargets.Contains(u.id));if(target==null)return;float tx=Width*((monsterCasting?.075f:.595f)+PositionSlot(target,!monsterCasting)*.11f),y=floor-height*.6f;float fade=age>melee.HitTiming+melee.HitStopDuration?Math.Max(0,1-(age-melee.HitTiming-melee.HitStopDuration)/melee.RecoveryDuration):1;Color c=monsterCasting?Color.FromArgb(177,180,137):Color.FromArgb(232,202,138);using(var b=new SolidBrush(Color.FromArgb((int)(210*fade),c))){if(age>=melee.HitTiming){for(int i=-5;i<=5;i++){g.FillRectangle(b,(int)tx+i*4,(int)y+i*5,4,6);}if(age<melee.HitTiming+melee.HitStopDuration){g.FillRectangle(b,(int)tx-20,(int)y,40,4);g.FillRectangle(b,(int)tx,(int)y-24,4,48);}}}
 }
}
