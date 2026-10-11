using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

public static class AeliaPixelChecks {
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Pump(int ms){var clock=Stopwatch.StartNew();while(clock.ElapsedMilliseconds<ms){Application.DoEvents();Thread.Sleep(5);}}
 static void Capture(PartyBattleCanvas canvas,string path){using(var bmp=new Bitmap(canvas.Width,canvas.Height)){canvas.DrawToBitmap(bmp,new Rectangle(0,0,bmp.Width,bmp.Height));bmp.Save(path);}}
 [STAThread] public static int Main(string[] args){try{
  string root=AppDomain.CurrentDomain.BaseDirectory,output=Path.Combine(root,"output","aelia-in-game");Directory.CreateDirectory(output);
  Application.EnableVisualStyles();var frames=AeliaPixelAnimation.Load(root);Check(frames!=null&&frames.Length==AeliaPixelAnimation.FrameCount,"Missing attack frames");
  using(var reference=Image.FromFile(Path.Combine(root,"assets","characters","aelia-pixel","attack-reference.gif"))) {
  Check(reference.GetFrameCount(System.Drawing.Imaging.FrameDimension.Time)==AeliaPixelAnimation.FrameCount,"Reference frame count");
  for(int i=0;i<AeliaPixelAnimation.FrameCount;i++){
   Check(AeliaPixelAnimation.FrameAt(AeliaPixelAnimation.FrameStart(i))==i,"Incorrect playback timing");
   reference.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Time,i);
   using(var original=new Bitmap(reference)){
    var actual=(Bitmap)frames[i];Check(actual.Size==new Size(150,150),"Frame dimensions changed");
    for(int y=0;y<150;y++)for(int x=0;x<150;x++){var a=actual.GetPixel(x,y);var b=original.GetPixel(149-x,y);Check(a.A==b.A&&(a.A==0||a.ToArgb()==b.ToArgb()),"Palette or mirroring mismatch at "+i);}
   }
  }
  }
  Check(AeliaPixelAnimation.FrameAt(AeliaPixelAnimation.Duration)==AeliaPixelAnimation.FrameCount-1,"Attack must not loop");
  using(var form=new Form{ClientSize=new Size(1280,800)})
  using(var canvas=new PartyBattleCanvas{Size=new Size(1280,800),FemalePixelAttack=frames,ArtRoot=root,Banner="远征 · 女骑士攻击预览"})
  using(var scene=Image.FromFile(Path.Combine(root,"assets","rogue","tower","forest.png")))
  using(var portrait=Image.FromFile(Path.Combine(root,"assets","characters","aelia-pixel","portrait.png")))
  using(var male=Image.FromFile(Path.Combine(root,"assets","rogue","combat","hero-male.png")))
  using(var enemy=Image.FromFile(Path.Combine(root,"assets","rogue","combat","spear-knight-thrust.png"))){
   canvas.Scene=scene;canvas.Female=portrait;canvas.Male=male;canvas.Enemy=enemy;
   var battle=new PartyCombatState{active="aelia",phase="attack-question",selectedSkill="bleed",target="enemy-0"};
   battle.heroes.Add(new PartyUnit{id="aelia",name="艾莉娅",rank=1,hp=100,maxHp=100,speed=4});
   battle.heroes.Add(new PartyUnit{id="luchuan",name="陆川",rank=3,hp=75,maxHp=75,speed=6});
   battle.enemies.Add(new PartyUnit{id="enemy-0",name="训练敌人",kind="beast",rank=1,hp=110,maxHp=110,speed=2});
   canvas.Battle=battle;canvas.Skill=HeroSkills.Get("aelia","bleed");form.Controls.Add(canvas);form.CreateControl();canvas.CreateControl();
   Capture(canvas,Path.Combine(output,"idle.png"));int impacts=0,completions=0;
   canvas.PlayMeleeAttackSequence("aelia","enemy-0","bleed",false,true,()=>impacts++,()=>completions++);
   var flags=BindingFlags.Instance|BindingFlags.NonPublic;var melee=(MeleeSequence)typeof(PartyBattleCanvas).GetField("melee",flags).GetValue(canvas);
   Check(melee.Duration==AeliaPixelAnimation.Duration&&melee.HitTiming==AeliaPixelAnimation.ImpactTime,"Impact not synchronized with slash frame");
   Check(melee.ScreenShakeStrength==0&&!melee.AfterimageEnabled&&melee.Position(500)==melee.OriginalPosition,"Unexpected shake or ghost frames");
   Pump(250);canvas.SetCastPaused(true);float frozen=canvas.CastVisualAge;Pump(180);Check(canvas.CastVisualAge==frozen&&impacts==0,"Pause changed attack age");canvas.SetCastPaused(false);
   var clock=Stopwatch.StartNew();int capture=0;int[] moments={100,400,650,1000};
   while(canvas.IsCasting&&clock.ElapsedMilliseconds<4600){
    Application.DoEvents();if(capture<moments.Length&&clock.ElapsedMilliseconds>=moments[capture]){Capture(canvas,Path.Combine(output,"attack-"+capture+".png"));capture++;}Thread.Sleep(5);
   }
   Check(impacts==1&&completions==1&&!canvas.IsCasting,"Attack did not finish exactly once");Capture(canvas,Path.Combine(output,"returned-idle.png"));
   canvas.FemaleSkillOneAttack=AeliaSkillOneAnimation.Load(root);Check(canvas.FemaleSkillOneAttack!=null&&canvas.FemaleSkillOneAttack.Length==60,"Missing second attack frames");
   Check(AeliaSkillOneAnimation.PlacementCenter(180,760,250,15)==180&&AeliaSkillOneAnimation.PlacementCenter(180,760,250,16)==180,"Must remain at original position through blink disappearance");Check(AeliaSkillOneAnimation.PlacementCenter(180,760,250,17)>760,"Frame18 must teleport behind target");Check(AeliaSkillOneAnimation.SourceRootX(15)==420,"Frame16 source root must remain fixed");
   for(int i=0;i<60;i++){Check(canvas.FemaleSkillOneAttack[i].Size==new Size(1080,600),"Second attack canvas changed");Check(AeliaSkillOneAnimation.FrameAt(AeliaSkillOneAnimation.FrameStart(i))==i,"Second attack skipped frame");}
   foreach(int i in new[]{16,31}){var empty=(Bitmap)canvas.FemaleSkillOneAttack[i];for(int y=0;y<600;y++)for(int x=0;x<1080;x++)Check(empty.GetPixel(x,y).A==0,"Blink frame must remain empty");}
   int strikes=0;canvas.SkillOneStrike+=n=>{strikes++;Check(n==strikes,"Strike repeated or skipped");var visible=(int)typeof(PartyBattleCanvas).GetMethod("VisibleSkillOneHp",flags).Invoke(canvas,new object[]{battle.enemies[0]});Check(visible==110-23*n/AeliaSkillOneAnimation.StrikeTimes.Length,"Combo health must distribute total damage without multiplying it");Check(battle.enemies[0].hp==110,"Preview must not mutate real combat health");};impacts=0;completions=0;canvas.Skill=HeroSkills.Get("aelia","slash");canvas.PlayMeleeAttackSequence("aelia","enemy-0","slash",false,true,()=>impacts++,()=>completions++);
   melee=(MeleeSequence)typeof(PartyBattleCanvas).GetField("melee",flags).GetValue(canvas);
   var predicted=new PartyCombatState();predicted.enemies.Add(new PartyUnit{id="enemy-0",hp=87});canvas.PreviewSkillOneDamage(predicted);
   Check(canvas.CastDuration==5680&&melee.Duration==5680&&melee.HitTiming==4440,"Second attack impact timing");
   Check(melee.ScreenShakeStrength==0&&!melee.AfterimageEnabled,"Second attack adds shake or trails");
   for(int ms=0;ms<=5680;ms+=60)Check(melee.Position(ms)==melee.OriginalPosition,"Second attack adds duplicate motion");
   clock.Restart();bool captured=false;while(canvas.IsCasting&&clock.ElapsedMilliseconds<9000){Application.DoEvents();if(!captured&&clock.ElapsedMilliseconds>2700){Capture(canvas,Path.Combine(output,"bleed-attack.png"));captured=true;}Thread.Sleep(5);}
   Check(strikes==6&&impacts==1&&completions==1&&!canvas.IsCasting,"Six strike reactions and one total damage resolution required");Capture(canvas,Path.Combine(output,"bleed-returned-idle.png"));

  }
  Console.WriteLine("PASS: 38 original-color mirrored frames; fixed pixel canvas; synchronized single impact; pause/resume; return to idle; first skill 60 frames, six reactions and distributed HP, one total resolution; second skill 38 frames.");return 0;
 }catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
