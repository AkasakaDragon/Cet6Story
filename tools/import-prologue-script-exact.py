"""Import every original prologue turn without rewriting its Chinese text."""
from pathlib import Path
import json,re,sys,hashlib
ROOT=Path(__file__).resolve().parent.parent
SRC=Path(sys.argv[1]);raw=SRC.read_text(encoding='utf-8-sig')
part=raw.split('# 序幕：雨里的一盏灯',1)[1].split('# 第一章：',1)[0]
original=[]
for match in re.finditer(r'\*\*(序-(\d+))｜([^：]+)：\*\*\s*([^\r\n]+)',part):
 original.append((match[1],int(match[2]),match[3],match[4]))
assert len(original)==123
english={int(s.split('|',1)[0]):s.split('|',1)[1] for s in (ROOT/'tools/prologue-script-english.txt').read_text(encoding='utf-8').splitlines() if s.strip()}
assert len(english)==123
chapter=json.loads((ROOT/'chapters/10-tavern-prologue.json').read_text(encoding='utf-8-sig'))
lines=[]
expressions={16:'surprised',18:'confused',20:'surprised',22:'sad',24:'confused',26:'confused',28:'confused',30:'serious',33:'serious',36:'confused',39:'confused',43:'confused',46:'serious',48:'serious',50:'confused',52:'confused',54:'serious',56:'happy',58:'serious',60:'surprised',62:'confused',64:'serious',66:'happy',68:'serious',70:'serious',73:'confused',75:'serious',76:'surprised',77:'serious',78:'serious',79:'serious',80:'serious',81:'serious',83:'surprised',85:'confused',87:'serious',89:'happy',91:'confused',93:'confused',95:'serious',97:'serious',100:'confused',102:'confused',104:'confused',110:'happy',112:'confused',114:'serious',116:'confused',118:'happy',120:'happy',122:'happy'}
history={n:'history01' if n<=33 else 'history02' if n<=36 else 'history03' if n<=37 else 'history04' if n<=40 else 'history05' for n in range(31,46)}
cg={11:'cg02',16:'cg03',72:'cg06',75:'cg04',95:'cg05'}
for script_id,n,actor,zh in original:
 scene='dorm' if n<16 else 'boundary' if n<72 else 'ruined' if n<99 else 'patched' if n<116 else 'morning'
 caption='原世界 · 深夜 · 下雨' if n<16 else '交界空间 · 时间未明 · 无天气' if n<72 else '归灯驿站 · 第1天 · 晚上 · 下雨' if n<116 else '归灯驿站 · 第2天 · 早上 · 小雨未停'
 phase='night' if n<16 or 72<=n<116 else 'morning' if n>=116 else ''
 who={'陆川':'luchuan','艾莉娅':'aelia','伊瑟雅':'iserya'}.get(actor,'')
 archive=n in history
 hidden=n<16 or not who or archive
 line=dict(scriptId=script_id,speaker=actor,actor=who,voiceRole='室友消息' if actor=='室友' else actor if actor in ('陆川','伊瑟雅','艾莉娅') else '旁白',text=english[n],translation=zh,
 scene='art/prologue-v3/'+history[n]+'.png' if archive else 'art/prologue-v3/'+scene+'.png' if scene.endswith('-script') else 'art/prologue-v2/'+scene+'.png',
 sceneSingle=True,sceneCaption=caption,timeOfDay=phase,portraitIds=[] if hidden else [who],hidePortraits=hidden,archiveScene=archive,expression=expressions.get(n,'serious' if who else 'neutral'),start=0,end=0)
 # The screen's English and Chinese are independent original cues, shown once each.
 if n==11:line['translation']=next(t for _,num,_,t in original if num==12);line['text']=zh
 if n==12:line['text']='';line['skipVoice']=True
 if n in cg:line['cgBefore']=cg[n]
 lines.append(line)
 if n==10:
  lead=dict(line);lead.update(scriptId="序-010-补",speaker="陆川",actor="luchuan",voiceRole="陆川",text="Wait, what is that on my screen?",translation="等等，我屏幕上的是什么？",expression="confused");lines.append(lead)
# Latest seven-shot revisions supersede the original wording for these cues.
lines=[l for l in lines if l['scriptId'] not in {'序-012','序-033','序-036'}]
for l in lines:
 if l['scriptId']=='序-034':l['text']=l['text'].removeprefix('Yes. ');l['translation']=l['translation'].removeprefix('是。')
 if l['scriptId']=='序-058':l['translation']=''
pos=next(i for i,l in enumerate(lines) if l.get('cgBefore')=='cg06')
lead=dict(lines[pos-1]);lead.update(scriptId='序-071-补',speaker='陆川',actor='luchuan',voiceRole='陆川',text='Come on. Through this door. There is nothing to be afraid of.',translation='来吧，穿过这扇门，没什么好怕的。',expression='serious',hidePortraits=False,portraitIds=['luchuan']);lead.pop('cgBefore',None);lines.insert(pos,lead)
chapter.update(lines=lines,audio='audio/tavern/prologue-script-exact.wav',description='剧本原文123轮 · 五页历史档案CG · 罗盘归途约定 · 应急修缮 · 战斗场景暂跳',source='严格读取2026-10-13修订合订本序幕原文；英文逐轮翻译。仅战斗场景按用户要求暂跳，保留战斗短对白。',ending='第2天早上，小雨未停。应急修缮完成，屋面、厨房、净水、楼梯和客房仍须检查。热早餐的约定继续。')
(ROOT/'chapters/10-tavern-prologue.json').write_text(json.dumps(chapter,ensure_ascii=False,indent=2),encoding='utf-8')
out=ROOT/'项目文档/序章重制交付';out.mkdir(exist_ok=True)
(out/'原文导入映射.json').write_text(json.dumps({'source':str(SRC),'sha256':hashlib.sha256(SRC.read_bytes()).hexdigest(),'turns':[dict(id=s,actor=a,original=t,gameLine=n) for s,n,a,t in original],'history':history,'battleDeferred':['序-080','序-081']},ensure_ascii=False,indent=2),encoding='utf-8')
print('Imported all123 original turns; 15 historical dialogue turns on5 archive CG plates.')
