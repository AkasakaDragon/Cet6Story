"""Build offline, word-timed listening captions; generated captions are auxiliary.
Uses locally cached faster-whisper base.en and OPUS-MT en-zh (CC-BY-4.0).
Run with the bundled Python 3.12 and .validation/asr-deps installed.
"""
import argparse, hashlib, json, re, sys, time, difflib
from pathlib import Path
from listening_sentences import sentences, abbreviation_continues, merge_abbreviation_rows, attach_verified_questions
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT/'.validation/asr-deps'))
import requests, sentencepiece, ctranslate2
from faster_whisper import WhisperModel, BatchedInferencePipeline

def translator():
    folder=ROOT/'.validation/translation-model';folder.mkdir(parents=True,exist_ok=True)
    for name in ['config.json','model.bin','shared_vocabulary.json','source.spm','target.spm','README.md']:
        target=folder/name
        if not target.exists():
            print('Downloading translation model: '+name,flush=True)
            with requests.get('https://huggingface.co/Sams200/opus-mt-en-zh/resolve/main/'+name,stream=True,timeout=180) as r:
                r.raise_for_status()
                with target.with_suffix(target.suffix+'.part').open('wb') as out:
                    for chunk in r.iter_content(1024*1024):out.write(chunk)
            target.with_suffix(target.suffix+'.part').replace(target)
    src=sentencepiece.SentencePieceProcessor(model_file=str(folder/'source.spm'))
    dst=sentencepiece.SentencePieceProcessor(model_file=str(folder/'target.spm'))
    engine=ctranslate2.Translator(str(folder),device='cpu',compute_type='int8',intra_threads=3)
    def translate(texts):
        tokens=[src.encode(text,out_type=str)+['</s>'] for text in texts]
        results=engine.translate_batch(tokens,beam_size=2,max_batch_size=16,max_decoding_length=180)
        return [dst.decode(r.hypotheses[0]) for r in results]
    return translate

def split(segments):
    rows=[]
    for segment in segments:
        words=segment['words'];group=[]
        for index,word in enumerate(words):
            if word['end']<word['start']:continue
            group.append(word)
            terminal=re.search(r'[.!?]["\u201d\u2019\']?$',word['word'].strip())
            next_gap=index+1<len(words) and words[index+1]['start']-word['end']>.65
            following=words[index+1]["word"].strip() if index+1<len(words) else ""
            if terminal and abbreviation_continues("".join(w["word"] for w in group),following):terminal=False
            if terminal or next_gap or len(group)>=34 or index==len(words)-1:
                text=''.join(w['word'] for w in group).strip()
                if text:rows.append(dict(speaker='听力原声',actor='',text=text,translation='',start=round(group[0]['start'],3),end=round(group[-1]['end']+.06,3)))
                group=[]
        if group:rows.append(dict(speaker='听力原声',actor='',text=''.join(w['word'] for w in group).strip(),translation='',start=round(group[0]['start'],3),end=round(group[-1]['end']+.06,3)))
    for i,row in enumerate(rows[:-1]):row['end']=min(row['end'],rows[i+1]['start'])
    return [r for r in rows if r['end']>r['start']]

