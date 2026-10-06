using System;
using System.IO;
using System.Drawing;
using System.Linq;

public partial class Game {
 Image HeroSkillImage(string hero,string skill){
  string key="hero-skill:"+hero+":"+skill;Image picture;if(imageCache.TryGetValue(key,out picture))return picture;
  string path=Path.Combine(root,"assets","skills",hero+"-"+skill+".png");
  if(File.Exists(path))picture=CachedImage(path);
  else if(hero=="aelia"){
   string[] tiles={"bleed","bash","guard","sweep","execute","advance","counter","break","recover","stand"};int i=Array.IndexOf(tiles,skill);if(i<0)return null;
   var sheet=CachedImage(Path.Combine(root,"assets","skills","aelia-sheet.png"));int[] x={3,311,618,925,1233};var bounds=new Rectangle(x[i%5],i<5?100:540,300,375);picture=new Bitmap(bounds.Width,bounds.Height);using(var g=Graphics.FromImage(picture)){g.DrawImage(sheet,new Rectangle(0,0,picture.Width,picture.Height),bounds,GraphicsUnit.Pixel);}
  }else return null;
  imageCache[key]=picture;return picture;
 }
}

