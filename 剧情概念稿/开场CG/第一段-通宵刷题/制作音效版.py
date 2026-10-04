from pathlib import Path
import subprocess
import wave
import numpy as np

folder = Path(__file__).resolve().parent
root = folder.parents[2]
sr = 44100
rng = np.random.default_rng(349)
t = np.arange(8 * sr) / sr
# A soft fan noise bed, without pitched music or speech.
noise = rng.normal(0, 1, (len(t), 2))
for channel in range(2):
    noise[:, channel] = np.convolve(noise[:, channel], np.ones(11) / 11, mode='same')
fade = np.minimum(t / .4, 1) * np.minimum((8 - t) / .5, 1)
mix = noise * .022 * fade[:, None]
with wave.open(str(folder / '深夜环境声.wav'), 'rb') as w:
    assert w.getframerate() == sr and w.getnchannels() == 2
    effects = np.frombuffer(w.readframes(w.getnframes()), dtype='<i2').reshape(-1, 2) / 32768
mix += effects * 2.5
with wave.open(str(folder / '纯音效混音-v3.wav'), 'wb') as w:
    w.setnchannels(2)
    w.setsampwidth(2)
    w.setframerate(sr)
    w.writeframes((np.clip(mix, -.95, .95) * 32767).astype('<i2').tobytes())
subprocess.run([
    str(root / 'tools/ffmpeg/ffmpeg.exe'), '-y', '-loglevel', 'error',
    '-i', str(folder / '第一段CG-通宵刷题-v1.mp4'),
    '-i', str(folder / '纯音效混音-v3.wav'),
    '-map', '0:v:0', '-map', '1:a:0', '-c:v', 'copy', '-c:a', 'aac',
    '-b:a', '192k', '-t', '8', '-movflags', '+faststart',
    str(folder / '第一段CG-通宵刷题-v3-纯音效版.mp4')
], check=True)
print('Created 8-second effects-only preview: fan noise, clock and keyboard; no voice or music.')
