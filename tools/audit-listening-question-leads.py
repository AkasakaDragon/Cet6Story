"""Retain spoken question numbers using timestamped original-audio transcripts."""
import argparse
import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
FOLDER = ROOT / 'assets/tavern/listening/real-exams/cettong'
NAMES = 'zero one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen twenty'.split()


def number(text):
    tokens = re.findall(r'[a-z0-9]+', text.lower())
    if len(tokens) == 1 and tokens[0].isdigit():
        return int(tokens[0])
    if tokens and all(t in NAMES for t in tokens):
        return sum(NAMES.index(t) for t in tokens)
    return None


def spoken_lead(words, question_number, prompt_start, pauses=()):
    """Match singular Question + number, or a bare spoken number, locally."""
    candidates = []
    for i, word in enumerate(words):
        if not prompt_start-4 <= word['start'] <= prompt_start+.1:
            continue
        label = word['word'].strip().lower().strip('.,:?!')
        is_question = label in ('question', 'questions')
        if not is_question and i > 0 and words[i-1]['word'].strip().lower().strip('.,:?!') == 'questions':
            continue
        offset = i+1 if is_question else i
        for count in (1, 2):
            part = words[offset:offset+count]
            if len(part) != count or part[-1]['end'] > prompt_start+(1.5 if is_question else .15):
                continue
            if is_question and part[0]['start'] > prompt_start+.2:
                continue
            if any(b['start']-a['end'] > .6 for a, b in zip(part, part[1:])):
                continue
            if number(' '.join(w['word'] for w in part)) != question_number:
                continue
            following = words[offset+count]['word'].strip().lower().strip('.,:?!') if offset+count < len(words) else ''
            if label == 'questions' and following in ('to', 'through', 'and', 'are', 'based'):
                continue
            if is_question and part[0]['start']-word['end'] > .7:
                continue
            start = word['start']
            # ASR sometimes assigns preceding silence to the word "Question".
            if is_question and word['end']-start > .9:
                leading_pauses = [b for a, b in pauses if a >= start-.05 and b <= part[0]['start']+.02 and b-a >= .25]
                if leading_pauses:
                    start = max(start, max(leading_pauses)-.04)
            candidates.append((start, is_question))
    return candidates


