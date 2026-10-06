using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class StoryCaption {public string zh {get;set;} public string en {get;set;}}
public class StoryFilm {public string image {get;set;} public List<StoryCaption> captions {get;set;}}
public class StoryPresentation {public string chapter {get;set;} public StoryFilm intro {get;set;} public StoryFilm ending {get;set;}}
public static class StoryRoutes {
 public static void Normalize(Save s){if(s.storyFlags==null)s.storyFlags=new List<string>();if(s.storyRoutes==null)s.storyRoutes=new Dictionary<string,string>();}
 public static bool Owns(Save s,string item){return StoryShopEngine.Items.Contains(item)||s.rogue.storyItems.Contains(item)||s.rogue.storyUsed.Contains(item);}
 public static void Flag(Save s,string flag){Normalize(s);if(!s.storyFlags.Contains(flag))s.storyFlags.Add(flag);}
 public static bool Enhanced(string id){return id=="neon-01-01"||id=="neon-01-02"||id=="neon-01-A";}
 public static bool Hidden(string id){return id=="neon-01-A"||id=="neon-01-B";}
 public static string Route(Save s,string chapter){Normalize(s);string route;return s.storyRoutes.TryGetValue(chapter,out route)?route:"";}
 public static bool Choose(Save s,string chapter,string route){Normalize(s);s.rogue.Normalize();if(!s.sectionStars.ContainsKey(chapter)||s.sectionStars[chapter]<2)return false;
 bool allowed=chapter=="neon-01-01"?(route=="direct"||(route=="signal"&&Owns(s,"echo-probe"))):chapter=="neon-01-02"?(route=="shelter"||(route=="spur"&&Owns(s,"reserve-power"))):chapter=="neon-01-A"&&s.storyFlags.Contains("clinic-discovered")&&((route=="rescue"&&Owns(s,"repair-pack"))||(route=="record"&&Owns(s,"memory-unit"))||(route=="both"&&Owns(s,"repair-pack")&&Owns(s,"memory-unit")));if(!allowed)return false;
 s.storyRoutes[chapter]=route;if(chapter=="neon-01-01"&&route=="signal")Flag(s,"clinic-discovered");if(chapter=="neon-01-A"){if(route=="rescue"||route=="both")Flag(s,"doctor-safe");if(route=="record"||route=="both")Flag(s,"doctor-testimony");}if(chapter=="neon-01-02"&&route=="spur")Flag(s,"station-discovered");return true;}

}

