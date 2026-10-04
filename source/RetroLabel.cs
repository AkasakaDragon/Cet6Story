using System.Drawing;
using System.Windows.Forms;

// Label keeps its native measuring/layout behavior; only its glyph painting changes.
public class RetroLabel : Label {
 protected override void OnPaint(PaintEventArgs e){
  var bounds=new Rectangle(Padding.Left,Padding.Top,System.Math.Max(1,Width-Padding.Horizontal),System.Math.Max(1,Height-Padding.Vertical));
  var flags=TextFormatFlags.WordBreak|TextFormatFlags.TextBoxControl;
  if(AutoEllipsis)flags|=TextFormatFlags.EndEllipsis;
  if(TextAlign==ContentAlignment.TopCenter||TextAlign==ContentAlignment.MiddleCenter||TextAlign==ContentAlignment.BottomCenter)flags|=TextFormatFlags.HorizontalCenter;
  if(TextAlign==ContentAlignment.TopRight||TextAlign==ContentAlignment.MiddleRight||TextAlign==ContentAlignment.BottomRight)flags|=TextFormatFlags.Right;
  if(TextAlign==ContentAlignment.MiddleLeft||TextAlign==ContentAlignment.MiddleCenter||TextAlign==ContentAlignment.MiddleRight)flags|=TextFormatFlags.VerticalCenter;
  if(TextAlign==ContentAlignment.BottomLeft||TextAlign==ContentAlignment.BottomCenter||TextAlign==ContentAlignment.BottomRight)flags|=TextFormatFlags.Bottom;
  GameTheme.DrawText(e.Graphics,Text,Font,bounds,Enabled?ForeColor:GameTheme.Muted,flags);
 }
}
