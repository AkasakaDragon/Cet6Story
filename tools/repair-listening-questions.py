"""Repair question subtitles only after checking the original audio transcript."""
import importlib.util
import json
import re
from pathlib import Path
from listening_sentences import answer_pauses

ROOT = Path(__file__).resolve().parents[1]
FOLDER = ROOT / 'assets/tavern/listening/real-exams/cettong'
NUMBER = re.compile(r'^(?:Question\s*)?\d+[.:]?$', re.I)


def question_translation(question, published):
    norm = re.sub(r'[^a-z0-9]', '', question['prompt'].lower())
    if published.get('sentences', {}).get(norm):
        return published['sentences'][norm]
    match = re.search(r'【翻译】\s*(.+?)(?=\s*A\s*[)）.、]|【|$)', question['explanation'], re.S)
    return re.sub(r'\s+', ' ', match.group(1)).strip() if match else ''


def repair():
    evidence = json.loads((ROOT/'.validation/listening-question-audit.json').read_text(encoding='utf-8'))
    unresolved = [(e['exam'],q['number'],q['score']) for e in evidence for q in e['questions'] if q['score'] < .75]
    if unresolved:
        raise ValueError('Original audio still needs checking: '+str(unresolved))
    assert len(evidence) == len(list(FOLDER.rglob('*.resources.json')))
    translate = None
    count = merged = 0
    for exam in evidence:
        file = next(f for f in FOLDER.rglob(exam['exam'][5:]+'.resources.json') if json.loads(f.read_text(encoding='utf-8'))['id'] == exam['exam'])
        resources = json.loads(file.read_text(encoding='utf-8'))
        caption_file = file.with_name(file.name.replace('.resources.', '.captions.'))
        doc = json.loads(caption_file.read_text(encoding='utf-8'))
        native_file = file.with_name(file.name.replace('.resources.', '.translations.'))
        published = json.loads(native_file.read_text(encoding='utf-8')) if native_file.exists() else {}
        rows = doc['lines']
        pauses = answer_pauses(json.loads((ROOT/'.validation/listening-audio-silences'/(exam['exam']+'.json')).read_text(encoding='utf-8'))['pauses'])
        for group in resources['groups']:
            verified = sorted((m for m in exam['questions'] if m['group'] == group['id']),key=lambda m:m['number'])
            assert all(a['end'] <= b['start'] for a,b in zip(verified,verified[1:])),(exam['exam'],group['id'],'question order/overlap')
        for match in exam['questions']:
            question = next(q for q in resources['questions'] if q['number'] == match['number'])
            start, end = match['start'], match['end']
            # Include the complete utterance, even if ASR missed a final word.
            boundaries = [a for a,b in pauses if start+.5 < a < start+20]
            if not boundaries:
                raise ValueError('Cannot verify complete spoken question: '+exam['exam']+'/'+str(question['number']))
            end = round(min(boundaries)+.15,3)
            preceding = [r for r in rows if (NUMBER.fullmatch(r['text'].strip()) and 0 <= start-r['end'] < 3)
                         or (re.match(r'^(?:Question\s*)?'+str(question['number'])+r'[.:]?\s',r['text'],re.I) and start-3 <= r['start'] <= start and r['end'] > start)]
            if preceding:
                start = min(r['start'] for r in preceding)
            translation = question_translation(question, published)
            if not translation:
                if translate is None:
                    spec = importlib.util.spec_from_file_location('caption_builder', ROOT/'tools/build-real-exam-captions.py')
                    builder = importlib.util.module_from_spec(spec); spec.loader.exec_module(builder)
                    translate = builder.translator()
                translation = translate([question['prompt']])[0]
            keep = []
            for row in rows:
                if row['start'] < end+.15 and row['end'] > start-.15:
                    # Preserve the preceding material when an old row extends into a question.
                    if row['start'] < start-2 and not re.match(r'^(?:Question\b|\d+[.:])', row['text'], re.I):
                        row = dict(row, end=start)
                        keep.append(row)
                    continue
                keep.append(row)
            row = dict(speaker='听力原声', actor='', text=str(question['number'])+'. '+question['prompt'],
                       translation='第 '+str(question['number'])+' 题：'+translation,
                       start=round(start,3),end=round(end,3),questionNumber=question['number'])
            rows = keep+[row]
            question['start'], question['end'] = row['start'], row['end']
            question['spoken_text'] = match['text']
            question['audio_match_score'] = match['score']
            count += 1
        rows.sort(key=lambda r: r['start'])
        clean = []
        for row in rows:
            if clean and NUMBER.fullmatch(clean[-1]['text'].strip()):
                previous = clean[-1]
                group = next((g for g in resources['groups'] if g['start']-.5 <= previous['start'] < g['end']),None)
                if row['start']-previous['end'] < 5 and (group is None or row['start'] < group['end']):
                    previous['text'] += ' '+row['text']
                    previous['translation'] = row['translation']
                    previous['end'] = row['end']
                    merged += 1
                    continue
                clean.pop()
            clean.append(row)
        if clean and NUMBER.fullmatch(clean[-1]['text'].strip()):
            clean.pop()
        for i,row in enumerate(clean[:-1]):
            row['end'] = min(row['end'],clean[i+1]['start'])
        assert all(r['end'] > r['start'] for r in clean),exam['exam']
        assert not any(NUMBER.fullmatch(r['text'].strip()) for r in clean)
        assert sorted(r['questionNumber'] for r in clean if r.get('questionNumber')) == list(range(1,26)),exam['exam']
        for group in resources['groups']:
            qs = [q for q in resources['questions'] if q['group_id'] == group['id']]
            group['end'] = round(max(group['end'],max(q['end'] for q in qs)+.1),3)
            assert all(group['start']-.5 <= q['start'] < q['end'] <= group['end']+.1 for q in qs),(exam['exam'],group['id'])
        doc['lines'] = clean
        resources['question_timing_source'] = 'original_audio_transcript_verified'
        doc['question_verification'] = 'All 25 published question prompts located in original audio; numbers and prompts share one replay unit.'
        for target,value in [(file,resources),(caption_file,doc)]:
            backup = ROOT/'.validation/listening-question-backup'/target.relative_to(FOLDER)
            backup.parent.mkdir(parents=True,exist_ok=True)
            if not backup.exists():backup.write_bytes(target.read_bytes())
            target.write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('PASS: repaired',count,'complete question subtitles;',merged,'other number fragments merged.')


if __name__ == '__main__':
    repair()
