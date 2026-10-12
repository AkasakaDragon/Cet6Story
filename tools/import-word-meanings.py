"""Use supplied JSONL definitions for existing CET words; preserve reviewed fixes."""
import json,re,html,hashlib,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
def clean(s):
 return re.sub(r"\s+"," ",html.unescape(re.sub(r"<[^>]*>","",s or ""))).strip()
def main(files):
 gathered={};sources=[]
 for file in files:
  raw=file.read_bytes();sources.append(dict(file=file.name,sha256=hashlib.sha256(raw).hexdigest()))
  for line in raw.decode('utf-8-sig').splitlines():
   if not line.strip():continue
   row=json.loads(line);word=clean(row.get('headWord') or row.get('word')).lower()
   content=row.get('content',{}).get('word',{}).get('content',{})
   groups=gathered.setdefault(word,{})
   for item in content.get('trans',[]):
    pos=clean(item.get('pos')).rstrip('.');text=clean(item.get('tranCn'))
    if not text or not re.search(r'[\u3400-\u9fff]',text):continue
    terms=groups.setdefault(pos,[])
    for term in re.split(r'[;；,，、]+',text):
     term=term.strip().rstrip('。')
     if term and term not in terms:terms.append(term)
 corrections=json.loads((ROOT/'assets/dictionary-corrections.json').read_text(encoding='utf-8'))
 merged={};report=dict(sources=sources,banks={})
 for name in ['cet4-words.json','cet6-words.json']:
  path=ROOT/'assets'/name;rows=json.loads(path.read_text(encoding='utf-8'));before=[r['word'] for r in rows];matched=0;missing=[]
  for row in rows:
   word=row['word'].lower();groups=gathered.get(word,{})
   meaning='；'.join((pos+'. ' if pos else '')+'、'.join(terms) for pos,terms in groups.items() if terms)
   if meaning:
    matched+=1;merged[word]=corrections.get(word,meaning);row['meaning']=merged[word]
   elif word in corrections:row['meaning']=corrections[word];merged[word]=row['meaning']
   else:missing.append(word)
  assert before==[r['word'] for r in rows]
  path.write_text(json.dumps(rows,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
  report['banks'][name]=dict(total=len(rows),matched=matched,missing=missing)
 (ROOT/'assets/word-meanings.json').write_text(json.dumps(merged,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 report['imported_words']=len(merged);report['reviewed_priority']=[w for w in merged if w in corrections]
 (ROOT/'项目文档/JSONL释义导入核对-20261011.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 print(json.dumps({k:{'total':v['total'],'matched':v['matched'],'missing':len(v['missing'])} for k,v in report['banks'].items()}));print('Shared definitions:',len(merged))
if __name__=='__main__':main([Path(x) for x in sys.argv[1:]])
