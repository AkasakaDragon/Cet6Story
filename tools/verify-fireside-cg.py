"""Verify retained text, PCM bytes, cue ordering and decodable CG assets."""
from pathlib import Path
import json, subprocess, wave
ROOT=Path(__file__).resolve().parent.parent
WORK=ROOT/'.validation/fireside-20261014'
c=json.loads((ROOT/'chapters/10-tavern-prologue.json').read_text(encoding='utf-8-sig'))
old=json.loads((WORK/'before.json').read_text(encoding='utf-8-sig'))
newids={'fireside-arrival-cue','fireside-kindling-cue'}
retained=[l for l in c['lines'] if l['scriptId'] not in newids]
assert len(c['lines'])==len(old['lines'])+2
for a,b in zip(old['lines'],retained):
 for key in a:
  if key not in {'start','end','cgBefore'}:assert a[key]==b[key],(a['scriptId'],key)
 assert a.get('cgBefore')==b.get('cgBefore') or (a['scriptId']=='序-116' and b['cgBefore']=='cg07')
def pcm(p):
 with wave.open(str(p),'rb') as w:return w.readframes(w.getnframes())
before=pcm(WORK/'before.wav');after=pcm(ROOT/'chapters'/c['audio'])
for a,b in zip(old['lines'],retained):
 n=round((a['end']-a['start'])*24000)*2
 x=round(a['start']*24000)*2;y=round(b['start']*24000)*2
 assert before[x:x+n]==after[y:y+n],(a['scriptId'],'PCM changed')
last=0
for i,l in enumerate(c['lines']):
 assert l['start']>=last and l['end']>l['start'];last=l['end']
 assert (ROOT/'chapters'/l['scene']).exists()
 if l['scriptId'] in newids:assert sum(x['scriptId']==l['scriptId'] for x in c['lines'])==1
 if l.get('cgBefore'):
  for ext in ['png','mp4','wav']:assert (ROOT/'assets/story/prologue-v2'/(l['cgBefore']+'.'+ext)).exists()
for sid,cg in [('fireside-arrival-cue','cg04'),('fireside-kindling-cue','cg05')]:
 i=next(i for i,l in enumerate(c['lines']) if l['scriptId']==sid)
 assert not c['lines'][i].get('cgBefore') and c['lines'][i+1]['cgBefore']==cg
i=next(i for i,l in enumerate(c['lines']) if l.get('cgBefore')=='cg07')
assert c['lines'][i-1]['translation']=='不用今晚都想清。等水烧开，先喝一点，再睡。'
assert c['lines'][i]['scriptId']=='序-116'
assert len(after)/48000>=last
subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'),'-v','error','-i',str(ROOT/'assets/story/prologue-v2/cg07.mp4'),'-f','null','-'],check=True)
print('PASS: two unique voiced lead-ins; all %d retained cues and PCM bytes unchanged; ordered timestamps; sleep CG between bedtime and morning; CG fully decodes.'%len(retained))
