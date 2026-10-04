from pathlib import Path
import subprocess
import wave
import numpy as np

folder = Path(__file__).resolve().parent
root = folder.parents[2]
sr = 44100
rng = np.random.default_rng(350)
t = np.arange(8 * sr) / sr
noise = rng.normal(0, 1, (len(t), 2))
for channel in range(2):
    noise[:, channel] = np.convolve(noise[:, channel], np.ones(11) / 11, mode='same')
fade = np.minimum(t / .4, 1) * np.minimum((8 - t) / .5, 1)
mix = noise * .022 * fade[:, None]

# One soft two-tone cue when each sentence appears; no keyboard or speech samples.
cue_t = np.arange(int(.22 * sr)) / sr
envelope = np.minimum(cue_t / .008, 1) * np.exp(-cue_t * 22) * np.minimum((.22 - cue_t) / .04, 1)
cue = (.075 * np.sin(2 * np.pi * 660 * cue_t) + .025 * np.sin(2 * np.pi * 990 * cue_t)) * envelope
for start in (1.5, 4.8):
    at = int(start * sr)
    mix[at:at + len(cue)] += cue[:, None]

for filename, samples in [('字幕出现提示音.wav', np.column_stack((cue, cue))), ('字幕提示混音-v4.wav', mix)]:
    with wave.open(str(folder / filename), 'wb') as w:
        w.setnchannels(2)
        w.setsampwidth(2)
        w.setframerate(sr)
        w.writeframes((np.clip(samples, -.95, .95) * 32767).astype('<i2').tobytes())

subprocess.run([
    str(root / 'tools/ffmpeg/ffmpeg.exe'), '-y', '-loglevel', 'error',
    '-i', str(folder / '第一段CG-通宵刷题-v1.mp4'),
    '-i', str(folder / '字幕提示混音-v4.wav'),
    '-map', '0:v:0', '-map', '1:a:0', '-c:v', 'copy', '-c:a', 'aac',
    '-b:a', '192k', '-t', '8', '-movflags', '+faststart',
    str(folder / '第一段CG-通宵刷题-v4-字幕提示音版.mp4')
], check=True)
print('Created v4: subtitle cues at 1.5 and 4.8 seconds, soft fan ambience, no keyboard or voice.')
