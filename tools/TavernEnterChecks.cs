using System;using System.Reflection;using System.Windows.Forms;
class TavernEnterChecks {
 static void Check(bool ok,string why){if(!ok)throw new Exception(why);}
 static void Key(VNButton b,Keys k){typeof(VNButton).GetMethod("OnKeyDown",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(b,new object[]{new KeyEventArgs(k)});}
 [STAThread]static void Main(){try{
  int count=0;var button=new VNButton();button.Click+=(s,e)=>count++;button.KeyboardActivation=false;Key(button,Keys.Enter);Key(button,Keys.Space);Check(count==0,"header keys trigger management");button.PerformClick();Check(count==1,"mouse/action disabled");
  button.KeyboardActivation=true;Key(button,Keys.Enter);Check(count==2,"normal Enter lost");button.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){e.Handled=e.SuppressKeyPress=true;button.PerformClick();}};Key(button,Keys.Enter);Check(count==3,"Enter clicks twice");Console.WriteLine("PASS: management keys suppressed, direct clicks retained, Enter handled exactly once.");
 }catch(Exception e){Console.WriteLine(e);Environment.Exit(1);}}
}
