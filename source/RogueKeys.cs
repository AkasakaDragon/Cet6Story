using System;
using System.Windows.Forms;
public partial class Game {
 void InitRogueKeys(){KeyDown+=(sender,e)=>{if(e.Handled||e.Alt||e.Control)return;if((page!="rogue"&&page!="prep-combat"&&page!="tavern-battle")||rogueArena==null)return;var r=page=="tavern-battle"?save.tavernBattle:page=="prep-combat"?save.rogue.prepSession:save.rogue.ActiveRun;if(r==null)return;if(r.state=="combat"&&r.cardBattle!=null&&!r.cardBattle.answering&&e.KeyCode==Keys.Enter&&!(ActiveControl is BattleFanHand)){if(page=="tavern-battle"?EndTavernTurn(r):CardBattle.EndTurn(r))SaveRogue();e.Handled=true;e.SuppressKeyPress=true;return;}if(r.state=="combat"&&(r.cardBattle==null||r.cardBattle.answering)&&r.question.kind!=3&&e.KeyCode>=Keys.D1&&e.KeyCode<=Keys.D4){AnswerRogue((int)e.KeyCode-(int)Keys.D1,null);e.Handled=true;e.SuppressKeyPress=true;}else if((r.state=="feedback"||(page=="tavern-battle"&&(r.state=="won"||r.state=="lost")))&&e.KeyCode==Keys.Enter){if(page=="tavern-battle")ContinueTavernBattle();else if(page=="prep-combat")ContinuePreparation();else{RogueEngine.Continue(save.rogue);SaveRogue();}e.Handled=true;e.SuppressKeyPress=true;}};}
}


