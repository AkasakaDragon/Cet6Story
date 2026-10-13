"""Measure/cut the approved sprite into editable layers; never redraw its pixels.

Usage: python tools/prepare-knight-rig.py --source path/to/sheet.png
Requires Pillow + numpy. Coordinates below are authoring coordinates (400 x 544).
"""
import argparse
import json
from pathlib import Path
from collections import deque
import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'assets' / 'characters' / 'red-knight-rig'
W, H = 400, 544

def largest_component(image):
    a = np.asarray(image).copy()
    mask = a[:, :, 3] >= 128
    seen = np.zeros(mask.shape, bool)
    largest = []
    for y, x in zip(*np.where(mask)):
        if seen[y, x]:
            continue
        q = deque([(y, x)])
        seen[y, x] = True
        cells = []
        while q:
            yy, xx = q.popleft()
            cells.append((yy, xx))
            for ny, nx in ((yy-1,xx),(yy+1,xx),(yy,xx-1),(yy,xx+1)):
                if 0 <= ny < H and 0 <= nx < W and mask[ny,nx] and not seen[ny,nx]:
                    seen[ny,nx] = True
                    q.append((ny,nx))
        if len(cells) > len(largest):
            largest = cells
    keep = np.zeros(mask.shape, bool)
    for y,x in largest:
        keep[y,x] = True
    a[~keep] = 0
    # The supplied sheet has near-opaque (mostly 252/253) interiors. Quantize
    # alpha to a binary pixel mask to avoid translucent seams in a cutout rig.
    a[keep,3] = 255
    return Image.fromarray(a)

