"""Reuse approved bilingual CG clips, then reveal the existing goddess scene."""
from pathlib import Path
import subprocess
ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / 'assets/opening/tavern'
WORK = ROOT / '.validation/tavern-opening-build'
FF = ROOT / 'tools/ffmpeg/ffmpeg.exe'
OUT.mkdir(parents=True, exist_ok=True)
WORK.mkdir(parents=True, exist_ok=True)
def run(args):
    subprocess.run([str(FF), '-hide_banner', '-loglevel', 'error', '-y']+args, check=True)
first = ROOT/'剧情概念稿/开场CG/第一段-通宵刷题/第一段CG-通宵刷题-v5-双语无提示音.mp4'
second = ROOT/'剧情概念稿/开场CG/第二段-屏幕异变/第二段CG-屏幕异变-v1.mp4'
arrival = WORK/'arrival.mp4'
vf = "scale=1280:720:flags=neighbor,fade=t=in:st=0:d=1.25:color=white"
run(['-loop','1','-framerate','24','-i',str(ROOT/'chapters/art/tavern/goddess-sanctuary.png'),'-f','lavfi','-i','anullsrc=r=44100:cl=stereo','-vf',vf,'-t','4','-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-c:a','aac',str(arrival)])
# A gentle arrival bell and fading portal rush; no dialogue before the listening scene.
audio_filter = "[1:a]atrim=0:1.6,afade=t=in:d=0.02,afade=t=out:st=0.15:d=1.45,volume=0.15[a];[2:a]atrim=0:2.8,afade=t=out:st=0:d=2.8,volume=0.08,adelay=500|500[b];[a][b]amix=inputs=2:normalize=0,apad,atrim=0:4[out]"
run(['-i',str(arrival),'-f','lavfi','-i','anoisesrc=color=pink:sample_rate=44100','-f','lavfi','-i','sine=frequency=659.25:sample_rate=44100','-filter_complex',audio_filter,'-map','0:v','-map','[out]','-c:v','copy','-c:a','aac',str(WORK/'arrival-sound.mp4')])
run(['-i',str(first),'-i',str(second),'-i',str(WORK/'arrival-sound.mp4'),'-filter_complex','[0:v][0:a][1:v][1:a][2:v][2:a]concat=n=3:v=1:a=1[v][a]','-map','[v]','-map','[a]','-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-c:a','aac','-b:a','160k','-t','18','-movflags','+faststart',str(OUT/'opening.mp4')])
run(['-i',str(OUT/'opening.mp4'),'-vn','-c:a','pcm_s16le','-ar','44100','-ac','2',str(OUT/'opening.wav')])
print('Created 18-second tavern opening with existing bilingual captions and effects.')
