import asyncio,json,wave,subprocess,urllib.request,hashlib
from pathlib import Path
import edge_tts
R=Path(__file__).resolve().parent.parent
C=R/'chapters/16-tavern-01-06.json'; O=R/'chapters/audio/tavern/chapter-six'; cache=R/'.validation/opening-meeting-voices';cache.mkdir(exist_ok=True,parents=True)
changes={12:('陆川','今天我先负责厨房，把汤热好，准备面包。莉瑟，你负责吧台记账和记录订单；艾莉娅，你负责传菜。客人的忌口先告诉我，做好的餐我会放到出餐处。','Today I will handle the kitchen, reheat the soup, and prepare the bread. Lyse, please keep the accounts and record orders at the bar. Aelia, please serve the food. Tell me about any dietary restrictions first, and I will put the finished dishes at the serving counter.'),13:('莉瑟','明白，我负责吧台记账和记单。桌号、份数、特殊要求，还有是否打包，逐项写清，再把订单交给你。','Understood. I will keep the accounts and record orders at the bar: table numbers, portions, special requests, and whether the food is to go. Then I will pass each order to you.'),14:('艾莉娅','那我负责传菜，从厨房把做好的餐端到桌边。端走前核对桌号和要求，送到以后再确认一次，空碗也由我带回来。','Then I will serve the food, carrying the finished dishes from the kitchen to the tables. I will check the table number and requests before leaving, confirm them again when serving, and bring back the empty bowls.'),17:('陆川','好。我先去厨房准备，忙的时候我们再互相补位。今晚先把这几张桌子照顾好，账本等客人吃完再算。大家换上方便工作的衣服吧。','Good. I will get the kitchen ready, and we can help each other when needed. Tonight, let us take care of these tables first. We can settle the accounts after the guests have eaten. Everyone, let us change into clothes that are comfortable to work in.')}
async def main():
 c=json.loads(C.read_text(encoding='utf-8-sig'));old=json.loads(C.read_text(encoding='utf-8-sig'));profiles=json.loads((O/'voices.json').read_text(encoding='utf-8-sig'))
 async def render(i,row):
  role,zh,en=row;spec=profiles[role];key=hashlib.sha256((en+json.dumps(spec)).encode()).hexdigest()[:20];mp3=cache/(key+'.mp3');wav=cache/(key+'.wav')
  if not wav.exists():
   await edge_tts.Communicate(en,**spec,proxy=urllib.request.getproxies().get('https'),receive_timeout=45).save(str(mp3))
   subprocess.run([str(R/'tools/ffmpeg/ffmpeg.exe'),'-nostdin','-loglevel','error','-y','-i',str(mp3),'-ar','24000','-ac','1','-c:a','pcm_s16le',str(wav)],check=True)
  with wave.open(str(wav),'rb') as r:return i,r.readframes(r.getnframes())
 data=dict(await asyncio.gather(*(render(i,row) for i,row in changes.items())))
 with wave.open(str(O/'story.wav'),'rb') as r:params=r.getparams();pcm=r.readframes(r.getnframes())
 out=bytearray();cursor=0
 for i,line in enumerate(c['lines']):
  start=round(old['lines'][i]['start']*24000);end=round(old['lines'][i]['end']*24000)
  out.extend(pcm[cursor*2:start*2]);line['start']=len(out)/2/24000
  out.extend(data[i] if i in data else pcm[start*2:end*2]);line['end']=len(out)/2/24000;cursor=end
  if i in changes:
   role,zh,en=changes[i];line.update(speaker=role,voiceRole=role,actor={'陆川':'luchuan','艾莉娅':'aelia','莉瑟':'lyse'}[role],text=en,translation=zh)
 out.extend(pcm[cursor*2:])
 with wave.open(str(O/'story.wav'),'wb') as w:w.setparams(params);w.writeframes(out)
 C.write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 p=R/'tools/chapter-one-continuation.json';sections=json.loads(p.read_text(encoding='utf-8-sig'))
 for i,row in changes.items():sections[3]['dialogue'][i]=list(row)
 p.write_text(json.dumps(sections,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 p=R/'tools/insert-opening-meeting.py';s=p.read_text(encoding='utf-8-sig');a=s.index('DIALOGUE=[');b=s.index('async def main():',a);s=s[:a]+'DIALOGUE='+repr([tuple(sections[3]['dialogue'][i]) for i in range(6,18)])+'\n'+s[b:];p.write_text(s,encoding='utf-8')
 for i,l in enumerate(c['lines']):
  if i not in changes:
   a,b=round(l['start']*24000)*2,round(l['end']*24000)*2;x,y=round(old['lines'][i]['start']*24000)*2,round(old['lines'][i]['end']*24000)*2
   assert out[a:b]==pcm[x:y]
 assert len(c['lines'])==len(old['lines']) and c['questions']==old['questions']
 print('PASS: 4 bilingual lines and matching character voices updated; all other audio and quiz positions preserved')
asyncio.run(main())
