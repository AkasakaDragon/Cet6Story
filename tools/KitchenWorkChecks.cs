using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Web.Script.Serialization;
class KitchenWorkChecks {
 const string Root="D:/WORK/Cet6Story";
 static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static IEnumerable<Control> All(Control parent){foreach(Control child in parent.Controls){yield return child;foreach(var nested in All(child))yield return nested;}}
 static void Shot(Control control,string name){using(var bmp=new Bitmap(control.Width,control.Height)){control.DrawToBitmap(bmp,control.ClientRectangle);bmp.Save(Root+"/.validation/"+name+".png");}}
 [STAThread]static void Main(){
  foreach(string cooking in new[]{"boil","bake","fry"}){
   var s=new KitchenWorkState();Check(!s.Start(cooking)&&!s.Plate(),"Unprepared cooking blocked");Check(s.TakeCarrot()&&!s.TakeCarrot(),"One held ingredient");Check(s.Start("cut"),"Cut starts");Check(!s.Answer("cut","wrong")&&s.cut==0,"Wrong answer must not advance");
   foreach(string word in KitchenWords.Words("cut"))Check(s.Answer("cut"," "+word.ToUpper()+" "),"Correct answer advances");Check(!s.Answer("cut","chop"),"Fourth answer blocked");
   var json=new JavaScriptSerializer();s=json.Deserialize<KitchenWorkState>(json.Serialize(s));Check(s.Pickup("cut")&&s.held=="chopped","Reload and pickup");Check(s.Start(cooking),"Cooking starts");Check(!s.Start(cooking=="boil"?"bake":"boil"),"Cannot duplicate food in another station");Check(!s.Pickup(cooking),"Cannot take unfinished food");
   foreach(string word in KitchenWords.Words(cooking))Check(s.Answer(cooking,word),"Three cooking steps");Check(s.Pickup(cooking)&&s.Plate()&&s.served==1&&!s.Plate(),"Plate once");Check(s.TakeCarrot(),"Next carrot available");
  }
  var clicked=new List<string>();
  using(var form=new Form{ClientSize=new Size(1280,760)})using(var room=new KitchenRoomView(Root,new KitchenWorkState(),x=>clicked.Add(x),()=>{}){Dock=DockStyle.Fill}){
   form.Controls.Add(room);form.Show();Application.DoEvents();foreach(string kind in KitchenRoomView.Kinds){var button=room.Controls.OfType<VNButton>().Single(x=>x.Name==kind);Check(button.Visible&&room.ClientRectangle.Contains(button.Bounds),"Station button visible");button.PerformClick();}Check(clicked.SequenceEqual(KitchenRoomView.Kinds),"Six buttons dispatch correct stations");Shot(room,"kitchen-room-preview");
   form.ClientSize=new Size(960,540);Application.DoEvents();foreach(var button in room.Controls.OfType<VNButton>())Check(room.ClientRectangle.Contains(button.Bounds),"Resized buttons visible");
  }
  using(var game=new Game()){
   typeof(Game).GetField("root",Private).SetValue(game,Root+"/.validation/kitchen-test");var save=new Save();save.tavernTime=new TavernTimeState{phase="night",weather="rain"};typeof(Game).GetField("save",Private).SetValue(game,save);game.Show();typeof(Game).GetMethod("ShowKitchenBusinessOptions",Private).Invoke(game,null);Application.DoEvents();Shot(game,"kitchen-entry-preview");
   foreach(string kind in new[]{"cut","boil","bake","fry"}){
    var state=new KitchenWorkState();if(kind=="cut"){state.held="raw";state.Start(kind);}else{state.held="chopped";state.Start(kind);}save.kitchenWork=state;int elapsed=0;Exception error=null;
    using(var timer=new Timer{Interval=150}){timer.Tick+=(s,e)=>{try{elapsed++;var dialog=Application.OpenForms.Cast<Form>().FirstOrDefault(f=>f is GuildWordDialog);if(dialog==null)return;var controls=All(dialog).ToArray();var input=controls.OfType<TextBox>().Single();var button=controls.OfType<VNButton>().Single();if(state.Progress(kind)<3)Check(input.Visible&&input.Parent.Visible,"Spelling field must be visible");if(elapsed==1)Shot(dialog,"kitchen-"+kind+"-start");if(button.Enabled&&state.Progress(kind)<3){input.Text=KitchenWords.Words(kind)[state.Progress(kind)];button.PerformClick();}else if(state.Progress(kind)==3&&button.Enabled){Shot(dialog,"kitchen-"+kind+"-done");button.PerformClick();timer.Stop();}if(elapsed>70)throw new Exception("Processing UI timeout");}catch(Exception ex){error=ex;timer.Stop();foreach(Form f in Application.OpenForms.Cast<Form>().ToArray())if(f is GuildWordDialog)f.Close();}};timer.Start();typeof(Game).GetMethod("ShowKitchenProcess",Private).Invoke(game,new object[]{kind});timer.Stop();}if(error!=null)throw error;Check(state.held==(kind=="cut"?"chopped":"finished"),"Actual UI three answers and pickup");
   }
   game.Close();
  }
  Console.WriteLine("PASS three cooking routes, wrong/repeated answers, persistence, pickup/plating, six clickable stations and resize, actual processing dialogs and screenshots.");
 }
}
