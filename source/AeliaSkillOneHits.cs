using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

public partial class PartyBattleCanvas {
 public event Action<int> SkillOneStrike;
 int skillOneStrikeCount,skillOneStrikeTime=-10000,skillOneStrikeDamage;
 string skillOneHitTarget;
 bool skillOneEvaded;
 readonly Dictionary<string,int> skillOneStartHp=new Dictionary<string,int>();
 readonly Dictionary<string,int> skillOneFinalHp=new Dictionary<string,int>();
 public void PreviewSkillOneDamage(PartyCombatState resolved){
  skillOneStartHp.Clear();skillOneFinalHp.Clear();
  foreach(var u in Battle.enemies){var after=resolved.enemies.FirstOrDefault(x=>x.id==u.id);if(after!=null&&after.hp<u.hp){skillOneStartHp[u.id]=u.hp;skillOneFinalHp[u.id]=after.hp;}}
  skillOneEvaded=skillOneStartHp.Count==0&&(resolved.log??"").Contains("闪避");
 }
 void ClearSkillOneHits(){skillOneStrikeCount=0;skillOneStrikeTime=-10000;skillOneHitTarget=null;skillOneEvaded=false;skillOneStartHp.Clear();skillOneFinalHp.Clear();}
 int VisibleSkillOneHp(PartyUnit u){int before,after;if(IsPixelAeliaSkillOne&&skillOneStartHp.TryGetValue(u.id,out before)&&skillOneFinalHp.TryGetValue(u.id,out after))return before-(before-after)*skillOneStrikeCount/AeliaSkillOneAnimation.StrikeTimes.Length;return u.hp;}
 void TickSkillOneStrikes(){
  if(!IsPixelAeliaSkillOne)return;
  while(skillOneStrikeCount<AeliaSkillOneAnimation.StrikeTimes.Length&&castClock.ElapsedMilliseconds>=AeliaSkillOneAnimation.StrikeTimes[skillOneStrikeCount]){
   int oldCount=skillOneStrikeCount++;skillOneStrikeTime=AeliaSkillOneAnimation.StrikeTimes[oldCount];skillOneHitTarget=skillOneStartHp.Keys.FirstOrDefault()??castTarget;skillOneStrikeDamage=0;
   int before,after;if(skillOneStartHp.TryGetValue(skillOneHitTarget,out before)&&skillOneFinalHp.TryGetValue(skillOneHitTarget,out after)){int total=before-after,n=AeliaSkillOneAnimation.StrikeTimes.Length;skillOneStrikeDamage=total*skillOneStrikeCount/n-total*oldCount/n;}
   var strike=SkillOneStrike;if(strike!=null)strike(skillOneStrikeCount);
  }
 }
 bool DrawSkillOneHit(Graphics g,PartyUnit u,Image image,Rectangle bounds){
  if(!IsPixelAeliaSkillOne||u.id!=skillOneHitTarget||image==null)return false;
  int elapsed=(int)castClock.ElapsedMilliseconds-skillOneStrikeTime;if(elapsed<0||elapsed>=180)return false;
  if(skillOneEvaded){Text(g,"闪避",new Rectangle(bounds.Left,bounds.Top-20,bounds.Width,25),12,GuildChrome.Muted,true);return false;}
  // Move and tint only the struck monster; the camera and heroine never shake.
  int recoil=elapsed<90?4:2;bounds.Offset(recoil,0);
  using(var attributes=new ImageAttributes()){
   float flash=elapsed<65?.38f:0;var matrix=new ColorMatrix();matrix.Matrix40=flash;matrix.Matrix41=flash*.8f;matrix.Matrix42=flash*.65f;attributes.SetColorMatrix(matrix);
   g.DrawImage(image,bounds,0,0,image.Width,image.Height,GraphicsUnit.Pixel,attributes);
  }
  int cx=bounds.Left+bounds.Width/2,cy=bounds.Top+bounds.Height/2;
  if(elapsed<95)using(var ink=new SolidBrush(Color.FromArgb(230,255,225,151)))for(int i=-3;i<=3;i++)g.FillRectangle(ink,cx+i*5,cy-i*5,4,4);
  string label=skillOneStartHp.Count==0?"命中 "+skillOneStrikeCount:skillOneStrikeDamage>0?"−"+skillOneStrikeDamage:"连击 "+skillOneStrikeCount;
  Text(g,label,new Rectangle(cx-60,bounds.Top-20-elapsed/12,120,25),12,Color.FromArgb(255,222,135),true);return true;
 }
}
