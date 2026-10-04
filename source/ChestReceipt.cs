using System;
using System.Drawing;
using System.Windows.Forms;

public sealed class ChestReceipt:PixelFrame {
 public ChestReceipt(int coins,int balance,bool rare){
  Text=rare?"稀有宝箱 · 奖励已领取":"宝箱 · 奖励已领取";Size=new Size(560,410);MinimumSize=new Size(420,380);
  var body=new Panel{Dock=DockStyle.Fill,BackColor=GameTheme.Navy};Controls.Add(body);
  var reward=new RetroLabel{Text="金币  +"+coins,ForeColor=CyberChrome.Amber,Font=GameTheme.Body(29,FontStyle.Bold),TextAlign=ContentAlignment.MiddleCenter,AutoSize=false};body.Controls.Add(reward);
  var balanceLabel=new RetroLabel{Text="现有金币  "+balance,ForeColor=GameTheme.Muted,Font=GameTheme.Body(12),TextAlign=ContentAlignment.MiddleCenter};body.Controls.Add(balanceLabel);
  var bonus=new RetroLabel{Text=rare?"另获得：赋能选择机会 × 1\n接下来可从三项赋能中选择一项。":"金币已加入余额，可用于商店购买。",ForeColor=GameTheme.Ink,Font=GameTheme.Body(12),TextAlign=ContentAlignment.MiddleCenter};body.Controls.Add(bonus);
  var confirm=new GameButton{Text=rare?"选择赋能":"继续探索",Primary=true,Size=new Size(240,50)};confirm.Click+=(s,e)=>{DialogResult=DialogResult.OK;Close();};body.Controls.Add(confirm);AcceptButton=confirm;
  Action layout=()=>{int width=Math.Max(100,body.ClientSize.Width-32);reward.Bounds=new Rectangle(16,28,width,62);balanceLabel.Bounds=new Rectangle(16,98,width,28);bonus.Bounds=new Rectangle(16,145,width,62);confirm.Location=new Point((body.Width-confirm.Width)/2,Math.Max(220,body.Height-70));};body.Resize+=(s,e)=>layout();layout();
  body.Paint+=(s,e)=>{using(var pen=new Pen(Color.FromArgb(80,CyberChrome.Neon)))e.Graphics.DrawLine(pen,40,135,Math.Max(41,body.Width-40),135);};
 }
}
public partial class Game {
 void OpenTowerChest(){var profile=save.rogue;var run=profile.ActiveRun;if(run==null||run.state!="chest")return;bool rare=run.chestKind==2;TowerEngine.Chest(profile);Persist();using(var receipt=new ChestReceipt(run.battleCoins,profile.coins,rare))receipt.ShowDialog(this);RenderRogue();}
}
