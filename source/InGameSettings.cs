using System;using System.Drawing;using System.Linq;using System.Windows.Forms;

public partial class Game {
 Form inGameSettings;
 IMessageFilter escapeSettingsFilter;
 sealed class EscapeSettingsFilter:IMessageFilter {
  readonly Game game;
  public EscapeSettingsFilter(Game owner){game=owner;}
  public bool PreFilterMessage(ref Message message){
   if(game.IsDisposed||message.Msg!=0x100&&message.Msg!=0x104||message.WParam.ToInt32()!=(int)Keys.Escape)return false;
   if((message.LParam.ToInt64()&(1L<<30))!=0)return true;
   var control=Control.FromHandle(message.HWnd);var form=control==null?Form.ActiveForm:control.FindForm();var owner=form;
   while(owner!=null&&owner!=game)owner=owner.Owner;
   if(owner!=game)return false;
   if(game.page=="home"&&(game.inGameSettings==null||game.inGameSettings.IsDisposed))return true;
   game.ShowInGameSettings(form);return true;
  }
 }
 void InitEscapeSettings(){escapeSettingsFilter=new EscapeSettingsFilter(this);Application.AddMessageFilter(escapeSettingsFilter);FormClosed+=(s,e)=>Application.RemoveMessageFilter(escapeSettingsFilter);}
 void ShowInGameSettings(Form owner){
  if(inGameSettings!=null&&!inGameSettings.IsDisposed){inGameSettings.Close();return;}
  string previousPage=page;int previousLine=index;bool wasTiming=sectionWatch.IsRunning;
  bool resumeVoice=!pausedAudio&&(originalPlaying||speech!=null&&speech.State==System.Speech.Synthesis.SynthesizerState.Speaking);
  if(resumeVoice)TogglePlay();PauseSectionTiming();
  var movies=Descendants(this).OfType<OpeningCgCanvas>().Where(c=>!c.IsDisposed).ToList();foreach(var movie in movies)movie.SetPaused(true);
  bool exit=false,returnHome=false;Form dialog=null;EventHandler reposition=null;
  try{
   using(dialog=new GuildWordDialog{QuestStyle=true,BackColor=Color.FromArgb(6,29,29),Text="游戏设置",Width=720,Height=700,KeyPreview=true}){
    inGameSettings=dialog;
    var area=Screen.FromControl(this).WorkingArea;dialog.Size=new Size(Math.Min(720,area.Width-32),Math.Min(700,area.Height-32));
    Action center=()=>dialog.Location=new Point(Left+(Width-dialog.Width)/2,Top+(Height-dialog.Height)/2);center();reposition=(s,e)=>center();Resize+=reposition;
    dialog.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Escape){dialog.Close();e.Handled=true;e.SuppressKeyPress=true;}};
    var footer=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=66,Padding=new Padding(12,6,12,6),WrapContents=false,BackColor=dialog.BackColor};dialog.Controls.Add(footer);
    footer.Controls.Add(Btn("继续游戏",()=>dialog.Close(),true));
    footer.Controls.Add(Btn("返回主界面",()=>{returnHome=true;dialog.Close();}));
    footer.Controls.Add(Btn("保存并退出游戏",()=>{exit=true;dialog.Close();}));
    var body=new Panel{Dock=DockStyle.Fill,Padding=new Padding(0,58,0,0),BackColor=dialog.BackColor};dialog.Controls.Add(body);body.BringToFront();
    var navigation=new FlowLayoutPanel{Location=Point.Empty,Width=650,Height=58,WrapContents=false,BackColor=dialog.BackColor};body.Controls.Add(navigation);
    var soundPage=new Panel{Dock=DockStyle.Fill,BackColor=dialog.BackColor};var picturePage=new Panel{Dock=DockStyle.Fill,BackColor=dialog.BackColor,Visible=false};body.Controls.Add(soundPage);body.Controls.Add(picturePage);navigation.BringToFront();
    GameButton soundTab=null,pictureTab=null;Action<bool> selectTab=isSound=>{picturePage.Visible=!isSound;soundPage.Visible=isSound;soundTab.Primary=isSound;pictureTab.Primary=!isSound;soundTab.Invalidate();pictureTab.Invalidate();};soundTab=(GameButton)Btn("声音",()=>selectTab(true),true);pictureTab=(GameButton)Btn("画面与字幕",()=>selectTab(false));soundTab.SelectionOnly=pictureTab.SelectionOnly=true;navigation.Controls.Add(soundTab);navigation.Controls.Add(pictureTab);
    var sound=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=false,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(12,8,12,8),BackColor=dialog.BackColor};soundPage.Controls.Add(sound);AddCompactAudioSettings(sound,dialog.Width-80);
    var picture=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=false,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(18,8,18,8),BackColor=dialog.BackColor};picturePage.Controls.Add(picture);
    picture.Controls.Add(Lab("画面设置",20,Gold));
    var fullscreen=new GoldCheckBox{Text="全屏显示",Checked=save.fullscreen,FlatStyle=FlatStyle.Flat,AutoSize=true,ForeColor=TextColor,Margin=new Padding(8,18,8,18)};
    picture.Controls.Add(fullscreen);picture.Controls.Add(Lab("窗口尺寸",13));var sizes=new FlowLayoutPanel{Width=580,Height=58,WrapContents=false,BackColor=dialog.BackColor,Enabled=!save.fullscreen};picture.Controls.Add(sizes);
    foreach(var option in new[]{new Size(1280,780),new Size(1600,940),new Size(1920,1080)}){var chosen=option;var button=Btn(chosen.Width+" × "+chosen.Height,()=>{if(save.fullscreen)return;WindowState=FormWindowState.Normal;Size=new Size(Math.Min(chosen.Width,area.Width),Math.Min(chosen.Height,area.Height));Location=new Point(area.Left+(area.Width-Width)/2,area.Top+(area.Height-Height)/2);center();});button.Width=170;sizes.Controls.Add(button);}
    fullscreen.CheckedChanged+=(s,e)=>{save.fullscreen=fullscreen.Checked;ApplyFullscreen();sizes.Enabled=!save.fullscreen;Persist();};
    picture.Controls.Add(Lab("字幕",18,Accent));
    var english=new GoldCheckBox{Text="显示英文字幕",Checked=save.english,FlatStyle=FlatStyle.Flat,AutoSize=true,ForeColor=TextColor,Margin=new Padding(8,12,8,8)};
    var chinese=new GoldCheckBox{Text="显示中文字幕",Checked=save.chinese,FlatStyle=FlatStyle.Flat,AutoSize=true,ForeColor=TextColor,Margin=new Padding(8)};picture.Controls.Add(english);picture.Controls.Add(chinese);
    bool changingSubtitle=false;english.CheckedChanged+=(s,e)=>{if(changingSubtitle)return;if(!SetStorySubtitle(false,english.Checked)){changingSubtitle=true;english.Checked=save.english;changingSubtitle=false;}};
    chinese.CheckedChanged+=(s,e)=>{if(changingSubtitle)return;if(!SetStorySubtitle(true,chinese.Checked)){changingSubtitle=true;chinese.Checked=save.chinese;changingSubtitle=false;}};
    var hint=Lab("设置自动保存。ESC 打开或关闭设置；F11 可切换全屏。\n本节开启字幕会计入星级评价。",11,Muted);hint.MaximumSize=new Size(550,0);picture.Controls.Add(hint);foreach(var label in picture.Controls.OfType<Label>()){label.AutoSize=false;label.Size=new Size(dialog.Width-90,label.Text.Contains("\n")?54:34);}
    dialog.ShowDialog(owner!=null&&!owner.IsDisposed?owner:this);
   }
  }finally{
   if(reposition!=null)Resize-=reposition;inGameSettings=null;
   if(!IsDisposed&&!Disposing){Persist();if(!exit&&!returnHome){foreach(var movie in movies)if(!movie.IsDisposed)movie.SetPaused(false);if(page==previousPage&&index==previousLine){if(page=="story")UpdateLine();if(wasTiming)ResumeSectionTiming();if(resumeVoice&&pausedAudio)TogglePlay();}}}
  }
  if(returnHome&&!IsDisposed){StopAudio();PauseSectionTiming();ShowMain();}
  if(exit&&!IsDisposed)Close();
 }
 void AddCompactAudioSettings(FlowLayoutPanel panel,int width){
  panel.Controls.Add(Lab("声音设置",18,Gold));
  Action<string,int,Action<int>> add=(title,value,set)=>{var row=new Panel{Width=Math.Max(260,width),Height=65,Margin=new Padding(4,3,4,3),BackColor=panel.BackColor};var label=Lab(title+"："+value+"%",12);label.AutoSize=false;label.Size=new Size(row.Width,28);label.Location=Point.Empty;row.Controls.Add(label);var slider=new SettingsVolumeSlider{Width=row.Width,Height=26,Location=new Point(0,30),BackColor=panel.BackColor,AccessibleName=title,Value=ClampVolume(value)};slider.ValueChanged+=(s,e)=>{set(slider.Value);label.Text=title+"："+slider.Value+"%";ApplySoundSettings();Persist();};row.Controls.Add(slider);panel.Controls.Add(row);};
  add("总音量",MasterSoundVolume(),v=>save.masterVolume=v);add("背景音乐（BGM）",save.rogueMusicVolume,v=>save.rogueMusicVolume=v);add("游戏音效",save.rogueEffectsVolume,v=>save.rogueEffectsVolume=v);add("剧情配音 / 听力",save.volume,v=>save.volume=v);add("单词发音",save.wordVolume??save.volume,v=>save.wordVolume=v);
  panel.Controls.Add(Btn("试听游戏音效",PlayMenuClick));panel.Controls.Add(Lab("调至 0 可静音 · 设置自动保存",10,Muted));foreach(var label in panel.Controls.OfType<Label>()){label.AutoSize=false;label.Size=new Size(width,label.Font.Size>15?32:26);}
 }
}

