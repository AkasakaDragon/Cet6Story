"""Reuse public ExamPace listening materials authorized by the site owner/user.
Only reads published CET papers/questions; does not change the remote site.
"""
import concurrent.futures,json,re,threading,time
from pathlib import Path
from urllib.parse import urljoin
import requests
ROOT=Path(__file__).resolve().parents[1];BASE='https://117.72.200.49'
FOLDER=ROOT/'assets/tavern/listening/real-exams/cettong';CACHE=ROOT/'.validation/exampace';CACHE.mkdir(parents=True,exist_ok=True)
LOCAL=threading.local()
def get(path,**kwargs):
    if not hasattr(LOCAL,'session'):LOCAL.session=requests.Session()
    for attempt in range(3):
        try:
            r=LOCAL.session.get(urljoin(BASE,path),timeout=(15,90),**kwargs);r.raise_for_status();return r
        except requests.RequestException:
            if attempt==2:raise
            time.sleep(2)
def clean(text):return (text or '').replace('\\n','\n').replace("\\'","'")
def download_audio(audio,temporary):
    size=audio.get('size',0)
    if size and temporary.exists() and temporary.stat().st_size==size:return
    if not size:
        with get(audio['url'],stream=True) as r,temporary.open('wb') as output:
            for chunk in r.iter_content(128*1024):output.write(chunk)
        return
    # The site's published media endpoint supports HTTP ranges. Three streams
    # avoid one slow long-running transfer; each range is checked before joining.
    ranges=[];width=(size+2)//3
    for i in range(3):ranges.append((i*width,min(size-1,(i+1)*width-1)))
    def part(bounds):
        start,end=bounds;path=CACHE/(audio['id']+'-'+str(start)+'.range')
        if path.exists() and path.stat().st_size==end-start+1:return path
        for attempt in range(6):
            offset=path.stat().st_size if path.exists() else 0
            if offset>end-start+1:path.unlink();offset=0
            resume=start+offset
            if resume>end:return path
            try:
                with get(audio['url'],headers={'Range':'bytes='+str(resume)+'-'+str(end)},stream=True) as r:
                    if r.status_code!=206 or r.headers.get('Content-Range')!='bytes '+str(resume)+'-'+str(end)+'/'+str(size):raise ValueError('Media range not supported')
                    with path.open('ab') as output:
                        for chunk in r.iter_content(128*1024):output.write(chunk)
                if path.stat().st_size!=end-start+1:raise ValueError('Incomplete media range')
                return path
            except requests.RequestException:
                if attempt==5:raise
                time.sleep(2)
    with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:parts=list(pool.map(part,ranges))
    with temporary.open('wb') as output:
        for path in parts:
            with path.open('rb') as source:
                for chunk in iter(lambda:source.read(1024*1024),b''):output.write(chunk)
def run(entry):
    level=entry['level'];year,month,number=map(int,entry['exam_id'].split('_'))
    existing=(ROOT/entry['files'][0]['path']).with_suffix('.resources.json')
    if existing.exists():
        saved=json.loads(existing.read_text(encoding='utf-8'))
        if (ROOT/saved['audio']).exists():return dict(id=saved['id'],questions=len(saved['questions']),groups=len(saved['groups']),sentences=len(saved['source_sentences']))
    candidates=[p for p in CATALOG['papers'] if p.get('examId')==level and int(p.get('year',0))==year and int(re.search(r'\d+',p.get('month','0')).group())==month and int(p.get('set',0))==number]
    if not candidates:return dict(id=level+'-'+entry['exam_id'],error='No corresponding published paper')
    item=candidates[0];cached=CACHE/(item['id']+'.json')
    if cached.exists():bundle=json.loads(cached.read_text(encoding='utf-8'))
    else:
        p=get('/api/content/paper',params={'id':item['id']}).json()['paper']
        ids=[q for section in p.get('sections',[]) if '听力' in section.get('title','') for q in section.get('questionIds',[])]
        if not ids:
            summary=get('/api/content/questions',params={'paperId':p['id'],'size':200,'page':1}).json()
            ids=[q['id'] for q in summary.get('questions',[]) if q.get('questionType')=='听力']
        questions=[]
        if ids:
            r=LOCAL.session.post(BASE+'/api/content/resolve',json={'ids':ids,'preview':False},timeout=90);r.raise_for_status();questions=r.json()['questions']
        bundle=dict(paper=p,questions=questions);cached.write_text(json.dumps(bundle,ensure_ascii=False),encoding='utf-8')
    p=bundle['paper'];audio=p.get('audio') or p.get('resources',{}).get('audio');original=ROOT/entry['files'][0]['path'];selected=original
    if isinstance(audio,dict) and audio.get('url') and audio.get('size')!=original.stat().st_size:
        selected=original.with_name(original.stem+'.exampace-audio.mp3')
        if not selected.exists():
            temporary=selected.with_suffix('.mp3.part')
            download_audio(audio,temporary)
            if audio.get('size') and temporary.stat().st_size!=audio['size']:raise ValueError('Incomplete source audio')
            temporary.replace(selected)
    groups=[]
    for group in p.get('listeningSegments',[]):
        groups.append(dict(id=group['id'],title=group.get('title','听力片段'),start=group['start'],end=group['end'],first=group.get('first',1),last=group.get('last',25),material=clean(group.get('material',''))))
    qs=[]
    for q in sorted(bundle['questions'],key=lambda q:q.get('order',0)):
        if len(q.get('options',[]))!=4 or not isinstance(q.get('answer'),int):continue
        qs.append(dict(number=q['order'],prompt=clean(q.get('text')),options=[clean(o) for o in q['options']],answer=q['answer'],explanation=clean(q.get('explanation','')),group_id=q.get('listeningGroupId',''),start=q.get('listeningStart',0),end=q.get('listeningEnd',0)))
    lines=p.get('listeningSentences') or [s for g in p.get('listeningSegments',[]) for s in g.get('sentences',[])]
    doc=dict(id=level+'-'+entry['exam_id'],title=entry['title'],level=int(level[-1]),audio=selected.relative_to(ROOT).as_posix(),source_url=entry['page_url'],material_source=BASE+'/#/library',material_paper_id=p['id'],original_text_source=p.get('sourceUrl',''),groups=groups,questions=qs,source_sentences=lines)
    original.with_suffix('.resources.json').write_text(json.dumps(doc,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(doc['id']+': '+str(len(qs))+' questions, '+str(len(groups))+' groups, '+str(len(lines))+' timed sentences',flush=True)
    return dict(id=doc['id'],questions=len(qs),groups=len(groups),sentences=len(lines))
catalog_file=ROOT/'.validation/junior-catalog.json'
if catalog_file.exists():CATALOG=json.loads(catalog_file.read_text(encoding='utf-8'))
else:
    CATALOG=get('/api/content/catalog',params={'size':5000,'page':1}).json()
    catalog_file.write_text(json.dumps(CATALOG,ensure_ascii=False),encoding='utf-8')
if __name__=='__main__':
    entries=[e for e in json.loads((FOLDER/'manifest.json').read_text(encoding='utf-8'))['entries'] if e['status']=='downloaded']
    with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:results=list(pool.map(run,entries))
    (FOLDER/'exampace-import.json').write_text(json.dumps(results,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('Imported '+str(len(results))+' papers',flush=True)
