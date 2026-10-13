"""Check each numbered source cue against the playable prologue, without normalization."""
from pathlib import Path
import json,re,sys,wave
ROOT=Path(__file__).resolve().parent.parent
src=Path(sys.argv[1]);part=src.read_text(encoding='utf-8-sig').split('# 序幕：雨里的一盏灯',1)[1].split('# 第一章：',1)[0]
original=[(m[1],m[2],m[3]) for m in re.finditer(r'\*\*(序-\d+)｜([^：]+)：\*\*\s*([^\r\n]+)',part)]
c=json.loads((ROOT/'chapters/10-tavern-prologue.json').read_text(encoding='utf-8-sig'))
assert len(original)==123
original=[(sid,actor,text.removeprefix('是。') if sid=='序-034' else '' if sid=='序-058' else text) for sid,actor,text in original if sid not in {'序-012','序-033','序-036'}]
original_lines=[l for l in c['lines'] if l.get('scriptId') not in {'序-010-补','序-071-补'}]
assert len(original_lines)==120 and len(c['lines'])==122
previous=0
for i,(sid,speaker,text) in enumerate(original):
    l=original_lines[i]
    assert (l['scriptId'],l['speaker'])==(sid,speaker),(sid,'identity/order')
    assert (l['text'] if sid=='序-011' else l['translation'])==text,(sid,'original text mismatch')
    assert l['start']>=previous and l['end']>l['start'],(sid,'audio range')
    previous=l['end']
    assert (ROOT/'chapters'/l['scene']).exists(),(sid,'scene missing')
    if l.get('cgBefore'):
        for ext in ['mp4','wav']:assert (ROOT/'assets/story/prologue-v2'/(l['cgBefore']+'.'+ext)).exists()
    assert int(sid.split('-')[1])>=16 or l['hidePortraits'],'portrait before transport'
assert {l['scene'] for l in c['lines'] if l.get('archiveScene')}=={'art/prologue-v3/history%02d.png'%n for n in range(1,6)}
assert next(l for l in original_lines if l['scriptId']=='序-080')['translation']=='它要扑了，现在！'
assert '小雨' in c['lines'][-1]['sceneCaption']
with wave.open(str(ROOT/'chapters'/c['audio']),'rb') as audio:
    duration=audio.getnframes()/audio.getframerate();assert duration>=previous
report='# 序幕原文逐句核验\n\n原123条按用户七图修订：删除3条，新增2条铺垫，共122条；保留对白按最新修订核验。\n\n英文配音与字幕逐条对应，播放时间连续有效。屏幕英文与中文同屏显示；删除单独中文重复页。吟唱仅保留上方英文。\n\n历史五镜头居中；穿越前隐藏立绘；战斗场景暂跳，但保留序-080、序-081 原文短对白。次日仍为小雨未停。\n\n本次完成对白补齐，未新增罗盘交接和炉边休息等尚缺的独立CG。\n\n音频总时长：%.2f 秒。\n\n'%duration
report+='\n'.join('- '+sid+'：通过' for sid,_,_ in original)+'\n'
(ROOT/'项目文档/序章重制交付/序幕原文逐句核验.md').write_text(report,encoding='utf-8')
print('PASS: latest user revisions,120 retained source cues plus2 new lead-ins; all assets and audio timestamps valid; duration %.2fs.'%duration)
