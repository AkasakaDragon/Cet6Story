using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

public partial class Game {
 Image RelicArt(string id){
  string key="relic-icon:"+id;Image cached;if(imageCache.TryGetValue(key,out cached))return cached;
  string[] order={"blade","shield","heart","regen","oracle","focus","fortune","ward","leech"};int i=Array.IndexOf(order,id);if(i<0)i=8;
  using(var atlas=new Bitmap(TowerArt("relic-icons"))){int cw=atlas.Width/3,ch=atlas.Height/3,sx=i%3*cw,sy=i/3*ch,left=sx+cw,top=sy+ch,right=sx,bottom=sy;
   for(int y=sy;y<sy+ch;y++)for(int x=sx;x<sx+cw;x++)if(atlas.GetPixel(x,y).A>32){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}
   cached=atlas.Clone(left>right?new Rectangle(sx,sy,cw,ch):Rectangle.FromLTRB(Math.Max(sx,left-2),Math.Max(sy,top-2),Math.Min(sx+cw,right+3),Math.Min(sy+ch,bottom+3)),PixelFormat.Format32bppArgb);
  }imageCache[key]=cached;return cached;
 }
 void AddRelicHud(RogueRun run,Control map=null){
  if(run==null||run.relics==null||run.relics.Count==0)return;
  var strip=new FlowLayoutPanel{BackColor=Color.Transparent,FlowDirection=FlowDirection.LeftToRight,WrapContents=true,Margin=new Padding(0),Padding=new Padding(0),AccessibleName="本局赋能"};
  foreach(var group in run.relics.GroupBy(x=>x)){var icon=new RelicHudIcon{Art=RelicArt(group.Key),Count=group.Count(),AccessibleName=RogueEngine.RelicName(group.Key)+" ×"+group.Count()};tips.SetToolTip(icon,RogueEngine.RelicName(group.Key)+" ×"+group.Count()+"\n"+RogueEngine.RelicDescription(group.Key)+"\n本局有效，重复获得可叠加");strip.Controls.Add(icon);}
  if(map!=null){map.Controls.Add(strip);Action place=()=>{int cols=Math.Max(1,Math.Min(strip.Controls.Count,Math.Min(5,(map.ClientSize.Width-130)/54)));strip.Size=new Size(cols*54,((strip.Controls.Count+cols-1)/cols)*54);strip.Location=new Point(map.ClientSize.Width-strip.Width-16,12);};EventHandler resized=(s,e)=>place();map.Resize+=resized;strip.Disposed+=(s,e)=>map.Resize-=resized;place();strip.BringToFront();return;}
  var title=rogueBody.Controls[0];rogueBody.Controls.Remove(title);var bar=new Panel{BackColor=Color.Transparent,Margin=new Padding(0),Width=Math.Max(650,rogueBody.ClientSize.Width-65)};bar.Controls.Add(title);bar.Controls.Add(strip);rogueBody.Controls.Add(bar);rogueBody.Controls.SetChildIndex(bar,0);
  Action layout=()=>{bar.Width=Math.Max(650,rogueBody.ClientSize.Width-65);int cols=Math.Max(1,Math.Min(strip.Controls.Count,Math.Min(9,bar.Width/2/54)));strip.Size=new Size(cols*54,((strip.Controls.Count+cols-1)/cols)*54);bar.Height=Math.Max(60,strip.Height);strip.Location=new Point(bar.Width-strip.Width,0);title.Location=new Point(0,12);title.MaximumSize=new Size(Math.Max(180,strip.Left-16),0);bar.Height=Math.Max(bar.Height,title.Bottom+8);};rogueBody.Resize+=(s,e)=>layout();layout();
 }
}

public class RelicHudIcon:Control {
 public Image Art;public int Count=1;
 public RelicHudIcon(){SetStyle(ControlStyles.SupportsTransparentBackColor|ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);BackColor=Color.Transparent;Size=new Size(50,50);Margin=new Padding(2);AccessibleRole=AccessibleRole.Graphic;}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(Art!=null){e.Graphics.InterpolationMode=InterpolationMode.NearestNeighbor;e.Graphics.PixelOffsetMode=PixelOffsetMode.Half;float scale=Math.Min(44f/Art.Width,44f/Art.Height);int w=(int)(Art.Width*scale),h=(int)(Art.Height*scale);e.Graphics.DrawImage(Art,new Rectangle((Width-w)/2,(Height-h)/2,w,h));}if(Count>1){string text=Count.ToString();var rect=new Rectangle(20,30,30,20);TextRenderer.DrawText(e.Graphics,text,Font,new Rectangle(rect.X+1,rect.Y+1,rect.Width,rect.Height),Color.Black,TextFormatFlags.Right|TextFormatFlags.Bottom|TextFormatFlags.NoPadding);TextRenderer.DrawText(e.Graphics,text,Font,rect,Color.FromArgb(255,230,145),TextFormatFlags.Right|TextFormatFlags.Bottom|TextFormatFlags.NoPadding);}}
}


