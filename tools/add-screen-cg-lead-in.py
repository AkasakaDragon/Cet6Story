"""Add the requested spoken lead-in and bilingual screen text, preserving existing audio."""
from pathlib import Path
import asyncio,json,sys,subprocess,wave,copy,urllib.request
ROOT=Path(__file__).resolve().parent.parent
sys.path.insert(0,str(ROOT/'.validation/tts-deps'))
import edge_tts
async def main():
 p=ROOT/'chapters/10-tavern-prologue.json';c=json.loads(p.read_text(encoding='utf-8-sig'))
 if any(l.get('scriptId')=='序-010-补' for l in c['lines']):raise RuntimeError('Lead-in already installed')
 where=next(i for i,l in enumerate(c['lines']) if l.get('scriptId')=='序-011')
 work=ROOT/'.validation/screen-lead-in-20261014';work.mkdir(parents=True,exist_ok=True)
 (work/'before.json').write_bytes(p.read_bytes())
 spec=json.loads((ROOT/'chapters/audio/tavern/voices.json').read_text(encoding='utf-8'))['陆川']
 en='Wait, what is that on my screen?'
 mp3=work/'lead-in.mp3';clip=work/'lead-in.wav'
 await edge_tts.Communicate(en,**spec,proxy=urllib.request.getproxies().get('https')).save(str(mp3))
 subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'),'-hide_banner','-loglevel','error','-y','-i',str(mp3),'-ar','24000','-ac','1','-c:a','pcm_s16le',str(clip)],check=True)
 audio=ROOT/'chapters'/c['audio']
 with wave.open(str(audio),'rb') as reader:
  params=reader.getparams();data=reader.readframes(reader.getnframes())
 with wave.open(str(clip),'rb') as reader:extra=reader.readframes(reader.getnframes())
 assert params.nchannels==1 and params.sampwidth==2 and params.framerate==24000
 extra+=bytes(12000);delta=len(extra)/48000;start=c['lines'][where]['start'];cut=round(start*24000)*2
 l=copy.deepcopy(c['lines'][where-1]);l.update(scriptId='序-010-补',speaker='陆川',actor='luchuan',voiceRole='陆川',text=en,translation='等等，我屏幕上的是什么？',expression='confused',hidePortraits=True,portraitIds=[],start=start,end=round(start+delta-.25,4));l.pop('cgBefore',None)
 for old in c['lines'][where:]:old['start']=round(old['start']+delta,4);old['end']=round(old['end']+delta,4)
 c['lines'].insert(where,l)
 screen=next(l for l in c['lines'] if l.get('scriptId')=='序-011')
 screen['translation']=next(l['translation'] for l in c['lines'] if l.get('scriptId')=='序-012')
 target=work/'updated.wav'
 with wave.open(str(target),'wb') as writer:
  writer.setparams(params);writer.writeframes(data[:cut]+extra+data[cut:])
 (work/'before.wav').write_bytes(audio.read_bytes())
 audio.write_bytes(target.read_bytes());p.write_text(json.dumps(c,ensure_ascii=False,indent=2),encoding='utf-8')
 print('Installed bilingual screen cue and Brian lead-in before cg02; preserved all123 original cues and existing voice audio.')
asyncio.run(main())
