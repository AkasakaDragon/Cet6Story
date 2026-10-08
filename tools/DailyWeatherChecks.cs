using System;
using System.Web.Script.Serialization;
class DailyWeatherChecks {
 class CountingRandom:Random {public int Calls;public override int Next(int maxValue){return Calls++%maxValue;}}
 static void Check(bool result,string message){if(!result)throw new Exception(message);}
 static void Main(){
  foreach(string weather in new[]{"clear","cloudy","rain"}){
   var save=new Save();save.tavernTime=new TavernTimeState{weather=weather,day=7};var random=new CountingRandom();
   TavernTime.Advance(save,random);Check(save.tavernTime.phase=="dusk"&&save.tavernTime.weather==weather&&random.Calls==0,"Morning to dusk changed weather");
   var json=new JavaScriptSerializer();save=json.Deserialize<Save>(json.Serialize(save));
   TavernTime.Advance(save,random);Check(save.tavernTime.phase=="night"&&save.tavernTime.weather==weather&&random.Calls==0,"Dusk to night changed weather after reload");
   TavernTime.Advance(save,random);Check(save.tavernTime.phase=="morning"&&save.tavernTime.day==8&&random.Calls==1,"Next morning must roll once");
   foreach(string phase in new[]{"morning","dusk","night"}){save.tavernTime.phase=phase;int day=save.tavernTime.day,calls=random.Calls;TavernTime.NextDay(save,random);Check(save.tavernTime.day==day+1&&save.tavernTime.phase=="morning"&&random.Calls==calls+1,"Business completion must roll once");}
  }
  Console.WriteLine("PASS all three daily weathers, within-day transitions, reload persistence, next morning and business next-day rollover.");
 }
}
