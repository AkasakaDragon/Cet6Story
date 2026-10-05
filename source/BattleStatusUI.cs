using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

public partial class RogueArena {
 readonly ToolTip statusTips=new ToolTip{InitialDelay=200,ReshowDelay=80,AutoPopDelay=15000,ShowAlways=true};readonly List<KeyValuePair<Rectangle,string>> statusAreas=new List<KeyValuePair<Rectangle,string>>();string activeStatusTip;
 void DrawStatuses(Graphics g,int x,int y,int width,bool enemy){
  if(!enemy)statusAreas.Clear();if(Run==null||Run.cardBattle==null)return;var b=Run.cardBattle;int slot=0;
  Action<string,int,Color,int> add=(name,count,color,shape)=>{if(count<=0)return;int left=x+slot%7*36,top=y-slot/7*30;slot++;var rect=new Rectangle(left,top,34,28);statusAreas.Add(new KeyValuePair<Rectangle,string>(rect,StatusExplanation(name,count)));var saved=g.Save();g.TranslateTransform(left,top);g.ScaleTransform(1.3f,1.3f);using(var ink=new SolidBrush(color)){
   if(shape==0)g.FillPolygon(ink,new[]{new Point(2,2),new Point(14,2),new Point(14,10),new Point(8,16),new Point(2,10)});
   else if(shape==1){g.FillPolygon(ink,new[]{new Point(2,14),new Point(1,8),new Point(5,10),new Point(8,0),new Point(10,7),new Point(14,4),new Point(15,13),new Point(9,17)});using(var core=new SolidBrush(Color.FromArgb(255,217,106)))g.FillRectangle(core,6,10,5,6);}
   else if(shape==2){g.FillPolygon(ink,new[]{new Point(8,0),new Point(15,7),new Point(10,8),new Point(14,16),new Point(7,11),new Point(2,16),new Point(3,6)});}
   else if(shape==3){g.FillRectangle(ink,6,0,5,13);g.FillRectangle(ink,2,10,13,3);g.FillRectangle(ink,7,13,3,5);}
   else if(shape==4){g.FillEllipse(ink,1,1,9,9);g.FillEllipse(ink,7,6,9,9);g.FillEllipse(ink,1,10,7,7);}
   else {g.FillPolygon(ink,new[]{new Point(8,0),new Point(15,8),new Point(8,16),new Point(1,8)});using(var core=new SolidBrush(Color.FromArgb(22,27,36)))g.FillRectangle(core,6,5,4,6);}
  }g.Restore(saved);using(var font=GameTheme.Latin(10,FontStyle.Bold))GameTheme.DrawText(g,count.ToString(),font,new Rectangle(left+17,top+10,20,18),Color.White,TextFormatFlags.NoPadding);};
  if(enemy){add("护盾 · 抵挡攻击伤害",b.enemyShield,Color.FromArgb(91,180,242),0);add("燃烧 · 行动结束扣血，随后减少 2 层",b.burning,Color.FromArgb(230,123,55),1);add("易伤 · 受到攻击伤害增加 25%，剩余回合",b.vulnerable,Color.FromArgb(221,111,150),2);add("热量 · 强化怪物技能",b.heat,Color.FromArgb(225,151,85),1);}
  else {add("护盾 · 抵挡伤害",b.shield,Color.FromArgb(91,180,242),0);add("增伤 · 本回合额外攻击伤害",b.bonus,Color.FromArgb(230,181,90),3);add("增伤百分比",b.percent,Color.FromArgb(233,121,104),3);add("孢子 · 强化孢子引爆",b.spores,Color.FromArgb(171,186,107),4);add("杂音 · 削弱连击增伤",b.noise,Color.FromArgb(170,142,211),5);add("攻击连击",b.attackChain,Color.FromArgb(109,212,192),3);add("精准判断 · 无提示答对增加抽牌",b.judgement,Color.FromArgb(115,182,223),5);add("词义催化 · 无提示答对增加燃烧",b.catalyst,Color.FromArgb(206,135,220),1);add("逆转之光 · 免死一次",b.rescue?1:0,Color.FromArgb(235,216,148),5);}
 }
 void DrawStatusBars(Graphics g,Rectangle track,int value,int max,bool enemy){if(Run==null||Run.cardBattle==null)return;var b=Run.cardBattle;int shield=enemy?b.enemyShield:b.shield;float unit=track.Width/(float)Math.Max(1,Math.Max(max,value+shield));int fill=(int)(Math.Max(0,value)*unit);
  if(enemy&&b.burning>0){int damage=Math.Min(value,b.burning),w=Math.Min(fill,(int)Math.Ceiling(damage*unit));using(var shadow=new SolidBrush(Color.FromArgb(190,0,0,0)))g.FillRectangle(shadow,track.Left+fill-w,track.Top,w,track.Height);}
  if(enemy&&AnimateHit&&Mode=="feedback"&&b.burnDamage>0&&frame<52){int w=Math.Min(track.Width-fill,(int)Math.Ceiling(b.burnDamage*unit));using(var shadow=new SolidBrush(Color.FromArgb(frame<44?215:Math.Max(0,215-(frame-44)*26),0,0,0)))g.FillRectangle(shadow,track.Left+fill,track.Top,w,track.Height);}
  if(shield>0){int w=Math.Min(track.Width-fill,Math.Max(1,(int)Math.Ceiling(shield*unit)));using(var blue=new SolidBrush(Color.FromArgb(88,180,246)))g.FillRectangle(blue,track.Left+fill,track.Top,w,track.Height);}
 }
 public void HoverStatus(Control source,Point position){string tip=null;foreach(var item in statusAreas)if(item.Key.Contains(position)){tip=item.Value;break;}if(tip!=activeStatusTip){statusTips.RemoveAll();activeStatusTip=tip;statusTips.SetToolTip(source,tip);}}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);HoverStatus(this,e.Location);}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);HoverStatus(this,new Point(-1,-1));}
 static string StatusExplanation(string name,int value){string title=name.Split('·')[0].Trim();string effect=title=="护盾"?"受到攻击时先消耗护盾，剩余伤害才扣除生命。蓝色条段表示当前护盾。":title=="燃烧"?"敌方行动结束时受到等于层数的伤害，无视护盾；结算后减少 2 层。黑色区段表示预计扣血。":title=="易伤"?"受到攻击伤害增加 25%；每次敌方行动结束减少 1 回合。":title=="增伤百分比"?"本回合答题攻击伤害提高 "+value+"%。":title=="攻击连击"?"本回合累计攻击次数；部分卡牌会根据次数获得额外效果，下一回合重新计数。":title=="逆转之光"?"抵挡一次致命伤害，保留 1 点生命，并获得 8 点护盾。":name.Contains("·")?name.Substring(name.IndexOf('·')+1).Trim():name;return title+" · "+value+"\n"+effect;}
}
