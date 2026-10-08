using System.Drawing;using System.Windows.Forms;
public sealed class GoldCheckBox:CheckBox {
 public GoldCheckBox(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.SupportsTransparentBackColor,true);BackColor=Color.Transparent;Cursor=Cursors.Hand;}
 protected override void OnPaint(PaintEventArgs e){base.OnPaintBackground(e);var g=e.Graphics;int side=14,y=(Height-side)/2;var box=new Rectangle(1,y,side,side);using(var fill=new SolidBrush(Color.FromArgb(10,38,36)))g.FillRectangle(fill,box);using(var edge=new Pen(Checked?Color.FromArgb(255,218,100):GuildChrome.Muted,1))g.DrawRectangle(edge,box);if(Checked)using(var tick=new Pen(Color.FromArgb(255,223,90),2)){g.DrawLines(tick,new[]{new Point(4,y+7),new Point(7,y+10),new Point(12,y+4)});}TextRenderer.DrawText(g,Text,Font,new Rectangle(21,0,System.Math.Max(1,Width-21),Height),Enabled?ForeColor:GuildChrome.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.NoPadding);
 }
}
