"""Import bilingual examples only; never replace vocabulary or player saves."""
import argparse
import hashlib
import html
import json
import re
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def clean(value):
    return re.sub(r'\s+', ' ', html.unescape(re.sub(r'<[^>]*>', '', value or ''))).strip()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('files', nargs='+', type=Path)
    args = parser.parse_args()
    gathered = defaultdict(list)
    sources = []
    rejected = 0
    for file in args.files:
        raw = file.read_bytes()
        count = 0
        for number, line in enumerate(raw.decode('utf-8-sig').splitlines(), 1):
            if not line.strip():
                continue
            try:
                row = json.loads(line)
            except ValueError as error:
                raise ValueError(f'{file.name}:{number}: {error}') from error
            count += 1
            word = clean(row.get('word') or row.get('headWord')).lower()
            if not word:
                continue
            content = row.get('content', {}).get('word', {}).get('content', {})
            sentences = row.get('sentences') or content.get('sentence', {}).get('sentences', [])
            for sentence in sentences:
                english = clean(sentence.get('sentence') or sentence.get('sContent'))
                chinese = clean(sentence.get('translation') or sentence.get('sCn'))
                if (not re.search(r'[A-Za-z]', english) or not re.search(r'[\u3400-\u9fff]', chinese)
                        or not 8 <= len(english) <= 360 or len(chinese) > 240
                        or '...' in english or '…' in english
                        or (not re.search(r'''[.!?]["”')]*$''', english)
                            and len(re.findall(r"[A-Za-z]+(?:['’][A-Za-z]+)?", english)) < 6)):
                    rejected += 1
                    continue
                pair = {'english': english, 'chinese': chinese}
                if not any(old['english'].casefold() == english.casefold() for old in gathered[word]):
                    gathered[word].append(pair)
        sources.append({'file': file.name, 'sha256': hashlib.sha256(raw).hexdigest(), 'rows': count})
    examples = {}
    for word, pairs in sorted(gathered.items()):
        exact = re.compile(r'(?<![A-Za-z])' + re.escape(word) + r'(?![A-Za-z])', re.I)
        pairs.sort(key=lambda pair: (not bool(exact.search(pair['english'])),
                                    '(=' in pair['english'].replace(' ', ''),
                                    len(pair['english']) > 150, abs(len(pair['english'])-75)))
        examples[word] = pairs[:2]
    output = ROOT/'assets/word-examples.json'
    output.write_text(json.dumps(examples, ensure_ascii=False, separators=(',', ':'))+'\n', encoding='utf-8')
    coverage = {}
    for name in ('cet4-words.json', 'cet6-words.json'):
        bank = json.loads((ROOT/'assets'/name).read_text(encoding='utf-8-sig'))
        missing = [row['word'] for row in bank if row['word'].lower() not in examples]
        coverage[name] = {'total': len(bank), 'matched': len(bank)-len(missing), 'missing': missing}
    report = {'sources': sources, 'words': len(examples), 'examples': sum(map(len, examples.values())),
              'rejected_pairs': rejected, 'coverage': coverage}
    report_path = ROOT/'项目文档/双语例句导入核对-20261011.json'
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(json.dumps({**{k: v for k, v in report.items() if k != 'coverage'},
                      'coverage': {k: {x: y for x, y in v.items() if x != 'missing'}
                                   for k, v in coverage.items()}}, ensure_ascii=True, indent=2))


if __name__ == '__main__':
    main()
