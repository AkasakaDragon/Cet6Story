using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public partial class Game {
 void BeginStoryBackgroundBlackout(){
  if(buildingCgDestination)return;
  foreach(var old in content.Controls.OfType<WhiteSceneReveal>().ToArray())old.Dispose();
  var reveal=new WhiteSceneReveal{Dock=DockStyle.Fill,FadeColor=Color.Black,HoldMs=100,DurationMs=180};
  string destination=page;
  content.Controls.Add(reveal);reveal.BringToFront();reveal.Update();
  // Wait for this line's text, actor and layout to be updated before taking its snapshot.
  BeginInvoke((Action)(()=>{
   if(reveal.IsDisposed)return;
   if(IsDisposed||page!=destination){reveal.Dispose();return;}
   OpeningRedrawMessage(Handle,0x000B,IntPtr.Zero,IntPtr.Zero);
   try{
    reveal.Visible=false;content.PerformLayout();
    reveal.CaptureScene(content);reveal.Visible=true;reveal.BringToFront();reveal.Start();
   }finally{
    OpeningRedrawMessage(Handle,0x000B,new IntPtr(1),IntPtr.Zero);Invalidate(true);Update();
   }
  }));
 }
}
