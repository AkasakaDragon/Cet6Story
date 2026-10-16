using System;using System.IO;using System.Windows.Forms;
public partial class Game {
 float millVillageReturnX=400;
 void ShowMillDungeon(){
  ClearPage();page="mill-dungeon";
  try {
   if(rogueAudio==null)rogueAudio=new RogueAudioMixer(Path.Combine(root,"assets","rogue","audio"));
   rogueAudio.LoadMillAudio(Path.Combine(root,"assets","dungeon","mill","audio"));
   var dungeon=new MillDungeonScene(root){Dock=DockStyle.Fill};
   dungeon.Return=ShowMaixiVillage;
   dungeon.Sound=key=>{if(rogueAudio!=null)rogueAudio.MillEffect(key);};
   dungeon.FlowChanged=on=>{if(rogueAudio!=null)rogueAudio.MillFlow=on;};
   rogueAudio.MillFlow=dungeon.State.waterOn;
   content.Controls.Add(dungeon);dungeon.Focus();UpdateRogueAudio();
  }catch(Exception ex){GameMessage.Show(this,"磨坊试玩未能加载："+ex.Message,"加载失败");ShowMaixiVillage();}
 }
}
