"""Export corroborated inflections for vocabulary used in the game."""
import argparse
import csv
import gzip
import hashlib
import json
import re
from pathlib import Path
import importlib.util

ROOT = Path(__file__).resolve().parents[1]
TYPES = {'p': '过去式', 'd': '过去分词', 'i': '现在分词', '3': '第三人称单数',
         's': '名词复数', 'r': '比较级', 't': '最高级'}
# Reviewed variants where the upstream automatic lemma has an extra/dropped e.
REVIEWED = {
    'authored': ('author', 'pd'), 'biases': ('bias', 's3'), 'campuses': ('campus', 's'),
    'colored': ('color', 'pd'), 'coloring': ('color', 'i'), 'endeavored': ('endeavor', 'pd'),
    'factored': ('factor', 'pd'), 'formulas': ('formula', 's'), 'honored': ('honor', 'pd'),
    'labeled': ('label', 'pd'), 'lied': ('lie', 'pd'), 'modeled': ('model', 'pd'),
    'rearranging': ('rearrange', 'i'), 'reuniting': ('reunite', 'i'),
    'signaled': ('signal', 'pd'), 'signaling': ('signal', 'i'), 'statuses': ('status', 's'),
    'tailoring': ('tailor', 'i'), 'ticketing': ('ticket', 'i'), 'traveled': ('travel', 'pd'),
    'underlies': ('underlie', '3'), 'vacuuming': ('vacuum', 'i'), 'videoed': ('video', 'pd'),
    'volcanoes': ('volcano', 's'), 'were': ('be', 'p'),
    'seasoned': ('season', 'pd'), 'whipped': ('whip', 'pd'),
    'cloned': ('clone', 'pd'), 'cloning': ('clone', 'i'),
    'postponing': ('postpone', 'i'), 'ruined': ('ruin', 'pd'),
    'ruining': ('ruin', 'i'), 'proven': ('prove', 'd'),
}


def exchange(raw):
    return dict(part.split(':', 1) for part in raw.split('/') if ':' in part)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    args = parser.parse_args()
    spec = importlib.util.spec_from_file_location('meanings', ROOT/'tools/audit-game-word-meanings.py')
    audit = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(audit)
    used = audit.vocabulary()
    used.update(REVIEWED.keys())
    # Include dialogue questions/options as well as subtitle and training vocabulary.
    for file in (ROOT/'assets/tavern/listening').rglob('*.resources.json'):
        doc = json.loads(file.read_text(encoding='utf-8-sig'))
        for question in doc.get('questions', []):
            for text in [question.get('prompt', ''), *question.get('options', [])]:
                used.update(word.lower() for word in re.findall(r"[A-Za-z]+(?:['’][A-Za-z]+)?", text))
    rows = {}
    with args.source.open(encoding='utf-8-sig', newline='') as stream:
        for row in csv.DictReader(stream):
            if row.get('exchange'):
                rows[row['word'].lower()] = exchange(row['exchange'])
    lex = {}
    for row in gzip.open(ROOT/'assets/dictionary.tsv.gz', 'rt', encoding='utf-8'):
        cells = row.rstrip('\n').split('\t', 2)
        if len(cells) == 3:
            lex[cells[0].lower()] = cells[2]
    forms, unconfirmed = {}, []
    for word in sorted(used):
        if word in REVIEWED:
            lemma, kinds = REVIEWED[word]
            if lemma in lex:
                forms[word] = {'lemma': lemma, 'types': [TYPES[kind] for kind in kinds]}
                continue
        ex = rows.get(word, {})
        lemma = ex.get('0', '').lower()
        if not lemma or lemma == word or not lemma.isalpha() or not word.isalpha():
            continue
        reverse = rows.get(lemma, {})
        kinds = [kind for kind in TYPES if word in re.split(r'[,/\s]+', reverse.get(kind, '').lower())]
        # Some upstream entries mistake derived adjectives for inflected verbs.
        # Retain only grammatical roles supported by the base entry itself.
        base = lex.get(lemma, '')
        kinds = [kind for kind in kinds if
                 (kind in ('p', 'd', 'i', '3') and re.search(r'(?<![A-Za-z])(?:v|vt|vi|vbl)\.', base))
                 or (kind == 's' and re.search(r'(?<![A-Za-z])n\.', base))
                 or (kind in ('r', 't') and re.search(r'(?<![A-Za-z])(?:a|adj|adv)\.', base))]
        if kinds and lemma in lex:
            forms[word] = {'lemma': lemma, 'types': [TYPES[kind] for kind in kinds]}
        else:
            unconfirmed.append({'word': word, 'candidate': lemma})
    (ROOT/'assets/dictionary-forms.json').write_text(json.dumps(forms, ensure_ascii=False, separators=(',', ':'))+'\n', encoding='utf-8')
    report = {'source': 'https://github.com/skywind3000/ECDICT',
              'sha256': hashlib.sha256(args.source.read_bytes()).hexdigest(),
              'vocabulary': len(used), 'confirmed_forms': len(forms), 'unconfirmed': unconfirmed,
              'forms': forms,
              'scope': 'Bidirectional word-form evidence, not a guarantee of completeness of every base-word meaning.'}
    (ROOT/'项目文档/单词变形释义全量核对-20261011.json').write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(f'PASS: {len(used)} vocabulary entries screened; {len(forms)} corroborated forms; {len(unconfirmed)} ambiguous/unconfirmed forms excluded.')
    print('Excluded: '+', '.join(item['word']+' -> '+item['candidate'] for item in unconfirmed))


if __name__ == '__main__':
    main()

