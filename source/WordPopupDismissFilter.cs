using System;
using System.Windows.Forms;

// Keep the owner enabled: native modal-owner blocking produces a Windows beep.
// Consume the dismissal click so it cannot also advance the story underneath.
public sealed class WordPopupDismissFilter:IMessageFilter {
 readonly Form popup;
 public WordPopupDismissFilter(Form form){popup=form;}
 public bool PreFilterMessage(ref Message message){
  if(popup.IsDisposed||!popup.Visible)return false;
  bool click=message.Msg==0x201||message.Msg==0x204||message.Msg==0x207||message.Msg==0xA1||message.Msg==0xA4;
  if(click&&!popup.Bounds.Contains(Cursor.Position)){popup.Close();return true;}
  if(message.Msg==0x100&&(Keys)message.WParam.ToInt32()==Keys.Escape){popup.Close();return true;}
  if(message.Msg==0x100&&!popup.ContainsFocus)return true;
  return false;
 }
}
