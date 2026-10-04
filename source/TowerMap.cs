using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

public class TowerMap:Control {
 public Image Art,IconAtlas;public Rectangle[] IconSources;public RogueRun Run;public Action<string> Selected;public Action GoBack;string hover;ToolTip tip=new ToolTip();Timer timer=new Timer();int pulse;
 public TowerMap(){DoubleBuffered=true;ResizeRedraw=true;TabStop=true;BackColor=Color.FromArgb(44,35,29);timer.Interval=120;timer.Tick+=(s,e)=>{pulse++;if(Run!=null&&Run.nodes!=null)foreach(var node in TowerEngine.Available(Run))Invalidate(Rectangle.Inflate(NodeBounds(node),24,12));};timer.Start();AccessibleName="背单词地图";}
 protected override void Dispose(bool d){if(d){timer.Dispose();tip.Dispose();}base.Dispose(d);}
 public Rectangle NodeBounds(TowerNode n){int gaps=Math.Max(1,TowerEngine.FloorCount(Run)-1);int size=Math.Max(27,Math.Min(52,Width/(gaps+2)/2));int x=(int)(Width*(.13+.74*n.row/gaps));int y=(int)(Height*(.30+.23*n.lane+(((n.row*13+n.lane*7+(Run.seed&1023))%7)-3)*.007));return new Rectangle(x-size/2,y-size/2,size,size);}
 public Rectangle ExitBounds(){return new Rectangle(16,14,84,38);}
 string Hit(Point p){if(Run==null)return null;return Run.nodes.Where(n=>TowerEngine.Available(Run).Contains(n)&&NodeBounds(n).Contains(p)).Select(n=>n.id).FirstOrDefault();}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);var hovered=Run.nodes.FirstOrDefault(n=>NodeBounds(n).Contains(e.Location));string id=hovered==null?null:hovered.id;if(hover!=id){hover=id;var node=Run.nodes.FirstOrDefault(n=>n.id==id);tip.SetToolTip(this,node==null?null:TowerEngine.KindName(node.kind)+" · "+TowerEngine.KindDescription(node.kind));Invalidate();}Cursor=TowerEngine.Available(Run).Any(n=>n.id==id)||ExitBounds().Contains(e.Location)?Cursors.Hand:Cursors.Default;}
 protected override void OnMouseLeave(EventArgs e){hover=null;Invalidate();base.OnMouseLeave(e);}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button!=MouseButtons.Left)return;if(ExitBounds().Contains(e.Location)){if(GoBack!=null)GoBack();return;}string id=Hit(e.Location);if(id!=null&&Selected!=null)Selected(id);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);var available=TowerEngine.Available(Run).OrderBy(n=>n.lane).ToList();if(available.Count==0)return;if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Right){int at=available.FindIndex(n=>n.id==hover);at=(at+(e.KeyCode==Keys.Left?-1:1)+available.Count)%available.Count;hover=available[at].id;Invalidate();e.Handled=true;}if(e.KeyCode==Keys.Enter){if(!available.Any(n=>n.id==hover))hover=available[0].id;if(hover!=null&&Selected!=null)Selected(hover);e.Handled=true;}}
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.SmoothingMode=SmoothingMode.None;
  if(Art!=null)g.DrawImage(Art,ClientRectangle);if(Run==null||Run.nodes==null)return;
  var available=TowerEngine.Available(Run);
  // Each boot points from its source to the actual permitted destination.
  foreach(var n in Run.nodes)foreach(string id in n.next){var to=Run.nodes.First(x=>x.id==id);var a=NodeBounds(n);var b=NodeBounds(to);bool active=n.id==Run.lastNode&&available.Contains(to),done=n.visited&&to.visited;Color ink=done?Color.FromArgb(175,91,75,53):active?Color.FromArgb(220,89,70,43):Color.FromArgb(125,120,99,70);float ax=a.Left+a.Width/2f,ay=a.Top+a.Height/2f,bx=b.Left+b.Width/2f,by=b.Top+b.Height/2f;float dx=bx-ax,dy=by-ay,dist=(float)Math.Sqrt(dx*dx+dy*dy);float unit=Math.Max(1,Width/1000f),inset=a.Width*.65f+8;int steps=Math.Max(2,(int)((dist-2*inset)/(unit*21)));
   for(int k=0;k<steps;k++){float along=inset+(dist-2*inset)*(k+.5f)/steps,side=(k%2==0?-1:1)*unit*4;DrawFootprint(g,ax+dx/dist*along-dy/dist*side,ay+dy/dist*along+dx/dist*side,(float)(Math.Atan2(dy,dx)*180/Math.PI),unit,ink);}
  }
  foreach(var n in Run.nodes){var box=NodeBounds(n);bool enabled=available.Contains(n),done=n.visited;Color color=Color.FromArgb(117,93,64);
   if(IconAtlas==null)DrawSymbol(g,n.kind,box,color);else{if(IconSources==null)IconSources=FindIconRegions(IconAtlas);int index=n.kind=="combat"?0:n.kind=="elite"?1:n.kind=="rest"?2:n.kind=="event"?3:n.kind=="chest"?4:n.kind=="shop"?5:6;var src=IconSources[index];float scale=Math.Min((float)box.Width/src.Width,(float)box.Height/src.Height);int w=(int)(src.Width*scale),h=(int)(src.Height*scale);var target=new Rectangle(box.Left+(box.Width-w)/2,box.Top+(box.Height-h)/2,w,h);using(var attr=new ImageAttributes()){var matrix=new ColorMatrix(new float[][]{new float[]{.35f,.20f,.16f,0,0},new float[]{.32f,.43f,.25f,0,0},new float[]{.10f,.10f,.23f,0,0},new float[]{0,0,0,(enabled||n.id==hover)?.95f:done?.66f:.76f,0},new float[]{.13f,.09f,.055f,0,1}});attr.SetColorMatrix(matrix);g.DrawImage(IconAtlas,target,src.X,src.Y,src.Width,src.Height,GraphicsUnit.Pixel,attr);}}
   if(done){using(var pen=new Pen(Color.FromArgb(80,99,61),3)){g.DrawLine(pen,box.Right-12,box.Bottom-6,box.Right-7,box.Bottom-1);g.DrawLine(pen,box.Right-7,box.Bottom-1,box.Right+2,box.Bottom-13);}}
   if(enabled)DrawFootprint(g,box.Left-13,box.Top+box.Height/2f,0,1.3f,Color.FromArgb(190,100,78,46));
  }
  using(var title=GameTheme.Body(20,FontStyle.Bold))TextRenderer.DrawText(g,"背单词地图",title,new Rectangle(Width/4,(int)(Height*.11),Width/2,44),Color.FromArgb(84,58,39),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);
  CyberChrome.Button(g,ExitBounds(),"返回",Font,false,false,true);
 }
 static void DrawFootprint(Graphics g,float x,float y,float angle,float unit,Color ink){var state=g.Save();g.TranslateTransform(x,y);g.RotateTransform(angle);g.SmoothingMode=SmoothingMode.None;using(var brush=new SolidBrush(ink)){g.FillRectangle(brush,-4*unit,-2*unit,3*unit,4*unit);g.FillRectangle(brush,0,-2*unit,4*unit,4*unit);g.FillRectangle(brush,unit,-3*unit,2*unit,6*unit);}g.Restore(state);} public static Rectangle[] FindIconRegions(Image atlas){
  var regions=new Rectangle[8];using(var pixels=new Bitmap(atlas)){int cw=pixels.Width/4,ch=pixels.Height/2;for(int i=0;i<8;i++){int sx=i%4*cw,sy=i/4*ch,left=sx+cw,top=sy+ch,right=sx,bottom=sy;for(int y=sy;y<sy+ch;y+=2)for(int x=sx;x<sx+cw;x+=2)if(pixels.GetPixel(x,y).A>32){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}regions[i]=left>right?new Rectangle(sx,sy,cw,ch):Rectangle.FromLTRB(Math.Max(sx,left-3),Math.Max(sy,top-3),Math.Min(sx+cw,right+4),Math.Min(sy+ch,bottom+4));}}return regions;
 }
 public static void DrawSymbol(Graphics g,string kind,Rectangle box,Color tint){
  string[] pixels=kind=="chest"?new[]{"  GGGGGGG  "," GYYYYYYYG ","GYYYYYYYYYG","GGGGGGGGGGG","GYYYGGGYYYG","GYYYGYGYYYG","GYYYGGGYYYG","GGGGGGGGGGG"}:kind=="rest"?new[]{"    Y     ","   YYY    "," Y YYYY   "," YYYYY Y  ","YYYYYYYYY "," YYYYYYY  ","  YYYYY   ","GG     GG "," GGGGGGG  "}:kind=="shop"?new[]{"    G     ","   GYG    ","  GYYY G  "," GYYYYYYG ","GGGGGGGGGG"," GY GY G  "," G  G  G  ","GGGGGGGGGG"}:kind=="event"?new[]{"  YYYYY  "," YY   YY ","      YY ","     YY  ","    YY   ","    YY   ","         ","    YY   "}:new[]{kind=="combat"?"         ":"Y       Y",kind=="combat"?"  YYYYY  ":"YY YYY YY"," YYYYYYY ","YYYYYYYYY","YYGYYY GY","YYYYYYYYY"," YYYGYYY ","  Y Y Y  ","  Y Y Y  "};
  int unit=Math.Max(2,Math.Min(box.Width/(pixels.Max(s=>s.Length)+2),box.Height/(pixels.Length+2)));int left=box.Left+(box.Width-pixels.Max(s=>s.Length)*unit)/2,top=box.Top+(box.Height-pixels.Length*unit)/2;
  for(int y=0;y<pixels.Length;y++)for(int x=0;x<pixels[y].Length;x++){char c=pixels[y][x];if(c==' ')continue;Color color=c=='G'?Color.FromArgb(90,80,47):kind=="rest"?Color.FromArgb(149,109,68):kind=="elite"||kind=="boss"?Color.FromArgb(143,100,76):tint;using(var b=new SolidBrush(color))g.FillRectangle(b,left+x*unit,top+y*unit,unit,unit);}
 }
}
