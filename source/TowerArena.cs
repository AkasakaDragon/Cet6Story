using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

public partial class RogueArena:Control {
 public string BannerTitle="词域远征",BannerSubtitle="答题 · 战斗 · 随机赋能";public bool ShowDrone=true;public Image Art,Hero,EnemyArt,Support;public Image[] PistolFrames,ReactionFrames,Effects,SupportFrames;public RogueRun Run;public string Mode="home";public bool Integrated;public int OverlayHeight;public bool AnimateHit;public Action<bool> HitSound;public Action<string> MonsterSound;public Action AnimationCompleted;Timer pulse;int frame;Bitmap backdrop;Image backdropArt;readonly Stopwatch animationTime=new Stopwatch();Bitmap enemyNormal,enemyFlash;Image gradedEnemy;int gradedTheme=-1;
 public RogueArena(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);ResizeRedraw=true;pulse=new Timer{Interval=16};pulse.Tick+=(s,e)=>{bool combatAnimation=AnimateHit&&Mode=="feedback"&&Run!=null;bool rewardAnimation=Mode=="loot"||Mode=="reward";if((!combatAnimation&&!rewardAnimation)||frame>=(combatAnimation?52:28)){pulse.Stop();return;}int next=Math.Min(combatAnimation?52:28,(int)(animationTime.Elapsed.TotalMilliseconds/35));if(next<=frame)return;while(frame<next){frame++;if(combatAnimation)EmitCombatSounds();}if(combatAnimation){if(frame>=52){pulse.Stop();if(AnimationCompleted!=null)AnimationCompleted();}}if(Integrated)Invalidate(new Rectangle(0,0,Width,Math.Max(1,Height-OverlayHeight)),false);else Invalidate();};animationTime.Start();pulse.Start();}
 public void RestartAnimation(){frame=0;animationTime.Restart();pulse.Stop();if(AnimateHit&&Mode=="feedback"||Mode=="loot"||Mode=="reward")pulse.Start();Invalidate();}
 protected override void OnPaintBackground(PaintEventArgs e){}
 protected override void Dispose(bool d){if(d){pulse.Dispose();statusTips.Dispose();if(backdrop!=null)backdrop.Dispose();ClearEnemyCache();}base.Dispose(d);}
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.SmoothingMode=SmoothingMode.None;
  if(backdrop==null||backdrop.Width!=Width||backdrop.Height!=Height||backdropArt!=Art){if(backdrop!=null)backdrop.Dispose();backdrop=new Bitmap(Math.Max(1,Width),Math.Max(1,Height));using(var bg=Graphics.FromImage(backdrop)){bg.Clear(Color.FromArgb(18,34,37));bg.InterpolationMode=InterpolationMode.NearestNeighbor;bg.PixelOffsetMode=PixelOffsetMode.Half;if(Art!=null){float scale=Math.Max((float)Width/Art.Width,(float)Height/Art.Height);float w=Art.Width*scale,h=Art.Height*scale;bg.DrawImage(Art,(Width-w)/2,(Height-h)*.62f,w,h);}using(var b=new SolidBrush(Color.FromArgb(35,5,14,18)))bg.FillRectangle(b,0,0,Width,Height);}backdropArt=Art;}
  bool hit=AnimateHit&&Mode=="feedback"&&frame<52;int shake=hit&&((frame>=12&&frame<23)||(frame>=36&&frame<42))?(frame%2==0?3:-3):0;g.DrawImageUnscaled(backdrop,shake,0);
  bool battle=Run!=null&&(Mode=="combat"||Mode=="feedback"||Mode=="boss-intro");float baseY=battle?BattleGroundY():Height*.94f;float heroHeight=Math.Max(80,Math.Min(Height*.43f,330)),enemyHeight=Math.Max(80,Math.Min(Height*.46f,350));
  float heroX=Width*.22f,supportX=Width*.13f,supportHeight=heroHeight*1.05f;
  if(battle&&Support!=null){
   // Reserve room for the widest attack/support pose, so casting never hides either actor.
   float heroWidth=Math.Max(MaxSpriteWidth(PistolFrames,heroHeight,PistolFrames==null?Hero:null),MaxSpriteWidth(ReactionFrames,heroHeight,null)),supportWidth=MaxSpriteWidth(SupportFrames,supportHeight,Support);
   float gap=Math.Max(12,heroHeight*.06f),available=Width*.43f;
   float fit=Math.Min(1,available/(heroWidth+supportWidth+gap));heroHeight*=fit;supportHeight*=fit;heroWidth*=fit;supportWidth*=fit;
   float left=Math.Max(12,Width*.23f-(heroWidth+supportWidth+gap)/2);
   supportX=left+supportWidth/2;heroX=left+supportWidth+gap+heroWidth/2;
  }
  bool animated=battle&&PistolFrames!=null&&PistolFrames.Length==8;
  float lunge=!animated&&hit&&Run.lastDamage>0&&frame<14?(float)Math.Sin(frame/14.0*Math.PI)*24:0;
  var heroRect=SpriteRect(Hero,heroX+lunge,baseY,heroHeight);var enemyRect=SpriteRect(EnemyArt,Width*.77f+(hit&&Run.lastDamage>0&&frame>=12&&frame<22?shake*2:0),baseY,enemyHeight);
  if(battle){DrawGroundShadow(g,heroX,baseY,heroHeight*.36f);DrawGroundShadow(g,Width*.77f,baseY,enemyHeight*.43f);if(Support!=null)DrawGroundShadow(g,supportX,baseY,supportHeight*.30f);}
  if(battle&&Support!=null){var supportRect=SpriteRect(Support,supportX,baseY,supportHeight);DrawSprite(g,Support,supportRect,false);if(Run.cardBattle!=null&&Run.cardBattle.lastCard!=null){using(var pen=new Pen(Color.FromArgb(170,95,218,244),3)){g.DrawLine(pen,supportRect.Right-10,supportRect.Top+supportRect.Height/3,heroRect.Left+heroRect.Width/2,heroRect.Top+heroRect.Height/2);g.DrawEllipse(pen,heroRect.Left-8,heroRect.Top+heroRect.Height/3,heroRect.Width+16,heroRect.Height*2/3);}}}if(animated&&ShowDrone){int pose=0;if(hit&&Run.lastDamage>0)pose=frame<5?0:frame<9?1:frame<12?2:frame<15?3:frame<21?5:6;Image character=PistolFrames[pose];if(hit&&Run.lastReceived>0&&frame>=36&&frame<45&&ReactionFrames!=null)character=ReactionFrames[Run.armor>0||Run.guardUsed?7:6];heroRect=SpriteRect(character,heroX+(hit&&Run.lastReceived>0&&frame>=36&&frame<42?shake:0),baseY,heroHeight);DrawSprite(g,character,heroRect,hit&&Run.lastReceived>0&&frame>=36&&frame<41);}
  else if(Hero!=null&&(battle||Run==null)&&ShowDrone){var facing=g.Save();g.TranslateTransform(heroRect.Left+heroRect.Right,0);g.ScaleTransform(-1,1);DrawSprite(g,Hero,heroRect,hit&&Run.lastReceived>0&&frame>=36&&frame<42);g.Restore(facing);}
  if(hit)enemyRect=MonsterCombat.Pose(Run,enemyRect,frame);
  if(battle&&EnemyArt!=null&&!(AnimateHit&&Mode=="feedback"&&Run.enemyHp==0&&frame>24))DrawEnemy(g,EnemyArt,enemyRect,hit&&Run.lastDamage>0&&frame>=(animated?16:12)&&frame<(animated?24:20));
  if(hit)MonsterCombat.Draw(g,Run,enemyRect,heroRect,frame);
  if(Run!=null){
   int barWidth=Math.Max(170,Math.Min(270,Width/5));
   int heroBarY=battle?Math.Max(100,(int)(baseY-Math.Max(heroHeight,supportHeight))-76):10;
   int enemyBarY=Math.Max(100,enemyRect.Top-76);
   DrawBar(g,battle?(int)heroX-barWidth/2:14,heroBarY,barWidth,Run.hp,Run.maxHp,Color.FromArgb(110,173,145),"双星 · 生命",false);
   if(battle){
    DrawBar(g,(int)(Width*.77f)-barWidth/2,enemyBarY,barWidth,Run.enemyHp,Run.enemyMax,Color.FromArgb(189,132,103),TowerEngine.EnemyName(Run),true);
    if(Run.cardBattle!=null)DrawIntent(g,(int)(Width*.77f),enemyBarY-36);
   }
  }
  if(Run==null){using(var f=GameTheme.Body(20,FontStyle.Bold))GameTheme.DrawText(g,BannerTitle,f,new Rectangle(Width/3,45,Width*2/3-20,50),Color.FromArgb(255,232,184));GameTheme.DrawText(g,BannerSubtitle,Font,new Rectangle(Width/3,100,Width*2/3-20,70),Color.FromArgb(223,217,178),TextFormatFlags.WordBreak);}
  if(hit&&!animated){float y=Height*.55f;int target=(int)(Width*.77f),start=(int)heroX;
   if(Run.lastDamage>0&&frame<16){int x=start+(target-start)*Math.Min(frame,13)/13;using(var b=new SolidBrush(Color.FromArgb(255,221,114))){g.FillRectangle(b,x-24,y-4,24,8);g.FillRectangle(b,x-38,y-2,12,4);} }
   if(Run.lastDamage>0&&frame>=12&&frame<25){Burst(g,target,(int)y,frame-12,Color.FromArgb(255,216,113));if(frame<21)using(var p=new Pen(Color.FromArgb(255,248,202),6)){g.DrawLine(p,target-26,(int)y+25,target+25,(int)y-26);g.DrawLine(p,target-16,(int)y+30,target+34,(int)y-14);}}
   using(var f=GameTheme.Latin(20,FontStyle.Bold)){if(Run.lastDamage>0&&frame>=12)GameTheme.DrawText(g,"-"+Run.lastDamage,f,new Rectangle(target-60,(int)y-50-(frame-12)*2,120,45),Color.FromArgb(255,236,148),TextFormatFlags.HorizontalCenter);if(Run.lastReceived>0&&frame>=36)GameTheme.DrawText(g,"-"+Run.lastReceived,f,new Rectangle(start-55,(int)y-45-(frame-36)*2,110,40),Color.FromArgb(255,151,136),TextFormatFlags.HorizontalCenter);}
  }
  if(hit&&animated){int sx=heroRect.Left+(int)(heroRect.Width*.91),sy=heroRect.Top+(int)(heroRect.Height*.23),tx=enemyRect.Left+enemyRect.Width/2,ty=enemyRect.Top+enemyRect.Height/2;
   if(Run.lastDamage>0){if(frame>=12&&frame<15)CombatEffect(g,0,sx,sy,Math.Max(36,heroRect.Height/3));if(frame>=13&&frame<17){float t=(frame-13)/3f;CombatEffect(g,1,(int)(sx+(tx-sx)*t),(int)(sy+(ty-sy)*t),Math.Max(45,heroRect.Height/2));}if(frame>=16&&frame<24)CombatEffect(g,2,tx,ty,70+(frame-16)*4);}
   if(Run.lastReceived>0&&frame>=36&&frame<46){int hx=heroRect.Left+heroRect.Width/2,hy=heroRect.Top+heroRect.Height/2;if(Run.armor>0||Run.guardUsed){CombatEffect(g,5,hx+heroRect.Width/3,hy,heroRect.Height);CombatEffect(g,6,hx+heroRect.Width/3,hy,heroRect.Height/2);}else CombatEffect(g,4,hx,hy,Math.Max(55,heroRect.Height/2));}
   using(var font=GameTheme.Latin(20,FontStyle.Bold)){if(Run.lastDamage>0&&frame>=16)GameTheme.DrawText(g,"-"+Run.lastDamage,font,new Rectangle(tx-55,ty-45-(frame-16)*2,110,40),Color.FromArgb(255,236,148),TextFormatFlags.HorizontalCenter);if(Run.lastReceived>0&&frame>=36)GameTheme.DrawText(g,"-"+Run.lastReceived,font,new Rectangle(heroRect.Left,heroRect.Top-20-(frame-36)*2,110,40),Color.FromArgb(255,151,136),TextFormatFlags.HorizontalCenter);}
  }
  if((Mode=="loot"||Mode=="reward")&&frame<28){Color color=Run!=null&&(Run.feedback??"").Contains("恢复")?Color.FromArgb(140,233,162):Color.FromArgb(255,213,106);using(var b=new SolidBrush(color))for(int i=0;i<16;i++){int x=Width/2+(i*47%200)-100,y=(int)(Height*.58f)-frame*3+i%4*13;g.FillRectangle(b,x,y,4,4);}}
  using(var p=new Pen(Color.FromArgb(113,143,113),2))g.DrawRectangle(p,1,1,Math.Max(1,Width-3),Math.Max(1,Height-3));
 }
 // One timeline for projectile launch, material hit, enemy cast, and player impact.
 void EmitCombatSounds(){
  if(frame==12&&Run.lastDamage>0&&HitSound!=null)HitSound(false);
  if(frame==16&&Run.lastDamage>0&&MonsterSound!=null)MonsterSound(MonsterCombat.SoundKey(Run,"hurt"));
  string action=MonsterCombat.Action(Run);
  if(frame==24&&action!="none"&&MonsterSound!=null)MonsterSound(MonsterCombat.SoundKey(Run,action));
  if(frame==36&&Run.lastReceived>0&&HitSound!=null)HitSound(true);
 }
 // Use the same cover transform as the backdrop. The stage floor is at 84% of the source scene.
 float BattleGroundY(){if(Art==null)return Height*.84f;float scale=Math.Max(Width/(float)Art.Width,Height/(float)Art.Height);float scaledHeight=Art.Height*scale;return Math.Max(Height*.70f,Math.Min(Height*.90f,(Height-scaledHeight)*.62f+scaledHeight*.84f));}
 static float MaxSpriteWidth(Image[] frames,float height,Image fallback){float width=fallback==null?0:height*fallback.Width/fallback.Height;if(frames!=null)foreach(var image in frames)if(image!=null)width=Math.Max(width,height*image.Width/image.Height);return width;}
 void CombatEffect(Graphics g,int cell,int x,int y,int size){if(Effects==null||Effects.Length<8||Effects[cell]==null)return;g.DrawImage(Effects[cell],new Rectangle(x-size/2,y-size/2,size,size));}
 static void DrawGroundShadow(Graphics g,float x,float y,float width){var state=g.Save();g.SmoothingMode=SmoothingMode.AntiAlias;for(int i=4;i>=0;i--)using(var brush=new SolidBrush(Color.FromArgb(22,0,0,0)))g.FillEllipse(brush,x-width/2-i*3,y-5-i,width+i*6,10+i*2);g.Restore(state);}
 static Rectangle SpriteRect(Image im,float center,float y,float h){if(im==null)return Rectangle.Empty;float w=h*im.Width/im.Height;return new Rectangle((int)(center-w/2),(int)(y-h),(int)w,(int)h);}
 // Grade every enemy, including elites and bosses, against the current stage palette.
 void DrawEnemy(Graphics g,Image image,Rectangle bounds,bool flash){
  int theme=Run==null?0:Math.Max(0,Math.Min(2,Run.theme));
  if(gradedEnemy!=image||gradedTheme!=theme){ClearEnemyCache();gradedEnemy=image;gradedTheme=theme;}
  var cached=flash?enemyFlash:enemyNormal;if(cached!=null){g.DrawImage(cached,bounds);return;}
  float[] tint=theme==0?new[]{.88f,.94f,.86f}:theme==1?new[]{.84f,.90f,.98f}:new[]{.98f,.86f,.80f};
  const float saturation=.52f;float gray=1-saturation,light=flash?.45f:1f;
  var rows=new float[5][];float[] luminance={.2126f,.7152f,.0722f};
  for(int input=0;input<3;input++){rows[input]=new float[5];for(int output=0;output<3;output++)rows[input][output]=(gray*luminance[input]+(input==output?saturation:0))*tint[output]*light;}
  rows[3]=new[]{0f,0f,0f,1f,0f};rows[4]=flash?new[]{.55f,.45f,.35f,0f,1f}:new[]{.025f,.025f,.025f,0f,1f};
  var result=new Bitmap(image.Width,image.Height,PixelFormat.Format32bppPArgb);using(var target=Graphics.FromImage(result))using(var attributes=new ImageAttributes()){attributes.SetColorMatrix(new ColorMatrix(rows));target.DrawImage(image,new Rectangle(0,0,image.Width,image.Height),0,0,image.Width,image.Height,GraphicsUnit.Pixel,attributes);}if(flash)enemyFlash=result;else enemyNormal=result;g.DrawImage(result,bounds);
 }
 void ClearEnemyCache(){if(enemyNormal!=null)enemyNormal.Dispose();if(enemyFlash!=null)enemyFlash.Dispose();enemyNormal=enemyFlash=null;}
 static void DrawSprite(Graphics g,Image im,Rectangle r,bool flash){if(!flash){g.DrawImage(im,r);return;}using(var attr=new ImageAttributes()){attr.SetColorMatrix(new ColorMatrix(new float[][]{new[]{.45f,0f,0f,0f,0f},new[]{0f,.45f,0f,0f,0f},new[]{0f,0f,.45f,0f,0f},new[]{0f,0f,0f,1f,0f},new[]{.55f,.45f,.35f,0f,1f}}));g.DrawImage(im,r,0,0,im.Width,im.Height,GraphicsUnit.Pixel,attr);}}
 static void Burst(Graphics g,int x,int y,int age,Color color){using(var b=new SolidBrush(color))for(int i=0;i<12;i++){double a=i*Math.PI/6;int distance=7+age*3;g.FillRectangle(b,x+(int)(Math.Cos(a)*distance),y+(int)(Math.Sin(a)*distance),Math.Max(2,6-age/3),Math.Max(2,6-age/3));}}
 void DrawIntent(Graphics g,int center,int barY){
  int width=Math.Min(360,Math.Max(220,Width*28/100));width=Math.Min(width,Width-24);
  string intent=Mode=="feedback"&&!String.IsNullOrEmpty(Run.cardBattle.log)?Run.cardBattle.log.Split((char)10)[0]:CardBattle.Intent(Run);int split=intent.IndexOf(" · ");string detail=split>=0?intent.Substring(split+3):intent;
  using(var font=GameTheme.Body(10))using(var titleFont=GameTheme.Body(9,FontStyle.Bold)){
   var flags=TextFormatFlags.WordBreak|TextFormatFlags.NoPadding;
   int detailHeight=TextRenderer.MeasureText(g,detail,font,new Size(width-32,200),flags).Height;
   int height=detailHeight+43,x=Math.Max(12,Math.Min(Width-width-12,center-width/2)),y=Math.Max(12,barY-height-10);
   var bounds=new Rectangle(x,y,width,height);var state=g.Save();g.SmoothingMode=SmoothingMode.AntiAlias;
   CyberChrome.Panel(g,bounds,CyberChrome.Neon);
   using(var accent=new SolidBrush(Color.FromArgb(194,163,112)))g.FillEllipse(accent,x+14,y+13,6,6);
   string move=Mode=="feedback"?MonsterCombat.Action(Run):MonsterCombat.PlannedAction(Run);string moveLabel=move=="skill-a"?"技能 A":move=="skill-b"?"技能 B":move=="normal"?"普通攻击":"行动意图";
   GameTheme.DrawText(g,"第 "+Run.cardBattle.turn+" 回合 · "+moveLabel,titleFont,new Rectangle(x+27,y+7,width-41,20),Color.FromArgb(205,185,146),TextFormatFlags.NoPadding|TextFormatFlags.VerticalCenter);
   GameTheme.DrawText(g,detail,font,new Rectangle(x+16,y+31,width-32,detailHeight+3),Color.FromArgb(232,231,218),flags);
   g.Restore(state);
  }
 }
 void DrawBar(Graphics g,int x,int y,int width,int value,int max,Color color,string name,bool enemy){
  x=Math.Max(10,Math.Min(Width-width-10,x));var state=g.Save();g.SmoothingMode=SmoothingMode.AntiAlias;
  DrawStatuses(g,x,y-34,width,enemy);var panel=new Rectangle(x,y,width,62);
  CyberChrome.Panel(g,panel,CyberChrome.Neon);
  using(var font=GameTheme.Body(10,FontStyle.Bold))GameTheme.DrawText(g,name,font,new Rectangle(x+12,y+7,width-24,21),Color.FromArgb(228,232,221),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding|TextFormatFlags.EndEllipsis);
  var track=new Rectangle(x+12,y+33,width-24,18);
  using(var path=ExpeditionVisuals.Rounded(track,5)){
   using(var brush=new SolidBrush(Color.FromArgb(38,49,52)))g.FillPath(brush,path);
   var clip=g.Save();g.SetClip(path,CombineMode.Intersect);
   int shield=Run!=null&&Run.cardBattle!=null?(enemy?Run.cardBattle.enemyShield:Run.cardBattle.shield):0;int capacity=Math.Max(1,Math.Max(max,value+shield));int fill=(int)(track.Width*Math.Min(Math.Max(0,value),(double)capacity)/capacity);
   if(fill>0){var progress=new Rectangle(track.X,track.Y,fill,track.Height);using(var brush=new LinearGradientBrush(track,Color.FromArgb(Math.Min(255,color.R+25),Math.Min(255,color.G+25),Math.Min(255,color.B+25)),color,90))g.FillRectangle(brush,progress);using(var shine=new Pen(Color.FromArgb(70,255,255,255)))g.DrawLine(shine,track.Left,track.Top+1,track.Left+fill,track.Top+1);}
   DrawStatusBars(g,track,value,max,enemy);g.Restore(clip);using(var edge=new Pen(Color.FromArgb(80,166,180,169)))g.DrawPath(edge,path);
  }
  string health=Math.Max(0,value)+" / "+Math.Max(1,max);
  using(var font=GameTheme.Body(9,FontStyle.Bold)){var flags=TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding;var shadow=track;shadow.Offset(0,1);GameTheme.DrawText(g,health,font,shadow,Color.FromArgb(15,25,27),flags);GameTheme.DrawText(g,health,font,track,Color.FromArgb(246,246,232),flags);}
  g.Restore(state);
 }
}