public partial class Game {
 Dictionary<string,StoryPresentation> storyPresentations;
 StoryPresentation Presentation(){if(storyPresentations==null){var list=Engine.Json.Deserialize<List<StoryPresentation>>(File.ReadAllText(Path.Combine(root,"assets","story","presentations.json")));storyPresentations=list.ToDictionary(x=>x.chapter);}StoryPresentation p;return storyPresentations.TryGetValue(current.id,out p)?p:null;}
 Image AtlasCell(string path,int columns,int rows,int cell){string key=path+"#"+columns+":"+rows+":"+cell;Image result;if(imageCache.TryGetValue(key,out result))return result;var original=CachedImage(path);int x=cell%columns,y=cell/columns;var rect=new Rectangle(x*original.Width/columns,y*original.Height/rows,(x+1)*original.Width/columns-x*original.Width/columns,(y+1)*original.Height/rows-y*original.Height/rows);var bitmap=new Bitmap(rect.Width,rect.Height);using(var g=Graphics.FromImage(bitmap)){g.InterpolationMode=InterpolationMode.NearestNeighbor;g.DrawImage(original,new Rectangle(Point.Empty,bitmap.Size),rect,GraphicsUnit.Pixel);}imageCache[key]=bitmap;return bitmap;}
 void UpdateStoryVisual(Line line){if(stage==null)return;UpdateWaystationPortraits();bool changed=false;if(!String.IsNullOrEmpty(line.scene)){string path=Engine.SafePath(folders[current.id],line.scene);var background=line.sceneSingle?CachedImage(path):AtlasCell(path,3,2,line.sceneCell);if(stage.Art!=background){stage.Art=background;changed=true;}}if(line.pose.HasValue){var actor=stage.Actors.FirstOrDefault(x=>x.Id=="xingyao");if(actor!=null){string[] poses={"urgent","worried","relieved"};var picture=CachedImage(Path.Combine(root,"chapters","art","routes","xingyao-"+poses[Math.Max(0,Math.Min(2,line.pose.Value))]+".png"));if(actor.Image!=picture){actor.Image=picture;changed=true;}}}if(changed)stage.Snap();}
 void PlayStoryFilm(StoryFilm film,Action finish){ClearPage();page="story-cg";var canvas=new StoryQuadCanvas(CachedImage(Path.Combine(root,"assets","story",film.image)),film.captions){Dock=DockStyle.Fill};content.Controls.Add(canvas);bool done=false;Action end=()=>{if(done)return;done=true;finish();};canvas.Completed=end;var skip=new VNButton{Text="跳过 CG",PixelStyle=true,Size=new Size(112,40),Font=GameTheme.Body(11)};canvas.Controls.Add(skip);skip.Click+=(s,e)=>end();Action layout=()=>skip.Location=new Point(canvas.Width-132,12);canvas.Resize+=(s,e)=>layout();layout();var pause=new VNButton{Text="暂停 / 继续",PixelStyle=true,Size=new Size(130,40),Font=GameTheme.Body(11)};canvas.Controls.Add(pause);pause.Click+=(s,e)=>canvas.TogglePause();Action placePause=()=>pause.Location=new Point(16,12);canvas.Resize+=(s,e)=>placePause();placePause();canvas.Start();}
 void ShowStoryIntroPreview(){
  var presentation=Presentation();
  if(presentation==null||presentation.intro==null){ShowStoryMap();SetStatus("本节暂无前置剧情。");return;}
  string id=current.id;
  PlayStoryFilm(presentation.intro,()=>{
   if(current==null||current.id!=id)return;
   Attempt().introShown=true;Persist();ShowStoryMap();
  });
 }
 bool TryStoryIntro(){if(!StoryRoutes.Enhanced(current.id)||Attempt().introShown)return false;var p=Presentation();if(p==null)return false;string id=current.id;PlayStoryFilm(p.intro,()=>{if(current.id!=id)return;Attempt().introShown=true;Persist();ShowStory();});return true;}
 void CompleteNeon(){if(!StoryRoutes.Enhanced(current.id)){CompleteNeonScore();return;}if(Attempt().finished==0){FinishSectionTiming();Persist();}if(Attempt().endingShown){CompleteNeonScore();return;}int correct=current.questions.Count(q=>save.quizAnswers.ContainsKey(InlineKey(q))&&save.quizAnswers[InlineKey(q)]==q.answer);int stars=SectionRules.Stars(correct,current.questions.Count,SectionSeconds()<=SectionLimit());save.sectionStars[current.id]=Math.Max(save.sectionStars.ContainsKey(current.id)?save.sectionStars[current.id]:0,stars);Persist();
  if(stars>=2&&!SelectStoryRoute())return;var presentation=Presentation();var baseFilm=stars<2?presentation.intro:presentation.ending;var film=new StoryFilm{image=baseFilm.image,captions=baseFilm.captions.Select(x=>new StoryCaption{zh=x.zh,en=x.en}).ToList()};string route=StoryRoutes.Route(save,current.id);
  if(stars<2){film.captions[3]=new StoryCaption{zh="这次行动尚未完成。重试听力，再决定下一步。",en="The mission is unfinished. Retry the listening challenge."};}
  else if(current.id=="neon-01-01")film.captions[3]=route=="signal"?new StoryCaption{zh="探针锁定诊所求救信号。隐藏路线已发现。",en="The probe locates a clinic distress signal. A hidden route opens."}:new StoryCaption{zh="两人抵达地下入口。远处的求救信号仍未定位。",en="They reach the underground entrance. The distant signal remains untraced."};
  else if(current.id=="neon-01-02")film.captions[3]=route=="spur"?new StoryCaption{zh="备用能源记录了零号站台坐标，等待后续调查。",en="Reserve power reveals the coordinates of Station Zero."}:new StoryCaption{zh="列车驶向庇护所。父亲的旧档案成为下一个目标。",en="The train heads toward the shelter. Her father's archive is their next goal."};
  else if(current.id=="neon-01-A"){film.image="clinic-end-"+(route=="both"?"both":route=="rescue"?"rescue":"record")+".png";film.captions[3]=route=="both"?new StoryCaption{zh="医生安全撤离，证词也已备份。你保住了两条线索。",en="The doctor escapes safely, and his testimony is preserved."}:route=="rescue"?new StoryCaption{zh="医生已安全撤离；详细记录仍留在损坏的终端。",en="The doctor is safe, but the detailed records remain in the damaged terminal."}:new StoryCaption{zh="证词已备份，医生留守照顾病人。以后仍可回来支援。",en="The testimony is preserved. The doctor stays to care for his patients."};}
  PlayStoryFilm(film,()=>{Attempt().endingShown=true;Persist();CompleteNeonScore();});
 }
 bool SelectStoryRoute(){string id=current.id;if(StoryRoutes.Route(save,id)!="")return true;using(var f=PixelDialog("系统支援 · 决定下一步",760,510)){var body=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(22),AutoScroll=true};f.Controls.Add(body);body.Controls.Add(Lab("行动选择",22,Gold));bool selected=false;Action<string> choose=route=>{if(!StoryRoutes.Choose(save,id,route))return;Persist();selected=true;f.Close();};
  if(id=="neon-01-01"){body.Controls.Add(Lab("巡逻暂时被甩开了，系统捕获到微弱求救信号。",12));body.Controls.Add(Btn("先前往地下车站",()=>choose("direct")));var investigate=Btn("使用回声探针 · 定位求救信号",()=>choose("signal"),true);investigate.Enabled=StoryRoutes.Owns(save,"echo-probe");body.Controls.Add(investigate);body.Controls.Add(Lab("定位求救信号后解锁无名诊所。",11,Muted));}
  else if(id=="neon-01-02"){body.Controls.Add(Lab("列车已经启动。控制台出现一条关闭的支线记录。",12));body.Controls.Add(Btn("继续前往庇护所",()=>choose("shelter")));var investigate=Btn("使用备用能源 · 记录隐藏站台坐标",()=>choose("spur"),true);investigate.Enabled=StoryRoutes.Owns(save,"reserve-power");body.Controls.Add(investigate);body.Controls.Add(Lab("零号站台为地图上的后续隐藏关，本次先发现坐标。",11,Muted));}
  else{bool repair=StoryRoutes.Owns(save,"repair-pack"),memory=StoryRoutes.Owns(save,"memory-unit");body.Controls.Add(Lab("你可以恢复备用电源帮助撤离，也可以保存医生的证词。",12));var rescue=Btn("修复包 · 恢复电源并护送医生",()=>choose("rescue"));rescue.Enabled=repair;body.Controls.Add(rescue);var record=Btn("记忆存储器 · 备份证词与名单",()=>choose("record"));record.Enabled=memory;body.Controls.Add(record);var both=Btn("同时使用两件道具 · 救援并保留证词",()=>choose("both"),true);both.Enabled=repair&&memory;body.Controls.Add(both);body.Controls.Add(Lab("道具永久拥有，重玩可选择另一种行动。",11,Muted));}
  var result=f.ShowDialog(this);if(!selected){ShowStoryMap();return false;}return true;}}
}

