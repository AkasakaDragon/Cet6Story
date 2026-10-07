using System;using System.Linq;using System.Drawing;using System.Collections.Generic;
public class CombatStatusView {public string Key,Name,Detail;public int Count;public Color Color;public string Pixels;}
public static class CombatStatusPixels {
 public static List<CombatStatusView> For(PartyUnit u){var list=new List<CombatStatusView>();Action<string,string,int,Color,string,string> add=(k,n,c,color,p,d)=>{if(c>0)list.Add(new CombatStatusView{Key=k,Name=n,Count=c,Color=color,Pixels=p,Detail=d});};
 add("block","格挡",u.block,Color.FromArgb(131,182,225),"111111010","下一次直接伤害减半，每次受击消耗1枚。");
 add("dodge","闪避",u.dodge,Color.FromArgb(129,210,198),"011110011","下一次攻击有50%概率闪避，无论成功与否消耗1枚。");
 add("strength","强力",u.strength,Color.FromArgb(236,191,104),"010111010","下一次攻击伤害+50%，群攻只消耗1枚。");
 add("weak","虚弱",u.weak,Color.FromArgb(166,148,185),"010010111","下一次攻击伤害减半。");
 add("vulnerable","易伤",u.vulnerable,Color.FromArgb(224,145,162),"101010101","下一次直接伤害+50%。");
 foreach(var k in new[]{"bleed","blight","burn"}){var dots=u.dots.Where(d=>d.kind==k).ToList();int turns=dots.Select(d=>d.turns).DefaultIfEmpty().Max();add(k,k=="bleed"?"流血":k=="blight"?"菌毒":"灼烧",turns,k=="bleed"?Color.FromArgb(224,91,94):k=="blight"?Color.FromArgb(155,185,103):Color.FromArgb(236,156,79),k=="bleed"?"010111011":k=="blight"?"101111101":"010101111","自身行动开始损失"+dots.Sum(d=>d.damage)+"生命，无视护盾；剩余"+turns+"次。同类再次施加：伤害叠加，剩余次数+1。" );}
 add("guard","守护",String.IsNullOrEmpty(u.guard)?0:1,Color.FromArgb(135,190,149),"110101110","替指定队友承受下一次单体攻击，不替挡群攻。");
 add("riposte","反击",u.riposte,Color.FromArgb(199,151,205),"101011101","受到直接攻击后反击50%；不触发连锁反击。");
 add("anchor","不退",u.kind=="boss"?1:u.anchored,Color.FromArgb(185,170,131),"010010111","免疫敌方强制位移，可主动移动。");
 add("silence","封咒",u.silence,Color.FromArgb(215,137,163),"101111101","禁止增益或施法准备；取消法术准备，不取消物理装填。");
 add("slow","迟缓",u.slow,Color.FromArgb(160,181,208),"111010110","下一回合速度-2。");
 add("marks","符印",u.marks,Color.FromArgb(161,158,234),"010101010","最多3层；用于骑士精准、引爆和封咒；剩余"+u.markTurns+"次自身行动。");
 add("curse","咒印",u.runeBoost,Color.FromArgb(159,146,221),"010101010","每累计2次攻击答题成功获得1层，最多3层；与施加给敌人的符印独立。当前攻击答题进度："+u.attackSuccesses+" / 2。");
 add("resolve","决意",u.resolve,Color.FromArgb(240,198,97),"101111010","格挡或护盾承伤积累；每累计2次防御答题成功额外获得1层，最多3层；供斩杀、突进和激励使用。当前防御答题进度："+u.defenseSuccesses+" / 2。");
 add("shield","护盾",u.shield,Color.FromArgb(112,183,237),"111101010","先吸收直接伤害，最多最大生命40%，持续伤害绕过。");
 add("ready","准备",String.IsNullOrEmpty(u.preparation)?0:1,Color.FromArgb(241,198,126),"111010111",u.kind=="boss"?"剧毒孢潮蓄力中，可用盾击或封咒打断；母株免疫位移。":"下一回合轮到自身释放；打断或被移出3/4号位取消。");
 add("rage","狂暴",u.enraged?1:0,Color.FromArgb(212,133,98),"101111101","母株半血狂暴：攻击+6，速度5；不再收拢菌冠获得格挡。");
 add("cool","散热",u.cooling?1:0,Color.FromArgb(171,181,184),"101010010","下一次行动必须散热，不能连续开炮。");return list;}
 public static void Draw(Graphics g,CombatStatusView v,Rectangle r){int cell=Math.Max(2,(r.Height-4)/3);int x=r.X+2,y=r.Y+2;using(var shade=new SolidBrush(Color.FromArgb(205,10,23,27)))g.FillRectangle(shade,r);using(var ink=new SolidBrush(v.Color))for(int i=0;i<9;i++)if(v.Pixels[i]=='1')g.FillRectangle(ink,x+i%3*cell,y+i/3*cell,cell,cell);using(var font=GameTheme.Latin(8,FontStyle.Bold))GameTheme.DrawText(g,v.Count.ToString(),font,new Rectangle(x+cell*3+2,y,18,r.Height),Color.FromArgb(245,233,208),System.Windows.Forms.TextFormatFlags.NoPadding);}
}
public static class PartyMonsterArt {
 static readonly Dictionary<string,Image> images=new Dictionary<string,Image>();
 public static Image Get(string root,string kind,Image fallback){if(String.IsNullOrEmpty(kind))return fallback;if(kind=="boss"){string boss=System.IO.Path.Combine(root,"assets","dungeons","toxic-woodland","corrupt-crown-boss.png");Image cached;if(images.TryGetValue("boss",out cached))return cached;if(System.IO.File.Exists(boss)){cached=Image.FromFile(boss);images["boss"]=cached;return cached;}return fallback;}Image found;if(images.TryGetValue(kind,out found))return found;string path=System.IO.Path.Combine(root,"assets","monsters","actions","monster-actions-v2.png");if(!System.IO.File.Exists(path))return fallback;
 int row=kind=="crab"?1:kind=="moth"?2:kind=="bard"?3:kind=="cannon"?4:0;
 using(var sheet=new Bitmap(path)){found=sheet.Clone(MonsterAnimationArt.Cell(sheet,row,0),System.Drawing.Imaging.PixelFormat.Format32bppArgb);images[kind]=found;return found;}
 }
}
