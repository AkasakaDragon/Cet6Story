using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

public static class PrologueV2 {
 public const string Version="prologue-exact-20261014";
 public static bool Is(Chapter c){return TavernStory.Is(c)&&c.lines!=null&&c.lines.Count>0&&c.lines[0].sceneCaption!=null;}
 public static string CgFlag(string cg){return Version+"-"+cg;}
 static int FiresideIndex(int old){return old+(old>=73?1:0)+(old>=93?1:0);}
 public static void MigrateFireside(Save s){
  const string flag="prologue-fireside-20261014";
  if(s==null||s.storyFlags==null||s.storyFlags.Contains(flag))return;
  if(s.storyFlags.Contains(Version)){
   int old;if(s.positions.TryGetValue(TavernStory.Id,out old))s.positions[TavernStory.Id]=FiresideIndex(old);
   string prefix=TavernStory.Id+"/line/";
   for(int i=0;i<s.heardLines.Count;i++)if(s.heardLines[i].StartsWith(prefix)&&int.TryParse(s.heardLines[i].Substring(prefix.Length),out old))s.heardLines[i]=prefix+FiresideIndex(old);
   SectionAttempt a;if(s.sectionAttempts.TryGetValue(TavernStory.Id,out a)&&a.replayAfterLine>=0)a.replayAfterLine=FiresideIndex(a.replayAfterLine);
  }
  s.storyFlags.Add(flag);
 }
 public static void Migrate(Save s){
  if(s==null||s.storyFlags==null||s.storyFlags.Contains(Version))return;
  if(s.positions.ContainsKey(TavernStory.Id))s.positions[TavernStory.Id]=0;
  s.heardLines.RemoveAll(k=>k.StartsWith(TavernStory.Id+"/line/"));
  s.sectionAttempts.Remove(TavernStory.Id);
  if(s.tavernBattle!=null&&s.tavernBattle.id=="tavern-tutorial")s.tavernBattle=null;
  s.storyFlags.Add(Version);
 }
 public static long AudioEnd(Chapter c,int index,long end){
  if(!Is(c))return end;
  if(c.lines[index].archiveScene)return Math.Min(end,(long)(c.lines[index].end*1000));
  for(int i=index+1;i<c.lines.Count;i++)if(!String.IsNullOrEmpty(c.lines[i].cgBefore))return Math.Min(end,(long)(c.lines[i-1].end*1000));
  return end;
 }
 public static bool Visible(Line line,string actor){return line!=null&&!line.hidePortraits&&line.portraitIds!=null&&line.portraitIds.Contains(actor);}
 public static double Seconds(string cg){return cg=="cg07"?6:cg=="cg06"?14:cg=="cg03"?8:4;}
}

