using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

// Render whole lines with normal font spacing; hit regions never affect text layout.
public class SentenceView:ScrollableControl {
 class Hit {public Rectangle Rect;public string Word;}
 List<Hit> hits=new List<Hit>();
 public HashSet<string> Marked=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
 public Action<string> WordClicked;public Action BlankClicked;public bool Shadow=false;
 int selected=-1;
 static TextFormatFlags Flags=TextFormatFlags.NoPadding|TextFormatFlags.NoPrefix|TextFormatFlags.SingleLine|TextFormatFlags.PreserveGraphicsClipping;
 public SentenceView(){SetStyle(ControlStyles.SupportsTransparentBackColor,true);DoubleBuffered=true;AutoScroll=true;TabStop=true;Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.Text;AccessibleName="英文对话，可点击单词查看释义";}
 protected override void OnTextChanged(EventArgs e){base.OnTextChanged(e);selected=-1;AutoScrollPosition=Point.Empty;Invalidate();}
 public int WrappedLineCount(int width){using(var g=CreateGraphics()){int rows=1;string line="";foreach(Match t in Regex.Matches(Text??"",@"\S+")){string next=line.Length==0?t.Value:line+" "+t.Value;if(line.Length>0&&Measure(g,next)>width){rows++;line=t.Value;}else line=next;}return rows;}}
 int Measure(Graphics g,string text){return TextRenderer.MeasureText(g,text,Font,new Size(int.MaxValue,int.MaxValue),Flags).Width;}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);hits.Clear();int width=Math.Max(30,ClientSize.Width-22),lineHeight=(int)Math.Ceiling(Font.Height*1.55),row=0;string line="";var lines=new List<string>();foreach(Match t in Regex.Matches(Text??"",@"\S+")){string next=line.Length==0?t.Value:line+" "+t.Value;if(line.Length>0&&Measure(e.Graphics,next)>width){lines.Add(line);line=t.Value;}else line=next;}if(line.Length>0)lines.Add(line);foreach(string content in lines){int y=8+row*lineHeight+AutoScrollPosition.Y;var rect=new Rectangle(0,y,width,lineHeight);if(Shadow){foreach(var offset in new[]{new Point(-1,0),new Point(1,0),new Point(0,-1),new Point(0,1)}){var shadowRect=rect;shadowRect.Offset(offset);TextRenderer.DrawText(e.Graphics,content,Font,shadowRect,Color.FromArgb(22,13,26),Flags);}}TextRenderer.DrawText(e.Graphics,content,Font,rect,ForeColor,Flags);foreach(Match m in Regex.Matches(content,"[A-Za-z]+(?:['’][A-Za-z]+)?")){int left=Measure(e.Graphics,content.Substring(0,m.Index));int right=Measure(e.Graphics,content.Substring(0,m.Index+m.Length));var hit=new Hit{Rect=new Rectangle(left,y,Math.Max(1,right-left),lineHeight),Word=m.Value.ToLowerInvariant()};hits.Add(hit);if(Marked.Contains(hit.Word)){var state=e.Graphics.Save();e.Graphics.SetClip(hit.Rect);TextRenderer.DrawText(e.Graphics,content,Font,rect,Color.FromArgb(255,207,130),Flags);e.Graphics.Restore(state);}if(Focused&&hits.Count-1==selected)ControlPaint.DrawFocusRectangle(e.Graphics,hit.Rect);}row++;}var needed=new Size(0,row*lineHeight+12);if(AutoScrollMinSize!=needed)AutoScrollMinSize=needed;}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button!=MouseButtons.Left)return;Focus();var hit=hits.FirstOrDefault(x=>x.Rect.Contains(e.Location));if(hit!=null&&WordClicked!=null)WordClicked(hit.Word);else if(hit==null&&BlankClicked!=null)BlankClicked();}
 protected override bool IsInputKey(Keys keyData){return keyData==Keys.Left||keyData==Keys.Right||base.IsInputKey(keyData);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(hits.Count==0)return;if(e.KeyCode==Keys.Right||e.KeyCode==Keys.Left){selected=(selected+(e.KeyCode==Keys.Right?1:-1)+hits.Count)%hits.Count;Invalidate();e.Handled=true;}if(e.KeyCode==Keys.Enter&&selected>=0&&WordClicked!=null){WordClicked(hits[selected].Word);e.Handled=true;}}
}

