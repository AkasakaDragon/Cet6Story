"""12-second illustrated entrance cutscene with narration, captions and Foley."""
from pathlib import Path
import json,math,subprocess,wave
import numpy as np
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'assets/opening/tavern'
FF=ROOT/'tools/ffmpeg/ffmpeg.exe'
W,H,FPS,SECONDS=1280,720,24,12
c=json.loads((ROOT/'chapters/10-tavern-prologue.json').read_text(encoding='utf-8'))
line=c['lines'][17]
room=Image.open(ROOT/'chapters/art/tavern/tavern-ruins.png').convert('RGB').resize((W,H),Image.Resampling.NEAREST)
entrance=Image.open(ROOT/'chapters/art/tavern/tavern-entrance-spore.png').convert('RGB').resize((W,H),Image.Resampling.NEAREST)
font=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',24)
small=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',22)
def wrap(text,f,chinese=False):
    result=[];current=''
    for token in list(text) if chinese else text.split():
        next=current+('' if chinese or not current else ' ')+token
        if current and f.getlength(next)>W-100:result.append(current);current=token
        else:current=next
    return result+[current]
def frame(t):
    mix=max(0,min(1,(t-1.45)/.20))
    im=Image.blend(room,entrance,mix)
    zoom=1+.025*math.sin(math.pi*max(0,min(1,(t-1.7)/10.3)))
    shake=math.exp(-max(0,t-1.35)*9)*3 if t>=1.35 else 0
    cw,ch=W/zoom,H/zoom;x=(W-cw)/2+math.sin(t*39)*shake;y=(H-ch)/2+math.sin(t*31)*shake
    x=max(0,min(W-cw,x));y=max(0,min(H-ch,y))
    im=im.crop((round(x),round(y),round(x+cw),round(y+ch))).resize((W,H),Image.Resampling.NEAREST)
    if 1.8<=t<1.8+line['end']-line['start']:
        layer=Image.new('RGBA',(W,H));d=ImageDraw.Draw(layer)
        rows=[(text,font) for text in wrap(line['translation'],font,True)]+[(text,small) for text in wrap(line['text'],small)]
        height=len(rows)*34+24;d.rectangle((0,H-height,W,H),fill=(0,0,0,155))
        for i,(text,f) in enumerate(rows):d.text(((W-f.getlength(text))/2,H-height+10+i*34),text,font=f,fill='white',stroke_width=1,stroke_fill='black')
        im=Image.alpha_composite(im.convert('RGBA'),layer).convert('RGB')
    return im
sr=24000;n=sr*SECONDS;rng=np.random.default_rng(60109)
sound=rng.normal(0,.002,(n,2))
def add(start,data,volume=1):
    i=round(start*sr);count=min(len(data),n-i);sound[i:i+count]+=data[:count,None]*volume
for start,strength in [(1.1,.08),(1.38,.15),(2.0,.045),(2.35,.04),(2.7,.04)]:
    t=np.arange(int(.23*sr))/sr
    hit=(rng.normal(0,1,len(t))*.5+np.sin(t*2*np.pi*85)) * np.exp(-t*24)
    add(start,hit,strength)
with wave.open(str(ROOT/'chapters/audio/tavern/prologue.wav'),'rb') as r:
    r.setpos(round(line['start']*sr));pcm=np.frombuffer(r.readframes(round((line['end']-line['start'])*sr)),dtype='<i2').astype(float)/32768
add(1.8,pcm)
growl=subprocess.run([str(FF),'-hide_banner','-loglevel','error','-i',str(ROOT/'assets/rogue/audio/monsters/spore-normal.wav'),'-ar',str(sr),'-ac','1','-f','s16le','pipe:1'],check=True,stdout=subprocess.PIPE).stdout
add(3.0,np.frombuffer(growl,dtype='<i2').astype(float)/32768,.16)
sound[-sr:]*=np.linspace(1,0,sr)[:,None]
audio=OUT/'knight-entrance.wav'
with wave.open(str(audio),'wb') as w:
    w.setnchannels(2);w.setsampwidth(2);w.setframerate(sr);w.writeframes((np.clip(sound,-.95,.95)*32767).astype('<i2').tobytes())
cmd=[str(FF),'-hide_banner','-loglevel','error','-y','-f','rawvideo','-pix_fmt','rgb24','-s',f'{W}x{H}','-r',str(FPS),'-i','pipe:0','-i',str(audio),'-t',str(SECONDS),'-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-c:a','aac','-b:a','160k','-movflags','+faststart',str(OUT/'knight-entrance.mp4')]
p=subprocess.Popen(cmd,stdin=subprocess.PIPE)
try:
    for i in range(FPS*SECONDS):p.stdin.write(frame(i/FPS).tobytes())
    p.stdin.close()
    if p.wait()!=0:raise RuntimeError('Encoder failed')
finally:
    if p.poll() is None:p.kill()
print('Created knight entrance: 12s, bilingual captions, Christopher narration, door and footsteps.')
