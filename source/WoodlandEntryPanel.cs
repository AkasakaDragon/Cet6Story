using System;using System.Drawing;using System.Windows.Forms;using System.Collections.Generic;
public sealed class WoodlandEntryPanel:BattleGlassPanel {
 readonly bool continuing;readonly OutlinedLabel title,intro,heading,rule;readonly List<VNButton> modes=new List<VNButton>();readonly VNButton enter,secondary,back;public string VocabularyMode{get;private set;}
 public WoodlandEntryPanel(bool active,string description,string mode,Action<string> chooseMode,Action start,Action configureOrAbandon,Action returnToTavern){
  continuing=active;VocabularyMode=mode;Size=new Size(620,440);
  title=Label("支线远征 · 剧毒林地",GuildChrome.Gold);
  intro=Label(description,GuildChrome.Ivory);
  heading=Label(active?"训练词库 · "+mode:"选择训练词库",GuildChrome.Gold);
  rule=Label("连对3次退场 · 选择与拼写交替\n间隔复习 · 跨战斗保存学习进度",GuildChrome.Muted);
  if(!active)foreach(var name in new[]{"四级词汇","六级词汇","四六级混合"}){
   var selected=name;var button=Button(name,()=>{if(VocabularyMode==selected)return;VocabularyMode=selected;foreach(var item in modes){item.Active=item.Text==selected;item.AccessibleName=item.Text+(item.Active?" · 已选择":"");item.Invalidate();}chooseMode(selected);});button.Active=name==mode;button.AccessibleName=name+(button.Active?" · 已选择":"");modes.Add(button);
  }
  enter=Button(active?"继续上次远征":"整备完毕 · 出发",start);
  secondary=Button(active?"放弃上次远征":"配置男女主技能",configureOrAbandon);
  back=Button("返回驿站",returnToTavern);Arrange();
 }
 OutlinedLabel Label(string text,Color color){var label=new OutlinedLabel{Text=text,ForeColor=color};Controls.Add(label);return label;}
 VNButton Button(string text,Action click){var button=new VNButton{Text=text,PixelStyle=true};button.Click+=(s,e)=>click();Controls.Add(button);return button;}
 protected override void OnResize(EventArgs e){base.OnResize(e);if(title!=null&&back!=null)Arrange();}
 void Place(Control control,int x,int y,int width,int height,float size,bool bold=false){control.Bounds=new Rectangle(x*Width/620,y*Height/440,width*Width/620,height*Height/440);float fontSize=size*Math.Min(1,Math.Min(Width/620f,Height/440f));var style=bold?FontStyle.Bold:FontStyle.Regular;if(Math.Abs(control.Font.Size-fontSize)<.01f&&control.Font.Style==style)return;var previous=control.Font;control.Font=GameTheme.Body(fontSize,style);previous.Dispose();}
 void Arrange(){
  Place(title,28,24,564,46,22);Place(intro,28,84,564,98,12);
  Place(heading,28,continuing?192:190,564,26,11,true);
  Place(rule,28,continuing?224:268,564,46,10);
  if(continuing){Place(enter,28,286,564,48,12);Place(secondary,28,346,274,44,11);Place(back,318,346,274,44,11);}
  else{for(int i=0;i<modes.Count;i++)Place(modes[i],28+i*192,222,180,38,11);Place(enter,28,326,274,48,12);Place(secondary,318,326,274,48,11);Place(back,218,388,184,36,10);}
 }
}
