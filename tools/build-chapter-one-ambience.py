"""Build deterministic, quiet scene ambience under chapter 1 section 1 dialogue.
Preserve dry original, narration samples and all subtitle timestamps. No API calls.
"""
import json, wave
from pathlib import Path
import numpy as np
from scipy.signal import butter, sosfilt
ROOT=Path(__file__).resolve().parents[1]
chapter_path=ROOT/'chapters/11-tavern-01-01.json'
c=json.loads(chapter_path.read_text(encoding='utf-8-sig'))
source=ROOT/'chapters/audio/tavern/chapter-one/morning.wav'
with wave.open(str(source),'rb') as w:
 params=w.getparams();assert params.sampwidth==2
 dry=np.frombuffer(w.readframes(w.getnframes()),dtype='<i2').reshape(-1,params.nchannels).astype(np.float64)/32768
rate=params.framerate;n=len(dry);rng=np.random.default_rng(20261008)
output_dir=source.parent

def write(path,signal):
 signal=np.asarray(signal);signal=signal[:,None] if signal.ndim==1 else signal
 with wave.open(str(path),'wb') as w:
  w.setnchannels(signal.shape[1]);w.setsampwidth(2);w.setframerate(rate);w.writeframes(np.rint(np.clip(signal,-.999,.999)*32767).astype('<i2').tobytes())
def noise(count,lo,hi):
 band=sosfilt(butter(2,[lo,hi],btype='bandpass',fs=rate,output='sos'),rng.normal(size=count))
 return band/max(np.sqrt(np.mean(band*band)),1e-9)
def add(track,second,sound):
 start=int(second*rate);end=min(len(track),start+len(sound))
 if end>start:track[start:end]+=sound[:end-start]
def clink():
 t=np.arange(int(rate*.6))/rate
 return .005*np.exp(-t*13)*(np.sin(2*np.pi*1800*t)+.35*np.sin(2*np.pi*3070*t))*(1-np.exp(-t*600))
def bird():
 t=np.arange(int(rate*.55))/rate
 phase=2*np.pi*(2350*t+420*np.sin(t*15)/15)
 return .006*np.sin(phase)*np.sin(np.pi*t/.55)**2*(.5+.5*np.sin(t*65)**2)
scenes=[('morning-room',0,c['lines'][10]['start'],'清晨酒馆：仅一次远处杯碟声'),('kitchen',c['lines'][10]['start'],c['lines'][22]['start'],'厨房：无环境底噪'),('stone-road',c['lines'][22]['start'],n/rate,'石路：树叶风声、稀疏鸟鸣')]
bed=np.zeros(n);manifest=[]
for index,(name,start,end,label) in enumerate(scenes):
 a=int(start*rate);b=min(n,int(end*rate));count=b-a;t=np.arange(count)/rate
 # Low wind / hearth textures rather than a broad, constant hiss.
 # Shape indoor textures first, then set a measured loudness below speech.
 track=noise(count,[160,220,160][index],[900,1250,2300][index])*[.00265,.00280,.0016][index]
 envelope=np.interp(t,np.arange(0,t[-1]+8,4),rng.uniform(.45,1.0,len(np.arange(0,t[-1]+8,4))))
 track*=envelope
 if index==0:
  pass # The single distant clink is a separate, non-looping sound.
 elif index==1:
  # Small soft air fluctuations give the hearth a less mechanical texture.
  track+=noise(count,300,1100)*.00025*envelope**2
  rng_state=rng.bit_generator.state
  for at in np.arange(2.5,t[-1]-1,7.3):
   length=int(rate*.35)
   ember=noise(length,380,1600)*.0015*np.exp(-np.arange(length)/rate*12)
   add(track,float(at),ember)
  rng.bit_generator.state=rng_state
 else:
  for at in [2.8,11.7,23.6,35.8,44.2]:add(track,at,bird())
 track*=3
 fade=min(int(rate*.8),count//2);track[:fade]*=np.linspace(0,1,fade);track[-fade:]*=np.linspace(1,0,fade)
 if index<2:
  track[:]=0
 bed[a:b]+=track;write(output_dir/('ambience-'+name+'.wav'),track)
 manifest.append({'scene':label,'start':start,'end':end,'rms_db':round(20*np.log10(np.sqrt(np.mean(track*track))+1e-12),1) if np.any(track) else None})
single_clink=np.zeros(int(rate*6));add(single_clink,5.3,clink()*1.5)
write(output_dir/'ambience-distant-clink-once.wav',single_clink)
preview_bed=bed.copy();add(preview_bed,5.3,clink()*1.5)
mixed=dry+preview_bed[:,None];peak=float(np.max(np.abs(mixed)))
assert peak<1,'Mix clipping; reduce ambience before saving'
output=output_dir/'morning-scene-ambience.wav';write(output,mixed)
c['audio']='audio/tavern/chapter-one/morning.wav';c['audioLabel']='离线英文角色配音 · 独立场景环境音'
chapter_path.write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
(output_dir/'ambience-scenes.json').write_text(json.dumps({'source':source.name,'output':output.name,'method':'locally synthesized scene ambience; dialogue remains unchanged','scenes':manifest},ensure_ascii=False,indent=2),encoding='utf-8')
preview=ROOT/'配音试听/chapter-one-ambience-v6.wav';preview.parent.mkdir(exist_ok=True)
clips=[];environment_clips=[]
for name,start,end,label in scenes:
 a=int(start*rate);b=min(n,a+int(20*rate))
 # Match default in-game dialogue (85%) and ambience/BGM (70%) levels.
 sample=dry[a:b]*.85+preview_bed[a:b,None]*.70
 clips.extend([sample,np.zeros((rate,params.nchannels))])
 if name!='stone-road':environment_clips.extend([preview_bed[a:b]*.70,np.zeros(rate)])
write(preview,np.concatenate(clips))
write(preview.parent/'chapter-one-ambience-v6-environment-only.wav',np.concatenate(environment_clips))
with wave.open(str(output),'rb') as w:assert w.getnframes()==n and w.getframerate()==rate
print(json.dumps({'duration':n/rate,'peak':peak,'scenes':manifest,'preview':str(preview)},ensure_ascii=True))
