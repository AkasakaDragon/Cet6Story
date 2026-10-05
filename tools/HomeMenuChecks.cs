using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

public static class HomeMenuChecks {
 static void Call(Game game,string method){typeof(Game).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);}
 static void Capture(Game game,string path){using(var image=new Bitmap(game.Width,game.Height)){game.DrawToBitmap(image,new Rectangle(Point.Empty,image.Size));image.Save(path);}}
 [STAThread]public static void Main(string[] args){
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
  Directory.CreateDirectory(args[0]);
  using(var game=new Game()){
   game.Opacity=0;game.Show();game.Bounds=new Rectangle(0,0,1280,780);Call(game,"ShowMain");Application.DoEvents();
   var stage=(ArtPanel)typeof(Game).GetField("stage",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
   var buttons=stage.Controls.OfType<VNButton>().Where(b=>b.StaticMenuStyle).ToArray();
   var tavern=stage.Controls.OfType<VNButton>().Single(b=>b.Text=="封印酒馆 · 新主线");
   if(buttons.Length!=6||!tavern.HomeStyle)throw new Exception("Static artwork, original six actions and independent tavern entry");
   var save=typeof(Game).GetField("save",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game) as Save;
   if(buttons[0].Enabled!=save.hasGame)throw new Exception("Continue enablement");
   Capture(game,Path.Combine(args[0],"home-wide.png"));int builds=stage.SceneBuilds;
   using(var first=new Bitmap(stage.Width,stage.Height))using(var second=new Bitmap(stage.Width,stage.Height)){
    stage.DrawToBitmap(first,new Rectangle(Point.Empty,first.Size));
    var until=DateTime.UtcNow.AddSeconds(1.2);while(DateTime.UtcNow<until){Application.DoEvents();System.Threading.Thread.Sleep(20);}
    stage.DrawToBitmap(second,new Rectangle(Point.Empty,second.Size));
    int changed=0;for(int y=0;y<stage.Height;y+=3)for(int x=0;x<stage.Width;x+=3)if(first.GetPixel(x,y)!=second.GetPixel(x,y))changed++;
    if(changed!=0)throw new Exception("Static home still animates");
    if(stage.SceneBuilds!=builds)throw new Exception("Static background was rebuilt");
   }
   game.Size=new Size(800,500);Application.DoEvents();
   var scene=stage.SceneArtBounds();
   if(!scene.Contains(tavern.Bounds))throw new Exception("Tavern entry clipped at minimum size");
   for(int i=0;i<buttons.Length;i++){
    var button=buttons[i];if(!scene.Contains(button.Bounds))throw new Exception("Button clipped at minimum size");
    var center=new Point(scene.X+(int)(316*scene.Width/1672.0),scene.Y+(int)((373+i*79)*scene.Height/941.0));
    if(!button.Bounds.Contains(center))throw new Exception("Hit area not aligned to artwork");
   }
   Capture(game,Path.Combine(args[0],"home-small.png"));
   buttons[4].PerformClick();
   var navigationDeadline=DateTime.UtcNow.AddSeconds(8);while(!stage.IsDisposed&&DateTime.UtcNow<navigationDeadline){Application.DoEvents();System.Threading.Thread.Sleep(20);}
   if(!stage.IsDisposed)throw new Exception("Settings button failed to navigate");
   var page=typeof(Game).GetField("page",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game) as string;
   if(page!="settings")throw new Exception("Settings destination");
   // Dispose without closing: avoid persisting preview state to the player's save.
   typeof(Game).GetMethod("CloseRogueAudio",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);
  }
  using(var reader=new BinaryReader(File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"assets","rogue","audio","menu-orbit.wav")))){
   if(new string(reader.ReadChars(4))!="RIFF")throw new Exception("Music WAV header");reader.BaseStream.Position=22;
   if(reader.ReadUInt16()!=2||reader.ReadInt32()!=44100)throw new Exception("Music stereo PCM format");
   reader.BaseStream.Position=44;int peak=0;while(reader.BaseStream.Position<reader.BaseStream.Length)peak=Math.Max(peak,Math.Abs((int)reader.ReadInt16()));
   if(peak<1000||peak>=32767)throw new Exception("Silent or clipped music");
  }
  Console.WriteLine("PASS: six menu hit areas, continue enablement, wide/minimum layouts, static pixels, cached scene reuse, settings click navigation, stereo music without clipping.");
 }
}
