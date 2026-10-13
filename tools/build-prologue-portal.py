"""Animate the existing gate plates: walking camera, hinged doors, entry flash."""
from pathlib import Path
from PIL import Image, ImageDraw
import math, subprocess, wave, json
ROOT=Path(__file__).resolve().parent.parent
OUT=ROOT/'assets/story/prologue-v2'
FF=ROOT/'tools/ffmpeg/ffmpeg.exe'
closed=Image.open(OUT/'boundary.png').convert('RGB')
# The original background coordinate system preserves the established stone gate.
aperture=[(822,528),(822,221),(840,171),(882,137),(923,121),(965,138),(1003,174),(1023,221),(1023,528)]
mask=Image.new('L',closed.size);ImageDraw.Draw(mask).polygon(aperture,fill=255)
opened=Image.open(OUT/'portal-open.png').convert('RGB').resize(closed.size,Image.Resampling.NEAREST)
plate=Image.composite(opened,closed,mask)
leaves=[]
for box in [(821,120,924,530),(924,120,1025,530)]:
 leaf=closed.crop(box).convert('RGBA');leaf.putalpha(mask.crop(box));leaves.append((box,leaf))
arrival=Image.open(OUT/'outside.png').convert('RGB')
arrival=arrival.crop((316,0,1293,977)).resize((800,800),Image.Resampling.NEAREST)
cmd=[str(FF),'-hide_banner','-loglevel','error','-y','-f','rawvideo','-pix_fmt','rgb24','-s','800x800','-r','24','-i','pipe:0','-an','-c:v','libx264','-preset','fast','-crf','16','-pix_fmt','yuv420p',str(OUT/'cg06.mp4')]
process=subprocess.Popen(cmd,stdin=subprocess.PIPE)
def camera(im,zoom,t,bob=True):
 span=977/zoom
 sway=math.sin(t*math.pi*1.2)*4 if bob else 0
 rise=math.sin(t*math.pi*2.4)*3 if bob else 0
 cx=923+sway;cy=450-100*min(1,(zoom-1)/1.2)+rise
 cy=max(span/2,min(im.height-span/2,cy))
 box=(round(cx-span/2),round(cy-span/2),round(cx+span/2),round(cy+span/2))
 return im.crop(box).resize((800,800),Image.Resampling.NEAREST)
try:
 for f in range(336):
  t=f/24
  if t<11.5:
   world=plate.copy().convert('RGBA')
   fraction=max(0,min(1,(t-6)/4));ease=fraction*fraction*(3-2*fraction)
   for n,(box,leaf) in enumerate(leaves):
    width=max(1,round(leaf.width*math.cos(ease*math.pi/2)))
    if fraction<1:
     narrow=leaf.resize((width,leaf.height),Image.Resampling.NEAREST)
     x=box[0] if n==0 else box[2]-width
     world.alpha_composite(narrow,(x,box[1]))
   zoom=1+1.2*min(1,t/6)**1.3
   if t>10:zoom+=((t-10)/1.5)**2*3.5
   frame=camera(world.convert('RGB'),zoom,t)
   flash=max(0,min(1,(t-10.6)/.9))
   if t>10.6:flash=min(1,flash+.12*(.5+.5*math.sin(t*40)))
   if flash:frame=Image.blend(frame,Image.new('RGB',(800,800),'white'),flash)
  else:
   frame=arrival.copy();white=max(0,1-(t-11.5)/.7)
   if white:frame=Image.blend(frame,Image.new('RGB',(800,800),'white'),white)
  if f==0:frame.save(OUT/'cg06.png')
  if f in [120,216,264,300]:frame.save(OUT/('portal-frame-'+str(f)+'.png'))
  process.stdin.write(frame.tobytes())
finally:process.stdin.close()
if process.wait()!=0:raise RuntimeError('Portal encoder failed')
with wave.open(str(OUT/'cg06.wav'),'wb') as w:
 w.setnchannels(1);w.setsampwidth(2);w.setframerate(24000);w.writeframes(bytes(24000*2*14))
p=ROOT/'chapters/10-tavern-prologue.json';chapter=json.loads(p.read_text(encoding='utf-8-sig'));chapter['lines'][49]['cgBefore']='cg06';p.write_text(json.dumps(chapter,ensure_ascii=False,indent=2),encoding='utf-8')
print('Built14s CG06:6s walking,4s hinged opening,1.5s entry flash,2.5s arrival.')
