"""Insert two voiced CG cues without re-recording retained dialogue (idempotent)."""
from pathlib import Path
import asyncio, copy, json, sys, subprocess, urllib.request, wave
ROOT=Path(__file__).resolve().parent.parent
sys.path.insert(0,str(ROOT/'.validation/tts-deps'))
import edge_tts
P=ROOT/'chapters/10-tavern-prologue.json'
WORK=ROOT/'.validation/fireside-20261014'
WORK.mkdir(parents=True,exist_ok=True)
async def main():
 c=json.loads(P.read_text(encoding='utf-8-sig'))
 if any(l.get('scriptId')=='fireside-arrival-cue' for l in c['lines']):
  print('Already installed');return
 voices=json.loads((ROOT/'chapters/audio/tavern/voices.json').read_text(encoding='utf-8-sig'))
 audio=ROOT/'chapters'/c['audio']
 (WORK/'before.json').write_bytes(P.read_bytes())
 (WORK/'before.wav').write_bytes(audio.read_bytes())
 with wave.open(str(audio),'rb') as w:params=w.getparams();data=w.readframes(w.getnframes())
 assert (params.nchannels,params.sampwidth,params.framerate)==(1,2,24000)
 specs=[('cg04','fireside-arrival-cue','陆川','luchuan','Hmm? It sounds as though someone is approaching outside... What a lively night.','嗯？外面好像有谁的脚步声正在靠近……今晚可真热闹啊。'),('cg05','fireside-kindling-cue','艾莉娅','aelia','All right, let us get moving. You clear away that pile of broken wood, and I will light a fire.','好了，现在动起来吧。你去把那堆破木头收拾了，我来生个火。')]
 for cg,sid,speaker,actor,en,zh in specs:
  mp3=WORK/(sid+'.mp3');clip=WORK/(sid+'.wav')
  await edge_tts.Communicate(en,**voices[speaker],proxy=urllib.request.getproxies().get('https')).save(str(mp3))
  subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'),'-hide_banner','-loglevel','error','-y','-i',str(mp3),'-ar','24000','-ac','1','-c:a','pcm_s16le',str(clip)],check=True)
  with wave.open(str(clip),'rb') as w:extra=w.readframes(w.getnframes())
  extra+=bytes(12000);delta=len(extra)/48000
  where=next(i for i,l in enumerate(c['lines']) if l.get('cgBefore')==cg)
  target=c['lines'][where];start=target['start'];cut=round(start*24000)*2
  l=copy.deepcopy(c['lines'][where-1]);l.update(scriptId=sid,speaker=speaker,actor=actor,voiceRole=speaker,text=en,translation=zh,expression='neutral',hidePortraits=False,portraitIds=[actor],start=start,end=round(start+delta-.25,4));l.pop('cgBefore',None)
  for old in c['lines'][where:]:
   old['start']=round(old['start']+delta,4);old['end']=round(old['end']+delta,4)
  c['lines'].insert(where,l);data=data[:cut]+extra+data[cut:]
 morning=next(l for l in c['lines'] if l['scriptId']=='序-116');morning['cgBefore']='cg07'
 with wave.open(str(WORK/'updated.wav'),'wb') as w:w.setparams(params);w.writeframes(data)
 audio.write_bytes((WORK/'updated.wav').read_bytes())
 P.write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 print('Installed two matching character voices and cg07; retained every original cue.')
asyncio.run(main())
