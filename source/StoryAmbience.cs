using System;using System.IO;using System.Text;
public partial class Game {
 string storyAmbienceScene="";bool storyAmbienceClinkPlayed;int storyAmbienceVolume=-1;
 string CurrentStoryAmbienceScene(){if(page!="story"||current==null||current.id!="tavern-01-01"||englishText==null||englishText.IsDisposed)return "";return index<10?"morning-room":index<22?"kitchen":"stone-road";}
 void StopStoryAmbience(){mciSendString("close sceneambience",null,0,IntPtr.Zero);mciSendString("close sceneclink",null,0,IntPtr.Zero);storyAmbienceScene="";storyAmbienceVolume=-1;storyAmbienceClinkPlayed=false;}
 void UpdateStoryAmbience(){
  string scene=CurrentStoryAmbienceScene();if(scene==""){if(storyAmbienceScene!="")StopStoryAmbience();return;}
  int volume=Math.Max(0,Math.Min(100,save.rogueMusicVolume))*MasterSoundVolume()/100;
  if(scene!=storyAmbienceScene||storyAmbienceVolume!=volume){if(scene!=storyAmbienceScene)mciSendString("close sceneclink",null,0,IntPtr.Zero);mciSendString("close sceneambience",null,0,IntPtr.Zero);string path=Path.Combine(root,"chapters","audio","tavern","chapter-one","ambience-"+scene+".wav");if(scene=="stone-road"&&!File.Exists(path))return;path=AudioVolume.Prepare(path,volume,root);
   if(scene=="stone-road"&&mciSendString("open \""+AudioDevicePath.Relative(path)+"\" type waveaudio alias sceneambience",null,0,IntPtr.Zero)!=0)return;storyAmbienceScene=scene;storyAmbienceVolume=volume;
   if(scene=="morning-room"&&!storyAmbienceClinkPlayed){storyAmbienceClinkPlayed=true;string clink=Path.Combine(root,"chapters","audio","tavern","chapter-one","ambience-distant-clink-once.wav");if(File.Exists(clink)&&mciSendString("open \""+AudioDevicePath.Relative(AudioVolume.Prepare(clink,volume,root))+"\" type waveaudio alias sceneclink",null,0,IntPtr.Zero)==0)mciSendString("play sceneclink from 0",null,0,IntPtr.Zero);}
  }
  if(scene!="stone-road")return;var mode=new StringBuilder(32);mciSendString("status sceneambience mode",mode,32,IntPtr.Zero);if(mode.ToString().Trim()=="stopped")mciSendString("play sceneambience from 0",null,0,IntPtr.Zero);
 }
}
