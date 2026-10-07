using System;using System.Drawing;using System.Collections.Generic;
class HeroActionPaletteChecks{
 static void Main(){try{Run();}catch(Exception e){Console.WriteLine(e.Message);Environment.Exit(1);}} static void Run(){using(var idle=(Bitmap)HeroIdleAnimation.Load("D:/WORK/Cet6Story/assets/characters/animations/aelia-idle.gif")[0])using(var actions=Image.FromFile("D:/WORK/Cet6Story/assets/characters/actions/aelia-actions.png"))using(var original=new Bitmap(actions))using(var matched=HeroActionPalette.Match(actions,idle)){
 var palette=new HashSet<int>();for(int y=0;y<idle.Height;y++)for(int x=0;x<idle.Width;x++){var p=idle.GetPixel(x,y);if(p.A>=230)palette.Add(p.ToArgb()|unchecked((int)0xff000000));}int changed=0;
 for(int y=0;y<matched.Height;y++)for(int x=0;x<matched.Width;x++){var a=original.GetPixel(x,y);var b=matched.GetPixel(x,y);if(a.A!=b.A)throw new Exception("alpha/shape changed");if(b.A>=64&&!palette.Contains(b.ToArgb()|unchecked((int)0xff000000)))throw new Exception("colour outside idle palette");if(a.ToArgb()!=b.ToArgb())changed++;}if(changed==0)throw new Exception("no palette adjustment");
 using(var preview=new Bitmap(820,460))using(var g=Graphics.FromImage(preview)){g.Clear(Color.FromArgb(25,45,48));g.DrawImage(idle,new Rectangle(40,45,265,350));int w=actions.Width/8,h=actions.Height/3;g.DrawImage(matched,new Rectangle(390,20,400,400),new Rectangle(4*w,0,w,h),GraphicsUnit.Pixel);preview.Save("D:/WORK/Cet6Story/.validation/aelia-palette-preview.png");}
 Console.WriteLine("PASS all action pixels use idle palette; alpha, positions and dimensions preserved ("+changed+" recoloured pixels)");}}
}

