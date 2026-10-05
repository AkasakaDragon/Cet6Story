using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

public class TowerMap:Control {
 public Image Art,IconAtlas;public Rectangle[] IconSources;public RogueRun Run;public Action<string> Selected;public Action GoBack;string hover;ToolTip tip=new ToolTip();Bitmap mapScene;string sceneKey;
 public TowerMap(){DoubleBuffered=true;ResizeRedraw=true;TabStop=true;BackColor=Color.FromArgb(44,35,29);AccessibleName="背单词地图";var back=new JourneyBackButton{Location=new Point(16,14),Size=new Size(100,54),AccessibleName="返回远征大厅或主线地图"};back.Click+=(s,e)=>{if(GoBack!=null)GoBack();};Controls.Add(back);tip.SetToolTip(back,"返回");}
 protected override void Dispose(bool d){if(d){if(mapScene!=null)mapScene.Dispose();tip.Dispose();}base.Dispose(d);}
 public Rectangle NodeBounds(TowerNode n){int gaps=Math.Max(1,TowerEngine.FloorCount(Run)-1);int size=Math.Max(27,Math.Min(52,Width/(gaps+2)/2));int x=(int)(Width*(.13+.74*n.row/gaps));int y=(int)(Height*(.30+.23*n.lane+(((n.row*13+n.lane*7+(Run.seed&1023))%7)-3)*.007));return new Rectangle(x-size/2,y-size/2,size,size);}
 Rectangle HoverBounds(TowerNode n){var box=NodeBounds(n);return Rectangle.Inflate(box,(int)Math.Ceiling(box.Width*.16),(int)Math.Ceiling(box.Height*.16));}
 public Rectangle ExitBounds(){return new Rectangle(16,14,100,54);}
 string Hit(Point p){if(Run==null)return null;return Run.nodes.Where(n=>TowerEngine.Available(Run).Contains(n)&&(n.id==hover?HoverBounds(n):NodeBounds(n)).Contains(p)).Select(n=>n.id).FirstOrDefault();}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(Run==null||Run.nodes==null)return;var hovered=Run.nodes.FirstOrDefault(n=>(n.id==hover&&TowerEngine.Available(Run).Contains(n)?HoverBounds(n):NodeBounds(n)).Contains(e.Location));string id=hovered==null?null:hovered.id;if(hover!=id){string previous=hover;hover=id;InvalidateNode(previous);InvalidateNode(hover);var node=Run.nodes.FirstOrDefault(n=>n.id==id);tip.SetToolTip(this,node==null?null:TowerEngine.KindName(node.kind)+" · "+TowerEngine.KindDescription(node.kind));}Cursor=TowerEngine.Available(Run).Any(n=>n.id==id)||ExitBounds().Contains(e.Location)?Cursors.Hand:Cursors.Default;}
 protected override void OnMouseLeave(EventArgs e){string previous=hover;hover=null;InvalidateNode(previous);base.OnMouseLeave(e);}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button!=MouseButtons.Left)return;if(ExitBounds().Contains(e.Location)){if(GoBack!=null)GoBack();return;}string id=Hit(e.Location);if(id!=null&&Selected!=null)Selected(id);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);var available=TowerEngine.Available(Run).OrderBy(n=>n.lane).ToList();if(available.Count==0)return;if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Right){int at=available.FindIndex(n=>n.id==hover);at=(at+(e.KeyCode==Keys.Left?-1:1)+available.Count)%available.Count;hover=available[at].id;Invalidate();e.Handled=true;}if(e.KeyCode==Keys.Enter){if(!available.Any(n=>n.id==hover))hover=available[0].id;if(hover!=null&&Selected!=null)Selected(hover);e.Handled=true;}}
 protected override void OnPaint(PaintEventArgs e){
  if(Width<=0||Height<=0)return;
  string key=Width+"/"+Height+"/"+(Art==null?0:Art.GetHashCode())+"/"+(IconAtlas==null?0:IconAtlas.GetHashCode())+"/"+(Run==null?"":Run.seed+"/"+Run.depth+"/"+Run.lastNode+"/"+String.Join(",",Run.nodes.Select(n=>n.id+":"+n.visited)));
  if(mapScene==null||sceneKey!=key){if(mapScene!=null)mapScene.Dispose();mapScene=new Bitmap(Width,Height,PixelFormat.Format32bppPArgb);string selected=hover;hover=null;try{using(var g=Graphics.FromImage(mapScene)){g.Clear(BackColor);DrawMap(g);}}finally{hover=selected;}sceneKey=key;}
  e.Graphics.DrawImageUnscaled(mapScene,0,0);
  if(Run!=null&&Run.nodes!=null){var node=TowerEngine.Available(Run).FirstOrDefault(n=>n.id==hover);if(node!=null){e.Graphics.InterpolationMode=InterpolationMode.NearestNeighbor;e.Graphics.PixelOffsetMode=PixelOffsetMode.Half;DrawNode(e.Graphics,node);}}
 }
 void InvalidateNode(string id){if(Run==null||Run.nodes==null)return;var node=Run.nodes.FirstOrDefault(n=>n.id==id);if(node!=null)Invalidate(Rectangle.Inflate(HoverBounds(node),20,6));}
 void DrawNode(Graphics g,TowerNode n){var box=NodeBounds(n);bool enabled=TowerEngine.Available(Run).Contains(n),done=n.visited,highlighted=enabled&&n.id==hover;if(highlighted)box=HoverBounds(n);Color color=highlighted?Color.FromArgb(181,117,35):Color.FromArgb(117,93,64);
   if(enabled){var glow=Rectangle.Inflate(box,9,9);var saved=g.Save();g.SmoothingMode=SmoothingMode.AntiAlias;for(int radius=9;radius>=1;radius-=2)using(var halo=new Pen(Color.FromArgb(22,255,207,60),radius*2))g.DrawEllipse(halo,glow);using(var edge=new Pen(Color.FromArgb(240,255,214,83),2))g.DrawEllipse(edge,glow);g.Restore(saved);}if(IconAtlas==null)DrawSymbol(g,n.kind,box,color);else{if(IconSources==null)IconSources=FindIconRegions(IconAtlas);int index=n.kind=="combat"?0:n.kind=="elite"?1:n.kind=="rest"?2:n.kind=="event"?3:n.kind=="chest"?4:n.kind=="shop"?5:6;var src=IconSources[index];float scale=Math.Min((float)box.Width/src.Width,(float)box.Height/src.Height);int w=(int)(src.Width*scale),h=(int)(src.Height*scale);var target=new Rectangle(box.Left+(box.Width-w)/2,box.Top+(box.Height-h)/2,w,h);using(var attr=new ImageAttributes()){var matrix=new ColorMatrix(new float[][]{new float[]{.35f,.20f,.16f,0,0},new float[]{.32f,.43f,.25f,0,0},new float[]{.10f,.10f,.23f,0,0},new float[]{0,0,0,highlighted?1f:enabled?.95f:done?.66f:.76f,0},new float[]{.13f,.09f,.055f,0,1}});if(highlighted){matrix.Matrix00+=.16f;matrix.Matrix11+=.12f;matrix.Matrix22+=.05f;matrix.Matrix40+=.12f;matrix.Matrix41+=.08f;}if(done)matrix=new ColorMatrix(new float[][]{new float[]{.2126f,.2126f,.2126f,0,0},new float[]{.7152f,.7152f,.7152f,0,0},new float[]{.0722f,.0722f,.0722f,0,0},new float[]{0,0,0,.65f,0},new float[]{0,0,0,0,1}});attr.SetColorMatrix(matrix);g.DrawImage(IconAtlas,target,src.X,src.Y,src.Width,src.Height,GraphicsUnit.Pixel,attr);}}
   if(done){using(var pen=new Pen(Color.FromArgb(80,99,61),3)){g.DrawLine(pen,box.Right-12,box.Bottom-6,box.Right-7,box.Bottom-1);g.DrawLine(pen,box.Right-7,box.Bottom-1,box.Right+2,box.Bottom-13);}}

 }
 void DrawMap(Graphics g){
  g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.SmoothingMode=SmoothingMode.None;
  if(Art!=null)g.DrawImage(Art,ClientRectangle);if(Run==null||Run.nodes==null)return;
  var available=TowerEngine.Available(Run);
  // Each boot points from its source to the actual permitted destination.
  foreach(var n in Run.nodes)foreach(string id in n.next){var to=Run.nodes.First(x=>x.id==id);var a=NodeBounds(n);var b=NodeBounds(to);bool active=n.id==Run.lastNode&&available.Contains(to),done=n.visited&&to.visited;Color ink=done?Color.FromArgb(175,91,75,53):active?Color.FromArgb(220,89,70,43):Color.FromArgb(125,120,99,70);float ax=a.Left+a.Width/2f,ay=a.Top+a.Height/2f,bx=b.Left+b.Width/2f,by=b.Top+b.Height/2f;float dx=bx-ax,dy=by-ay,dist=(float)Math.Sqrt(dx*dx+dy*dy);float unit=Math.Max(1,Width/1000f),inset=a.Width*.65f+8;int steps=Math.Max(2,(int)((dist-2*inset)/(unit*21)));
   for(int k=0;k<steps;k++){float along=inset+(dist-2*inset)*(k+.5f)/steps,side=(k%2==0?-1:1)*unit*4;DrawFootprint(g,ax+dx/dist*along-dy/dist*side,ay+dy/dist*along+dx/dist*side,(float)(Math.Atan2(dy,dx)*180/Math.PI),unit,ink);}
  }
  foreach(var n in Run.nodes)DrawNode(g,n);
  using(var title=GameTheme.Body(20,FontStyle.Bold))TextRenderer.DrawText(g,"背单词地图",title,new Rectangle(Width/4,(int)(Height*.11),Width/2,44),Color.FromArgb(84,58,39),TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);

 }
 static void DrawFootprint(Graphics g,float x,float y,float angle,float unit,Color ink){var state=g.Save();g.TranslateTransform(x,y);g.RotateTransform(angle);g.SmoothingMode=SmoothingMode.None;using(var brush=new SolidBrush(ink)){g.FillRectangle(brush,-4*unit,-2*unit,3*unit,4*unit);g.FillRectangle(brush,0,-2*unit,4*unit,4*unit);g.FillRectangle(brush,unit,-3*unit,2*unit,6*unit);}g.Restore(state);} public static Rectangle[] FindIconRegions(Image atlas){
  var regions=new Rectangle[8];using(var pixels=new Bitmap(atlas)){int cw=pixels.Width/4,ch=pixels.Height/2;for(int i=0;i<8;i++){int sx=i%4*cw,sy=i/4*ch,left=sx+cw,top=sy+ch,right=sx,bottom=sy;for(int y=sy;y<sy+ch;y+=2)for(int x=sx;x<sx+cw;x+=2)if(pixels.GetPixel(x,y).A>32){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}if(i==5){var clean=MainIconBounds(pixels,new Rectangle(sx,sy,cw,ch));left=clean.Left;top=clean.Top;right=clean.Right-1;bottom=clean.Bottom-1;}regions[i]=left>right?new Rectangle(sx,sy,cw,ch):Rectangle.FromLTRB(Math.Max(sx,left-3),Math.Max(sy,top-3),Math.Min(sx+cw,right+4),Math.Min(sy+ch,bottom+4));}}return regions;
 }
 static Rectangle MainIconBounds(Bitmap pixels,Rectangle cell){
  var seen=new bool[cell.Width*cell.Height];var queue=new System.Collections.Generic.Queue<Point>();int largest=0;Rectangle result=cell;
  for(int y=0;y<cell.Height;y++)for(int x=0;x<cell.Width;x++){int at=y*cell.Width+x;if(seen[at])continue;seen[at]=true;if(pixels.GetPixel(cell.Left+x,cell.Top+y).A<=32)continue;queue.Enqueue(new Point(x,y));int count=0,left=x,right=x,top=y,bottom=y;
   while(queue.Count>0){var p=queue.Dequeue();count++;left=Math.Min(left,p.X);right=Math.Max(right,p.X);top=Math.Min(top,p.Y);bottom=Math.Max(bottom,p.Y);for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=p.X+dx,ny=p.Y+dy;if(nx<0||ny<0||nx>=cell.Width||ny>=cell.Height)continue;int next=ny*cell.Width+nx;if(seen[next])continue;seen[next]=true;if(pixels.GetPixel(cell.Left+nx,cell.Top+ny).A>32)queue.Enqueue(new Point(nx,ny));}}
   if(count>largest){largest=count;result=Rectangle.FromLTRB(cell.Left+left,cell.Top+top,cell.Left+right+1,cell.Top+bottom+1);}
  }return result;
 }
 public static void DrawSymbol(Graphics g,string kind,Rectangle box,Color tint){
  string[] pixels=kind=="chest"?new[]{"  GGGGGGG  "," GYYYYYYYG ","GYYYYYYYYYG","GGGGGGGGGGG","GYYYGGGYYYG","GYYYGYGYYYG","GYYYGGGYYYG","GGGGGGGGGGG"}:kind=="rest"?new[]{"    Y     ","   YYY    "," Y YYYY   "," YYYYY Y  ","YYYYYYYYY "," YYYYYYY  ","  YYYYY   ","GG     GG "," GGGGGGG  "}:kind=="shop"?new[]{"    G     ","   GYG    ","  GYYY G  "," GYYYYYYG ","GGGGGGGGGG"," GY GY G  "," G  G  G  ","GGGGGGGGGG"}:kind=="event"?new[]{"  YYYYY  "," YY   YY ","      YY ","     YY  ","    YY   ","    YY   ","         ","    YY   "}:new[]{kind=="combat"?"         ":"Y       Y",kind=="combat"?"  YYYYY  ":"YY YYY YY"," YYYYYYY ","YYYYYYYYY","YYGYYY GY","YYYYYYYYY"," YYYGYYY ","  Y Y Y  ","  Y Y Y  "};
  int unit=Math.Max(2,Math.Min(box.Width/(pixels.Max(s=>s.Length)+2),box.Height/(pixels.Length+2)));int left=box.Left+(box.Width-pixels.Max(s=>s.Length)*unit)/2,top=box.Top+(box.Height-pixels.Length*unit)/2;
  for(int y=0;y<pixels.Length;y++)for(int x=0;x<pixels[y].Length;x++){char c=pixels[y][x];if(c==' ')continue;Color color=c=='G'?Color.FromArgb(90,80,47):kind=="rest"?Color.FromArgb(149,109,68):kind=="elite"||kind=="boss"?Color.FromArgb(143,100,76):tint;using(var b=new SolidBrush(color))g.FillRectangle(b,left+x*unit,top+y*unit,unit,unit);}
 }
}
