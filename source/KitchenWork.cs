using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

public class KitchenWorkState {
 public bool cutCorrect{get;set;}public bool cookCorrect{get;set;}public bool Correct(string kind){return kind=="cut"?cutCorrect:cookCorrect;}public void SetCorrect(string kind,bool value){if(kind=="cut")cutCorrect=value;else cookCorrect=value;} public string cutFeedback{get;set;}public string cookFeedback{get;set;}public string Feedback(string kind){return kind=="cut"?cutFeedback:cookFeedback;}public void SetFeedback(string kind,string value){if(kind=="cut")cutFeedback=value;else cookFeedback=value;} public RogueQuestion cutQuestion{get;set;}public RogueQuestion cookQuestion{get;set;}public RogueQuestion Question(string kind){return kind=="cut"?cutQuestion:cookQuestion;}public void SetQuestion(string kind,RogueQuestion question){if(kind=="cut")cutQuestion=question;else cookQuestion=question;} public string wordMode{get;set;}public List<RogueEntry> cutWords{get;set;}public List<RogueEntry> cookWords{get;set;}public List<string> recentWords{get;set;}public string cutDraft{get;set;}public string cookDraft{get;set;}public List<RogueEntry> Words(string kind){return kind=="cut"?cutWords:cookWords;} public string held{get;set;}public string cooking{get;set;}public bool boardLoaded{get;set;}public int cut{get;set;}public int heat{get;set;}public int served{get;set;}
 public KitchenWorkState(){held="";cooking="";wordMode="四六级混合";recentWords=new List<string>();}
 public bool TakeCarrot(){if(!String.IsNullOrEmpty(held)||boardLoaded||!String.IsNullOrEmpty(cooking))return false;held="raw";return true;}
 public bool Start(string station){if(station=="cut"){if(boardLoaded)return true;if(held!="raw")return false;held="";boardLoaded=true;cut=0;return true;}if(cooking==station)return true;if(!String.IsNullOrEmpty(cooking)||held!="chopped")return false;held="";cooking=station;heat=0;return true;}
 public int Progress(string station){return station=="cut"?cut:heat;}
 public bool Answer(string station,string answer){int step=Progress(station);if(step>=3||!(station=="cut"?boardLoaded:cooking==station))return false;if(!String.Equals((answer??"").Trim(),(Words(station)!=null&&Words(station).Count==3?Words(station)[step].word:KitchenWords.Words(station)[step]),StringComparison.OrdinalIgnoreCase))return false;if(station=="cut")cut++;else heat++;return true;}
 public bool Pickup(string station){if(!String.IsNullOrEmpty(held)||Progress(station)!=3)return false;if(station=="cut"&&boardLoaded){held="chopped";boardLoaded=false;cut=0;cutWords=null;cutQuestion=null;cutDraft="";return true;}if(station!="cut"&&cooking==station){held="finished";cooking="";heat=0;cookWords=null;cookQuestion=null;cookDraft="";return true;}return false;}
 public bool Plate(){if(held!="finished")return false;held="";served++;return true;}
}
public static class KitchenWords {
 public static string[] Words(string kind){return kind=="cut"?new[]{"carrot","slice","chop"}:kind=="boil"?new[]{"water","steam","tender"}:kind=="bake"?new[]{"oven","roast","golden"}:new[]{"pan","fry","crisp"};}
 public static string[] Meanings(string kind){return kind=="cut"?new[]{"胡萝卜（名词）","切成薄片（动词）","切碎（动词）"}:kind=="boil"?new[]{"水（名词）","蒸汽（名词）","软嫩的（形容词）"}:kind=="bake"?new[]{"烤炉（名词）","烘烤（动词）","金黄的（形容词）"}:new[]{"平底锅（名词）","煎（动词）","酥脆的（形容词）"};}
 public static string Title(string kind){return kind=="cut"?"切配":kind=="boil"?"蒸煮":kind=="bake"?"烘烤":"煎制";}
 public static string[] Stages(string kind){return kind=="cut"?new[]{"整根胡萝卜","切去根部","切成厚片","切配完成"}:kind=="boil"?new[]{"放入锅中","加热冒泡","蒸汽升起","蒸煮完成"}:kind=="bake"?new[]{"放入烤盘","开始烘烤","表面变色","烘烤完成"}:new[]{"放入煎锅","加热滋响","翻动上色","煎制完成"};}
}

