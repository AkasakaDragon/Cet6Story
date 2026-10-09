using System;using System.Drawing;using System.Windows.Forms;using System.Collections.Generic;
public partial class Game {
 void ShowSpellWord(string word,string meaning,string example,string phonetic,bool favorite){
  var dialog=SpellDialog("单词释义",640,460);var dismiss=new WordPopupDismissFilter(dialog);Application.AddMessageFilter(dismiss);
  var header=new FlowLayoutPanel{Dock=DockStyle.Top,Height=62,WrapContents=false,BackColor=dialog.BackColor};dialog.Controls.Add(header);
  var title=new OutlinedLabel{Text=word,Font=GameTheme.Latin(23),ForeColor=GuildChrome.Ivory,Size=new Size(Math.Min(370,TextRenderer.MeasureText(word,GameTheme.Latin(23)).Width+8),48),Margin=new Padding(0,3,2,0)};header.Controls.Add(title);
  var hear=new RogueIcon{Kind="speaker",Size=new Size(38,38),Margin=new Padding(0,5,2,0),BackColor=dialog.BackColor,AccessibleName="朗读单词"};hear.Click+=(s,e)=>SpeakWord(word);header.Controls.Add(hear);
  var star=new RogueIcon{Kind="star",Selected=favorite,Enabled=favorite||!String.IsNullOrWhiteSpace(meaning),Size=new Size(38,38),Margin=new Padding(0,5,0,0),BackColor=dialog.BackColor,AccessibleName="收藏单词"};star.Click+=(s,e)=>ToggleRogueFavorite(new RogueEntry{word=word,meaning=meaning,example=example??""},star);header.Controls.Add(star);
  var pronunciation=new OutlinedLabel{Dock=DockStyle.Top,Height=35,Font=GameTheme.Latin(13),ForeColor=GuildChrome.Muted,Text=String.IsNullOrWhiteSpace(phonetic)?"":"/"+phonetic+"/"};dialog.Controls.Add(pronunciation);pronunciation.BringToFront();
  var definition=new OutlinedLabel{Dock=DockStyle.Fill,Font=GameTheme.Body(14),ForeColor=GuildChrome.Ivory};dialog.Controls.Add(definition);definition.BringToFront();
  var footer=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=46,WrapContents=false};dialog.Controls.Add(footer);footer.BringToFront();var pages=new List<string>();int at=0;string full=String.IsNullOrWhiteSpace(meaning)?"词典暂未收录此词。":meaning;
  Action refresh=()=>{definition.Text=pages[at];dialog.Text="单词释义"+(pages.Count>1?" · "+(at+1)+" / "+pages.Count+" 页":"");};
  Action paginate=()=>{pages.Clear();int width=Math.Max(50,dialog.ClientSize.Width-dialog.Padding.Horizontal-12),height=Math.Max(30,dialog.ClientSize.Height-dialog.Padding.Vertical-header.Height-pronunciation.Height-footer.Height-12);string rest=full;while(rest.Length>0){int low=1,high=rest.Length;while(low<high){int mid=(low+high+1)/2;if(TextRenderer.MeasureText(rest.Substring(0,mid),definition.Font,new Size(width,Int32.MaxValue),TextFormatFlags.WordBreak|TextFormatFlags.NoPadding).Height<=height)low=mid;else high=mid-1;}pages.Add(rest.Substring(0,low));rest=rest.Substring(low);}at=Math.Min(at,pages.Count-1);refresh();};
  footer.Controls.Add(PartyButton("上一页",()=>{at=Math.Max(0,at-1);refresh();},170,38));footer.Controls.Add(PartyButton("下一页",()=>{at=Math.Min(pages.Count-1,at+1);refresh();},170,38));footer.Controls.Add(PartyButton("关闭",()=>dialog.Close(),170,38));dialog.Resize+=(s,e)=>paginate();dialog.FormClosed+=(s,e)=>{Application.RemoveMessageFilter(dismiss);dialog.Dispose();};paginate();dialog.Show(this);dialog.Location=new Point(Left+(Width-dialog.Width)/2,Top+(Height-dialog.Height)/2);
 }
}
