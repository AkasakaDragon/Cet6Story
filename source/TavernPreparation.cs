using System;using System.Drawing;using System.IO;using System.Windows.Forms;
public partial class Game {
 void ConfirmTavernPreparation(){
  if(TavernTime.State(save).phase!=StoryTime.Night){GameMessage.Show(this,"酒馆只在晚上开放。","酒馆营业");return;}
  using(var dialog=new GuildWordDialog{Text="营业准备",Width=Math.Min(620,ClientSize.Width-40),Height=300}){
   dialog.Location=new Point(Left+(Width-dialog.Width)/2,Top+(Height-dialog.Height)/2);
   var text=new OutlinedLabel{Dock=DockStyle.Fill,Text="是否立刻进行营业准备？\n\n营业结束后，时间将跳至第二天早上。",Font=GameTheme.Body(15),ForeColor=GuildChrome.Ivory,Padding=new Padding(22)};dialog.Controls.Add(text);
   var row=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=68,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(14,8,14,8)};dialog.Controls.Add(row);
   var wait=PartyButton("再等等",()=>{dialog.DialogResult=DialogResult.Cancel;dialog.Close();},150,44);var proceed=PartyButton("继续",()=>{dialog.DialogResult=DialogResult.OK;dialog.Close();},150,44);row.Controls.Add(wait);row.Controls.Add(proceed);dialog.KeyPreview=true;dialog.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Escape){dialog.DialogResult=DialogResult.Cancel;dialog.Close();}};
   if(dialog.ShowDialog(this)!=DialogResult.OK)return;
  }

  ShowTavernPreparation();
 }
 public static string PreparationBackground(string weather){return weather=="rain"?"business-preparation-night-rain.png":weather=="cloudy"?"business-preparation-night-cloudy.png":"business-preparation-night-preview.png";}
 void ShowTavernPreparation(){
  var time=TavernTime.State(save);string file=PreparationBackground(time.weather);
  ClearPage();page="tavern-preparation";
  var scene=new CardRewardSurface{Dock=DockStyle.Fill,Art=CachedImage(Path.Combine(root,"assets","waystation",file)),PixelArt=true,AutoScroll=false,CompositeChildren=true};content.Controls.Add(scene);
  var back=ShowcaseButton("返回酒馆",ShowTavernHub,200,64);back.GuildStyle=true;back.LibraryStyle=true;AddWoodMenuHover(back);scene.Controls.Add(back);
  Action layout=()=>{back.Size=new Size(Math.Min(200,Math.Max(130,scene.Width/7)),Math.Min(64,Math.Max(44,scene.Height/12)));back.Location=new Point(24,24);SetPreparationButtonFont(back,scene);};scene.Resize+=(s,e)=>layout();layout();AddPreparationHotspot(scene,"今日菜单",()=>OpenHallManagement(1),.84f,.57f);AddPreparationHotspot(scene,"检查食材",()=>OpenHallManagement(1),.92f,.46f);AddPreparationHotspot(scene,"开始营业",ShowTavernBusiness,.45f,.40f);AddPreparationHotspot(scene,"人员管理",()=>OpenHallManagement(0),.665f,.53f);scene.Focus();
 }
 void SetPreparationButtonFont(Control button,Control scene){var surface=scene as CardRewardSurface;float scale=surface!=null&&surface.Art!=null?Math.Max(scene.Width/(float)surface.Art.Width,scene.Height/(float)surface.Art.Height):1;float size=Math.Max(6,18*scale);if(Math.Abs(button.Font.Size-size)>.1f){var previous=button.Font;button.Font=GameTheme.Body(size,previous.Style);previous.Dispose();}}
 void ShowPreparationStaff(){using(var dialog=new GuildWordDialog{Text="人员管理",Width=Math.Min(620,ClientSize.Width-40),Height=380}){dialog.Location=new Point(Left+(Width-dialog.Width)/2,Top+(Height-dialog.Height)/2);var text=new OutlinedLabel{Dock=DockStyle.Fill,Font=GameTheme.Body(15),ForeColor=GuildChrome.Ivory,Padding=new Padding(24),Text="今晚的工作分工\n\n陆川：吧台经营安排、英语指导\n莉瑟：厨房制作、补柴与厨具整理\n艾莉娅：接待、送餐、备饮与收桌\n\n大家换上方便工作的衣服，准备迎接客人。"};dialog.Controls.Add(text);dialog.ShowDialog(this);}}
 void EndTavernBusinessDay(){TavernTime.NextDay(save,tavernWeatherRandom);var time=TavernTime.State(save);Persist();GameMessage.Show(this,"营业结束，已进入第"+time.day+"天早上。\n本次经营收益已保存。","营业结束");ShowTavernHub();}
}
