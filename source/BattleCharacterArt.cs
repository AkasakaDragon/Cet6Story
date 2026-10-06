using System.Drawing;
using System.IO;

public partial class Game {
 Image BattleCharacter(string name){
  string path=Path.Combine(root,"assets","rogue","combat",name+".png");if(!File.Exists(path))return null;
  string key="battle-character:"+name;Image cached;if(imageCache.TryGetValue(key,out cached))return cached;
  using(var bitmap=new Bitmap(CachedImage(path))){int left=bitmap.Width,top=bitmap.Height,right=-1,bottom=-1;
   for(int y=0;y<bitmap.Height;y++)for(int x=0;x<bitmap.Width;x++)if(bitmap.GetPixel(x,y).A>=48){left=System.Math.Min(left,x);top=System.Math.Min(top,y);right=System.Math.Max(right,x);bottom=System.Math.Max(bottom,y);}
   cached=right>=left?bitmap.Clone(Rectangle.FromLTRB(left,top,right+1,bottom+1),System.Drawing.Imaging.PixelFormat.Format32bppArgb):new Bitmap(bitmap);
  }
  // Match the approved muted battle palette while keeping the extracted pixels and alpha.
  var toned=new Bitmap(cached.Width,cached.Height,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
  using(var g=Graphics.FromImage(toned))using(var attributes=new System.Drawing.Imaging.ImageAttributes()){
   float[] luma={.2126f,.7152f,.0722f},tint={.94f,.95f,.91f};var rows=new float[5][];
   for(int input=0;input<3;input++){rows[input]=new float[5];for(int output=0;output<3;output++)rows[input][output]=(.18f*luma[input]+(input==output?.82f:0))*tint[output];}
   rows[3]=new[]{0f,0f,0f,1f,0f};rows[4]=new[]{0f,0f,0f,0f,1f};attributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(rows));
   g.DrawImage(cached,new Rectangle(0,0,cached.Width,cached.Height),0,0,cached.Width,cached.Height,GraphicsUnit.Pixel,attributes);
  }cached.Dispose();imageCache[key]=toned;return toned;
 }
 Image BattleHero(){return BattleCharacter("hero-male")??RogueHero();}
 Image[] BattleHeroFrames(string fallback){var hero=BattleCharacter("hero-male");if(hero==null)return CombatFrames(fallback,false);var frames=new Image[8];for(int i=0;i<frames.Length;i++)frames[i]=hero;return frames;}
}
