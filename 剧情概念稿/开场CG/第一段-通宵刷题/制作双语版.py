from pathlib import Path
import subprocess
import wave
import numpy as np

folder = Path(__file__).resolve().parent
root = folder.parents[2]
sr = 44100
rng = np.random.default_rng(351)
t = np.arange(8 * sr) / sr
noise = rng.normal(0, 1, (len(t), 2))
for channel in range(2):
    noise[:, channel] = np.convolve(noise[:, channel], np.ones(11) / 11, mode='same')
fade = np.minimum(t / .4, 1) * np.minimum((8 - t) / .5, 1)
mix = noise * .022 * fade[:, None]
with wave.open(str(folder / '白噪声-v5.wav'), 'wb') as w:
    w.setnchannels(2)
    w.setsampwidth(2)
    w.setframerate(sr)
    w.writeframes((mix * 32767).astype('<i2').tobytes())

captions = [
    (1, 1.5, 4.6, '再做一套……', 'Just one more practice test...'),
    (2, 4.8, 7.8, '做完就睡。', "I'll go to bed when I'm done."),
]
font = 'C\\:/Windows/Fonts/msyh.ttc'
vf = "scale=640:360:flags=neighbor,zoompan=z='1+0.055*on/191':x='iw*0.68-iw/zoom*0.68':y='ih*0.53-ih/zoom*0.53':d=192:s=640x360:fps=24,scale=1280:720:flags=neighbor"
# Render from the original uncaptioned still, avoiding old subtitles underneath.
vf += ",drawbox=x=0:y=ih-120:w=iw:h=120:color=black@0.30:t=fill:enable='between(t,1.5,4.6)+between(t,4.8,7.8)'"
for index, begin, end, chinese, english in captions:
    for language, text, size, y, color in [('中文', chinese, 28, 'h-98', 'white'), ('英文', english, 24, 'h-56', '0xE7E9ED')]:
        filename = f'双语{index}-{language}.txt'
        (folder / filename).write_text(text, encoding='utf-8')
        vf += f",drawtext=fontfile='{font}':textfile={filename}:fontsize={size}:fontcolor={color}:borderw=2:bordercolor=black:x=(w-tw)/2:y={y}:enable='between(t,{begin},{end})'"
subprocess.run([
    str(root / 'tools/ffmpeg/ffmpeg.exe'), '-y', '-loglevel', 'error',
    '-i', str(folder / '场景-v1.png'), '-i', str(folder / '白噪声-v5.wav'),
    '-vf', vf, '-t', '8', '-c:v', 'libx264', '-crf', '18',
    '-pix_fmt', 'yuv420p', '-c:a', 'aac', '-b:a', '160k',
    '-movflags', '+faststart', str(folder / '第一段CG-通宵刷题-v5-双语无提示音.mp4')
], cwd=folder, check=True)
print('Created bilingual CG: quiet fan ambience only, no subtitle cues or speech.')
