[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
$taskRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskFfmpeg=Join-Path $taskRoot 'tools/ffmpeg/ffmpeg.exe'
$taskSpecs=Get-Content (Join-Path $taskRoot 'chapters/audio/tavern/chapter-two/cg.json') -Raw | ConvertFrom-Json
foreach($taskKind in @('intro','outro')){
 $taskScene=if($taskKind -eq 'intro'){'chapter-two-market.png'}else{'chapter-two-lantern.png'}
 $taskImage=Join-Path $taskRoot ('chapters/art/tavern/'+$taskScene)
 $taskOutput=Join-Path $taskRoot ('assets/opening/tavern/chapter-two-'+$taskKind+'.mp4')
 $taskTemporary=Join-Path $taskRoot ('assets/opening/tavern/chapter-two-'+$taskKind+'.rendering.mp4')
 $taskDuration=[double]$taskSpecs.$taskKind.seconds
 $taskEnd=($taskDuration-.45).ToString('0.####',[Globalization.CultureInfo]::InvariantCulture)
 $taskLength=$taskDuration.ToString('0.####',[Globalization.CultureInfo]::InvariantCulture)
 $taskFilter="scale=1280:720:force_original_aspect_ratio=increase:flags=neighbor,crop=1280:720,setsar=1,fade=t=in:st=0:d=0.35,fade=t=out:st=${taskEnd}:d=0.45"
 try{
  & $taskFfmpeg -nostdin -loglevel error -y -loop 1 -framerate 24 -i $taskImage -t $taskLength -vf $taskFilter -an -c:v libx264 -preset medium -crf 18 -pix_fmt yuv420p -movflags +faststart $taskTemporary
  if($LASTEXITCODE -ne 0){throw "CG rendering failed: $taskKind"}
  Move-Item -LiteralPath $taskTemporary -Destination $taskOutput -Force
 }finally{if(Test-Path -LiteralPath $taskTemporary){Remove-Item -LiteralPath $taskTemporary}}
 Write-Output "Section two CG rendered: $taskKind ($taskLength seconds)"
}
