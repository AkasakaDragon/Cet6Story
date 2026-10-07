using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public partial class Game {
 void ShowChapterOneStayEpilogue(){
  StopAudio();ClearPage();page="waystation-stay-epilogue";
  string[] speakers={"旁白","陆川","艾莉娅","陆川","艾莉娅","陆川","艾莉娅","旁白"};
  string[] zh={
   "打烊之后，陆川收起账本。艾莉娅却仍站在矿道记录旁，目光停在那枚修补过的晶核上。",
   "今天多亏你了。不过，你明天是不是也该继续赶路了？",
   "矿道里的事还没查清。那块碎片为什么会在那里，晶核又为什么会回应它……我想留下弄明白。",
   "所以，是为了调查？",
   "还有，你今天没有逞强。该配合的时候配合，该撤退的时候也听得进去。和这样的人一起做事，我比较放心。",
   "那明天，还是给你留一份早饭？",
   "留吧。桌子你擦，别又全推给我。",
   "陆川拿起抹布，艾莉娅把记录折好。留下来的理由有了答案：未解的线索，还有一个值得信任的搭档。门边的灯，仍然亮着。"};
  string[] en={
   "After closing, Lu Chuan puts away the ledger. Aelia remains beside the mine notes, studying the repaired crystal.",
   "Thanks for everything today. But shouldn't you be continuing your journey tomorrow?",
   "We still haven't solved the mystery in the mine. Why was that fragment there, and why did the crystal respond to it? I want to stay and find out.",
   "So you're staying to investigate?",
   "And you didn't try to be a hero today. You worked with us and listened when it was time to retreat. I feel safer working with someone like that.",
   "Then shall I save you some breakfast tomorrow?",
   "Please do. You're wiping the tables, though. Don't leave them all to me again.",
   "Lu Chuan picks up the cloth as Aelia folds the notes. Unanswered questions and a trusted partner give her a reason to stay. The lamp by the door is still burning."};
  var surface=new Panel{Dock=DockStyle.Fill,BackColor=Color.Black};content.Controls.Add(surface);
  var picture=new PictureBox{SizeMode=PictureBoxSizeMode.Zoom,BackColor=Color.Black};surface.Controls.Add(picture);
  var box=new Panel{Height=210,Padding=new Padding(28,16,28,12),BackColor=Color.FromArgb(8,25,31)};surface.Controls.Add(box);box.BringToFront();
  Action layout=()=>{int top=Math.Max(0,surface.ClientSize.Height-210);picture.SetBounds(0,0,surface.ClientSize.Width,top);box.SetBounds(0,top,surface.ClientSize.Width,210);box.BringToFront();};surface.Resize+=(s,e)=>layout();layout();
  var controls=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=48,FlowDirection=FlowDirection.RightToLeft};box.Controls.Add(controls);
  var text=new Label{Dock=DockStyle.Fill,ForeColor=Color.FromArgb(240,225,190),Font=GameTheme.Body(15),TextAlign=ContentAlignment.MiddleLeft};box.Controls.Add(text);text.BringToFront();
  var next=new VNButton{Text="点击继续",PixelStyle=true,Size=new Size(150,42),Font=GameTheme.Body(12)};
  var previous=new VNButton{Text="上一句",PixelStyle=true,Size=new Size(120,42),Font=GameTheme.Body(12)};controls.Controls.Add(next);controls.Controls.Add(previous);
  int line=0;
  Action update=()=>{var nextImage=CachedImage(Path.Combine(root,"chapters","art","tavern",line<4?"chapter-one-stay-investigation.png":"chapter-one-stay-partners.png"));if(picture.Image!=null&&picture.Image!=nextImage)BeginStoryBackgroundBlackout();picture.Image=nextImage;text.Text="【"+speakers[line]+"】\n"+en[line]+"\n"+zh[line];previous.Enabled=line>0;next.Text=line==zh.Length-1?"完成第一章":"点击继续";};
  Action advance=()=>{if(page!="waystation-stay-epilogue")return;if(line<zh.Length-1){line++;update();}else{Attempt().endingShown=true;Persist();CompleteChapterOne();}};
  next.Click+=(s,e)=>advance();previous.Click+=(s,e)=>{if(line>0){line--;update();}};picture.Click+=(s,e)=>advance();text.Click+=(s,e)=>advance();update();
 }
}