public partial class Game {
 string prologueActiveCg="";OpeningCgCanvas prologueCgCanvas;
 System.Media.SoundPlayer prologueRain;int prologueRainVolume=-1;
 bool PrologueRainWanted(){
  if(hallListeningQuiet||!PrologueV2.Is(current))return false;
  if(page=="story"){string caption=current.lines[index].sceneCaption??"";return caption.Contains("下雨")||caption.Contains("小雨");}
  if(page!="prologue-v2-cg")return false;
  if(prologueActiveCg=="cg03")return prologueCgCanvas!=null&&!prologueCgCanvas.IsDisposed&&prologueCgCanvas.PlaybackSeconds<5;
  if(prologueActiveCg=="cg06")return prologueCgCanvas!=null&&!prologueCgCanvas.IsDisposed&&prologueCgCanvas.PlaybackSeconds>=11.5;
  return prologueActiveCg=="cg01"||prologueActiveCg=="cg02"||prologueActiveCg=="cg04"||prologueActiveCg=="cg05"||prologueActiveCg=="cg07";
 }
 void StopPrologueRain(){if(prologueRain!=null){prologueRain.Stop();prologueRain.Dispose();prologueRain=null;}prologueRainVolume=-1;}
 void UpdatePrologueRain(){
  int volume=EffectSoundVolume()*18/100;
  if(!PrologueRainWanted()||volume<=0){if(prologueRain!=null)StopPrologueRain();return;}
  if(prologueRain!=null&&prologueRainVolume==volume)return;
  StopPrologueRain();string path=Path.Combine(root,"assets","audio","weather","rain-preview.wav");
  if(!File.Exists(path))return;
  try{prologueRain=new System.Media.SoundPlayer(AudioVolume.Prepare(path,volume,root));prologueRain.Load();prologueRain.PlayLooping();prologueRainVolume=volume;}catch{StopPrologueRain();}
 }
 void ReplayNewPrologue(){
  current=chapters.FirstOrDefault(PrologueV2.Is);if(current==null)return;
  foreach(string flag in save.storyFlags.Where(f=>f.StartsWith(PrologueV2.Version+"-cg")).ToArray())save.storyFlags.Remove(flag);
  index=0;save.positions[current.id]=0;Attempt().review=true;Persist();EnterTavern();
 }
 bool TryNewPrologueCg(){
  if(!PrologueV2.Is(current))return false;
  string cg=current.lines[index].cgBefore;
  if(String.IsNullOrEmpty(cg)||save.storyFlags.Contains(PrologueV2.CgFlag(cg)))return false;
  ShowNewPrologueCg(cg,index);return true;
 }
 void ShowNewPrologueCg(string cg,int destination){
  StopAudio();PauseSectionTiming();ClearPage();page="prologue-v2-cg";
  prologueActiveCg=cg;prologueCgCanvas=null;
  Chapter chapter=current;bool done=false;
  Action finish=()=>{if(done||page!="prologue-v2-cg"||current!=chapter)return;done=true;
   StoryRoutes.Flag(save,PrologueV2.CgFlag(cg));
   if(cg=="cg01")StoryRoutes.Flag(save,TavernStory.OpeningFlag);
   index=destination;save.positions[current.id]=index;save.lastChapter=current.id;Persist();TransitionCgPage(ShowStory);
  };
  try{
   string folder=Path.Combine(root,"assets","story","prologue-v2");
   var canvas=new OpeningCgCanvas(Path.Combine(root,"tools","ffmpeg","ffmpeg.exe"),Path.Combine(folder,cg+".mp4"),AudioDevicePath.Relative(AudioVolume.Prepare(Path.Combine(folder,cg+".wav"),EffectSoundVolume(),root)),PrologueV2.Seconds(cg),cg=="cg07"?1280:800,800){Dock=DockStyle.Fill,FillFrame=true,WaitForContinue=true,EndFadeHoldSeconds=0,SceneCaption=current.lines[Math.Min(destination,current.lines.Count-1)].sceneCaption,SceneCaptionFromSeconds=cg=="cg06"?11.5:cg=="cg03"?5:0};
   if(cg=="cg07")canvas.SceneCaption=current.lines[Math.Max(0,destination-1)].sceneCaption;
   prologueCgCanvas=canvas;content.Controls.Add(canvas);UpdatePrologueRain();canvas.Completed=finish;canvas.Failed=message=>{if(page!="prologue-v2-cg")return;GameMessage.Show(this,"序章CG播放失败："+message,"播放提示");finish();};
   var skip=new VNButton{Text="跳过 CG",PixelStyle=true,Size=new Size(125,46),Font=GameTheme.Body(12)};
   canvas.Controls.Add(skip);skip.Click+=(s,e)=>finish();Action place=()=>skip.Location=new Point(Math.Max(8,canvas.Width-skip.Width-16),16);canvas.Resize+=(s,e)=>place();place();
   var pause=new VNButton{Text="暂停 / 继续",PixelStyle=true,Size=new Size(140,46),Font=GameTheme.Body(12)};canvas.Controls.Add(pause);pause.Location=new Point(16,16);bool stopped=false;pause.Click+=(s,e)=>{stopped=!stopped;canvas.SetPaused(stopped);};canvas.Start();
  }catch(Exception ex){GameMessage.Show(this,"序章CG资源未能播放："+ex.Message,"播放提示");finish();}
 }
 void CompleteNewPrologue(){
  StopAudio();if(Attempt().finished==0)FinishSectionTiming();StoryQuizRules.MarkRead(save,current.id);
  bool fresh=!save.completed.Contains(current.id);if(fresh){save.completed.Add(current.id);save.xp+=60;}
  StoryRoutes.Flag(save,"waystation-basic-repair-complete");StoryRoutes.Flag(save,"prologue-v2-battle-deferred");
  StoryRoutes.Flag(save,TavernStory.BattleFlag);
  if(save.tavernTime==null)save.tavernTime=new TavernTimeState();
  if(save.tavernTime.day<=2){save.tavernTime.day=2;save.tavernTime.phase=StoryTime.Morning;save.tavernTime.weather="rain";}
  Persist();ClearPage();page="tavern-ending";
  content.Controls.Add(new PrologueCompletion{Dock=DockStyle.Fill,BackgroundArt=CachedImage(Engine.SafePath(folders[current.id],current.lines.Last().scene)),Chapter=current,Save=save,Correct=0,Stars=0,FirstCompletion=fresh,
   ReadOnlyStory=true,CompletionTitle="序章完成 · 雨里的一盏灯",CompletionText="第2天早上 · 小雨未停。雨夜应急修缮完成。\n屋面、厨房、净水、楼梯和客房仍须检查；战斗演出暂跳。",
   ReturnHome=ShowMain,Chapters=ShowTavernHub,Replay=ReplayNewPrologue});
 }
}
