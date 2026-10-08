using System;using System.Collections.Concurrent;using System.Threading;
public partial class Game {
 int mciSendString(string command,System.Text.StringBuilder answer,int length,IntPtr window){
  if(command.IndexOf("storyaudio",StringComparison.OrdinalIgnoreCase)<0||Thread.CurrentThread==storyAudioWorker)return NativeMciSendString(command,answer,length,window);
  if(storyAudioCommands.IsAddingCompleted)return 263;
  int result=0;using(var done=new ManualResetEvent(false)){QueueStoryAudio(()=>{try{result=NativeMciSendString(command,answer,length,window);}finally{done.Set();}});done.WaitOne();}return result;
 }
 readonly BlockingCollection<Action> storyAudioCommands=new BlockingCollection<Action>();Thread storyAudioWorker;int storyAudioGeneration;
 void QueueStoryAudio(Action command){if(storyAudioWorker==null){storyAudioWorker=new Thread(()=>{foreach(var action in storyAudioCommands.GetConsumingEnumerable())action();}){IsBackground=true,Name="Story audio commands"};storyAudioWorker.Start();}if(!storyAudioCommands.IsAddingCompleted)storyAudioCommands.Add(command);}
 void FlushStoryAudio(){if(storyAudioWorker==null||storyAudioCommands.IsAddingCompleted)return;using(var done=new ManualResetEvent(false)){QueueStoryAudio(()=>done.Set());done.WaitOne();}}
 void CloseStoryAudioDispatch(){if(storyAudioWorker==null||storyAudioCommands.IsAddingCompleted)return;QueueStoryAudio(()=>NativeMciSendString("close storyaudio",null,0,IntPtr.Zero));storyAudioCommands.CompleteAdding();storyAudioWorker.Join();}
 void PlayStoryAudioCommand(string command){int generation=storyAudioGeneration;originalPlaying=true;QueueStoryAudio(()=>{
  if(generation!=VolatileStoryAudioGeneration())return;int result=mciSendString(command,null,0,IntPtr.Zero);if(result==0&&pausedAudio&&generation==VolatileStoryAudioGeneration())mciSendString("pause storyaudio",null,0,IntPtr.Zero);
  if(IsDisposed||!IsHandleCreated)return;try{BeginInvoke((Action)(()=>{if(IsDisposed||generation!=storyAudioGeneration||!originalPlaying)return;if(result!=0){StopAudio();SetStatus("播放失败：请确认音频格式和分句时间点。");return;}if(!pausedAudio)audioTimer.Start();}));}catch(InvalidOperationException){}
 });}
 int VolatileStoryAudioGeneration(){return Interlocked.CompareExchange(ref storyAudioGeneration,0,0);}
}
