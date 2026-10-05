using System;
using System.Drawing;
using System.IO;

public partial class RogueArena {
 public Image[] SpearFrames;
 Image SpearFrame(Image fallback,bool hit){if(Run==null||MonsterCombat.Profile(Run)!=7||SpearFrames==null )return fallback;if(!hit||MonsterCombat.Action(Run)=="none"||MonsterCombat.Action(Run)=="skill-a")return SpearFrames[0];int index=frame<24?0:frame<28?1:frame<31?2:frame<34?3:frame<36?4:frame<38?5:frame<43?6:7;return SpearFrames[index];}
 Rectangle SpearPose(Rectangle original,Rectangle hero,int frame){if(MonsterCombat.Action(Run)=="none"||MonsterCombat.Action(Run)=="skill-a"||frame<24||frame>47)return MonsterCombat.Pose(Run,original,frame);float t=frame<34?(frame-24)/10f:frame<38?1:Math.Max(0,(47-frame)/9f);float ease=t*t*(3-2*t);int target=hero.Right-hero.Width/4;original.X-=(int)((original.Left-target)*ease);if(frame>=24&&frame<32)original.Y-=(int)(Math.Abs(Math.Sin((frame-24)*1.6))*4);return original;}
}
public partial class Game {
 Image[] LoadSpearFrames(){string file=Path.Combine(root,"assets","rogue","combat","spear-knight-thrust.png");if(!File.Exists(file))return null;const string key="spear-thrust:";Image image;if(!imageCache.TryGetValue(key+0,out image)){var frames=SpearKnightSprites.Extract(CachedImage(file));for(int i=0;i<8;i++)imageCache[key+i]=frames[i];}var result=new Image[8];for(int i=0;i<8;i++)result[i]=imageCache[key+i];return result;}
}

public static class SpearKnightSprites {
 // Extract connected figures rather than grid cells: the extended spear and
 // rear boot can extend beyond the nominal cell without being cropped.
 public static Image[] Extract(Image atlas){
  using(var src=new Bitmap(atlas)){
   int w=src.Width,h=src.Height;var seen=new bool[w*h];var groups=new System.Collections.Generic.List<System.Collections.Generic.List<int>>();var queue=new System.Collections.Generic.Queue<int>();
   for(int y=0;y<h;y++)for(int x=0;x<w;x++){int p=y*w+x;if(seen[p]||src.GetPixel(x,y).A<32)continue;var group=new System.Collections.Generic.List<int>();seen[p]=true;queue.Enqueue(p);while(queue.Count>0){int a=queue.Dequeue();group.Add(a);int ax=a%w,ay=a/w;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=ax+dx,ny=ay+dy;if(nx<0||nx>=w||ny<0||ny>=h)continue;int n=ny*w+nx;if(!seen[n]){seen[n]=true;if(src.GetPixel(nx,ny).A>=32)queue.Enqueue(n);}}}if(group.Count>1000)groups.Add(group);}
   groups.Sort((a,b)=>b.Count.CompareTo(a.Count));if(groups.Count<8)throw new InvalidDataException("Spear animation requires eight complete figures.");groups=groups.GetRange(0,8);groups.Sort((a,b)=>{int ar=a[0]/w<h/2?0:1,br=b[0]/w<h/2?0:1;return ar!=br?ar.CompareTo(br):(a[0]%w).CompareTo(b[0]%w);});
   var boxes=new Rectangle[8];int fw=0,fh=0;for(int i=0;i<8;i++){int l=w,t=h,r=0,b=0;foreach(int p in groups[i]){l=Math.Min(l,p%w);t=Math.Min(t,p/w);r=Math.Max(r,p%w);b=Math.Max(b,p/w);}boxes[i]=Rectangle.FromLTRB(l,t,r+1,b+1);fw=Math.Max(fw,r-l+5);fh=Math.Max(fh,b-t+5);}
   var result=new Image[8];for(int i=0;i<8;i++){var cell=new Bitmap(fw,fh);var box=boxes[i];int ox=(fw-box.Width)/2-box.Left,oy=fh-2-box.Bottom;foreach(int p in groups[i])cell.SetPixel(p%w+ox,p/w+oy,src.GetPixel(p%w,p/w));result[i]=cell;}return result;
  }
 }
}
