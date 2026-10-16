using System;using System.IO;using System.Collections.Generic;
public sealed partial class RogueAudioMixer {
 short[] millMusic,millWater;readonly Dictionary<string,short[]> millEffects=new Dictionary<string,short[]>();
 public volatile bool MillFlow=true;
 public void LoadMillAudio(string folder){lock(audioGate){if(millMusic!=null)return;
  millMusic=Read(Path.Combine(folder,"mill-bgm.wav"));millWater=Read(Path.Combine(folder,"mill-water.wav"));
  foreach(string key in new[]{"door","wood","ceramic","loot","lever","step-stone","step-wood","switch"})millEffects[key]=Read(Path.Combine(folder,key+".wav"));
 }}
 public void MillEffect(string key){lock(audioGate){short[] clip;if(!disposed&&active&&EffectsVolume>0&&MasterVolume>0&&millEffects.TryGetValue(key,out clip)){if(voices.Count>=8)voices.RemoveAt(0);voices.Add(new Voice{clip=clip});}}}
 double MixMillWater(int channel,double fx){return musicTrack==3&&millWater!=null?millWater[(position+channel)%millWater.Length]*fx*(MillFlow?.46:.08):0;}
}
