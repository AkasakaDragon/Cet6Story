using System;using System.Linq;using System.IO;using System.Drawing;using System.Reflection;using System.Windows.Forms;
class TavernHallChecks {
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static System.Collections.Generic.IEnumerable<Control> All(Control c){yield return c;foreach(Control child in c.Controls)foreach(var item in All(child))yield return item;}
 [STAThread]static void Main(){try { Run(); } catch(Exception ex){Console.WriteLine(ex.Message);Console.WriteLine(ex.StackTrace);Environment.Exit(1);}} static void Run(){
  var save=new Save();var state=TavernHall.State(save);int initial=save.rogue.coins;state.boosts=2;TavernHall.Start(state);
  for(int i=0;i<90;i++)TavernHall.Tick(save,.1);Check(state.guests.Count==1,"guest arrives through cycle");
  save=Engine.Json.Deserialize<Save>(Engine.Json.Serialize(save));state=save.tavernHall;Check(state.open&&state.guests.Count==1,"active order survives save");
  for(int day=0;day<5;day++){int guard=0;while(!state.settled&&guard++<10000){TavernHall.Tick(save,.1);Check(state.guests.Count<=2,"capacity");Check(state.stock>=0,"stock");}Check(state.settled&&state.guests.Count==0&&state.served>=6,"day drains and goal achievable");Check(state.revenue>state.cost,"positive net");int before=save.rogue.coins;TavernHall.Tick(save,20);Check(before==save.rogue.coins,"no duplicate settlement");if(day<4){Check(TavernHall.Next(state),"next day");TavernHall.Start(state);}}
  Check(state.Level>1&&save.rogue.coins>initial,"growth and income");Check(state.boosts==0,"quality bonuses consumed on actual production");var closed=new Save();TavernHall.Start(TavernHall.State(closed));TavernHall.Tick(closed,9);closed.tavernHall.open=false;closed.tavernHall.closing=true;for(int i=0;i<1000&&!closed.tavernHall.settled;i++)TavernHall.Tick(closed,.1);Check(closed.tavernHall.settled&&closed.tavernHall.served==1,"manual closing serves existing guest only");Console.WriteLine("PASS: five days, capacity, stock, persistence, drain, net income, growth, bonuses, manual closing, no repeated payout");
  var flags=BindingFlags.Instance|BindingFlags.NonPublic;using(var game=new Game()){
   var uiSave=new Save();typeof(Game).GetField("save",flags).SetValue(game,uiSave);game.Show();typeof(Game).GetMethod("ShowTavernHall",flags).Invoke(game,null);
   foreach(var size in new[]{new Size(1280,780),new Size(800,500)}){game.Size=size;Application.DoEvents();using(var shot=new Bitmap(game.Width,game.Height)){game.DrawToBitmap(shot,new Rectangle(Point.Empty,game.Size));shot.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"hall-"+size.Width+".png"));}var content=(Panel)typeof(Game).GetField("content",flags).GetValue(game);Check(!All(content).OfType<ScrollableControl>().Any(c=>c.AutoScroll),"no default scrollbars");}
   var buttons=All(game).OfType<VNButton>().ToList();var advance=buttons.First(b=>b.Text=="下一题");int money=uiSave.rogue.coins;
   foreach(var key in new[]{"A.","C.","B."}){buttons.First(b=>b.Text.StartsWith(key)).PerformClick();advance.PerformClick();}
   Check(uiSave.tavernHall.boosts==1&&uiSave.tavernHall.xp==0&&uiSave.rogue.coins==money,"guidance provides pending quality only");
   foreach(var key in new[]{"A.","C.","B."}){buttons.First(b=>b.Text.StartsWith(key)).PerformClick();advance.PerformClick();}Check(uiSave.tavernHall.boosts==1,"repeat guidance cannot farm");
   game.Size=new Size(1280,780);buttons.First(b=>b.Text=="开门营业").PerformClick();for(int i=0;i<230;i++)TavernHall.Tick(uiSave,.1);Application.DoEvents();using(var shot=new Bitmap(game.Width,game.Height)){game.DrawToBitmap(shot,new Rectangle(Point.Empty,game.Size));shot.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"hall-active.png"));}game.Close();
  }Console.WriteLine("PASS: 1280/800 live layouts, real guidance clicks, repeat cap, active scene, isolated test save");
 }
}
