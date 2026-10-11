using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public partial class Game {
 bool developerMode;

 void AddDeveloperSettings(FlowLayoutPanel settings){
  var toggle=new CheckBox{Text="开发者模式（本次运行有效）",Checked=developerMode,AutoSize=true,ForeColor=Gold,Margin=new Padding(8,14,8,5)};
  var open=Btn("打开关卡调试",ShowDeveloperPanel);open.Visible=developerMode;
  toggle.CheckedChanged+=(s,e)=>{developerMode=toggle.Checked;open.Visible=developerMode;};
  settings.Controls.Add(toggle);settings.Controls.Add(open);
 }

 sealed class DebugDestination {
  public string Label,Kind;public int Row,Event;
  public DebugDestination(string label,string kind,int row=0,int eventIndex=-1){Label=label;Kind=kind;Row=row;Event=eventIndex;}
  public override string ToString(){return Label;}
 }

 void ShowDeveloperPanel(){
  if(!developerMode){ShowSettings();return;}
  ClearPage();page="developer";
  var shell=new Panel{Dock=DockStyle.Fill,BackColor=Color.FromArgb(14,22,36)};content.Controls.Add(shell);
  var header=new Panel{Dock=DockStyle.Top,Height=57,BackColor=Color.FromArgb(22,32,49)};shell.Controls.Add(header);
  var title=new Label{Text="开发者调试",Font=GameTheme.Body(21,FontStyle.Bold),ForeColor=Gold,TextAlign=ContentAlignment.MiddleLeft,Location=new Point(24,8),Size=new Size(250,42)};header.Controls.Add(title);
  var back=Btn("返回设置",ShowSettings);back.AutoSize=false;back.Size=new Size(135,40);header.Controls.Add(back);header.Resize+=(s,e)=>back.Location=new Point(header.Width-back.Width-16,8);back.Location=new Point(Math.Max(0,header.Width-back.Width-16),8);
  var columns=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(16)};columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));columns.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));shell.Controls.Add(columns);columns.BringToFront();
  var story=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=false,Padding=new Padding(12),BackColor=Color.FromArgb(23,34,52)};
  var rogue=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=false,Padding=new Padding(12),BackColor=Color.FromArgb(23,34,52)};
  columns.Controls.Add(story,0,0);columns.Controls.Add(rogue,1,0);
  story.Controls.Add(Lab("主线剧情",18,Gold));
  var chapterSelect=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Height=32,Font=GameTheme.Body(11),BackColor=GameTheme.Navy,ForeColor=TextColor};
  foreach(var chapter in chapters)chapterSelect.Items.Add(chapter.id+" · "+chapter.title);
  if(chapterSelect.Items.Count>0)chapterSelect.SelectedIndex=Math.Max(0,chapters.IndexOf(current));story.Controls.Add(chapterSelect);
  var lineLabel=Lab("跳到句子（从 1 开始）",10,Muted);story.Controls.Add(lineLabel);
  var line=new NumericUpDown{Minimum=1,Maximum=1,Value=1,Font=GameTheme.Body(11),BackColor=GameTheme.Navy,ForeColor=TextColor};story.Controls.Add(line);
  chapterSelect.SelectedIndexChanged+=(s,e)=>{if(chapterSelect.SelectedIndex<0)return;line.Maximum=Math.Max(1,chapters[chapterSelect.SelectedIndex].lines.Count);line.Value=1;};
  if(chapterSelect.SelectedIndex>=0)line.Maximum=Math.Max(1,chapters[chapterSelect.SelectedIndex].lines.Count);
  var enter=Btn("直接进入所选剧情",()=>{if(chapterSelect.SelectedIndex<0)return;current=chapters[chapterSelect.SelectedIndex];index=(int)line.Value-1;save.lastChapter=current.id;save.hasGame=true;Persist();ShowStory();});story.Controls.Add(enter);
  var star=Btn("所选关卡标记三星",()=>{if(chapterSelect.SelectedIndex<0)return;var chapter=chapters[chapterSelect.SelectedIndex];save.sectionStars[chapter.id]=3;if(!save.completed.Contains(chapter.id))save.completed.Add(chapter.id);Persist();ShowDeveloperPanel();});story.Controls.Add(star);
  var unlock=Btn("解锁全部现有剧情",()=>{foreach(var chapter in chapters){save.sectionStars[chapter.id]=3;if(!save.completed.Contains(chapter.id))save.completed.Add(chapter.id);}Persist();ShowDeveloperPanel();});story.Controls.Add(unlock);

  rogue.Controls.Add(Lab("词域远征",18,Gold));
  var mode=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Font=GameTheme.Body(11),BackColor=GameTheme.Navy,ForeColor=TextColor};mode.Items.AddRange(new object[]{"基础训练","四级训练","六级挑战"});mode.SelectedIndex=1;rogue.Controls.Add(mode);
  var target=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Font=GameTheme.Body(11),BackColor=GameTheme.Navy,ForeColor=TextColor};
  for(int row=0;row<TowerEngine.Floors;row++)target.Items.Add(new DebugDestination("第 "+(row+1)+" 层地图","map",row));
  foreach(var kind in new[]{"combat","elite","rest","chest","shop","boss"})target.Items.Add(new DebugDestination(TowerEngine.KindName(kind),kind));
  for(int eventIndex=0;eventIndex<5;eventIndex++)target.Items.Add(new DebugDestination("问号事件 · "+TowerEngine.EventTitle(eventIndex),"event",0,eventIndex));
  target.Items.Add(new DebugDestination("远征完成画面","ended"));target.SelectedIndex=0;rogue.Controls.Add(target);
  var jump=Btn("跳到所选远征节点",()=>{var selected=target.SelectedItem as DebugDestination;if(selected!=null)DebugJumpRogue(mode.Text,selected);});rogue.Controls.Add(jump);
  var newRun=Btn("重新开始调试远征",()=>{save.rogue.preparationActive=false;RogueEngine.NewRun(save.rogue,TrainingPool(mode.Text),mode.Text,Environment.TickCount);Persist();RenderRogue();});rogue.Controls.Add(newRun);
  var note=Lab("调试跳转会写入当前存档。",10,Muted);rogue.Controls.Add(note);
  rogue.Controls.Add(Btn("试玩酒馆大厅",ShowTavernHall));
  Action fit=()=>{int left=Math.Max(160,story.ClientSize.Width-story.Padding.Horizontal-14),right=Math.Max(160,rogue.ClientSize.Width-rogue.Padding.Horizontal-14);foreach(Control control in story.Controls)control.Width=left;foreach(Control control in rogue.Controls)control.Width=right;};columns.Resize+=(s,e)=>fit();story.Resize+=(s,e)=>fit();rogue.Resize+=(s,e)=>fit();fit();
 }

 void DebugJumpRogue(string mode,DebugDestination destination){
  var profile=save.rogue;profile.preparationActive=false;
  var run=profile.run;if(run==null||run.settled||run.mode!=mode)run=RogueEngine.NewRun(profile,TrainingPool(mode),mode,Environment.TickCount);
  if(!TowerEngine.IsTower(run)||run.nodes==null||run.nodes.Count==0)TowerEngine.Initialize(run);
  if(destination.Kind=="ended"){run.depth=TowerEngine.FloorCount(run);RogueEngine.Finish(profile,true);Persist();RenderRogue();return;}
  int row=destination.Kind=="map"?destination.Row:run.nodes.Where(n=>n.kind==destination.Kind).Select(n=>n.row).DefaultIfEmpty(-1).First();
  if(row<0)return;
  for(int previous=0;previous<row;previous++){var visited=run.nodes.First(n=>n.row==previous&&n.lane==1);visited.visited=true;}
  run.depth=row;run.lastNode=row==0?null:run.nodes.First(n=>n.row==row-1&&n.lane==1).id;run.currentNode=null;run.question=null;run.hp=Math.Max(1,run.hp);TowerEngine.Routes(run);
  if(destination.Kind!="map"){
   var node=run.nodes.First(n=>n.row==row&&n.kind==destination.Kind&&TowerEngine.Available(run).Any(a=>a.id==n.id));
   TowerEngine.Choose(run,node.id,profile);
   if(destination.Kind=="event")run.eventKind=destination.Event;
   if(destination.Kind=="chest")run.chestKind=2;
  }
  Persist();RenderRogue();
 }
}