def audit(apply=False):
    results = []
    for file in sorted(FOLDER.rglob('*.resources.json')):
        source = json.loads(file.read_text(encoding='utf-8'))
        caption_file = file.with_name(file.name.replace('.resources.', '.captions.'))
        captions = json.loads(caption_file.read_text(encoding='utf-8'))
        pause_file = ROOT/'.validation/listening-sentence-pauses'/(source['id']+'.json')
        pauses = json.loads(pause_file.read_text(encoding='utf-8'))['pauses']
        streams = []
        for transcript in sorted((ROOT/'.validation').glob('aligned-asr-'+source['id']+'*.json')):
            segments = json.loads(transcript.read_text(encoding='utf-8'))
            streams.append([w for s in segments for w in s.get('words', []) if w['end'] >= w['start']])
        changed = False
        for question in source['questions']:
            row = next(r for r in captions['lines'] if r.get('questionNumber') == question['number'])
            # Original prompt location may already include a number; search to the first prompt word.
            prompt_word = re.findall(r'[a-z0-9]+', question['prompt'].lower())[0]
            prefix = re.findall(r'[a-z0-9]+', question['prompt'].lower())[:3]
            anchors = []
            weak_anchors = []
            wh = {'what', 'why', 'how', 'who', 'when', 'where', 'which'}
            for words in streams:
                for index, word in enumerate(words):
                    if row['start']-.1 <= word['start'] <= row['start']+6:
                        heard = re.findall(r'[a-z0-9]+', ' '.join(w['word'].lower() for w in words[index:index+3]))
                        if heard and (heard[0] == prompt_word or heard[0] in wh and prompt_word in wh):
                            weak_anchors.append(word['start'])
                        if heard[:3] == prefix or len(heard) >= 3 and heard[0] in wh and prefix[0] in wh and heard[1:3] == prefix[1:3]:
                            anchors.append(word['start'])
            prompt_start = max(anchors or weak_anchors) if anchors or weak_anchors else row['start']
            leads = [lead for anchor in (anchors or weak_anchors or [prompt_start]) for words in streams
                     for lead in spoken_lead(words, question['number'], anchor, pauses)]
            strong = [start for start, explicit in leads if explicit]
            starts = strong or [start for start, explicit in leads]
            record = dict(exam=source['id'], number=question['number'], old_start=row['start'], prompt_start=prompt_start)
            if not starts:
                record['status'] = 'unresolved'
            else:
                number_rows = [r for r in captions['lines'] if r is not row
                               and min(starts)-.2 <= r['start'] <= row['start']
                               and number(re.sub(r'^question\s+', '', r['text'].strip(), flags=re.I)) == question['number']]
                prompt_key = re.sub(r'[^a-z0-9]', '', question['prompt'].lower())
                number_rows += [r for r in captions['lines'] if r is not row and r not in number_rows
                                and min(starts)-7 <= r['start'] <= row['start']
                                and r['end'] <= row['start']+.1 and re.search(r'\bquestion\b', r['text'], re.I)
                                and prompt_key in re.sub(r'[^a-z0-9]', '', r['text'].lower())]
                if number_rows:
                    captions['lines'] = [r for r in captions['lines'] if r not in number_rows]
                    record['merged_number_rows'] = len(number_rows)
                    changed = True
                preceding = [r['end'] for r in captions['lines'] if r['start'] < row['start']]
                if max(preceding, default=0) > min(starts)+.001:
                    previous = max((r for r in captions['lines'] if r['start'] < row['start']), key=lambda r: r['start'])
                    tail = re.findall(r'[a-z0-9]+', previous['text'].lower())[-2:]
                    endings = []
                    for words in streams:
                        for index in range(1, len(words)):
                            pair = words[index-1:index+1]
                            if [re.sub(r'[^a-z0-9]', '', w['word'].lower()) for w in pair] == tail and previous['start'] < pair[0]['start'] < min(starts) and pair[-1]['end'] < min(starts):
                                endings.append(pair[-1]['end'])
                            if ''.join(tail) == 'allright' and re.sub(r'[^a-z0-9]', '', words[index]['word'].lower()) == 'alright' and previous['start'] <= words[index]['start'] < min(starts) and words[index]['end'] < min(starts):
                                endings.append(words[index]['end'])
                    if not endings:
                        record.update(status='overlap', lead_start=min(starts))
                        results.append(record)
                        continue
                    previous['end'] = previous['alignment_end'] = round(max(endings)+.08, 3)
                    previous['playback_end'] = min(round(max(endings)+.18, 3), min(starts)-.06)
                    preceding = [r['end'] for r in captions['lines'] if r['start'] < row['start']]
                    record['previous_tail_corrected'] = True
                    changed = True
                lead_start = round(max(max(preceding, default=0), min(starts)-.06), 3)
                record.update(status='verified', lead_start=lead_start, explicit_question=bool(strong))
                if row.get('spoken_number_start') != lead_start:
                    row['spoken_number_start'] = lead_start
                    question['spoken_number_start'] = lead_start
                    changed = True
                if question.get('spoken_number_start') != lead_start:
                    question['spoken_number_start'] = lead_start
                    changed = True
                if row['start'] > lead_start+.001:
                    record['status'] = 'corrected'
                    row['start'] = row['alignment_start'] = lead_start
                    changed = True
                if question['start'] > row['start']+.001:
                    question['start'] = row['start']
                    changed = True
            results.append(record)
        for previous, following in zip(captions['lines'], captions['lines'][1:]):
            if previous.get('playback_end', previous['end']) > following['start'] and previous['end'] <= following['start']:
                previous['playback_end'] = following['start']
                changed = True
        if apply and changed:
            for path, doc in ((file, source), (caption_file, captions)):
                path.write_text(json.dumps(doc, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    report = ROOT/'.validation/listening-question-lead-audit.json'
    report.write_text(json.dumps(results, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(json.dumps({status: sum(r['status'] == status for r in results) for status in ('verified', 'corrected', 'unresolved', 'overlap')}))
    return results


def transcribe_missing(results, retry=False):
    sys.path.insert(0, str(ROOT/'.validation/asr-deps'))
    from faster_whisper import WhisperModel
    model = WhisperModel('base.en', device='cpu', compute_type='int8',
                         download_root=str(ROOT/'.validation/asr-models'), cpu_threads=6, local_files_only=True)
    for file in sorted(FOLDER.rglob('*.resources.json')):
        exam = json.loads(file.read_text(encoding='utf-8'))
        missing = [r for r in results if r['exam'] == exam['id'] and r['status'] in ('unresolved', 'overlap')]
        if not missing:
            continue
        output = ROOT/'.validation'/('aligned-asr-'+exam['id']+('-question-leads-retry.json' if retry else '-question-leads.json'))
        if output.exists():
            continue
        clips = [v for r in missing for v in (max(0, r['prompt_start']-(20 if r['status'] == 'overlap' else 4)), r['prompt_start']+(5 if retry else .6))]
        print('Checking question leads:', exam['id'], len(missing), flush=True)
        segments, _ = model.transcribe(str(ROOT/exam['audio']), language='en', word_timestamps=True,
                                      beam_size=5, condition_on_previous_text=False, vad_filter=False,
                                      no_speech_threshold=None, clip_timestamps=clips)
        data = [dict(start=s.start, end=s.end, text=s.text,
                     words=[dict(word=w.word, start=w.start, end=w.end) for w in s.words]) for s in segments]
        output.write_text(json.dumps(data, ensure_ascii=False), encoding='utf-8')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--apply', action='store_true')
    parser.add_argument('--transcribe-missing', action='store_true')
    parser.add_argument('--retry-missing', action='store_true')
    args = parser.parse_args()
    results = audit(args.apply)
    if args.transcribe_missing or args.retry_missing:
        transcribe_missing(results, args.retry_missing)
        audit(args.apply)
