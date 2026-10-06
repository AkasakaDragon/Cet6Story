"""Render a 15s layered-illustration animatic, not neural video generation.

Generated plates/cutouts are preserved. Pillow composites animation frames;
FFmpeg encodes H.264/AAC. No source game art or save files are overwritten.
"""
from pathlib import Path
import math
import subprocess
import wave
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent.parent
ART = ROOT / '剧情概念稿/开场CG/电脑穿越-v1'
OUT = ROOT / '预览/序幕CG-v1'
FFMPEG = ROOT / 'tools/ffmpeg/ffmpeg.exe'
W, H, FPS, SECONDS = 1280, 720, 24, 15
NEAREST = Image.Resampling.NEAREST

def ease(a, b, t):
    u = max(0.0, min(1.0, (t-a)/(b-a)))
    return u*u*(3-2*u)

def load(name):
    return Image.open(ART/name).convert('RGBA')

plate = load('dorm-plate.png').resize((W,H), NEAREST)
study = load('hero-study.png')
reaction = load('hero-reaction-v2.png')
sanctuary = Image.open(ROOT/'chapters/art/tavern/goddess-sanctuary.png').convert('RGBA').resize((W,H),NEAREST)
rng = np.random.default_rng(60106)
particles = [(rng.uniform(0,1),rng.uniform(0,1),rng.uniform(.65,1.5)) for _ in range(32)]

def character(frame, asset, scale, x, y, alpha=1, angle=0):
    height = round(420*scale)
    layer = asset.resize((round(height*asset.width/asset.height),height),NEAREST)
    if angle:
        layer = layer.rotate(angle, resample=NEAREST, expand=True)
    if alpha < 1:
        layer.putalpha(layer.getchannel('A').point(lambda a: round(a*alpha)))
    frame.alpha_composite(layer,(round(x),round(y)))

