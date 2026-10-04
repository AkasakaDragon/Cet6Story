using System;using System.Runtime.InteropServices;using System.Windows.Forms;
// Keep the current frame visible while a batch of child controls is replaced.
public sealed class BattleRedrawScope:IDisposable {
 readonly Control surface;readonly IntPtr window;bool disposed;
 [DllImport("user32.dll")]static extern IntPtr SendMessage(IntPtr h,int message,IntPtr w,IntPtr l);
 [DllImport("user32.dll")]static extern bool RedrawWindow(IntPtr h,IntPtr rect,IntPtr region,uint flags);
 public BattleRedrawScope(Control control){surface=control;surface.SuspendLayout();if(control.IsHandleCreated&&control.Visible){window=control.Handle;SendMessage(window,0x000B,IntPtr.Zero,IntPtr.Zero);}}
 public void Dispose(){if(disposed)return;disposed=true;if(surface.IsDisposed)return;try{surface.ResumeLayout(true);}finally{if(window!=IntPtr.Zero){SendMessage(window,0x000B,new IntPtr(1),IntPtr.Zero);RedrawWindow(window,IntPtr.Zero,IntPtr.Zero,0x0185);}else surface.Invalidate(true);}}
}
