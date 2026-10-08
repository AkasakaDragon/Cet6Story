using System;using System.IO;using System.Reflection;using System.Threading;
class AudioContinuityChecks {
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 static FieldInfo Field(string name){return typeof(RogueAudioMixer).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic);}
 static void Main(){using(var mixer=new RogueAudioMixer(Path.Combine(Directory.GetCurrentDirectory(),"assets","rogue","audio"))){mixer.SetMusicTrack(2);mixer.SetActive(true);Thread.Sleep(250);long before=mixer.SubmittedBuffers,underruns=mixer.BufferUnderruns;
 // UI thread stalls must not stop the dedicated audio worker.
 Thread.Sleep(700);Check(mixer.SubmittedBuffers>before+20,"worker stalled with caller thread");
 var queue=Field("buffers").GetValue(mixer);for(int i=0;i<50;i++){mixer.SetMusicTrack(2);mixer.Ducked=i%2==0;var garbage=new byte[600000];garbage[0]=1;if(i%5==0)GC.Collect();Thread.Sleep(20);}
 Check(Object.ReferenceEquals(queue,Field("buffers").GetValue(mixer)),"ducking replaced buffers");Check(mixer.Error==null,"audio device error");Check(mixer.BufferUnderruns==underruns,"buffer starvation under GC and rapid voice changes");
 mixer.SetActive(false);long stopped=mixer.SubmittedBuffers;Thread.Sleep(60);Check(mixer.SubmittedBuffers==stopped,"inactive mixer keeps submitting");mixer.SetActive(true);Thread.Sleep(80);Check(mixer.SubmittedBuffers>stopped,"mixer cannot resume");Console.WriteLine("PASS: UI stall, GC pressure, 50 duck transitions, no buffer starvation, deactivate/resume.");}}
}
