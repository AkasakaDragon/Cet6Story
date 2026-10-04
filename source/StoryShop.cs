using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

public static class StoryShopEngine {
 public static readonly string[] Items={"identity-shield","jammer","scanner","repair","echo-probe","repair-pack","memory-unit","reserve-power"};
 public static int Price(string id){if(id=="echo-probe")return 240;if(id=="repair-pack"||id=="reserve-power")return 300;if(id=="memory-unit")return 360;return id=="identity-shield"?120:id=="jammer"?120:id=="scanner"?180:id=="repair"?240:0;}
 public static string Name(string id){if(id=="echo-probe")return "回声探针";if(id=="repair-pack")return "应急修复包";if(id=="memory-unit")return "记忆存储器";if(id=="reserve-power")return "备用能源";return id=="identity-shield"?"临时身份屏蔽器":id=="jammer"?"信号干扰器":id=="scanner"?"档案扫描器":id=="repair"?"协议修复芯片":"未知道具";}
 public static string Description(string id){if(id=="echo-probe")return "1.1 两星通关后定位求救信号，解锁隐藏 A 无名诊所。";if(id=="repair-pack")return "无名诊所：恢复备用电源，护送医生安全撤离。";if(id=="memory-unit")return "无名诊所：备份证词和转移名单，为后续调查保留证据。";if(id=="reserve-power")return "1.2 两星通关后记录零号站台坐标；该隐藏关将在后续扩展。";return id=="identity-shield"?"1.1 雨夜追踪：建立临时身份屏蔽，争取穿过雨巷的安全窗口。":id=="jammer"?"1.2 地下末班车：遮蔽身份追踪，让星遥启动废弃列车。":id=="scanner"?"1.3 失名档案：读取被隐藏的原始记录，寻找星遥父亲留下的证据。":"1.4 黎明信号：修复公共广播协议，公开证据并恢复失名者的身份。";}
 public static bool Buy(RogueProfile p,string id){p.Normalize();int cost=Price(id);if(cost<=0||p.coins<cost||p.storyItems.Contains(id)||p.storyUsed.Contains(id))return false;p.coins-=cost;p.storyItems.Add(id);return true;}
 public static bool Ready(RogueProfile p,Chapter c){if(c.id=="neon-01-A")return p.storyItems.Contains("repair-pack")||p.storyItems.Contains("memory-unit")||p.storyUsed.Contains("repair-pack")||p.storyUsed.Contains("memory-unit");return p.legacyStoryAccess.Contains(c.id)||String.IsNullOrWhiteSpace(c.requiredItem)||p.storyUsed.Contains(c.requiredItem);}
 public static bool Use(RogueProfile p,Chapter c){p.Normalize();if(Ready(p,c))return false;if(!p.storyItems.Contains(c.requiredItem))return false;p.storyItems.Remove(c.requiredItem);p.storyUsed.Add(c.requiredItem);return true;}
}

