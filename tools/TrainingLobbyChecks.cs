using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

public static class TrainingLobbyChecks {
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 static object Call(Game game,string name,params object[] args){return typeof(Game).GetMethod(name,Private).Invoke(game,args);}
 static CardRewardSurface Scene(Game game){return ((Panel)typeof(Game).GetField("content",Private).GetValue(game)).Controls.OfType<CardRewardSurface>().Single();}
 static void Capture(Game game,string path){Application.DoEvents();using(var image=new Bitmap(game.Width,game.Height)){game.DrawToBitmap(image,new Rectangle(Point.Empty,image.Size));image.Save(path);}}
 static void LayoutCheck(CardRewardSurface scene){
  var cards=scene.Controls.OfType<RogueAdventureCard>().ToArray();if(cards.Length!=3||cards.Any(c=>!c.GuildStyle||c.Total<4))throw new Exception("Training cards");
  foreach(var c in scene.Controls.Cast<Control>().Where(c=>c.Visible))if(!scene.ClientRectangle.Contains(c.Bounds))throw new Exception("Clipped control: "+c.Text);
  for(int i=0;i<cards.Length;i++)for(int j=i+1;j<cards.Length;j++)if(cards[i].Bounds.IntersectsWith(cards[j].Bounds))throw new Exception("Overlapping cards");
  var nav=scene.Controls.OfType<RogueShowcaseButton>().ToArray();if(nav.Length!=5||nav.Any(b=>!b.GuildStyle))throw new Exception("Navigation style");
 }
 [STAThread]public static void Main(string[] args){
  Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);Directory.CreateDirectory(args[0]);
  using(var game=new Game()){
   // All test mutations persist inside a separate sandbox, never in the player save.
   var saveField=typeof(Game).GetField("save",Private);var original=(Save)saveField.GetValue(game);
   var test=Engine.Json.Deserialize<Save>(Engine.Json.Serialize(original));test.rogue.run=null;test.rogue.preparationActive=false;
   saveField.SetValue(game,test);typeof(Game).GetField("root",Private).SetValue(game,Path.Combine(args[0],"sandbox"));
   game.Opacity=0;game.Show();game.Bounds=new Rectangle(0,0,1280,780);Call(game,"ShowRogueHome");Application.DoEvents();
   var scene=Scene(game);LayoutCheck(scene);Capture(game,Path.Combine(args[0],"training-wide.png"));
   var cards=scene.Controls.OfType<RogueAdventureCard>().ToArray();var before=Engine.Json.Serialize(test.rogue.memory);
   game.Size=new Size(800,500);Application.DoEvents();LayoutCheck(scene);Capture(game,Path.Combine(args[0],"training-small.png"));
   if(Engine.Json.Serialize(test.rogue.memory)!=before)throw new Exception("Rendering changed learning memory");
   // Exercise each real training card; the original handler must start that mode.
   foreach(string mode in new[]{"基础训练","四级训练","六级挑战"}){
    test.rogue.run=null;Call(game,"ShowRogueHome");scene=Scene(game);
    var card=scene.Controls.OfType<RogueAdventureCard>().Single(c=>c.Mode==mode);
    typeof(Control).GetMethod("OnClick",Private).Invoke(card,new object[]{EventArgs.Empty});
    if(test.rogue.ActiveRun==null||test.rogue.ActiveRun.mode!=mode)throw new Exception("Training entry failed: "+mode);
   }
   game.Size=new Size(1280,780);Call(game,"ShowRogueHome");scene=Scene(game);LayoutCheck(scene);
   if(scene.Controls.OfType<RogueAdventureCard>().Any(c=>c.Enabled))throw new Exception("Busy run allows new training");
   var resume=scene.Controls.OfType<RogueShowcasePanel>().Single(p=>p.Visible&&p.Controls.OfType<RogueShowcaseButton>().Any());
   if(!resume.Controls.OfType<RogueShowcaseButton>().Any(b=>b.Text=="继续本局"))throw new Exception("Resume entry missing");
   Capture(game,Path.Combine(args[0],"training-busy.png"));
   scene.Controls.OfType<RogueShowcaseButton>().Single(b=>b.Text=="主界面").PerformClick();Application.DoEvents();
   if((string)typeof(Game).GetField("page",Private).GetValue(game)!="home")throw new Exception("Return home navigation");
   Call(game,"CloseRogueAudio");
  }
  Console.WriteLine("PASS: responsive lobby, live progress unchanged, all three training actions, busy/resume state, navigation home. Player save untouched.");
 }
}
