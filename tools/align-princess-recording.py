"""Locally transcribe the user-provided recording for dialogue alignment."""
import json
import sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / '.validation/asr-deps'))
from faster_whisper import WhisperModel
model = WhisperModel('base.en', device='cpu', compute_type='int8', download_root=str(ROOT / '.validation/asr-models'))
segments, info = model.transcribe(sys.argv[1], language='en', beam_size=5, word_timestamps=True)
result = []
for segment in segments:
    result.extend({'word': w.word, 'start': w.start, 'end': w.end} for w in segment.words)
    print(f'{segment.start:.2f}-{segment.end:.2f}: {segment.text}', flush=True)
(ROOT / '.validation/princess-words.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
