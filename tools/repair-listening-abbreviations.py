"""Repair existing abbreviation fragments without changing recording alignment."""
import json
from pathlib import Path
from listening_sentences import abbreviation_continues, sentences
ROOT = Path(__file__).resolve().parents[1]
FOLDER = ROOT / 'assets/tavern/listening/real-exams/cettong'

def main():
    assert sentences('The U.S. average price is $4.25. It rose.') == ['The U.S. average price is $4.25.', 'It rose.']
    assert sentences('Mr. Jones met Dr. Smith. They left.') == ['Mr. Jones met Dr. Smith.', 'They left.']
    assert sentences('He lives in the U.S. The next speaker lives abroad.') == ['He lives in the U.S.', 'The next speaker lives abroad.']
    assert sentences('Former U.S. President spoke. It ended.') == ['Former U.S. President spoke.', 'It ended.']
    pending = []
    for file in FOLDER.rglob('*.captions.json'):
        doc = json.loads(file.read_text(encoding='utf-8'))
        resources = json.loads(file.with_name(file.name.replace('.captions.', '.resources.')).read_text(encoding='utf-8'))
        def group(row):
            return next((g['id'] for g in resources['groups'] if g['start']-.5 <= row['start'] < g['end']), None)
        rows = []
        merged = []
        for row in doc['lines']:
            if rows and abbreviation_continues(rows[-1]['text'], row['text']) and 0 <= row['start']-rows[-1]['end'] < 2.5 and group(rows[-1]) == group(row) and rows[-1].get('speaker') == row.get('speaker'):
                previous = rows[-1]
                previous['text'] += ' ' + row['text']
                previous['end'] = row['end']
                previous['translation'] = ''
                previous.pop('translation_source', None)
                if previous not in merged:
                    merged.append(previous)
            else:
                rows.append(dict(row))
        if merged:
            doc['lines'] = rows
            pending.append((file, doc, merged))
    print('Repairing', sum(len(rows) for _,_,rows in pending), 'sentences in', len(pending), 'exams', flush=True)
    # Use the already cached local model; no audio is regenerated or downloaded.
    from importlib.util import spec_from_file_location, module_from_spec
    spec = spec_from_file_location('caption_builder', ROOT/'tools/build-real-exam-captions.py')
    builder = module_from_spec(spec)
    spec.loader.exec_module(builder)
    translate = builder.translator()
    for file, doc, merged in pending:
        translations = file.with_name(file.name.replace('.captions.', '.translations.'))
        published = json.loads(translations.read_text(encoding='utf-8')) if translations.exists() else {'sentences': {}}
        for row in merged:
            exact = published['sentences'].get(builder.re.sub(r'[^a-z0-9]', '', row['text'].lower()))
            row['translation'] = exact or translate([row['text']])[0]
            if exact:
                row['translation_source'] = published['source']
        backup = ROOT/'.validation/subtitle-abbreviations'/file.relative_to(FOLDER)
        backup.parent.mkdir(parents=True, exist_ok=True)
        if not backup.exists():
            backup.write_bytes(file.read_bytes())
        file.write_text(json.dumps(doc, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
        print(doc['id'], len(merged), flush=True)
    target = json.loads((FOLDER/'cet4/2025/2025_12_2.captions.json').read_text(encoding='utf-8'))
    assert not any(r['text'] == 'The U.S.' for r in target['lines'])
    assert any(r['text'].startswith('The U.S. average price') and r['start']==53.22 and r['end']==60.09 for r in target['lines'])
    print('PASS abbreviation splitting and repaired example timing', flush=True)

if __name__ == '__main__':
    main()
