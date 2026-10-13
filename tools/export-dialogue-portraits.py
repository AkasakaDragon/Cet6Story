"""Preserve supplied identities and face-only variants on the 500px native grid."""
from pathlib import Path
from PIL import Image
import json
import numpy as np
ROOT=Path(__file__).resolve().parent.parent
SRC=ROOT/'assets/characters/expressions-v1'
OUT=ROOT/'assets/characters/dialogue-v2'
report=[]
for folder,actor in [('01-dagger','dagger'),('02-noya','noya'),('03-lyse','lyse'),('04-mila','mila'),('05-aelia','aelia'),('06-luchuan','luchuan')]:
 (OUT/actor).mkdir(parents=True,exist_ok=True)
 original=Image.open(SRC/'inputs'/(folder+'-original.png')).convert('RGBA')
 latest=OUT/'references'/(actor+'.png')
 supplied=Image.open(latest).convert('RGBA') if latest.exists() else original.copy()
 alpha=original.getchannel('A').point(lambda v:255 if v>=128 else 0)
 bbox=alpha.getbbox();w=round((bbox[2]-bbox[0])*500/(bbox[3]-bbox[1]))
 files=[(SRC/'inputs'/(folder+'-original.png'),'neutral')]+[(p,n) for p,n in zip(sorted((SRC/folder).glob('*.png')),['angry','confused','surprised','sad','serious','happy'])]
 for file,emotion in files:
  variant=Image.open(file).convert('RGBA')
  pixels=np.array(supplied);old=np.array(original);face=np.array(variant)
  changed=np.any(face!=old,axis=2);pixels[changed]=face[changed]
  im=Image.fromarray(pixels);im.putalpha(im.getchannel('A').point(lambda v:255 if v>=128 else 0))
  im=im.crop(bbox).resize((w,500),Image.Resampling.NEAREST)
  canvas=Image.new('RGBA',(500,500));canvas.paste(im,((500-w)//2,0))
  canvas.save(OUT/actor/(emotion+'.png'))
  report.append(dict(actor=actor,expression=emotion,grid=[500,500],subjectBounds=canvas.getchannel('A').getbbox(),source=str(file)))
(OUT/'manifest.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
chapter=ROOT/'chapters/10-tavern-prologue.json'
c=json.loads(chapter.read_text(encoding='utf-8-sig'))
choices={'confused':[20,22,24,26,28,30,33,38,40,42,52,56,59,63,75,79,81,89,96],
 'surprised':[45,50,54,73], 'happy':[47,66,84,86,94,98,100,106,108,110,112],
 'sad':[102], 'angry':[65]}
for i,l in enumerate(c['lines']):
 l['expression']=next((emotion for emotion,indices in choices.items() if i in indices),'serious')
chapter.write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('42 portraits exported. Prologue emotions assigned per line.')
