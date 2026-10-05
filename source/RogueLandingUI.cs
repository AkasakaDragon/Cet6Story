using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public partial class Game {
 Label ShowcaseLabel(string value,int size,Color color,ContentAlignment align=ContentAlignment.MiddleCenter){return new RetroLabel{Text=value,Font=GameTheme.Body(size),ForeColor=color,TextAlign=align,BackColor=Color.Transparent,AutoEllipsis=true};}
 Image RogueUiArt(string name){return CachedImage(Path.Combine(root,"assets","rogue","ui",name+".png"));}
 RogueShowcaseButton ShowcaseButton(string text,Action action,int width=170,int height=44){var b=new RogueShowcaseButton{Text=text,Art=RogueUiArt("amethyst-button"),Size=new Size(width,height),Font=GameTheme.Body(11),Cursor=Cursors.Hand};b.Click+=(s,e)=>action();return b;}
 CardRewardSurface RogueShowcase(Image background){ClearPage();page="rogue";rogueArena=null;var scene=new CardRewardSurface{Dock=DockStyle.Fill,Art=background,AutoScroll=false,CompositeChildren=true};content.Controls.Add(scene);return scene;}
 static Rectangle ShowcaseBounds(float scale,int offsetX,int offsetY,int x,int y,int width,int height){return new Rectangle(offsetX+(int)Math.Round(x*scale),offsetY+(int)Math.Round(y*scale),Math.Max(1,(int)Math.Round(width*scale)),Math.Max(1,(int)Math.Round(height*scale)));}
 static void PlaceShowcase(Control control,float scale,int offsetX,int offsetY,int x,int y,int width,int height,int fontSize=0){var button=control as RogueShowcaseButton;if(button!=null)button.VisualScale=scale;var card=control as RogueAdventureCard;if(card!=null)card.VisualScale=scale;var metric=control as RogueResultMetric;if(metric!=null)metric.VisualScale=scale;var panel=control as RogueShowcasePanel;if(panel!=null)panel.VisualScale=scale;if(fontSize>0){float target=Math.Max(6,fontSize*scale);if(Math.Abs(control.Font.Size-target)>.1f){var old=control.Font;control.Font=GameTheme.Body(target,old.Style);old.Dispose();}}control.Bounds=ShowcaseBounds(scale,offsetX,offsetY,x,y,width,height);control.Invalidate();}

 void RenderRogueLanding(){
  var p=save.rogue;bool busy=p.ActiveRun!=null&&p.ActiveRun.state!="ended";
  // Decode and inspect the artwork before replacing the visible page, so the first frame is complete.
  var background=RogueUiArt("adventure-crossroads");var frame=RogueUiArt("gothic-adventure-frame");var plaque=RogueUiArt("amethyst-plaque");var buttonArt=RogueUiArt("amethyst-button");
  RogueOrnateSprite.Prepare(plaque,buttonArt);
  var scene=RogueShowcase(background);
  var title=ShowcaseLabel("词域远征",27,Gold);scene.Controls.Add(title);
  var subtitle=ShowcaseLabel("选择你的冒险  ·  每局八层，学习进度永久保留",12,TextColor);scene.Controls.Add(subtitle);
  var stats=ShowcaseLabel("◆  金币 "+p.coins+"     ◇  通关 "+p.clears+" 次     ✦  正确作答 "+p.correct+" / "+p.answers,11,Accent);scene.Controls.Add(stats);
  var entries=new RogueAdventureCard[3];string[] modes={"基础训练","四级训练","六级挑战"};string[] notes={"从基础词汇出发","穿越四级词域","迎战六级挑战"};Color[] colors={Color.FromArgb(174,168,207),Color.FromArgb(174,168,207),Color.FromArgb(174,168,207)};
  for(int i=0;i<3;i++){
   string mode=modes[i];var pool=TrainingPool(mode);int practiced=0,mastered=0;foreach(var word in pool){RogueMemory m;if(!p.memory.TryGetValue(word.word,out m)||m==null)continue;if(m.correct>0||m.wrong>0)practiced++;if((m.mask&3)==3)mastered++;}
   var card=new RogueAdventureCard{Mode=mode,Note=notes[i],Index=i+1,Practiced=practiced,Mastered=mastered,Total=pool.Count,Accent=colors[i],Art=frame,Enabled=!busy,Cursor=busy?Cursors.Default:Cursors.Hand,AccessibleName=mode+"，已练习 "+practiced+"，已掌握 "+mastered};
   card.Click+=(s,e)=>StartRogue(mode);scene.Controls.Add(card);entries[i]=card;
  }
  var resume=new RogueShowcasePanel{Visible=busy,Art=plaque};scene.Controls.Add(resume);
  var resumeText=ShowcaseLabel(busy?"继续 "+p.ActiveRun.mode+"    ·    已完成 "+p.ActiveRun.depth+" / "+TowerEngine.FloorCount(p.ActiveRun)+" 节点    ·    生命 "+p.ActiveRun.hp+" / "+p.ActiveRun.maxHp:"",12,Gold,ContentAlignment.MiddleLeft);resume.Controls.Add(resumeText);
  var continueButton=ShowcaseButton("继续本局",()=>NavigateMenu(()=>{Persist();RenderRogue();},"继续本局",false),132,40);resume.Controls.Add(continueButton);
  var quitButton=ShowcaseButton("结束本局",()=>{if(GameMessage.Show(this,"结束当前远征？已获得的金币、词汇准备与学习记录保留。","结束远征",MessageBoxButtons.YesNo)==DialogResult.Yes){RogueEngine.Finish(p,false);SaveRogue();}},132,40);resume.Controls.Add(quitButton);
  var info=ShowcaseLabel("无提示答对两种选择题即可掌握单词  ·  答错的词自动加入生词本",10,Muted);scene.Controls.Add(info);
  var nav=new[]{new{Text="玩法说明",Run=(Action)(()=>GameMessage.Show(this,"点击亮起的地图节点向上推进。小怪掉落金币和卡牌；精英额外提供赋能；火堆可回血或强化攻击，宝箱和问号事件也有奖励。\n每词每日首次无提示答对可获得 3 金币。每回合先出牌，再回答单词题。答错的词会自动收藏。","远征玩法"))},new{Text="卡牌图鉴",Run=(Action)ShowCardCollection},new{Text="生词本",Run=(Action)ShowWords},new{Text="系统商店",Run=(Action)ShowSystemShop},new{Text="主界面",Run=(Action)ShowMain}};
  var buttons=nav.Select(n=>ShowcaseButton(n.Text,n.Run,112,40)).ToArray();foreach(var b in buttons)scene.Controls.Add(b);
  Action layout=()=>{
   const int logicalWidth=1280,logicalHeight=740,cardWidth=290,gap=25;float scale=Math.Min(scene.ClientSize.Width/(float)logicalWidth,scene.ClientSize.Height/(float)logicalHeight);if(scale<=0)return;int ox=(int)((scene.ClientSize.Width-logicalWidth*scale)/2),oy=(int)((scene.ClientSize.Height-logicalHeight*scale)/2);int full=cardWidth*3+gap*2,x=(logicalWidth-full)/2,top=busy?260:225,below=top+310+8;
   PlaceShowcase(title,scale,ox,oy,20,42,1240,70,27);PlaceShowcase(subtitle,scale,ox,oy,20,110,1240,30,12);PlaceShowcase(stats,scale,ox,oy,20,145,1240,28,11);
   PlaceShowcase(resume,scale,ox,oy,x,182,full,62);PlaceShowcase(resumeText,scale,0,0,20,8,full-325,43,12);PlaceShowcase(continueButton,scale,0,0,full-296,10,132,40,11);PlaceShowcase(quitButton,scale,0,0,full-153,10,132,40,11);
   for(int i=0;i<3;i++)PlaceShowcase(entries[i],scale,ox,oy,x+i*(cardWidth+gap),top,cardWidth,310);
   PlaceShowcase(info,scale,ox,oy,20,below,1240,29,10);
   int navX=(logicalWidth-buttons.Length*118)/2;for(int i=0;i<buttons.Length;i++)PlaceShowcase(buttons[i],scale,ox,oy,navX+i*118,below+35,112,40,11);
  };
  scene.Resize+=(s,e)=>layout();layout();
 }

 void RenderRogueEnding(RogueRun r){
  var p=save.rogue;bool prep=p.preparationActive;var scene=RogueShowcase(RogueUiArt("adventure-finale"));
  var panel=new RogueShowcasePanel{Art=RogueUiArt("gothic-result-frame"),CropFrame=true};scene.Controls.Add(panel);
  var eyebrow=ShowcaseLabel(prep?"本节词汇准备":r.mode,11,Accent);panel.Controls.Add(eyebrow);
  var title=ShowcaseLabel(r.won?"远征完成":"远征结束",29,Gold);panel.Controls.Add(title);
  var sub=ShowcaseLabel(r.won?"古界守门者已被击败":"这次旅程告一段落，学习成果已经保留",12,TextColor);panel.Controls.Add(sub);
  var divider=new Panel{BackColor=Color.FromArgb(91,118,130),Height=1};panel.Controls.Add(divider);
  string accuracy=r.answered==0?"—":(100.0*r.correct/r.answered).ToString("0")+"%";
  var plaque=RogueUiArt("amethyst-plaque");var metrics=new[]{new RogueResultMetric{Caption="完成节点",Value=r.depth+" / "+TowerEngine.FloorCount(r),Art=plaque},new RogueResultMetric{Caption="答题正确率",Value=accuracy,Art=plaque},new RogueResultMetric{Caption="本局获得金币",Value="+"+r.earnedCoins,Art=plaque}};foreach(var metric in metrics)panel.Controls.Add(metric);
  var detail=ShowcaseLabel("作答 "+r.answered+" 题  ·  答对 "+r.correct+" 题  ·  当前金币 "+p.coins+"  ·  剩余生命 "+r.hp+" / "+r.maxHp,11,TextColor);panel.Controls.Add(detail);
  var retained=ShowcaseLabel("金币、收藏与词汇准备已保留；本局攻击、护甲和赋能已结算。",10,Muted);panel.Controls.Add(retained);
  var wrong=ShowcaseLabel(r.wrongWords!=null&&r.wrongWords.Count>0?"待复习  ·  "+String.Join("  /  ",r.wrongWords.Take(8))+(r.wrongWords.Count>8?"  …":""):"本局没有待复习错词",11,r.wrongWords!=null&&r.wrongWords.Count>0?Gold:Accent);panel.Controls.Add(wrong);
  var back=ShowcaseButton("返回远征大厅",ShowRogueHome,174,46);panel.Controls.Add(back);
  var shop=ShowcaseButton("主线系统商店",ShowSystemShop,174,46);panel.Controls.Add(shop);
  var review=ShowcaseButton("复习错词",ShowWords,174,46);panel.Controls.Add(review);
  RogueShowcaseButton enter=null,restart=null,map=null;Label prepNote=null;
  if(prep){enter=ShowcaseButton("进入本节剧情",ShowStory,174,46);enter.Enabled=current!=null&&PreparationReady(current);panel.Controls.Add(enter);restart=ShowcaseButton("再来一局",StartPreparation,174,46);panel.Controls.Add(restart);map=ShowcaseButton("返回主线地图",ShowStoryMap,174,46);panel.Controls.Add(map);prepNote=ShowcaseLabel(enter.Enabled?"本节词汇已准备完成":"本节词汇尚未准备完成，可以再来一局继续积累进度。",10,enter.Enabled?Accent:Muted);panel.Controls.Add(prepNote);}
  Action layout=()=>{
   const int logicalWidth=1280,logicalHeight=740,pw=1070;float scale=Math.Min(scene.ClientSize.Width/(float)logicalWidth,scene.ClientSize.Height/(float)logicalHeight);if(scale<=0)return;int ox=(int)((scene.ClientSize.Width-logicalWidth*scale)/2),oy=(int)((scene.ClientSize.Height-logicalHeight*scale)/2),ph=prep?690:630,x=(logicalWidth-pw)/2,y=(logicalHeight-ph)/2;
   PlaceShowcase(panel,scale,ox,oy,x,y,pw,ph);PlaceShowcase(eyebrow,scale,0,0,65,ph*18/100,pw-130,25,11);PlaceShowcase(title,scale,0,0,65,ph*22/100,pw-130,52,29);PlaceShowcase(sub,scale,0,0,65,ph*31/100,pw-130,28,12);PlaceShowcase(divider,scale,0,0,95,ph*39/100,pw-190,1);
   int metricWidth=(pw-240)/3,metricGap=18,metricX=(pw-3*metricWidth-2*metricGap)/2,metricY=ph*41/100,metricHeight=Math.Max(76,ph*14/100);for(int i=0;i<3;i++)PlaceShowcase(metrics[i],scale,0,0,metricX+i*(metricWidth+metricGap),metricY,metricWidth,metricHeight);
   PlaceShowcase(detail,scale,0,0,60,ph*(prep?55:59)/100,pw-120,27,11);PlaceShowcase(retained,scale,0,0,60,ph*(prep?59:64)/100,pw-120,27,10);PlaceShowcase(wrong,scale,0,0,65,ph*(prep?63:69)/100,pw-130,27,11);
   int buttonWidth=190,buttonGap=17,buttonX=(pw-3*buttonWidth-2*buttonGap)/2,buttonY=ph*(prep?70:74)/100;PlaceShowcase(back,scale,0,0,buttonX,buttonY,buttonWidth,48,11);PlaceShowcase(shop,scale,0,0,buttonX+buttonWidth+buttonGap,buttonY,buttonWidth,48,11);PlaceShowcase(review,scale,0,0,buttonX+2*(buttonWidth+buttonGap),buttonY,buttonWidth,48,11);
   if(prep){PlaceShowcase(prepNote,scale,0,0,60,ph*67/100,pw-120,24,10);PlaceShowcase(enter,scale,0,0,buttonX,ph*78/100,buttonWidth,46,11);PlaceShowcase(restart,scale,0,0,buttonX+buttonWidth+buttonGap,ph*78/100,buttonWidth,46,11);PlaceShowcase(map,scale,0,0,buttonX+2*(buttonWidth+buttonGap),ph*78/100,buttonWidth,46,11);}
  };scene.Resize+=(s,e)=>layout();layout();
 }
}

