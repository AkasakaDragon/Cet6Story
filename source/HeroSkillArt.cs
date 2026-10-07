using System;
using System.IO;
using System.Drawing;
using System.Linq;

public partial class Game {
 Image HeroSkillImage(string hero,string skill){
  string key="hero-skill:"+hero+":"+skill;Image picture;if(imageCache.TryGetValue(key,out picture))return picture;
  string[] concepts=hero=="aelia"?new[]{"slash","bleed","bash","guard","counter","advance","execute","recover"}:new[]{"bolt","inscribe","detonate","pull","wave","silence","screen","bandage","burn","retreat"};int tile=Array.IndexOf(concepts,skill);string preview=Path.Combine(root,"assets","skills","concepts",hero=="aelia"?"aelia-eight-skills-v4-preview.png":"luchuan-ten-skills-v4-preview.png");Rectangle region=Rectangle.Empty;
  if(tile>=0){if(hero=="aelia")region=new Rectangle(new[]{94,513,935,1355}[tile%4],tile<4?103:480,330,307);else region=new Rectangle(new[]{84,424,747,1072,1395}[tile%5],tile<5?103:466,295,285);}
  if(hero=="aelia"&&(skill=="sweep"||skill=="inspire")){preview=Path.Combine(root,"assets","skills","concepts","aelia-two-extra-skills-v4-preview.png");region=new Rectangle(skill=="sweep"?283:954,171,540,537);}
  if(!region.IsEmpty&&File.Exists(preview)){var sheet=CachedImage(preview);region=new Rectangle(region.X*sheet.Width/1774,region.Y*sheet.Height/887,region.Width*sheet.Width/1774,region.Height*sheet.Height/887);picture=new Bitmap(region.Width,region.Height);using(var g=Graphics.FromImage(picture))g.DrawImage(sheet,new Rectangle(0,0,picture.Width,picture.Height),region,GraphicsUnit.Pixel);imageCache[key]=picture;return picture;}
  string path=Path.Combine(root,"assets","skills",hero+"-"+skill+".png");
  if(File.Exists(path))picture=CachedImage(path);
  else if(hero=="aelia"){
   string[] tiles={"bleed","bash","guard","sweep","execute","advance","counter","break","recover","stand"};int i=Array.IndexOf(tiles,skill);if(i<0)return null;
   var sheet=CachedImage(Path.Combine(root,"assets","skills","aelia-sheet.png"));int[] x={3,311,618,925,1233};var bounds=new Rectangle(x[i%5],i<5?100:540,300,375);picture=new Bitmap(bounds.Width,bounds.Height);using(var g=Graphics.FromImage(picture)){g.DrawImage(sheet,new Rectangle(0,0,picture.Width,picture.Height),bounds,GraphicsUnit.Pixel);}
  }else return null;
  imageCache[key]=picture;return picture;
 }
}

