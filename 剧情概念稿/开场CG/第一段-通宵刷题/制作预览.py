from pathlib import Path
import subprocess
import wave
import numpy as np

folder=Path(__file__).resolve().parent
root=folder.parents[2]
sr=44100
rng=np.random.default_rng(347)
audio=np.zeros((8*sr,2))
for second in range(8):
    t=np.arange(int(.045*sr))/sr
    sound=(np.sin(2*np.pi*(1550 if second%2==0 else 1250)*t)*.18+rng.normal(0,.3,len(t)))*np.exp(-t*180)*.055
    at=int((second+.2)*sr)
    audio[at:at+len(sound),0]+=sound*.7
    audio[at:at+len(sound),1]+=sound
for second in (.9,1.15,1.36,2.4,2.62,3.5):
    t=np.arange(int(.055*sr))/sr
    sound=(rng.normal(0,.35,len(t))*.25+np.sin(2*np.pi*650*t)*.15)*np.exp(-t*95)*.14
    at=int(second*sr)
    audio[at:at+len(sound)]+=sound[:,None]
with wave.open(str(folder/'深夜环境声.wav'),'wb') as out:
    out.setnchannels(2);out.setsampwidth(2);out.setframerate(sr)
    out.writeframes((audio*32767).astype('<i2').tobytes())
(folder/'独白1.txt').write_text('再做一套……',encoding='utf-8')
(folder/'独白2.txt').write_text('做完就睡。',encoding='utf-8')
font="C\\:/Windows/Fonts/msyh.ttc"
vf="scale=640:360:flags=neighbor,zoompan=z='1+0.055*on/191':x='iw*0.68-iw/zoom*0.68':y='ih*0.53-ih/zoom*0.53':d=192:s=640x360:fps=24,scale=1280:720:flags=neighbor"
for i,begin,end in [(1,1.5,4.6),(2,4.8,7.8)]:
    vf+=",drawtext=fontfile='"+font+"':textfile=独白"+str(i)+".txt:fontsize=28:fontcolor=white:borderw=2:bordercolor=black:x=(w-tw)/2:y=h-70:enable='between(t,"+str(begin)+","+str(end)+")'"
command=[str(root/'tools/ffmpeg/ffmpeg.exe'),'-y','-i','场景-v1.png','-i','深夜环境声.wav','-vf',vf,'-t','8','-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-c:a','aac','-b:a','160k','-movflags','+faststart','第一段CG-通宵刷题-v1.mp4']
subprocess.run(command,cwd=folder,check=True,stdout=subprocess.DEVNULL,stderr=subprocess.PIPE)
print('Created 8-second 1280x720 CG preview.')
