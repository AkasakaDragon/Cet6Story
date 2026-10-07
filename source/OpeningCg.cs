using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public partial class Game {
 void ShowOpeningCg(bool introduction){
  ClearPage();page="opening-cg";
  string movie=Path.Combine(root,"assets","opening","opening.mp4"),audio=Path.Combine(root,"assets","opening","opening.wav");
  try{
   var canvas=new OpeningCgCanvas(Path.Combine(root,"tools","ffmpeg","ffmpeg.exe"),movie,AudioDevicePath.Relative(AudioVolume.Prepare(audio,EffectSoundVolume(),root))){Dock=DockStyle.Fill};content.Controls.Add(canvas);
   bool finished=false;Action finish=()=>{if(finished||page!="opening-cg")return;finished=true;if(introduction){save.openingCgPending=false;Persist();ShowStoryMap();}else ShowMain();};canvas.Completed=()=>TransitionCgPage(finish);
   canvas.Failed=message=>{if(page!="opening-cg")return;GameMessage.Show(this,"开场 CG 未能播放："+message,"播放提示");finish();};
   var skip=new VNButton{Text="跳过 CG",PixelStyle=true,Size=new Size(125,46),Font=GameTheme.Body(12),AccessibleName="跳过开场 CG"};canvas.Controls.Add(skip);Action place=()=>skip.Location=new Point(Math.Max(8,canvas.ClientSize.Width-skip.Width-20),20);canvas.Resize+=(s,e)=>place();place();skip.Click+=(s,e)=>TransitionCgPage(finish);tips.SetToolTip(skip,"跳过后进入主线地图；回看时返回主菜单");canvas.Start();
  }catch(Exception ex){GameMessage.Show(this,"开场 CG 未能播放："+ex.Message,"播放提示");if(introduction){save.openingCgPending=false;Persist();ShowStoryMap();}else ShowMain();}
 }
}

