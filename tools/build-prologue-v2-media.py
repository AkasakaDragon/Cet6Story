"""Build native800 square CG clips, including the stone-door push-in transition."""
from pathlib import Path
import subprocess, wave
ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'assets/story/prologue-v2';FF=ROOT/'tools/ffmpeg/ffmpeg.exe'
WORK=ROOT/'.validation/prologue-rebuild-20261013/media';WORK.mkdir(parents=True,exist_ok=True)
def run(args):subprocess.run([str(FF),'-hide_banner','-loglevel','error','-y']+args,check=True)
def encode(image,target,seconds,filters=None):
 args=['-loop','1','-framerate','24','-i',str(image)]
 if filters:args+=['-vf',filters]
 run(args+['-t',str(seconds),'-an','-c:v','libx264','-preset','fast','-crf','16','-pix_fmt','yuv420p',str(target)])
for cg in ['cg01','cg02','cg04','cg05']:
 encode(OUT/(cg+'.png'),OUT/(cg+'.mp4'),4)
 print('Built '+cg,flush=True)
door=WORK/'door.mp4';arrival=WORK/'arrival.mp4'
filter="zoompan=z='1+3.2*pow(min(on/119,1),2)':x='max(0,min(iw-iw/zoom,iw*0.55-iw/zoom/2))':y='max(0,min(ih-ih/zoom,ih*0.42-ih/zoom/2))':d=120:s=800x800:fps=24,split[sharp][blur];[blur]gblur=sigma=14[soft];[sharp][soft]blend=all_expr='A*(1-min(1,max(0,(T-3.6)/0.8)))+B*min(1,max(0,(T-3.6)/0.8))',fade=t=out:st=4.35:d=0.65:color=white"
run(['-i',str(OUT/'cg02.png'),'-filter_complex',filter,'-t','5','-an','-c:v','libx264','-preset','fast','-crf','16','-pix_fmt','yuv420p',str(door)])
encode(OUT/'cg03.png',arrival,3,'fade=t=in:st=0:d=0.65:color=white')
listing=WORK/'concat.txt';listing.write_text("file 'door.mp4'\nfile 'arrival.mp4'\n",encoding='utf-8')
run(['-f','concat','-safe','0','-i',str(listing),'-c','copy',str(OUT/'cg03.mp4')])
# Weather loops independently across movies, continue prompts and dialogue.
for cg in ['cg01','cg02','cg03','cg04','cg05']:
 seconds=8 if cg=='cg03' else 4
 with wave.open(str(OUT/(cg+'.wav')),'wb') as w:
  w.setnchannels(1);w.setsampwidth(2);w.setframerate(24000);w.writeframes(bytes(24000*2*seconds))
print('Built5 CGs and800px push-in/blur/arrival transition.')
if (OUT/'portal-open.png').exists():
 import sys
 subprocess.run([sys.executable,str(ROOT/'tools/build-prologue-portal.py')],check=True)
