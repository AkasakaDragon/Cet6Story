import json,wave
from pathlib import Path
ROOT=Path(__file__).resolve().parent.parent
old=json.loads((ROOT/'.git/audio-backups/opening-meeting/16-tavern-01-06.json').read_text(encoding='utf-8-sig'))
new=json.loads((ROOT/'chapters/16-tavern-01-06.json').read_text(encoding='utf-8-sig'))
def pcm(path):
 with wave.open(str(path),'rb') as r:
  assert r.getparams()[:3]==(1,2,24000)
  return r.readframes(r.getnframes())
a=pcm(ROOT/'.git/audio-backups/opening-meeting/story.wav');b=pcm(ROOT/'chapters/audio/tavern/chapter-six/story.wav')
assert len(new['lines'])==len(old['lines'])+12
for i,l in enumerate(old['lines']):
 n=new['lines'][i if i<6 else i+12]
 assert {k:v for k,v in n.items() if k not in ('start','end')}=={k:v for k,v in l.items() if k not in ('start','end')}
 assert a[round(l['start']*24000)*2:round(l['end']*24000)*2]==b[round(n['start']*24000)*2:round(n['end']*24000)*2]
for i,q in enumerate(old['questions']):
 n=new['questions'][i]
 assert {k:v for k,v in n.items() if k!='afterLine'}=={k:v for k,v in q.items() if k!='afterLine'}
 assert n['afterLine']==q['afterLine']+(12 if q['afterLine']>=6 else 0)
assert new['ending']==old['ending'] and new['decisions']==old['decisions']
for i,l in enumerate(new['lines']):
 assert l['end']>l['start']>=0
 if i:assert l['start']>=new['lines'][i-1]['end']
 assert (ROOT/'chapters'/l['scene']).is_file()
 assert l.get('hidePortraits',False)==(6<=i<18)
print('PASS 12 inserted lines, all 30 original subtitles/PCM identical, 4 question triggers shifted correctly, ending unchanged, valid audio/CG')
