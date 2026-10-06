using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public sealed class JourneyEventChoice:Control {
 public string Title,Effect;bool hover;
 public JourneyEventChoice(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);BackColor=Color.Transparent;TabStop=true;Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.PushButton;MouseEnter+=(s,e)=>{hover=true;Invalidate();};MouseLeave+=(s,e)=>{hover=false;Invalidate();};}
 protected override void OnMouseEnter(EventArgs e){if(Enabled)VNButton.PlayControlSound(this,false);base.OnMouseEnter(e);}
 protected override void OnClick(EventArgs e){if(Enabled)VNButton.PlayControlSound(this,true);base.OnClick(e);}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){e.Handled=true;OnClick(EventArgs.Empty);}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;CyberChrome.Button(g,new Rectangle(2,2,Width-5,Height-5),"",Font,false,hover||Focused,Enabled);
  using(var titleFont=GameTheme.Body(Width<400?12:14,FontStyle.Bold))using(var effectFont=GameTheme.Body(Width<400?10:12)){
   int textWidth=Math.Max(1,Width-48);var flags=TextFormatFlags.Left|TextFormatFlags.WordBreak|TextFormatFlags.NoPadding;
   int titleHeight=TextRenderer.MeasureText(g,Title,titleFont,new Size(textWidth,Int32.MaxValue),flags).Height;
   int effectHeight=TextRenderer.MeasureText(g,Effect,effectFont,new Size(textWidth,Int32.MaxValue),flags).Height;
   int top=Math.Max(8,(Height-titleHeight-effectHeight-6)/2);
   GameTheme.DrawText(g,Title,titleFont,new Rectangle(24,top,textWidth,titleHeight+2),Enabled?GameTheme.Gold:GameTheme.Muted,flags);
   GameTheme.DrawText(g,Effect,effectFont,new Rectangle(24,top+titleHeight+6,textWidth,effectHeight+2),Enabled?GameTheme.Ink:GameTheme.Muted,flags);
  }
 }

}

