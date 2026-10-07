using System;using System.Drawing;using System.IO;using System.Windows.Forms;
public partial class Game {
 void ShowTavernHub(){
  ClearPage();page="tavern-hub";rogueArena=null;
  var scene=new CardRewardSurface{Dock=DockStyle.Fill,Art=CachedImage(Path.Combine(root,"assets","waystation","hub-morning.png")),AutoScroll=false,CompositeChildren=true};content.Controls.Add(scene);
  var entries=new[]{
   ShowcaseButton("支线远征",ShowToxicWoodlandEntry,190,74),
   ShowcaseButton("主线任务",ShowTavernMainQuest,190,74),
   ShowcaseButton("前往城镇",ShowWaystationWorldMap,190,74),
   ShowcaseButton("返回主界面",ShowMain,220,70),
   ShowcaseButton("伙伴房间",()=>GameMessage.Show(this,"伙伴房间尚未开放。","伙伴房间"),190,74)
  };
  var locations=new[]{new Rectangle(465,420,190,74),new Rectangle(1010,495,190,74),new Rectangle(716,510,190,74),new Rectangle(42,34,220,70),new Rectangle(1310,190,190,74)};
  foreach(var entry in entries){entry.GuildStyle=true;entry.LibraryStyle=true;AddWoodMenuHover(entry);scene.Controls.Add(entry);}
  tips.SetToolTip(entries[0],"公告板 · 剧毒林地：十节点路线远征");tips.SetToolTip(entries[1],"吧台 · 继续主线与重看序幕");tips.SetToolTip(entries[2],"大门 · 世界地图与主线篇章");tips.SetToolTip(entries[4],"楼上客房 · 伙伴房间（尚未开放）");
  Action layout=()=>{float scale=Math.Max(scene.Width/(float)scene.Art.Width,scene.Height/(float)scene.Art.Height);int ox=(int)Math.Round((scene.Width-scene.Art.Width*scale)/2),oy=(int)Math.Round((scene.Height-scene.Art.Height*scale)/2);for(int i=0;i<entries.Length;i++){var r=locations[i];PlaceShowcase(entries[i],scale,ox,oy,r.X,r.Y,r.Width,r.Height,18);}};
  scene.Resize+=(s,e)=>layout();layout();scene.Focus();
 }
 void ShowTavernMainQuest(){
  using(var dialog=new GuildWordDialog{Text="主线任务",Width=760,Height=Math.Min(740,Screen.FromControl(this).WorkingArea.Height-32)}){
   dialog.Location=new Point(Left+(Width-dialog.Width)/2,Top+(Height-dialog.Height)/2);
   var panel=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(16),BackColor=dialog.BackColor};dialog.Controls.Add(panel);
   panel.Controls.Add(Lab("序幕：第一盏灯",18,GuildChrome.Ivory));
   Action next=null;
   panel.Controls.Add(Btn(save.completed.Contains(TavernStory.Id)?"回顾序幕":"继续序幕",()=>{next=EnterTavern;dialog.Close();},true));
   panel.Controls.Add(Btn("从头重看序幕",()=>{next=()=>{current=chapters.Find(TavernStory.Is);ResetSection();save.tavernBattle=null;save.storyFlags.Remove(TavernStory.BattleFlag);save.storyFlags.Remove(TavernStory.OpeningFlag);save.storyFlags.Remove(TavernStory.TransferFlag);save.storyFlags.Remove(TavernStory.EntranceFlag);ShowStory();};dialog.Close();}));
   panel.Controls.Add(Btn("角色技能 · 配置五个携带技能",()=>ShowHeroSkillBook(true)));
   var first=Btn("第一章第一节 · 老板的第一天",()=>{next=EnterChapterOne;dialog.Close();},true);first.Enabled=save.completed.Contains(TavernStory.Id);panel.Controls.Add(first);
   for(int i=1;i<WaystationChapterOne.Ids.Length;i++){string id=WaystationChapterOne.Ids[i];var chapter=chapters.Find(c=>c.id==id);var entry=Btn("第一章第"+new[]{"一","二","三","四","五","六"}[i]+"节 · "+WaystationChapterOne.Names[i],()=>{next=()=>EnterWaystationSection(id);dialog.Close();},true);entry.Width=650;entry.Enabled=chapter!=null&&SectionRules.Unlocked(chapter,chapters,save);panel.Controls.Add(entry);}panel.Controls.Add(Lab("每节四道分段听力题 · 完成后依次开放下一节",10,GuildChrome.Muted));
   dialog.ShowDialog(this);if(next!=null)NavigateMenu(next,"主线任务",false,true);
  }
 }
}
