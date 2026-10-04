using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

public sealed class SystemShopCanvas:Control {
 readonly Image art;readonly RogueProfile profile;readonly string[] items;
 readonly Image grayArt;
 Rectangle scene;int hovered=-1;string feedback="点击商品购买 · 已购买的装备不会重复扣费";
 public Action<bool> PurchaseSound;public Func<string,bool> Purchase;public Action Return,Home;
 public SystemShopCanvas(Image image,RogueProfile state,string[] catalog){art=image;profile=state;items=catalog;var gray=new Bitmap(image.Width,image.Height);using(var g=Graphics.FromImage(gray))using(var attr=new ImageAttributes()){attr.SetColorMatrix(new ColorMatrix(new float[][]{new[]{.21f,.21f,.21f,0,0},new[]{.42f,.42f,.42f,0,0},new[]{.07f,.07f,.07f,0,0},new[]{0f,0f,0f,1f,0f},new[]{0f,0f,0f,0f,1f}}));g.DrawImage(image,new Rectangle(0,0,gray.Width,gray.Height),0,0,image.Width,image.Height,GraphicsUnit.Pixel,attr);}grayArt=gray;DoubleBuffered=true;ResizeRedraw=true;TabStop=true;BackColor=GameTheme.Navy;AccessibleName="第一章系统商城";}
 protected override void Dispose(bool disposing){if(disposing)grayArt.Dispose();base.Dispose(disposing);}
 string ItemPrompt(int i){string id=items[i];if(Owned(id))return "已购买 · 无需重复购买";int cost=StoryShopEngine.Price(id);return profile.coins<cost?"金币不足 · 还差 "+(cost-profile.coins)+" 金币":"点击购买 · 花费 "+cost+" 金币";}
 void LayoutScene(){float scale=Math.Min(Width/(float)art.Width,Height/(float)art.Height);int w=(int)(art.Width*scale),h=(int)(art.Height*scale);scene=new Rectangle((Width-w)/2,(Height-h)/2,w,h);}
 Rectangle Area(float x,float y,float w,float h){return new Rectangle(scene.X+(int)(x*scene.Width),scene.Y+(int)(y*scene.Height),(int)(w*scene.Width),(int)(h*scene.Height));}
 public Rectangle ItemBounds(int i){LayoutScene();return Area(.118f+(i%4)*.195f,i<4?.224f:.46f,.182f,.232f);}
 bool Owned(string id){return profile.storyItems.Contains(id)||profile.storyUsed.Contains(id);}
 int Hit(Point p){for(int i=0;i<items.Length;i++)if(ItemBounds(i).Contains(p))return i;return -1;}
 void TextOn(Graphics g,string value,Rectangle r,float size,Color color,bool center=true){using(var font=GameTheme.Body(size,FontStyle.Bold)){var flags=TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak|TextFormatFlags.NoPadding|(center?TextFormatFlags.HorizontalCenter:TextFormatFlags.Left);var shadow=r;shadow.Offset(1,1);TextRenderer.DrawText(g,value,font,shadow,Color.Black,flags);TextRenderer.DrawText(g,value,font,r,color,flags);}}
 void ItemName(Graphics g,string value,Rectangle plaque,Color color){
  var r=Rectangle.Inflate(plaque,-Math.Max(3,plaque.Width/30),-Math.Max(2,plaque.Height/10));if(r.Width<1||r.Height<1)return;
  var flags=TextFormatFlags.SingleLine|TextFormatFlags.NoPadding|TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter;
  float size=Math.Max(5,plaque.Height*.65f);Font font=null;
  try{while(size>=4){if(font!=null)font.Dispose();font=GameTheme.Body(size,FontStyle.Bold);var measured=TextRenderer.MeasureText(g,value,font,new Size(Int32.MaxValue,Int32.MaxValue),TextFormatFlags.SingleLine|TextFormatFlags.NoPadding);if(measured.Width<=r.Width&&measured.Height<=r.Height)break;size-=.25f;}g.SetClip(plaque,CombineMode.Intersect);TextRenderer.DrawText(g,value,font,r,color,flags);g.ResetClip();}finally{if(font!=null)font.Dispose();}
 }
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);LayoutScene();var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.DrawImage(art,scene);float scale=scene.Width/1280f;
  for(int i=0;i<items.Length;i++){string id=items[i];bool owned=Owned(id);var box=ItemBounds(i);if(owned){var source=new Rectangle((int)((box.X-scene.X)*(double)art.Width/scene.Width),(int)((box.Y-scene.Y)*(double)art.Height/scene.Height),(int)(box.Width*(double)art.Width/scene.Width),(int)(box.Height*(double)art.Height/scene.Height));g.DrawImage(grayArt,box,source,GraphicsUnit.Pixel);TextOn(g,"已购买",Area(.15f+(i%4)*.195f,i<4?.247f:.483f,.12f,.043f),Math.Max(9,14*scale),GameTheme.Ink);}if(i==hovered)using(var pen=new Pen(owned?GameTheme.Gold:GameTheme.Cyan,2))g.DrawRectangle(pen,Rectangle.Inflate(box,-2,-2));var plaque=Area(.158f+(i%4)*.195f,i<4?.407f:.631f,.105f,.042f);ItemName(g,StoryShopEngine.Name(id),plaque,owned?GameTheme.Muted:GameTheme.Gold);}
  TextOn(g,"金币  "+profile.coins,Area(.817f,.03f,.166f,.12f),Math.Max(12,20*scale),GameTheme.Gold);
  string title=hovered>=0?StoryShopEngine.Name(items[hovered])+" · "+(profile.storyUsed.Contains(items[hovered])?"已激活":Owned(items[hovered])?"已持有":StoryShopEngine.Price(items[hovered])+" 金币"):"第一章 · 系统支援装备";
  TextOn(g,title,Area(.27f,.715f,.46f,.045f),Math.Max(10,16*scale),GameTheme.Gold);
  string description=hovered>=0?StoryShopEngine.Description(items[hovered]):feedback;
  TextOn(g,description,Area(.27f,.768f,.46f,.075f),Math.Max(9,12*scale),GameTheme.Ink);
  if(hovered>=0)TextOn(g,feedback,Area(.26f,.851f,.48f,.035f),Math.Max(8,10*scale),GameTheme.Cyan);
  GameTheme.Button(g,Area(.03f,.905f,.15f,.065f),"返回地图",Font,false,false,true);GameTheme.Button(g,Area(.82f,.905f,.15f,.065f),"返回主界面",Font,false,false,true);
 }
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);int next=Hit(e.Location);if(next!=hovered){hovered=next;feedback=next>=0?ItemPrompt(next):"点击商品购买 · 已购买的装备不会重复扣费";Invalidate();}Cursor=next>=0||Area(.03f,.905f,.15f,.065f).Contains(e.Location)||Area(.82f,.905f,.15f,.065f).Contains(e.Location)?Cursors.Hand:Cursors.Default;}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hovered=-1;Invalidate();}
 protected override void OnMouseClick(MouseEventArgs e){base.OnMouseClick(e);if(e.Button!=MouseButtons.Left)return;LayoutScene();if(Area(.03f,.905f,.15f,.065f).Contains(e.Location)){if(Return!=null)Return();return;}if(Area(.82f,.905f,.15f,.065f).Contains(e.Location)){if(Home!=null)Home();return;}int index=Hit(e.Location);if(index<0)return;string id=items[index];bool success=false;if(Owned(id))feedback="已拥有这件装备，无需重复购买。";else if(profile.coins<StoryShopEngine.Price(id))feedback="金币不足 · 还差 "+(StoryShopEngine.Price(id)-profile.coins)+" 金币";else if(Purchase!=null&&Purchase(id)){success=true;feedback="已购买 "+StoryShopEngine.Name(id)+" · 装备已加入背包";}else feedback="购买未完成，请稍后重试。";if(PurchaseSound!=null)PurchaseSound(success);Invalidate();}
}
