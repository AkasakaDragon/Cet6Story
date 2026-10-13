using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;

class TavernProgressionChecks {
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static IEnumerable<Control> All(Control c){yield return c;foreach(Control child in c.Controls)foreach(var sub in All(child))yield return sub;}
 static Save Reload(Save s){return Engine.Json.Deserialize<Save>(Engine.Json.Serialize(s));}
 [STAThread]static void Main(){try{Simulation();Interface();Console.WriteLine("PASS progression and management UI");}catch(Exception e){Console.WriteLine(e);Environment.Exit(1);}}
 static void Simulation(){
  var save=new Save();save.rogue.coins=2000;var s=TavernHall.State(save);TavernHall.Start(s);
  for(int i=0;i<2400;i++)TavernHall.Tick(save,.1);save=Reload(save);s=TavernHall.State(save);
  int guard=0;while(!s.settled&&guard++<12000)TavernHall.Tick(save,.1);
  Check(s.settled&&s.dailyVisitors==12&&s.admitted==12&&s.served==12,"fixed 12 visitors with reload and draining");
  Check(s.summary.Contains("厨房峰值")&&s.renown>0,"real outcome diagnostics and renown");
  int coins=save.rogue.coins,xp=s.chef.workXp,day=s.day;TavernHall.Tick(save,3600);
  Check(save.rogue.coins==coins&&s.chef.workXp==xp&&s.day==day,"after-hours does not mint work, money or dates");
  for(int i=0;i<22;i++)Check(TavernHall.WordResearch(s,"word"+i,0,i%2==0),"new learning units rewarded");
  Check(s.rewards==1&&s.research==10&&s.boosts==3,"continuous research works beyond guidance storage limit");
  Check(TavernHall.ClaimReward(s,0)&&!TavernHall.ClaimReward(s,0),"claim exactly once");
  int before=s.chef.itemXp;Check(TavernHall.UseItem(s,0,0)&&s.chef.itemXp==before+12,"actual employee item XP");
  Check(!TavernHall.UseItem(s,0,0),"no duplicate item consumption");
  for(int i=0;i<6;i++)TavernHall.WordResearch(s,"repeat",0,true);int units=s.studyUnits;
  save=Reload(save);s=TavernHall.State(save);s.day++;TavernHall.WordResearch(s,"repeat",0,true);
  Check(s.studyUnits==units,"reload and game-day changes cannot reset word stage rewards");
  TavernHall.WordResearch(s,"repeat",1,true);Check(s.studyUnits==units+5,"new mastery stage rewarded");
  TavernHall.AddResearch(s,300);Check(TavernHall.ClaimReward(s,1),"gift claim");Check(TavernHall.UseItem(s,1,1)&&s.server.affinity==6,"gift increases partner affinity");
  Check(TavernHall.ClaimReward(s,2)&&TavernHall.UseItem(s,2,0),"preparation item can be equipped after hours");
  Check(s.prepJobs==3&&s.planUsed,"preparation survives until work");
  Check(TavernHall.ClaimReward(s,2)&&!TavernHall.UseItem(s,2,0)&&TavernHall.ItemCount(s,2)==1,"daily plan limit preserves item");
  int prior=s.research;s.study=new HallStudy{position=44000,answers=new Dictionary<string,int>{{"1",2}}};
  Check(TavernHall.Listen(s,"new-group",1,0,3,false,false,new List<int>{1,2,3})>0,"low-score listening grows employee");
  Check(s.research!=prior||s.rewards>0,"listening contributes research");
  TavernHall.Listen(s,"new-group",1,3,3,true,false,new List<int>());units=s.studyUnits;s.day++;
  Check(TavernHall.Listen(s,"new-group",1,3,3,false,false,new List<int>())==0&&s.studyUnits==units,"game days cannot reset listening rewards");
  Check(!TavernHall.Next(s)&&s.study.position==44000,"uncompleted listening retained at next-day boundary");s.study.finished=true;
  s.renown=280;Check(TavernHall.Next(s)&&s.dailyVisitors==26,"promotion applies on next day");
  Check(!TavernHall.Recruit(save,0),"main quest recruitment gate");save.completed.Add("tavern-01-05");
  Check(TavernHall.Recruit(save,0)&&s.helpers.Count==1,"unlocked real helper");Check(!TavernHall.Recruit(save,1),"rank slot gate");
  s.guideAuto=0;s.receptionLimit=20;TavernHall.Start(s);guard=0;while(!s.settled&&guard++<15000){TavernHall.Tick(save,.1);var claims=TavernHall.Employees(s).SelectMany(e=>e.batch).ToList();Check(claims.Count==claims.Distinct().Count(),"helpers never share task claims");Check(s.carrots>=0&&s.flour>=0,"nonnegative reserved stock");}
  Check(s.settled&&s.admitted<=20&&s.deferred==26-s.admitted&&s.helpers[0].workXp>0,"helper actually works and reception limit accounts for total");
  var local=new Save();var g=TavernHall.State(local);g.open=true;g.arrival=-100;g.guideAuto=0;g.boosts=2;g.guests.Add(new HallGuest{id=50,table=0,stage=2,recipe=0});
  Check(TavernHall.UseGuide(g,0)&&g.boosts==1&&g.prepJobs==2,"manual preparation activates");
  for(int i=0;i<100;i++)TavernHall.Tick(local,.1);Check(g.recipes[0].made>0&&g.prepJobs==1,"guidance consumed by actual preparation");
  Check(!TavernHall.UseGuide(g,0)&&g.boosts==1,"invalid or stacked guidance retained");
  var legacy=new Save{tavernHall=new TavernHallState{open=true,served=4}};var old=TavernHall.State(legacy);Check(old.closing&&!old.open&&old.served==4,"legacy live day safely drains");
  var practiceProfile=new RogueProfile();var practiceBank=new List<RogueEntry>{new RogueEntry{word="practice",meaning="练习"}};var mastered=ExpeditionVocabulary.Progress(practiceProfile,"practice",DateTime.Today);mastered.stage=6;bool free;var freeQuestion=TavernHall.PracticeQuestion(practiceProfile,practiceBank,DateTime.Today,out free);Check(free&&freeQuestion.kind==3&&freeQuestion.entry.word=="practice"&&mastered.stage==6,"all-mastered bank still offers continuing practice without changing mastery");
  Console.WriteLine("PASS fixed visitors, renown, ongoing study, gifts, XP, guides, recruitment, claims, save migration");
 }
 static void Interface(){
  var flags=BindingFlags.Instance|BindingFlags.NonPublic;using(var game=new Game()){
   var save=new Save();save.rogue.coins=500;save.completed.AddRange(WaystationChapterOne.Ids);var state=TavernHall.State(save);state.renownLevel=3;state.dailyVisitors=26;state.rewards=3;state.renown=280;TavernHall.Recruit(save,1);
   typeof(Game).GetField("save",flags).SetValue(game,save);game.Size=new Size(1280,780);game.Show();typeof(Game).GetMethod("ShowTavernHall",flags).Invoke(game,null);Application.DoEvents();
   Check(All(game).OfType<HallMeter>().Count()==4,"four compact overview cards");Check(All(game).OfType<TavernHallCanvas>().Single().Resources.Length==4,"resource icons live inside the tavern scene");Capture(game,"progression-hall.png");
   foreach(var size in new[]{new Size(760,650),new Size(540,390)})using(var form=new PixelFrame{Size=size,Text="酒馆经营",StartPosition=FormStartPosition.Manual,Location=new Point(40,40)}){
    var panel=new Panel{Dock=DockStyle.Fill,Padding=new Padding(12),BackColor=Color.FromArgb(25,53,50)};form.Controls.Add(panel);typeof(Game).GetField("hallManagementTab",flags).SetValue(game,0);
    typeof(Game).GetMethod("BuildHallManagement",flags).Invoke(game,new object[]{panel,state,new Func<bool>(()=>false),new Action<bool>(v=>{})});form.Show();Application.DoEvents();
    foreach(var tab in new[]{"员工","菜单","设施","调度","奖励","账本","招募"}){All(form).OfType<VNButton>().First(b=>b.Text==tab).PerformClick();Application.DoEvents();Check(!All(form).OfType<ScrollableControl>().Any(c=>c.AutoScroll),"no native scrollbars "+tab);foreach(var b in All(form).OfType<VNButton>().Where(c=>c.Visible)){Check(b.Width>=30&&b.Height>=18,"accessible management buttons "+tab);}
     Capture(form,"progression-"+size.Width+"-"+tab+".png");}
    All(form).OfType<VNButton>().First(b=>b.Text=="菜单").PerformClick();Application.DoEvents();bool on=state.recipes[0].enabled;All(form).OfType<HallShelfCard>().First(c=>c.Visible).Controls.OfType<VNButton>().First().PerformClick();Check(state.recipes[0].enabled!=on,"illustrated recipe card changes actual menu");state.recipes[0].enabled=on;
    All(form).OfType<VNButton>().First(b=>b.Text=="奖励").PerformClick();Application.DoEvents();int pending=state.rewards,items=TavernHall.ItemCount(state,0);All(form).OfType<HallShelfCard>().First(c=>c.Visible).Controls.OfType<VNButton>().First().PerformClick();Check(state.rewards==pending-1&&TavernHall.ItemCount(state,0)==items+1,"illustrated reward card grants actual item");
    form.Close();
   }
   typeof(Game).GetField("hallManagementTab",flags).SetValue(game,-1);game.Size=new Size(800,500);Application.DoEvents();typeof(Game).GetMethod("ShowTavernHall",flags).Invoke(game,null);Application.DoEvents();Capture(game,"progression-hall-800.png");game.Close();
  }
 }
 static void Capture(Form form,string name){using(var shot=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(shot,new Rectangle(Point.Empty,form.Size));shot.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,name));}}
}