def main():
    parser=argparse.ArgumentParser();parser.add_argument('--limit',type=int,default=0);parser.add_argument('--part',type=int,default=0);parser.add_argument('--parts',type=int,default=1);parser.add_argument('--threads',type=int,default=4);args=parser.parse_args()
    folder=ROOT/'assets/tavern/listening/real-exams/cettong'
    manifest=json.loads((folder/'manifest.json').read_text(encoding='utf-8'))
    entries=[e for e in manifest['entries'] if e['status']=='downloaded']
    entries.sort(key=lambda e:(-int(e['exam_id'].split('_')[0]),e['level'],e['exam_id']),reverse=False)
    entries=entries[args.part::args.parts]
    if args.limit:entries=entries[:args.limit]
    translate=translator()
    model=WhisperModel('base.en',device='cpu',compute_type='int8',download_root=str(ROOT/'.validation/asr-models'),cpu_threads=args.threads)
    pipeline=BatchedInferencePipeline(model)
    for count,entry in enumerate(entries,1):
        original=ROOT/entry['files'][0]['path'];out=original.with_suffix('.captions.json')
        resources=original.with_suffix('.resources.json')
        if not resources.exists():print('Waiting for source '+entry['exam_id'],flush=True);continue
        reference=json.loads(resources.read_text(encoding='utf-8'));audio=ROOT/reference['audio']
        if out.exists() and json.loads(out.read_text(encoding='utf-8')).get('material_source')==reference['material_source']:print('Ready '+entry['level']+'/'+entry['exam_id'],flush=True);continue
        raw=ROOT/'.validation'/('aligned-asr-'+entry['level']+'-'+entry['exam_id']+'.json')
        print('Transcribing '+entry['level']+'/'+entry['exam_id']+' '+str(count)+'/'+str(len(entries)),flush=True);started=time.monotonic()
        if not raw.exists() and not reference['source_sentences']:
            benchmark=ROOT/'.validation/cet6-2025-asr.json'
            if entry['level']=='cet6' and entry['exam_id']=='2025_12_1' and benchmark.exists() and audio==original:
                raw.write_bytes(benchmark.read_bytes())
            else:
                segments,info=pipeline.transcribe(str(audio),language='en',word_timestamps=True,beam_size=1,batch_size=2,condition_on_previous_text=False)
                data=[dict(start=s.start,end=s.end,text=s.text,words=[dict(word=w.word,start=w.start,end=w.end) for w in s.words]) for s in segments]
                raw.write_text(json.dumps(data,ensure_ascii=False),encoding='utf-8')
        if reference['source_sentences']:
            rows=[dict(speaker='听力原声',actor='',text=r['text'],translation='',start=r['start'],end=r['end']) for r in reference['source_sentences'] if r['end']>r['start'] and r.get('text')]
        else:
            rows=align_materials(reference,json.loads(raw.read_text(encoding='utf-8')))
        # Begin with the first real material rather than transcribed exam boilerplate.
        first=min(g['start'] for g in reference['groups'])
        rows=attach_verified_questions(reference,rows)
        rows=merge_abbreviation_rows([r for r in rows if r['start']>=first-.5])
        if any(re.fullmatch(r'(?:Question\s*)?\d+[.:]?',r['text'].strip(),re.I) for r in rows):
            raise ValueError('Standalone spoken number remains: '+reference['id'])
        assert sorted(r['questionNumber'] for r in rows if r.get('questionNumber'))==list(range(1,26))
        published=original.with_suffix('.translations.json')
        native=json.loads(published.read_text(encoding='utf-8')) if published.exists() else dict(sentences={},source='')
        for row in rows:
            exact=native['sentences'].get(re.sub(r'[^a-z0-9]','',row['text'].lower()))
            if exact:row['translation']=exact;row['translation_source']=native['source']
        missing=[r for r in rows if not r['translation']]
        for at in range(0,len(missing),16):
            batch=missing[at:at+16]
            for row,zh in zip(batch,translate([r['text'] for r in batch])):row['translation']=zh
        assert rows and all(r['translation'] for r in rows)
        doc=dict(id=entry['level']+'-'+entry['exam_id'],title=entry['title'],level=int(entry['level'][-1]),source_url=entry['page_url'],material_source=reference['material_source'],audio=reference['audio'],caption_kind='published_material_with_auxiliary_timing_and_translation',caption_models=['Systran/faster-whisper-base.en','Sams200/opus-mt-en-zh'],lines=rows)
        out.write_text(json.dumps(doc,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        import runpy
        runpy.run_path(str(ROOT/'tools/refine-listening-replay-ends.py'))['refine'](out)
        print('Completed '+entry['level']+'/'+entry['exam_id']+' '+str(len(rows))+' lines in '+str(round(time.monotonic()-started,1))+'s',flush=True)

def align_materials(reference,segments):
    # Published original text supplies every material sentence. ASR only locates words.
    rows=[];covered=[]
    words=[w for s in segments for w in s['words']]
    norm=lambda s:re.sub(r'[^a-z0-9]','',s.lower())
    for group in reference['groups']:
        material=re.sub(r'(?m)^\s*(?:M|W|Man|Woman|Speaker \d+)\s*:\s*','',group['material']).strip()
        if not material:continue
        parts=sentences(material)
        source=[(token,i) for i,s in enumerate(parts) for token in s.split()]
        target=[w for w in words if group['start']-.5<=w['start']<group['end']]
        matches={}
        for block in difflib.SequenceMatcher(None,[norm(t) for t,i in source],[norm(w['word']) for w in target],autojunk=False).get_matching_blocks():
            for offset in range(block.size):matches[block.a+offset]=target[block.b+offset]
        if not matches:raise ValueError('Cannot align published material: '+reference['id']+' '+group['id'])
        # Interpolate unmatched words between reliable anchors, including proper names.
        anchors=sorted(matches)
        for pos,(token,sentence) in enumerate(source):
            if pos in matches:continue
            before=max((a for a in anchors if a<pos),default=-1);after=min((a for a in anchors if a>pos),default=len(source))
            left=matches[before]['end'] if before>=0 else group['start']
            right=matches[after]['start'] if after<len(source) else group['end']
            span=max(0,right-left)/(after-before-1)
            start=left+span*(pos-before-1);matches[pos]=dict(start=start,end=start+span)
        for i,sentence in enumerate(parts):
            positions=[pos for pos,(_,si) in enumerate(source) if si==i]
            start=matches[positions[0]]['start'];end=matches[positions[-1]]['end']
            if end>start:rows.append(dict(speaker='听力原声',actor='',text=sentence,translation='',start=round(start,3),end=round(end,3)))
        covered.append((group['start'],rows[-1]['end']))
    # Keep the actual spoken questions as well; never synthesize their answers.
    for row in split(segments):
        if not any(start<=row['start']<end for start,end in covered):rows.append(row)
    rows.sort(key=lambda r:r['start'])
    for i,row in enumerate(rows[:-1]):row['end']=min(row['end'],rows[i+1]['start'])
    return [r for r in rows if r['end']>r['start']]

if __name__=='__main__':main()