public partial class Game {
 static readonly string[] JourneyEventImages={"altar","medicine","merchant","woodspirit","wordbox"};
 static readonly string[] JourneyEventNarratives={
  "洞窟深处，一座古老石碑仍在低声共鸣。裂缝中的符文透出红光，仿佛等待着新的契约。\n\n你可以献出一部分生命，换取未知的力量；也可以拾起石碑旁散落的金币，继续前行。",
  "驿站角落遗落着一只旅行者的药箱。里面的药剂仍泛着柔和的绿光，绷带也保存完好。\n\n药箱上留着一只收款袋：支付金币可以使用治疗药剂，或只取一些绷带，做一次简单包扎。",
  "一盏孤灯照亮了暮色中的布棚。披着斗篷的商人掀开布帘，露出一枚散发锋芒的符石。\n\n“它能让你的攻击更加有力。”商人伸出手，报出价格。你也可以向他告别，把金币留给下一段旅程。",
  "一阵微弱的求救声从树根间传来。小木灵被困在猎手的绳网中，眼中的光芒正逐渐暗淡。\n\n解开绳网会惊动附近的敌人。你可以迎战救出木灵，也可以留下一份祝福，沿着安全的小径离开。",
  "旧书架之间放着一只刻满古老符文的词匣。匣盖轻轻颤动，一缕蓝色回声从缝隙间溢出。\n\n里面的小怪似乎正等待一场词汇考验。你可以打开词匣迎接挑战，也可以带走一缕回声，获得一次答题提示。"
 };
 void RenderJourneyEvent(RogueRun r){
  ClearPage();page="rogue";rogueBody=null;rogueArena=null;
  int kind=Math.Max(0,Math.Min(4,r.eventKind));
  var surface=new CardRewardSurface{Dock=DockStyle.Fill,PixelArt=true,Art=CachedImage(System.IO.Path.Combine(root,"assets","rogue","events",JourneyEventImages[kind]+".png"))};content.Controls.Add(surface);
  var title=Lab(TowerEngine.EventTitle(kind),22,Gold);title.BackColor=Color.Transparent;surface.Controls.Add(title);
  var story=Lab(JourneyEventNarratives[kind],12,TextColor);story.BackColor=Color.Transparent;surface.Controls.Add(story);
  var balance=Lab("生命 "+r.hp+" / "+r.maxHp+"  ·  金币 "+save.rogue.coins,11,Muted);balance.BackColor=Color.Transparent;surface.Controls.Add(balance);
  string[] yes={"触碰石碑 · 献出生命","使用治疗药剂","购买锋刃符石","解开绳网 · 救出木灵","打开词匣 · 接受考验"};
  string[] effects={"消耗 12 生命，获得一项随机赋能","支付 15 金币，恢复 35 生命","支付 45 金币，本局攻击 +4","进入一场词汇战斗","进入一场词汇战斗"};
  string[] no={"拾起金币","简单包扎","向商人告别","留下祝福","带走回声"};
  string[] safe={"获得 8 金币，继续前行","免费恢复 10 生命，继续前行","保留金币，继续前行","获得 6 金币，继续前行","提示次数 +1，继续前行"};
  bool allowed=kind==0?r.hp>12:kind==1?save.rogue.coins>=15&&r.hp<r.maxHp:kind==2?save.rogue.coins>=45:true;
  string reason=kind==0?"生命不足，需高于 12":kind==1&&r.hp>=r.maxHp?"生命已满":"金币不足";
  var choices=new List<JourneyEventChoice>();
  var risk=new JourneyEventChoice{Title=yes[kind],Effect=effects[kind]+(allowed?"":" · "+reason),Enabled=allowed,AccessibleName=yes[kind]+" · "+effects[kind]};
  var leave=new JourneyEventChoice{Title=no[kind],Effect=safe[kind],AccessibleName=no[kind]+" · "+safe[kind]};
  choices.Add(risk);choices.Add(leave);surface.Controls.Add(risk);surface.Controls.Add(leave);
  Action layout=()=>{
   int left=(int)(surface.Width*.46),width=Math.Max(160,surface.Width-left-36);float fontSize=surface.Width<900?11:13;story.Font=GameTheme.Body(fontSize);title.Font=GameTheme.Body(surface.Width<900?19:24,FontStyle.Bold);
   title.AutoSize=story.AutoSize=false;title.Size=new Size(width,42);story.Size=new Size(width,TextRenderer.MeasureText(story.Text,story.Font,new Size(width,Int32.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height+12);
   int buttonHeight=surface.Width<900?78:88,total=title.Height+story.Height+choices.Count*(buttonHeight+12)+28,top=Math.Max(24,(surface.Height-total)/2);
   title.Location=new Point(left,top);story.Location=new Point(left,title.Bottom+18);int y=story.Bottom+24;foreach(var choice in choices){choice.Bounds=new Rectangle(left,y,width,buttonHeight);y+=buttonHeight+12;}
   balance.Location=new Point(24,Math.Max(10,surface.Height-46));
  };
  Action<bool> choose=accept=>{
   if(save.rogue.ActiveRun!=r||r.state!="event"||!TowerEngine.Event(save.rogue,accept))return;
   if(r.state=="combat"){SaveRogue();return;}
   Persist();story.Text=r.feedback+"\n\n旅途还在继续。";balance.Text="生命 "+r.hp+" / "+r.maxHp+"  ·  金币 "+save.rogue.coins;
   foreach(var choice in choices){surface.Controls.Remove(choice);choice.Dispose();}choices.Clear();
   var next=new JourneyEventChoice{Title="继续前行 →",Effect="返回旅途地图",AccessibleName="返回旅途地图"};next.Click+=(s,e)=>{if(r.state!="loot")return;RenderRogue();};choices.Add(next);surface.Controls.Add(next);layout();next.Focus();
  };
  risk.Click+=(s,e)=>choose(true);leave.Click+=(s,e)=>choose(false);surface.Resize+=(s,e)=>layout();layout();
 }
}
