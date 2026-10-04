using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;using System.Collections.Generic;
public sealed class RelicRewardChoice:Control {
 public string RelicId,Title,Description,FooterText;public Image Art;bool hover;
 public RelicRewardChoice(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);BackColor=Color.Transparent;Cursor=Cursors.Hand;TabStop=true;AccessibleRole=AccessibleRole.PushButton;MouseEnter+=(s,e)=>{hover=true;Invalidate();};MouseLeave+=(s,e)=>{hover=false;Invalidate();};}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){e.Handled=true;OnClick(EventArgs.Empty);}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;g.InterpolationMode=InterpolationMode.HighQualityBicubic;
  CyberChrome.Panel(g,new Rectangle(3,3,Width-7,Height-7),!Enabled?Color.FromArgb(90,115,122):hover||Focused?CyberChrome.Amber:CyberChrome.Neon,false,Enabled&&(hover||Focused));
  int diameter=Math.Min(Width-42,(int)(String.IsNullOrEmpty(FooterText)?Height*.48:(Height-130)*.48));var circle=new Rectangle((Width-diameter)/2,26,diameter,diameter);
  using(var glow=new GraphicsPath()){glow.AddEllipse(circle);using(var brush=new PathGradientBrush(glow)){brush.CenterColor=Color.FromArgb(90,132,192,205);brush.SurroundColors=new[]{Color.FromArgb(0,50,99,115)};g.FillPath(brush,glow);}}
  using(var ring=new Pen(Color.FromArgb(70,214,189,134),1))g.DrawEllipse(ring,circle);
  if(Art!=null){float scale=Math.Min((diameter-18f)/Art.Width,(diameter-18f)/Art.Height);float w=Art.Width*scale,h=Art.Height*scale;g.DrawImage(Art,new RectangleF(Width/2f-w/2,circle.Top+diameter/2f-h/2,w,h));}
  int titleY=circle.Bottom+16;using(var f=GameTheme.Body(Width<210?14:17))GameTheme.DrawText(g,Title,f,new Rectangle(12,titleY,Width-24,32),Color.FromArgb(255,223,161),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);
  using(var pen=new Pen(Color.FromArgb(65,212,190,142),1))g.DrawLine(pen,30,titleY+38,Width-30,titleY+38);
  using(var f=GameTheme.Body(Width<210?10:12))GameTheme.DrawText(g,Description,f,new Rectangle(18,titleY+49,Width-36,Math.Max(String.IsNullOrEmpty(FooterText)?30:44,Height-titleY-68-(String.IsNullOrEmpty(FooterText)?0:42))),Color.FromArgb(231,236,226),TextFormatFlags.HorizontalCenter|TextFormatFlags.WordBreak|TextFormatFlags.NoPadding);
  if(!String.IsNullOrEmpty(FooterText)){using(var f=GameTheme.Body(Width<210?11:13,FontStyle.Bold))GameTheme.DrawText(g,FooterText,f,new Rectangle(10,Height-43,Width-20,28),Enabled?GameTheme.Gold:GameTheme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);}
 }
}
public partial class Game {
 void RenderRelicRewards(RogueRun r){
  ClearPage();page="rogue";rogueBody=null;rogueArena=null;
  var surface=new CardRewardSurface{Dock=DockStyle.Fill,Art=TowerBackground(r)};content.Controls.Add(surface);
  var title=Lab("获得赋能 · 三选一",22,Gold);title.BackColor=Color.Transparent;title.TextAlign=ContentAlignment.MiddleCenter;surface.Controls.Add(title);
  string origin=TowerEngine.Node(r)!=null&&TowerEngine.Node(r).kind=="chest"?"稀有宝箱":"精英战斗胜利";
  var description=Lab(origin+" · 已获得 "+r.battleCoins+" 金币\n选择一项赋能，本局有效，重复获得可叠加。",11,Muted);description.BackColor=Color.Transparent;description.TextAlign=ContentAlignment.MiddleCenter;surface.Controls.Add(description);
  var choices=new List<RelicRewardChoice>();foreach(string id in r.rewards){string selected=id;var choice=new RelicRewardChoice{RelicId=id,Title=RogueEngine.RelicName(id),Description=RogueEngine.RelicDescription(id),Art=RelicArt(id),AccessibleName=RogueEngine.RelicName(id)+" · "+RogueEngine.RelicDescription(id)};choice.Click+=(s,e)=>{if(r.state!="reward")return;RogueEngine.Reward(save.rogue,selected);SaveRogue();};surface.Controls.Add(choice);choices.Add(choice);}
  Action layout=()=>{int count=Math.Max(1,choices.Count),gap=22;int width=Math.Min(270,Math.Max(140,(surface.Width-64-gap*(count-1))/count));int height=Math.Min(350,Math.Max(230,surface.Height-170));int textWidth=Math.Max(100,surface.Width-48);title.MaximumSize=description.MaximumSize=new Size(textWidth,0);title.Size=title.GetPreferredSize(new Size(textWidth,0));description.Size=description.GetPreferredSize(new Size(textWidth,0));int blockHeight=title.Height+description.Height+height+40,top=Math.Max(16,(surface.Height-blockHeight)/2);title.Location=new Point((surface.Width-title.Width)/2,top);description.Location=new Point((surface.Width-description.Width)/2,title.Bottom+10);int rowY=description.Bottom+26,total=choices.Count*width+Math.Max(0,choices.Count-1)*gap;for(int i=0;i<choices.Count;i++)choices[i].Bounds=new Rectangle((surface.Width-total)/2+i*(width+gap),rowY,width,height);};surface.Resize+=(s,e)=>layout();layout();
 }
}