public class StoryQuadCanvas:Control {
 readonly Image sheet;readonly List<StoryCaption> captions;readonly Timer timer=new Timer{Interval=2000};int visible;public Action Completed;
 public StoryQuadCanvas(Image art,List<StoryCaption> text){sheet=art;captions=text;DoubleBuffered=true;BackColor=GameTheme.Navy;ResizeRedraw=true;timer.Tick+=(s,e)=>Advance();MouseClick+=(s,e)=>{if(e.Button==MouseButtons.Left)Advance();};AccessibleName="四宫格剧情：左上、右上、左下、右下";}
 public void Start(){visible=1;timer.Start();Invalidate();} public void TogglePause(){timer.Enabled=!timer.Enabled;}
 void Advance(){if(IsDisposed)return;if(visible>=4){timer.Stop();if(Completed!=null)Completed();return;}visible++;timer.Stop();timer.Start();Invalidate();}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;int width=Math.Min(Width-32,(Height-58)*16/9),height=width*9/16;var area=new Rectangle((Width-width)/2,(Height-height)/2+10,width,height);for(int i=0;i<4;i++){var destination=new Rectangle(area.X+(i%2)*area.Width/2,area.Y+(i/2)*area.Height/2,area.Width/2,area.Height/2);if(i>=visible){using(var b=new SolidBrush(Color.FromArgb(9,14,26)))g.FillRectangle(b,destination);continue;}var src=new Rectangle(i%2*sheet.Width/2,i/2*sheet.Height/2,sheet.Width/2,sheet.Height/2);g.DrawImage(sheet,destination,src,GraphicsUnit.Pixel);int footer=Math.Max(90,destination.Height/3);using(var shade=new SolidBrush(Color.FromArgb(210,8,14,26)))g.FillRectangle(shade,destination.X,destination.Bottom-footer,destination.Width,footer);using(var font=GameTheme.Body(Math.Max(10,Math.Min(13,destination.Width/40f)))){GameTheme.DrawText(g,captions[i].zh,font,new Rectangle(destination.X+14,destination.Bottom-footer+7,destination.Width-28,footer/2),GameTheme.Ink,TextFormatFlags.WordBreak|TextFormatFlags.NoPadding);GameTheme.DrawText(g,captions[i].en,font,new Rectangle(destination.X+14,destination.Bottom-footer+footer/2,destination.Width-28,footer/2-4),GameTheme.Muted,TextFormatFlags.WordBreak|TextFormatFlags.NoPadding);}using(var pen=new Pen(i==visible-1?GameTheme.Gold:GameTheme.Violet,2))g.DrawRectangle(pen,Rectangle.Inflate(destination,-2,-2));}}
 protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
}
