using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public sealed class CampfireChoice:Control {
 public Image Atlas;public bool Heal;public string Description;bool hover;
 public CampfireChoice(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);BackColor=Color.Transparent;TabStop=true;Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.PushButton;MouseEnter+=(s,e)=>{hover=true;Invalidate();};MouseLeave+=(s,e)=>{hover=false;Invalidate();};}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){e.Handled=true;OnClick(EventArgs.Empty);}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
  using(var titleFont=GameTheme.Body(Width<200?16:20,FontStyle.Bold))using(var effectFont=GameTheme.Body(Width<200?10:11)){
   var titleFlags=TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.SingleLine|TextFormatFlags.NoPadding;
   var effectFlags=TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak|TextFormatFlags.NoPadding;
   string title=Heal?"休息":"磨练武器";
   int titleHeight=TextRenderer.MeasureText(g,title,titleFont,new Size(Width,Int32.MaxValue),titleFlags).Height+6;
   int effectHeight=TextRenderer.MeasureText(g,Description,effectFont,new Size(Math.Max(1,Width-8),Int32.MaxValue),effectFlags).Height+6;
   int h=Math.Max(50,Height-titleHeight-effectHeight-24);var plate=new Rectangle(4,4,Width-9,h-5);
   CyberChrome.Panel(g,plate,Enabled?(hover||Focused?CyberChrome.Amber:Heal?Color.FromArgb(132,188,107):Color.FromArgb(213,137,82)):GameTheme.Muted,false,Enabled&&(hover||Focused));
   if(Atlas!=null){int cell=Atlas.Width/2,side=Math.Min(Width-35,h-15);var rect=new Rectangle((Width-side)/2,8,side,side);g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.DrawImage(Atlas,rect,new Rectangle(Heal?0:cell,0,cell,Atlas.Height),GraphicsUnit.Pixel);if(!Enabled)using(var b=new SolidBrush(Color.FromArgb(140,12,20,25)))g.FillRectangle(b,plate);}
   TextRenderer.DrawText(g,title,titleFont,new Rectangle(0,h+10,Width,titleHeight),Enabled?GameTheme.Gold:GameTheme.Muted,titleFlags);
   TextRenderer.DrawText(g,Description,effectFont,new Rectangle(4,h+14+titleHeight,Width-8,effectHeight),Enabled?GameTheme.Ink:GameTheme.Muted,effectFlags);
  }
 }

}

public partial class Game {
 void RenderCampfire(RogueRun r){
  ClearPage();page="rogue";rogueBody=null;rogueArena=null;
  var surface=new CardRewardSurface{Dock=DockStyle.Fill,PixelArt=true,Art=CachedImage(System.IO.Path.Combine(root,"assets","rogue","camp","campfire.png"))};content.Controls.Add(surface);
  var title=Lab("我该做什么呢？",24,TextColor);title.BackColor=Color.Transparent;title.TextAlign=ContentAlignment.MiddleCenter;surface.Controls.Add(title);
  var balance=Lab("生命 "+r.hp+" / "+r.maxHp+"  ·  攻击 "+r.attack,11,Muted);balance.BackColor=Color.Transparent;surface.Controls.Add(balance);
  var atlas=CachedImage(System.IO.Path.Combine(root,"assets","rogue","camp","choices.png"));
  int healing=(int)Math.Ceiling(r.maxHp*.3);
  var rest=new CampfireChoice{Atlas=atlas,Heal=true,Description=r.hp<r.maxHp?"恢复 "+Math.Min(healing,r.maxHp-r.hp)+" 生命":"生命已满",Enabled=r.hp<r.maxHp,AccessibleName="休息，恢复 "+healing+" 生命"};
  var train=new CampfireChoice{Atlas=atlas,Heal=false,Description="本局攻击 +3",AccessibleName="磨练武器，本局攻击增加 3"};surface.Controls.Add(rest);surface.Controls.Add(train);tips.SetToolTip(rest,"恢复生命上限的 30%（向上取整），不超过生命上限");
  var result=Lab("",17,Gold);result.BackColor=Color.Transparent;result.TextAlign=ContentAlignment.MiddleCenter;result.Visible=false;surface.Controls.Add(result);
  var next=RogueButton("继续探索 →",()=>{if(r.state=="loot")RenderRogue();},230,44);next.Visible=false;surface.Controls.Add(next);
  Action layout=()=>{
   int w=Math.Min(235,Math.Max(155,surface.Width/5)),gap=Math.Min(100,surface.Width/10),h=Math.Min(235,Math.Max(180,(int)(surface.Height*.29)));
   int textWidth=Math.Max(100,surface.Width-48);title.AutoSize=false;title.Bounds=new Rectangle(24,Math.Max(16,surface.Height/22),textWidth,44);int top=title.Bottom+20,total=w*2+gap;
   rest.Bounds=new Rectangle((surface.Width-total)/2,top,w,h);train.Bounds=new Rectangle(rest.Right+gap,top,w,h);
   result.AutoSize=false;result.Bounds=new Rectangle(24,title.Bottom+40,textWidth,90);balance.Location=new Point(24,surface.Height-43);next.Location=new Point(Math.Max(16,surface.Width-next.Width-24),surface.Height-next.Height-20);
  };
  Action<bool> choose=heal=>{if(save.rogue.ActiveRun!=r||r.state!="rest"||(heal&&r.hp>=r.maxHp))return;int previous=r.hp;TowerEngine.Rest(save.rogue,heal);Persist();rest.Visible=train.Visible=false;title.Text="短暂的休整";result.Text=heal?"在火光旁休息片刻……\n恢复 "+(r.hp-previous)+" 生命":"重新磨利武器，准备下一段旅程。\n本局攻击 +3";result.Visible=next.Visible=true;balance.Text="生命 "+r.hp+" / "+r.maxHp+"  ·  攻击 "+r.attack;layout();next.Focus();};
  rest.Click+=(s,e)=>choose(true);train.Click+=(s,e)=>choose(false);surface.Resize+=(s,e)=>layout();layout();
 }
}
