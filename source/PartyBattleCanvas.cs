using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

public class PartyBattleCanvas:Panel {
 public PartyCombatState Battle;public Image Scene,Male,Female,Enemy;public HeroSkill Skill;public Action<int> HeroSelected;public Action<string> TargetSelected;public int FooterHeight=238;public string Banner="剧情战斗";
 public Action InterfaceCleanup;
 [System.Runtime.InteropServices.DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr handle,int message,IntPtr wparam,IntPtr lparam);
 public void BeginInterfaceUpdate(){SuspendLayout();if(IsHandleCreated)SendMessage(Handle,0x000B,IntPtr.Zero,IntPtr.Zero);}
 public void EndInterfaceUpdate(){ResumeLayout(true);if(IsHandleCreated)SendMessage(Handle,0x000B,new IntPtr(1),IntPtr.Zero);Invalidate(true);}
 public void ClearInterface(){if(InterfaceCleanup!=null){InterfaceCleanup();InterfaceCleanup=null;}while(Controls.Count>0)Controls[0].Dispose();}
 public Image[] MaleIdle,FemaleIdle;readonly Timer idle=new Timer{Interval=31};int idleFrame=-1;
 readonly Timer pulse=new Timer{Interval=35};int flash;string floatText;Rectangle heroPortrait;public Rectangle[] HeroBounds=new Rectangle[0];public Rectangle[] EnemyBounds=new Rectangle[0];
 public PartyBattleCanvas(){idle.Tick+=(s,e)=>{if(!Visible||Battle==null||FindForm()==null||!FindForm().Enabled)return;int frame=HeroIdleAnimation.FrameIndex;if(frame==idleFrame)return;idleFrame=frame;foreach(var bounds in HeroBounds)if(!bounds.IsEmpty)Invalidate(Rectangle.Inflate(bounds,3,3));int floor=Height-FooterHeight-75;Invalidate(new Rectangle(0,floor-(int)(Width*.016f)-40,Width,(int)(Width*.032f)+44));};idle.Start();DoubleBuffered=true;BackColor=Color.FromArgb(12,24,29);pulse.Tick+=(s,e)=>{flash--;if(flash<=0)pulse.Stop();Invalidate();};}
 void DrawPositionGrid(Graphics g,int floor){
  float width=Width*.092f,depth=Width*.032f,skew=Width*.012f;
  for(int side=0;side<2;side++)for(int slot=0;slot<4;slot++){
   float cx=Width*((side==0?.075f:.595f)+slot*.11f);
   Color color=Color.FromArgb(90,150,155);var unit=(side==0?Battle.heroes:Battle.enemies).FirstOrDefault(u=>PositionSlot(u,side==1)==slot);if(unit!=null&&unit.hp>0){color=side==0?Color.FromArgb(93,224,222):Color.FromArgb(167,105,174);if(side==0&&Battle.heroes.IndexOf(unit)==Battle.selectedHero)color=Color.FromArgb(249,204,112);if(side==1&&Battle.target==unit.id)color=Color.FromArgb(213,107,210);}
   var points=new[]{new PointF(cx-width/2+skew,floor-depth/2),new PointF(cx+width/2+skew,floor-depth/2),new PointF(cx+width/2-skew,floor+depth/2),new PointF(cx-width/2-skew,floor+depth/2)};
   using(var fill=new SolidBrush(Color.FromArgb(35,color)))g.FillPolygon(fill,points);
   for(int k=5;k>=1;k--)using(var pen=new Pen(Color.FromArgb(10+k*3,color),k*2))g.DrawPolygon(pen,points);
   using(var pen=new Pen(Color.FromArgb(230,color),2))g.DrawPolygon(pen,points);
   for(int n=0;n<12;n++){float t=(n+.5f)/12;int edge=n%4;PointF a=points[edge],b=points[(edge+1)%4];float x=a.X+(b.X-a.X)*t,y=a.Y+(b.Y-a.Y)*t;int rise=9+(n*13+slot*7+HeroIdleAnimation.FrameIndex/2)%28;for(int k=0;k<rise;k+=3)using(var ink=new SolidBrush(Color.FromArgb((int)(100*(1-k/(float)rise)),color)))g.FillRectangle(ink,x,y-k,2,3);using(var ink=new SolidBrush(Color.FromArgb(180,color)))g.FillRectangle(ink,x,y-rise,3,3);}
  }
 }
 int PositionSlot(PartyUnit unit,bool enemy){var units=(enemy?Battle.enemies:Battle.heroes).OrderBy(u=>enemy?u.rank:-u.rank).ToList();return Math.Max(0,Math.Min(3,units.IndexOf(unit)+(enemy?(units.Count==1?2:0):(units.Count==2?1:0))));}
 public void ShowImpact(string text){floatText=text;flash=24;pulse.Start();Invalidate();}
 protected override void Dispose(bool d){if(d){idle.Dispose();pulse.Dispose();}base.Dispose(d);}
 protected override void OnPaintBackground(PaintEventArgs e){}
 static new void Text(Graphics g,string text,Rectangle r,int size,Color color,bool center=false){using(var f=GameTheme.Body(size))using(var ink=new SolidBrush(color))using(var format=new StringFormat{Alignment=center?StringAlignment.Center:StringAlignment.Near})GameTheme.DrawPixelString(g,text,f,ink,r,format);}
 static Rectangle Sprite(Image image,float cx,float ground,float height){if(image==null)return Rectangle.Empty;float w=height*image.Width/image.Height;return new Rectangle((int)(cx-w/2),(int)(ground-height),(int)w,(int)height);}
 static void Portrait(Graphics g,Image image,Rectangle r,bool monster=false){
  if(!monster)GuildChrome.Draw(g,r);if(image==null)return;var state=g.Save();float h=image.Height*.30f,w=h,x=image.Width*.56f;var crop=new RectangleF(Math.Max(0,Math.Min(image.Width-w,x-w/2)),0,Math.Min(w,image.Width),h);
  if(monster){var points=new[]{new Point(r.Left+r.Width/2,r.Top),new Point(r.Right,r.Top+r.Height/2),new Point(r.Left+r.Width/2,r.Bottom),new Point(r.Left,r.Top+r.Height/2)};using(var path=new GraphicsPath()){path.AddPolygon(points);g.SetClip(path);using(var dark=new SolidBrush(Color.FromArgb(18,35,38)))g.FillPath(dark,path);crop=new RectangleF(0,image.Height*.30f,image.Width*.65f,image.Height*.65f);g.DrawImage(image,r,crop,GraphicsUnit.Pixel);g.Restore(state);using(var pen=new Pen(GuildChrome.Gold,2))g.DrawPolygon(pen,points);}return;}
  g.SetClip(Rectangle.Inflate(r,-7,-7));g.DrawImage(image,Rectangle.Inflate(r,-8,-8),crop,GraphicsUnit.Pixel);g.Restore(state);
 }
 void Bar(Graphics g,PartyUnit u,int cx,int y,int width,bool enemy){
  Text(g,u.name+"  "+u.hp+" / "+u.maxHp,new Rectangle(cx-width/2,y,width,22),11,GuildChrome.Ivory,true);
  var r=new Rectangle(cx-width/2,y+25,width,8);using(var dark=new SolidBrush(Color.FromArgb(16,28,31)))g.FillRectangle(dark,r);using(var b=new SolidBrush(enemy?Color.FromArgb(203,110,87):Color.FromArgb(102,184,147)))g.FillRectangle(b,r.X,r.Y,Math.Max(0,r.Width*u.hp/Math.Max(1,u.maxHp)),r.Height);
  string status=(u.shield>0?"护盾 "+u.shield+"  ":"")+(u.resolve>0?"决意 "+u.resolve+"  ":"")+(u.marks>0?"符印 "+u.marks+"  ":"")+(u.bleed>0?"流血 "+u.bleed+"  ":"")+(u.burn>0?"灼烧 "+u.burn+"  ":"");
  Text(g,status,new Rectangle(cx-width/2,y+36,width,25),10,GameTheme.Gold,true);
 }
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.SmoothingMode=SmoothingMode.None;
  if(Scene!=null){float scale=Math.Max(Width/(float)Scene.Width,(Height-FooterHeight)/(float)Scene.Height);float w=Scene.Width*scale,h=Scene.Height*scale;g.DrawImage(Scene,(Width-w)/2,(Height-FooterHeight-h)/2,w,h);}else g.Clear(BackColor);
  using(var shade=new SolidBrush(Color.FromArgb(40,6,17,20)))g.FillRectangle(shade,0,0,Width,Height-FooterHeight);
  if(Battle==null)return;int floor=Height-FooterHeight-75;int available=Math.Max(80,floor-110);float height=Math.Min(available,Math.Min(340,Width*.25f));
  DrawPositionGrid(g,floor);
  HeroBounds=new Rectangle[Battle.heroes.Count];EnemyBounds=new Rectangle[Battle.enemies.Count];
  var heroes=Battle.heroes.OrderByDescending(h=>h.rank).ToList();
  for(int j=0;j<heroes.Count;j++){var h=heroes[j];int i=Battle.heroes.IndexOf(h);float cx=Width*(.075f+PositionSlot(h,false)*.11f);var image=h.id=="aelia"?Female:Male;var frames=h.id=="aelia"?FemaleIdle:MaleIdle;if(h.hp>0&&frames!=null&&frames.Length>0)image=frames[HeroIdleAnimation.FrameIndex%frames.Length];bool animated=h.hp>0&&frames!=null&&frames.Length>0;var r=animated?Sprite(image,cx,floor+height*50/395f,height*468/395f):Sprite(image,cx,floor,height);HeroBounds[i]=r;
   bool selected=Battle.selectedHero==i;using(var b=new SolidBrush(Color.FromArgb(100,4,12,15)))g.FillEllipse(b,r.Left+r.Width/8,floor-8,r.Width*3/4,15);
   if(selected){Text(g,"▼",new Rectangle((int)cx-14,r.Top-30,30,26),18,GameTheme.Gold,true);}
   if(image!=null){if(h.hp<=0||h.acted){using(var attr=new System.Drawing.Imaging.ImageAttributes()){attr.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(new float[][]{new[]{.45f,0f,0f,0f,0f},new[]{0f,.45f,0f,0f,0f},new[]{0f,0f,.45f,0f,0f},new[]{0f,0f,0f,1f,0f},new[]{0f,0f,0f,0f,1f}}));g.DrawImage(image,r,0,0,image.Width,image.Height,GraphicsUnit.Pixel,attr);}}else g.DrawImage(image,r);}
   Bar(g,h,(int)cx,floor+(int)(Width*.016f)+8,Math.Min(165,(int)(Width*.095f)),false);
  }
  for(int i=0;i<Battle.enemies.Count;i++){var u=Battle.enemies[i];float cx=Width*(.595f+PositionSlot(u,true)*.11f);var r=Sprite(Enemy,cx,floor,height*.82f);EnemyBounds[i]=r;if(u.hp>0){using(var b=new SolidBrush(Color.FromArgb(100,4,12,15)))g.FillEllipse(b,r.Left+r.Width/8,floor-8,r.Width*3/4,15);if(Enemy!=null)g.DrawImage(Enemy,r);if(Battle.target==u.id){Text(g,"▼",new Rectangle((int)cx-14,r.Top-30,30,26),18,GameTheme.Gold,true);}}Bar(g,u,(int)cx,floor+(int)(Width*.016f)+8,Math.Min(215,(int)(Width*.095f)),true);}
  using(var b=new SolidBrush(Color.FromArgb(210,10,24,27)))g.FillRectangle(b,0,0,Width,66);using(var p=new Pen(GuildChrome.Gold))g.DrawLine(p,0,65,Width,65);
  Text(g,Banner,new Rectangle(115,20,Math.Max(100,Width/3-100),26),14,GameTheme.Gold);Text(g,"第 "+Battle.round+" 回合",new Rectangle(Width/2-90,20,180,30),16,GameTheme.Gold,true);
  string intent=Battle.round%3==0?"孢子喷吐 · 群体20伤害 · 灼烧":Battle.round%3==2?"扎根 · 获得12护盾":"撕咬 · 单体20伤害";int intentW=Math.Min(310,Width*35/100);var intentRect=new Rectangle((int)(Width*.79f)-intentW/2,102,intentW,36);GuildChrome.Draw(g,intentRect);Text(g,intent,new Rectangle(intentRect.X+10,intentRect.Y+9,intentW-20,24),Width<1000?8:10,GameTheme.Gold,true);
  var order=PartyCombat.EnemyOrder(Battle);int avatar=46;int start=Width-16-order.Count*(avatar+9);
  for(int i=0;i<order.Count;i++){var r=new Rectangle(start+i*(avatar+9),8,avatar,avatar);Portrait(g,Enemy,r,true);Text(g,(i+1).ToString(),new Rectangle(r.Right-15,r.Bottom-17,17,18),9,GameTheme.Gold,true);}Text(g,"怪物行动顺序",new Rectangle(Math.Max(0,start-125),24,120,22),10,GuildChrome.Muted,true);
  int top=Height-FooterHeight;var footerState=g.Save();float footerScale=Math.Min(1.3f,Width/1280f);g.TranslateTransform(0,top);g.ScaleTransform(footerScale,footerScale);int logicalWidth=(int)(Width/footerScale);GuildChrome.Draw(g,new Rectangle(0,0,logicalWidth,(int)(FooterHeight/footerScale)));
  var hero=Battle.heroes[Math.Max(0,Math.Min(Battle.heroes.Count-1,Battle.selectedHero))];int portraitSize=150;heroPortrait=new Rectangle(20,28,portraitSize,portraitSize);Portrait(g,hero.id=="aelia"?Female:Male,heroPortrait);
  Text(g,hero.name+" · "+(hero.id=="aelia"?"剑盾骑士":"言契术士"),new Rectangle(20,heroPortrait.Bottom+10,portraitSize+25,25),12,GameTheme.Gold);
  int infoX=192,infoWidth=logicalWidth*28/100-25;Text(g,Skill==null?"选择技能":Skill.Name,new Rectangle(infoX,28,infoWidth,30),17,GameTheme.Gold);
  Text(g,Skill==null?"点击人物查看其五个携带技能。":Skill.Description,new Rectangle(infoX,68,infoWidth,100),11,GuildChrome.Ivory);
  Text(g,hero.id=="aelia"?"护盾承伤积累决意，强化自身攻击。":"叠加自己的符印，再选择时机引爆。",new Rectangle(infoX,174,infoWidth,46),10,GuildChrome.Muted);g.Restore(footerState);
  if(flash>0){using(var b=new SolidBrush(Color.FromArgb(Math.Min(120,flash*4),237,193,108)))g.FillRectangle(b,0,66,Width,3);Text(g,floatText,new Rectangle(Width/2-260,90+(24-flash)/2,520,60),15,GameTheme.Gold,true);}
  base.OnPaint(e);
 }
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(Battle==null||e.Button!=MouseButtons.Left)return;for(int i=0;i<HeroBounds.Length;i++)if(HeroBounds[i].Contains(e.Location)){if(Skill!=null&&Skill.Target=="ally"&&TargetSelected!=null)TargetSelected(Battle.heroes[i].id);else if(HeroSelected!=null)HeroSelected(i);return;}for(int i=0;i<EnemyBounds.Length;i++)if(EnemyBounds[i].Contains(e.Location)&&TargetSelected!=null){TargetSelected(Battle.enemies[i].id);return;}}
}

