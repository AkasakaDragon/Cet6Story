"""Package renderer-exported PNGs as GIF previews; no image synthesis."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

root=Path(__file__).resolve().parents[1]
out=root/'output'/'knight-rig'
for clip,duration in [('idle',100),('attack',50),('hit',30)]:
    frames=[Image.open(p).convert('RGB') for p in sorted((out/clip).glob('frame-*.png'))]
    contact=Image.new('RGB',(800*6,680*4))
    for i,f in enumerate(frames):contact.paste(f,((i%6)*800,(i//6)*680))
    palette=contact.quantize(colors=256,method=Image.Quantize.MEDIANCUT)
    quantized=[f.quantize(palette=palette,dither=Image.Dither.NONE) for f in frames]
    quantized[0].save(out/(clip+'.gif'),save_all=True,append_images=quantized[1:],duration=duration,loop=0,disposal=2,optimize=False)
    check=Image.open(out/(clip+'.gif'))
    assert check.n_frames==24
    print(clip,check.n_frames,'frames')

# Compact side-by-side view with matching normalized phases.
font=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',20)
frames=[]
for i in range(24):
    image=Image.new('RGB',(960,310),(17,40,38))
    draw=ImageDraw.Draw(image)
    for k,(clip,label) in enumerate([('idle','待机呼吸'),('attack','双手挥剑'),('hit','受击后仰')]):
        f=Image.open(out/clip/f'frame-{i:02}.png').resize((320,272),Image.Resampling.NEAREST)
        image.paste(f,(k*320,36))
        draw.text((k*320+20,5),label,font=font,fill=(246,201,119))
    frames.append(image)
frames[0].save(out/'actions-overview.gif',save_all=True,append_images=frames[1:],duration=80,loop=0,disposal=2)
frames[7].save(out/'actions-overview.png')
