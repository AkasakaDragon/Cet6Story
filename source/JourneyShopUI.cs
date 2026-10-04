using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

public partial class Game {
 void RenderJourneyShop(RogueRun r){
  ClearPage();page="rogue";rogueBody=null;rogueArena=null;
  var surface=new CardRewardSurface{Dock=DockStyle.Fill,Art=TowerBackground(r)};content.Controls.Add(surface);
  var title=Lab("旅途商店",22,Gold);title.BackColor=Color.Transparent;title.TextAlign=ContentAlignment.MiddleCenter;surface.Controls.Add(title);
  var summary=Lab("金币 "+save.rogue.coins+"  ·  生命 "+r.hp+" / "+r.maxHp+"\n每种商品本次限购一次 · 赋能仅本局生效",11,Muted);summary.BackColor=Color.Transparent;summary.TextAlign=ContentAlignment.MiddleCenter;surface.Controls.Add(summary);
  var choices=new List<RelicRewardChoice>();
  foreach(string id in new[]{"potion","shield","blade","hint"}){
   string selected=id;int price=TowerEngine.SupplyPrice(id);bool bought=r.shopBought.Contains(id),full=id=="potion"&&r.hp>=r.maxHp,poor=save.rogue.coins<price;
   string name=id=="potion"?"生命补给":id=="hint"?RogueEngine.RelicName("oracle"):RogueEngine.RelicName(id);
   string effect=id=="potion"?"恢复 30 生命":id=="hint"?"提示次数 +1":RogueEngine.RelicDescription(id);
   string state=bought?"已购买":full?"生命已满":poor?"金币不足":"点击购买";
   var choice=new RelicRewardChoice{RelicId=id,Title=name,Description=effect,FooterText=price+" 金币 · "+state,Art=RelicArt(id=="potion"?"heart":id=="hint"?"oracle":id),Enabled=!bought&&!full&&!poor,AccessibleName=name+" · "+effect+" · "+price+" 金币 · "+state};
   choice.Click+=(s,e)=>{if(TowerEngine.Buy(save.rogue,selected)){PlayShopPurchase(true);SaveRogue();}};surface.Controls.Add(choice);choices.Add(choice);
  }
  var back=new JourneyBackButton();back.Click+=(s,e)=>{if(r.state!="shop")return;TowerEngine.Complete(save.rogue);SaveRogue();};surface.Controls.Add(back);tips.SetToolTip(back,"离开商店 · 返回地图");
  Action layout=()=>{
   int gap=18,width=Math.Min(250,Math.Max(110,(surface.Width-64-gap*3)/4));
   int height=Math.Min(360,Math.Max(220,surface.Height-220)),available=Math.Max(1,surface.Height-74);
   int textWidth=Math.Max(100,surface.Width-48);title.MaximumSize=summary.MaximumSize=new Size(textWidth,0);title.Size=title.GetPreferredSize(new Size(textWidth,0));summary.Size=summary.GetPreferredSize(new Size(textWidth,0));
   int block=title.Height+summary.Height+height+32,top=Math.Max(12,(available-block)/2);title.Location=new Point((surface.Width-title.Width)/2,top);summary.Location=new Point((surface.Width-summary.Width)/2,title.Bottom+8);
   int total=width*4+gap*3,y=summary.Bottom+24;for(int i=0;i<choices.Count;i++)choices[i].Bounds=new Rectangle((surface.Width-total)/2+i*(width+gap),y,width,height);
   back.Bounds=new Rectangle(18,Math.Max(8,surface.Height-70),100,54);
  };surface.Resize+=(s,e)=>layout();layout();
 }
}

public sealed class JourneyBackButton:Control {
 bool hover;
 public JourneyBackButton(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);BackColor=Color.Transparent;Cursor=Cursors.Hand;TabStop=true;AccessibleName="离开商店，返回地图";AccessibleRole=AccessibleRole.PushButton;MouseEnter+=(s,e)=>{hover=true;Invalidate();};MouseLeave+=(s,e)=>{hover=false;Invalidate();};}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){e.Handled=true;OnClick(EventArgs.Empty);}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.ScaleTransform(Width/100f,Height/54f);
  PointF[] banner={new PointF(2,5),new PointF(23,5),new PointF(28,2),new PointF(94,4),new PointF(83,27),new PointF(94,49),new PointF(27,51),new PointF(23,48),new PointF(2,49)};
  using(var shadow=new SolidBrush(Color.FromArgb(110,0,0,0))){g.TranslateTransform(2,3);g.FillPolygon(shadow,banner);g.TranslateTransform(-2,-3);}
  using(var fill=new LinearGradientBrush(new Rectangle(0,0,100,54),hover||Focused?Color.FromArgb(193,66,34):Color.FromArgb(155,47,23),Color.FromArgb(87,22,17),90))g.FillPolygon(fill,banner);
  using(var edge=new Pen(hover||Focused?Color.FromArgb(255,197,101):Color.FromArgb(184,98,46),2))g.DrawPolygon(edge,banner);
  using(var arrow=new GraphicsPath()){arrow.AddPolygon(new[]{new PointF(23,25),new PointF(43,10),new PointF(43,20),new PointF(50,20)});arrow.AddBezier(43,20,62,20,72,30,72,42);arrow.AddLine(72,42,61,42);arrow.AddBezier(61,42,60,31,53,30,43,30);arrow.AddLine(43,30,43,39);arrow.AddLine(43,39,23,25);using(var outline=new Pen(Color.FromArgb(54,26,16),4))g.DrawPath(outline,arrow);using(var ink=new SolidBrush(Color.FromArgb(255,237,192)))g.FillPath(ink,arrow);}
 }
}