public sealed class RogueShowcasePanel:Panel {
 public Image Art;public bool CropFrame;public float VisualScale=1;
 public RogueShowcasePanel(){DoubleBuffered=true;SetStyle(ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;}
 protected override void OnPaintBackground(PaintEventArgs e){base.OnPaintBackground(e);if(Art==null)return;var state=e.Graphics.Save();e.Graphics.ScaleTransform(VisualScale,VisualScale);var target=new Rectangle(0,0,(int)Math.Round(Width/VisualScale),(int)Math.Round(Height/VisualScale));if(!CropFrame)RogueOrnateSprite.DrawWide(e.Graphics,Art,target);else{e.Graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;var source=new Rectangle(8,18,Art.Width-16,Art.Height-78);e.Graphics.DrawImage(Art,target,source,GraphicsUnit.Pixel);}e.Graphics.Restore(state);}
}

public sealed class RogueShowcaseButton:Button {
 bool hover;
 public Image Art;public float VisualScale=1;
 public RogueShowcaseButton(){DoubleBuffered=true;SetStyle(ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;TabStop=true;}
 protected override void OnMouseEnter(EventArgs e){hover=true;Invalidate();base.OnMouseEnter(e);}
 protected override void OnMouseLeave(EventArgs e){hover=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;int width=(int)Math.Round(Width/VisualScale),height=(int)Math.Round(Height/VisualScale);var bounds=new Rectangle(0,0,width,height);var state=g.Save();g.ScaleTransform(VisualScale,VisualScale);if(Art!=null)RogueOrnateSprite.Draw(g,Art,bounds,true,true);else RogueStoneSkin.Draw(g,bounds,hover||Focused,hover||Focused);if(hover||Focused)using(var glow=new Pen(Color.FromArgb(120,177,172,208),2))g.DrawRectangle(glow,3,3,width-7,height-7);if(!Enabled)using(var shade=new SolidBrush(Color.FromArgb(100,8,8,16)))g.FillRectangle(shade,bounds);using(var brush=new SolidBrush(Enabled?Color.White:Color.FromArgb(158,161,158)))using(var font=GameTheme.Body(Math.Max(6,Font.Size/VisualScale),Font.Style)){var sf=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center};GameTheme.DrawPixelString(g,Text,font,brush,new RectangleF(12,5,width-24,height-10),sf);}g.Restore(state);}
}

public sealed class RogueResultMetric:Control {
 public string Caption="",Value="";public Image Art;public float VisualScale=1;
 public RogueResultMetric(){DoubleBuffered=true;SetStyle(ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;}
 protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;int width=(int)Math.Round(Width/VisualScale),height=(int)Math.Round(Height/VisualScale);var bounds=new Rectangle(0,0,width,height);var state=g.Save();g.ScaleTransform(VisualScale,VisualScale);if(Art!=null)RogueOrnateSprite.Draw(g,Art,bounds,true,true);else RogueStoneSkin.Draw(g,bounds);g.SmoothingMode=SmoothingMode.AntiAlias;using(var label=GameTheme.Body(10))using(var value=GameTheme.Body(24,FontStyle.Bold))using(var muted=new SolidBrush(Color.FromArgb(230,228,236)))using(var gold=new SolidBrush(Color.FromArgb(255,218,151))){var center=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center};GameTheme.DrawPixelString(g,Caption,label,muted,new RectangleF(9,height*.16f,width-18,height*.26f),center);GameTheme.DrawPixelString(g,Value,value,gold,new RectangleF(9,height*.38f,width-18,height*.48f),center);}g.Restore(state);}
}

