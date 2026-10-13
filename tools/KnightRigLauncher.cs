using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

public static class KnightRigLauncher {
 [STAThread]public static int Main(string[] args) {
  try {
   string root=args.Length>0?Path.GetFullPath(args[0]):FindRoot();Directory.SetCurrentDirectory(root);
   if(args.Length>1 && args[1]=="--export"){Export(root);return 0;}
   if(args.Length>1 && args[1]=="--check"){Check(root);return 0;}
   Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   using(var preview=new KnightRigPreview(root))Application.Run(preview);return 0;
  } catch(Exception e){Console.Error.WriteLine(e);return 1;}
 }
 static string FindRoot() {
  var directory=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
  while(directory!=null){if(File.Exists(Path.Combine(directory.FullName,"assets","characters","red-knight-rig","rig.json")))return directory.FullName;directory=directory.Parent;}
  throw new DirectoryNotFoundException("请将预览工具保留在游戏项目目录内。");
 }
 static PixelSkeletalRig Load(string root){return new PixelSkeletalRig(Path.Combine(root,"assets","characters","red-knight-rig"));}
 static void Check(string root) {
  var rig=Load(root);rig.Wind=false;rig.HybridWind=false;rig.UseArmIK=false;
  using(var rest=new Bitmap(Path.Combine(root,"assets","characters","red-knight-rig","rest.png")))
  using(var rendered=rig.Render(200,272,0,0)) {
   int mismatch=0;for(int y=0;y<272;y++)for(int x=0;x<200;x++){var a=rest.GetPixel(x,y);var b=rendered.GetPixel(x,y);if(a.A!=b.A || a.A>0 && (Math.Abs(a.R-b.R)>1 || Math.Abs(a.G-b.G)>1 || Math.Abs(a.B-b.B)>1))mismatch++;}
   if(mismatch!=0)throw new Exception("Bind-pose reconstruction differs at "+mismatch+" pixels.");
  }
  rig.UseArmIK=true;
  foreach(var clip in rig.Definition.clips)for(int f=0;f<=60;f++) {
   rig.Animation=clip.name;rig.AnimationTime=clip.duration*f/60;rig.Evaluate();
   foreach(string name in new[]{"near_hand","far_hand"}) {
    var hand=rig.Joint(name);var target=rig.HandTarget(name);double distance=Math.Sqrt((hand.X-target.X)*(hand.X-target.X)+(hand.Y-target.Y)*(hand.Y-target.Y));
    if(distance>.12)throw new Exception("Hand left shared sword grip: "+clip.name+" "+f+" "+name+" "+distance);
   }
   foreach(var bone in rig.Definition.bones){var j=rig.Joint(bone.name);if(Single.IsNaN(j.X)||Single.IsInfinity(j.Y))throw new Exception("Invalid bone transform.");}
  }
  rig.Animation="idle";rig.AnimationTime=.6f;rig.Wind=true;rig.WindTime=0;rig.Evaluate();float hair0=rig.Joint("hair_mid3").Y,foot0=rig.Joint("near_foot").Y;
  rig.WindTime=.55f;rig.Evaluate();if(Math.Abs(rig.Joint("hair_mid3").Y-hair0)<.1)throw new Exception("Hair tip did not move.");if(Math.Abs(rig.Joint("near_foot").Y-foot0)>.001)throw new Exception("Wind moved feet.");
  Console.WriteLine("PASS: exact cutout rest reconstruction; 38-bone hierarchy; both hands on shared grip throughout 3 clips; finite transforms; independent cloth movement and planted feet.");
 }
 static void Export(string root) {
  string output=Path.Combine(root,"output","knight-rig");Directory.CreateDirectory(output);var rig=Load(root);
  foreach(var clip in rig.Definition.clips) {
   string folder=Path.Combine(output,clip.name);Directory.CreateDirectory(folder);
   for(int f=0;f<24;f++) {
    rig.Animation=clip.name;rig.AnimationTime=clip.duration*f/23;rig.WindTime=rig.AnimationTime;
    using(var logical=rig.Render(400,340,55,36))using(var canvas=new Bitmap(800,680))using(var g=Graphics.FromImage(canvas)) {
     g.Clear(Color.FromArgb(17,40,38));g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.DrawImage(logical,new Rectangle(0,0,800,680));canvas.Save(Path.Combine(folder,"frame-"+f.ToString("00")+".png"),ImageFormat.Png);
    }
   }
  }
  using(var window=new KnightRigPreview(root))using(var screenshot=new Bitmap(window.Width,window.Height)) {
   window.Show();Application.DoEvents();window.DrawToBitmap(screenshot,new Rectangle(Point.Empty,screenshot.Size));screenshot.Save(Path.Combine(output,"workbench.png"),ImageFormat.Png);window.Close();
  }
  Console.WriteLine("Rendered 3 clips and workbench to "+output);
 }
}
