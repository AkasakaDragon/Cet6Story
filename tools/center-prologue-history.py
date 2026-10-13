"""Publish the five centered archive plates and enable a fitted story viewport."""
from pathlib import Path
import json
from PIL import Image
ROOT=Path(__file__).resolve().parent.parent
sources=json.loads((ROOT/'.validation/history-center-sources.json').read_text(encoding='utf-8'))
for entry in sources:
    im=Image.open(entry['path']).convert('RGB').resize((800,800),Image.Resampling.NEAREST)
    for folder in ['assets/story/prologue-v3','chapters/art/prologue-v3']:
        target=ROOT/folder;target.mkdir(parents=True,exist_ok=True)
        im.save(target/('history%02d.png'%entry['i']))
chapter_path=ROOT/'chapters/10-tavern-prologue.json'
c=json.loads(chapter_path.read_text(encoding='utf-8-sig'))
for i,line in enumerate(c['lines']):
    if line.get('scriptId'):
        n=int(line['scriptId'].split('-')[1])
        plate=1 if n<=33 else 2 if n<=36 else 3 if n==37 else 4 if n<=40 else 5
        historical=31<=n<=45
    else:
        historical=29<=i<=36
        plate={29:1,30:2,31:3,32:4}.get(i,5)
    if historical:
        line.update(scene='art/prologue-v3/history%02d.png'%plate,sceneSingle=True,archiveScene=True,hidePortraits=True,portraitIds=[])
chapter_path.write_text(json.dumps(c,ensure_ascii=False,indent=2),encoding='utf-8')
print('Published five native 800x800 centered archive images.')
