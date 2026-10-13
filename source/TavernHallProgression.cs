using System;
using System.Linq;
using System.Collections.Generic;

public partial class HallEmployee {
 public int role{get;set;}
 public int direction{get;set;}
 public int training{get;set;}
 public int itemXp{get;set;}
 public int affinity{get;set;}
 public int gifts{get;set;}
}
public partial class HallListeningRecord {public bool correctedReward{get;set;}public string reviewDate{get;set;}}
public partial class TavernHallState {
 public int managementVersion{get;set;}
 public int renown{get;set;}
 public int renownLevel{get;set;}
 public int dailyVisitors{get;set;}
 public int admitted{get;set;}
 public int lost{get;set;}
 public int deferred{get;set;}
 public int receptionLimit{get;set;}
 public int kitchenPeak{get;set;}
 public int hallPeak{get;set;}
 public int renownEarned{get;set;}
 public int research{get;set;}
 public int rewards{get;set;}
 public int rewardsClaimed{get;set;}
 public int studyUnits{get;set;}
 public int wordUnits{get;set;}
 public int guideMode{get;set;}
 public int guideAuto{get;set;}
 public int prepDish{get;set;}
 public int prepJobs{get;set;}
 public int hallPriority{get;set;}
 public int hallJobs{get;set;}
 public bool planUsed{get;set;}
 public int businessFocus{get;set;}
 public List<HallEmployee> helpers{get;set;}
 public Dictionary<string,int> items{get;set;}
 public Dictionary<string,int> wordAwards{get;set;}
 public string summary{get;set;}
}
public static partial class TavernHall {
 public const double DaySeconds=600;
 public static readonly int[] VisitorCounts={12,18,26,36,48,60};
 public static readonly int[] RenownThresholds={0,100,280,560,960,1500};
 public static readonly string[] RenownNames={"无名小馆","街坊熟知","镇上热门","商路名店","冒险者据点","地区招牌"};
 public static readonly string[] ItemNames={"实务心得","茶点礼盒","备餐计划"};
 public static readonly string[] FocusNames={"快速接待","招牌推广","熟客经营"};
 public static void NormalizeManagement(TavernHallState s){
  if(s.helpers==null)s.helpers=new List<HallEmployee>();
  if(s.items==null)s.items=new Dictionary<string,int>();
  if(s.wordAwards==null)s.wordAwards=new Dictionary<string,int>();
  foreach(var e in Employees(s)){if(e.batch==null)e.batch=new List<int>();}
  s.chef.role=0;s.server.role=1;
  if(s.managementVersion==0){s.managementVersion=2;s.renownLevel=1;s.dailyVisitors=12;s.prepDish=-1;s.guideAuto=1;
   // An already running legacy day retains its orders and finishes on its original clock.
   if(s.open||s.closing){s.dailyVisitors=Math.Max(12,s.served+s.guests.Count);s.admitted=s.served+s.guests.Count;s.deferred=Math.Max(0,s.dailyVisitors-s.admitted);s.receptionLimit=s.admitted;s.closing=true;s.open=false;}
  }
  s.renownLevel=Math.Max(1,Math.Min(6,s.renownLevel));
  if(s.dailyVisitors<=0)s.dailyVisitors=VisitorCounts[s.renownLevel-1];
 }
 public static List<HallEmployee> Employees(TavernHallState s){var result=new List<HallEmployee>{s.chef,s.server};if(s.helpers!=null)result.AddRange(s.helpers);return result.Where(e=>e!=null).ToList();}
 public static string RankName(TavernHallState s){return RenownNames[Math.Max(0,Math.Min(5,s.renownLevel-1))];}
 public static int NextRenown(TavernHallState s){return s.renownLevel>=6?0:RenownThresholds[s.renownLevel];}
 public static int WaveCount(TavernHallState s,int wave){int first=(int)Math.Round(s.dailyVisitors*.25);int second=(int)Math.Round(s.dailyVisitors*.45);return wave==0?first:wave==1?second:s.dailyVisitors-first-second;}
 public static int Wave(TavernHallState s){return s.elapsed<180?0:s.elapsed<390?1:2;}
 static double ArrivalTime(TavernHallState s,int visitor){int first=WaveCount(s,0),second=WaveCount(s,1);if(visitor<first)return 7+visitor*38; if(visitor<first+second)return 185+(visitor-first)*Math.Min(22,170.0/Math.Max(1,second));return 400+(visitor-first-second)*Math.Min(30,175.0/Math.Max(1,WaveCount(s,2)));}
 static void Arrive(TavernHallState s,double dt){
  // Negative arrival is used by isolated task simulations; it advances back to zero normally.
  s.arrival+=dt;if(s.arrival<0)return;
  int cap=s.receptionLimit>0?Math.Min(s.receptionLimit,s.dailyVisitors):s.dailyVisitors;
  if(s.admitted>=cap)return;
  if(s.elapsed<ArrivalTime(s,s.admitted)||s.guests.Count>=HallSeats.Count)return;
  var menu=Enumerable.Range(0,4).Where(i=>Available(s,i)).ToList();
  if(menu.Count==0){s.notice="菜单缺料：可补购或调整菜单，学习进度会保留。";return;}
  int seat=HallSeats.NextFree(s.guests,s.nextId);int dish=menu[s.nextId%menu.Count];
  s.guests.Add(new HallGuest{id=s.nextId++,table=seat,recipe=dish,regular=s.regularVisits>=3&&s.nextId%4==0});s.admitted++;
 }
 public static void CloseDoors(TavernHallState s){s.open=false;s.closing=true;s.deferred=Math.Max(0,s.dailyVisitors-s.admitted);}
 static void CompleteDay(TavernHallState s){
  s.closing=false;s.settled=true;s.deferred=Math.Max(0,s.dailyVisitors-s.admitted);
  foreach(var e in Employees(s)){e.action="待命";e.gifts=0;}
  int earned=s.renownEarned;s.renown+=earned;
  int next=s.renownLevel;while(next<6&&s.renown>=RenownThresholds[next])next++;
  string promotion=next>s.renownLevel?"\n明日起晋升「"+RenownNames[next-1]+"」，固定来客 "+VisitorCounts[next-1]+" 人。":"";
  s.summary="第 "+s.day+" 日经营账本\n固定来客 "+s.dailyVisitors+" · 入店 "+s.admitted+" · 完成 "+s.served+"\n离店取消 "+s.lost+" · 未接待 "+s.deferred+"\n收入 "+s.revenue+" − 采购 "+s.cost+" = 净收益 "+(s.revenue-s.cost)+"\n满意度 "+(s.served==0?0:s.satisfaction/s.served)+" · 知名度 +"+earned+"\n厨房峰值积压 "+s.kitchenPeak+" · 大厅峰值积压 "+s.hallPeak+"\n"+Diagnosis(s)+promotion+"\n已打烊，可继续背词和听力，研修奖励持续累积。";
  s.notice="已打烊 · 学习继续，结算不会打断录音。";
 }
 public static string Diagnosis(TavernHallState s){if(s.kitchenPeak>s.hallPeak)return "建议：厨房偏忙，试试快手菜、双锅或新增厨师。";if(s.hallPeak>1)return "建议：大厅偏忙，优先收桌、培养周转或增加服务员。";return "分工顺畅，可推广招牌菜或尝试更多接待。";}
 public static bool UseGuide(TavernHallState s,int mode){
  if(s.boosts<=0){s.notice="先完成一组词汇学习，获得指导机会。";return false;}
  if(!s.open&&!s.closing){s.notice="指导次数跨日保留，开店后选择现场任务。";return false;}
  if(mode==0){var g=s.guests.FirstOrDefault(v=>v.stage==2&&v.recipe!=3&&Available(s,v.recipe));if(g==null||s.prepJobs>0){s.notice="目前没有可备餐订单，指导已保留。";return false;}s.prepDish=g.recipe;s.prepJobs=2;s.notice="集中备餐：接下来两次对应制作减少重复准备，原料照常消耗。";}
  else if(mode==1){if(s.hallJobs>0||!s.guests.Any(g=>g.stage==3||g.stage==6)){s.notice="目前无需大厅协调，指导已保留。";return false;}s.hallPriority=s.guests.Any(g=>g.stage==6)?6:3;s.hallJobs=3;s.notice="大厅协调：接下来三项工作优先"+(s.hallPriority==6?"收桌。":"送餐。");}
  else {var targets=s.guests.Where(g=>(g.stage==1||g.stage==2||g.stage==3)&&g.wait>20&&!g.calmed).ToList();if(targets.Count==0){s.notice="目前没有需要安抚的顾客，指导已保留。";return false;}foreach(var g in targets){g.wait=Math.Max(0,g.wait-25);g.calmed=true;}s.notice="已安抚等候顾客，恢复部分耐心。";}
  s.boosts--;return true;
 }
 static void AutoGuide(TavernHallState s){if(s.guideAuto==0||s.boosts<=0)return;int mode=s.guideAuto==1?0:s.guideAuto==2?1:2;bool needed=mode==0?s.guests.Count(g=>g.stage==2&&g.recipe!=3)>=2&&s.prepJobs==0:mode==1?s.guests.Count(g=>g.stage==6)>=1&&s.hallJobs==0:s.guests.Any(g=>g.wait>80&&!g.calmed&&(g.stage==1||g.stage==2||g.stage==3));if(needed)UseGuide(s,mode);}
 public static void AddResearch(TavernHallState s,int amount){if(amount<=0)return;s.research+=amount;while(s.research>=100){s.research-=100;s.rewards++;}s.studyUnits+=amount;}
 // Each word mastery stage has its own identity. Game-day changes do not reset it.
 public static bool WordResearch(TavernHallState s,string word,int stage,bool correct){string key=word.ToLowerInvariant()+":"+stage;int awards;s.wordAwards.TryGetValue(key,out awards);int max=correct?3:1;if(awards>=max)return false;s.wordAwards[key]=awards+1;AddResearch(s,5);s.wordUnits++;if(s.wordUnits%3==0)Guide(s);return true;}
 public static bool ClaimReward(TavernHallState s,int item){if(s.rewards<=0||item<0||item>=ItemNames.Length)return false;int old;s.items.TryGetValue(ItemNames[item],out old);s.items[ItemNames[item]]=old+1;s.rewards--;s.rewardsClaimed++;s.notice="研修奖励已领取："+ItemNames[item]+"，可在奖励箱使用。";return true;}
 public static int ItemCount(TavernHallState s,int item){int count;return s.items.TryGetValue(ItemNames[item],out count)?count:0;}
 public static bool UseItem(TavernHallState s,int item,int employee){
  if(ItemCount(s,item)<=0)return false;var all=Employees(s);if(employee<0||employee>=all.Count)return false;var e=all[employee];
  if(item==0){if(e.level>=5){s.notice=e.name+"已达到当前等级上限，心得保留。";return false;}e.xp+=12;e.itemXp+=12;e.Grow();s.notice=e.name+"使用实务心得：经验 +12。";}
  else if(item==1){int amount=e.gifts==0?6:e.gifts==1?3:1;e.affinity+=amount;e.gifts++;s.notice=e.name+"与你分享茶点：好感 +"+amount+"。关系进度不会跳过主线条件。";}
  else {if(s.open||s.closing||s.planUsed||s.prepJobs>0){s.notice="备餐计划在开店前使用，每个营业日一份；当前计划会保留。";return false;}s.planUsed=true;s.prepDish=-1;s.prepJobs=3;s.notice="备餐计划已准备：下次三项制作减少重复准备，正常耗料。";}
  s.items[ItemNames[item]]--;return true;
 }
 public static bool CanRecruit(Save save){return save.completed.Contains("tavern-01-05");}
 public static bool Recruit(Save save,int role){var s=State(save);if(s.open||s.closing){s.notice="请在准备或打烊时招募。";return false;}if(!CanRecruit(save)){s.notice="推进主线「旧矿道：第一次主动出发」后开放镇民招募。";return false;}int slots=s.renownLevel>=4?2:s.renownLevel>=3?1:0;if(s.helpers.Count>=slots){s.notice="镇上热门开放首位帮工，商路名店开放第二位。";return false;}if(save.rogue.coins<60){s.notice="招募需要60金币。";return false;}save.rogue.coins-=60;s.helpers.Add(new HallEmployee{name=role==0?"镇民帮工 · 厨房":"镇民帮工 · 大厅",role=role});s.notice="帮工已加入，会实际认领并完成岗位任务；可用心得帮助成长。";return true;}
 static void HelperWork(Save save,double dt){var s=State(save);foreach(var helper in s.helpers){if(helper.role==0)Chef(s,dt,helper);else Server(save,dt,helper);}}
 static int ServerPriority(TavernHallState s,HallGuest g,HallEmployee worker){if(s.hallJobs>0&&g.stage==s.hallPriority)return -1;if(worker.direction==0&&worker.training>=60&&g.stage==6)return 0;return g.stage==3?0:g.stage==5?1:g.stage==6?2:3;}
}
public partial class HallGuest {public bool calmed{get;set;}}

public partial class HallRecipe {public int insight{get;set;}}
public partial class TavernHallState {public bool freeStudy{get;set;}}
public partial class TavernHall {
 public static RogueQuestion PracticeQuestion(RogueProfile profile,System.Collections.Generic.List<RogueEntry> bank,DateTime today,out bool free){
  var q=ExpeditionVocabulary.Next(profile,bank,today);free=q.kind<0;if(!free)return q;
  var entries=bank.Where(e=>!String.IsNullOrWhiteSpace(e.word)&&!String.IsNullOrWhiteSpace(e.meaning)).ToList();if(entries.Count==0)return q;
  var entry=entries[new Random(Guid.NewGuid().GetHashCode()).Next(entries.Count)];RogueEngine.RecordWordAppearance(profile,entry.word);
  return new RogueQuestion{entry=entry,kind=3,answer=-1,options=new System.Collections.Generic.List<string>()};
 }
}
