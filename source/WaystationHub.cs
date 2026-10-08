using System;using System.Drawing;using System.IO;using System.Windows.Forms;
public partial class Game {
 void ShowTavernHub(){
  ClearPage();page="tavern-hub";rogueArena=null;
  var time=TavernTime.State(save);var backdrop=new Bitmap(CachedImage(Path.Combine(root,"assets","waystation",TavernTime.BackgroundFile(time))));
  var scene=new CardRewardSurface{Dock=DockStyle.Fill,Art=backdrop,PixelArt=true,AutoScroll=false,CompositeChildren=true};scene.Disposed+=(s,e)=>backdrop.Dispose();content.Controls.Add(scene);
  var clock=new TavernClock{Art=CachedImage(Path.Combine(root,"assets","ui","tavern-time-clock-dial-transparent.png")),Time=time,AccessibleDescription="第"+time.day+"天 · "+StoryTime.Caption(time.phase)+" · "+TavernTime.Weather(time.weather)};clock.Click+=(s,e)=>SkipTavernTime(clock);scene.Controls.Add(clock);tips.SetToolTip(clock,"点击跳过当前时段 · 早上 → 黄昏 → 晚上 → 次日早上");
  var entries=new[]{
   ShowcaseButton("支线远征",ShowToxicWoodlandEntry,190,74),
   ShowcaseButton("酒馆经营",ConfirmTavernPreparation,190,74),
   ShowcaseButton("前往城镇",ShowWaystationWorldMap,190,74),
   ShowcaseButton("返回主界面",ShowMain,220,70),
   ShowcaseButton("伙伴房间",()=>GameMessage.Show(this,"伙伴房间尚未开放。","伙伴房间"),190,74)
  };
  var locations=new[]{new Rectangle(465,420,190,74),new Rectangle(1010,495,190,74),new Rectangle(716,510,190,74),new Rectangle(42,34,220,70),new Rectangle(1310,190,190,74)};
  foreach(var entry in entries){entry.GuildStyle=true;entry.LibraryStyle=true;AddWoodMenuHover(entry);scene.Controls.Add(entry);}
  tips.SetToolTip(entries[0],"公告板 · 剧毒林地：十节点路线远征");tips.SetToolTip(entries[1],"吧台 · 进入营业准备");tips.SetToolTip(entries[2],"大门 · 世界地图与主线篇章");tips.SetToolTip(entries[4],"楼上客房 · 伙伴房间（尚未开放）");
  Action layout=()=>{float scale=Math.Max(scene.Width/(float)scene.Art.Width,scene.Height/(float)scene.Art.Height);int ox=(int)Math.Round((scene.Width-scene.Art.Width*scale)/2),oy=(int)Math.Round((scene.Height-scene.Art.Height*scale)/2);for(int i=0;i<entries.Length;i++){var r=locations[i];PlaceShowcase(entries[i],scale,ox,oy,(int)(r.X*scene.Art.Width/1672f),(int)(r.Y*scene.Art.Height/941f),(int)(r.Width*scene.Art.Width/1672f),(int)(r.Height*scene.Art.Height/941f),18);}};
  Action clockLayout=()=>{int side=Math.Max(100,Math.Min(190,Math.Min(scene.Width/5,scene.Height/4)));clock.Size=new Size(side+40,side+42);clock.Location=new Point(scene.Width-clock.Width-18,scene.Height-clock.Height-14);clock.BringToFront();};
  scene.Resize+=(s,e)=>{layout();clockLayout();};layout();clockLayout();scene.Focus();
 }
 void ShowTavernMainQuest(){
  using(var dialog=new GuildWordDialog{QuestStyle=true,ShowHeader=false,BackColor=Color.FromArgb(6,29,29),Padding=new Padding(32,40,32,32),Text="第一章 · 选择节",Width=760,Height=Math.Min(760,Screen.FromControl(this).WorkingArea.Height-32)}){
   dialog.Location=new Point(Left+(Width-dialog.Width)/2,Top+(Height-dialog.Height)/2);
   var body=new Panel{Dock=DockStyle.Fill,BackColor=dialog.BackColor};dialog.Controls.Add(body);Action next=null;var cards=new System.Collections.Generic.List<MainQuestCard>();
   var heading=new OutlinedLabel{Text="第一章 · 今天开始营业",Font=GameTheme.Body(25,FontStyle.Bold),ForeColor=GameTheme.Gold,BackColor=Color.Transparent};body.Controls.Add(heading);
   var back=PartyButton("返回世界地图",()=>dialog.Close(),180,40);body.Controls.Add(back);
   bool marked=false;
   for(int i=0;i<WaystationChapterOne.Ids.Length;i++){
    string id=WaystationChapterOne.Ids[i];var chapter=chapters.Find(c=>c.id==id);bool unlocked=chapter!=null&&SectionRules.Unlocked(chapter,chapters,save);bool done=save.completed.Contains(id);bool active=unlocked&&!done&&!marked;if(active)marked=true;
    var card=new MainQuestCard{Heading="第"+new[]{"一","二","三","四","五","六"}[i]+"节 · "+WaystationChapterOne.Names[i],Detail=done?"已完成 · 点击重温":unlocked?"已解锁 · 四道分段听力题":"完成前一节后解锁",Complete=done,Current=active,Locked=!unlocked,Enabled=unlocked};card.AccessibleName=card.Heading+" · "+card.Detail;card.Click+=(s,e)=>{next=()=>EnterWaystationSection(id);dialog.Close();};cards.Add(card);body.Controls.Add(card);
   }
   Action layout=()=>{int width=body.ClientSize.Width;heading.Bounds=new Rectangle(24,6,width-48,48);int footer=body.Height-44,gap=10,available=Math.Max(1,footer-86);int height=Math.Min(84,Math.Max(32,(available-gap*(cards.Count-1))/cards.Count));int used=height*cards.Count+gap*(cards.Count-1);int top=76+Math.Max(0,(available-used)/2);for(int i=0;i<cards.Count;i++)cards[i].Bounds=new Rectangle(18,top+i*(height+gap),width-36,height);back.Bounds=new Rectangle(18,footer,180,36);};body.Resize+=(s,e)=>layout();layout();dialog.ShowDialog(this);if(next!=null)NavigateMenu(next,"主线任务",false,true);
  }
 }
}
