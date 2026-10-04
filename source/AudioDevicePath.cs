using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

public static class AudioDevicePath {
 // waveaudio internally expands relative paths and still rejects long filenames.
 // A short private temporary cache works regardless of where the ZIP is extracted.
 public static string Relative(string file){
  string full=Path.GetFullPath(file);if(full.Length<=110)return full;
  var info=new FileInfo(full);string key;using(var hash=SHA256.Create())key=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(full+"|"+info.Length+"|"+info.LastWriteTimeUtc.Ticks))).Replace("-","").Substring(0,16).ToLowerInvariant();
  string folder=Path.Combine(Path.GetTempPath(),"Cet6Story-audio");Directory.CreateDirectory(folder);
  string target=Path.Combine(folder,key+Path.GetExtension(full));
  if(!File.Exists(target)||new FileInfo(target).Length!=info.Length){string temporary=target+"."+Guid.NewGuid().ToString("N")+".tmp";File.Copy(full,temporary);try{if(File.Exists(target))File.Delete(target);File.Move(temporary,target);}finally{if(File.Exists(temporary))File.Delete(temporary);}}
  return target;
 }
}
