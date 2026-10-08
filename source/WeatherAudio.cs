using System;
using System.IO;

public sealed partial class RogueAudioMixer {
 short[] rain;int rainPosition;double rainGain;
 public volatile bool RainEnabled;
 void LoadRain(string folder){string path=Path.GetFullPath(Path.Combine(folder,"..","..","audio","weather","rain-moderate-preview.wav"));if(File.Exists(path))rain=Read(path);}
 // The rain recording is 8.3 dB quieter than the music; this gain leaves
 // it roughly 10 dB below music, without applying an excessive second reduction.
 // Rain follows music volume and ducking; effects/master mute still applies.
 double MixRain(int channel,double musicGain,double effectsGain){
  if(rain==null)return 0;
  if(channel==0){double target=RainEnabled?Math.Min(musicGain,effectsGain)*.85:0;if(musicGain==0||effectsGain==0)rainGain=0;else rainGain+=(target-rainGain)*.00008;}
  int crossfade=Math.Min(13230,rain.Length/4);crossfade-=crossfade%2;
  double value=rain[rainPosition+channel];
  if(rainPosition>=rain.Length-crossfade){int start=rainPosition-(rain.Length-crossfade);double blend=(double)start/crossfade;value=value*(1-blend)+rain[start+channel]*blend;}
  if(channel==1){rainPosition+=2;if(rainPosition>=rain.Length)rainPosition=crossfade;}
  return value*rainGain;
 }
}
