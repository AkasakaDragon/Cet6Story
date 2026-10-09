using System;using System.Drawing;using System.Windows.Forms;using System.Collections.Generic;
public partial class Game {
 Action refreshKitchenProgress;
 void AddKitchenModeButtons(KitchenRoomView view){refreshKitchenProgress=()=>{if(!view.IsDisposed){view.WordProgress=KitchenState().wordMode+" · "+VocabularyProgressText(KitchenState().wordMode);view.Invalidate();}};refreshKitchenProgress();}
 void ShowKitchenVocabularySelection(){
  string chosen=null;
  using(var dialog=SpellDialog("厨房工作 · 选择词库",700,440)){
   var body=new Panel{Dock=DockStyle.Fill,BackColor=dialog.BackColor};dialog.Controls.Add(body);
   var title=new OutlinedLabel{Text="选择本次厨房词库 · 与支线远征共享学习进度",Font=GameTheme.Body(12),ForeColor=GuildChrome.Ivory};body.Controls.Add(title);
   var buttons=new List<VNButton>();foreach(string mode in KitchenVocabulary.Modes){string selected=mode;var button=PartyButton(mode+"\n"+VocabularyProgressText(mode),()=>{chosen=selected;dialog.Close();},560,68);button.Font=GameTheme.Body(11);button.Active=mode==KitchenState().wordMode;buttons.Add(button);body.Controls.Add(button);}
   Action layout=()=>{int w=body.Width;title.Bounds=new Rectangle(0,0,w,48);int gap=8;int height=Math.Min(72,Math.Max(48,(body.Height-58-gap*2)/3));for(int i=0;i<buttons.Count;i++)buttons[i].Bounds=new Rectangle(0,58+i*(height+gap),w,height);};body.Resize+=(s,e)=>layout();layout();dialog.ShowDialog(this);
  }
  if(chosen==null)return;KitchenState().wordMode=chosen;Persist();ShowKitchenWork();
 }
}