def light(frame,t,strength):
    if strength<=0:return frame
    # Low-resolution procedural light is upsampled with hard pixel boundaries.
    yy,xx=np.mgrid[0:H//4,0:W//4]
    radius=40+190*ease(5.8,8.6,t)
    dist=((xx-286)**2+(yy-91)**2)**.5
    opacity=(np.maximum(0,1-dist/radius)**2*strength*210).clip(0,230).astype('uint8')
    glow=np.zeros((H//4,W//4,4),dtype=np.uint8)
    glow[:,:,:3]=(224,240,255);glow[:,:,3]=opacity
    frame=Image.alpha_composite(frame,Image.fromarray(glow).resize((W,H),NEAREST))
    rays=Image.new('RGBA',(W,H));draw=ImageDraw.Draw(rays)
    for i,(phase,offset,speed) in enumerate(particles):
        progress=(phase+t*.30*speed)%1
        sx=660+offset*340;sy=210+(phase*390)
        x=sx+(1144-sx)*progress;y=sy+(360-sy)*progress
        opacity=int(130*strength*math.sin(progress*math.pi))
        tail=6+18*strength
        draw.line((int(x-tail),int(y+(sy-360)*.025),int(x),int(y)),fill=(220,242,255,opacity),width=2)
    return Image.alpha_composite(frame,rays)

def camera(frame,zoom,cx,cy):
    if abs(zoom-1)<.001:return frame
    cw,ch=W/zoom,H/zoom
    left=max(0,min(W-cw,cx-cw/2));top=max(0,min(H-ch,cy-ch/2))
    return frame.crop((round(left),round(top),round(left+cw),round(top+ch))).resize((W,H),NEAREST)

def render(t):
    if t<9.1:
        frame=plate.copy()
        pull=ease(6.0,8.6,t)
        x=820+190*pull;y=232-30*pull
        scale=1-.18*pull
        breath=round(math.sin(t*2.5)*1.2)*(1-pull)
        mix=ease(4.5,4.85,t)
        if mix<1:character(frame,study,scale,x,y+breath,1-mix)
        if mix>0:character(frame,reaction,scale,x-6*(1-pull),y+breath,mix,angle=-3*ease(5.1,6,t)+5*pull)
        screen=Image.new('RGBA',(W,H));d=ImageDraw.Draw(screen)
        pulse=(math.exp(-((t-4.22)/.085)**2)+.7*math.exp(-((t-4.48)/.08)**2))
        gain=min(1,.11*pulse+ease(4.75,7.4,t))
        d.polygon([(1138,327),(1189,333),(1162,394),(1111,390)],fill=(242,250,255,round(gain*245)))
        frame=Image.alpha_composite(frame,screen)
        frame=light(frame,t,gain)
        zoom=1+.07*ease(.0,4,t)+.055*ease(5.8,8.5,t)
        shake=ease(6.0,7.7,t)*(1-ease(8.0,8.6,t))
        frame=camera(frame,zoom,840+4*math.sin(t*31)*shake,360+2*math.sin(t*27)*shake)
        white=ease(7.75,8.85,t)
    else:
        frame=sanctuary.copy()
        # A small reveal settles exactly on the accepted conversation frame.
        frame=camera(frame,1+.06*(1-ease(9.1,13.9,t)),730,355)
        sparkle=Image.new('RGBA',(W,H));d=ImageDraw.Draw(sparkle)
        for phase,offset,speed in particles[:16]:
            x=int(300+offset*750);y=int(95+((phase*290-t*10*speed)%300))
            a=int(65*(.5+.5*math.sin(t*2+phase*8))*(1-ease(13,14.2,t)))
            d.rectangle((x,y,x+2,y+2),fill=(255,248,218,a))
        frame=Image.alpha_composite(frame,sparkle)
        white=1-ease(9.1,10.45,t)
    if white>0:
        frame=Image.alpha_composite(frame,Image.new('RGBA',(W,H),(250,252,255,round(white*255))))
    if t<.45:
        frame=Image.alpha_composite(frame,Image.new('RGBA',(W,H),(3,7,15,round((1-ease(0,.45,t))*255))))
    return frame.convert('RGB')

def audio():
    sr=44100;n=sr*SECONDS;time=np.arange(n)/sr
    mix=np.zeros((n,2),dtype=np.float64)
    # Very quiet room tone, original keyboard Foley, rising pull and arrival bell.
    bed=rng.normal(0,.0007,n)
    mix[:,0]+=bed;mix[:,1]+=bed
    def add(start,signal,pan=.0):
        begin=round(start*sr);end=min(n,begin+len(signal));signal=signal[:end-begin]
        mix[begin:end,0]+=signal*math.sqrt((1-pan)/2)
        mix[begin:end,1]+=signal*math.sqrt((1+pan)/2)
    for start in [.8,1.02,1.65,2.0,2.32,3.1,3.35]:
        tt=np.arange(round(.06*sr))/sr
        click=(rng.normal(0,1,len(tt))*.035+np.sin(tt*2*np.pi*1700)*.012)*np.exp(-tt*90)
        add(start,click,.42)
    for start in [4.22,4.48]:
        tt=np.arange(round(.13*sr))/sr
        add(start,np.sin(2*np.pi*tt*1050)*np.exp(-tt*28)*.021,.45)
    start=5.1;length=3.9;tt=np.arange(round(length*sr))/sr;u=tt/length
    env=np.sin(np.pi*u)**1.8
    filtered=np.convolve(rng.normal(0,1,len(tt)),np.ones(18)/18,mode='same')
    whoosh=filtered*.19*env+np.sin(2*np.pi*(70*tt+32*tt**2))*.052*env
    add(start,whoosh,.2)
    tt=np.arange(round(3.6*sr))/sr
    bell=sum(np.sin(2*np.pi*f*tt)*np.exp(-tt*(1.6+i*.5))/(i+1) for i,f in enumerate([659.25,1318.5,1846]))
    add(9.55,bell*.075,-.15)
    fade=np.minimum(1,time/.12)*np.minimum(1,(SECONDS-time)/.65)
    pcm=(np.clip(mix*fade[:,None],-.85,.85)*32767).astype('<i2')
    path=OUT/'电脑穿越-音效-v1.wav'
    with wave.open(str(path),'wb') as stream:
        stream.setnchannels(2);stream.setsampwidth(2);stream.setframerate(sr);stream.writeframes(pcm.tobytes())
    return path

def main():
    OUT.mkdir(parents=True,exist_ok=True)
    sound=audio();video=OUT/'电脑穿越-15秒-v2.mp4'
    cmd=[str(FFMPEG),'-hide_banner','-loglevel','error','-y','-f','rawvideo','-pixel_format','rgb24','-video_size',f'{W}x{H}','-framerate',str(FPS),'-i','pipe:0','-i',str(sound),'-c:v','libx264','-preset','medium','-crf','17','-pix_fmt','yuv420p','-c:a','aac','-b:a','192k','-t',str(SECONDS),'-movflags','+faststart',str(video)]
    encoder=subprocess.Popen(cmd,stdin=subprocess.PIPE)
    try:
        for i in range(FPS*SECONDS):
            encoder.stdin.write(render(i/FPS).tobytes())
            if i%72==0:print(f'Rendered {i}/{FPS*SECONDS} frames',flush=True)
        encoder.stdin.close()
        if encoder.wait()!=0:raise RuntimeError('FFmpeg encode failed')
    finally:
        if encoder.poll() is None:encoder.kill()
    print(video)

if __name__=='__main__':main()