public sealed class SettingsVolumeSlider:Control {
 int value;public event EventHandler ValueChanged;
 public int Value{get{return value;}set{int next=Math.Max(0,Math.Min(100,value));if(this.value==next)return;this.value=next;AccessibleDescription=next+"%";Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
 public SettingsVolumeSlider(){DoubleBuffered=true;TabStop=true;Cursor=Cursors.Hand;AccessibleRole=AccessibleRole.Slider;SetStyle(ControlStyles.Selectable,true);}
 protected override bool IsInputKey(Keys keyData){return keyData==Keys.Left||keyData==Keys.Right||keyData==Keys.Home||keyData==Keys.End||base.IsInputKey(keyData);}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(e.KeyCode==Keys.Left)Value--;else if(e.KeyCode==Keys.Right)Value++;else if(e.KeyCode==Keys.Home)Value=0;else if(e.KeyCode==Keys.End)Value=100;else return;e.Handled=true;}
 void Choose(int x){Value=(int)Math.Round(100.0*(x-8)/Math.Max(1,Width-16));}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left)return;Focus();Capture=true;Choose(e.X);}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(Capture&&(e.Button&MouseButtons.Left)!=0)Choose(e.X);}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);Capture=false;}
 protected override void OnGotFocus(EventArgs e){base.OnGotFocus(e);Invalidate();}protected override void OnLostFocus(EventArgs e){base.OnLostFocus(e);Invalidate();}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);int y=Height/2,x=8+(Width-16)*Value/100;using(var track=new SolidBrush(Color.FromArgb(12,32,35)))e.Graphics.FillRectangle(track,8,y-3,Math.Max(1,Width-16),6);using(var fill=new SolidBrush(GuildChrome.Gold))e.Graphics.FillRectangle(fill,8,y-3,Math.Max(1,x-8),6);using(var thumb=new SolidBrush(Focused?Color.FromArgb(239,205,133):GuildChrome.Gold))e.Graphics.FillRectangle(thumb,x-5,y-8,10,16);}
}
