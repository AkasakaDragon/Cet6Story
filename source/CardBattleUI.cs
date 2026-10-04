using System;using System.IO;using System.Linq;using System.Drawing;using System.Drawing.Drawing2D;using System.Windows.Forms;
public class SupportCardView:Control {
 public SupportCard Card;public Image Atlas,FrameAtlas;public bool Playable=true;public bool Large;public bool Locked;
 public SupportCardView(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);BackColor=Color.Transparent;Size=new Size(130,176);Cursor=Cursors.Hand;Margin=new Padding(5);TabStop=true;AccessibleRole=AccessibleRole.PushButton;}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){OnClick(EventArgs.Empty);e.Handled=true;}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);DrawCard(e.Graphics,Width,Height);} public void DrawCard(Graphics g,int width,int height){if(Card==null)return;g.SmoothingMode=SmoothingMode.AntiAlias;g.InterpolationMode=InterpolationMode.HighQualityBicubic;var state=g.Save();g.ScaleTransform(width/512f,height/768f);


  if(FrameAtlas!=null){int fw=FrameAtlas.Width/3;g.DrawImage(FrameAtlas,new RectangleF(0,0,512,768),new RectangleF(Card.rarity*fw,0,fw,FrameAtlas.Height),GraphicsUnit.Pixel);}else{using(var path=ExpeditionVisuals.Rounded(new RectangleF(30,25,452,718),65))using(var brush=new LinearGradientBrush(new Rectangle(30,25,452,718),Color.FromArgb(162,123,65),Color.FromArgb(48,36,29),90))g.FillPath(brush,path);using(var brush=new SolidBrush(Color.FromArgb(226,210,171)))g.FillRectangle(brush,72,420,368,260);}
  if(Atlas!=null){var portrait=new RectangleF(111,107,300,323);var saved=g.Save();using(var aperture=new GraphicsPath()){aperture.AddEllipse(portrait);g.SetClip(aperture);int cw=Atlas.Width/4,ch=Atlas.Height/4;g.DrawImage(Atlas,portrait,new RectangleF(Card.art%4*cw,Card.art/4*ch,cw,ch),GraphicsUnit.Pixel);}g.Restore(saved);if(FrameAtlas!=null){int fw=FrameAtlas.Width/3;float y=402f/768*FrameAtlas.Height;g.DrawImage(FrameAtlas,new RectangleF(0,402,512,366),new RectangleF(Card.rarity*fw,y,fw,FrameAtlas.Height-y),GraphicsUnit.Pixel);}using(var rim=new Pen(Color.FromArgb(130,240,217,163),3))g.DrawArc(rim,portrait,190,160);}
  using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center,Trimming=StringTrimming.EllipsisCharacter}){
   using(var font=new Font(GameTheme.BodyName,46,FontStyle.Bold,GraphicsUnit.Pixel))using(var dark=new SolidBrush(Color.FromArgb(45,54,87)))using(var white=new SolidBrush(Color.White)){g.DrawString(Card.cost.ToString(),font,dark,new RectangleF(60,88,102,76),format);g.DrawString(Card.cost.ToString(),font,white,new RectangleF(58,85,102,76),format);}
   using(var font=new Font(GameTheme.BodyName,Large?31:34,FontStyle.Bold,GraphicsUnit.Pixel))using(var ink=new SolidBrush(Color.FromArgb(53,33,23)))g.DrawString(Card.name,font,ink,new RectangleF(90,415,337,62),format);
   string text=Large?Card.text:Card.text.Replace("本回合答对：","答对：").Replace("本回合攻击","攻击");float effectSize=Large?34:31;while(effectSize>24){using(var measure=new Font(GameTheme.BodyName,effectSize,FontStyle.Regular,GraphicsUnit.Pixel)){if(g.MeasureString(text,measure,340,format).Height<=114)break;}effectSize-=1;}using(var font=new Font(GameTheme.BodyName,effectSize,FontStyle.Regular,GraphicsUnit.Pixel))using(var ink=new SolidBrush(Color.FromArgb(63,47,37)))g.DrawString(text,font,ink,new RectangleF(86,495,340,114),format);
   using(var font=new Font(GameTheme.BodyName,24,FontStyle.Bold,GraphicsUnit.Pixel))using(var ink=new SolidBrush(Color.FromArgb(95,68,43)))g.DrawString(Card.Quality+(Locked?" · 未解锁":" · 支援"),font,ink,new RectangleF(107,614,296,30),format);
  }
  if(!Playable)using(var tint=new SolidBrush(Color.FromArgb(120,13,20,28)))g.FillEllipse(tint,new RectangleF(111,107,300,323));g.Restore(state);
 }
}
public sealed class BattleEnergyBadge:Control {
 public int Remaining,Capacity;
 public BattleEnergyBadge(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);BackColor=Color.Transparent;AccessibleRole=AccessibleRole.StaticText;}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;var state=g.Save();g.ScaleTransform(Width/100f,Height/100f);
  using(var haloPath=new GraphicsPath()){haloPath.AddEllipse(1,1,98,98);using(var glow=new PathGradientBrush(haloPath)){glow.CenterColor=Color.FromArgb(105,50,194,240);glow.SurroundColors=new[]{Color.FromArgb(0,30,120,180)};g.FillPath(glow,haloPath);}}
  using(var shadow=new SolidBrush(Color.FromArgb(100,0,9,19)))g.FillEllipse(shadow,11,14,80,80);
  using(var outer=new LinearGradientBrush(new Rectangle(10,10,80,80),Color.FromArgb(245,221,154),Color.FromArgb(91,58,31),70))g.FillEllipse(outer,10,10,80,80);
  using(var dark=new SolidBrush(Color.FromArgb(17,31,43)))g.FillEllipse(dark,14,14,72,72);
  using(var rim=new Pen(Color.FromArgb(215,181,110),1.3f))g.DrawEllipse(rim,16,16,68,68);
  using(var gemPath=new GraphicsPath()){gemPath.AddEllipse(20,20,60,60);using(var gem=new PathGradientBrush(gemPath)){gem.CenterPoint=new PointF(36,30);gem.CenterColor=Color.FromArgb(104,229,251);gem.SurroundColors=new[]{Color.FromArgb(6,34,87)};g.FillPath(gem,gemPath);}}
  using(var facet=new SolidBrush(Color.FromArgb(35,169,245,255)))g.FillPolygon(facet,new[]{new PointF(26,30),new PointF(49,22),new PointF(69,34),new PointF(52,48)});
  using(var facet=new SolidBrush(Color.FromArgb(55,2,16,57)))g.FillPolygon(facet,new[]{new PointF(52,48),new PointF(75,41),new PointF(70,68),new PointF(45,78)});
  using(var edge=new Pen(Color.FromArgb(140,112,223,250),1))g.DrawEllipse(edge,20,20,60,60);
  using(var shine=new Pen(Color.FromArgb(200,190,248,255),2))g.DrawArc(shine,24,24,52,52,205,88);
  float fraction=Math.Max(0,Math.Min(1,Remaining/(float)Math.Max(1,Capacity)));
  using(var track=new Pen(Color.FromArgb(90,89,142,163),2.2f))g.DrawArc(track,6,6,88,88,35,290);
  if(fraction>0)using(var charge=new Pen(Color.FromArgb(200,103,223,248),2.2f)){charge.StartCap=LineCap.Round;charge.EndCap=LineCap.Round;g.DrawArc(charge,6,6,88,88,35,290*fraction);}
  foreach(var center in new[]{new PointF(50,10),new PointF(90,50),new PointF(50,90),new PointF(10,50)}){using(var ornament=new LinearGradientBrush(new RectangleF(center.X-4,center.Y-6,8,12),Color.FromArgb(255,230,168),Color.FromArgb(137,88,40),90))g.FillPolygon(ornament,new[]{new PointF(center.X,center.Y-6),new PointF(center.X+4,center.Y),new PointF(center.X,center.Y+6),new PointF(center.X-4,center.Y)});}
  using(var glint=new Pen(Color.FromArgb(205,223,254,255),1)){g.DrawLine(glint,30,24,30,34);g.DrawLine(glint,25,29,35,29);}
  string text=Remaining+"/"+Capacity;using(var font=new Font(GameTheme.LatinName,25,FontStyle.Bold,GraphicsUnit.Pixel))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center}){using(var shadow=new SolidBrush(Color.FromArgb(5,18,42)))foreach(var offset in new[]{new PointF(-1,0),new PointF(1,0),new PointF(0,-1),new PointF(0,2)})g.DrawString(text,font,shadow,new RectangleF(5+offset.X,30+offset.Y,90,40),format);using(var ink=new SolidBrush(Color.FromArgb(255,249,223)))g.DrawString(text,font,ink,new RectangleF(5,30,90,40),format);}
  g.Restore(state);AccessibleName="能量 "+Remaining+" / "+Capacity;
 }
}
public sealed class SupportCardPopup:Form {
 public SupportCardView View;
 public SupportCardPopup(SupportCard card,Image art,Image frame,Size size){FormBorderStyle=FormBorderStyle.None;ShowInTaskbar=false;StartPosition=FormStartPosition.Manual;BackColor=Color.Magenta;TransparencyKey=Color.Magenta;ClientSize=size;View=new SupportCardView{Card=card,Atlas=art,FrameAtlas=frame,Large=true,Dock=DockStyle.Fill,Cursor=Cursors.Default};Controls.Add(View);}
 protected override bool ShowWithoutActivation{get{return true;}}
 protected override CreateParams CreateParams{get{var p=base.CreateParams;p.ExStyle|=0x08000000;return p;}}
}
public partial class Game {
 string cardSpellingText="",cardSpellingKey="";
 Image SupportAtlas(){return CachedImage(Path.Combine(root,"assets","rogue","cards","illustrations.png"));}
 Image SupportSprite(int cell){if(cell<0)return null;string path=Path.Combine(root,"assets","rogue","cards","support-sprites.png");if(!File.Exists(path))return null;string key="support-sprite:"+cell;Image result;if(imageCache.TryGetValue(key,out result))return result;var atlas=CachedImage(path);int w=atlas.Width/4,h=atlas.Height/2;var bitmap=CombatSprites.Character(atlas,cell);w=bitmap.Width;h=bitmap.Height;int left=w,top=h,right=-1,bottom=-1;for(int y=0;y<h;y++)for(int x=0;x<w;x++)if(bitmap.GetPixel(x,y).A>48){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}if(right>=left){var trimmed=bitmap.Clone(Rectangle.FromLTRB(left,top,right+1,bottom+1),System.Drawing.Imaging.PixelFormat.Format32bppArgb);bitmap.Dispose();bitmap=trimmed;}imageCache[key]=bitmap;return bitmap;}
 Bitmap SupportHandArt(SupportCard card,bool playable){string key="support-hand:"+card.id+":"+playable;Image cached;if(imageCache.TryGetValue(key,out cached))return (Bitmap)cached;var art=new Bitmap(512,768,System.Drawing.Imaging.PixelFormat.Format32bppArgb);using(var view=new SupportCardView{Card=card,Atlas=SupportAtlas(),FrameAtlas=CardFrameAtlas(),Large=true,Playable=playable})using(var g=Graphics.FromImage(art))view.DrawCard(g,512,768);imageCache[key]=art;return art;}
 void AddSupportHand(RogueRun r,Panel glass,Control arena){
  var b=r.cardBattle;if(b==null||r.state!="combat"||b.answering)return;
  var hand=new BattleFanHand();arena.Controls.Add(hand);
  var energy=new BattleEnergyBadge{Remaining=b.energy,Capacity=b.energyCapacity>0?b.energyCapacity:Math.Max(3,b.energy)};arena.Controls.Add(energy);
  var info=new OutlinedLabel{Visible=b.overflow.Count>0,Text="请选择一张手牌弃置",ForeColor=Gold,BackColor=Color.Transparent,Font=GameTheme.Body(10)};arena.Controls.Add(info);
  var end=RogueButton("结束回合 · Enter",()=>{if(CardBattle.EndTurn(r))SaveRogue();},160,46);end.Enabled=b.overflow.Count==0;arena.Controls.Add(end);
  for(int i=0;i<Math.Min(8,b.hand.Count);i++){var card=CardBattle.Get(b.hand[i]);bool playable=b.overflow.Count>0||card.cost<=b.energy&&(card.id!="rethink"||b.hand.Count>1);hand.AddCard(SupportHandArt(card,playable),card.name+" · "+card.text,false);}
  hand.PlayCard=index=>{if(b.overflow.Count>0?CardBattle.DiscardChoice(r,index):CardBattle.Play(r,index))SaveRogue();};
  Action layout=()=>{bool compact=arena.Height<560;int cardHeight=compact?190:Math.Min(340,Math.Max(250,(int)(arena.Height*.32)));hand.CardHeight=cardHeight;int side=Math.Min(150,Math.Max(100,arena.Width/6));int height=Math.Min(arena.Height-105,(int)(cardHeight*1.32)+40);hand.Bounds=new Rectangle(side,arena.Height-height,Math.Max(100,arena.Width-side*2),height);int badgeSize=compact?76:100;energy.Bounds=new Rectangle(24,arena.Height-badgeSize-45,badgeSize,badgeSize);info.Bounds=new Rectangle(18,Math.Max(115,arena.Height-height-28),Math.Max(80,arena.Width-200),26);end.Location=new Point(Math.Max(0,arena.Width-175),arena.Height-70);hand.Invalidate();};
  EventHandler resized=(sender,args)=>layout();arena.Resize+=resized;battleLayoutCleanup=()=>arena.Resize-=resized;layout();hand.BringToFront();info.BringToFront();energy.BringToFront();end.BringToFront();
 }
 void RenderCardRewards(RogueRun r){
  ClearPage();page="rogue";rogueBody=null;rogueArena=null;
  var b=r.cardBattle;var surface=new CardRewardSurface{Dock=DockStyle.Fill,Art=TowerBackground(r)};content.Controls.Add(surface);
  var title=Lab("卡牌战利品 · 三选一",22,Gold);title.TextAlign=ContentAlignment.MiddleCenter;title.BackColor=Color.Transparent;surface.Controls.Add(title);
  var description=Lab("金币已结算。选择一张加入本局牌库，也可以跳过。"+(String.IsNullOrEmpty(b.unlockNotice)?"":"\n永久解锁："+b.unlockNotice),11,Muted);description.TextAlign=ContentAlignment.MiddleCenter;description.BackColor=Color.Transparent;surface.Controls.Add(description);
  var cards=new System.Collections.Generic.List<SupportCardView>();foreach(string id in b.rewards){string selected=id;var card=new SupportCardView{Card=CardBattle.Get(id),Atlas=SupportAtlas(),FrameAtlas=CardFrameAtlas(),Large=true};card.Click+=(sender,args)=>{if(CardBattle.Pick(save.rogue,selected))SaveRogue();};surface.Controls.Add(card);cards.Add(card);}
  var skip=RogueButton("跳过 · 保持当前牌库",()=>{if(CardBattle.Pick(save.rogue,null))SaveRogue();},280,44);surface.Controls.Add(skip);
  Action layout=()=>{int gap=18,count=Math.Max(1,cards.Count);int cardHeight=Math.Min(390,Math.Max(150,Math.Min(surface.Height-175,((surface.Width-64-gap*(count-1))/count)*3/2)));int cardWidth=cardHeight*2/3;int textWidth=Math.Max(100,Math.Min(960,surface.Width-48));title.MaximumSize=description.MaximumSize=new Size(textWidth,0);title.Size=title.GetPreferredSize(new Size(textWidth,0));description.Size=description.GetPreferredSize(new Size(textWidth,0));int blockHeight=title.Height+description.Height+cardHeight+44+48;int top=Math.Max(16,(surface.Height-blockHeight)/2);title.Location=new Point((surface.Width-title.Width)/2,top);description.Location=new Point((surface.Width-description.Width)/2,title.Bottom+10);int rowY=description.Bottom+18,total=cards.Count*cardWidth+Math.Max(0,cards.Count-1)*gap;for(int i=0;i<cards.Count;i++)cards[i].Bounds=new Rectangle((surface.Width-total)/2+i*(cardWidth+gap),rowY,cardWidth,cardHeight);skip.Width=Math.Min(280,surface.Width-48);skip.Location=new Point((surface.Width-skip.Width)/2,rowY+cardHeight+18);};surface.Resize+=(sender,args)=>layout();layout();
 }
 void ShowCardCollection(){CardBattle.Profile(save.rogue);RoguePage("卡牌图鉴 · 男主支援");RogueCard("收藏进度", "已解锁 "+save.rogue.unlockedCards.Count+" / 16 · 小怪击杀 "+save.rogue.smallKills+" · 精英击杀 "+save.rogue.eliteKills+"\n首次击败不同小怪解锁专属牌；精英解锁稀有牌，累计 3 / 6 / 9 次精英解锁史诗牌。新远征从 8 张基础牌开始。");var row=new FlowLayoutPanel{AutoSize=true,WrapContents=true,MaximumSize=new Size(Math.Max(650,content.Width-70),0)};rogueBody.Controls.Add(row);foreach(var c in CardBattle.Cards){bool owned=save.rogue.unlockedCards.Contains(c.id);var view=new SupportCardView{Card=c,Atlas=SupportAtlas(),FrameAtlas=CardFrameAtlas(),Large=true,Size=new Size(190,285),Playable=owned,Locked=!owned};tips.SetToolTip(view,(owned?"已解锁":"尚未解锁")+" · "+c.text);row.Controls.Add(view);}RogueLayout();}
}























public sealed class CardRewardSurface:Panel {
 public Image Art;
 public CardRewardSurface(){DoubleBuffered=true;ResizeRedraw=true;BackColor=Color.FromArgb(13,25,34);}
 protected override void OnPaintBackground(PaintEventArgs e){ExpeditionVisuals.Background(e.Graphics,Art,ClientRectangle,70);}
}


