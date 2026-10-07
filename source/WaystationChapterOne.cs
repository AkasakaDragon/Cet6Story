using System;using System.IO;using System.Linq;using System.Drawing;using System.Windows.Forms;
public static class WaystationChapterOne {
 public const string Id="tavern-01-01";
 public static bool Is(Chapter c){return c!=null&&c.id==Id;}
 public static bool Dialogue(Chapter c){return TavernStory.Is(c)||Is(c);}
}
public partial class Game {
 void EnterChapterOne(){var chapter=chapters.FirstOrDefault(WaystationChapterOne.Is);if(chapter==null){GameMessage.Show(this,"第一章资源未加载，请保留完整章节文件夹。");return;}if(!SectionRules.Unlocked(chapter,chapters,save)){GameMessage.Show(this,"完成序幕后开放第一章第一节。");return;}current=chapter;index=save.positions.ContainsKey(chapter.id)?Math.Max(0,Math.Min(chapter.lines.Count-1,save.positions[chapter.id])):0;save.lastChapter=chapter.id;save.hasGame=true;Persist();ShowStory();}
 bool TryChapterOneIntro(){if(!WaystationChapterOne.Is(current)||Attempt().introShown)return false;PlayChapterOneCg(true);return true;}
 void PlayChapterOneCg(bool intro){
  ClearPage();page="waystation-chapter-cg";string kind=intro?"intro":"outro";var folder=Path.Combine(root,"chapters","audio","tavern","chapter-one");var specs=Engine.Json.Deserialize<System.Collections.Generic.Dictionary<string,ChapterOneCg>>(File.ReadAllText(Path.Combine(folder,"cg.json")));var spec=specs[kind];bool finished=false;
  Action finish=()=>{if(finished||page!="waystation-chapter-cg"||!WaystationChapterOne.Is(current))return;finished=true;if(intro){Attempt().introShown=true;Persist();ShowStory();BeginInvoke((Action)(()=>{if(page=="story"&&WaystationChapterOne.Is(current))PlayCurrent();}));}else{Attempt().endingShown=true;CompleteChapterOne();}};
  try{var canvas=new OpeningCgCanvas(Path.Combine(root,"tools","ffmpeg","ffmpeg.exe"),Path.Combine(root,"assets","opening","tavern","chapter-one-"+kind+".mp4"),AudioDevicePath.Relative(AudioVolume.Prepare(Path.Combine(folder,kind+".wav"),EffectSoundVolume(),root)),spec.seconds){Dock=DockStyle.Fill,FillFrame=true};content.Controls.Add(canvas);canvas.Completed=finish;canvas.Failed=message=>{GameMessage.Show(this,"章节 CG 播放失败："+message);finish();};
   var captions=new OutlinedLabel{Text=(save.english?spec.en+"\n":"")+(save.chinese?spec.zh:""),Font=GameTheme.Body(16),ForeColor=Color.White,BackColor=Color.FromArgb(190,5,22,30),Height=110,Dock=DockStyle.Bottom};canvas.Controls.Add(captions);
   var skip=new VNButton{Text="跳过 CG",PixelStyle=true,Size=new Size(125,46),Font=GameTheme.Body(12)};canvas.Controls.Add(skip);Action place=()=>skip.Location=new Point(Math.Max(8,canvas.Width-145),20);canvas.Resize+=(s,e)=>place();place();skip.Click+=(s,e)=>finish();canvas.Start();
  }catch(Exception ex){GameMessage.Show(this,"章节 CG 播放失败："+ex.Message);finish();}
 }
 void CompleteChapterOne(){
  if(Attempt().finished==0)FinishSectionTiming();int correct=current.questions.Count(q=>save.quizAnswers.ContainsKey(InlineKey(q))&&save.quizAnswers[InlineKey(q)]==q.answer);int stars=SectionRules.Stars(correct,current.questions.Count,SectionSeconds()<=SectionLimit());save.sectionStars[current.id]=Math.Max(stars,save.sectionStars.ContainsKey(current.id)?save.sectionStars[current.id]:0);bool fresh=!save.completed.Contains(current.id);if(fresh){save.completed.Add(current.id);save.xp+=60;}StoryRoutes.Flag(save,"waystation-chapter-one-section-one-complete");Persist();ClearPage();page="waystation-section-ending";
  content.Controls.Add(new PrologueCompletion{Dock=DockStyle.Fill,BackgroundArt=CachedImage(Path.Combine(root,"chapters","art","tavern","chapter-one-road.png")),Chapter=current,Save=save,Correct=correct,Stars=stars,FirstCompletion=fresh,CompletionTitle="第一节完成 · 老板的第一天",CompletionText="采购清单与分工已确定。陆川启程前往边境小镇。\n第二节“集市：不愿透露姓名的少女”后续开放。",ReturnHome=ShowMain,Chapters=ShowWaystationWorldMap,Replay=()=>{Attempt().review=true;index=0;ShowStory();ReviewSection();}});
 }
}
public class ChapterOneCg {public string zh{get;set;}public string en{get;set;}public double seconds{get;set;}}

