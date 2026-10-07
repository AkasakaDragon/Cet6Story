using System;using System.Collections.Generic;using System.Linq;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;using System.Diagnostics;

public partial class PartyBattleCanvas {
 public Image MaleActions,FemaleActions;
 Bitmap femalePaletteActions;Image femalePaletteSource;
 Image FemaleMatchedActions(){if(FemaleActions==null)return null;if(femalePaletteActions==null||femalePaletteSource!=FemaleActions){if(femalePaletteActions!=null)femalePaletteActions.Dispose();var reference=FemaleIdle!=null&&FemaleIdle.Length>0?FemaleIdle[0]:Female;if(reference==null)return FemaleActions;femalePaletteActions=HeroActionPalette.Match(FemaleActions,reference);femalePaletteSource=FemaleActions;}return femalePaletteActions;}
 readonly Timer castTimer=new Timer{Interval=16};readonly Stopwatch castClock=new Stopwatch();
 readonly Dictionary<string,int> castFeet=new Dictionary<string,int>();
 int CastFoot(Image image,string hero,int row,int frame,int w,int h){string key=hero+":"+row+":"+frame;int foot;if(castFeet.TryGetValue(key,out foot))return foot;var bitmap=image as Bitmap;foot=h-3;if(bitmap!=null)for(int y=h-1;y>h*3/5;y--){int count=0;for(int x=w/5;x<w*17/20;x++){var c=bitmap.GetPixel(frame*w+x,row*h+y);if(c.A>180&&c.G<180&&c.B<180)count++;}if(count>=5){foot=y+1;break;}}castFeet[key]=foot;return foot;}
 int CastHead(Image image,string hero,int row,int w,int h){string key=hero+":head:"+row;int head;if(castFeet.TryGetValue(key,out head))return head;head=h/10;var b=image as Bitmap;if(b!=null)for(int y=0;y<h/2;y++){int n=0;for(int x=w/3;x<w*2/3;x++){var c=b.GetPixel(x,row*h+y);if(c.A>180&&c.R<180&&c.G<140&&c.B<150)n++;}if(n>=5){head=y;break;}}castFeet[key]=head;return head;}
 Action castHit;bool castHitResolved;
 public void SetCastImpact(Action impact){castHit=impact;castHitResolved=false;}
 public float CastVisualAge{get{float age=castClock.ElapsedMilliseconds;if(melee!=null)return age;return age<CastHitTime?age:age-Math.Min(70,age-CastHitTime);}}
 public int CastDuration{get{return !monsterCasting&&castHero=="luchuan"?1650:1450;}}
 int CastReleaseTime{get{return !monsterCasting&&castHero=="luchuan"?920:750;}}
 float TimedVfxAge(float age){int release=CastReleaseTime,hit=CastHitTime;if(age<release)return age*440/release;if(age<hit)return 440+(age-release)*300/(hit-release);return 740+(age-hit)*310/(CastDuration-hit);}
 int TimedHeroFrame(float age){int[] ends={180,360,550,750,840,1000,1190,1450};for(int i=0;i<ends.Length;i++)if(age<ends[i])return i;return 7;}
 int MaleCastFrame(float age,int row){int[] ends={220,440,620,920,1005,1130,1350,1650};int phase=7;for(int i=0;i<ends.Length;i++)if(age<ends[i]){phase=i;break;}return row==0?new[]{0,1,3,3,4,5,6,7}[phase]:phase;}
 float MaleVfxAge(float age){if(age<920)return age*440/920;if(age<1190)return 440+(age-920)*300/270;return 740+(age-1190)*310/460;}
 int CastHitTime{get{return melee!=null?melee.HitTiming:!monsterCasting&&castHero=="luchuan"?1190:1000;}}
 readonly List<Control> castOverlays=new List<Control>();
 public void DiscardCastOverlays(){foreach(var overlay in castOverlays)if(!overlay.IsDisposed)overlay.Dispose();castOverlays.Clear();}
 public void HideCastOverlay(Control overlay){Controls.Remove(overlay);castOverlays.Add(overlay);}
 string castHero,castTarget,castSkill;int castRow;bool castCorrect;Action castFinished;
 public bool IsCasting {get{return castClock.IsRunning;}}
 public void PlayCast(string hero,string skill,string target,bool correct,Action finished){
  StopCast();castHero=hero;castTarget=target;castSkill=skill;castCorrect=correct;
  var definition=HeroSkills.Get(hero,skill);castRow=skill=="guard"||skill=="screen"||skill=="counter"?2:definition!=null&&definition.Damage>0?0:1;
  castFinished=finished;castTimer.Tick+=CastTick;castClock.Restart();castTimer.Start();Invalidate();
 }
 void CastTick(object sender,EventArgs e){if(melee!=null&&!melee.Resolved&&castClock.ElapsedMilliseconds>=melee.HitTiming){melee.Resolved=true;var hit=melee.Hit;if(hit!=null)hit();}if(melee==null&&castHit!=null&&!castHitResolved&&castClock.ElapsedMilliseconds>=CastHitTime){castHitResolved=true;castHit();}Invalidate();if(castClock.ElapsedMilliseconds<(melee==null?CastDuration+70:melee.Duration))return;var done=castFinished;StopCast();if(done!=null&&!IsDisposed)done();}
 void StopCast(){foreach(var overlay in castOverlays)if(!overlay.IsDisposed){Controls.Add(overlay);overlay.Visible=true;overlay.BringToFront();}castOverlays.Clear();castTimer.Stop();castTimer.Tick-=CastTick;castClock.Reset();castHit=null;castHitResolved=false;melee=null;castHero=castTarget=null;monsterCasting=false;monsterHits.Clear();castFinished=null;}
 bool DrawCastHero(Graphics g,PartyUnit hero,float center,int floor,float bodyHeight,out Rectangle bounds){
  bounds=Rectangle.Empty;if(!IsCasting||hero.id!=castHero)return false;var sheet=hero.id=="aelia"?FemaleMatchedActions():MaleActions;if(sheet==null)return false;
  if(melee!=null)center=melee.Position(CastVisualAge);int frame=melee==null?TimedHeroFrame(CastVisualAge):melee.HeroFrame(CastVisualAge);if(hero.id=="luchuan")frame=MaleCastFrame(CastVisualAge,castRow);int w=sheet.Width/8,h=sheet.Height/3;
  if(hero.id=="luchuan")return DrawMaleRig(g,center,floor,bodyHeight,out bounds);
  int baseFoot=CastFoot(sheet,hero.id,castRow,0,w,h),head=CastHead(sheet,hero.id,castRow,w,h);
  float size=bodyHeight*h/Math.Max(1,baseFoot-head);bounds=new Rectangle((int)(center-size/2),(int)(floor-size*(hero.id=="luchuan"?baseFoot:CastFoot(sheet,hero.id,castRow,frame,w,h))/h),(int)size,(int)size);
  float age=CastVisualAge,total=melee==null?CastDuration:melee.Duration;
  float blend=Math.Min(1,Math.Min(age/130,(total-age)/130));
  if(blend<1){var frames=hero.id=="aelia"?FemaleIdle:MaleIdle;var idleArt=frames!=null&&frames.Length>0?frames[HeroIdleAnimation.FrameIndex%frames.Length]:hero.id=="aelia"?Female:Male;if(idleArt!=null){var idleBounds=frames!=null&&frames.Length>0?Sprite(idleArt,center,floor+bodyHeight*50/395f,bodyHeight*468/395f):Sprite(idleArt,center,floor,bodyHeight);using(var attr=new System.Drawing.Imaging.ImageAttributes()){var matrix=new System.Drawing.Imaging.ColorMatrix();matrix.Matrix33=1-blend;attr.SetColorMatrix(matrix);g.DrawImage(idleArt,idleBounds,0,0,idleArt.Width,idleArt.Height,GraphicsUnit.Pixel,attr);}}}
  DrawMeleeTrail(g,sheet,new Rectangle(frame*w,castRow*h,w,h),bounds);DrawIsolatedAction(g,sheet,new Rectangle(frame*w,castRow*h,w,h),bounds,hero.id+":"+castRow+":"+frame,castRow==0,blend);return true;
 }
 void DrawCastEffects(Graphics g,int floor,float height){
  if(!IsCasting||Battle==null)return;if(melee!=null){DrawMeleeEffects(g,floor,height);return;}if(monsterCasting){DrawMonsterEffects(g,floor,height);return;}var actor=Battle.heroes.FirstOrDefault(h=>h.id==castHero);if(actor==null)return;float age=CastVisualAge;
  float center=Width*(.075f+PositionSlot(actor,false)*.11f),handX=center+height*.30f,handY=floor-height*(actor.id=="luchuan"?.52f:.64f);
  if(actor.id=="luchuan"){var palm=MaleRigPalm(center,floor,height);handX=palm.X;handY=palm.Y;}
  var target=Battle.heroes.Concat(Battle.enemies).FirstOrDefault(h=>h.id==castTarget);float targetX=target==null?Width*.8f:Width*((Battle.enemies.Contains(target)?.595f:.075f)+PositionSlot(target,Battle.enemies.Contains(target))*.11f);
  PixelSkillVfx.Draw(g,castRow,actor.id=="aelia",castSkill,TimedVfxAge(age),handX,handY,targetX,floor,height,castCorrect);
 }
}


