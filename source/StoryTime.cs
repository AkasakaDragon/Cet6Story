using System;
using System.Drawing;
using System.Windows.Forms;

// Narrative time belongs to the scene, so replay and previous-line navigation
// restore it without advancing the tavern's daily reward counter.
public static class StoryTime {
 public const string Morning="morning", Dusk="dusk", Night="night";
 public static string Phase(Chapter chapter,Line line){
  if(line!=null&&!String.IsNullOrEmpty(line.timeOfDay))return line.timeOfDay;
  return chapter==null?null:chapter.timeOfDay;
 }
 public static string Caption(string phase){
  return phase==Morning?"早上":phase==Dusk?"黄昏":phase==Night?"晚上":"";
 }
 public static void AddBadge(Control parent,string phase){
  string caption=Caption(phase);if(caption.Length==0)return;
  var badge=new OutlinedLabel{PixelText=true,Text="第一天 · "+caption,Font=GameTheme.Body(12),ForeColor=GameTheme.Gold,BackColor=Color.Transparent,Location=new Point(22,18),Size=new Size(180,34)};
  parent.Controls.Add(badge);badge.BringToFront();
 }
}

