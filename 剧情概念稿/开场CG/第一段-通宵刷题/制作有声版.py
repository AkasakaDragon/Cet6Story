from pathlib import Path
import asyncio
import sys
import subprocess
import wave
import numpy as np

folder=Path(__file__).resolve().parent
root=folder.parents[2]
sys.path.insert(0,str(root/'.validation/tts-runtime'))
import edge_tts
import aiohttp
sr=44100
ffmpeg=str(root/'tools/ffmpeg/ffmpeg.exe')

async def speech():
    for i,text in [(1,'再做一套……'),(2,'做完就睡。')]:
        target=folder/f'男主独白{i}-云希.mp3'
        if target.exists() and target.stat().st_size>0:
            continue
        async with aiohttp.TCPConnector(resolver=aiohttp.ThreadedResolver()) as connector:
            await edge_tts.Communicate(text,'zh-CN-YunxiNeural',rate='-12%',pitch='-2Hz',connector=connector,receive_timeout=20).save(str(target))
asyncio.run(speech())

rng=np.random.default_rng(348)
t=np.arange(8*sr)/sr
# Quiet original sustained synth chords and computer-fan white noise.
music=np.zeros((len(t),2))
for midi,level,pan in [(50,.019,-.3),(57,.013,.3),(60,.009,-.15),(64,.006,.2)]:
    f=440*2**((midi-69)/12)
    tone=(np.sin(2*np.pi*f*t)+.12*np.sin(2*np.pi*f*2*t))*(.9+.1*np.sin(2*np.pi*.18*t))
    music[:,0]+=tone*level*(1-pan)
    music[:,1]+=tone*level*(1+pan)
noise=rng.normal(0,1,(len(t),2))
noise=(noise+np.roll(noise,1,axis=0)+np.roll(noise,2,axis=0))/3
music+=noise*.008
fade=np.minimum(t/.6,1)*np.minimum((8-t)/.6,1)
music*=fade[:,None]
with wave.open(str(folder/'深夜环境声.wav'),'rb') as w:
    ambience=np.frombuffer(w.readframes(w.getnframes()),dtype='<i2').reshape(-1,2)/32768
mix=music+ambience*1.5
for i,start in [(1,1.5),(2,4.8)]:
    wav=folder/f'男主独白{i}-云希.wav'
    subprocess.run([ffmpeg,'-y','-loglevel','error','-i',str(folder/f'男主独白{i}-云希.mp3'),'-ar',str(sr),'-ac','2','-c:a','pcm_s16le',str(wav)],check=True)
    with wave.open(str(wav),'rb') as w:
        voice=np.frombuffer(w.readframes(w.getnframes()),dtype='<i2').reshape(-1,2)/32768
    peak=np.max(np.abs(voice))
    if peak>0:
        voice*=.62/peak
    at=int(start*sr)
    end=min(len(mix),at+len(voice))
    mix[at:end]*=.55
    mix[at:end]+=voice[:end-at]
with wave.open(str(folder/'深夜配乐-白噪声.wav'),'wb') as w:
    w.setnchannels(2);w.setsampwidth(2);w.setframerate(sr);w.writeframes((music*32767).astype('<i2').tobytes())
with wave.open(str(folder/'有声混音-v2.wav'),'wb') as w:
    w.setnchannels(2);w.setsampwidth(2);w.setframerate(sr);w.writeframes((np.clip(mix,-.95,.95)*32767).astype('<i2').tobytes())
subprocess.run([ffmpeg,'-y','-loglevel','error','-i',str(folder/'第一段CG-通宵刷题-v1.mp4'),'-i',str(folder/'有声混音-v2.wav'),'-map','0:v:0','-map','1:a:0','-c:v','copy','-c:a','aac','-b:a','192k','-t','8','-movflags','+faststart',str(folder/'第一段CG-通宵刷题-v2-有声版.mp4')],check=True)
print('Created voiced CG with original ambient score and white noise.')
