using System;using System.Drawing;using System.Drawing.Drawing2D;using System.Linq;using System.Windows.Forms;
public class PrologueCompletion:Control {
 public Image BackgroundArt;public Chapter Chapter;public Save Save;public int Correct,Stars;public bool FirstCompletion;
 public string CompletionTitle="序幕完成 · 第一盏灯",CompletionText="驿站基础修复完成。陆川与艾莉娅一起迎来天亮，\n经营基地已解锁。第一章第一节已开放。";
 public Action ReturnHome,Replay,Chapters;
 readonly VNButton home,replay,chapters;readonly Color gold=Color.FromArgb(244,205,99),ivory=Color.FromArgb(247,242,222);
 public PrologueCompletion(){DoubleBuffered=true;ResizeRedraw=true;home=Button("← 返回主界面",()=>ReturnHome());replay=Button("整节连续重听",()=>Replay());chapters=Button("主线章节",()=>Chapters());}
 VNButton Button(string text,Action action){var button=new VNButton{Text=text,HomeStyle=true,AccessibleName=text};button.Click+=(s,e)=>action();Controls.Add(button);return button;}
 protected override void OnResize(EventArgs e){base.OnResize(e);float k=Math.Min(Width/1672f,Height/941f);float x=(Width-1672*k)/2,y=(Height-941*k)/2;Place(home,x,y,k,42,40,250,64);Place(replay,x,y,k,480,805,340,78);Place(chapters,x,y,k,845,805,340,78);}
 void Place(VNButton b,float x,float y,float k,int left,int top,int width,int height){b.Bounds=new Rectangle((int)(x+left*k),(int)(y+top*k),(int)(width*k),(int)(height*k));b.Font=GameTheme.Body(Math.Max(9,19*k),FontStyle.Bold);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(Chapter==null||Save==null)return;var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;
  if(BackgroundArt!=null){float cover=Math.Max(Width/(float)BackgroundArt.Width,Height/(float)BackgroundArt.Height);g.DrawImage(BackgroundArt,(Width-BackgroundArt.Width*cover)/2,(Height-BackgroundArt.Height*cover)/2,BackgroundArt.Width*cover,BackgroundArt.Height*cover);}
  float k=Math.Min(Width/1672f,Height/941f);g.TranslateTransform((Width-1672*k)/2,(Height-941*k)/2);g.ScaleTransform(k,k);g.SmoothingMode=SmoothingMode.AntiAlias;
  using(var backdrop=new LinearGradientBrush(new Rectangle(180,106,1312,198),Color.Transparent,Color.Transparent,0f)){backdrop.InterpolationColors=new ColorBlend{Colors=new[]{Color.Transparent,Color.FromArgb(210,5,26,30),Color.FromArgb(210,5,26,30),Color.Transparent},Positions=new[]{0f,.12f,.88f,1f}};g.FillRectangle(backdrop,180,106,1312,198);}
  using(var pen=new Pen(gold,2))g.DrawLine(pen,530,210,1142,210);
  DrawText(g,CompletionTitle,new RectangleF(220,113,1232,92),44,gold,true,true);
  DrawText(g,CompletionText,new RectangleF(250,218,1172,84),21,ivory,false,true);
  Panel(g,new Rectangle(428,303,802,73));DrawText(g,"听力  "+Correct+" / "+Chapter.questions.Count,new RectangleF(464,320,220,40),27,gold,true);
  DrawText(g,new string('★',Stars)+new string('☆',3-Stars),new RectangleF(742,316,170,48),32,gold,true);
  DrawText(g,FirstCompletion?"首次完成  +60 XP":"完成奖励已领取",new RectangleF(935,320,270,40),25,gold,true);
  int count=Math.Max(1,Chapter.questions.Count);
  for(int i=0;i<Chapter.questions.Count;i++){
   var q=Chapter.questions[i];var card=QuestionBounds(i,count);int left=card.Left,top=card.Top;Panel(g,card);
   using(var pen=new Pen(gold,2))g.DrawPolygon(pen,new[]{new Point(left+40,top+17),new Point(left+63,top+40),new Point(left+40,top+63),new Point(left+17,top+40)});
   DrawText(g,(i+1).ToString(),new RectangleF(left+22,top+21,36,37),25,gold,true,true);
   DrawText(g,q.prompt,new RectangleF(left+78,top+12,card.Width-100,62),24,ivory,true);
   int selected;bool answered=Save.quizAnswers.TryGetValue(Chapter.id+"/q/"+q.afterLine,out selected);string answer=answered?((char)('A'+selected)).ToString():"未作答";
   DrawText(g,"你的答案  "+answer,new RectangleF(left+28,top+80,250,32),21,answered&&selected==q.answer?Color.FromArgb(96,239,121):Color.FromArgb(249,151,126),true);
   DrawText(g,"正确答案  "+((char)('A'+q.answer)),new RectangleF(left+290,top+80,card.Width-316,32),21,Color.FromArgb(96,239,121),true);
   using(var pen=new Pen(Color.FromArgb(142,174,139,63)))g.DrawLine(pen,left+25,top+116,card.Right-25,top+116);
   DrawText(g,q.explanation,new RectangleF(left+28,top+124,card.Width-56,card.Height-136),18,ivory);
  }
 }
 void Panel(Graphics g,Rectangle r){using(var fill=new SolidBrush(Color.FromArgb(244,5,43,44)))g.FillPolygon(fill,GameTheme.Outline(r,12));using(var edge=new Pen(gold,2))g.DrawPolygon(edge,GameTheme.Outline(r,12));using(var edge=new Pen(Color.FromArgb(166,139,63),1))g.DrawPolygon(edge,GameTheme.Outline(Rectangle.Inflate(r,-5,-5),9));}
 public static Rectangle QuestionBounds(int index,int count){int columns=count>=3?2:1,rows=(Math.Max(1,count)+columns-1)/columns;int width=columns==2?622:960,height=Math.Min(182,(380-(rows-1)*16)/rows),left=columns==2?206:356;return new Rectangle(left+(index%columns)*(width+22),400+(index/columns)*(height+16),width,height);}
 public static float FittedTextSize(Graphics g,string text,RectangleF r,float size,bool bold){using(var format=new StringFormat()){for(float fitted=size;fitted>=12;fitted-=1){using(var font=new Font("Microsoft YaHei",fitted,bold?FontStyle.Bold:FontStyle.Regular,GraphicsUnit.Pixel)){int characters,lines;var measured=g.MeasureString(text??"",font,r.Size,format,out characters,out lines);if(characters>=(text??"").Length&&measured.Height<=r.Height)return fitted;}}}return 12;}
 void DrawText(Graphics g,string text,RectangleF r,float size,Color ink,bool bold=false,bool center=false){if(r.Width<=0||r.Height<=0)return;size=FittedTextSize(g,text,r,size,bold);using(var font=new Font("Microsoft YaHei",size,bold?FontStyle.Bold:FontStyle.Regular,GraphicsUnit.Pixel))using(var brush=new SolidBrush(ink))using(var format=new StringFormat{Alignment=center?StringAlignment.Center:StringAlignment.Near,LineAlignment=StringAlignment.Center,Trimming=StringTrimming.None})g.DrawString(text,font,brush,r,format);}
}