public partial class Game {
 KitchenWorkState KitchenState(){if(save.kitchenWork==null)save.kitchenWork=new KitchenWorkState();if(!KitchenVocabulary.ValidMode(save.kitchenWork.wordMode))save.kitchenWork.wordMode="四六级混合";return save.kitchenWork;}
 void ShowKitchenWork(){if(TavernTime.State(save).phase!=StoryTime.Night){GameMessage.Show(this,"酒馆只在晚上开放。","厨房工作");return;}ClearPage();page="kitchen-work";var view=new KitchenRoomView(root,KitchenState(),station=>KitchenInteract(station),ShowTavernBusiness){Dock=DockStyle.Fill};content.Controls.Add(view);AddKitchenModeButtons(view);view.Focus();}
 void KitchenInteract(string station){var state=KitchenState();if(station=="shelf"){
   using(var dialog=new GuildWordDialog{Text="食材架 · 已有食材",Size=new Size(Math.Min(560,ClientSize.Width-24),300)}){
    dialog.Location=new Point(Left+(Width-dialog.Width)/2,Top+(Height-dialog.Height)/2);var label=new OutlinedLabel{Dock=DockStyle.Fill,Font=GameTheme.Body(14),ForeColor=GuildChrome.Ivory,Padding=new Padding(20),Text="胡萝卜 · Carrot\n\n本次厨房练习只开放胡萝卜。每次拿取一根。"};dialog.Controls.Add(label);
    var take=PartyButton("拿取一根胡萝卜",()=>{if(state.TakeCarrot()){Persist();dialog.Close();}else label.Text="请先处理手中或工作台上的胡萝卜，装盘后再拿取。";},260,46);take.Dock=DockStyle.Bottom;dialog.Controls.Add(take);dialog.ShowDialog(this);
   }
  }else if(station=="plate"){if(state.Plate()){Persist();GameMessage.Show(this,"装盘完成！\n已完成 "+state.served+" 份胡萝卜料理。","厨房工作");}else GameMessage.Show(this,"先拿取加工完成的胡萝卜，再来装盘。","装盘台");}
  else {if(!state.Start(station)){GameMessage.Show(this,station=="cut"?"先从食材架拿取一根胡萝卜。":"先完成切配并拿取，再选择蒸煮、烘烤或煎制。\n若已有食材正在加工，请回到对应工作台继续。",KitchenWords.Title(station));return;}Persist();ShowKitchenProcess(station);}
  if(content.Controls.Count>0){content.Controls[0].Invalidate();content.Controls[0].Focus();}
 }
 void ShowKitchenProcess(string kind){
  var state=KitchenState();PrepareKitchenWords(kind);
  using(var dialog=new GuildWordDialog{Text=KitchenWords.Title(kind)+" · 胡萝卜",Size=new Size(Math.Min(980,ClientSize.Width-24),Math.Min(760,ClientSize.Height-24)),KeyPreview=true}){
   dialog.Location=new Point(Left+(Width-dialog.Width)/2,Top+(Height-dialog.Height)/2);
   var body=new Panel{Dock=DockStyle.Fill,BackColor=Color.FromArgb(25,53,55)};dialog.Controls.Add(body);
   var upper=new Panel{BackColor=body.BackColor};body.Controls.Add(upper);
   var prompt=new KitchenWordPrompt{BackColor=body.BackColor,ForeColor=GuildChrome.Ivory,Font=GameTheme.Body(16)};upper.Controls.Add(prompt);
   var hear=new RogueIcon{Kind="speaker",Size=new Size(32,32),AccessibleName="朗读单词",BackColor=body.BackColor};upper.Controls.Add(hear);tips.SetToolTip(hear,"朗读本题英文单词");
   var favorite=new RogueIcon{Kind="star",Size=new Size(32,32),AccessibleName="收藏单词",BackColor=body.BackColor};upper.Controls.Add(favorite);
   var status=new OutlinedLabel{ForeColor=GuildChrome.Gold,Font=GameTheme.Body(11)};upper.Controls.Add(status);
   var input=new TextBox{Font=GameTheme.Latin(18),BackColor=Color.FromArgb(34,68,65),ForeColor=GuildChrome.Ivory,BorderStyle=BorderStyle.FixedSingle,ImeMode=ImeMode.Disable,MaxLength=100,AccessibleName="输入英文拼写"};upper.Controls.Add(input);
   var definition=new OutlinedLabel{Font=GameTheme.Body(14),ForeColor=GuildChrome.Ivory,Visible=false};upper.Controls.Add(definition);
   var scene=new KitchenProcessView(root,kind,state);body.Controls.Add(scene);
   bool busy=false;bool feedback=false;int ticks=0;RogueQuestion question=null;var animation=new Timer{Interval=35};var choices=new List<VNButton>();VNButton submit=null;Action layout=null;Action refresh=null;Action<int> answer=null;
   hear.Click+=(sender,e)=>{if(!busy&&question!=null&&question.kind>=0&&(feedback||question.kind!=1))SpeakWord(question.entry.word);};
   favorite.Click+=(sender,e)=>{if(!busy&&question!=null&&question.kind>=0)ToggleRogueFavorite(question.entry,favorite);};
   refresh=()=>{
    int step=state.Progress(kind);question=step<3?state.Question(kind):null;
    if(step<3&&question==null){question=ExpeditionVocabulary.Next(save.rogue,KitchenBank(),DateTime.Today);state.SetQuestion(kind,question);state.Words(kind)[step]=question.entry;Persist();}
    feedback=question!=null&&question.answered;bool word=question!=null&&question.kind>=0;
    prompt.Font=word&&(feedback||question.kind==0)?GameTheme.Latin(18):GameTheme.Body(16);
    prompt.Text=step==3?"处理成功 · 胡萝卜":!word?"今日词汇练习完成":feedback||question.kind==0?question.entry.word:ExpeditionVocabulary.Meaning(question.entry);
    status.Text=step==3?"本次加工完成 · 可拿取食材":"第 "+(step+1)+" / 3 题 · "+(!word?"继续加工":question.kind==0?"选择中文释义":question.kind==1?"选择英文单词":"拼写英文单词")+" · "+KitchenWords.Stages(kind)[step]+(word?"\n"+ExpeditionVocabulary.Status(save.rogue,question,DateTime.Today):"");
    if(feedback)status.Text=(state.Correct(kind)?"回答正确 · ":"回答错误 · ")+state.Feedback(kind)+"\n"+(state.Correct(kind)?"确认单词后，点击继续完成当前加工。":"记住正确单词，点击继续换下一题。");definition.Visible=feedback&&word;definition.Text=word?ExpeditionVocabulary.Meaning(question.entry):"";
    hear.Visible=word&&(feedback||question.kind!=1);favorite.Visible=word;hear.Enabled=favorite.Enabled=!busy;
    if(word){favorite.Selected=save.words.Any(w=>w.text.Equals(question.entry.word,StringComparison.OrdinalIgnoreCase));favorite.Invalidate();tips.SetToolTip(favorite,favorite.Selected?"已收藏 · 再次点击取消":"收藏到生词本");}
    input.Visible=!feedback&&word&&question.kind==3;submit.Visible=feedback||step==3||!word||question.kind==3;submit.Text=feedback?(state.Correct(kind)?"继续加工（Enter）":"下一单词（Enter）"):step==3?"拿取处理好的胡萝卜":!word?"继续加工":"确认拼写（Enter）";
    input.Text=question==null?"":question.spellingDraft??"";
    for(int i=0;i<choices.Count;i++){bool showChoice=!feedback&&word&&question.kind<3;choices[i].Visible=showChoice;choices[i].Enabled=!busy;if(showChoice)choices[i].Text=((char)('A'+i))+". "+question.options[i];}
    if(layout!=null)layout();
   };
   answer=selected=>{
    if(busy)return;
    if(state.Progress(kind)==3){if(state.Pickup(kind)){Persist();dialog.Close();}return;}
    if(feedback){
     if(!state.Correct(kind)){state.SetQuestion(kind,null);state.SetFeedback(kind,null);Persist();refresh();if(input.Visible)input.Focus();return;}
     if(!state.Answer(kind,question.entry.word))return;
     state.SetQuestion(kind,null);state.SetFeedback(kind,null);Persist();busy=true;feedback=false;submit.Enabled=input.Enabled=hear.Enabled=favorite.Enabled=false;foreach(var choice in choices)choice.Enabled=false;
     ticks=0;scene.Animating=true;scene.Motion=0;status.Text="正在加工 · "+KitchenWords.Stages(kind)[state.Progress(kind)];animation.Start();return;
    }
    if(question.kind>=0)ExpeditionVocabulary.Progress(save.rogue,question.entry.word,DateTime.Today).lastQuestion=ExpeditionVocabulary.Profile(save.rogue).serial;
    bool correct=ExpeditionVocabulary.Correct(question,selected,input.Text);
    if(!correct){state.SetFeedback(kind,ExpeditionVocabulary.Answer(save.rogue,question,false,DateTime.Today));state.SetCorrect(kind,false);question.answered=true;Persist();refresh();return;}
    string learning=question.kind<0?"今日没有待学或到期词汇":ExpeditionVocabulary.Answer(save.rogue,question,true,DateTime.Today);
    if(question.kind>=0){TavernBusiness.Begin(save,"words");TavernBusiness.Reward(save,"word:"+question.entry.word,true);}
    state.SetCorrect(kind,true);question.answered=true;state.SetFeedback(kind,learning);Persist();refresh();
   };
   submit=PartyButton("确认拼写（Enter）",()=>answer(-1),250,46);upper.Controls.Add(submit);
   for(int i=0;i<4;i++){int selected=i;var button=PartyButton("",()=>answer(selected),240,44);button.Font=GameTheme.Body(12);choices.Add(button);upper.Controls.Add(button);}
   layout=()=>{
    int w=body.Width;int pw=Math.Min(Math.Max(1,w-116),TextRenderer.MeasureText(prompt.Text,prompt.Font,Size.Empty,TextFormatFlags.NoPadding).Width+8);
    int ph=56;
    prompt.Bounds=new Rectangle(18,8,pw,ph);int iconY=prompt.Top+(ph-32)/2;hear.Bounds=new Rectangle(prompt.Right+4,iconY,32,32);favorite.Bounds=new Rectangle((question!=null&&question.kind>=0&&(feedback||question.kind!=1)?hear.Right:prompt.Right)+4,iconY,32,32);
    status.Bounds=new Rectangle(18,prompt.Bottom+4,Math.Max(1,w-36),52);int y=status.Bottom+8;int needed=244;
    upper.Bounds=new Rectangle(0,0,w,needed);scene.Bounds=new Rectangle(0,needed,w,Math.Max(1,body.Height-needed));
    int bw=Math.Min(260,Math.Max(140,w/3));int inputY=y+25;submit.Bounds=new Rectangle(w-bw-18,inputY,bw,46);input.Bounds=new Rectangle(18,inputY+3,Math.Max(60,w-bw-54),42);definition.Bounds=new Rectangle(18,y,Math.Max(60,w-bw-54),104);
    for(int i=0;i<choices.Count;i++)choices[i].Bounds=new Rectangle(18+(i%2)*(w-30)/2,y+(i/2)*50,(w-42)/2,44);
   };
   body.Resize+=(sender,e)=>layout();refresh();
   input.TextChanged+=(sender,e)=>{if(question!=null&&question.kind==3){question.spellingDraft=input.Text;if(kind=="cut")state.cutDraft=input.Text;else state.cookDraft=input.Text;}};
   input.KeyDown+=(sender,e)=>{if(e.KeyCode==Keys.Enter){e.SuppressKeyPress=true;answer(-1);}};
   animation.Tick+=(sender,e)=>{scene.Motion=++ticks/34f;scene.Invalidate();if(ticks>=34){animation.Stop();scene.Animating=false;busy=false;submit.Enabled=input.Enabled=true;refresh();if(input.Visible)input.Focus();}};
   dialog.KeyDown+=(sender,e)=>{if(e.KeyCode==Keys.Escape)dialog.Close();else if(feedback&&e.KeyCode==Keys.Enter){e.Handled=e.SuppressKeyPress=true;answer(-1);}else if(!busy&&!feedback&&question!=null&&question.kind>=0&&question.kind<3&&e.KeyCode>=Keys.D1&&e.KeyCode<=Keys.D4){e.Handled=e.SuppressKeyPress=true;answer((int)e.KeyCode-(int)Keys.D1);}};
   dialog.Shown+=(sender,e)=>{if(input.Visible)input.Focus();};try{dialog.ShowDialog(this);}finally{Persist();animation.Dispose();scene.Dispose();if(refreshKitchenProgress!=null)refreshKitchenProgress();}
  }
 }

}

