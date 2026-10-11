"""Use original recording pauses for per-sentence replay stops, retaining caption times."""
import concurrent.futures
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CACHE = ROOT / '.validation/listening-sentence-pauses'


def refine(file):
    CACHE.mkdir(parents=True, exist_ok=True)
    doc = json.loads(file.read_text(encoding='utf-8-sig'))
    audio = ROOT / doc['audio']
    identity = [audio.stat().st_size, audio.stat().st_mtime_ns]
    cache = CACHE / (doc['id'] + '.json')
    cached = json.loads(cache.read_text()) if cache.exists() else {}
    if cached.get('identity') == identity:
        pauses = cached['pauses']
    else:
        result = subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'), '-nostdin', '-hide_banner',
                                 '-i', str(audio), '-af', 'silencedetect=noise=-40dB:d=0.08',
                                 '-f', 'null', '-'], capture_output=True, text=True, check=True,
                                creationflags=subprocess.CREATE_NO_WINDOW)
        pauses = []
        beginning = None
        for kind, value in re.findall(r'silence_(start|end): ([0-9.]+)', result.stderr):
            value = float(value)
            if kind == 'start':
                beginning = value
            elif beginning is not None:
                pauses.append([beginning, value])
                beginning = None
        cache.write_text(json.dumps(dict(identity=identity, pauses=pauses)))
    detected = fallback = 0
    rows = doc['lines']
    for i, row in enumerate(rows):
        end = row['end']
        limit = rows[i+1]['start'] if i+1 < len(rows) else end+1
        if row.get('questionNumber'):
            stop = end  # Already audited against the complete spoken question.
        else:
            candidates = [(a, b) for a, b in pauses
                          if b > end and a >= end-.12 and a <= min(end+1, limit-.025)]
            # Also allow a timestamp already within the pause after its trailing phoneme.
            candidates += [(a, b) for a, b in pauses if a <= end < b and b-end >= .05]
            if candidates:
                a, b = min(candidates, key=lambda p: max(0, p[0]-end))
                stop = min(limit, max(end, a)+min(.10, (b-max(end, a))/2))
                detected += 1
            else:
                stop = min(end+.5, limit)
                fallback += 1
        assert end-.001 <= stop <= limit+.001
        row['playback_end'] = round(stop, 3)
    doc['replay_boundary_source'] = 'original_audio_pauses_with_caption_fallback'
    file.write_text(json.dumps(doc, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    return detected, fallback


if __name__ == '__main__':
    CACHE.mkdir(parents=True, exist_ok=True)
    files = list((ROOT/'assets/tavern/listening/real-exams/cettong').rglob('*.captions.json'))
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        results = list(pool.map(refine, files))
    print('Updated', len(files), 'recordings;', sum(r[0] for r in results),
          'material sentence stops from pauses;', sum(r[1] for r in results), 'fallbacks.')
