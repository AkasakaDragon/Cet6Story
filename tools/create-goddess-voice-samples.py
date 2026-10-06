"""Generate comparable goddess voice auditions without changing game audio."""
from pathlib import Path
import asyncio,json,subprocess,sys,urllib.request,wave
ROOT=Path(__file__).resolve().parent.parent
sys.path.insert(0,str(ROOT/'.validation/tts-deps'))
import edge_tts
OUT=ROOT/'预览/女神声线对比'
OUT.mkdir(parents=True,exist_ok=True)
FF=ROOT/'tools/ffmpeg/ffmpeg.exe'
VOICES=['en-US-AriaNeural','en-US-JennyNeural','en-US-EmmaNeural','en-US-AvaNeural','en-GB-SoniaNeural','en-GB-LibbyNeural']
TEXT='Welcome, Lu Chuan. I am Iserya. I need your help to protect another world. A demon king is sealed beneath a border town. The seal is weakening.'
async def main():
    proxy=urllib.request.getproxies().get('https')
    available={v['ShortName'] for v in await edge_tts.list_voices(proxy=proxy)}
    selected=[v for v in VOICES if v in available]
    if len(selected)<6:
        selected.extend(v for v in ['en-US-MichelleNeural','en-US-SerenaMultilingualNeural'] if v in available and v not in selected)
    selected=selected[:6]
    sem=asyncio.Semaphore(3)
    async def render(i,voice):
        name=voice.split('-')[2].replace('Neural','')
        mp3=OUT/(str(i+1)+'-'+name+'.mp3');wav=mp3.with_suffix('.wav')
        async with sem:
            for attempt in range(3):
                try:
                    await edge_tts.Communicate(TEXT,voice,rate='-8%',pitch='-2Hz',proxy=proxy,receive_timeout=30).save(str(mp3));break
                except Exception:
                    if attempt==2:raise
                    await asyncio.sleep(2)
            subprocess.run([str(FF),'-hide_banner','-loglevel','error','-y','-i',str(mp3),'-ar','24000','-ac','1','-c:a','pcm_s16le',str(wav)],check=True)
            with wave.open(str(wav),'rb') as r:
                data=r.readframes(r.getnframes());duration=r.getnframes()/r.getframerate()
            print(str(i+1)+' '+voice+' '+str(round(duration,2))+'s',flush=True)
            return {'number':i+1,'voice':voice,'name':name,'file':wav.name,'seconds':duration},data
    results=await asyncio.gather(*(render(i,v) for i,v in enumerate(selected)))
    with wave.open(str(OUT/'六种声线-连续对比.wav'),'wb') as w:
        w.setnchannels(1);w.setsampwidth(2);w.setframerate(24000)
        for _,data in results:w.writeframes(data);w.writeframes(bytes(48000))
    (OUT/'声线清单.json').write_text(json.dumps({'text':TEXT,'rate':'-8%','pitch':'-2Hz','samples':[r[0] for r in results]},ensure_ascii=False,indent=2),encoding='utf-8')
    (OUT/'试听说明.md').write_text('# 女神声线对比\n\n所有声线使用同一段女神台词，语速 -8%、音调 -2Hz，无混响、音乐或音效。游戏现有配音未变更。\n\n'+TEXT+'\n\n中文：欢迎你，陆川。我是伊瑟雅。我需要你帮助守护另一个世界。一位魔王被封印在边境小镇地下。如今，封印正在衰弱。\n\n'+'\n'.join(str(r[0]['number'])+'. '+r[0]['voice'] for r in results)+'\n\n连续对比按以上顺序，每段之间留一秒静音。\n',encoding='utf-8')
asyncio.run(main())
