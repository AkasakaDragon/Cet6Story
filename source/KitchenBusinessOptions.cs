using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public partial class Game {
 void ShowKitchenBusinessOptions(){
  if(TavernTime.State(save).phase!=StoryTime.Night){GameMessage.Show(this,"酒馆只在晚上开放。","酒馆营业");return;}
  ClearPage();page="tavern-business-options";
  var surface=new CardRewardSurface{Dock=DockStyle.Fill,Art=CachedImage(Path.Combine(root,"assets","waystation",PreparationBackground(TavernTime.State(save).weather))),PixelArt=true,CompositeChildren=true};content.Controls.Add(surface);
  var panel=new Panel{Dock=DockStyle.Right,Width=Math.Min(470,Math.Max(290,content.Width/3)),BackColor=Color.FromArgb(25,53,55)};surface.Controls.Add(panel);
  panel.Paint+=(s,e)=>GuildChrome.Draw(e.Graphics,panel.ClientRectangle);
  var title=new OutlinedLabel{Text="酒馆营业 · 今晚的工作",Font=GameTheme.Body(20),ForeColor=GuildChrome.Gold};panel.Controls.Add(title);
  var detail=new OutlinedLabel{Text="厨房：选择四六级词库，通过拼写完成切配和烹饪。\n\n咒语练习：逐句听四六级真题，自由切换中英字幕、查看单词，并完成真题练习。",Font=GameTheme.Body(12),ForeColor=GuildChrome.Ivory};panel.Controls.Add(detail);
  var enter=PartyButton("进入厨房开始工作",ShowKitchenWork,300,58);panel.Controls.Add(enter);
  var practice=PartyButton("咒语练习 · 四六级听力",()=>ShowTavernListeningList(SpellPracticeState().level),300,48);panel.Controls.Add(practice);
  var back=PartyButton("返回营业准备",ShowTavernPreparation,300,48);panel.Controls.Add(back);
  Action layout=()=>{int w=panel.Width-40,h=panel.Height;title.Bounds=new Rectangle(20,28,w,60);detail.Bounds=new Rectangle(20,100,w,Math.Max(85,Math.Min(180,h/3)));int y=Math.Max(215,h/2);enter.Bounds=new Rectangle(20,y,w,54);practice.Bounds=new Rectangle(20,y+66,w,48);back.Bounds=new Rectangle(20,y+126,w,48);};panel.Resize+=(s,e)=>layout();layout();
 }
}
