$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;using System.Drawing;using System.Drawing.Drawing2D;
public static class Expressions {
public static int Export(string source,string atlas,string output,int x,int y,int w,int h,int index,int[] eyes,int[] mouth){
using(var original=new Bitmap(source))using(var sheet=new Bitmap(atlas))using(var patch=new Bitmap(w,h))using(var result=(Bitmap)original.Clone()){
using(var g=Graphics.FromImage(patch)){g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.DrawImage(sheet,new Rectangle(0,0,w,h),new Rectangle(index%3*sheet.Width/3,index/3*sheet.Height/2,sheet.Width/3,sheet.Height/2),GraphicsUnit.Pixel);}
int changed=0;for(int py=0;py<h;py++)for(int px=0;px<w;px++){bool allowed=false;foreach(var r in new[]{eyes,mouth}){double dx=(px-(r[0]+r[2]/2.0))/(r[2]/2.0),dy=(py-(r[1]+r[3]/2.0))/(r[3]/2.0);if((Object.ReferenceEquals(r,mouth)&&px>=r[0]&&px<r[0]+r[2]&&py>=r[1]&&py<r[1]+r[3])||dx*dx+dy*dy<=1)allowed=true;}if(!allowed)continue;var a=original.GetPixel(x+px,y+py);var b=patch.GetPixel(px,py);if(a.A==0||b.A==0)continue;result.SetPixel(x+px,y+py,Color.FromArgb(a.A,b.R,b.G,b.B));if(a.ToArgb()!=result.GetPixel(x+px,y+py).ToArgb())changed++;}
result.Save(output,System.Drawing.Imaging.ImageFormat.Png);return changed;}}
}
'@ -ReferencedAssemblies @('System.Drawing.Common','System.Drawing.Primitives','System.Private.Windows.GdiPlus','System.Private.Windows.Core')
$dir=$PSScriptRoot
$jobs=Get-Content -LiteralPath "$dir\jobs.json" -Raw | ConvertFrom-Json
$regions=@(@{e=@(44,12,51,43);m=@(54,50,44,40)},@{e=@(3,9,39,25);m=@(9,28,27,14)},@{e=@(17,23,98,52);m=@(40,65,47,38)},@{e=@(14,24,104,61);m=@(35,75,57,43)},@{e=@(20,23,80,45);m=@(35,60,45,38)},@{e=@(30,12,99,57);m=@(52,50,55,48)})
$names=@('01-愤怒','02-疑惑','03-惊讶','04-伤心','05-认真','06-高兴')
$report=@()
for($c=0;$c -lt 6;$c++){$j=$jobs[$c];$folder="$dir\$($j.id)";New-Item -ItemType Directory -Force -Path $folder | Out-Null
for($e=0;$e -lt 6;$e++){$out="$folder\$($names[$e]).png";$n=[Expressions]::Export("$dir\inputs\$($j.id)-original.png","$dir\inputs\$($j.id)-atlas.png",$out,$j.x,$j.y,$j.w,$j.h,$e,[int[]]$regions[$c].e,[int[]]$regions[$c].m);$report+=@{character=$j.id;expression=$names[$e];changedPixels=$n;file=$out}}}
$report | ConvertTo-Json | Set-Content -LiteralPath "$dir\export-report.json" -Encoding utf8
Write-Output "Exported $($report.Count) portraits."
