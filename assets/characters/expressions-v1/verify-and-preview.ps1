$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System.Drawing;
public static class CheckPortrait {
public static string Check(string a,string b,int x,int y,int w,int h){using(var s=new Bitmap(a))using(var t=new Bitmap(b)){if(s.Width!=t.Width||s.Height!=t.Height)return "SIZE MISMATCH";int outside=0,alpha=0;for(int py=0;py<s.Height;py++)for(int px=0;px<s.Width;px++){var u=s.GetPixel(px,py);var v=t.GetPixel(px,py);if(u.A!=v.A)alpha++;if((px<x||px>=x+w||py<y||py>=y+h)&&u.ToArgb()!=v.ToArgb())outside++;}return "outsideFace="+outside+", alphaChanges="+alpha;}}
}
'@ -ReferencedAssemblies @('System.Drawing.Common','System.Drawing.Primitives','System.Private.Windows.GdiPlus','System.Private.Windows.Core')
$dir=$PSScriptRoot
$jobs=Get-Content -LiteralPath "$dir\jobs.json" -Raw | ConvertFrom-Json
$names=@('01-愤怒','02-疑惑','03-惊讶','04-伤心','05-认真','06-高兴')
$sheet=[System.Drawing.Bitmap]::new(1080,1080)
$g=[System.Drawing.Graphics]::FromImage($sheet)
$g.Clear([System.Drawing.Color]::FromArgb(30,39,39))
$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
$results=@()
for($row=0;$row -lt 6;$row++){
 $j=$jobs[$row]
 $full=[System.Drawing.Bitmap]::new(1500,580)
 $fg=[System.Drawing.Graphics]::FromImage($full)
 $fg.Clear([System.Drawing.Color]::FromArgb(30,39,39))
 $fg.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
 $fg.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::Half
 $font=[System.Drawing.Font]::new('Microsoft YaHei',14)
 for($col=0;$col -lt 6;$col++){
  $path="$dir\$($j.id)\$($names[$col]).png"
  $check=[CheckPortrait]::Check("$dir\inputs\$($j.id)-original.png",$path,$j.x,$j.y,$j.w,$j.h)
  if($check -ne 'outsideFace=0, alphaChanges=0'){throw "$path $check"}
  $results+="$($j.id)/$($names[$col]): $check"
  $b=[System.Drawing.Bitmap]::new($path)
  $g.DrawImage($b,[System.Drawing.Rectangle]::new($col*180,$row*180,180,180),[System.Drawing.Rectangle]::new($j.x,$j.y,$j.w,$j.h),[System.Drawing.GraphicsUnit]::Pixel)
  $boundX=$b.Width;$boundY=$b.Height;$maxX=0;$maxY=0
  # Use consistent source canvas and aspect ratio for previews; do not rescale individual expressions differently.
  $scale=[Math]::Min(245.0/$b.Width,520.0/$b.Height)
  $pw=[int]($b.Width*$scale);$ph=[int]($b.Height*$scale)
  $fg.DrawImage($b,[System.Drawing.Rectangle]::new($col*250+[int]((250-$pw)/2),30,$pw,$ph))
  $fg.DrawString($names[$col].Substring(3),$font,[System.Drawing.Brushes]::White,$col*250+95,545)
  $b.Dispose()
 }
 $full.Save("$dir\$($j.id)-完整立绘预览.png",[System.Drawing.Imaging.ImageFormat]::Png)
 $font.Dispose();$fg.Dispose();$full.Dispose()
}
$sheet.Save("$dir\六人六种表情预览.png",[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$sheet.Dispose()
$results | Set-Content -LiteralPath "$dir\像素一致性检查.txt" -Encoding utf8
Write-Output "Verified 36 images: original dimensions and alpha preserved; zero pixel changes outside face crops."
