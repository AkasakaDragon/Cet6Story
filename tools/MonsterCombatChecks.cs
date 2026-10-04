using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;

class MonsterCombatChecks {
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Assert(bool condition,string message){if(!condition)throw new Exception(message);}
 static RogueRun Run(int profile,int turn){return new RogueRun{theme=profile<6?profile/2:profile<9?profile-6:0,enemy=profile<6?"combat":profile<9?"elite":"boss",state="feedback",hp=999,maxHp=999,enemyHp=999,enemyMax=999,cardBattle=new CardBattleState{monster=profile<6?CardBattle.Monsters[profile]:profile<9?"精英":"首领",turn=turn,spores=2,heat=2,enemyShield=16}};}
 static object Call(Game game,string name,params object[] args){return typeof(Game).GetMethod(name,Private).Invoke(game,args);}
 static void CheckSounds(RogueRun run){
  using(var arena=new RogueArena{Run=run,Mode="feedback",AnimateHit=true}){
   var heard=new System.Collections.Generic.List<string>();int shots=0,hurt=0;
   arena.MonsterSound=heard.Add;arena.HitSound=player=>{if(player)hurt++;else shots++;};
   for(int frame=0;frame<=52;frame++){typeof(RogueArena).GetField("frame",Private).SetValue(arena,frame);typeof(RogueArena).GetMethod("EmitCombatSounds",Private).Invoke(arena,null);}
   Assert(shots==(run.lastDamage>0?1:0),"hero shot synchronization");Assert(hurt==(run.lastReceived>0?1:0),"player impact synchronization");
   Assert(heard.Count==(run.lastDamage>0?1:0)+(MonsterCombat.Action(run)=="none"?0:1),"enemy effects count");
   if(MonsterCombat.Action(run)!="none")Assert(heard.Last()==MonsterCombat.SoundKey(run,MonsterCombat.Action(run)),"enemy sound identity");
  }
 }
 [STAThread]static int Main(string[] args){try{
  Application.EnableVisualStyles();Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
  var hashes=new System.Collections.Generic.HashSet<string>();using(var sha=SHA256.Create())for(int profile=0;profile<10;profile++)for(int action=0;action<4;action++){
   string path=Path.Combine("assets","rogue","audio","monsters",MonsterAudio.Profiles[profile]+"-"+MonsterAudio.Events[action]+".wav");var clip=RogueAudioMixer.Read(path);
   Assert(clip.Length%2==0&&clip.Length>20000,"PCM format and duration");Assert(clip.Max(x=>Math.Abs((int)x))<30000,"sound clipping");Assert(Math.Abs(clip[0])<100&&Math.Abs(clip[clip.Length-1])<100,"sound envelope boundaries");
   Assert(hashes.Add(Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(path)))),"duplicate sound");
  }
  for(int profile=0;profile<10;profile++)for(int turn=1;turn<=4;turn++)foreach(bool correct in new[]{false,true}){
   var run=Run(profile,turn);string planned=MonsterCombat.PlannedAction(run);run.lastReceived=CardBattle.Enemy(run,correct,correct,5);run.lastDamage=5;
   Assert(run.cardBattle.lastMonsterAction==planned,"resolved action snapshot");CheckSounds(run);
   var saved=Engine.Json.Deserialize<RogueRun>(Engine.Json.Serialize(run));Assert(saved.cardBattle.lastMonsterAction==planned,"persisted action snapshot");
  }
  var interrupted=Run(2,2);CardBattle.Enemy(interrupted,true,true,20);Assert(MonsterCombat.Action(interrupted)=="none","stone interrupt must cancel cast");CheckSounds(interrupted);
  var broken=Run(5,3);broken.cardBattle.broken=true;CardBattle.Enemy(broken,false,false,5);Assert(MonsterCombat.Action(broken)=="none","broken armor must cancel cast");CheckSounds(broken);
  var killed=Run(1,3);killed.enemyHp=0;killed.lastDamage=999;CardBattle.Enemy(killed,false,false,999);Assert(MonsterCombat.Action(killed)=="none","killed enemy must not cast");CheckSounds(killed);
  var blocked=Run(1,2);blocked.cardBattle.shield=100;blocked.lastReceived=CardBattle.Enemy(blocked,false,false,0);Assert(blocked.lastReceived==0&&blocked.cardBattle.lastBlocked>0&&MonsterCombat.Action(blocked)=="normal","fully shielded attack still casts");CheckSounds(blocked);
  var evaded=Run(1,2);CardBattle.Enemy(evaded,true,true,5);Assert(evaded.cardBattle.lastEvaded&&MonsterCombat.Action(evaded)=="normal","evaded attack still casts");CheckSounds(evaded);
  using(var mixer=new RogueAudioMixer(Path.Combine("assets","rogue","audio"))){
   mixer.MusicVolume=0;mixer.EffectsVolume=0;mixer.SetActive(true);mixer.MonsterEffect("wood/normal");Assert(mixer.ActiveEffects==0,"effects mute");
   mixer.EffectsVolume=1;for(int profile=0;profile<10;profile++)mixer.MonsterEffect(MonsterAudio.Profiles[profile]+"/skill-a");Assert(mixer.ActiveEffects>0&&mixer.ActiveEffects<=8,"overlapping enemy sounds");
   System.Threading.Thread.Sleep(80);Assert(mixer.Error==null&&mixer.SubmittedBuffers>0,"audio output feed");mixer.SetActive(false);Assert(mixer.ActiveEffects==0,"leaving battle clears audio");
  }
  using(var window=new Form())using(var arena=new RogueArena{Run=blocked,Mode="feedback",AnimateHit=true}){
   bool completed=false;arena.AnimationCompleted=()=>completed=true;window.Controls.Add(arena);window.Opacity=0;window.Show();arena.RestartAnimation();
   var clock=System.Diagnostics.Stopwatch.StartNew();while(!completed&&clock.Elapsed.TotalSeconds<4){Application.DoEvents();System.Threading.Thread.Sleep(10);}
   Assert(completed,"animation completion callback");window.Close();
  }
  if(args.Length>0){Directory.CreateDirectory(args[0]);using(var game=new Game()){
   var support=new Image[4];for(int i=0;i<4;i++)support[i]=(Image)Call(game,"SupportSprite",i);
   using(var montage=new Bitmap(1200,2500))using(var graphics=Graphics.FromImage(montage)){
    for(int profile=0;profile<10;profile++)for(int action=0;action<3;action++){
     var run=Run(profile,action==0?4:action==1?1:profile==2?2:3);run.lastReceived=CardBattle.Enemy(run,false,false,0);
     // Ember's normal attack is its second turn, while turn four is a cooldown.
     if(profile==4&&action==0){run=Run(profile,2);run.lastReceived=CardBattle.Enemy(run,false,false,0);}
     using(var arena=new RogueArena{Size=new Size(1280,800),Integrated=true,AnimateHit=true,Mode="feedback",Run=run,Art=(Image)Call(game,"TowerBackground",run),Hero=(Image)Call(game,"RogueHero"),Support=support[0],SupportFrames=support,PistolFrames=(Image[])Call(game,"CombatFrames","pistol",false),EnemyArt=(Image)Call(game,"TowerEnemy",run)}){
      typeof(RogueArena).GetField("frame",Private).SetValue(arena,action==1?29:34);
      using(var bitmap=new Bitmap(1280,800)){arena.DrawToBitmap(bitmap,new Rectangle(0,0,1280,800));graphics.DrawImage(bitmap,new Rectangle(action*400,profile*250,400,250));if(profile==1&&action==2)bitmap.Save(Path.Combine(args[0],"monster-skill-b.png"));}
      foreach(int frame in new[]{16,24,29,36,44,51}){arena.Size=new Size(800,600);typeof(RogueArena).GetField("frame",Private).SetValue(arena,frame);using(var bitmap=new Bitmap(800,600))arena.DrawToBitmap(bitmap,new Rectangle(0,0,800,600));}
     }
    }montage.Save(Path.Combine(args[0],"monster-actions.png"));
   }
  }}
  Console.WriteLine("PASS: 40 unique PCM clips; 80 resolved turns; timeline callbacks; persistence; interruption, kill, shield, evasion; all monster actions and small-window rendering.");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
