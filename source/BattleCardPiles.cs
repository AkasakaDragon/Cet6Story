using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public sealed class BattleCardPile:Control {
 public int Count;public bool Discard;
 public BattleCardPile(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);BackColor=Color.Transparent;Cursor=Cursors.Hand;TabStop=true;AccessibleRole=AccessibleRole.PushButton;}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){OnClick(EventArgs.Empty);e.Handled=true;}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;int layers=Math.Min(20,Math.Max(0,Count)),w=Math.Min(62,Width-16),x=(Width-w)/2,baseline=Height-35,h=76;
  using(var edge=new SolidBrush(Color.FromArgb(67,63,61)))using(var paper=new SolidBrush(Color.FromArgb(202,190,161)))using(var back=new SolidBrush(Discard?Color.FromArgb(87,65,87):Color.FromArgb(48,75,86)))using(var motif=new SolidBrush(Discard?Color.FromArgb(150,116,137):Color.FromArgb(130,164,160))){
   if(layers==0){using(var pen=new Pen(GameTheme.Muted)){pen.DashStyle=System.Drawing.Drawing2D.DashStyle.Dot;g.DrawRectangle(pen,x,baseline-32,w,30);}}
   for(int i=0;i<layers;i++){int y=baseline-h-i*2;g.FillRectangle(edge,x,y,w,h);g.FillRectangle(paper,x+2,y+2,w-4,h-4);if(i==layers-1){g.FillRectangle(back,x+5,y+5,w-10,h-10);for(int row=0;row<5;row++)for(int col=0;col<3;col++)g.FillRectangle(motif,x+12+col*13,y+13+row*11,3,3);g.FillRectangle(paper,x+w/2-3,y+h/2-7,6,14);}}
  }
  GameTheme.DrawText(g,(Discard?"弃牌堆":"抽牌堆")+" "+Count,Font,new Rectangle(0,Height-28,Width,26),GameTheme.Ink,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);AccessibleName=(Discard?"弃牌堆":"抽牌堆")+Count+" 张，点击查看";
 }
}
public partial class Game {
 void AddBattlePiles(RogueRun r,Control arena){var b=r.cardBattle;if(b==null)return;var draw=new BattleCardPile{Count=b.draw.Count,Font=GameTheme.Body(10)};var discard=new BattleCardPile{Count=b.discard.Count,Discard=true,Font=GameTheme.Body(10)};arena.Controls.Add(draw);arena.Controls.Add(discard);draw.Click+=(s,e)=>ShowBattlePiles(r,false);discard.Click+=(s,e)=>ShowBattlePiles(r,true);tips.SetToolTip(draw,"查看抽牌堆与本局牌组");tips.SetToolTip(discard,"查看弃牌堆与消耗区");Action layout=()=>{int side=Math.Min(150,Math.Max(100,arena.Width/6)),w=Math.Min(108,side-12),h=arena.Height<560?130:150,y=Math.Max(115,arena.Height-(arena.Height<560?270:310));int badge=arena.Height<560?76:100;int drawHeight=arena.Height<560?116:130;draw.Bounds=new Rectangle(24+badge+10,Math.Max(10,arena.Height-45-drawHeight),w,drawHeight);discard.Bounds=new Rectangle(arena.Width-side+(side-w)/2,y,w,h);draw.BringToFront();discard.BringToFront();};EventHandler resize=(s,e)=>layout();arena.Resize+=resize;var previous=battleLayoutCleanup;battleLayoutCleanup=()=>{arena.Resize-=resize;if(previous!=null)previous();};layout();} void ShowBattlePiles(RogueRun r,bool discard){var b=r.cardBattle;if(b==null)return;using(var dialog=DialogForm("本局牌堆",920,680)){
  var tabs=new TabControl{Dock=DockStyle.Fill,Font=GameTheme.Body(11)};dialog.Controls.Add(tabs);
  string[] titles={"抽牌堆","弃牌堆","本局牌组","消耗区"};var lists=new[]{b.draw,b.discard,b.deck,b.exhausted};
  for(int i=0;i<lists.Length;i++){var tab=new TabPage(titles[i]+" · "+lists[i].Count+" 张"){BackColor=GameTheme.Navy};tabs.TabPages.Add(tab);var scroll=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,WrapContents=true,BackColor=GameTheme.Navy,Padding=new Padding(12)};tab.Controls.Add(scroll);
   if(lists[i].Count==0)scroll.Controls.Add(Lab("这里暂时没有卡牌。",13,GameTheme.Muted));
   foreach(var group in lists[i].GroupBy(id=>id).OrderBy(g=>CardBattle.Get(g.Key).name)){var panel=new FlowLayoutPanel{Width=172,Height=278,FlowDirection=FlowDirection.TopDown,WrapContents=false,BackColor=Color.Transparent};var card=CardBattle.Get(group.Key);panel.Controls.Add(new SupportCardView{Card=card,Atlas=SupportAtlas(),FrameAtlas=CardFrameAtlas(),Large=true,Size=new Size(156,234),Cursor=Cursors.Default});panel.Controls.Add(Lab("数量 × "+group.Count(),11,GameTheme.Ink));scroll.Controls.Add(panel);}
  }
  var info=new RetroLabel{Dock=DockStyle.Bottom,Height=64,Padding=new Padding(12,6,12,6),Font=GameTheme.Body(10),ForeColor=GameTheme.Muted,Text="抽牌堆按卡名归类，不显示抽牌顺序。抽空后弃牌堆会洗回抽牌堆。\n易伤：直接伤害 +25%；燃烧：敌方行动后结算，再减少 2 层；攻击连击每回合重置。"};dialog.Controls.Add(info);tabs.BringToFront();tabs.SelectedIndex=discard?1:0;dialog.ShowDialog(this);
 }}
}