def polygon(points):
    mask = Image.new('L', (W,H))
    ImageDraw.Draw(mask).polygon(points, fill=255)
    return np.asarray(mask) != 0

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--source', required=True)
    args = parser.parse_args()
    OUT.mkdir(parents=True, exist_ok=True)
    sheet = Image.open(args.source).convert('RGBA')
    if sheet.size != (1448,1086):
        raise ValueError('This measured rig expects the supplied 1448 x 1086 sheet.')
    sheet.save(OUT/'source-sheet.png')
    frames = []
    for i in range(8):
        x = (0,350,700,1080)[i%4]
        y = (0,544)[i//4]
        f = largest_component(sheet.crop((x,y,x+W,y+H)))
        frames.append(f)
    # Register all frames against the stable sword/arm pixels, not blowing hair.
    base = np.asarray(frames[0]).astype(np.float32)
    registrations = []
    for i in range(8):
        best = (float('inf'),0,0)
        a = np.asarray(frames[i]).astype(np.float32)
        for dy in range(-8,9):
            for dx in range(-18,19):
                ref = base[270:500,302:329]
                sample = a[270+dy:500+dy,302+dx:329+dx]
                score = float(np.mean((sample-ref)**2))
                if score < best[0]:
                    best = (score,dx,dy)
        canvas = Image.new('RGBA',(W,H))
        canvas.alpha_composite(frames[i],(-best[1],-best[2]))
        frames[i] = canvas
        registrations.append({'frame':i,'dx':-best[1],'dy':-best[2]})

    # Last wins. Masks partition the actual drawn pixels into articulated parts.
    # Small seams on the cape belong to the cape; the original torso does not
    # secretly retain copies of detached arms or sword.
    definitions = [
        ('cape','cape_a0',None),
        ('hair','hair_mid0',[(0,0),(226,0),(240,53),(251,102),(236,145),(233,188),(214,217),(162,244),(80,253),(0,232)]),
        ('far_thigh','far_thigh',[(307,281),(339,285),(352,365),(341,392),(318,387),(304,337)]),
        ('far_shin','far_shin',[(317,375),(347,370),(342,456),(336,470),(304,469),(304,430)]),
        ('far_foot','far_foot',[(296,452),(337,451),(334,491),(368,514),(370,533),(294,533)]),
        ('near_thigh','near_thigh',[(245,281),(280,285),(280,337),(269,387),(226,390),(224,337)]),
        ('near_shin','near_shin',[(226,376),(269,374),(242,457),(238,472),(202,474),(210,418)]),
        ('near_foot','near_foot',[(203,452),(242,452),(238,499),(243,532),(190,538),(193,508)]),
        ('pelvis','pelvis',[(252,215),(336,218),(342,302),(317,327),(311,365),(288,374),(268,330),(237,304),(224,269)]),
        ('torso','chest',[(253,111),(319,111),(342,139),(342,171),(324,194),(330,222),(287,241),(249,229),(234,195),(233,146)]),
        ('far_upper','far_upper',[(319,143),(346,153),(361,177),(349,207),(329,207),(326,177)]),
        ('far_lower','far_lower',[(342,184),(361,183),(374,207),(357,231),(329,227),(317,205),(323,188)]),
        ('near_upper','near_upper',[(218,113),(254,108),(270,134),(262,158),(251,183),(245,206),(225,215),(213,195),(215,170),(206,153)]),
        ('near_lower','near_lower',[(233,187),(253,196),(289,201),(310,207),(306,228),(270,231),(242,224),(222,211)]),
        ('head','head',[(222,0),(346,0),(346,82),(315,103),(291,116),(274,111),(268,159),(257,176),(254,113),(231,106),(216,95),(201,90),(200,77),(217,53)]),
        ('sword','weapon',[(304,230),(325,230),(329,252),(350,252),(352,273),(331,274),(331,515),(323,544),(311,544),(305,518),(307,275),(283,271),(281,254),(301,252)]),
        ('far_hand','far_hand',[(300,185),(323,183),(330,201),(322,210),(302,210)]),
        ('near_hand','near_hand',[(296,207),(325,207),(324,230),(306,235),(291,227)]),
    ]
    owner = np.zeros((H,W),dtype=np.int16)
    for j,(_,_,points) in enumerate(definitions):
        if points:
            owner[polygon(points)] = j
    data = np.asarray(frames[0])
    active = data[:,:,3] > 0
    layers = []
    small = (W//2,H//2)
    for j,(name,bone,_) in enumerate(definitions):
        a = data.copy()
        a[owner != j] = 0
        image = Image.fromarray(a).resize(small,Image.Resampling.NEAREST)
        image.save(OUT/(name+'.png'))
        variants = []
        if name in ('hair','cape'):
            for i,f in enumerate(frames):
                a = np.asarray(f).copy()
                a[owner != j] = 0
                file = name+'-'+str(i)+'.png'
                Image.fromarray(a).resize(small,Image.Resampling.NEAREST).save(OUT/file)
                variants.append(file)
        layers.append({'name':name,'bone':bone,'image':name+'.png','variants':variants,
                       'skin':name if name in ('hair','cape') else '', 'z':j})
    # Exact rest reconstruction is a measurable invariant for the cutout.
    rebuilt = np.zeros_like(data)
    for j in range(len(definitions)):
        rebuilt[owner == j] = data[owner == j]
    assert np.array_equal(data,rebuilt)
    frames[0].resize(small,Image.Resampling.NEAREST).save(OUT/'rest.png')
    for i,f in enumerate(frames):
        f.resize(small,Image.Resampling.NEAREST).save(OUT/('original-'+str(i)+'.png'))
    bones=[]
    def bone(name,parent,x,y,label,group='body'):
        bones.append({'name':name,'parent':parent,'x':x/2,'y':y/2,'label':label,'group':group})
    bone('root','',275,532,'整体')
    bone('pelvis','root',277,255,'骨盆')
    bone('chest','pelvis',284,158,'胸部')
    bone('neck','chest',284,112,'颈部')
    bone('head','neck',285,68,'头部')
    bone('weapon','chest',313,206,'剑与握柄')
    bone('far_upper','chest',331,164,'远侧上臂')
    bone('far_lower','far_upper',351,208,'远侧前臂')
    bone('far_hand','far_lower',313,195,'远侧手')
    bone('near_upper','chest',239,145,'近侧上臂')
    bone('near_lower','near_upper',239,209,'近侧前臂')
    bone('near_hand','near_lower',309,219,'近侧手')
    bone('far_thigh','pelvis',325,302,'远侧大腿')
    bone('far_shin','far_thigh',330,377,'远侧小腿')
    bone('far_foot','far_shin',319,469,'远侧脚')
    bone('near_thigh','pelvis',255,303,'近侧大腿')
    bone('near_shin','near_thigh',245,377,'近侧小腿')
    bone('near_foot','near_shin',221,467,'近侧脚')
    for group,chains in [('hair',[
        ('top',[(227,69),(164,90),(91,116),(39,150)]),
        ('mid',[(235,103),(177,139),(121,179),(70,199)]),
        ('low',[(232,148),(202,189),(160,220),(111,236)])]),
        ('cape',[
        ('a',[(246,190),(211,257),(131,307),(50,303)]),
        ('b',[(271,240),(228,300),(189,359),(173,414)])])]:
        for tag,points in chains:
            parent='head' if group=='hair' else 'chest'
            for k,(x,y) in enumerate(points):
                name=group+'_'+tag+str(k)
                bone(name,parent,x,y,('发束' if group=='hair' else '披风')+tag+' · '+str(k+1),group)
                parent=name
    # Authoring masks are saved for future cleanup of occluded shoulders/hips.
    Image.fromarray((owner+1).astype('uint8')).save(OUT/'part-mask.png')
    clips=[
        {'name':'idle','label':'待机呼吸','duration':2.4,'loop':True,'keys':[
            {'t':0,'chestY':0},{'t':.6,'chestY':-.8},{'t':1.2,'chestY':0},{'t':1.8,'chestY':.4},{'t':2.4,'chestY':0}]},
        {'name':'attack','label':'双手挥剑','duration':1.1,'loop':False,'keys':[
            {'t':0},{'t':.3,'chestAngle':-5,'weaponAngle':-125,'weaponY':-10},
            {'t':.46,'rootX':9,'chestAngle':9,'weaponAngle':-42,'weaponX':4,'weaponY':-4},
            {'t':.54,'rootX':9,'chestAngle':9,'weaponAngle':-42,'weaponX':4,'weaponY':-4},
            {'t':.8,'rootX':3,'chestAngle':3,'weaponAngle':-12},{'t':1.1}]},
        {'name':'hit','label':'受击后仰','duration':.65,'loop':False,'keys':[
            {'t':0},{'t':.1,'rootX':-6,'chestAngle':-10,'headAngle':-5,'weaponAngle':8},
            {'t':.2,'rootX':-6,'chestAngle':-10,'headAngle':-5,'weaponAngle':8},
            {'t':.4,'rootX':-2,'chestAngle':-4},{'t':.65}]}
    ]
    rig={'version':1,'width':small[0],'height':small[1], 'facing':'right',
         'frameSeconds':.12,'bones':bones,'layers':layers,'clips':clips,
         'registrations':registrations,'source':'source-sheet.png',
         'notes':'Measured cutout prototype. Hidden surfaces require hand cleanup for extreme poses.'}
    (OUT/'rig.json').write_text(json.dumps(rig,ensure_ascii=False,indent=2),encoding='utf-8')
    print('Prepared',len(bones),'bones,',len(layers),'layers, 8 aligned wind frames at',OUT)

if __name__=='__main__':main()
