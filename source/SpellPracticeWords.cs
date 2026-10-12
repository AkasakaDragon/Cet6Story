using System;using System.Drawing;using System.Windows.Forms;using System.Collections.Generic;using System.Linq;using System.Text.RegularExpressions;
public partial class Game {
 string CompactWordMeaning(string meaning){
  if(String.IsNullOrWhiteSpace(meaning))return "词典暂未收录此词。";
  meaning=meaning.Replace("\\n","\n");
  int baseAt=meaning.IndexOf("【原形释义】",StringComparison.Ordinal);if(baseAt>=0)meaning=meaning.Substring(baseAt+"【原形释义】".Length);
  int extra=meaning.IndexOf("【变形词补充释义】",StringComparison.Ordinal);if(extra>=0)meaning=meaning.Substring(0,extra);
  var parts=new List<string>();int length=0;
  foreach(string raw in Regex.Split(meaning,@"[；;。\r\n]+")){string part=raw.Trim();if(part.Length==0)continue;if(parts.Count>=6||length+part.Length>180)break;parts.Add(part);length+=part.Length;}
  return parts.Count>0?String.Join("；",parts)+"。":meaning.Trim();
 }
 void ShowSpellWord(string word,string meaning,string example,string phonetic,bool favorite){
  var dialog=SpellDialog("单词释义",640,560);var dismiss=new WordPopupDismissFilter(dialog);Application.AddMessageFilter(dismiss);
  var header=new FlowLayoutPanel{Dock=DockStyle.Top,Height=62,WrapContents=false,BackColor=dialog.BackColor};dialog.Controls.Add(header);
  var title=new OutlinedLabel{Text=word,Font=GameTheme.Latin(23),ForeColor=GuildChrome.Ivory,Size=new Size(Math.Min(370,TextRenderer.MeasureText(word,GameTheme.Latin(23),Size.Empty,TextFormatFlags.NoPadding|TextFormatFlags.SingleLine).Width+6),48),Margin=new Padding(0,3,0,0)};header.Controls.Add(title);
  var hear=new RogueIcon{Kind="speaker",Size=new Size(38,38),Margin=new Padding(-4,10,0,0),BackColor=dialog.BackColor,AccessibleName="朗读单词"};hear.Click+=(s,e)=>SpeakWord(word);header.Controls.Add(hear);
  var star=new RogueIcon{Kind="star",Selected=favorite,Enabled=favorite||!String.IsNullOrWhiteSpace(meaning),Size=new Size(38,38),Margin=new Padding(0,10,0,0),BackColor=dialog.BackColor,AccessibleName="收藏单词"};star.Click+=(s,e)=>ToggleRogueFavorite(new RogueEntry{word=word,meaning=meaning,example=example??""},star);header.Controls.Add(star);
  var pronunciation=new OutlinedLabel{Dock=DockStyle.Top,Height=35,Font=GameTheme.Latin(13),ForeColor=GuildChrome.Muted,Text=String.IsNullOrWhiteSpace(phonetic)?"":"/"+phonetic+"/"};dialog.Controls.Add(pronunciation);pronunciation.BringToFront();
  var definition=new OutlinedLabel{Dock=DockStyle.Fill,Font=GameTheme.Body(14),ForeColor=GuildChrome.Ivory};dialog.Controls.Add(definition);definition.BringToFront();
  string full=CompactWordMeaning(meaning);string examples=WordExamplesText(word,example);if(!String.IsNullOrWhiteSpace(examples))full+="\n\n"+examples;
  definition.Text=full;
  Action fit=()=>{int width=Math.Max(60,definition.ClientSize.Width-12),height=Math.Max(40,definition.ClientSize.Height-12);float size=14;while(size>8&&TextRenderer.MeasureText(full,GameTheme.Body(size),new Size(width,Int32.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height>height)size-=0.5f;definition.Font=GameTheme.Body(size);};
  dialog.Resize+=(s,e)=>fit();dialog.Shown+=(s,e)=>fit();dialog.FormClosed+=(s,e)=>{Application.RemoveMessageFilter(dismiss);dialog.Dispose();};fit();if(wordPreviewFile!=null){dialog.Opacity=0;dialog.Show(this);Application.DoEvents();using(var bitmap=new Bitmap(dialog.Width,dialog.Height)){dialog.DrawToBitmap(bitmap,new Rectangle(0,0,dialog.Width,dialog.Height));bitmap.Save(wordPreviewFile);}dialog.Close();}else{dialog.Show(this);dialog.Location=new Point(Left+(Width-dialog.Width)/2,Top+(Height-dialog.Height)/2);}
 }
}
