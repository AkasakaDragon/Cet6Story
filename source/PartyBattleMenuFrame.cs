using System;
using System.Drawing;
using System.Drawing.Drawing2D;

// Clip the dialog to its stepped frame so the battlefield remains visible outside it.
public sealed class PartyBattleMenuFrame:PixelFrame {
 public PartyBattleMenuFrame(){BackColor=Color.FromArgb(25,53,55);Resize+=(sender,e)=>FitFrame();FitFrame();}
 void FitFrame(){if(ClientSize.Width<24||ClientSize.Height<24)return;using(var path=new GraphicsPath()){path.AddPolygon(GameTheme.Outline(new Rectangle(5,5,ClientSize.Width-11,ClientSize.Height-11),11));var previous=Region;Region=new Region(path);if(previous!=null)previous.Dispose();}}
}
