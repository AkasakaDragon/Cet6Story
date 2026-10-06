using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

public partial class Game {
 bool pendingTowerEffect; Rectangle[] towerMapIconRegions;
 Image[] CombatFrames(string name,bool effects){string path=Path.Combine(root,"assets","rogue","combat",name+".png");if(!File.Exists(path))return null;var sheet=CachedImage(path);var frames=new Image[8];for(int i=0;i<8;i++){string key="combat:"+name+":"+i;Image sprite;if(!imageCache.TryGetValue(key,out sprite)){sprite=effects?CombatSprites.Effect(sheet,i):CombatSprites.Character(sheet,i);imageCache[key]=sprite;}frames[i]=sprite;}return frames;}
 Image TowerArt(string name){return CachedImage(Path.Combine(root,"assets","rogue","tower",name+".png"));}
 // Separate source-sheet regions at rendering time; original PNGs stay intact.
 Image TowerEnemy(RogueRun r){
  if(r.enemy=="combat"&&r.cardBattle!=null){string m=r.cardBattle.monster;Image alternate=SupportSprite(m=="荆棘孢子兽"?4:m=="回响灯灵"?5:m=="熔甲幼兽"?6:-1);if(alternate!=null&&m!="苔藓木灵"&&m!="石像哨兵"&&m!="余烬蜥蜴")return alternate;}string key=r.enemy=="boss"?"tower-sprite:boss":"tower-sprite:"+r.theme+":"+r.enemy;Image cached;if(imageCache.TryGetValue(key,out cached))return cached;
  if(r.enemy=="boss"){using(var source=new Bitmap(TowerArt("boss-enemy"))){int left=source.Width,top=source.Height,right=0,bottom=0;for(int y=0;y<source.Height;y+=2)for(int x=0;x<source.Width;x+=2)if(source.GetPixel(x,y).A>32){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}cached=source.Clone(Rectangle.FromLTRB(Math.Max(0,left-3),Math.Max(0,top-3),Math.Min(source.Width,right+4),Math.Min(source.Height,bottom+4)),PixelFormat.Format32bppArgb);}imageCache[key]=cached;return cached;}
  var sheet=TowerArt(new[]{"forest-enemies","temple-enemies","lava-enemies"}[Math.Min(2,r.theme)]);bool elite=r.enemy=="elite";int split=r.theme==0?620:r.theme==1?625:780;
  using(var canvas=new Bitmap(sheet.Width,sheet.Height,PixelFormat.Format32bppArgb)){
   using(var g=Graphics.FromImage(canvas)){using(var region=new Region(new Rectangle(elite?split:0,0,elite?sheet.Width-split:split,sheet.Height))){if(r.theme==1){if(elite)region.Union(new Rectangle(480,0,145,340));else region.Exclude(new Rectangle(480,0,145,340));}g.SetClip(region,CombineMode.Replace);g.DrawImageUnscaled(sheet,0,0);}}
   int left=canvas.Width,top=canvas.Height,right=0,bottom=0;for(int y=0;y<canvas.Height;y+=2)for(int x=0;x<canvas.Width;x+=2)if(canvas.GetPixel(x,y).A>32){left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}
   var bounds=Rectangle.FromLTRB(Math.Max(0,left-3),Math.Max(0,top-3),Math.Min(canvas.Width,right+4),Math.Min(canvas.Height,bottom+4));cached=canvas.Clone(bounds,PixelFormat.Format32bppArgb);
  }imageCache[key]=cached;return cached;
 }
 Image TowerBackground(RogueRun r){return QuietScene(r.theme);}
 void ShowRogueHome(){
  save.rogue.preparationActive=false;
  TowerEngine.Migrate(save.rogue);var p=save.rogue;
  if(p.run!=null&&(p.run.mode=="进阶训练"||p.run.mode=="四级训练"||p.run.mode=="六级挑战")){if(p.run.mode=="进阶训练")p.run.mode="四级训练";p.run.pool=TrainingPool(p.run.mode);}
  RenderRogueLanding(true);
 }
 void RenderRogue(){
  var p=save.rogue;TowerEngine.Migrate(p);var r=p.ActiveRun;if(VocabularyCorrections.Apply(save))Persist();if(RefreshStoryExamples(r))Persist();if(r==null){ShowRogueHome();return;}if(r.state=="loot"){TowerEngine.Complete(p);Persist();RenderRogue();return;}if(r.state=="map"){ShowTowerMap(r);return;}if(r.state=="combat"||r.state=="feedback"||r.state=="boss-intro"){RenderFullBattle(r);return;}
  if(r.state=="card-reward"){RenderCardRewards(r);return;}
  if(r.state=="reward"){RenderRelicRewards(r);return;}
  if(r.state=="shop"){RenderJourneyShop(r);return;}
  if(r.state=="chest"){RenderChestEncounter(r);return;}
  if(r.state=="event"){RenderJourneyEvent(r);return;}
  if(r.state=="rest"){RenderCampfire(r);return;}
  if(r.state=="ended"){RenderRogueEnding(r);return;}
  RoguePage("词域远征 · "+r.mode);rogueBody.Controls.Add(Lab("生命 "+r.hp+" / "+r.maxHp+"  ·  攻击 "+r.attack+"  ·  护甲 "+r.armor+"  ·  金币 "+p.coins+"  ·  连击 "+r.combo,12,Gold));
  if(!String.IsNullOrEmpty(r.vocabularyChapter))rogueBody.Controls.Add(Lab("本节词汇已准备 "+PreparationEngine.Count(p,r.vocabularyChapter,r.pool)+" / "+r.pool.Count,11,Accent));
  bool battle=r.state=="combat"||r.state=="feedback"||r.state=="boss-intro";
  rogueArena=new RogueArena{Art=TowerBackground(r),Hero=BattleHero(),EnemyArt=battle?TowerEnemy(r):null,PistolFrames=BattleHeroFrames("pistol"),ReactionFrames=BattleHeroFrames("reactions"),Effects=CombatFrames("effects",true),Run=r,Height=Math.Max(190,Math.Min(300,content.Height*40/100)),Mode=r.state,ShowDrone=battle,HitSound=PlayRogueHit,AnimateHit=pendingTowerEffect};pendingTowerEffect=false;rogueArena.Dispose();rogueArena=null;
  if(r.state=="combat")RenderRogueQuestion(r);
  else if(r.state=="feedback")RenderTowerFeedback(r);
  else if(r.state=="card-reward"){RenderCardRewards(r);}else if(r.state=="reward"){
   var card=RogueCard("获得赋能 · 三选一",(TowerEngine.Node(r)!=null&&TowerEngine.Node(r).kind=="chest"?"稀有宝箱":"精英战斗胜利")+" · 已获得 "+r.battleCoins+" 金币。赋能只在本局生效，可叠加。");
   foreach(string id in r.rewards){string chosen=id;card.Controls.Add(RogueButton(RogueEngine.RelicName(id)+" · "+RogueEngine.RelicDescription(id),()=>{RogueEngine.Reward(p,chosen);SaveRogue();},600,64));}

  }else if(r.state=="rest"){
   var card=RogueCard("火堆 · 一次选择","恢复生命，或磨练武器。选择后返回地图。");var heal=RogueButton("休息 · 恢复 "+(int)Math.Ceiling(r.maxHp*.3)+" 生命",()=>{TowerEngine.Rest(p,true);SaveRogue();},300);heal.Enabled=r.hp<r.maxHp;RogueActions(card,heal,RogueButton("磨练 · 本局攻击 +3",()=>{TowerEngine.Rest(p,false);SaveRogue();},300));
  }else if(r.state=="chest"){
   var card=RogueCard(r.chestKind==2?"稀有宝箱":"遗迹宝箱",r.chestKind==2?"获得 20 金币，并选择一项赋能。":"获得 15 金币。每只宝箱只能领取一次。");card.Controls.Add(RogueButton("打开宝箱",OpenTowerChest,260));
  }else if(r.state=="event")RenderTowerEvent(r);
  else if(r.state=="shop")RenderTowerShop(r);
  else if(r.state=="boss-intro"){
   var card=RogueCard("BOSS 遭遇 · 古界守门者","遗迹尽头，守门者苏醒。通过词汇作答发动攻击；胜利获得 40–50 金币。\n基础生命 "+TowerEngine.EnemyHealth(r,"boss")+"，答错基础伤害 18；护甲和赋能继续生效。");card.Controls.Add(RogueButton("挑战守门者",()=>{if(r.state!="boss-intro")return;TowerEngine.StartBattle(r,p,"boss");SaveRogue();},300));
  }

  AddRelicHud(r);RogueLayout();
 }
 void EnterTowerNode(RogueRun run,string id){
  if(menuLoading!=null&&!menuLoading.IsDisposed)return;
  var node=TowerEngine.Available(run).FirstOrDefault(n=>n.id==id);if(node==null)return;
  Action enter=()=>{RogueEngine.ChooseRoute(run,id,save.rogue);SaveRogue();};
  if(node.kind=="combat"||node.kind=="elite"||node.kind=="boss")NavigateMenu(enter,"战斗",false);else enter();
 }
 void ShowTowerMap(RogueRun r){ClearPage();page="rogue";rogueArena=null;var map=new TowerMap{Dock=DockStyle.Fill,Art=TowerArt("study-map-desk"),IconAtlas=TowerArt("map-icons"),IconSources=towerMapIconRegions??(towerMapIconRegions=TowerMap.FindIconRegions(TowerArt("map-icons"))),Run=r,Font=GameTheme.Body(11),GoBack=save.rogue.preparationActive?(Action)ShowStoryMap:ShowRogueHome,Selected=id=>EnterTowerNode(r,id)};content.Controls.Add(map);if(save.rogue.preparationActive&&PreparationReady(current)){var enter=new VNButton{Text="词汇准备完成 · 进入剧情",PixelStyle=true,Size=new Size(280,42),Font=GameTheme.Body(11)};map.Controls.Add(enter);enter.Click+=(s,e)=>ShowStory();Action place=()=>enter.Location=new Point(Math.Max(12,(map.Width-enter.Width)/2),12);map.Resize+=(s,e)=>place();place();}AddRelicHud(r,map);map.Focus();}
 void RenderTowerFeedback(RogueRun r){var card=RogueCard("本回合反馈",RogueFeedbackText(r));var heading=card.Controls[0];card.Controls.Remove(heading);var row=new FlowLayoutPanel{AutoSize=true,WrapContents=false};heading.Margin=new Padding(6,13,12,6);row.Controls.Add(heading);row.Controls.Add(RogueFeedbackIcon(r.question.entry,"speaker"));row.Controls.Add(RogueFeedbackIcon(r.question.entry,"star"));card.Controls.Add(row);card.Controls.SetChildIndex(row,0);card.Controls.Add(RogueButton(r.hp<=0?"查看结果":r.enemyHp<=0?"领取战利品":"下一回合",()=>{RogueEngine.Continue(save.rogue);SaveRogue();},280));}
 void RenderTowerEvent(RogueRun r){
  var card=RogueCard(TowerEngine.EventTitle(r.eventKind),TowerEngine.EventDescription(r.eventKind));string[] yes={"献祭 · 12 生命换随机赋能","购买治疗 · 15 金币，恢复 35 生命","购买锋刃符石 · 45 金币","解救木灵 · 开始战斗","接受考验 · 开始战斗"},no={"拾取金币 · +8 金币","简单包扎 · 恢复 10 生命","告别商人","留下祝福 · +6 金币","带走回声 · 提示 +1"};var risk=RogueButton(yes[r.eventKind],()=>{if(TowerEngine.Event(save.rogue,true))SaveRogue();},400,64);risk.Enabled=r.eventKind==0?r.hp>12:r.eventKind==1?save.rogue.coins>=15&&r.hp<r.maxHp:r.eventKind==2?save.rogue.coins>=45:true;RogueActions(card,risk,RogueButton(no[r.eventKind],()=>{TowerEngine.Event(save.rogue,false);SaveRogue();},400,64));
 }
 void RenderTowerShop(RogueRun r){var card=RogueCard("旅途商店 · 永久金币","当前金币 "+save.rogue.coins+"。本局赋能不能带到主线，每次到店每种商品限购一次。");foreach(string id in new[]{"potion","shield","blade","hint"}){string chosen=id;int price=TowerEngine.SupplyPrice(id);string desc=id=="potion"?"恢复 30 生命":id=="hint"?"提示次数 +1":RogueEngine.RelicName(id)+" · "+RogueEngine.RelicDescription(id);var b=RogueButton(desc+" · "+price+" 金币",()=>{if(TowerEngine.Buy(save.rogue,chosen))SaveRogue();},600,60);b.Enabled=!r.shopBought.Contains(id)&&save.rogue.coins>=price&&(id!="potion"||r.hp<r.maxHp);card.Controls.Add(b);}card.Controls.Add(RogueButton("离开商店 · 返回地图",()=>{TowerEngine.Complete(save.rogue);SaveRogue();},300));}
}








