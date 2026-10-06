using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Linq;using System.Windows.Forms;
public class PrologueCompletion:Control {
 public Image BackgroundArt;public Chapter Chapter;public Save Save;public int Correct,Stars;public bool FirstCompletion;
 public Action ReturnHome,Replay,Chapters;
 readonly VNButton home,replay,chapters;readonly Color gold=Color.FromArgb(244,205,99),ivory=Color.FromArgb(247,242,222);
 public PrologueCompletion(){DoubleBuffered=true;ResizeRedraw=true;home=Button("← 返回主界面",()=>ReturnHome());replay=Button("整节连续重听",()=>Replay());chapters=Button("主线章节",()=>Chapters());}
 VNButton Button(string text,Action action){var button=new VNButton{Text=text,HomeStyle=true,AccessibleName=text};button.Click+=(s,e)=>action();Controls.Add(button);return button;}
 protected override void OnResize(EventArgs e){base.OnResize(e);float k=Math.Min(Width/1672f,Height/941f);float x=(Width-1672*k)/2,y=(Height-941*k)/2;Place(home,x,y,k,42,40,250,64);Place(replay,x,y,k,480,805,340,78);Place(chapters,x,y,k,845,805,340,78);}
 void Place(VNButton b,float x,float y,float k,int left,int top,int width,int height){b.Bounds=new Rectangle((int)(x+left*k),(int)(y+top*k),(int)(width*k),(int)(height*k));b.Font=GameTheme.Body(Math.Max(9,19*k),FontStyle.Bold);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(Chapter==null||Save==null)return;var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
  if(BackgroundArt!=null){float cover=Math.Max(Width/(float)BackgroundArt.Width,Height/(float)BackgroundArt.Height);g.DrawImage(BackgroundArt,(Width-BackgroundArt.Width*cover)/2,(Height-BackgroundArt.Height*cover)/2,BackgroundArt.Width*cover,BackgroundArt.Height*cover);}
  float k=Math.Min(Width/1672f,Height/941f);g.TranslateTransform((Width-1672*k)/2,(Height-941*k)/2);g.ScaleTransform(k,k);g.SmoothingMode=SmoothingMode.AntiAlias;
  using(var backdrop=new LinearGradientBrush(new Rectangle(350,106,972,191),Color.Transparent,Color.Transparent,0f)){backdrop.InterpolationColors=new ColorBlend{Colors=new[]{Color.Transparent,Color.FromArgb(185,5,26,30),Color.FromArgb(185,5,26,30),Color.Transparent},Positions=new[]{0f,.15f,.85f,1f}};g.FillRectangle(backdrop,350,106,972,191);}
  using(var pen=new Pen(gold,2)){g.DrawEllipse(pen,760,66,152,152);g.DrawLine(pen,836,58,836,140);g.DrawLine(pen,765,143,907,143);g.DrawLine(pen,530,205,1142,205);}
  DrawText(g,"序幕完成 · 第一盏灯",new RectangleF(370,113,932,90),64,gold,true,true);
  DrawText(g,"驿站基础修复完成。陆川与艾莉娅一起迎来天亮，\n经营基地已解锁。第一章“今天开始营业”后续开放。",new RectangleF(470,224,732,65),21,ivory,false,true);
  Panel(g,new Rectangle(428,303,802,73));DrawText(g,"听力  "+Correct+" / "+Chapter.questions.Count,new RectangleF(464,320,220,40),27,gold,true);
  DrawText(g,new string('★',Stars)+new string('☆',3-Stars),new RectangleF(742,316,170,48),32,gold,true);
  DrawText(g,FirstCompletion?"首次完成  +60 XP":"完成奖励已领取",new RectangleF(935,320,270,40),25,gold,true);
  int count=Math.Max(1,Chapter.questions.Count);float h=Math.Min(168,350f/count-12);
  for(int i=0;i<Chapter.questions.Count;i++){
   var q=Chapter.questions[i];int top=(int)(428+i*(h+12));Panel(g,new Rectangle(352,top,960,(int)h));
   using(var pen=new Pen(gold,2))g.DrawPolygon(pen,new[]{new Point(422,top+12),new Point(460,top+50),new Point(422,top+88),new Point(384,top+50)});
   DrawText(g,(i+1).ToString(),new RectangleF(395,top+25,54,55),30,gold,true,true);
   DrawText(g,q.prompt,new RectangleF(491,top+12,770,53),25,ivory,true);
   int selected;bool answered=Save.quizAnswers.TryGetValue(Chapter.id+"/q/"+q.afterLine,out selected);string answer=answered?((char)('A'+selected)).ToString():"未作答";
   DrawText(g,"你的答案",new RectangleF(495,top+69,124,34),22,ivory);DrawText(g,answer,new RectangleF(618,top+67,96,38),25,answered&&selected==q.answer?Color.FromArgb(96,239,121):Color.FromArgb(249,151,126),true);
   DrawText(g,"·  正确答案",new RectangleF(716,top+69,147,34),22,ivory);DrawText(g,((char)('A'+q.answer)).ToString(),new RectangleF(873,top+67,80,38),25,Color.FromArgb(96,239,121),true);
   using(var pen=new Pen(Color.FromArgb(142,174,139,63)))g.DrawLine(pen,490,top+62,1265,top+62);
   Panel(g,new Rectangle(480,top+108,798,(int)h-117));DrawText(g,q.explanation,new RectangleF(501,top+112,755,h-125),18,ivory);
  }
 }
 void Panel(Graphics g,Rectangle r){using(var fill=new SolidBrush(Color.FromArgb(244,5,43,44)))g.FillPolygon(fill,GameTheme.Outline(r,12));using(var edge=new Pen(gold,2))g.DrawPolygon(edge,GameTheme.Outline(r,12));using(var edge=new Pen(Color.FromArgb(166,139,63),1))g.DrawPolygon(edge,GameTheme.Outline(Rectangle.Inflate(r,-5,-5),9));}
 void DrawText(Graphics g,string text,RectangleF r,float size,Color ink,bool bold=false,bool center=false){using(var font=new Font("Microsoft YaHei",size,bold?FontStyle.Bold:FontStyle.Regular,GraphicsUnit.Pixel))using(var brush=new SolidBrush(ink))using(var format=new StringFormat{Alignment=center?StringAlignment.Center:StringAlignment.Near,LineAlignment=StringAlignment.Center,Trimming=StringTrimming.EllipsisCharacter})g.DrawString(text,font,brush,r,format);}
}
