using System;using System.Linq;using System.Drawing;using System.Windows.Forms;

public sealed class TavernBattleGuide:Control {
 public PartyCombatState Battle;public bool Collapsed;public Action Toggle;
 public TavernBattleGuide(){DoubleBuffered=true;ResizeRedraw=true;Cursor=Cursors.Hand;AccessibleName="序幕战斗教程；点击收起或展开";}
 public static string Heading(PartyCombatState b){if(b.outcome=="won")return "战斗完成";if(b.outcome=="lost")return "可以重新挑战";if(b.phase=="defense-question")return "防御 · 答题减少伤害";if(b.phase=="attack-question")return "发动 · 选择正确词义";if(b.phase=="feedback"||b.phase=="enemy-feedback")return "结算 · 查看技能效果";return b.questions==0?"行动 · 角色 → 技能 → 目标":"配合 · 每名角色每回合行动一次";}
 public static string Instruction(PartyCombatState b){
  if(b.outcome=="won")return "点击战斗结算中的继续，回到剧情。艾莉娅会与你一起修好驿站。";
  if(b.outcome=="lost")return "失败可以重试。先留意双方生命与护盾，再选择攻击、守护或治疗。";
  if(b.phase=="attack-question")return "选择单词的正确释义（也可按1—4）。答对发动完整技能；答错基础效果减半，无附加效果。";
  if(b.phase=="defense-question")return "敌人正在攻击！答对本题可让伤害减半，护盾会优先吸收伤害。答错则承受完整伤害。";
  if(b.phase=="feedback"||b.phase=="enemy-feedback")return "查看生命、护盾与状态变化，再点击继续（或按Enter）。继续后按右上角速度队列轮到下一个单位，双方穿插行动。";
  var hero=b.heroes[b.selectedHero];var skill=HeroSkills.Get(hero.id,b.selectedSkill);
  string first=b.questions==0?"先看右上角轮到谁。黄色圆点为施法位置，蓝色为目标；陆川先手，艾莉娅守护。\n":"双方按速度穿插行动，每单位每回合一次；前移、后移也消耗行动。提前结束会放弃剩余行动。\n";
  return first+"当前："+hero.name+" · "+(skill==null?"选择技能":skill.Name)+"。确认右侧目标，点击施放技能。灰色技能需查看冷却或剩余次数。";
 }
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(Battle==null)return;var g=e.Graphics;using(var b=new SolidBrush(Color.FromArgb(12,29,33)))g.FillRectangle(b,ClientRectangle);using(var p=new Pen(GuildChrome.Gold,1))g.DrawRectangle(p,1,1,Math.Max(0,Width-3),Math.Max(0,Height-3));using(var f=GameTheme.Body(11,FontStyle.Bold))GameTheme.DrawText(g,"新手引导 · "+Heading(Battle),f,new Rectangle(12,9,Width-78,27),GuildChrome.Gold,TextFormatFlags.NoPadding|TextFormatFlags.EndEllipsis);using(var f=GameTheme.Body(9))GameTheme.DrawText(g,Collapsed?"展开":"收起",f,new Rectangle(Width-52,12,42,25),GuildChrome.Muted,TextFormatFlags.NoPadding);if(!Collapsed)using(var f=GameTheme.Body(10))GameTheme.DrawText(g,Instruction(Battle),f,new Rectangle(12,43,Width-24,Height-51),GuildChrome.Ivory,TextFormatFlags.WordBreak|TextFormatFlags.NoPadding);}
 protected override void OnMouseClick(MouseEventArgs e){base.OnMouseClick(e);if(e.Button==MouseButtons.Left&&Toggle!=null)Toggle();}
}
public partial class Game {
 bool tavernGuideCollapsed;
 TavernBattleGuide AddTavernBattleGuide(PartyBattleCanvas arena,PartyCombatState battle){var guide=new TavernBattleGuide{Battle=battle,Collapsed=tavernGuideCollapsed};guide.Toggle=()=>{tavernGuideCollapsed=!tavernGuideCollapsed;guide.Collapsed=tavernGuideCollapsed;guide.Height=guide.Collapsed?44:154;guide.Invalidate();};arena.Controls.Add(guide);return guide;}
}
