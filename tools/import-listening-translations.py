"""Reuse publicly published bilingual originals where sentence pairing is exact."""
import concurrent.futures,json,re,time
from pathlib import Path
import requests
from bs4 import BeautifulSoup
ROOT=Path(__file__).resolve().parents[1];FOLDER=ROOT/'assets/tavern/listening/real-exams/cettong'
def sentences(text,chinese=False):
    text=re.sub(r'(?m)^\s*(?:M|W|Man|Woman)\s*:\s*','',text).strip()
    return [s.strip() for s in re.split(r'(?<=[。！？])|(?<=[.!?])\s+' if chinese else r'(?<=[.!?])\s+|\n+',text) if s.strip()]
def normalize(text):return re.sub(r'[^a-z0-9]','',text.lower())
def run(exam):
    audio=ROOT/exam['files'][0]['path'];output=audio.with_suffix('.translations.json')
    if output.exists():return
    slug=exam['exam_id'].replace('_','-');url='https://english-exam.lazynote.cn/'+exam['level']+'/sections/listening/'+slug+'/'
    try:
        r=requests.get(url,timeout=(15,45));r.raise_for_status();r.encoding='utf-8';soup=BeautifulSoup(r.text,'html.parser');pairs={}
        for paragraph in soup.select('.lt-body p.lt-en'):
            for number in paragraph.select('.exam-pno'):number.decompose()
            zh=paragraph.find_next_sibling('p',class_='lt-zh')
            if not zh:continue
            en_parts=sentences(paragraph.get_text(' ',strip=True));zh_parts=sentences(zh.get_text(' ',strip=True),True)
            if len(en_parts)==1:pairs[normalize(en_parts[0])]=''.join(zh_parts)
            elif len(en_parts)==len(zh_parts):
                for en,chinese in zip(en_parts,zh_parts):pairs[normalize(en)]=chinese
        if not pairs:raise ValueError('No bilingual material found')
        output.write_text(json.dumps(dict(source=url,sentences=pairs),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        print(exam['level']+'/'+exam['exam_id']+': '+str(len(pairs))+' published sentence translations',flush=True)
    except Exception as error:print(exam['level']+'/'+exam['exam_id']+': translation fallback ('+str(error)+')',flush=True)
if __name__=='__main__':
    manifest=json.loads((FOLDER/'manifest.json').read_text(encoding='utf-8'))
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(run,[e for e in manifest['entries'] if e['status']=='downloaded']))
    count=0
    for audio in FOLDER.rglob('*.translations.json'):
        captions=audio.with_name(audio.name.replace('.translations.json','.captions.json'))
        if not captions.exists():continue
        source=json.loads(audio.read_text(encoding='utf-8'));doc=json.loads(captions.read_text(encoding='utf-8'))
        for line in doc['lines']:
            translated=source['sentences'].get(normalize(line['text']))
            if translated:line['translation']=translated;line['translation_source']=source['source'];count+=1
        doc['translation_sources']=[source['source'],'Sams200/opus-mt-en-zh (unmatched sentences only)']
        temporary=captions.with_suffix('.json.tmp');temporary.write_text(json.dumps(doc,ensure_ascii=False,indent=2)+'\n',encoding='utf-8');temporary.replace(captions)
    print('Applied '+str(count)+' published translations',flush=True)