// Stream a bounded pair of raw frame buffers. No external player window or codec install.
public class OpeningCgCanvas:Control {
 const int FrameWidth=960,FrameHeight=540,FrameBytes=FrameWidth*FrameHeight*3;
 readonly string ffmpeg,movie,audio;readonly double duration;readonly byte[] pending=new byte[FrameBytes];readonly object gate=new object();readonly ManualResetEvent started=new ManualResetEvent(false);readonly Stopwatch clock=new Stopwatch();
 Process decoder;Thread worker;System.Windows.Forms.Timer timer;Bitmap picture;bool fresh,audioStarted,notified;volatile bool closed;volatile string failure;string decoderError="";
 public Action Completed;public Action<string> Failed;public int FramesShown{get;private set;}public bool AudioStarted{get{return audioStarted;}}
 public bool FillFrame{get;set;}
 public string SceneCaption{get;set;}public double SceneCaptionFromSeconds{get;set;}
 public bool WaitForContinue{get;set;}public bool AwaitingContinue{get;private set;}bool continued,transitioning;public double EndFadeHoldSeconds{get;set;}
 void PlaybackEnded(){notified=true;if(timer!=null)timer.Stop();clock.Stop();if(audioStarted)mciSendString("stop openingaudio",null,0,IntPtr.Zero);if(WaitForContinue){AwaitingContinue=true;Cursor=Cursors.Hand;Focus();Invalidate();}else ContinueAfterPlayback();}
 void ContinueAfterPlayback(){if(continued||closed)return;continued=true;AwaitingContinue=false;Cursor=Cursors.Default;if(picture!=null&&FramesShown>0&&timer!=null){transitioning=true;clock.Restart();timer.Start();Invalidate();}else if(Completed!=null)Completed();}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(AwaitingContinue&&e.Button==MouseButtons.Left)ContinueAfterPlayback();}
 protected override void OnKeyDown(KeyEventArgs e){base.OnKeyDown(e);if(AwaitingContinue&&(e.KeyCode==Keys.Enter||e.KeyCode==Keys.Space)){e.Handled=e.SuppressKeyPress=true;ContinueAfterPlayback();}}
 bool paused;
 public void SetPaused(bool value){if(closed||notified||paused==value)return;paused=value;if(value){if(timer!=null)timer.Stop();clock.Stop();if(audioStarted)mciSendString("pause openingaudio",null,0,IntPtr.Zero);}else{if(audioStarted){mciSendString("resume openingaudio",null,0,IntPtr.Zero);clock.Start();}if(timer!=null)timer.Start();}}
 [DllImport("winmm.dll",CharSet=CharSet.Auto)] static extern int mciSendString(string command,StringBuilder result,int length,IntPtr callback);
 public OpeningCgCanvas(string decoderPath,string moviePath,string audioPath,double seconds=67){ffmpeg=decoderPath;movie=moviePath;audio=audioPath;duration=seconds;WaitForContinue=true;EndFadeHoldSeconds=.65;BackColor=Color.Black;DoubleBuffered=true;ResizeRedraw=true;AccessibleName="中英双语开场 CG";TabStop=true;}
 public void Start(){
  if(worker!=null)return;if(!File.Exists(ffmpeg)||!File.Exists(movie)||!File.Exists(audio))throw new FileNotFoundException("缺少开场 CG 或播放资源。");
  decoder=new Process{StartInfo=new ProcessStartInfo{FileName=ffmpeg,Arguments="-nostdin -loglevel error -i \""+movie+"\" -an -vf scale=960:540:flags=neighbor,fps=24 -f rawvideo -pix_fmt bgr24 pipe:1",UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true}};
  decoder.ErrorDataReceived+=(s,e)=>{if(!String.IsNullOrEmpty(e.Data))decoderError=e.Data;};decoder.Start();decoder.BeginErrorReadLine();
  picture=new Bitmap(FrameWidth,FrameHeight,PixelFormat.Format24bppRgb);timer=new System.Windows.Forms.Timer{Interval=20};timer.Tick+=(s,e)=>TickPlayback();timer.Start();worker=new Thread(Decode){IsBackground=true,Name="Opening CG decoder"};worker.Start();
 }
 void Decode(){try{var input=decoder.StandardOutput.BaseStream;var frame=new byte[FrameBytes];int index=0;while(!closed){int filled=0;while(filled<frame.Length&&!closed){int count=input.Read(frame,filled,frame.Length-filled);if(count==0)break;filled+=count;}if(closed)break;if(filled!=frame.Length){if(index==0)throw new Exception(String.IsNullOrEmpty(decoderError)?"无法解码视频。":decoderError);if(index<duration*24-3)throw new Exception("视频解码提前结束。"+decoderError);break;}
    if(index>0){while(!closed&&clock.Elapsed.TotalSeconds<index/24.0)Thread.Sleep(5);}
    if(closed)break;lock(gate){Buffer.BlockCopy(frame,0,pending,0,FrameBytes);fresh=true;}if(index==0)started.WaitOne();index++;
   }}catch(Exception ex){if(!closed)failure=ex.Message;}}
 void TickPlayback(){
  if(closed)return;if(transitioning){Invalidate();if(clock.ElapsedMilliseconds>=260){clock.Stop();Invalidate();Update();transitioning=false;timer.Stop();if(Completed!=null)Completed();}return;}if(notified||paused)return;
  if(failure!=null){notified=true;if(Failed!=null)Failed(failure);return;}
  lock(gate){if(fresh&&(!audioStarted||!WaitForContinue||clock.Elapsed.TotalSeconds<Math.Max(0,duration-EndFadeHoldSeconds))){var data=picture.LockBits(new Rectangle(0,0,FrameWidth,FrameHeight),ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);try{Marshal.Copy(pending,0,data.Scan0,FrameBytes);}finally{picture.UnlockBits(data);}fresh=false;FramesShown++;Invalidate();}}
  if(FramesShown>0&&!audioStarted){mciSendString("close openingaudio",null,0,IntPtr.Zero);int error=mciSendString("open \""+audio+"\" type waveaudio alias openingaudio",null,0,IntPtr.Zero);if(error==0)error=mciSendString("play openingaudio",null,0,IntPtr.Zero);if(error!=0){failure="无法播放 CG 音效（错误 "+error+"）。";return;}audioStarted=true;clock.Start();started.Set();}
  if(audioStarted&&clock.Elapsed.TotalSeconds>=duration)PlaybackEnded();
 }
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(picture==null||FramesShown==0){GameTheme.DrawText(e.Graphics,"正在播放开场 CG…",Font,ClientRectangle,Color.White,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);return;}float scale=FillFrame?Math.Max(Width/(float)FrameWidth,Height/(float)FrameHeight):Math.Min(Width/(float)FrameWidth,Height/(float)FrameHeight);var target=new RectangleF((Width-FrameWidth*scale)/2,(Height-FrameHeight*scale)/2,FrameWidth*scale,FrameHeight*scale);e.Graphics.InterpolationMode=InterpolationMode.NearestNeighbor;e.Graphics.PixelOffsetMode=PixelOffsetMode.Half;e.Graphics.DrawImage(picture,target);if(!String.IsNullOrEmpty(SceneCaption)&&audioStarted&&(AwaitingContinue||transitioning||clock.Elapsed.TotalSeconds>=SceneCaptionFromSeconds)){var caption=new Rectangle(32,Math.Max(0,Height-190),Math.Max(1,Width-64),110);using(var brush=new SolidBrush(Color.FromArgb(175,5,18,27)))e.Graphics.FillRectangle(brush,caption);using(var font=GameTheme.Body(16))GameTheme.DrawText(e.Graphics,SceneCaption,font,caption,Color.White,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.WordBreak);}if(AwaitingContinue){int h=52;var hint=new Rectangle(0,Math.Max(0,Height-h-20),Width,h);using(var font=GameTheme.Body(13))GameTheme.DrawText(e.Graphics,"点击继续",font,hint,GuildChrome.Ivory,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}if(transitioning)using(var dim=new SolidBrush(Color.FromArgb(Math.Min(255,(int)(255*clock.ElapsedMilliseconds/260.0)),0,0,0)))e.Graphics.FillRectangle(dim,ClientRectangle);}
 protected override void Dispose(bool disposing){if(disposing&&!closed){closed=true;started.Set();if(timer!=null)timer.Dispose();mciSendString("close openingaudio",null,0,IntPtr.Zero);if(decoder!=null){ThreadPool.QueueUserWorkItem(_=>{try{if(!decoder.HasExited)decoder.Kill();}catch{}if(worker!=null)worker.Join(5000);if(worker==null||!worker.IsAlive){decoder.Dispose();started.Dispose();}});}else if(worker==null||!worker.IsAlive)started.Dispose();if(picture!=null)picture.Dispose();}base.Dispose(disposing);}
}