public sealed class RogueAdventureCard:Control {
 public string Mode="",Note="";public int Index,Practiced,Mastered,Total;public Color Accent=Color.CadetBlue;public Image Art;public float VisualScale=1;
 bool hovered;
 public RogueAdventureCard(){DoubleBuffered=true;ResizeRedraw=true;TabStop=true;SetStyle(ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;}
 protected override void OnMouseEnter(EventArgs e){hovered=true;Invalidate();base.OnMouseEnter(e);}
 protected override void OnMouseLeave(EventArgs e){hovered=false;Invalidate();base.OnMouseLeave(e);}
 protected override void OnGotFocus(EventArgs e){Invalidate();base.OnGotFocus(e);}
 protected override void OnLostFocus(EventArgs e){Invalidate();base.OnLostFocus(e);}
 protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space){OnClick(EventArgs.Empty);e.Handled=true;}base.OnKeyDown(e);}
 protected override void OnPaint(PaintEventArgs e){
  var g=e.Graphics;var state=g.Save();g.ScaleTransform(VisualScale,VisualScale);g.SmoothingMode=SmoothingMode.AntiAlias;var c=Enabled?Accent:Color.FromArgb(107,119,124);int w=(int)Math.Round(Width/VisualScale),h=(int)Math.Round(Height/VisualScale);var bounds=new Rectangle(0,0,w,h);
  if(Art!=null)RogueOrnateSprite.Draw(g,Art,bounds);else{using(var fill=new LinearGradientBrush(bounds,Color.FromArgb(238,19,39,53),Color.FromArgb(246,8,18,29),LinearGradientMode.Vertical))g.FillRectangle(fill,bounds);using(var pen=new Pen(Color.FromArgb(hovered||Focused?210:115,c),hovered||Focused?3:2))g.DrawRectangle(pen,2,2,w-5,h-5);}
  if(hovered||Focused)using(var glow=new Pen(Color.FromArgb(160,c),2))g.DrawRectangle(glow,7,7,w-15,h-15);
  var center=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center};int emblem=Math.Max(58,h/4),emblemY=h*19/100;
  using(var halo=new SolidBrush(Color.FromArgb(35,c)))g.FillEllipse(halo,(w-emblem)/2,emblemY,emblem,emblem);
  using(var ring=new Pen(Color.FromArgb(180,c),2))g.DrawEllipse(ring,(w-emblem)/2,emblemY,emblem,emblem);
  using(var symbol=GameTheme.Body(Math.Max(25,emblem/2),FontStyle.Bold))using(var ink=new SolidBrush(c))GameTheme.DrawPixelString(g,Index==1?"Ⅰ":Index==2?"Ⅱ":"Ⅲ",symbol,ink,new RectangleF((w-emblem)/2,emblemY,emblem,emblem),center);
  int titleY=h*47/100;using(var name=GameTheme.Body(17,FontStyle.Bold))using(var ink=new SolidBrush(Color.FromArgb(245,222,175)))GameTheme.DrawPixelString(g,Mode,name,ink,new RectangleF(20,titleY,w-40,32),center);
  using(var text=GameTheme.Body(10))using(var ink=new SolidBrush(Color.FromArgb(185,198,215)))GameTheme.DrawPixelString(g,Note,text,ink,new RectangleF(20,h*58/100,w-40,23),center);
  using(var text=GameTheme.Body(9))using(var ink=new SolidBrush(Color.FromArgb(231,228,237))){GameTheme.DrawPixelString(g,"已练习  "+Practiced+" / "+Total,text,ink,new RectangleF(23,h*68/100,w-46,22),center);GameTheme.DrawPixelString(g,"已掌握  "+Mastered+" / "+Total,text,ink,new RectangleF(23,h*75/100,w-46,22),center);}
  int progressY=h*85/100;using(var track=new SolidBrush(Color.FromArgb(58,66,86)))g.FillRectangle(track,26,progressY,w-52,4);
  if(Total>0)using(var progress=new SolidBrush(c))g.FillRectangle(progress,26,progressY,(int)((w-52)*(double)Mastered/Total),4);
  g.Restore(state);
 }
}
