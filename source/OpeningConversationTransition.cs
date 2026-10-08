using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public partial class Game {
 bool buildingCgDestination;
 [DllImport("user32.dll",EntryPoint="SendMessage")]
 static extern IntPtr OpeningRedrawMessage(IntPtr window,int message,IntPtr value,IntPtr unused);
 void RevealOpeningConversation(){TransitionCgPage(ShowStory);}
 void TransitionCgPage(Action build){
  // Build and capture the entire dialogue page before permitting a screen repaint.
  // The outgoing CG ends at opaque black; the incoming page starts at the same black.
  OpeningRedrawMessage(content.Handle,0x000B,IntPtr.Zero,IntPtr.Zero);
  SuspendLayout();content.SuspendLayout();
  try{
   buildingCgDestination=true;
   try{build();}finally{buildingCgDestination=false;}
   content.ResumeLayout(true);ResumeLayout(true);content.PerformLayout();
   if(content.Controls.Count==0)return;
   if(page=="story"&&stage!=null){stage.PerformLayout();stage.Snap();StopAudio();}
   string destination=page;Chapter chapter=current;int line=index;
   var reveal=new WhiteSceneReveal{Dock=DockStyle.Fill,FadeColor=Color.Black,DurationMs=320};
   reveal.CaptureScene(content);content.Controls.Add(reveal);reveal.BringToFront();
   reveal.Completed=()=>{if(!IsDisposed&&page==destination&&page=="story"&&current==chapter&&index==line)PlayCurrent();};
   reveal.Start();
  }finally{
   content.ResumeLayout(true);ResumeLayout(true);
   OpeningRedrawMessage(content.Handle,0x000B,new IntPtr(1),IntPtr.Zero);
   Invalidate(true);Update();
  }
 }
}