public class HeroSkillButton:VNButton {
 public Image Art;public HeroSkill Skill;public int Slot,Remaining,UsesLeft=-1;
 void DrawSkillArt(Graphics g){
  g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.DrawImage(Art,ClientRectangle);
  if(!Enabled)using(var shade=new SolidBrush(Color.FromArgb(155,9,20,24)))g.FillRectangle(shade,ClientRectangle);
  if(Active){using(var p=new Pen(Color.FromArgb(255,226,146),2))g.DrawRectangle(p,2,2,Math.Max(1,Width-5),Math.Max(1,Height-5));using(var p=new Pen(Color.FromArgb(115,255,210,105),2))g.DrawRectangle(p,0,0,Math.Max(1,Width-1),Math.Max(1,Height-1));}
  string caption=Remaining>0?"冷却"+Remaining:UsesLeft==0?"已用尽":Slot.ToString();int h=Math.Max(15,Height/4);using(var shade=new SolidBrush(Color.FromArgb(210,9,22,26)))g.FillRectangle(shade,3,Height-h-3,Width-6,h);
  using(var f=GameTheme.Body(Math.Max(7,Width/9f)))GameTheme.DrawText(g,caption,f,new Rectangle(3,Height-h-3,Width-6,h),GuildChrome.Ivory,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);
 }
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;if(Art!=null){DrawSkillArt(g);return;}GuildChrome.Draw(g,ClientRectangle,Active,Enabled);var state=g.Save();float scale=Math.Min(Width/78f,Height/82f);g.TranslateTransform(Width/2f,Height/2f-7*scale);g.ScaleTransform(scale,scale);int cx=0,cy=0;Color color=!Enabled?GuildChrome.Muted:Skill.Target=="enemy"?Color.FromArgb(214,180,121):GameTheme.Cyan;
  using(var p=new Pen(color,3))using(var b=new SolidBrush(color)){
   switch(Skill.Icon){case "sword":case "bleed":case "sweep":case "break":g.DrawLine(p,cx-14,cy+14,cx+14,cy-14);g.DrawLine(p,cx-15,cy+5,cx-5,cy+15);if(Skill.Icon=="bleed")g.FillRectangle(b,cx+11,cy+8,5,9);if(Skill.Icon=="sweep")g.DrawArc(p,cx-22,cy-22,44,44,200,130);break;
    case "shield":case "guard":case "counter":g.DrawPolygon(p,new[]{new Point(cx-15,cy-17),new Point(cx+15,cy-17),new Point(cx+13,cy+5),new Point(cx,cy+19),new Point(cx-13,cy+5)});g.DrawLine(p,cx,cy-10,cx,cy+10);if(Skill.Icon=="counter")g.DrawLine(p,cx-8,cy,cx+8,cy);break;
    case "heal":g.FillRectangle(b,cx-5,cy-17,10,34);g.FillRectangle(b,cx-17,cy-5,34,10);break;
    case "move":g.DrawLines(p,new[]{new Point(cx-17,cy+10),new Point(cx+15,cy-10),new Point(cx+1,cy-13)});g.DrawLine(p,cx+15,cy-10,cx+12,cy+5);break;
    case "fire":g.FillPolygon(b,new[]{new Point(cx,cy-21),new Point(cx+5,cy-3),new Point(cx+15,cy-11),new Point(cx+13,cy+14),new Point(cx,cy+20),new Point(cx-14,cy+12),new Point(cx-7,cy-6)});break;
    case "bolt":g.DrawLines(p,new[]{new Point(cx+8,cy-20),new Point(cx-10,cy+1),new Point(cx+7,cy+1),new Point(cx-8,cy+21)});break;
    default:g.DrawPolygon(p,new[]{new Point(cx,cy-20),new Point(cx+16,cy),new Point(cx,cy+20),new Point(cx-16,cy)});g.DrawLine(p,cx-22,cy,cx+22,cy);g.DrawLine(p,cx,cy-25,cx,cy+25);break;
   }
  }
  g.Restore(state);using(var f=GameTheme.Body(Math.Max(6,9*scale)))GameTheme.DrawText(g,Remaining>0?"冷却 "+Remaining:UsesLeft==0?"已用尽":Slot.ToString(),f,new Rectangle(3,Height-23,Width-6,20),GuildChrome.Ivory,TextFormatFlags.HorizontalCenter|TextFormatFlags.NoPadding);
 }
}





