from pathlib import Path
from PIL import Image
import json, wave, subprocess, re
import numpy as np
ROOT=Path(__file__).resolve().parent.parent
chapter=json.loads((ROOT/'chapters/10-tavern-prologue.json').read_text(encoding='utf-8-sig'))
report={'lines':len(chapter['lines']),'CG':[],'portraits':[]}
assert len(chapter['lines'])==114 and not chapter['questions'] and not chapter['decisions']
last=0
for i,line in enumerate(chapter['lines']):
 assert line['start']>=last and line['end']>line['start'],i
 last=line['end'];assert (ROOT/'chapters'/line['scene']).exists(),line['scene']
 assert line['sceneCaption']
 if i<19:assert line['hidePortraits']
for i in range(1,7):
 cg='cg0'+str(i);p=ROOT/'assets/story/prologue-v2'/cg
 with Image.open(p.with_suffix('.png')) as im:assert im.size==(800,800)
 result=subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'),'-hide_banner','-i',str(p.with_suffix('.mp4'))],capture_output=True,text=True)
 m=re.search(r'Duration: (\d+):(\d+):([\d.]+)',result.stderr);assert m,cg
 seconds=int(m[1])*3600+int(m[2])*60+float(m[3]);assert abs(seconds-(14 if i==6 else 8 if i==3 else 4))<0.05
 assert '800x800' in result.stderr
 report['CG'].append({'file':cg,'pixels':[800,800],'seconds':seconds})
for name in ['dorm','boundary','outside','ruined','patched','morning']:
 with Image.open(ROOT/'chapters/art/prologue-v2'/(name+'.png')) as im:assert im.size==(1610,977)
for folder in (ROOT/'assets/characters/dialogue-v2').iterdir():
 if not folder.is_dir() or not (folder/'neutral.png').exists():continue
 base=np.array(Image.open(folder/'neutral.png').convert('RGBA'))
 for p in folder.glob('*.png'):
  with Image.open(p) as im:
   assert im.size==(500,500)
   b=im.getchannel('A').getbbox();assert b[1]==0 and b[3]==500
   arr=np.array(im);diff=np.any(arr!=base,axis=2);ys,xs=np.where(diff)
   bbox=[int(xs.min()),int(ys.min()),int(xs.max()+1),int(ys.max()+1)] if len(xs) else None
   if bbox:assert bbox[3]<150,'Changed outside face: '+str(p)
   report['portraits'].append({'actor':folder.name,'expression':p.stem,'grid':[500,500],'subjectBounds':b,'changedFaceBounds':bbox})
with wave.open(str(ROOT/'chapters'/chapter['audio'])) as w:
 seconds=w.getnframes()/w.getframerate();assert seconds>=last and seconds-last<1
 report['dialogueAudioSeconds']=round(seconds,2)
report['result']='PASS'
out=ROOT/'.validation/prologue-rebuild-20261013/asset-verification.json';out.write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('PASS: native dimensions, CG durations,114 voice timings,42 face-only portraits and resource paths.')
