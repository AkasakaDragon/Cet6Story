"""Locate every published question in the cached original recording transcript."""
import argparse
import difflib
import json
import re
from pathlib import Path
from listening_sentences import answer_pauses

ROOT = Path(__file__).resolve().parents[1]
FOLDER = ROOT / 'assets/tavern/listening/real-exams/cettong'


def tokens(text):
    return re.findall(r"[a-z0-9]+", text.lower())


def locate(prompt, words):
    heard = [''.join(tokens(w['word'])) for w in words]
    wanted = [re.sub(r'[^a-z0-9]', '', w.lower()) for w in prompt.split()]
    matcher = difflib.SequenceMatcher(None, wanted, heard, autojunk=False)
    seeds = {max(0, b.b-b.a) for b in matcher.get_matching_blocks() if b.size >= 2}
    best = (0, 0, 0)
    for seed in seeds:
        for start in range(max(0, seed-4), min(len(words), seed+5)):
            for length in range(max(3, len(wanted)-6), len(wanted)+7):
                end = min(len(words), start+length)
                wh = ('what','why','how','who','when','where','which')
                if not heard[start] or (heard[start] not in (wanted[0],wanted[0]+'s') and not (heard[start] in wh and wanted[0] in wh)):
                    continue
                if words[end-1]['end']-words[start]['start'] > 18 or any(words[i+1]['start']-words[i]['end'] > 2.5 for i in range(start,end-1)):
                    continue
                score = difflib.SequenceMatcher(None, wanted, heard[start:end], autojunk=False).ratio()
                # ASR often writes compounds/acronyms as separate words.
                score = max(score,difflib.SequenceMatcher(None,''.join(wanted),''.join(heard[start:end]),autojunk=False).ratio())
                if score > best[0]:
                    best = score, start, end
    score, start, end = best
    if not end:
        return dict(score=0, text='', start=0, end=0)
    return dict(score=round(score, 4), text=''.join(w['word'] for w in words[start:end]).strip(),
                start=words[start]['start'], end=words[end-1]['end'])


