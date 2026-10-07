using System;using System.Collections.Generic;using System.Drawing;using System.Drawing.Imaging;using System.Linq;using System.Runtime.InteropServices;
// Recolour only: alpha, frame geometry and pixel positions are never modified.
public static class HeroActionPalette {
 static int Family(Color c){float s=c.GetSaturation(),h=c.GetHue();if(c.GetBrightness()<.17f)return 0;if(s<.23f)return 1;if(h<19||h>330)return 2;if(h<70)return s<.40f?3:4;return 5;}
 static int Light(Color c){return (c.R*54+c.G*183+c.B*19)/256;}
 public static Bitmap Match(Image actions,Image idle){
  var palette=new Dictionary<int,int>[6];var target=new int[6,256];var source=new int[6,256];for(int k=0;k<6;k++)palette[k]=new Dictionary<int,int>();
  using(var reference=new Bitmap(idle)){for(int y=0;y<reference.Height;y++)for(int x=0;x<reference.Width;x++){var c=reference.GetPixel(x,y);if(c.A<230)continue;int f=Family(c),rgb=c.ToArgb()|unchecked((int)0xff000000),n;palette[f].TryGetValue(rgb,out n);palette[f][rgb]=n+1;target[f,Light(c)]++;}}
  var result=new Bitmap(actions.Width,actions.Height,PixelFormat.Format32bppArgb);using(var g=Graphics.FromImage(result))g.DrawImageUnscaled(actions,0,0);
  var data=result.LockBits(new Rectangle(0,0,result.Width,result.Height),ImageLockMode.ReadWrite,PixelFormat.Format32bppArgb);var bytes=new byte[data.Stride*result.Height];Marshal.Copy(data.Scan0,bytes,0,bytes.Length);
  for(int y=0;y<result.Height;y++)for(int x=0;x<result.Width;x++){int i=y*data.Stride+x*4;if(bytes[i+3]<64)continue;var c=Color.FromArgb(bytes[i+2],bytes[i+1],bytes[i]);source[Family(c),Light(c)]++;}
  var ramps=new Color[6][];var mapping=new int[6,256];
  for(int f=0;f<6;f++){ramps[f]=palette[f].OrderByDescending(p=>p.Value).Take(128).Select(p=>Color.FromArgb(p.Key)).ToArray();long a=0,b=0;for(int l=0;l<256;l++){a+=source[f,l];b+=target[f,l];}long accum=0;for(int l=0;l<256;l++){accum+=source[f,l];double percentile=a==0?0:(accum-source[f,l]*.5)/a;long sum=0;int dest=0;while(dest<255){sum+=target[f,dest];if(sum>=percentile*b)break;dest++;}mapping[f,l]=dest;}}
  var all=ramps.SelectMany(r=>r).ToArray();for(int f=0;f<6;f++)if(ramps[f].Length==0)ramps[f]=all;
  var cache=new Dictionary<int,Color>();for(int y=0;y<result.Height;y++)for(int x=0;x<result.Width;x++){int i=y*data.Stride+x*4;if(bytes[i+3]==0)continue;var c=Color.FromArgb(bytes[i+2],bytes[i+1],bytes[i]);Color mapped;if(!cache.TryGetValue(c.ToArgb(),out mapped)){int f=Family(c);var choices=ramps[f];if(choices.Length==0){mapped=c;}else{double best=double.MaxValue;mapped=choices[0];foreach(var p in choices){double hue=Math.Abs(c.GetHue()-p.GetHue());hue=Math.Min(hue,360-hue);double dl=Light(p)-mapping[f,Light(c)],ds=(p.GetSaturation()-c.GetSaturation())*100;double score=dl*dl*4+ds*ds*.2+hue*hue*.08;if(score<best){best=score;mapped=p;}}}cache[c.ToArgb()]=mapped;}bytes[i]=mapped.B;bytes[i+1]=mapped.G;bytes[i+2]=mapped.R;}
  Marshal.Copy(bytes,0,data.Scan0,bytes.Length);result.UnlockBits(data);return result;
 }
}

