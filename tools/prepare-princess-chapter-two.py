"""Align known lines with local ASR timestamps; split only between paragraphs."""
import difflib
import hashlib
import json
import re
import shutil
import subprocess
import sys
import wave
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
source = Path(sys.argv[1])
out = ROOT / 'chapters/audio/tavern/chapter-two/lyse-minimax'
out.mkdir(parents=True, exist_ok=True)
chapter = json.loads((ROOT / 'chapters/12-tavern-01-02.json').read_text(encoding='utf-8-sig'))
lines = [line for line in chapter['lines'] if line['speaker'] == '莉瑟']
words = json.loads((ROOT / '.validation/princess-words.json').read_text(encoding='utf-8'))
tokenize = lambda text: re.findall(r'[a-z0-9]+', text.lower())
actual, token_words = [], []
for index, word in enumerate(words):
    tokens = tokenize(word['word'])
    actual.extend(tokens)
    token_words.extend([index] * len(tokens))
expected, ranges = [], []
for line in lines:
    start = len(expected)
    expected.extend(tokenize(line['text']))
    ranges.append((start, len(expected)))
matcher = difflib.SequenceMatcher(None, expected, actual, autojunk=False)
mapping = {}
for a, b, size in matcher.get_matching_blocks():
    mapping.update({a + n: b + n for n in range(size)})
spans = []
for line, (a, b) in zip(lines, ranges):
    matched = [mapping[n] for n in range(a, b) if n in mapping]
    ratio = len(matched) / (b - a)
    if ratio < .85 or a not in mapping or b - 1 not in mapping:
        raise SystemExit('Manual alignment required for: ' + line['text'])
    start, end = words[token_words[mapping[a]]]['start'], words[token_words[mapping[b-1]]]['end']
    spans.append((start, end))
    print(f'{start:.2f}-{end:.2f} match={ratio:.2f}: {line["text"]}')
ff = ROOT / 'tools/ffmpeg/ffmpeg.exe'
decoded = ROOT / '.validation/princess-normalized.wav'
subprocess.run([str(ff), '-nostdin', '-loglevel', 'error', '-y', '-i', str(source),
                '-af', 'loudnorm=I=-19:TP=-2:LRA=7', '-ar', '24000', '-ac', '1', '-c:a', 'pcm_s16le', str(decoded)], check=True)
with wave.open(str(decoded), 'rb') as reader:
    pcm = reader.readframes(reader.getnframes())
duration = len(pcm) / 48000
cuts = [0] + [(spans[n][1] + spans[n+1][0]) / 2 for n in range(len(spans)-1)] + [duration]
manifest = {'voice_id': 'Chinese (Mandarin)_Warm_Girl', 'language': 'English',
            'source_file': 'source.mp3', 'source_sha256': hashlib.sha256(source.read_bytes()).hexdigest(),
            'alignment': 'Local Whisper base.en word timestamps checked against chapter text', 'lines': []}
for n, line in enumerate(lines):
    start, end = cuts[n], cuts[n+1]
    assert end > start and start <= spans[n][0] and end >= spans[n][1]
    filename = f'{n+1:02}.wav'
    chunk = pcm[round(start*24000)*2:round(end*24000)*2]
    with wave.open(str(out / filename), 'wb') as writer:
        writer.setparams((1, 2, 24000, 0, 'NONE', 'not compressed'))
        writer.writeframes(chunk)
    manifest['lines'].append({'file': filename, 'text': line['text'], 'source_start': start, 'source_end': end})
shutil.copy2(source, out / 'source.mp3')
(out / 'manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
