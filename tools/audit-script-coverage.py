"""Read-only comparison of the supplied Chinese script and playable story text."""
from pathlib import Path
import re,json,difflib,sys
ROOT=Path(__file__).resolve().parent.parent
source=Path(sys.argv[1]);text=source.read_text(encoding='utf-8-sig')
out=ROOT/'项目文档/剧本核对';out.mkdir(exist_ok=True)
lines=[];cgs=[];section=''
for n,s in enumerate(text.splitlines(),1):
 if s.startswith('## '):section=s[3:]
 m=re.match(r'\*\*([序一二三四五六七]-\d+)｜([^：]+)：\*\*\s*(.*)',s)
 if m:lines.append(dict(id=m[1],actor=m[2],text=m[3],section=section,sourceLine=n))
 m=re.match(r'【(CG-[^】]+)】(.*)',s)
 if m:cgs.append(dict(id=m[1],text=m[2],section=section,sourceLine=n))
chapters=[];pool=[]
for p in sorted((ROOT/'chapters').glob('*tavern*.json')):
 c=json.loads(p.read_text(encoding='utf-8-sig'));chapters.append(c)
 for n,l in enumerate(c['lines'],1):pool.append(dict(file=p.name,chapter=c['title'],line=n,actor=l.get('speaker',''),text=l.get('translation',''),audio=c.get('audio',''),start=l.get('start'),end=l.get('end')))
def norm(s):return re.sub(r'[\W_]+','',s)
for l in lines:
 candidates=[p for p in pool if (l['id'].startswith('序-'))==(p['file']=='10-tavern-prologue.json')]
 exact=next((p for p in candidates if norm(p['text'])==norm(l['text'])),None)
 l['status']='原文完整出现' if exact else '原文未逐句接入（当前版本改写）' if l['id'][0] in '序一' else '章节未接入'
 if exact:l['match']=exact
 elif l['id'][0] in '序一':
  nearest=max(candidates,key=lambda p:difflib.SequenceMatcher(None,norm(l['text']),norm(p['text'])).ratio())
  ratio=difflib.SequenceMatcher(None,norm(l['text']),norm(nearest['text'])).ratio()
  if ratio>=.38:l['candidate']=nearest;l['similarity']=round(ratio,3)
reasons={
 'CG-序-01':'部分对应：cg01复习书桌、cg02屏幕石门及cg03开头白光。未按剧本呈现陆川现实侧脸、屏幕中的窗与墙面矛盾；现版屏幕句子改写。穿越前不展示立绘遵循用户此前要求。',
 'CG-序-02':'未体现：缺古庭检修刻度、被破坏村庄、赛维恩铸石、五武器剪影、遗迹封门这五段历史镜头；现版cg03是男主抵达交界空间。',
 'CG-序-03':'未完整体现：cg06有走近石门、开门、白光与雨夜落地，但没有女神把罗盘交到陆川手中的镜头，也没有白光退成雨滴特写。cg06按用户新增要求制作。',
 'CG-序-04':'未完整体现：当前cg05是男主清理木板、女主点火（用户最新指定）；缺炉边休息中景、天亮手套、接雨桶、修缮清单的连续镜头。次日有无人物背景和对白，不能代替这组CG。',
 'CG-一-01':'部分对应：旧第一节outro含收采购清单、石桥出发；未确认两张清单与村口石渠水声衔接，现版前往集市，剧本前往村庄。',
 'CG-一-02':'未体现：现版第一章旧矿道遭遇，不是裂牙獠王獠牙卡水轮、三人侧沟站位的BOSS镜头。',
 'CG-一-03':'未完整体现：旧准备酒馆CG/背景不构成次日木匠检梁、陆川搬料、艾莉娅核通道、莉瑟照顾老人的工作镜头组。',
 'CG-一-04':'部分对应：旧第一次营业CG有开门与服务主题；没有按本版三人试菜、门牌翻面、第一碗汤落桌逐镜制作。',
 'CG-一-05':'未体现：现版旧矿道碎晶/营业晶石剧情，不是三人围未启动石台、抄本和法典、空金座无宝石镜头。',
}
for cg in cgs:cg['status']=reasons.get(cg['id'],'未接入：第二至第七章暂无此新版剧情的可播放章节、CG触发或对应镜头。')
report=['# 剧本逐句与CG核对（2026-10-13）','',f'核对原文件：`{source}`。仅核对、列出差异，本次没有擅自重写对白或制作缺失CG。','',
 '## 结论','',
 '当前游戏不是这份修订合订本的逐句实现。可播放奇幻主线为114段重制序章及旧第一章六节；旧第一章六节不是新剧本第二至第七章。英文配音匹配当前JSON对白，不代表匹配附件原句。第二至第七章尚未接入；两个归途结局也未按本版实现。','',
 '原文匹配采用去掉标点和空格后的整轮文字核对，避免将主题相似当作完整接入。原文未匹配不等于相应剧情全部消失：序章/第一章有改写或缩写；下方保留相近游戏句供复核，但相近句不是完整实现。','',
 '| 章节 | 剧本发言轮次 | 原文完整匹配 | 未逐句接入 | CG设计数 |','|---|---:|---:|---:|---:|']
