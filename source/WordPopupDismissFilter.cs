using System;
using System.Windows.Forms;

// Keep the owner enabled: native modal-owner blocking produces a Windows beep.
// Consume the dismissal click so it cannot also advance the story underneath.
public sealed class WordPopupDismissFilter:IMessageFilter {
 sealed class DismissalReleaseFilter:IMessageFilter {
  public int Pending;
  public bool PreFilterMessage(ref Message message){if(Pending==0||message.Msg!=Pending)return false;Pending=0;Application.RemoveMessageFilter(this);return true;}
 }
 static readonly DismissalReleaseFilter release=new DismissalReleaseFilter();
 readonly Form popup;
 public WordPopupDismissFilter(Form form){popup=form;}
 public bool PreFilterMessage(ref Message message){
  if(popup.IsDisposed||!popup.Visible)return false;
  bool click=message.Msg==0x201||message.Msg==0x204||message.Msg==0x207||message.Msg==0xA1||message.Msg==0xA4;
  if(click&&!popup.Bounds.Contains(Cursor.Position)){
   // The popup closes on mouse-down; keep consuming its mouse-up after disposal.
   if(release.Pending==0)Application.AddMessageFilter(release);
   release.Pending=message.Msg==0x201?0x202:message.Msg==0x204?0x205:message.Msg==0x207?0x208:message.Msg==0xA1?0xA2:0xA5;
   popup.Close();return true;
  }
  if(message.Msg==0x100&&!popup.ContainsFocus)return true;
  return false;
 }
}
