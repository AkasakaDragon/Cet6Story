using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

public class TowerMap:Control {
 public Image Art,IconAtlas;public Rectangle[] IconSources;public RogueRun Run;public Action<string> Selected;public Action GoBack;string hover;ToolTip tip=new ToolTip();Timer timer=new Timer();int pulse;
 public TowerMap(){DoubleBuffered=true;ResizeRedraw=true;TabStop=true;BackColor=Color.FromArgb(12,32,33);timer.Interval=120;timer.Tick+=(s,e)=>{pulse++;Invalidate();};timer.Start();AccessibleName="远征分叉地图";}
 protected override void Dispose(bool d){if(d){timer.Dispose();tip.Dispose();}base.Dispose(d);}
 public Rectangle NodeBounds(TowerNode n){int gaps=Math.Max(1,TowerEngine.FloorCount(Run)-1);int size=Math.Max(28,Math.Min(64,(Height-102)/gaps-8));int x=(int)(Width*(.28+.22*n.lane+(((n.row*13+n.lane*7+(Run.seed&1023))%7)-3)*.008));int y=Height-50-(int)((Height-102)*n.row/(double)gaps);return new Rectangle(x-size/2,y-size/2,size,size);}
 public Rectangle ExitBounds(){return new Rectangle(16,14,84,38);}
 string Hit(Point p){if(Run==null)return null;return Run.nodes.Where(n=>TowerEngine.Available(Run).Contains(n)&&NodeBounds(n).Contains(p)).Select(n=>n.id).FirstOrDefault();}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);var hovered=Run.nodes.FirstOrDefault(n=>NodeBounds(n).Contains(e.Location));string id=hovered==null?null:hovered.id;if(hover!=id){hover=id;var node=Run.nodes.FirstOrDefault(n=>n.id==id);tip.SetToolTip(this,node==null?null:TowerEngine.KindName(node.kind)+" · "+TowerEngine.KindDescription(node.kind));Invalidate();}Cursor=TowerEngine.Available(Run).Any(n=>n.id==id)||ExitBounds().Contains(e.Location)?Cursors.Hand:Cursors.Default;}
 protected override void OnMouseLeave(EventArgs e){hover=null;Invalidate();base.OnMouseLeave(e);}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button!=MouseButtons.Left)return;if(ExitBounds().Contains(e.Location)){if(GoBack!=null)GoBack();return;}string id=Hit(e.Location);if(id!=null&&Selected!=null)Selected(id);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);var available=TowerEngine.Available(Run).OrderBy(n=>n.lane).ToList();if(available.Count==0)return;if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Right){int at=available.FindIndex(n=>n.id==hover);at=(at+(e.KeyCode==Keys.Left?-1:1)+available.Count)%available.Count;hover=available[at].id;Invalidate();e.Handled=true;}if(e.KeyCode==Keys.Enter){if(hover==null)hover=available[0].id;if(hover!=null&&Selected!=null)Selected(hover);e.Handled=true;}}
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.SmoothingMode=SmoothingMode.None;
  if(Art!=null)g.DrawImage(Art,ClientRectangle);using(var b=new SolidBrush(Color.FromArgb(85,7,25,25)))g.FillRectangle(b,ClientRectangle);if(Run==null||Run.nodes==null)return;
  var available=TowerEngine.Available(Run);
  foreach(var n in Run.nodes)foreach(string id in n.next){var to=Run.nodes.First(x=>x.id==id);var a=NodeBounds(n);var b=NodeBounds(to);bool active=n.id==Run.lastNode&&available.Contains(to),done=n.visited&&to.visited;Color c=done?Color.FromArgb(240,245,188,95):active?Color.FromArgb(255,255,220,137):Color.FromArgb(115,115,149,135);float ax=a.Left+a.Width/2,ay=a.Top+a.Height/2,bx=b.Left+b.Width/2,by=b.Top+b.Height/2;double dist=Math.Sqrt((bx-ax)*(bx-ax)+(by-ay)*(by-ay));int steps=Math.Max(1,(int)(dist/10));using(var brush=new SolidBrush(c))for(int k=1;k<steps;k++){float t=(float)k/steps;if(dist*t<a.Width*.55||dist*(1-t)<b.Width*.55)continue;g.FillRectangle(brush,(int)(ax+(bx-ax)*t)-2,(int)(ay+(by-ay)*t)-2,4,4);}}
  foreach(var n in Run.nodes){var box=NodeBounds(n);bool enabled=available.Contains(n),done=n.visited;Color color=done?Color.FromArgb(154,192,155):enabled?Color.FromArgb(255,221,137):Color.FromArgb(102,130,126);
   if(IconAtlas==null)DrawSymbol(g,n.kind,box,color);else{if(IconSources==null)IconSources=FindIconRegions(IconAtlas);int index=n.kind=="combat"?0:n.kind=="elite"?1:n.kind=="rest"?2:n.kind=="event"?3:n.kind=="chest"?4:n.kind=="shop"?5:6;var src=IconSources[index];float scale=Math.Min((float)box.Width/src.Width,(float)box.Height/src.Height);int w=(int)(src.Width*scale),h=(int)(src.Height*scale);var target=new Rectangle(box.Left+(box.Width-w)/2,box.Top+(box.Height-h)/2,w,h);using(var attr=new ImageAttributes()){var matrix=new ColorMatrix();matrix.Matrix33=enabled||n.id==hover?1f:done?.8f:.75f;attr.SetColorMatrix(matrix);g.DrawImage(IconAtlas,target,src.X,src.Y,src.Width,src.Height,GraphicsUnit.Pixel,attr);}}
   if(done){using(var pen=new Pen(Color.FromArgb(163,233,184),3)){g.DrawLine(pen,box.Right-12,box.Bottom-6,box.Right-7,box.Bottom-1);g.DrawLine(pen,box.Right-7,box.Bottom-1,box.Right+2,box.Bottom-13);}}
   if(enabled){int x=box.Left-15-(pulse%8<4?0:2),y=box.Top+box.Height/2;using(var brush=new SolidBrush(n.id==hover?Color.White:Color.FromArgb(255,225,139))){g.FillRectangle(brush,x,y-6,3,12);g.FillRectangle(brush,x+3,y-4,3,8);g.FillRectangle(brush,x+6,y-2,3,4);}}
  }
  var exit=ExitBounds();using(var b=new SolidBrush(Color.FromArgb(215,17,35,35)))g.FillRectangle(b,exit);using(var p=new Pen(Color.FromArgb(231,197,128),2))g.DrawRectangle(p,exit);TextRenderer.DrawText(g,"返回",Font,exit,Color.FromArgb(255,231,177),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
 }
 public static Rectangle[] FindIconRegions(Image atlas){
  var regions=new Rectangle[8];using(var pixels=new Bitmap(atlas)){int cw=pixels.Width/4,ch=pixels.Height/2;for(int i=0;i<8;i++){int sx=i%4*cw,sy=i/4*ch,left=sx+cw,top=sy+ch,right=sx,bottom=sy;for(int y=sy;y<sy+ch;y+=2)for(int x=sx;x<sx+cw;x+=2)if(pixels.GetPixel(x,y).A>32){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}regions[i]=left>right?new Rectangle(sx,sy,cw,ch):Rectangle.FromLTRB(Math.Max(sx,left-3),Math.Max(sy,top-3),Math.Min(sx+cw,right+4),Math.Min(sy+ch,bottom+4));}}return regions;
 }
 public static void DrawSymbol(Graphics g,string kind,Rectangle box,Color tint){
  string[] pixels=kind=="chest"?new[]{"  GGGGGGG  "," GYYYYYYYG ","GYYYYYYYYYG","GGGGGGGGGGG","GYYYGGGYYYG","GYYYGYGYYYG","GYYYGGGYYYG","GGGGGGGGGGG"}:kind=="rest"?new[]{"    Y     ","   YYY    "," Y YYYY   "," YYYYY Y  ","YYYYYYYYY "," YYYYYYY  ","  YYYYY   ","GG     GG "," GGGGGGG  "}:kind=="shop"?new[]{"    G     ","   GYG    ","  GYYY G  "," GYYYYYYG ","GGGGGGGGGG"," GY GY G  "," G  G  G  ","GGGGGGGGGG"}:kind=="event"?new[]{"  YYYYY  "," YY   YY ","      YY ","     YY  ","    YY   ","    YY   ","         ","    YY   "}:new[]{kind=="combat"?"         ":"Y       Y",kind=="combat"?"  YYYYY  ":"YY YYY YY"," YYYYYYY ","YYYYYYYYY","YYGYYY GY","YYYYYYYYY"," YYYGYYY ","  Y Y Y  ","  Y Y Y  "};
  int unit=Math.Max(2,Math.Min(box.Width/(pixels.Max(s=>s.Length)+2),box.Height/(pixels.Length+2)));int left=box.Left+(box.Width-pixels.Max(s=>s.Length)*unit)/2,top=box.Top+(box.Height-pixels.Length*unit)/2;
  for(int y=0;y<pixels.Length;y++)for(int x=0;x<pixels[y].Length;x++){char c=pixels[y][x];if(c==' ')continue;Color color=c=='G'?Color.FromArgb(90,80,47):kind=="rest"?Color.FromArgb(255,157,64):kind=="elite"||kind=="boss"?Color.FromArgb(232,122,92):tint;using(var b=new SolidBrush(color))g.FillRectangle(b,left+x*unit,top+y*unit,unit,unit);}
 }
}