public sealed class KitchenRoomView:Control {
 public string WordProgress{get;set;}
 readonly Bitmap room;readonly KitchenWorkState state;readonly Action<string> interact;readonly Action back;
 readonly VNButton[] buttons=new VNButton[6];readonly VNButton exit;
 public static readonly PointF[] Stations={new PointF(.24f,.45f),new PointF(.51f,.48f),new PointF(.425f,.39f),new PointF(.53f,.20f),new PointF(.625f,.39f),new PointF(.75f,.43f)};
 public static readonly string[] Kinds={"shelf","cut","boil","bake","fry","plate"};
 static readonly string[] Labels={"食材架","切菜板","蒸煮锅","烘烤炉","煎制台","装盘台"};
 public KitchenRoomView(string root,KitchenWorkState value,Action<string> action,Action returnAction){state=value;interact=action;back=returnAction;DoubleBuffered=true;ResizeRedraw=true;TabStop=true;BackColor=Color.FromArgb(19,35,36);room=new Bitmap(Path.Combine(root,"assets/waystation/kitchen-woodfire-pixel-preview.png"));
  for(int i=0;i<buttons.Length;i++){int index=i;buttons[i]=new VNButton{Text=Labels[i],Name=Kinds[i],Font=GameTheme.Body(11)};buttons[i].Click+=(s,e)=>{interact(Kinds[index]);Invalidate();};Controls.Add(buttons[i]);}
  exit=new VNButton{Text="返回营业选项",Font=GameTheme.Body(11)};exit.Click+=(s,e)=>back();Controls.Add(exit);LayoutButtons();
 }
 RectangleF Viewport(){float ratio=(float)room.Width/room.Height;float w=Width,h=w/ratio;if(h>Height){h=Height;w=h*ratio;}return new RectangleF((Width-w)/2,(Height-h)/2,w,h);}
 void LayoutButtons(){if(room==null)return;var v=Viewport();int bw=Math.Max(86,(int)(v.Width*.085f)),bh=Math.Max(32,(int)(v.Height*.045f));for(int i=0;i<buttons.Length;i++)buttons[i].Bounds=new Rectangle((int)(v.X+Stations[i].X*v.Width)-bw/2,(int)(v.Y+Stations[i].Y*v.Height)-bh/2,bw,bh);exit.Bounds=new Rectangle(14,12,175,46);}
 protected override void OnResize(EventArgs e){base.OnResize(e);LayoutButtons();}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Escape){back();e.Handled=true;}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.DrawImage(room,Viewport());
  string held=state.held=="raw"?"胡萝卜":state.held=="chopped"?"已切胡萝卜":state.held=="finished"?"加工好的胡萝卜":"无";
  var header=new Rectangle(200,12,Math.Max(50,Width-215),46);GuildChrome.Draw(g,header);DrawText(g,"厨房工作 · 当前食材："+held+" · 已装盘 "+state.served+" 份",header,12,GuildChrome.Ivory);
  var hint=new Rectangle(20,Height-74,Math.Max(1,Width-40),60);GuildChrome.Draw(g,hint);DrawText(g,(WordProgress??"")+"\n拿取 → 切配 → 烹饪 → 拿取 → 装盘 · 与支线远征共享学习进度",hint,10,GuildChrome.Ivory);
 }
 internal static void DrawText(Graphics g,string text,Rectangle r,float size,Color color){using(var f=GameTheme.Body(size))GameTheme.DrawText(g,text,f,r,color,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak);}
 protected override void Dispose(bool disposing){if(disposing&&room!=null)room.Dispose();base.Dispose(disposing);}
}
public sealed class KitchenProcessView:Control {
 readonly Bitmap atlas,food;readonly string kind;readonly KitchenWorkState state;readonly Timer idle=new Timer{Interval=50};float time;public float Motion;public bool Animating;
 public KitchenProcessView(string root,string type,KitchenWorkState value){kind=type;state=value;atlas=new Bitmap(Path.Combine(root,"assets/kitchen/process-stations.png"));food=new Bitmap(Path.Combine(root,"assets/kitchen/carrot-stages.png"));DoubleBuffered=true;ResizeRedraw=true;idle.Tick+=(s,e)=>{time+=.05f;Invalidate();};idle.Start();}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;int q=kind=="cut"?0:kind=="boil"?1:kind=="bake"?2:3;g.DrawImage(atlas,ClientRectangle,new Rectangle(q%2*atlas.Width/2,q/2*atlas.Height/2,atlas.Width/2,atlas.Height/2),GraphicsUnit.Pixel);
  var old=g.Save();g.TranslateTransform(Width*.5f,Height*(kind=="boil"?.52f:kind=="bake"?.61f:kind=="fry"?.58f:.59f));float scale=Math.Min(Width/800f,Height/430f);g.ScaleTransform(scale,scale);int step=state.Progress(kind);int visible=Animating?Math.Max(0,step-1):step;
  if(kind=="cut"){if(visible==0)Carrot(g,-75,-10,150,26,false);else for(int i=0;i<4+visible*3;i++)Carrot(g,-100+i*19,-8+(i%2)*7,25,23,false);if(Animating){float motion=(float)Math.Abs(Math.Sin(Motion*Math.PI*8));using(var b=new SolidBrush(Color.FromArgb(215,219,207)))g.FillRectangle(b,-100+Motion*190,-80+motion*52,13,78);using(var b=new SolidBrush(Color.FromArgb(85,57,40)))g.FillRectangle(b,-100+Motion*190,-105+motion*52,13,30);}}
  else {for(int i=0;i<8;i++){float toss=kind=="fry"&&Animating?(float)Math.Sin(Motion*Math.PI)*25:0;Carrot(g,-80+(i%4)*42,-30+(i/4)*35-toss,28,20,step>=2);}if(kind=="boil"){using(var pen=new Pen(Color.FromArgb(180,206,225,219),3))for(int i=0;i<7;i++){float y=(float)Math.Sin(time*3+i)*8;g.DrawRectangle(pen,-90+i*29,-35+y,5,5);}}if(kind=="boil"||kind=="fry"){using(var brush=new SolidBrush(Color.FromArgb(105,224,235,214)))for(int i=0;i<4+step;i++){float lift=(time*24+i*21)%85;g.FillRectangle(brush,-75+i*24,-45-lift,8,13);}}if(kind=="bake"&&Animating){using(var brush=new SolidBrush(Color.FromArgb((int)(28+Math.Sin(Motion*Math.PI)*28),234,163,60)))g.FillRectangle(brush,-140,-90,280,160);}}
  g.Restore(old);var bar=new Rectangle(22,Height-52,Math.Max(1,Width-44),34);GuildChrome.Draw(g,bar);KitchenRoomView.DrawText(g,KitchenWords.Stages(kind)[step]+"　"+step+" / 3",bar,12,GuildChrome.Gold);
 }
 void Carrot(Graphics g,float x,float y,float w,float h,bool cooked){int cell=w>70?0:cooked?(kind=="boil"?2:3):1;float[] left={.018f,.33f,.565f,.81f},width={.30f,.165f,.17f,.17f};var source=new RectangleF(food.Width*left[cell],food.Height*.25f,food.Width*width[cell],food.Height*.43f);if(cell==0){y-=13;h+=26;}g.DrawImage(food,new RectangleF(x,y,w,h),source,GraphicsUnit.Pixel);}
 protected override void Dispose(bool disposing){if(disposing){idle.Dispose();atlas.Dispose();food.Dispose();}base.Dispose(disposing);}
}

public sealed class KitchenWordPrompt:Control{public KitchenWordPrompt(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);}protected override void OnPaint(PaintEventArgs e){GameTheme.DrawText(e.Graphics,Text,Font,new Rectangle(4,0,Math.Max(1,Width-8),Height),ForeColor,TextFormatFlags.WordBreak|TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);}protected override void OnTextChanged(EventArgs e){base.OnTextChanged(e);Invalidate();}}
