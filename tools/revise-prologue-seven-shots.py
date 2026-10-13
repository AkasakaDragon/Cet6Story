from pathlib import Path
import asyncio,copy,json,sys,wave,subprocess,urllib.request
ROOT=Path(__file__).resolve().parent.parent
sys.path.insert(0,str(ROOT/'.validation/tts-deps'))
import edge_tts
async def main():
 p=ROOT/'chapters/10-tavern-prologue.json';c=json.loads(p.read_text(encoding='utf-8-sig'));work=ROOT/'.validation/seven-shots-20261014';work.mkdir(exist_ok=True,parents=True)
 if any(l.get('scriptId')=='序-071-补' for l in c['lines']):raise RuntimeError('Already applied')
 (work/'before.json').write_bytes(p.read_bytes());audio=ROOT/'chapters'/c['audio'];(work/'before.wav').write_bytes(audio.read_bytes())
 with wave.open(str(audio),'rb') as r:params=r.getparams();data=r.readframes(r.getnframes())
 pieces={l['scriptId']:data[round(l['start']*24000)*2:round(l['end']*24000)*2] for l in c['lines']}
 removed={'序-012','序-033','序-036'};c['lines']=[l for l in c['lines'] if l['scriptId'] not in removed]
 goddess=next(l for l in c['lines'] if l['scriptId']=='序-034');assert goddess['text'].startswith('Yes. ');goddess['text']=goddess['text'][5:]
 if goddess['translation'].startswith('是。'):goddess['translation']=goddess['translation'][2:]
 next(l for l in c['lines'] if l['scriptId']=='序-058')['translation']=''
 pos=next(i for i,l in enumerate(c['lines']) if l.get('cgBefore')=='cg06')
 lead=copy.deepcopy(c['lines'][pos-1]);lead.update(scriptId='序-071-补',speaker='陆川',actor='luchuan',voiceRole='陆川',text="Come on. Through this door. There is nothing to be afraid of.",translation='来吧，穿过这扇门，没什么好怕的。',expression='serious',hidePortraits=False,portraitIds=['luchuan']);lead.pop('cgBefore',None);c['lines'].insert(pos,lead)
 voices=json.loads((ROOT/'chapters/audio/tavern/voices.json').read_text(encoding='utf-8'))
 async def render(l):
  name=l['scriptId'];mp3=work/(name+'.mp3');wav=work/(name+'.wav')
  await edge_tts.Communicate(l['text'],**voices[l['voiceRole']],proxy=urllib.request.getproxies().get('https')).save(str(mp3))
  subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'),'-hide_banner','-loglevel','error','-y','-i',str(mp3),'-ar','24000','-ac','1','-c:a','pcm_s16le',str(wav)],check=True)
  with wave.open(str(wav),'rb') as r:pieces[name]=r.readframes(r.getnframes())
 await asyncio.gather(render(goddess),render(lead))
 frames=0;target=work/'updated.wav'
 with wave.open(str(target),'wb') as w:
  w.setparams(params)
  for l in c['lines']:
   clip=pieces[l['scriptId']];l['start']=round(frames/24000,4);w.writeframes(clip);frames+=len(clip)//2;l['end']=round(frames/24000,4);w.writeframes(bytes(12000));frames+=6000
 audio.write_bytes(target.read_bytes());p.write_text(json.dumps(c,ensure_ascii=False,indent=2),encoding='utf-8')
 print('Updated122 cues: removed3 cues and audio, regenerated goddess without Yes, English-only spell and new portal lead-in.')
asyncio.run(main())
