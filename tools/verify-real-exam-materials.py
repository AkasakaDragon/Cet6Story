"""Check playable exam resources, question integrity and sentence timing."""
import json,argparse,subprocess,concurrent.futures
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
FOLDER=ROOT/'assets/tavern/listening/real-exams/cettong'
manifest=json.loads((FOLDER/'manifest.json').read_text(encoding='utf-8'))
exams=[e for e in manifest['entries'] if e['status']=='downloaded']
parser=argparse.ArgumentParser();parser.add_argument('--available',action='store_true');parser.add_argument('--decode',action='store_true');args=parser.parse_args()
total_lines=total_questions=checked=0
playback=[]
for exam in exams:
    audio=ROOT/exam['files'][0]['path']
    if args.available and (not audio.with_suffix('.resources.json').exists() or not audio.with_suffix('.captions.json').exists()):continue
    source=json.loads(audio.with_suffix('.resources.json').read_text(encoding='utf-8'))
    captions=json.loads(audio.with_suffix('.captions.json').read_text(encoding='utf-8'))
    assert source['id']==captions['id'] and source['audio']==captions['audio']
    assert (ROOT/captions['audio']).is_file()
    assert len(source['questions'])==25
    assert sorted(q['number'] for q in source['questions'])==list(range(1,26))
    for q in source['questions']:
        assert len(q['options'])==4 and 0<=q['answer']<4
        assert q['prompt'] and q['explanation'] and q['end']>q['start']
    assert len(source['groups']) in (7,8)
    lines=captions['lines'];assert lines
    for i,line in enumerate(lines):
        assert line['text'].strip() and line['translation'].strip()
        assert 0<=line['start']<line['end']
        if i:assert line['start']>=lines[i-1]['end']-.001,source['id']+' overlap'
    for group in source['groups']:
        assert any(group['start']-.5<=line['start']<group['end'] for line in lines),source['id']+' empty group'
    total_lines+=len(lines);total_questions+=len(source['questions']);checked+=1
    playback.append((ROOT/captions['audio'],max(r['end'] for r in lines)))
if args.decode:
    def decode(item):
        file,last=item
        result=subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'),'-nostdin','-hide_banner','-loglevel','error','-xerror','-i',str(file),'-progress','pipe:1','-f','null','-'],capture_output=True,text=True,check=True,creationflags=subprocess.CREATE_NO_WINDOW)
        times=[int(line.split('=',1)[1]) for line in result.stdout.splitlines() if line.startswith('out_time_us=')]
        assert times and last<=max(times)/1000000+.3,str(file)+' captions exceed audio'
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(decode,playback))
print(f'PASS: {checked}/{len(exams)} exams, {total_questions} questions, {total_lines} bilingual timed sentences; audio paths, answers, grouping and order checked.')
if args.decode:print('PASS: every playback file fully decoded and caption end times fit the recording.')
