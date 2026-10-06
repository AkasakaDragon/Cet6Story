"""Render fixed character voices; publish audio and timestamps only after all lines succeed."""
from pathlib import Path
import asyncio, hashlib, json, shutil, subprocess, sys, wave, urllib.request
ROOT=Path(__file__).resolve().parent.parent
sys.path.insert(0,str(ROOT/'.validation/tts-deps'))
import edge_tts
CHAPTER=ROOT/'chapters/10-tavern-prologue.json'
OUT=ROOT/'chapters/audio/tavern'
WORK=ROOT/'.validation/tavern-neural'
WORK.mkdir(parents=True,exist_ok=True)
FF=ROOT/'tools/ffmpeg/ffmpeg.exe'
chapter=json.loads(CHAPTER.read_text(encoding='utf-8-sig'))
audio_path=CHAPTER.parent/chapter['audio']
voices=json.loads((OUT/'voices.json').read_text(encoding='utf-8'))
async def main():
    proxy=urllib.request.getproxies().get('https')
    available={v['ShortName'] for v in await edge_tts.list_voices(proxy=proxy)}
    for spec in voices.values():
        if spec['voice'] not in available: raise RuntimeError('Voice unavailable: '+spec['voice'])
    sem=asyncio.Semaphore(3)
    async def render(i,line):
        role=line.get('voiceRole') or line['speaker']
        if role not in voices: raise RuntimeError('Unknown voice role: '+role)
        line['voiceRole']=role
        spec=voices[role]
        key=hashlib.sha256(json.dumps([line['text'],spec],sort_keys=True).encode()).hexdigest()[:16]
        mp3=WORK/(str(i)+'-'+key+'.mp3'); wav=mp3.with_suffix('.wav')
        async with sem:
            if not mp3.exists() or mp3.stat().st_size<1000:
                for attempt in range(3):
                    try:
                        pending=mp3.with_suffix('.partial.mp3')
                        await edge_tts.Communicate(line['text'],**spec,proxy=proxy,receive_timeout=30).save(str(pending))
                        pending.replace(mp3);break
                    except Exception:
                        if mp3.exists(): mp3.unlink()
                        if attempt==2: raise
                        await asyncio.sleep(2*(attempt+1))
            subprocess.run([str(FF),'-hide_banner','-loglevel','error','-y','-i',str(mp3),'-ar','24000','-ac','1','-c:a','pcm_s16le',str(wav)],check=True)
            with wave.open(str(wav),'rb') as reader:
                data=reader.readframes(reader.getnframes())
            if len(data)<4800: raise RuntimeError('Empty line '+str(i))
            print('Rendered line '+str(i)+' / '+str(len(chapter['lines'])-1)+': '+spec['voice'],flush=True)
            return data
    pieces=await asyncio.gather(*(render(i,l) for i,l in enumerate(chapter['lines'])))
    frames=0
    target=WORK/'prologue.wav'
    with wave.open(str(target),'wb') as writer:
        writer.setnchannels(1);writer.setsampwidth(2);writer.setframerate(24000)
        for line,data in zip(chapter['lines'],pieces):
            line['start']=round(frames/24000,4);writer.writeframes(data);frames+=len(data)//2
            line['end']=round(frames/24000,4);writer.writeframes(bytes(12000));frames+=6000
    goddess=voices['伊瑟雅']['voice'].split('-')[2].replace('Neural','')
    chapter['audioLabel']='离线英文角色配音 · Brian / '+goddess+' / Michelle / Christopher / Guy'
    (WORK/'chapter.json').write_text(json.dumps(chapter,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    if not (WORK/('original-'+audio_path.name)).exists(): shutil.copy2(audio_path,WORK/('original-'+audio_path.name))
    if not (WORK/'original-chapter.json').exists(): shutil.copy2(CHAPTER,WORK/'original-chapter.json')
    shutil.copy2(target,audio_path);shutil.copy2(WORK/'chapter.json',CHAPTER)
    (ROOT/'预览/序幕CG-v1').mkdir(parents=True,exist_ok=True)
    for i,name in [(0,'女神-'+goddess+'-试听.wav'),(1,'男主-Brian-试听.wav')]:
        with wave.open(str(ROOT/'预览/序幕CG-v1'/name),'wb') as writer:
            writer.setnchannels(1);writer.setsampwidth(2);writer.setframerate(24000);writer.writeframes(pieces[i])
    print('Published '+str(round(frames/24000,2))+' seconds; all timestamps regenerated.')
asyncio.run(main())
