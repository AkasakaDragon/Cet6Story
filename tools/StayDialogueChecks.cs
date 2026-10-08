using System;using System.Drawing;using System.Linq;using System.Reflection;using System.Windows.Forms;using System.IO;
class StayDialogueChecks {
 [STAThread]static void Main(){var f=BindingFlags.NonPublic|BindingFlags.Instance;using(var g=new Game()){
  g.Show();var t=typeof(Game);var chapters=(System.Collections.Generic.List<Chapter>)t.GetField("chapters",f).GetValue(g);var original=chapters.Single(c=>c.id=="tavern-01-06");
  var save=(Save)t.GetField("save",f).GetValue(g);save.completed.Add("tavern-01-05");save.sectionAttempts[original.id]=new SectionAttempt{review=true,introShown=true};save.positions[original.id]=29;save.english=save.chinese=true;
  t.GetField("current",f).SetValue(g,original);t.GetField("index",f).SetValue(g,29);t.GetMethod("ShowStory",f).Invoke(g,null);int normalHeight=((Control)t.GetField("dialogue",f).GetValue(g)).Height;
  t.GetMethod("ShowChapterOneStayEpilogue",f).Invoke(g,null);var stage=(ArtPanel)t.GetField("stage",f).GetValue(g);var dialog=(Control)t.GetField("dialogue",f).GetValue(g);
  if(stage.Actors.Count!=0||dialog.Height<normalHeight)throw new Exception("portrait or shared dialogue layout");
  var bar=stage.Controls.OfType<FlowLayoutPanel>().Single();var buttons=bar.Controls.OfType<VNButton>().ToArray();if(buttons.Length!=11||buttons.Any(b=>b.Text=="上一句"||b.Text=="下一句"))throw new Exception("nonstandard toolbar");
  foreach(var size in new[]{new Size(800,500),new Size(1280,780),new Size(1920,1080)}){g.Size=size;Application.DoEvents();if(stage.Bounds!=stage.Parent.ClientRectangle)throw new Exception("scene coverage");var area=stage.SceneArtBounds();if(area.Width<stage.Width||area.Height<stage.Height)throw new Exception("letterbox");}
  for(int i=0;i<7;i++)t.GetMethod("Next",f).Invoke(g,null);if((int)t.GetField("index",f).GetValue(g)!=7)throw new Exception("next");
  t.GetMethod("SetStorySubtitle",f).Invoke(g,new object[]{true,true});
  foreach(var size in new[]{new Size(800,500),new Size(1280,780),new Size(1920,1080)}){g.Size=size;Application.DoEvents();var en=(SentenceView)t.GetField("englishText",f).GetValue(g);var zh=(Control)t.GetField("translationLabel",f).GetValue(g);if(en.Height<8+en.WrappedLineCount(en.Width-22)*(int)Math.Ceiling(en.Font.Height*1.55))throw new Exception("English clipped");int needed=TextRenderer.MeasureText(zh.Text,zh.Font,new Size(zh.Width-5,int.MaxValue),TextFormatFlags.NoPadding|TextFormatFlags.WordBreak).Height+4;if(zh.Height<needed)throw new Exception("Chinese clipped");}
  for(int i=0;i<7;i++){t.GetMethod("Previous",f).Invoke(g,null);t.GetMethod("StopAudio",f).Invoke(g,null);}if((int)t.GetField("index",f).GetValue(g)!=0||save.positions[original.id]!=29)throw new Exception("previous/save position");
  t.GetMethod("SetStorySubtitle",f).Invoke(g,new object[]{false,false});if(((Control)t.GetField("englishText",f).GetValue(g)).Visible)throw new Exception("subtitle off");t.GetMethod("SetStorySubtitle",f).Invoke(g,new object[]{false,true});
  using(var bmp=new Bitmap(stage.Width,stage.Height)){stage.DrawToBitmap(bmp,stage.ClientRectangle);bmp.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"stay-standard-ui.png"));}
  t.GetField("index",f).SetValue(g,7);t.GetMethod("Next",f).Invoke(g,null);if(!Object.ReferenceEquals(t.GetField("current",f).GetValue(g),original)||!save.completed.Contains(original.id)||!save.storyFlags.Contains("waystation-chapter-one-complete"))throw new Exception("completion");
  Console.WriteLine("PASS shared dialogue height/11 toolbar controls, no portraits, 3 sizes, 8-line navigation, subtitle toggle, original progress and chapter completion");
 }}
}
