from pathlib import Path
import subprocess
import wave
import numpy as np

folder=Path(__file__).resolve().parent
root=folder.parents[2]
sr=44100
duration=6
rng=np.random.default_rng(352)
t=np.arange(duration*sr)/sr
sound=np.zeros((len(t),2))
noise=rng.normal(0,1,len(t))
soft=np.convolve(noise,np.ones(17)/17,mode='same')
# Room fan fades as the screen's electrical disturbance grows.
sound+=soft[:,None]*.025*np.minimum(t/.15,1)[:,None]
for start in (.35,.62,1.08):
    local=np.arange(int(.08*sr))/sr
    burst=rng.normal(0,1,len(local))*.035*np.sin(np.pi*local/.08)**2
    offset=int(start*sr)
    sound[offset:offset+len(burst)]+=burst[:,None]
# Broad-band portal rush rather than a dialogue/UI notification tone.
progress=np.clip((t-3.6)/1.7,0,1)
env=np.sin(np.pi*np.clip((t-3.6)/2.4,0,1))**2
sound[:,0]+=soft*(.04+.32*progress)*env
sound[:,1]+=np.roll(soft,int(.012*sr))*(.04+.32*progress)*env
low=np.sin(2*np.pi*(42*t+7*t*t))*.035*env
sound+=low[:,None]
sound*=np.minimum((duration-t)/.35,1)[:,None]
with wave.open(str(folder/'屏幕异变-环境音效.wav'),'wb') as w:
    w.setnchannels(2);w.setsampwidth(2);w.setframerate(sr)
    w.writeframes((np.clip(sound,-.9,.9)*32767).astype('<i2').tobytes())

captions=[(1,.65,2.4,'怎么回事？','What is going on?'),(2,2.65,5.2,'跨界连接已建立。','Connection to another world established.')]
font='C\\:/Windows/Fonts/msyh.ttc'
vf="scale=640:360:flags=neighbor,zoompan=z='1.055+0.06*on/143':x='iw*0.68-iw/zoom*0.68':y='ih*0.53-ih/zoom*0.53':d=144:s=640x360:fps=24,scale=1280:720:flags=neighbor,eq=brightness='0.009*sin(t*13)':eval=frame"
vf+=",drawbox=x=0:y=ih-120:w=iw:h=120:color=black@0.30:t=fill:enable='between(t,0.65,2.4)+between(t,2.65,5.2)'"
for index,begin,end,zh,en in captions:
    for language,text,size,y,color in [('中文',zh,28,'h-98','white'),('英文',en,24,'h-56','0xE7E9ED')]:
        filename=f'字幕{index}-{language}.txt'
        (folder/filename).write_text(text,encoding='utf-8')
        vf+=f",drawtext=fontfile='{font}':textfile={filename}:fontsize={size}:fontcolor={color}:borderw=2:bordercolor=black:x=(w-tw)/2:y={y}:enable='between(t,{begin},{end})'"
vf+=',fade=t=out:st=5.25:d=0.7:color=white'
subprocess.run([str(root/'tools/ffmpeg/ffmpeg.exe'),'-y','-loglevel','error','-i','场景-v1.png','-i','屏幕异变-环境音效.wav','-vf',vf,'-t','6','-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-c:a','aac','-b:a','160k','-movflags','+faststart','第二段CG-屏幕异变-v1.mp4'],cwd=folder,check=True)
print('Created second CG: 6 seconds, bilingual subtitles, portal transition and original effects.')
