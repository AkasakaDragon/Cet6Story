"""Simple transfer effect over existing art; last second is completely white."""
from pathlib import Path
import subprocess
ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'assets/opening/tavern'
FF=ROOT/'tools/ffmpeg/ffmpeg.exe'
vf="scale=1280:720:flags=neighbor,fade=t=out:st=2.4:d=2.6:color=white"
sound="[1:a]atrim=0:6,afade=t=in:d=1.4,afade=t=out:st=4.4:d=1.6,volume=0.18[a];[2:a]atrim=0:6,afade=t=in:d=2.0,afade=t=out:st=4.0:d=2.0,volume=0.2[b];[a][b]amix=inputs=2:normalize=0[audio]"
subprocess.run([str(FF),'-hide_banner','-loglevel','error','-y','-loop','1','-framerate','24','-i',str(ROOT/'chapters/art/tavern/goddess-sanctuary.png'),'-f','lavfi','-i','anoisesrc=color=pink:sample_rate=44100','-f','lavfi','-i','sine=frequency=523.25:sample_rate=44100','-vf',vf,'-filter_complex',sound,'-map','0:v','-map','[audio]','-t','6','-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-c:a','aac','-b:a','160k','-movflags','+faststart',str(OUT/'goddess-transfer.mp4')],check=True)
subprocess.run([str(FF),'-hide_banner','-loglevel','error','-y','-i',str(OUT/'goddess-transfer.mp4'),'-vn','-c:a','pcm_s16le',str(OUT/'goddess-transfer.wav')],check=True)
print('Created 6s transfer CG; last second is pure white.')
