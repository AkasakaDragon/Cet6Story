using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

public static class StoryShopEngine {
 public static readonly string[] Items={"identity-shield","jammer","scanner","repair","echo-probe","repair-pack","memory-unit","reserve-power"};
 public static int Price(string id){if(id=="echo-probe")return 240;if(id=="repair-pack"||id=="reserve-power")return 300;if(id=="memory-unit")return 360;return id=="identity-shield"?120:id=="jammer"?120:id=="scanner"?180:id=="repair"?240:0;}
 public static string Name(string id){if(id=="echo-probe")return "回声探针";if(id=="repair-pack")return "应急修复包";if(id=="memory-unit")return "记忆存储器";if(id=="reserve-power")return "备用能源";return id=="identity-shield"?"临时身份屏蔽器":id=="jammer"?"信号干扰器":id=="scanner"?"档案扫描器":id=="repair"?"协议修复芯片":"未知道具";}
 public static string Description(string id){if(id=="echo-probe")return "1.1 两星通关后定位求救信号，解锁隐藏 A 无名诊所。";if(id=="repair-pack")return "无名诊所：恢复备用电源，护送医生安全撤离。";if(id=="memory-unit")return "无名诊所：备份证词和转移名单，为后续调查保留证据。";if(id=="reserve-power")return "1.2 两星通关后记录零号站台坐标；该隐藏关将在后续扩展。";return id=="identity-shield"?"1.1 雨夜追踪：建立临时身份屏蔽，争取穿过雨巷的安全窗口。":id=="jammer"?"1.2 地下末班车：遮蔽身份追踪，让星遥启动废弃列车。":id=="scanner"?"1.3 失名档案：读取被隐藏的原始记录，寻找星遥父亲留下的证据。":"1.4 黎明信号：修复公共广播协议，公开证据并恢复失名者的身份。";}
 public static bool Ready(RogueProfile p,Chapter c){return true;}
 public static bool Use(RogueProfile p,Chapter c){p.Normalize();if(Ready(p,c))return false;if(!p.storyItems.Contains(c.requiredItem))return false;p.storyItems.Remove(c.requiredItem);p.storyUsed.Add(c.requiredItem);return true;}
}
