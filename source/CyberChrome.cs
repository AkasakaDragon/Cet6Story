using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// Shared guild chrome for ordinary buttons, choices, dialogs and combat HUDs.
public static class CyberChrome {
 public static readonly Color Neon=Color.FromArgb(105,207,221),Amber=Color.FromArgb(211,182,123),Magenta=Color.FromArgb(171,119,182);
 public static void Panel(Graphics g,Rectangle bounds,Color accent,bool capsule=false,bool active=false,bool enabled=true){
  GuildChrome.Draw(g,bounds,false,enabled);
 }
 public static void Button(Graphics g,Rectangle bounds,string text,Font font,bool primary,bool hover,bool enabled,bool left=false){
  GuildChrome.Draw(g,bounds,primary,enabled);
  using(var ink=new SolidBrush(enabled?GuildChrome.Ivory:GuildChrome.Muted))
  using(var format=new StringFormat{Alignment=left?StringAlignment.Near:StringAlignment.Center,LineAlignment=StringAlignment.Center})
   GameTheme.DrawPixelString(g,text,font,ink,new RectangleF(bounds.Left+12,bounds.Top+5,Math.Max(1,bounds.Width-24),Math.Max(1,bounds.Height-10)),format);
 }
}