public partial class Game {
 void AddStoryBag(FlowLayoutPanel bar){bar.Controls.Add(Mini("背包","","系统道具背包",ShowStoryBag));}
 void ShowStoryBag(){StopAudio();using(var dialog=PixelDialog("万象系统 · 道具背包",850,660)){
  var footer=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=65,Padding=new Padding(16,6,16,6),FlowDirection=FlowDirection.RightToLeft};dialog.Controls.Add(footer);footer.Controls.Add(Btn("前往系统商店",()=>{dialog.DialogResult=DialogResult.Yes;dialog.Close();},true));footer.Controls.Add(Btn("返回剧情",()=>dialog.Close()));
  var body=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(20,10,20,12)};dialog.Controls.Add(body);body.BringToFront();body.Controls.Add(Lab("你的系统装备",22,Gold));body.Controls.Add(Lab("金币  "+save.rogue.coins+"    ·    完成单词远征可获得金币",12,Muted));
  foreach(var id in StoryShopEngine.Items){bool used=save.rogue.storyUsed.Contains(id),owned=save.rogue.storyItems.Contains(id);string state=used?"能力已激活":owned?"已持有":"尚未购买";var card=new RogueCard{Width=750,Height=90,BackColor=GameTheme.Card,Padding=new Padding(8),Margin=new Padding(6,4,6,4),FlowDirection=FlowDirection.LeftToRight,WrapContents=false};card.Controls.Add(new StoryItemArt{Item=id,Size=new Size(65,66)});var text=new FlowLayoutPanel{Width=630,Height=72,FlowDirection=FlowDirection.TopDown,WrapContents=false,Margin=new Padding(0)};text.Controls.Add(Lab(StoryShopEngine.Name(id)+"   /   "+state,14,used||owned?Accent:Gold));var description=Lab(StoryShopEngine.Description(id),11,Muted);description.MaximumSize=new Size(600,0);text.Controls.Add(description);card.Controls.Add(text);body.Controls.Add(card);}
  Action layout=()=>{int width=Math.Max(300,body.ClientSize.Width-54);foreach(var card in body.Controls.OfType<RogueCard>()){card.Width=width;var text=(FlowLayoutPanel)card.Controls[1];text.Width=Math.Max(170,width-95);foreach(var label in text.Controls.OfType<Label>())label.MaximumSize=new Size(text.Width-12,0);}};body.Resize+=(s,e)=>layout();layout();if(dialog.ShowDialog(this)==DialogResult.Yes)ShowSystemShop();}}

 void ShowSystemShop(){
  ClearPage();page="system-shop";
  var shop=new SystemShopCanvas(CachedImage(System.IO.Path.Combine(root,"assets","shop","chapter-01.png")),save.rogue,StoryShopEngine.Items){Dock=DockStyle.Fill};
  shop.Purchase=id=>{if(!StoryShopEngine.Buy(save.rogue,id))return false;Persist();return true;};
  shop.PurchaseSound=PlayShopPurchase;
  shop.Return=()=>{if(current!=null&&save.hasGame)ShowStoryMap();else ShowMain();};shop.Home=ShowMain;content.Controls.Add(shop);shop.Focus();
 }
 void ShowStoryGate(){
  if(current.id=="neon-01-A"){RoguePage("无名诊所 · 支援装备准备");page="story-gate";var support=RogueCard("至少准备一件支援装备","应急修复包：恢复电力、帮助医生撤离。记忆存储器：保留证词和名单。两件都有可以同时救援与备份。设备永久拥有，不会因重玩再次扣费。");RogueActions(support,RogueButton("前往系统商店",ShowSystemShop,260),RogueButton("背单词赚金币",ShowRogueHome,260),RogueButton("返回主线地图",ShowStoryMap,220));RogueLayout();return;}
  RoguePage(current.title+" · 系统支援节点");page="story-gate";
  var scene=new RogueArena{Art=CachedImage(Engine.SafePath(folders[current.id],current.background)),Hero=RogueHero(),Height=170,Mode="home",BannerTitle="系统支援",BannerSubtitle="部署道具 · 帮助星遥",ShowDrone=false};rogueBody.Controls.Add(scene);
  var card=RogueCard("陈星遥需要你的帮助",current.itemScene);card.Controls.Add(RogueButton("查看本节词汇准备",ShowPreparationHome,300));string item=current.requiredItem;card.Controls.Add(new StoryItemArt{Item=item,Width=160,Height=116});card.Controls.Add(Lab("所需："+StoryShopEngine.Name(item)+" · 商店售价 "+StoryShopEngine.Price(item)+" 金币\n当前金币 "+save.rogue.coins+"。支援节点不计入听力用时，使用后进入本节。",12,Gold));
  var use=RogueButton("使用"+StoryShopEngine.Name(item)+" · 开始本节",()=>{if(StoryShopEngine.Use(save.rogue,current)){Persist();ShowStory();}},400);use.Enabled=save.rogue.storyItems.Contains(item);card.Controls.Add(use);RogueActions(card,RogueButton("前往系统商店",ShowSystemShop,260),RogueButton("背单词赚金币",ShowRogueHome,260),RogueButton("返回章节",ShowChapters,200));RogueLayout();
 }
}

public class StoryItemArt:Control {
 public string Item;static Image sheet;
 public StoryItemArt(){DoubleBuffered=true;}
 public static Rectangle Source(string id){switch(id){case "identity-shield":return new Rectangle(263,261,186,122);case "jammer":return new Rectangle(589,257,171,126);case "scanner":return new Rectangle(928,252,161,134);case "repair":return new Rectangle(1246,274,151,111);case "echo-probe":return new Rectangle(270,473,177,124);case "repair-pack":return new Rectangle(581,481,183,113);case "memory-unit":return new Rectangle(939,466,129,134);case "reserve-power":return new Rectangle(1258,464,130,136);default:return Rectangle.Empty;}}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(sheet==null)sheet=Image.FromFile(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"assets","shop","chapter-01.png"));var source=Source(Item);if(source.IsEmpty)return;float scale=Math.Min((Width-8)/(float)source.Width,(Height-8)/(float)source.Height);int w=(int)(source.Width*scale),h=(int)(source.Height*scale);e.Graphics.InterpolationMode=System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;e.Graphics.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.Half;e.Graphics.DrawImage(sheet,new Rectangle((Width-w)/2,(Height-h)/2,w,h),source,GraphicsUnit.Pixel);}
}
