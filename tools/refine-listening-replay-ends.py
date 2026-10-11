"""Use original recording pauses for per-sentence replay stops, retaining caption times."""
import concurrent.futures
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CACHE = ROOT / '.validation/listening-sentence-pauses'


def token(text):
    value = ''.join(re.findall(r'[a-z0-9]+', text.lower()))
    if value in ('um', 'uh', 'ah', 'hmm', 'hm', 'mm', 'erm'):
        return '<hesitation>'
    numbers = ('zero','one','two','three','four','five','six','seven','eight','nine','ten','eleven','twelve')
    return str(numbers.index(value)) if value in numbers else value


def word_at(words, row, first):
    tokens = re.findall(r"[a-z0-9]+(?:'[a-z0-9]+)?", row['text'].lower())
    wanted = token(tokens[0 if first else -1])
    timestamp = row['alignment_start' if first else 'alignment_end']
    if first:
        prefix = [token(t) for t in tokens[:3]]
        candidates = []
        for i, word in enumerate(words):
            if not timestamp-.10 <= word['start'] <= timestamp+1.5:
                continue
            heard = [token(w['word']) for w in words[i:i+len(prefix)]]
            # Require the following words too: a nearby repeated "I" is not an anchor.
            # A substituted first word is usable only with two matching following words.
            if heard == prefix or (len(prefix)==3 and heard[1:]==prefix[1:]):
                candidates.append(word)
    else:
        candidates = [w for w in words if token(w['word']) == wanted
                      and abs(w['end']-timestamp) < .10]
    return min(candidates, key=lambda w: abs(w['start' if first else 'end']-timestamp)) if candidates else None


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
    duration = cached.get('duration') if cached.get('identity') == identity else None
    if duration is None:
        metadata = subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'), '-nostdin', '-hide_banner',
                                   '-i', str(audio)], capture_output=True, text=True,
                                  creationflags=subprocess.CREATE_NO_WINDOW)
        match = re.search(r'Duration: (\d+):(\d+):([0-9.]+)', metadata.stderr)
        if not match:
            raise ValueError('Cannot read recording duration: '+str(audio))
        duration = int(match[1])*3600+int(match[2])*60+float(match[3])
        cache.write_text(json.dumps(dict(identity=identity, pauses=pauses, duration=duration)))
    merged = []
    for a, b in pauses:
        if merged and a-merged[-1][1] < .003:
            merged[-1][1] = b
        else:
            merged.append([a, b])
    pauses = merged
    detected = fallback = corrected_starts = corrected_ends = 0
    rows = doc['lines']
    raw = ROOT / '.validation' / ('aligned-asr-'+doc['id']+'.json')
    words = [w for s in json.loads(raw.read_text(encoding='utf-8')) for w in s['words']] if raw.exists() else []
    for row in rows:
        row.setdefault('alignment_start', row['start'])
        row.setdefault('alignment_end', row['end'])
        if words:
            row['start'], row['end'] = row['alignment_start'], row['alignment_end']
        if row.get('questionNumber'):
            continue
        first = word_at(words, row, True)
        # An ASR first word can absorb the preceding speaker's trailing sound and pause.
        # Anchor its onset after an actual pause within that inflated word interval.
        if first:
            gaps = [(a, b) for a, b in pauses if a >= row['alignment_start']-.02
                    and b <= first['end']-.04 and b-a >= .16
                    and b < row['end']-.05]
            if gaps:
                a, b = max(gaps, key=lambda p: p[1]-p[0])
                # Keep quiet consonant onsets as well as the first loud voiced sample.
                onset = max(a+min(.05, (b-a)/4), b-.20)
                row['start'] = round(max(row['start'], onset), 3)
                corrected_starts += 1
        last = word_at(words, row, False)
        if last:
            gaps = [(a, b) for a, b in pauses if a >= last['start']+.10
                    and last['end']-.08 <= b <= last['end']+.02 and b-a >= .16
                    and a > row['start']+.10]
            if gaps:
                a, b = max(gaps, key=lambda p: p[1]-p[0])
                row['end'] = round(min(row['end'], a), 3)
                corrected_ends += 1
    for i, row in enumerate(rows):
        end = row['end']
        limit = rows[i+1]['start'] if i+1 < len(rows) else duration
        if row.get('questionNumber'):
            stop = end  # Already audited against the complete spoken question.
            row['replay_boundary_kind'] = 'verified_question'
        else:
            candidates = [(a, b) for a, b in pauses
                          if b > end and a >= end-.12 and a <= min(end+1, limit-.025)]
            # Also allow a timestamp already within the pause after its trailing phoneme.
            candidates += [(a, b) for a, b in pauses if a <= end < b and b-end >= .05]
            if candidates:
                a, b = min(candidates, key=lambda p: max(0, p[0]-end))
                stop = min(limit, max(end, a)+min(.10, (b-max(end, a))/2))
                detected += 1
                row['replay_boundary_kind'] = 'audio_pause'
            else:
                stop = min(end+.5, limit)
                fallback += 1
                row['replay_boundary_kind'] = 'no_clear_pause'
        assert end-.001 <= stop <= limit+.001
        row['playback_end'] = round(stop, 3)
    doc['replay_boundary_source'] = 'original_audio_pauses_and_word_onset_audit'
    doc['audio_duration'] = duration
    doc['replay_boundary_audit'] = dict(pause_ends=detected, no_clear_pause=fallback,
                                      corrected_onsets=corrected_starts, corrected_ends=corrected_ends)
    file.write_text(json.dumps(doc, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    return detected, fallback, corrected_starts, corrected_ends


if __name__ == '__main__':
    CACHE.mkdir(parents=True, exist_ok=True)
    files = list((ROOT/'assets/tavern/listening/real-exams/cettong').rglob('*.captions.json'))
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        results = list(pool.map(refine, files))
    print('Updated', len(files), 'recordings;', sum(r[0] for r in results),
          'material sentence stops from pauses;', sum(r[1] for r in results), 'fallbacks;',
          sum(r[2] for r in results), 'corrected onsets;', sum(r[3] for r in results), 'corrected ends.')
