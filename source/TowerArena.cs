using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

public class RogueArena:Control {
 public string BannerTitle="词域远征",BannerSubtitle="答题 · 战斗 · 随机赋能";public bool ShowDrone=true;public Image Art,Hero,EnemyArt,Support;public Image[] PistolFrames,ReactionFrames,Effects;public RogueRun Run;public string Mode="home";public bool Integrated;public int OverlayHeight;public bool AnimateHit;public Action<bool> HitSound;Timer pulse;int frame;Bitmap backdrop;Image backdropArt;
 public RogueArena(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);ResizeRedraw=true;pulse=new Timer{Interval=35};pulse.Tick+=(s,e)=>{bool combatAnimation=AnimateHit&&Mode=="feedback"&&Run!=null;bool rewardAnimation=Mode=="loot"||Mode=="reward";if((!combatAnimation&&!rewardAnimation)||frame>=(combatAnimation?34:28)){pulse.Stop();return;}frame++;if(combatAnimation&&HitSound!=null){if(frame==12&&Run.lastDamage>0)HitSound(false);if(frame==22&&Run.lastReceived>0)HitSound(true);}if(Integrated)Invalidate(new Rectangle(0,0,Width,Math.Max(1,Height-OverlayHeight)),false);else Invalidate();};pulse.Start();}
 public void RestartAnimation(){frame=0;pulse.Stop();if(AnimateHit&&Mode=="feedback"||Mode=="loot"||Mode=="reward")pulse.Start();Invalidate();}
 protected override CreateParams CreateParams{get{var p=base.CreateParams;if(Integrated)p.ExStyle|=0x02000000;return p;}}
 protected override void OnPaintBackground(PaintEventArgs e){}
 protected override void Dispose(bool d){if(d){pulse.Dispose();if(backdrop!=null)backdrop.Dispose();}base.Dispose(d);}
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.SmoothingMode=SmoothingMode.None;
  if(backdrop==null||backdrop.Width!=Width||backdrop.Height!=Height||backdropArt!=Art){if(backdrop!=null)backdrop.Dispose();backdrop=new Bitmap(Math.Max(1,Width),Math.Max(1,Height));using(var bg=Graphics.FromImage(backdrop)){bg.Clear(Color.FromArgb(18,34,37));bg.InterpolationMode=InterpolationMode.NearestNeighbor;bg.PixelOffsetMode=PixelOffsetMode.Half;if(Art!=null){float scale=Math.Max((float)Width/Art.Width,(float)Height/Art.Height);float w=Art.Width*scale,h=Art.Height*scale;bg.DrawImage(Art,(Width-w)/2,(Height-h)*.62f,w,h);}using(var b=new SolidBrush(Color.FromArgb(35,5,14,18)))bg.FillRectangle(b,0,0,Width,Height);}backdropArt=Art;}
  bool hit=AnimateHit&&Mode=="feedback"&&frame<34;int shake=hit&&frame>=12&&frame<23?(frame%2==0?3:-3):0;g.DrawImageUnscaled(backdrop,shake,0);
  bool battle=Run!=null&&(Mode=="combat"||Mode=="feedback"||Mode=="boss-intro");float fieldHeight=Height;float baseY=battle?(Integrated?Height*.75f:Height*.82f):fieldHeight*.94f;float heroHeight=Math.Max(80,Math.Min(Height*.43f,330)),enemyHeight=Math.Max(80,Math.Min(Height*.46f,350));
  bool animated=battle&&PistolFrames!=null&&PistolFrames.Length==8;
  float lunge=!animated&&hit&&Run.lastDamage>0&&frame<14?(float)Math.Sin(frame/14.0*Math.PI)*24:0;
  var heroRect=SpriteRect(Hero,Width*.22f+lunge,baseY,heroHeight);var enemyRect=SpriteRect(EnemyArt,Width*.77f+(hit&&Run.lastDamage>0&&frame>=12&&frame<22?shake*2:0),baseY,enemyHeight);
  if(battle){DrawGroundShadow(g,Width*.22f,baseY,heroHeight*.36f);DrawGroundShadow(g,Width*.77f,baseY,enemyHeight*.43f);if(Support!=null)DrawGroundShadow(g,Width*.13f,baseY,heroHeight*.30f);}
  if(battle&&Support!=null){var supportRect=SpriteRect(Support,Width*.13f,baseY,heroHeight*.91f);DrawSprite(g,Support,supportRect,false);if(Run.cardBattle!=null&&Run.cardBattle.lastCard!=null){using(var pen=new Pen(Color.FromArgb(170,95,218,244),3)){g.DrawLine(pen,supportRect.Right-10,supportRect.Top+supportRect.Height/3,heroRect.Left+heroRect.Width/2,heroRect.Top+heroRect.Height/2);g.DrawEllipse(pen,heroRect.Left-8,heroRect.Top+heroRect.Height/3,heroRect.Width+16,heroRect.Height*2/3);}}}if(animated&&ShowDrone){int pose=0;if(hit&&Run.lastDamage>0)pose=frame<5?0:frame<9?1:frame<12?2:frame<15?3:frame<21?5:6;Image character=PistolFrames[pose];if(hit&&Run.lastReceived>0&&frame>=22&&frame<31&&ReactionFrames!=null)character=ReactionFrames[Run.armor>0||Run.guardUsed?7:6];heroRect=SpriteRect(character,Width*.22f+(hit&&Run.lastReceived>0&&frame>=22&&frame<28?shake:0),baseY,heroHeight);DrawSprite(g,character,heroRect,hit&&Run.lastReceived>0&&frame>=22&&frame<27);}
  else if(Hero!=null&&(battle||Run==null)&&ShowDrone){var facing=g.Save();g.TranslateTransform(heroRect.Left+heroRect.Right,0);g.ScaleTransform(-1,1);DrawSprite(g,Hero,heroRect,hit&&Run.lastReceived>0&&frame>=22&&frame<28);g.Restore(facing);}
  if(battle&&EnemyArt!=null&&!(AnimateHit&&Mode=="feedback"&&Run.enemyHp==0&&frame>24))DrawSprite(g,EnemyArt,enemyRect,hit&&Run.lastDamage>0&&frame>=(animated?16:12)&&frame<(animated?24:20));
  if(Run!=null){int barWidth=Math.Min(230,Width/4),barY=battle?Math.Max(Integrated?112:10,(int)(baseY-Math.Max(heroHeight,enemyHeight))-44):10;DrawBar(g,battle?(int)(Width*.22f)-barWidth/2:14,barY,barWidth,Run.hp,Run.maxHp,Color.FromArgb(124,215,153),"生命 "+Run.hp+" / "+Run.maxHp);if(battle)DrawBar(g,(int)(Width*.77f)-barWidth/2,barY,barWidth,Run.enemyHp,Run.enemyMax,Color.FromArgb(243,150,104),TowerEngine.EnemyName(Run)+"  "+Run.enemyHp+" / "+Run.enemyMax);}
  if(battle&&Run.cardBattle!=null){using(var f=GameTheme.Body(10)){var box=new Rectangle(Math.Max(10,(int)(Width*.50)),12,Height<560?Math.Max(180,(int)(Width*.27)):Math.Max(180,(int)(Width*.47)),Height<560?50:56);using(var brush=new SolidBrush(Color.FromArgb(185,12,23,35)))g.FillRectangle(brush,box);TextRenderer.DrawText(g,CardBattle.Intent(Run),f,box,Color.FromArgb(255,224,159),TextFormatFlags.WordBreak);}}if(Run==null){using(var f=new Font(GameTheme.BodyName,20,FontStyle.Bold))TextRenderer.DrawText(g,BannerTitle,f,new Rectangle(Width/3,45,Width*2/3-20,50),Color.FromArgb(255,232,184));TextRenderer.DrawText(g,BannerSubtitle,Font,new Rectangle(Width/3,100,Width*2/3-20,70),Color.FromArgb(223,217,178),TextFormatFlags.WordBreak);}
  if(hit&&!animated){float y=Height*.55f;int target=(int)(Width*.77f),start=(int)(Width*.26f);
   if(Run.lastDamage>0&&frame<16){int x=start+(target-start)*Math.Min(frame,13)/13;using(var b=new SolidBrush(Color.FromArgb(255,221,114))){g.FillRectangle(b,x-24,y-4,24,8);g.FillRectangle(b,x-38,y-2,12,4);} }
   if(Run.lastDamage>0&&frame>=12&&frame<25){Burst(g,target,(int)y,frame-12,Color.FromArgb(255,216,113));if(frame<21)using(var p=new Pen(Color.FromArgb(255,248,202),6)){g.DrawLine(p,target-26,(int)y+25,target+25,(int)y-26);g.DrawLine(p,target-16,(int)y+30,target+34,(int)y-14);}}
   if(Run.lastReceived>0&&frame>=15&&frame<23){int x=target+(start-target)*(frame-15)/7;using(var b=new SolidBrush(Color.FromArgb(255,112,88))){g.FillRectangle(b,x,y-4,25,8);g.FillRectangle(b,x+27,y-2,13,4);}}
   if(Run.lastReceived>0&&frame>=20&&frame<34)Burst(g,start,(int)y,frame-20,Color.FromArgb(255,113,103));
   using(var f=new Font(GameTheme.LatinName,20,FontStyle.Bold)){if(Run.lastDamage>0&&frame>=12)TextRenderer.DrawText(g,"-"+Run.lastDamage,f,new Rectangle(target-60,(int)y-50-(frame-12)*2,120,45),Color.FromArgb(255,236,148),TextFormatFlags.HorizontalCenter);if(Run.lastReceived>0&&frame>=20)TextRenderer.DrawText(g,"-"+Run.lastReceived,f,new Rectangle(start-55,(int)y-45-(frame-20)*2,110,40),Color.FromArgb(255,151,136),TextFormatFlags.HorizontalCenter);}
  }
  if(hit&&animated){int sx=heroRect.Left+(int)(heroRect.Width*.91),sy=heroRect.Top+(int)(heroRect.Height*.23),tx=enemyRect.Left+enemyRect.Width/2,ty=enemyRect.Top+enemyRect.Height/2;
   if(Run.lastDamage>0){if(frame>=12&&frame<15)CombatEffect(g,0,sx,sy,Math.Max(36,heroRect.Height/3));if(frame>=13&&frame<17){float t=(frame-13)/3f;CombatEffect(g,1,(int)(sx+(tx-sx)*t),(int)(sy+(ty-sy)*t),Math.Max(45,heroRect.Height/2));}if(frame>=16&&frame<24)CombatEffect(g,2,tx,ty,70+(frame-16)*4);}
   if(Run.lastReceived>0){if(frame>=18&&frame<22){float t=(frame-18)/3f;int x=(int)(tx+(heroRect.Left+heroRect.Width/2-tx)*t);using(var brush=new SolidBrush(Color.FromArgb(255,112,88)))g.FillRectangle(brush,x,ty,25,5);}if(frame>=22&&frame<30){int hx=heroRect.Left+heroRect.Width/2,hy=heroRect.Top+heroRect.Height/2;if(Run.armor>0||Run.guardUsed){CombatEffect(g,5,hx+heroRect.Width/3,hy,heroRect.Height);CombatEffect(g,6,hx+heroRect.Width/3,hy,heroRect.Height/2);}else CombatEffect(g,4,hx,hy,Math.Max(55,heroRect.Height/2));}}
   using(var font=new Font(GameTheme.LatinName,20,FontStyle.Bold)){if(Run.lastDamage>0&&frame>=16)TextRenderer.DrawText(g,"-"+Run.lastDamage,font,new Rectangle(tx-55,ty-45-(frame-16)*2,110,40),Color.FromArgb(255,236,148),TextFormatFlags.HorizontalCenter);if(Run.lastReceived>0&&frame>=22)TextRenderer.DrawText(g,"-"+Run.lastReceived,font,new Rectangle(heroRect.Left,heroRect.Top-20-(frame-22)*2,110,40),Color.FromArgb(255,151,136),TextFormatFlags.HorizontalCenter);}
  }
  if((Mode=="loot"||Mode=="reward")&&frame<28){Color color=Run!=null&&(Run.feedback??"").Contains("恢复")?Color.FromArgb(140,233,162):Color.FromArgb(255,213,106);using(var b=new SolidBrush(color))for(int i=0;i<16;i++){int x=Width/2+(i*47%200)-100,y=(int)(Height*.58f)-frame*3+i%4*13;g.FillRectangle(b,x,y,4,4);}}
  using(var p=new Pen(Color.FromArgb(113,143,113),2))g.DrawRectangle(p,1,1,Math.Max(1,Width-3),Math.Max(1,Height-3));
 }
 void CombatEffect(Graphics g,int cell,int x,int y,int size){if(Effects==null||Effects.Length<8||Effects[cell]==null)return;g.DrawImage(Effects[cell],new Rectangle(x-size/2,y-size/2,size,size));}
 static void DrawGroundShadow(Graphics g,float x,float y,float width){var state=g.Save();g.SmoothingMode=SmoothingMode.AntiAlias;for(int i=4;i>=0;i--)using(var brush=new SolidBrush(Color.FromArgb(22,0,0,0)))g.FillEllipse(brush,x-width/2-i*3,y-5-i,width+i*6,10+i*2);g.Restore(state);}
 static Rectangle SpriteRect(Image im,float center,float y,float h){if(im==null)return Rectangle.Empty;float w=h*im.Width/im.Height;return new Rectangle((int)(center-w/2),(int)(y-h),(int)w,(int)h);}
 static void DrawSprite(Graphics g,Image im,Rectangle r,bool flash){if(!flash){g.DrawImage(im,r);return;}using(var attr=new ImageAttributes()){attr.SetColorMatrix(new ColorMatrix(new float[][]{new[]{.45f,0f,0f,0f,0f},new[]{0f,.45f,0f,0f,0f},new[]{0f,0f,.45f,0f,0f},new[]{0f,0f,0f,1f,0f},new[]{.55f,.45f,.35f,0f,1f}}));g.DrawImage(im,r,0,0,im.Width,im.Height,GraphicsUnit.Pixel,attr);}}
 static void Burst(Graphics g,int x,int y,int age,Color color){using(var b=new SolidBrush(color))for(int i=0;i<12;i++){double a=i*Math.PI/6;int distance=7+age*3;g.FillRectangle(b,x+(int)(Math.Cos(a)*distance),y+(int)(Math.Sin(a)*distance),Math.Max(2,6-age/3),Math.Max(2,6-age/3));}}
 void DrawBar(Graphics g,int x,int y,int width,int value,int max,Color color,string text){
  var bounds=new Rectangle(x,y,width,22);var flags=TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding|TextFormatFlags.EndEllipsis;
  using(var font=GameTheme.Body(10)){foreach(var offset in new[]{new Point(-1,0),new Point(1,0),new Point(0,-1),new Point(0,1)}){var outline=bounds;outline.Offset(offset);TextRenderer.DrawText(g,text,font,outline,Color.FromArgb(8,13,20),flags);}TextRenderer.DrawText(g,text,font,bounds,Color.FromArgb(255,240,206),flags);}
  using(var b=new SolidBrush(Color.FromArgb(32,44,39)))g.FillRectangle(b,x,y+24,width,7);
  int fill=width*Math.Min(Math.Max(0,value),Math.Max(1,max))/Math.Max(1,max);using(var b=new SolidBrush(color))g.FillRectangle(b,x,y+24,fill,7);
  if(fill>0)using(var b=new SolidBrush(Color.FromArgb(200,255,255,255)))g.FillRectangle(b,x,y+24,fill,1);
 }
}








