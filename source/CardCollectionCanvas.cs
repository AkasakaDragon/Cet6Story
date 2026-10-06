using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

// A single buffered surface owns the scene, album, transparent cards and page turn.
public sealed class CardCollectionCanvas:Control {
 readonly List<Bitmap> cards=new List<Bitmap>();readonly List<SupportCard> definitions=new List<SupportCard>();
 readonly ToolTip tips=new ToolTip();readonly Timer turnTimer=new Timer{Interval=16};readonly Stopwatch turnClock=new Stopwatch();
 readonly Font titleFont=GameTheme.Body(20,FontStyle.Bold);
 HashSet<string> ownedCards;Image atlas,frames;Bitmap backdrop,enlarged,oldSpread,newSpread;
 Size backdropSize;Image backdropArt;int pageIndex,hover=-1,selected=-1,direction;string hoverButton="";
 public Image SceneArt;public string Description;public string ReturnText="返回图书馆";public Action PageTurnSound,ReturnToLibrary,ButtonHoverSound,ButtonClickSound;
 public int PageIndex{get{return pageIndex;}}public int PageCount{get{return Math.Max(1,(cards.Count+7)/8);}}
 public bool IsTurning{get{return oldSpread!=null;}}public int ScrollOffset{get{return pageIndex*8;}}
 public CardCollectionCanvas(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.Selectable,true);BackColor=GameTheme.Navy;Font=GameTheme.Body(11);TabStop=true;AccessibleName="卡牌图鉴 · 翻页书册";turnTimer.Tick+=(s,e)=>{if(turnClock.ElapsedMilliseconds>=380)FinishTurn();Invalidate();};}
 public void LoadCards(IEnumerable<SupportCard> source,IEnumerable<string> unlocked,Image atlas,Image frames){
  FinishTurn();foreach(var image in cards)image.Dispose();cards.Clear();definitions.Clear();ownedCards=new HashSet<string>(unlocked);this.atlas=atlas;this.frames=frames;
  foreach(var card in source.OrderBy(c=>ownedCards.Contains(c.id)?0:1)){
   definitions.Add(card);cards.Add(RenderCard(card,384,576));
  }
  pageIndex=0;Invalidate();
 }
 Bitmap RenderCard(SupportCard card,int w,int h){
  var image=new Bitmap(w,h,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
  using(var view=new SupportCardView{Card=card,Atlas=atlas,FrameAtlas=frames,Large=true,Playable=ownedCards.Contains(card.id),Locked=!ownedCards.Contains(card.id)})
  using(var g=Graphics.FromImage(image)){g.Clear(Color.Transparent);view.DrawCard(g,w,h);}
  return image;
 }
 public Rectangle BookBounds{get{int w=(int)(Width*.64),h=(int)(Height*.61);return new Rectangle((Width-w)/2,(int)(Height*.235),Math.Max(1,w),Math.Max(1,h));}}
 Rectangle PaperBounds{get{var book=BookBounds;return Rectangle.Inflate(book,-Math.Max(8,book.Width/45),-Math.Max(9,book.Height/30));}}
 Rectangle BackBounds{get{return new Rectangle(Math.Max(14,Width/25),Height-Math.Max(62,Height/12),180,48);}}
 Rectangle PreviousBounds{get{var b=BookBounds;return new Rectangle(b.Left,b.Bottom+12,118,40);}}
 Rectangle NextBounds{get{var b=BookBounds;return new Rectangle(b.Right-118,b.Bottom+12,118,40);}}
 public Rectangle GetCardBounds(int slot){
  var paper=PaperBounds;int half=paper.Width/2,margin=Math.Max(12,paper.Width/42),top=Math.Max(26,paper.Height/13),bottom=Math.Max(20,paper.Height/14),gap=Math.Max(8,paper.Height/35);
  int w=Math.Max(1,Math.Min((half-margin*3)/2,(paper.Height-top-bottom-gap)/3));int h=w*3/2;
  int side=slot/4,local=slot%4,col=local%2,row=local/2;
  int group=2*w+margin;int left=paper.Left+side*half+(half-group)/2;
  int height=2*h+gap;int y=paper.Top+top+(paper.Height-top-bottom-height)/2;
  return new Rectangle(left+col*(w+margin),y+row*(h+gap),w,h);
 }
 public void TurnPage(int step){
  if(IsTurning||selected>=0||step==0)return;int target=Math.Max(0,Math.Min(PageCount-1,pageIndex+Math.Sign(step)));if(target==pageIndex)return;
  oldSpread=RenderSpread(pageIndex);direction=Math.Sign(step);pageIndex=target;newSpread=RenderSpread(pageIndex);hover=-1;tips.SetToolTip(this,null);
  if(PageTurnSound!=null)PageTurnSound();turnClock.Restart();turnTimer.Start();Invalidate();
 }
 // Kept for callers that previously scrolled the collection programmatically.
 public void SetOffset(int value){FinishTurn();pageIndex=Math.Max(0,Math.Min(PageCount-1,value/8));Invalidate();}
 void FinishTurn(){turnTimer.Stop();turnClock.Stop();if(oldSpread!=null){oldSpread.Dispose();oldSpread=null;}if(newSpread!=null){newSpread.Dispose();newSpread=null;}}
 protected override void OnResize(EventArgs e){FinishTurn();base.OnResize(e);Invalidate();}
 protected override void OnPaintBackground(PaintEventArgs e){}
 protected override void OnMouseWheel(MouseEventArgs e){if(selected<0&&e.Delta!=0)TurnPage(e.Delta<0?1:-1);base.OnMouseWheel(e);}
 protected override void OnKeyDown(KeyEventArgs e){
  if(e.KeyCode==Keys.Escape&&selected>=0){CloseEnlarged();e.Handled=true;return;}
  if(e.KeyCode==Keys.Right||e.KeyCode==Keys.Down||e.KeyCode==Keys.PageDown)TurnPage(1);
  else if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Up||e.KeyCode==Keys.PageUp)TurnPage(-1);
  else {base.OnKeyDown(e);return;}e.Handled=true;
 }
 void CloseEnlarged(){selected=-1;if(enlarged!=null){enlarged.Dispose();enlarged=null;}Invalidate();}
 protected override void OnMouseDown(MouseEventArgs e){
  Focus();if(e.Button!=MouseButtons.Left){base.OnMouseDown(e);return;}
  if(selected>=0){CloseEnlarged();return;}
  if(BackBounds.Contains(e.Location)){if(ButtonClickSound!=null)ButtonClickSound();if(ReturnToLibrary!=null)ReturnToLibrary();return;}
  if(IsTurning)return;
  if(PreviousBounds.Contains(e.Location)){TurnPage(-1);return;}if(NextBounds.Contains(e.Location)){TurnPage(1);return;}
  for(int slot=0;slot<8;slot++){int index=pageIndex*8+slot;if(index<cards.Count&&GetCardBounds(slot).Contains(e.Location)){selected=index;enlarged=RenderCard(definitions[index],512,768);tips.SetToolTip(this,null);Invalidate();break;}}
  base.OnMouseDown(e);
 }
 protected override void OnMouseMove(MouseEventArgs e){
  base.OnMouseMove(e);int next=-1;string button="";
  if(selected<0&&!IsTurning){for(int slot=0;slot<8;slot++)if(pageIndex*8+slot<cards.Count&&GetCardBounds(slot).Contains(e.Location)){next=slot;break;}
   if(BackBounds.Contains(e.Location))button="back";else if(PreviousBounds.Contains(e.Location)&&pageIndex>0)button="previous";else if(NextBounds.Contains(e.Location)&&pageIndex<PageCount-1)button="next";
  }
  if(button!=hoverButton&&button!=""&&ButtonHoverSound!=null)ButtonHoverSound();
  if(next!=hover||button!=hoverButton){hover=next;hoverButton=button;Cursor=next>=0||button!=""?Cursors.Hand:Cursors.Default;tips.SetToolTip(this,next<0?null:definitions[pageIndex*8+next].name+" · "+definitions[pageIndex*8+next].text);Invalidate();}
 }
 protected override void OnMouseLeave(EventArgs e){hover=-1;hoverButton="";Invalidate();base.OnMouseLeave(e);}
 void DrawBackdrop(Graphics graphics){
  if(backdrop==null||backdropSize!=ClientSize||backdropArt!=SceneArt){if(backdrop!=null)backdrop.Dispose();backdrop=new Bitmap(Math.Max(1,Width),Math.Max(1,Height));using(var g=Graphics.FromImage(backdrop))ExpeditionVisuals.Background(g,SceneArt,ClientRectangle,12,true);backdropSize=ClientSize;backdropArt=SceneArt;}
  graphics.DrawImageUnscaled(backdrop,0,0);
 }
 void DrawPaper(Graphics g,Rectangle r,int spread){
  g.SmoothingMode=SmoothingMode.None;int middle=r.Left+r.Width/2;
  // Stepped bowed page edges, fanned paper layers and a recessed binding make
  // the carrier read as an open volume rather than a rectangular UI panel.
  int half=r.Width/2,bow=Math.Max(5,r.Height/32);
  var leftLeaf=new[]{new Point(r.Left,r.Top+bow),new Point(r.Left+half/3,r.Top+2),new Point(middle-30,r.Top+bow),new Point(middle-5,r.Top+bow*2),new Point(middle-5,r.Bottom-bow),new Point(middle-30,r.Bottom-bow*2),new Point(r.Left+half/3,r.Bottom-bow),new Point(r.Left,r.Bottom)};
  var rightLeaf=new[]{new Point(middle+5,r.Top+bow*2),new Point(middle+30,r.Top+bow),new Point(r.Right-half/3,r.Top+2),new Point(r.Right,r.Top+bow),new Point(r.Right,r.Bottom),new Point(r.Right-half/3,r.Bottom-bow),new Point(middle+30,r.Bottom-bow*2),new Point(middle+5,r.Bottom-bow)};
  using(var binding=new SolidBrush(Color.FromArgb(122,85,45)))g.FillRectangle(binding,middle-8,r.Top+bow,16,r.Height-bow);
  for(int layer=3;layer>=0;layer--){
   var left=leftLeaf.Select(p=>new Point(p.X,p.Y+layer*2)).ToArray();var right=rightLeaf.Select(p=>new Point(p.X,p.Y+layer*2)).ToArray();
   using(var fill=new SolidBrush(layer==0?Color.FromArgb(250,239,205):Color.FromArgb(218-layer*5,192-layer*5,142-layer*5))){g.FillPolygon(fill,left);g.FillPolygon(fill,right);}
   using(var edge=new Pen(Color.FromArgb(175,138,79),1)){g.DrawPolygon(edge,left);g.DrawPolygon(edge,right);}
  }
  using(var crease=new SolidBrush(Color.FromArgb(224,199,149))){g.FillRectangle(crease,middle-20,r.Top+bow*2,14,r.Height-bow*3);g.FillRectangle(crease,middle+6,r.Top+bow*2,14,r.Height-bow*3);}
  using(var edge=new Pen(Color.FromArgb(196,163,105),1)){
   g.DrawLines(edge,new[]{new Point(r.Left+8,r.Top+bow+8),new Point(r.Left+half/3,r.Top+11),new Point(middle-30,r.Top+bow+8)});
   g.DrawLines(edge,new[]{new Point(middle+30,r.Top+bow+8),new Point(r.Right-half/3,r.Top+11),new Point(r.Right-8,r.Top+bow+8)});
  }
  using(var ribbon=new SolidBrush(Color.FromArgb(34,101,110)))g.FillPolygon(ribbon,new[]{new Point(r.Right-46,r.Top+6),new Point(r.Right-30,r.Top+6),new Point(r.Right-30,r.Top+36),new Point(r.Right-38,r.Top+30),new Point(r.Right-46,r.Top+36)});
  using(var ink=new SolidBrush(Color.FromArgb(112,78,40)))using(var font=GameTheme.Body(Math.Max(8,r.Height/48f))){
   var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center};
   GameTheme.DrawPixelString(g,"卡牌图鉴",font,ink,new RectangleF(r.Left+20,r.Top+5,r.Width/2-40,22),format);
   GameTheme.DrawPixelString(g,"已解锁 "+ownedCards.Count+" / "+cards.Count,font,ink,new RectangleF(middle+20,r.Top+5,r.Width/2-40,22),format);
   GameTheme.DrawPixelString(g,(spread*2+1).ToString(),font,ink,new RectangleF(r.Left,r.Bottom-23,r.Width/2,18),format);
   GameTheme.DrawPixelString(g,(spread*2+2).ToString(),font,ink,new RectangleF(middle,r.Bottom-23,r.Width/2,18),format);
  }
 }
 void DrawSpread(Graphics g,int spread,bool feedback){
  DrawPaper(g,PaperBounds,spread);g.InterpolationMode=InterpolationMode.HighQualityBicubic;
  for(int slot=0;slot<8;slot++){int index=spread*8+slot;if(index>=cards.Count)continue;var box=GetCardBounds(slot);
   if(feedback&&hover==slot){int grow=Math.Max(2,box.Width/45);box.Inflate(grow,grow*3/2);}
   g.DrawImage(cards[index],box);
  }
 }
 Bitmap RenderSpread(int spread){var image=new Bitmap(Math.Max(1,Width),Math.Max(1,Height));using(var g=Graphics.FromImage(image)){g.Clear(Color.Transparent);DrawSpread(g,spread,false);}return image;}
 void DrawTurningSpread(Graphics g){
  double progress=Math.Min(1,turnClock.Elapsed.TotalMilliseconds/380);var paper=PaperBounds;int half=paper.Width/2,middle=paper.Left+half;
  var left=new Rectangle(paper.Left,paper.Top,half,paper.Height);var right=new Rectangle(middle,paper.Top,paper.Right-middle,paper.Height);
  if(progress<.5){g.DrawImageUnscaled(oldSpread,0,0);var under=direction>0?right:left;g.DrawImage(newSpread,under,under,GraphicsUnit.Pixel);}
  else g.DrawImageUnscaled(newSpread,0,0);
  var source=progress<.5?(direction>0?right:left):(direction>0?left:right);
  float width=(float)(half*Math.Abs(Math.Cos(progress*Math.PI)));bool toRight=progress<.5?direction>0:direction<0;
  float edge=middle+(toRight?width:-width),curl=(float)(Math.Sin(progress*Math.PI)*Math.Min(15,paper.Height*.035));
  var image=progress<.5?oldSpread:newSpread;
  if(width>1){var points=toRight?new[]{new PointF(middle,paper.Top),new PointF(edge,paper.Top+curl),new PointF(middle,paper.Bottom)}:new[]{new PointF(edge,paper.Top+curl),new PointF(middle,paper.Top),new PointF(edge,paper.Bottom-curl)};
   g.DrawImage(image,points,source,GraphicsUnit.Pixel);
   using(var shade=new SolidBrush(Color.FromArgb((int)(Math.Sin(progress*Math.PI)*65),97,69,34)))g.FillPolygon(shade,new[]{points[0],points[1],new PointF(toRight?edge:middle,toRight?paper.Bottom-curl:paper.Bottom),points[2]});
  }
 }
 void DrawButton(Graphics g,Rectangle bounds,string text,string key,bool enabled,bool shrink=false){
  bool over=hoverButton==key;if(shrink&&!over){int w=(int)(bounds.Width*.8),h=(int)(bounds.Height*.8);bounds=new Rectangle(bounds.Left+(bounds.Width-w)/2,bounds.Top+(bounds.Height-h)/2,w,h);}
  GuildChrome.Draw(g,bounds,over,enabled);using(var ink=new SolidBrush(enabled?GuildChrome.Ivory:GuildChrome.Muted))using(var font=GameTheme.Body(shrink&&!over?10:11))using(var sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})GameTheme.DrawPixelString(g,text,font,ink,bounds,sf);
 }
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;DrawBackdrop(g);var book=BookBounds;int edge=Math.Max(8,book.Width/70);
  using(var shadow=new SolidBrush(Color.FromArgb(90,17,19,24)))g.FillRectangle(shadow,new Rectangle(book.Left+8,book.Top+10,book.Width,book.Height));
  using(var cover=new SolidBrush(Color.FromArgb(75,53,37)))g.FillPolygon(cover,GameTheme.Outline(book,edge));
  using(var gold=new Pen(Color.FromArgb(187,141,72),2))g.DrawPolygon(gold,GameTheme.Outline(Rectangle.Inflate(book,-4,-4),Math.Max(3,edge-2)));
  using(var pages=new Pen(Color.FromArgb(194,165,109),1))for(int i=0;i<3;i++)g.DrawLine(pages,book.Left+edge,book.Bottom-4-i*3,book.Right-edge,book.Bottom-4-i*3);
  if(IsTurning)DrawTurningSpread(g);else DrawSpread(g,pageIndex,true);
  var title=new Rectangle((Width-260)/2,Math.Max(16,(int)(Height*.10)),260,60);GuildChrome.Draw(g,title);using(var ink=new SolidBrush(GuildChrome.Ivory))using(var sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})GameTheme.DrawPixelString(g,"卡牌图鉴",titleFont,ink,title,sf);
  GameTheme.DrawText(g,"点击卡牌放大 · 滚轮或左右方向键翻页",Font,new Rectangle(book.Left,book.Top-30,book.Width,24),GuildChrome.Ivory,TextFormatFlags.HorizontalCenter);
  DrawButton(g,BackBounds,ReturnText,"back",true,true);DrawButton(g,PreviousBounds,"上一页","previous",pageIndex>0&&!IsTurning);DrawButton(g,NextBounds,"下一页","next",pageIndex<PageCount-1&&!IsTurning);
  GameTheme.DrawText(g,"第 "+(pageIndex+1)+" / "+PageCount+" 组",Font,new Rectangle(book.Left+125,book.Bottom+18,book.Width-250,24),GuildChrome.Ivory,TextFormatFlags.HorizontalCenter);
  if(enlarged!=null){using(var shade=new SolidBrush(Color.FromArgb(185,8,15,20)))g.FillRectangle(shade,ClientRectangle);int h=Math.Max(1,Math.Min(768,Math.Min(Height-80,(Width-60)*3/2))),w=h*2/3;g.DrawImage(enlarged,new Rectangle((Width-w)/2,(Height-h)/2-10,w,h));GameTheme.DrawText(g,"再次点击或按 Esc 收起",Font,new Rectangle(0,(Height+h)/2+8,Width,28),GuildChrome.Ivory,TextFormatFlags.HorizontalCenter);}
 }
 protected override void Dispose(bool disposing){if(disposing){FinishTurn();turnTimer.Dispose();if(backdrop!=null)backdrop.Dispose();if(enlarged!=null)enlarged.Dispose();foreach(var image in cards)image.Dispose();tips.Dispose();titleFont.Dispose();}base.Dispose(disposing);}
}