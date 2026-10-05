using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;

public static class CardArtChecks {
 public static int Main(string[] args){try{
  var originals=new HashSet<string>();var portraits=new HashSet<string>();
  using(var sha=SHA256.Create())using(var atlas=Image.FromFile("assets/rogue/cards/illustrations.png"))using(var frames=Image.FromFile("assets/rogue/cards/ornate-frames.png"))using(var sheet=new Bitmap(1440,1080))using(var g=Graphics.FromImage(sheet)){
   g.Clear(GameTheme.Navy);
   for(int i=0;i<CardBattle.Cards.Length;i++){var card=CardBattle.Cards[i];string path=Path.Combine("assets","rogue","cards","portraits",card.id+".png");if(!File.Exists(path))throw new Exception("Missing portrait: "+card.id);
    if(!originals.Add(Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(path)))))throw new Exception("Repeated source artwork: "+card.id);
    var portrait=CardPortraitLibrary.Get(card.id);if(portrait==null||portrait.Width!=320||portrait.Height!=344)throw new Exception("Runtime portrait: "+card.id);
    using(var stream=new MemoryStream()){portrait.Save(stream,System.Drawing.Imaging.ImageFormat.Png);if(!portraits.Add(Convert.ToBase64String(sha.ComputeHash(stream.ToArray()))))throw new Exception("Repeated runtime artwork: "+card.id);}
    using(var view=new SupportCardView{Card=card,Atlas=atlas,FrameAtlas=frames,Large=true})using(var image=new Bitmap(180,270))using(var graphics=Graphics.FromImage(image)){graphics.Clear(GameTheme.Navy);view.DrawCard(graphics,180,270);g.DrawImageUnscaled(image,i%8*180,i/8*270);}
   }
   if(args.Length>0)sheet.Save(args[0]);
  }
  Console.WriteLine("PASS: 32 independent original images, 32 distinct runtime crops, complete card rendering.");return 0;
 }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