def audit():
    results = []
    for file in sorted(FOLDER.rglob('*.resources.json')):
        exam = json.loads(file.read_text(encoding='utf-8'))
        raw = ROOT / '.validation' / ('aligned-asr-'+exam['id']+'.json')
        if not raw.exists():
            raise ValueError('Missing original recording transcript: '+exam['id'])
        words = [w for segment in json.loads(raw.read_text(encoding='utf-8')) for w in segment['words'] if w['end'] >= w['start']]
        extra = raw.with_name(raw.stem+'-questions.json')
        extra_segments = json.loads(extra.read_text(encoding='utf-8')) if extra.exists() else []
        retry = raw.with_name(raw.stem+'-questions-retry.json')
        if retry.exists():
            extra_segments += json.loads(retry.read_text(encoding='utf-8'))
        focused = raw.with_name(raw.stem+'-questions-focused.json')
        if focused.exists():
            extra_segments += json.loads(focused.read_text(encoding='utf-8'))
        final = raw.with_name(raw.stem+'-questions-final.json')
        if final.exists():
            extra_segments += json.loads(final.read_text(encoding='utf-8'))
        pause_transcript = raw.with_name(raw.stem+'-questions-pauses.json')
        if pause_transcript.exists():
            extra_segments += json.loads(pause_transcript.read_text(encoding='utf-8'))
        ordinal = raw.with_name(raw.stem+'-questions-ordinal.json')
        if ordinal.exists():
            extra_segments += json.loads(ordinal.read_text(encoding='utf-8'))
        # A decoder can split a question at a chunk boundary.
        joined = []
        for a,b in zip(extra_segments,extra_segments[1:]):
            if 0 <= b['start']-a['end'] < 2.5:
                joined.append(dict(words=a['words']+b['words']))
        extra_segments += joined
        nearby = words+[w for s in extra_segments for w in s['words']]
        pauses = answer_pauses(json.loads((ROOT/'.validation/listening-audio-silences'/(exam['id']+'.json')).read_text(encoding='utf-8'))['pauses'])
        matches = []
        for group in exam['groups']:
            count = group['last']-group['first']+1
            ends = [a for a,b in pauses if group['start'] < a <= group['end']+2 and b-a >= 8][-count:]
            if len(ends) != count:
                raise ValueError('Cannot locate every answer pause: '+exam['id']+'/'+group['id'])
            for question in [q for q in exam['questions'] if q['group_id'] == group['id']]:
                question_end = ends[question['number']-group['first']]+.3
                question_start = max(group['start']-.5,question_end-18)
                group_words = [w for w in words if question_start <= w['start'] < question_end]
                match = locate(question['prompt'], group_words)
                for segment in extra_segments:
                    candidate = locate(question['prompt'], [w for w in segment['words'] if w['end'] >= w['start'] and question_start <= w['start'] < question_end])
                    if candidate['score'] > match['score']:
                        match = candidate
                # Include the spoken number in the same audio replay as its prompt.
                numbers = [w for w in nearby if ''.join(tokens(w['word'])) == str(question['number']) and match['start']-3 <= w['start'] < match['start'] and w['end'] <= match['start']+.1]
                if numbers:
                    number_start = max(w['start'] for w in numbers)
                    leads = [w['start'] for w in nearby if ''.join(tokens(w['word'])) == 'question' and number_start-1 <= w['start'] < number_start]
                    match['start'] = min(leads) if leads else number_start
                match.update(number=question['number'], group=group['id'], prompt=question['prompt'])
                matches.append(match)
        results.append(dict(exam=exam['id'], questions=matches))
        print(exam['id'], 'min similarity', min(m['score'] for m in matches), flush=True)
    target = ROOT / '.validation/listening-question-audit.json'
    target.write_text(json.dumps(results, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print('Questions:', sum(len(e['questions']) for e in results))
    return results


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--retranscribe', action='store_true')
    parser.add_argument('--retry', action='store_true')
    parser.add_argument('--focused', action='store_true')
    parser.add_argument('--final-pass', action='store_true')
    parser.add_argument('--by-pauses', action='store_true')
    args = parser.parse_args()
    results = audit()
    if args.retranscribe or args.retry or args.focused or args.final_pass or args.by_pauses:
        import sys
        sys.path.insert(0, str(ROOT / '.validation/asr-deps'))
        from faster_whisper import WhisperModel, BatchedInferencePipeline
        model = WhisperModel('base.en', device='cpu', compute_type='int8', download_root=str(ROOT/'.validation/asr-models'), cpu_threads=6)
        pipeline = BatchedInferencePipeline(model)
        for result in results:
            if args.focused and not (ROOT/'.validation'/('aligned-asr-'+result['exam']+'-questions-retry.json')).exists():
                continue
            low = [q for q in result['questions'] if q['score'] < .75]
            if not low:
                continue
            # cet4 and cet6 use the same file names.
            file = next(f for f in FOLDER.rglob(result['exam'][5:]+'.resources.json') if json.loads(f.read_text(encoding='utf-8'))['id'] == result['exam'])
            exam = json.loads(file.read_text(encoding='utf-8'))
            suffix = '-questions-ordinal.json' if args.by_pauses else '-questions-final.json' if args.final_pass else '-questions-focused.json' if args.focused else '-questions-retry.json' if args.retry else '-questions.json'
            out = ROOT / '.validation' / ('aligned-asr-'+exam['id']+suffix)
            if out.exists():
                continue
            clips = []
            for group in exam['groups']:
                qs = [q for q in result['questions'] if q['group'] == group['id']]
                for q in qs:
                    if q['score'] >= .75:
                        continue
                    if args.by_pauses:
                        pauses = answer_pauses(json.loads((ROOT/'.validation/listening-audio-silences'/(exam['id']+'.json')).read_text())['pauses'])
                        count = group['last']-group['first']+1
                        ends = [a for a,b in pauses if group['start'] < a <= group['end']+2 and b-a >= 8][-count:]
                        end = ends[q['number']-group['first']]
                        clips.append(dict(start=end-12,end=end+.4))
                        continue
                    anchors = [(p['number'], (p['start']+p['end'])/2) for p in qs if p['score'] >= .75]
                    # The last spoken question ends just before the published group end.
                    anchors.append((group['last'], group['end']-3))
                    nearest = min(anchors, key=lambda p: abs(p[0]-q['number']))
                    centre = nearest[1]+(q['number']-nearest[0])*20
                    narrow = args.focused or args.final_pass
                    clips.append(dict(start=max(group['start'],centre-(3 if narrow else 13)),end=min(group['end']+2,centre+(9 if narrow else 13))))
            clips.sort(key=lambda c: c['start'])
            print('Checking original question audio:', exam['id'], len(clips), 'clips', flush=True)
            if args.retry or args.focused or args.final_pass or args.by_pauses:
                segments, _ = model.transcribe(str(ROOT/exam['audio']), language='en', word_timestamps=True, beam_size=5, condition_on_previous_text=False, vad_filter=False, no_speech_threshold=None, clip_timestamps=[v for c in clips for v in (c['start'],c['end'])])
            else:
                segments, _ = pipeline.transcribe(str(ROOT/exam['audio']), language='en', word_timestamps=True, beam_size=3, batch_size=4, condition_on_previous_text=False, clip_timestamps=clips)
            data = [dict(start=s.start,end=s.end,text=s.text,words=[dict(word=w.word,start=w.start,end=w.end) for w in s.words]) for s in segments]
            out.write_text(json.dumps(data,ensure_ascii=False),encoding='utf-8')
        audit()
