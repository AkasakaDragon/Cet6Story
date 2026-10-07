import gzip, json, re
from pathlib import Path
root=Path(__file__).resolve().parent.parent
words={row.split('\t')[0].lower() for row in gzip.open(root/'assets/dictionary.tsv.gz','rt',encoding='utf-8')}
chapter=json.loads((root/'chapters/12-tavern-01-02.json').read_text(encoding='utf-8-sig'))
tokens={x.lower().replace('’',"'") for line in chapter['lines'] for x in re.findall(r"[A-Za-z]+(?:['’][A-Za-z]+)?",line['text'])}
missing={x for x in tokens if x not in words and x not in {'lu','chuan','aelia','lyse'} and not (x.endswith("'s") and x[:-2] in words|{'aelia','lyse'})}
assert not missing, 'Missing: '+str(sorted(missing))
print('PASS: all section two dialogue vocabulary resolves, including possessives and names.')
