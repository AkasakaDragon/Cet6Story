using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

public partial class RogueArena {
 readonly Stopwatch cardEffectClock=new Stopwatch();readonly Timer cardEffectTimer=new Timer{Interval=16};string activeCardEffect;
 public Image[][] CastFrames;public Action<int> CastSound;int castingStyle;
 bool cardEffectBound;public int SceneRevision{get{return frame+(activeCardEffect==null?0:100+(int)(CardEffectAge*1000));}}
 bool CardEffectActive{get{return activeCardEffect!=null&&Mode=="combat"&&cardEffectClock.Elapsed.TotalMilliseconds<720;}}
 float CardEffectAge{get{return Math.Min(1,(float)cardEffectClock.Elapsed.TotalMilliseconds/720);}}
 public void PlayCardEffect(string id){if(String.IsNullOrEmpty(id))return;activeCardEffect=id;castingStyle=CardBattle.Get(id).Archetype=="燃烧过载"?2:CardBattle.Get(id).Archetype=="破绽连击"?0:1;if(CastSound!=null)CastSound(castingStyle);cardEffectClock.Restart();if(!cardEffectBound){cardEffectBound=true;cardEffectTimer.Tick+=(s,e)=>{if(!CardEffectActive){activeCardEffect=null;cardEffectTimer.Stop();}Invalidate();};}cardEffectTimer.Start();Invalidate();}
 float CardStep(){return 0;}
 Image CastingPose(Image fallback){if(CastFrames==null||CastFrames[castingStyle]==null)return fallback;if(!CardEffectActive)return CastFrames[castingStyle][0];int index=Math.Min(CastFrames[castingStyle].Length-1,(int)(CardEffectAge*CastFrames[castingStyle].Length));return CastFrames[castingStyle][index];}
 float CastingWidth(float height){float width=0;if(CastFrames!=null)foreach(var set in CastFrames)width=Math.Max(width,MaxSpriteWidth(set,height,null));return width;}
}
public partial class Game {
 Image[][] LoadCastingFrames(){var result=new Image[3][];for(int style=0;style<3;style++){string file=Path.Combine(root,"assets","rogue","combat","cast-"+style+".png");if(!File.Exists(file))continue;string prefix="casting:"+style+":";Image cached;if(!imageCache.TryGetValue(prefix+0,out cached)){var frames=CastingSprites.Smooth(CastingSprites.Extract(CachedImage(file)));for(int i=0;i<frames.Length;i++)imageCache[prefix+i]=frames[i];}result[style]=new Image[8];for(int i=0;i<8;i++)result[style][i]=imageCache[prefix+i];}return result;}
}