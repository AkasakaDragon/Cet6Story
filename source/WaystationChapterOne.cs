using System;using System.IO;using System.Linq;using System.Drawing;using System.Windows.Forms;
public static class WaystationChapterOne {
 public const string Id="tavern-01-01";
 public const string SecondId="tavern-01-02";
 public static readonly string[] Ids={Id,SecondId,"tavern-01-03","tavern-01-04","tavern-01-05","tavern-01-06"};
 public static readonly string[] Names={"老板的第一天","集市：不愿透露姓名的少女","午后：第一批客人","傍晚：一盏灯的来客","旧矿道：第一次主动出发","夜晚：第一次打烊"};
 public static readonly string[] Media={"chapter-one","chapter-two","chapter-three","chapter-four","chapter-five","chapter-six"};
 public static int Number(Chapter c){return c==null?-1:Array.IndexOf(Ids,c.id);}
 public static bool Second(Chapter c){return c!=null&&c.id==SecondId;}
 public static bool Is(Chapter c){return Number(c)>=0;}
 public static bool Dialogue(Chapter c){return TavernStory.Is(c)||Is(c);}
}
public partial class Game {
 void EnterChapterOne(){EnterWaystationSection(WaystationChapterOne.Id);}
 void EnterChapterOneSecond(){EnterWaystationSection(WaystationChapterOne.SecondId);}
 void EnterWaystationSection(string id){var chapter=chapters.FirstOrDefault(c=>c.id==id);if(chapter==null){GameMessage.Show(this,"第一章资源未加载，请保留完整章节文件夹。");return;}if(!SectionRules.Unlocked(chapter,chapters,save)){GameMessage.Show(this,"完成前一节后开放本节；第一节需先完成序幕。");return;}current=chapter;var attempt=Attempt();if(save.completed.Contains(chapter.id)||attempt.review||attempt.finished>0)ResetSection();index=save.positions.ContainsKey(chapter.id)?Math.Max(0,Math.Min(chapter.lines.Count-1,save.positions[chapter.id])):0;save.lastChapter=chapter.id;save.hasGame=true;Persist();ShowStory();}
 bool TryChapterOneIntro(){if(!WaystationChapterOne.Is(current)||Attempt().introShown)return false;PlayChapterOneCg(true);return true;}
 void PlayChapterOneCg(bool intro){
  ClearPage();page="waystation-chapter-cg";string kind=intro?"intro":"outro",section=WaystationChapterOne.Media[WaystationChapterOne.Number(current)];var folder=Path.Combine(root,"chapters","audio","tavern",section);var specs=Engine.Json.Deserialize<System.Collections.Generic.Dictionary<string,ChapterOneCg>>(File.ReadAllText(Path.Combine(folder,"cg.json")));var spec=specs[kind];bool finished=false;
  Action finish=()=>{if(finished||page!="waystation-chapter-cg"||!WaystationChapterOne.Is(current))return;finished=true;if(intro){Attempt().introShown=true;Persist();ShowStory();}else if(WaystationChapterOne.Number(current)==5){ShowChapterOneStayEpilogue();}else{Attempt().endingShown=true;CompleteChapterOne();}};
  try{var canvas=new OpeningCgCanvas(Path.Combine(root,"tools","ffmpeg","ffmpeg.exe"),Path.Combine(root,"assets","opening","tavern",section+"-"+kind+".mp4"),AudioDevicePath.Relative(AudioVolume.Prepare(Path.Combine(folder,kind+".wav"),EffectSoundVolume(),root)),spec.seconds){Dock=DockStyle.Fill,FillFrame=true};content.Controls.Add(canvas);canvas.Completed=()=>TransitionCgPage(finish);canvas.Failed=message=>{GameMessage.Show(this,"章节 CG 播放失败："+message);finish();};
   if(save.english||save.chinese){var captions=new OutlinedLabel{Text=(save.english?spec.en+"\n":"")+(save.chinese?spec.zh:""),Font=GameTheme.Body(16),ForeColor=Color.White,BackColor=Color.FromArgb(190,5,22,30),Height=110,Dock=DockStyle.Bottom};canvas.Controls.Add(captions);}
   var skip=new VNButton{Text="跳过 CG",PixelStyle=true,Size=new Size(125,46),Font=GameTheme.Body(12)};canvas.Controls.Add(skip);Action place=()=>skip.Location=new Point(Math.Max(8,canvas.Width-145),20);canvas.Resize+=(s,e)=>place();place();skip.Click+=(s,e)=>TransitionCgPage(finish);canvas.Start();
  }catch(Exception ex){GameMessage.Show(this,"章节 CG 播放失败："+ex.Message);finish();}
 }
 void CompleteChapterOne(){
  if(Attempt().finished==0)FinishSectionTiming();int correct=current.questions.Count(q=>save.quizAnswers.ContainsKey(InlineKey(q))&&save.quizAnswers[InlineKey(q)]==q.answer);int stars=SectionStars(correct,current.questions.Count);save.sectionStars[current.id]=Math.Max(stars,save.sectionStars.ContainsKey(current.id)?save.sectionStars[current.id]:0);bool fresh=!save.completed.Contains(current.id);if(fresh){save.completed.Add(current.id);save.xp+=60;}bool second=WaystationChapterOne.Second(current);StoryRoutes.Flag(save,second?"waystation-chapter-one-section-two-complete":"waystation-chapter-one-section-one-complete");if(second){StoryRoutes.Flag(save,"lyse-met");StoryRoutes.Flag(save,"rune-lantern-north-clue");}Persist();ClearPage();page="waystation-section-ending";
  int number=WaystationChapterOne.Number(current);StoryRoutes.Flag(save,"waystation-chapter-one-section-"+(number+1)+"-complete");if(number==4)StoryRoutes.Flag(save,"first-seal-fragment");if(number==5){StoryRoutes.Flag(save,"waystation-chapter-one-complete");StoryRoutes.Flag(save,"lyse-royal-identity");}Persist();
  content.Controls.Add(new PrologueCompletion{Dock=DockStyle.Fill,BackgroundArt=CachedImage(Engine.SafePath(folders[current.id],current.lines.Last().scene)),Chapter=current,Save=save,Correct=correct,Stars=stars,FirstCompletion=fresh,CompletionTitle="第"+(number+1)+"节完成 · "+WaystationChapterOne.Names[number],CompletionText=current.ending+"\n"+(number<5?"下一节“"+WaystationChapterOne.Names[number+1]+"”已解锁。":"第一章完成 · 今天，驿站终于迎来了客人。"),ReturnHome=ShowMain,Chapters=ShowTavernMainQuest,Replay=()=>{Attempt().review=true;index=0;ShowStory();ReviewSection();}});
 }
}
public class ChapterOneCg {public string zh{get;set;}public string en{get;set;}public double seconds{get;set;}}

