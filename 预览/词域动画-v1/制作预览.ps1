param([Parameter(Mandatory=$true)][string]$SourceSheet)
$ErrorActionPreference = 'Stop'
$projectRoot = 'D:\Game\Cet6Story-v3.1'
$ffmpeg = Join-Path $projectRoot 'tools\ffmpeg\ffmpeg.exe'
Set-Location $PSScriptRoot
Copy-Item -LiteralPath $SourceSheet -Destination 'CG分镜.png'
Copy-Item -LiteralPath (Join-Path $projectRoot 'assets\rogue\audio\forest-loop.wav') -Destination 'forest-loop.wav'
$durations = @(6,7,7,6,6,5)
for ($i=0; $i -lt 6; $i++) {
  $x = $i % 2
  $y = [Math]::Floor($i / 2)
  $frames = $durations[$i] * 24
  $filter = "crop=iw/2:ih/3:iw/2*$x`:ih/3*$y,scale=1920:1080:flags=neighbor,zoompan=z='1+0.055*on/$frames':x='iw/2-iw/zoom/2':y='ih/2-ih/zoom/2':d=$frames`:s=960x540:fps=24,fade=t=in:st=0:d=0.25,fade=t=out:st=$($durations[$i]-0.25):d=0.25,setsar=1"
  & $ffmpeg -hide_banner -loglevel error -y -i 'CG分镜.png' -vf $filter -t $durations[$i] -c:v libx264 -preset fast -crf 19 -pix_fmt yuv420p "shot-$i.mp4"
  if ($LASTEXITCODE -ne 0) { throw "镜头 $i 制作失败" }
}
(0..5 | ForEach-Object { "file 'shot-$_.mp4'" }) | Set-Content -LiteralPath 'clips.txt' -Encoding utf8
& $ffmpeg -hide_banner -loglevel error -y -f concat -safe 0 -i clips.txt -stream_loop -1 -i forest-loop.wav -vf 'subtitles=cg-subtitles.ass' -af 'volume=0.30,afade=t=in:d=1,afade=t=out:st=35:d=2' -t 37 -c:v libx264 -preset fast -crf 19 -pix_fmt yuv420p -c:a aac -b:a 160k -movflags +faststart '词域背景CG-动态分镜.mp4'
if ($LASTEXITCODE -ne 0) { throw 'CG合成失败' }
Copy-Item -LiteralPath (Join-Path $projectRoot 'assets\rogue\tower\terrain.png') -Destination 'map.png'
Copy-Item -LiteralPath (Join-Path $projectRoot 'assets\rogue\tower\forest.png') -Destination 'forest.png'
Copy-Item -LiteralPath (Join-Path $projectRoot 'chapters\art\neon\xingyao.png') -Destination 'xingyao.png'
Copy-Item -LiteralPath (Join-Path $projectRoot 'assets\rogue\tower\forest-enemies.png') -Destination 'enemies.png'
Copy-Item -LiteralPath (Join-Path $projectRoot 'assets\rogue\audio\attack.wav') -Destination 'attack.wav'
$battleFilter = "[0:v]scale=960:540:flags=neighbor,setsar=1,format=rgba[m];[1:v]scale=960:540:flags=neighbor,setsar=1,format=rgba[f];[2:v]scale=-1:380:flags=neighbor[p];[3:v]crop=iw*0.36:ih*0.62:0:ih*0.38,scale=-1:250:flags=neighbor[e];[f][p]overlay=x=90:y=160:format=auto[b1];[b1][e]overlay=x=660:y=290:format=auto,format=yuv444p[b];[m]format=yuv444p[a];[a][b]xfade=transition=custom:duration=0.65:offset=0.3:expr='if(lt(abs(X/W+Y/H-2*(1-P)),0.018),if(eq(PLANE,0),220,if(eq(PLANE,1),70,155)),if(lt(X/W+Y/H,2*(1-P)),B,A))',subtitles=battle-subtitles.ass,format=yuv420p[v]"
& $ffmpeg -hide_banner -loglevel error -y -loop 1 -framerate 30 -i map.png -loop 1 -framerate 30 -i forest.png -loop 1 -framerate 30 -i xingyao.png -loop 1 -framerate 30 -i enemies.png -i attack.wav -filter_complex $battleFilter -map '[v]' -map 4:a -af 'adelay=300|300,apad' -t 1.5 -c:v libx264 -preset fast -crf 18 -c:a aac -movflags +faststart '进入战斗-裂隙转场.mp4'
if ($LASTEXITCODE -ne 0) { throw '战斗转场制作失败' }