for prefix,name in zip('序一二三四五六七',['序幕','第一章','第二章','第三章','第四章','第五章','第六章','第七章']):
 group=[l for l in lines if l['id'][0]==prefix];count=sum('match'in l for l in group);cgcount=sum(c['id'].startswith('CG-'+prefix+'-') for c in cgs)
 report.append(f'| {name} | {len(group)} | {count} | {len(group)-count} | {cgcount} |')
report+=['','## 序幕重点遗漏与改写','',
 '| 剧本编号 | 与当前游戏的差异 |','|---|---|',
 '| 序-007～010 | 包子钱“明早给”、月底只等到月亮的互动没有保留，现版改为关窗、室友入睡和电脑闪动。 |',
 '| 序-011～015 | 指定英文屏幕句及中文字幕没有出现；窗倒影与现实墙面的矛盾改成电脑中的石门。 |',
 '| 序-031～051 | 女神历史说明被压缩；无声之庭的五地协调、王座单向抽取力量、军队后勤、赛维恩安置后去世、替代封锁后才能取宝石、女神不能进入核心等没有完整逐句说明。 |',
 '| 序-057～061 | 指定空杯练习、原咒句、击碎杯沿、赔杯玩笑缺失；现版改为指尖雷光及另一句咒语。 |',
 '| 序-062～065 | “雷电不是第六颗宝石”、短时同步必须持有者配合、不能支配伙伴这段规则没有完整呈现。 |',
 '| 序-067～071 | 罗盘指引、正式归途约定的原对白与CG字幕缺失；现版是屋檐/巡查路口提示后石门传送。 |',
 '| 序-072～081 | 入场顺序、怪物身份和战斗短指令不同：棘背狼改为灰背裂牙兽；实际战斗按用户要求暂跳。 |',
 '| 序-082～094 | 战后身体检查、断椅/炉边石阶、追狼调查和黑泥线索改写；现版用巡查、手部检查和旧档案核实承接。 |',
 '| 序-095～109 | 抢修被改写/缩减；先撑梁、油布、清烟道、失踪巡逻队、修楼梯第二/第五级、清一间客房与晾床褥没有完整演出。现版楼梯仍封闭，客房尚未检查。 |',
 '| 序-110～123 | 让房间、分毯子、半块面包、补热早餐的约定及最后CG字幕没有逐句保留；现版改为轮流守夜、次晨吃干粮后去村庄。 |','']
report+=['## 所有CG逐项核对','']
for cg in cgs:report +=[f"### {cg['id']}（原文件第{cg['sourceLine']}行）",'',cg['text'],'',f"**核对：** {cg['status']}",'']
report+=['## 每一轮对白、旁白、屏幕文字核对','', '以下逐条列出全部编号，不把未接入内容省略成一句总括。','']
for l in lines:
 report +=[f"### {l['id']}｜{l['actor']}（原文件第{l['sourceLine']}行）",'',f"**状态：** {l['status']}",f"**剧本原文：** {l['text']}"]
 if 'match'in l:
  p=l['match'];report +=[f"**游戏位置：** {p['file']} 第{p['line']}段；英文音轨 `{p['audio']}` {p['start']}～{p['end']}秒。"]
 if 'candidate'in l:
  p=l['candidate'];report +=[f"**相近但不相同的游戏句：** {p['file']} 第{p['line']}段：{p['text']}"]
 report+=['']
report+=['## 其他演出差异','',
 '- 序幕战斗按用户要求暂时跳过，因此战斗中的对白、吟唱和动作并没有逐项演出；属于已授权延期，不能计为完整实现。',
 '- 最新剧本的二十项支线请求及任务栏落点，尚未按新版完整接入。已有经营/任务功能不能视为这二十项支线已实现。',
 '- 最新剧本第七章留下/返回两个分支尚未接入。现版第一章旧“留宿后日谈”不等于第七章归途分支。',
 '- 现版新增cg04女骑士门口进入、cg05清理木板/点火、cg06步行石门传送遵循用户后续具体要求；这些镜头与附件原CG规划存在差异，已在上面标出。',
 '- 本次对白核对为静态资源及触发结构核对，雨声修复另做运行回归。没有把未播放的音轨或未绑定的图片计作完整演出。','']
(out/'逐句与CG差异清单.md').write_text('\n'.join(report),encoding='utf-8')
(out/'核对数据.json').write_text(json.dumps(dict(source=str(source),playableChapters=[dict(id=c['id'],title=c['title'],lines=len(c['lines'])) for c in chapters],lines=lines,cgs=cgs),ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({p:dict(lines=sum(l['id'][0]==p for l in lines),exact=sum(l['id'][0]==p and 'match'in l for l in lines),CG=sum(c['id'].startswith('CG-'+p+'-') for c in cgs)) for p in '序一二三四五六七'},ensure_ascii=False))
print('Report:',out/'逐句与CG差异清单.md')
