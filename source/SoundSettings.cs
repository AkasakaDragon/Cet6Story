using System;
using System.Windows.Forms;

public partial class Game {
 static int ClampVolume(int value){return Math.Max(0,Math.Min(100,value));}
 int MasterSoundVolume(){return ClampVolume(save.masterVolume??100);}
 int CombinedVolume(int category){return (int)Math.Round(ClampVolume(category)*MasterSoundVolume()/100.0);}
 int StorySoundVolume(){return CombinedVolume(save.volume);}
 int WordSoundVolume(){return CombinedVolume(save.wordVolume??save.volume);}
 int EffectSoundVolume(){return CombinedVolume(save.rogueEffectsVolume);}
 void NormalizeSoundSettings(){save.volume=ClampVolume(save.volume);save.rogueMusicVolume=ClampVolume(save.rogueMusicVolume);save.rogueEffectsVolume=ClampVolume(save.rogueEffectsVolume);save.masterVolume=MasterSoundVolume();save.wordVolume=ClampVolume(save.wordVolume??save.volume);}
 void ApplySoundSettings(){if(speech!=null)speech.Volume=StorySoundVolume();UpdateRogueAudio();}
 void AddAudioSlider(FlowLayoutPanel panel,string title,string detail,int value,Action<int> set){
  var label=Lab(title+"："+value+"%",13);panel.Controls.Add(label);
  panel.Controls.Add(Lab(detail,10,Muted));
  var slider=new TrackBar{Minimum=0,Maximum=100,Value=ClampVolume(value),Width=Math.Min(450,Math.Max(250,content.Width-100)),TickFrequency=10,BackColor=Bg,AccessibleName=title};
  slider.ValueChanged+=(s,e)=>{set(slider.Value);label.Text=title+"："+slider.Value+"%";ApplySoundSettings();Persist();};panel.Controls.Add(slider);
 }
 void AddAudioSettings(FlowLayoutPanel panel){
  panel.Controls.Add(Lab("声音设置",18,Accent));
  AddAudioSlider(panel,"总音量","统一控制游戏中所有声音。",MasterSoundVolume(),v=>save.masterVolume=v);
  AddAudioSlider(panel,"背景音乐（BGM）","主菜单待机音乐、背单词与战斗背景音乐。",save.rogueMusicVolume,v=>save.rogueMusicVolume=v);
  AddAudioSlider(panel,"游戏音效","按钮点击、划牌与出牌、攻击、受击、技能、金币、商店及过场音效。",save.rogueEffectsVolume,v=>save.rogueEffectsVolume=v);
  AddAudioSlider(panel,"剧情配音 / 听力","剧情台词、听力原音与剧情合成语音。",save.volume,v=>save.volume=v);
  AddAudioSlider(panel,"单词发音","单词、生词本及词汇练习中的英文发音。",save.wordVolume??save.volume,v=>save.wordVolume=v);
  panel.Controls.Add(Btn("试听游戏音效",PlayMenuClick));
  panel.Controls.Add(Lab("每项调至 0 可静音；总音量与分类音量共同生效，设置自动保存。\n单词发音时，背景音乐会自动降低。",10,Muted));
 }
}
