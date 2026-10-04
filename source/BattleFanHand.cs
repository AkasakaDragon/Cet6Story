using System;using System.Diagnostics;using System.Drawing;using System.Drawing.Drawing2D;using System.Drawing.Imaging;using System.Collections.Generic;using System.Windows.Forms;
// One animated surface keeps card geometry, overlap and hit testing in sync.
public sealed class BattleFanHand:Control {
 readonly List<Bitmap> cards=new List<Bitmap>();readonly List<bool> owned=new List<bool>();readonly List<bool> playable=new List<bool>();readonly List<string> descriptions=new List<string>();
 readonly Dictionary<string,Bitmap> renderCards=new Dictionary<string,Bitmap>();Bitmap scene;int renderHeight;Control sceneParent;Point sceneLocation;
 readonly Stopwatch commitTime=new Stopwatch();readonly Timer animation=new Timer{Interval=16};float expansion,targetExpansion;int dragged=-1,commitFrame;bool committing,pressed;Point pressPoint,pressOrigin,pointer;PointF dragCenter;
 public int CardHeight=250;public int HoveredIndex{get;private set;}public int CardCount{get{return cards.Count;}}
 public Func<int,int,int,Bitmap> CachedCard;public Action<int> PlayCard;public Func<int,bool> TryPlayCard;public Action CardPlayed,HoverSound,PlaySound;
 public bool Expanded{get{return targetExpansion>0;}}public bool Dragging{get{return dragged>=0&&pressed;}}
 public BattleFanHand(){HoveredIndex=-1;SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);BackColor=Color.Transparent;TabStop=true;AccessibleRole=AccessibleRole.List;ResizeRedraw=true;animation.Tick+=(s,e)=>Animate();}
 public void AddCard(Bitmap art,string description,bool ownsImage=true,bool canPlay=true){if(cards.Count>=8)throw new InvalidOperationException("手牌上限为 8 张。");cards.Add(art);owned.Add(ownsImage);playable.Add(canPlay);descriptions.Add(description);Invalidate();}
 void ClearRenderCards(){foreach(var image in renderCards.Values)image.Dispose();renderCards.Clear();}
 Bitmap RenderCard(int index,int width,int height){if(CachedCard!=null)return CachedCard(index,width,height);if(renderHeight!=CardHeight){ClearRenderCards();renderHeight=CardHeight;}string key=index+":"+width+":"+height;Bitmap result;if(renderCards.TryGetValue(key,out result))return result;result=new Bitmap(Math.Max(1,width),Math.Max(1,height),PixelFormat.Format32bppPArgb);using(var g=Graphics.FromImage(result)){g.InterpolationMode=InterpolationMode.HighQualityBicubic;g.PixelOffsetMode=PixelOffsetMode.HighQuality;g.DrawImage(cards[index],new Rectangle(0,0,width,height));}renderCards[key]=result;return result;}
 protected override void OnPaintBackground(PaintEventArgs e){
  // Combat poses are static while choosing cards; cache just the parent scene,
  // excluding all child UI. Transparent WinForms repaint otherwise redraws it
  // for every hover, mouse move and animation tick.
  Size sceneSize=Parent==null?Size:Parent.ClientSize;
  if(scene==null||scene.Size!=sceneSize||sceneParent!=Parent||sceneLocation!=Location){if(scene!=null)scene.Dispose();scene=new Bitmap(Math.Max(1,sceneSize.Width),Math.Max(1,sceneSize.Height),PixelFormat.Format32bppPArgb);sceneParent=Parent;sceneLocation=Location;using(var g=Graphics.FromImage(scene)){if(Parent!=null){using(var args=new PaintEventArgs(g,new Rectangle(Point.Empty,sceneSize))){InvokePaintBackground(Parent,args);InvokePaint(Parent,args);}}else g.Clear(GameTheme.Navy);}}
  e.Graphics.DrawImageUnscaled(scene,Parent==null?0:-Left,Parent==null?0:-Top);
 }
 void Expand(bool value){targetExpansion=value?1:0;animation.Start();}
 void Animate(){expansion+=(targetExpansion-expansion)*.24f;if(Math.Abs(expansion-targetExpansion)<.005f)expansion=targetExpansion;if(committing){int next=Math.Min(14,(int)(commitTime.Elapsed.TotalMilliseconds/16));dragCenter.Y-=9*(next-commitFrame);commitFrame=next;KeepDraggedCardVisible();if(commitFrame>=14){animation.Stop();committing=false;dragged=-1;HoveredIndex=-1;Invalidate();if(CardPlayed!=null)CardPlayed();return;}}Invalidate();if(!committing&&!pressed&&expansion==targetExpansion)animation.Stop();}
 // The hand surface spans the arena so lifted cards aren't cut at its old top edge.
 void KeepDraggedCardVisible(){float h=Math.Min(Height-12,CardHeight*1.32f),w=h*2/3;dragCenter.X=Math.Max(w/2+4,Math.Min(Width-w/2-4,dragCenter.X));dragCenter.Y=Math.Max(h/2+6,Math.Min(Height-h/2-6,dragCenter.Y));}
 Matrix CardTransform(int index,bool raised,out float w,out float h){
  h=raised?Math.Min(Height-12,CardHeight*1.32f):CardHeight;w=h*2/3;
  if(index==dragged){var drag=new Matrix();drag.Translate(dragCenter.X-w/2,dragCenter.Y-h/2);return drag;}
  float normalWidth=CardHeight*2/3f,margin=CardHeight*.22f;
  float step=cards.Count<=1?0:Math.Max(0,Math.Min(normalWidth*.68f,(Width-normalWidth-margin*2)/Math.Max(1,cards.Count-1)));
  float offset=index-(cards.Count-1)/2f,unit=cards.Count<=1?0:offset/Math.Max(1,(cards.Count-1)/2f),cx=Width/2f+offset*step;
  float top=(raised?Height-h-6:Height-CardHeight*.87f+unit*unit*Math.Min(30,CardHeight*.1f))+(1-expansion)*CardHeight*.34f;
  if(raised)cx=Math.Max(w/2+4,Math.Min(Width-w/2-4,cx));var matrix=new Matrix();matrix.Translate(cx,top+h/2);matrix.Rotate(raised?0:unit*15);matrix.Translate(-w/2,-h/2);return matrix;
 }
 public RectangleF CardBounds(int index,bool raised){float w,h;using(var transform=CardTransform(index,raised,out w,out h)){var corners=new[]{new PointF(0,0),new PointF(w,0),new PointF(w,h),new PointF(0,h)};transform.TransformPoints(corners);float left=corners[0].X,right=left,top=corners[0].Y,bottom=top;foreach(var p in corners){left=Math.Min(left,p.X);right=Math.Max(right,p.X);top=Math.Min(top,p.Y);bottom=Math.Max(bottom,p.Y);}return RectangleF.FromLTRB(left,top,right,bottom);}}
 bool ContainsCard(int index,Point point,bool raised){float w,h;using(var transform=CardTransform(index,raised,out w,out h)){transform.Invert();var points=new[]{new PointF(point.X,point.Y)};transform.TransformPoints(points);var p=points[0];if(p.X<0||p.Y<0||p.X>=w||p.Y>=h)return false;return cards[index].GetPixel(Math.Min(cards[index].Width-1,(int)(p.X/w*cards[index].Width)),Math.Min(cards[index].Height-1,(int)(p.Y/h*cards[index].Height))).A>32;}}
 public int HitCard(Point point){int nearest=-1;float distance=float.MaxValue;for(int i=0;i<cards.Count;i++)if(ContainsCard(i,point,false)){var r=CardBounds(i,false);float d=Math.Abs(point.X-r.Left-r.Width/2);if(d<distance){distance=d;nearest=i;}}if(nearest>=0)return nearest;if(HoveredIndex>=0&&ContainsCard(HoveredIndex,point,true))return HoveredIndex;return -1;}
 void Hover(int index){if(HoveredIndex==index)return;HoveredIndex=index;Cursor=index>=0?Cursors.Hand:Cursors.Default;AccessibleName=index>=0?descriptions[index]:"手牌 · "+cards.Count+" 张";if(index>=0&&HoverSound!=null)HoverSound();Invalidate();}
 bool InHandArea(Point point){return ClientRectangle.Contains(point)&&(point.Y>=Height-CardHeight*1.02f||HitCard(point)>=0);}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(committing)return;pointer=e.Location;if(pressed&&dragged>=0){dragCenter.X+=pointer.X-pressPoint.X;dragCenter.Y+=pointer.Y-pressPoint.Y;KeepDraggedCardVisible();pressPoint=pointer;Expand(true);Invalidate();return;}Expand(InHandArea(e.Location));Hover(HitCard(e.Location));}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);if(pressed||committing)return;Expand(false);Hover(-1);}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left||committing)return;int index=HitCard(e.Location);if(index<0)return;Focus();Hover(index);Expand(true);var bounds=CardBounds(index,true);dragCenter=new PointF(bounds.Left+bounds.Width/2,bounds.Top+bounds.Height/2);dragged=index;pressed=true;pressOrigin=pressPoint=pointer=e.Location;Capture=true;animation.Start();}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button!=MouseButtons.Left||!pressed)return;pressed=false;Capture=false;int index=dragged;
  dragCenter.X+=e.X-pressPoint.X;dragCenter.Y+=e.Y-pressPoint.Y;KeepDraggedCardVisible();
  bool dragRelease=pressOrigin.Y-e.Y>=Math.Max(55,CardHeight*.22f)&&e.Y<Height-CardHeight*.65f&&e.X>=0&&e.X<Width;
  bool click=Math.Abs(e.Y-pressOrigin.Y)<9&&Math.Abs(e.X-pressOrigin.X)<9;
  if((dragRelease||click)&&Commit(index))return;
  dragged=-1;Hover(ClientRectangle.Contains(e.Location)?HitCard(e.Location):-1);Expand(InHandArea(e.Location));Invalidate();
 }
 protected override void OnMouseCaptureChanged(EventArgs e){base.OnMouseCaptureChanged(e);if(!Capture&&pressed){pressed=false;dragged=-1;Expand(false);Hover(-1);}}
 bool Commit(int index){if(index<0||index>=cards.Count||!playable[index])return false;bool accepted=TryPlayCard!=null?TryPlayCard(index):PlayCard!=null;if(!accepted)return false;
  if(TryPlayCard==null){if(PlaySound!=null)PlaySound();PlayCard(index);return true;}
  dragged=index;committing=true;commitFrame=0;commitTime.Restart();if(PlaySound!=null)PlaySound();animation.Start();Invalidate();return true;
 }
 protected override bool IsInputKey(Keys keyData){return keyData==Keys.Left||keyData==Keys.Right||base.IsInputKey(keyData);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(cards.Count==0||committing)return;if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Right){Expand(true);Hover(HoveredIndex<0?0:(HoveredIndex+(e.KeyCode==Keys.Right?1:cards.Count-1))%cards.Count);e.Handled=true;}else if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){if(HoveredIndex>=0){var bounds=CardBounds(HoveredIndex,true);dragCenter=new PointF(bounds.Left+bounds.Width/2,bounds.Top+bounds.Height/2);Commit(HoveredIndex);}e.Handled=true;}}
 protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);if(!pressed&&!committing){Expand(false);Hover(-1);}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.InterpolationMode=InterpolationMode.Bilinear;g.PixelOffsetMode=PixelOffsetMode.HighQuality;g.SmoothingMode=SmoothingMode.AntiAlias;
  for(int i=0;i<cards.Count;i++)if(i!=HoveredIndex&&i!=dragged)Draw(g,i,false);if(HoveredIndex>=0&&HoveredIndex!=dragged)Draw(g,HoveredIndex,true);
  if(dragged>=0){if(pressed)DrawReleaseGuide(g);Draw(g,dragged,true);DrawTrail(g);}
 }
 void DrawReleaseGuide(Graphics g){float y=Height-CardHeight*.65f;using(var pen=new Pen(Color.FromArgb(playable[dragged]?160:70,CyberChrome.Neon),1.4f)){pen.DashStyle=DashStyle.Dash;g.DrawLine(pen,Math.Max(15,Width/2-170),y,Math.Min(Width-15,Width/2+170),y);}
  using(var font=GameTheme.Body(10))GameTheme.DrawText(g,playable[dragged]?"向上拖动 · 松开出牌":"当前无法使用这张牌",font,new Rectangle(0,8,Width,24),playable[dragged]?GameTheme.Cyan:GameTheme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
 }
 void DrawTrail(Graphics g){float w=CardHeight*.7f;int count=committing?14:7;for(int i=0;i<count;i++){float x=dragCenter.X+(i%2==0?-1:1)*(w*.36f+i%3*4),y=dragCenter.Y+CardHeight*.35f+i*7;int alpha=committing?Math.Max(0,180-commitFrame*10-i*7):Math.Max(0,130-i*15);using(var pen=new Pen(Color.FromArgb(alpha,CyberChrome.Neon),i%3==0?2:1))g.DrawLine(pen,x,y,x,y+8);}
  if(committing){float radius=18+commitFrame*7;using(var pen=new Pen(Color.FromArgb(Math.Max(0,170-commitFrame*11),CyberChrome.Neon),2))g.DrawEllipse(pen,dragCenter.X-radius,dragCenter.Y-radius*.6f,radius*2,radius*1.2f);}
 }
 void Draw(Graphics g,int index,bool raised){float w,h;var state=g.Save();using(var transform=CardTransform(index,raised,out w,out h)){g.MultiplyTransform(transform);using(var shadow=new SolidBrush(Color.FromArgb(raised?85:45,0,0,0)))g.FillEllipse(shadow,w*.10f,h*.85f,w*.8f,h*.11f);
  var art=RenderCard(index,(int)w,(int)h);
  if(committing&&index==dragged)using(var attributes=new ImageAttributes()){var matrix=new ColorMatrix();matrix.Matrix33=Math.Max(0,1-commitFrame/14f);attributes.SetColorMatrix(matrix);g.DrawImage(art,new Rectangle(0,0,art.Width,art.Height),0,0,art.Width,art.Height,GraphicsUnit.Pixel,attributes);}
  else g.DrawImage(art,new Rectangle(0,0,art.Width,art.Height));}g.Restore(state);
 }
 protected override void Dispose(bool disposing){if(disposing){animation.Dispose();ClearRenderCards();if(scene!=null)scene.Dispose();for(int i=0;i<cards.Count;i++)if(owned[i])cards[i].Dispose();}base.Dispose(disposing);}
}
