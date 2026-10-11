"""Audit vocabulary used by the game; apply explicitly reviewed common meanings."""
import argparse
import collections
import gzip
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def vocabulary():
    words = collections.Counter()
    for file in list((ROOT/'chapters').rglob('*.json'))+list((ROOT/'assets/tavern/listening').rglob('*.captions.json')):
        doc = json.loads(file.read_text(encoding='utf-8-sig'))
        if isinstance(doc, dict):
            for row in doc.get('lines', []):
                words.update(w.lower() for w in re.findall(r"[A-Za-z]+(?:['’][A-Za-z]+)?", row.get('text', '')))
    for file in ('cet4-words.json', 'cet6-words.json'):
        words.update(r['word'].lower() for r in json.loads((ROOT/'assets'/file).read_text(encoding='utf-8')))
    for row in (ROOT/'assets/rogue-words.tsv').read_text(encoding='utf-8-sig').splitlines():
        if row.strip():
            words[row.split('\t')[0].lower()] += 1
    return words


def main(apply=False):
    words = vocabulary()
    path = ROOT/'assets/dictionary.tsv.gz'
    rows = gzip.decompress(path.read_bytes()).decode('utf-8').splitlines()
    lex = {row.split('\t', 2)[0].lower(): row.split('\t', 2)[2] for row in rows if row.count('\t') >= 2}
    corrections = json.loads((ROOT/'assets/dictionary-corrections.json').read_text(encoding='utf-8'))
    missing = set(corrections)-set(lex)
    assert not missing, ('unknown correction words', missing)
    if apply:
        for index, row in enumerate(rows):
            cells = row.split('\t', 2)
            if len(cells) == 3 and cells[0].lower() in corrections:
                cells[2] = corrections[cells[0].lower()]
                rows[index] = '\t'.join(cells)
        path.write_bytes(gzip.compress(('\n'.join(rows)+'\n').encode('utf-8'), mtime=0))
        for name in ('cet4-words.json', 'cet6-words.json'):
            file = ROOT/'assets'/name
            doc = json.loads(file.read_text(encoding='utf-8'))
            changed = False
            for row in doc:
                if row['word'].lower() in corrections and row['meaning'] != corrections[row['word'].lower()]:
                    row['meaning'] = corrections[row['word'].lower()]
                    changed = True
            if changed:
                file.write_text(json.dumps(doc, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
        lex.update(corrections)
    assert all(lex[w] == meaning for w, meaning in corrections.items()), 'dictionary corrections missing; run --apply'
    flags = [dict(word=w, occurrences=words[w], meaning=lex[w]) for w in words if w in lex
             and (re.match(r'^abbr\.', lex[w]) or re.search(r'人名|姓氏|队名|郡名', lex[w])) and w not in corrections]
    report = dict(vocabulary_count=len(words), dictionary_count=len(lex), corrections_count=len(corrections),
                  corrected_words=corrections, remaining_candidates=sorted(flags, key=lambda r: -r['occurrences']),
                  scope='Existing story/listening vocabulary and CET4/CET6/rogue training banks; risk screening plus reviewed corrections, not an exhaustive semantic proof of the full dictionary.')
    (ROOT/'.validation/game-word-meaning-audit.json').write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(f'PASS: {len(words)} used/training vocabulary entries screened; {len(lex)} dictionary entries loaded; {len(corrections)} reviewed corrections synchronized.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--apply', action='store_true')
    main(parser.parse_args().apply)
