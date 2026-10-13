"""Export the requested native pixel grids; preserve generated originals."""
from pathlib import Path
from PIL import Image
import json, shutil, subprocess, wave
ROOT=Path(__file__).resolve().parent.parent
GEN=Path('C:/Users/Administrator/.codex/generated_images/01a1305b-7c1e-7420-ba38-494aaf4de506')
OUT=ROOT/'assets/story/prologue-v2'
ART=ROOT/'chapters/art/prologue-v2'
FF=ROOT/'tools/ffmpeg/ffmpeg.exe'
sources={
 'cg01':'exec-5e90035c-87a1-4c0a-bde9-b2dde3301d53.png',
 'cg02':'exec-3ae14dc8-62dc-44fd-8ac3-e31f14a8b127.png',
 'cg03':'exec-bef67200-49f2-4870-90db-c177f1f114d6.png',
 'cg04':'exec-c99002f3-e0b4-4e2a-9fcb-f0b3852e8aea.png',
 'cg05':'exec-4be207f4-5391-42db-919c-87673b99bdd8.png',
 'dorm':'exec-f383f03b-b4f3-4a3c-a714-298f9692f688.png',
 'boundary':'exec-da40ab07-66ff-4aa4-99bb-f5ffece765bf.png',
 'outside':'exec-74efe17e-9b4f-4d3d-bf54-a2f9f7254402.png',
 'ruined':'exec-41297213-42e8-4072-a362-2aa63dcd1c9c.png',
 'patched':'exec-d578e42c-2ffa-43ec-a8cd-af6201557b16.png',
 'morning':'exec-bbb412ec-84a7-4c4e-b3d0-942cb62879dc.png',
}
manifest={}
for name,file in sources.items():
 im=Image.open(GEN/file).convert('RGB')
 size=(800,800) if name.startswith('cg') else (1610,977)
 im=im.resize(size,Image.Resampling.NEAREST)
 target=OUT/(name+'.png');im.save(target)
 if not name.startswith('cg'):shutil.copy2(target,ART/target.name)
 manifest[name]={'source':str(GEN/file),'width':size[0],'height':size[1]}
for name,file in [('luchuan',ROOT/'assets/characters/expressions-v1/inputs/06-luchuan-original.png'),('aelia',ROOT/'assets/characters/expressions-v1/inputs/05-aelia-original.png'),('iserya',GEN/'exec-3e492c2e-a9b8-4cbe-86e7-f7250afa4417.png')]:
 im=Image.open(file).convert('RGBA')
 a=im.getchannel('A').point(lambda v:255 if v>=128 else 0)
 im.putalpha(a);im=im.crop(a.getbbox())
 w=max(1,round(im.width*500/im.height));im=im.resize((w,500),Image.Resampling.NEAREST)
 canvas=Image.new('RGBA',(500,500));canvas.paste(im,((500-w)//2,0))
 canvas.save(OUT/(name+'.png'));canvas.save(ART/(name+'.png'))
 manifest[name]={'source':str(file),'canvas':[500,500],'subjectBounds':list(canvas.getchannel('A').getbbox()),'nativeGrid':True,'proportionsPreserved':True}
(OUT/'export-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
print('Exported native CG800, backgrounds1610x977, fullbody height500 portraits.')
