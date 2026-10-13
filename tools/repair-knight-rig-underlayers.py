"""Install generated occlusion repair only BEHIND opaque original sword/arms.

The approved visible pixels and bind-pose silhouette are preserved. Run after
prepare-knight-rig.py. Use --underpainting for a newly generated repair source.
"""
import argparse
import json
from pathlib import Path
import numpy as np
from PIL import Image

root=Path(__file__).resolve().parents[1]
folder=root/'assets'/'characters'/'red-knight-rig'
parser=argparse.ArgumentParser()
parser.add_argument('--underpainting',default=str(folder/'underpainting-source.png'))
args=parser.parse_args()
source=Image.open(args.underpainting).convert('RGBA')
source.save(folder/'underpainting-source.png')
patch=source.resize((200,272),Image.Resampling.NEAREST)
patch.save(folder/'underpainting-aligned.png')
rig=json.loads((folder/'rig.json').read_text(encoding='utf-8'))
rig['layers']=[layer for layer in rig['layers'] if not layer['name'].endswith('_under')]
owner=np.asarray(Image.open(folder/'part-mask.png').resize((200,272),Image.Resampling.NEAREST))-1
rest=np.asarray(Image.open(folder/'rest.png'))
# Parts 11/13/16/17 are the two forearms and two hands; part 15 is the sword.
hidden=np.isin(owner,[11,13,15,16,17]) & (rest[:,:,3]==255)
pixels=np.asarray(patch)
regions={
 'cape':(140,125,178,227),
 'far_thigh':(148,145,178,192),
 'far_shin':(149,185,174,239),
 'far_foot':(146,226,174,267),
 'pelvis':(140,120,177,187),
 'torso':(111,87,179,126),
}
for i,layer in enumerate(sorted(rig['layers'],key=lambda l:l['z'])):layer['z']=i*10
for name,(x0,y0,x1,y1) in regions.items():
 mask=np.zeros((272,200),bool);mask[y0:y1,x0:x1]=hidden[y0:y1,x0:x1]
 a=pixels.copy();a[~mask]=0
 file=name+'_under.png';Image.fromarray(a).save(folder/file)
 base=next(layer for layer in rig['layers'] if layer['name']==name)
 rig['layers'].append({'name':name+'_under','bone':base['bone'],'image':file,'variants':[],
                       'skin':base['skin'],'z':base['z']-1})
rig['layers'].sort(key=lambda layer:layer['z'])
rig['notes']='Measured original cutout + six generated hidden-surface repair layers. Three-quarter view; extreme rotations still need additional views.'
(folder/'rig.json').write_text(json.dumps(rig,ensure_ascii=False,indent=2),encoding='utf-8')
print('Installed 6 hidden repair layers; visible source pixels retained.')
