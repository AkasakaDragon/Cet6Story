"""Insert opening discussion; preserve all existing dialogue and PCM samples."""
import asyncio, hashlib, json, shutil, subprocess, sys, urllib.request, wave
from pathlib import Path
ROOT=Path(__file__).resolve().parent.parent
sys.path.insert(0,str(ROOT/'.validation/tts-deps'))
import edge_tts
CHAPTER=ROOT/'chapters/16-tavern-01-06.json'
OUT=ROOT/'chapters/audio/tavern/chapter-six'
BACKUP=ROOT/'.git/audio-backups/opening-meeting'
CACHE=ROOT/'.validation/opening-meeting-voices'
SCENE='art/tavern/chapter-six-opening-meeting.png'
DIALOGUE=[
 ('旁白','开门之前，三人在桌边坐下。陆川将空白订单簿摊开，先说明自己为什么要经营这间酒馆。','Before opening, the three sit around a table. Lu Chuan opens the blank order book and explains why he needs to run the tavern.'),
 ('陆川','女神把我送到这里，是因为这间驿站的晶石维系着地面封印。地下的魔王还被困着，可封印已经开始衰弱。','The goddess sent me here because this waystation\'s crystal maintains the seal above ground. The demon king is still trapped below, but the seal is weakening.'),
 ('陆川','她说，人们在这里休息、交谈、互相帮助，形成的共鸣能为晶石供能。找回碎片是修复结构，开门营业则是维持能量。','She said that people resting, talking, and helping one another here create resonance that supplies the crystal with energy. Fragments repair its structure; running the tavern maintains its power.'),
 ('艾莉娅','明白了。我们不能只找碎片，也不能只把桌子摆好。要让这里真正成为有人愿意停下来的地方。','I understand. We cannot just collect fragments or arrange the tables. This needs to become a place where people actually want to stop and rest.'),
 ('陆川','对。封印稳定以后，女神才能为我打开安全的归途。在那之前，我也希望路过的人能在这里安心吃顿饭。','Exactly. Once the seal is stable, the goddess can open a safe route home for me. Until then, I want travelers to be able to enjoy a meal here without fear.'),
 ('莉瑟','我也明白了。让旅人聚在一起，同时守住镇子的安全……今晚我愿意帮忙。晶石的线索，我们以后继续查。','I understand too. Bringing travelers together also helps keep the town safe. I would like to help tonight, and we can continue investigating the crystal afterward.'),
 ('艾莉娅','那我负责厨房。先把汤重新热好，准备面包；客人有忌口，先告诉我。做好的餐放到出餐处，再叫你来端。','Then I will handle the kitchen. I will reheat the soup and prepare the bread. Tell me about any dietary restrictions first. When the food is ready, I will place it at the serving counter and call you.'),
 ('莉瑟','我在吧台记单。桌号、份数、特殊要求，还有是否打包，逐项写清，再把订单交给厨房。','I will record orders at the bar: the table number, portions, special requests, and whether the food is to go. Then I will pass each order to the kitchen.'),
 ('陆川','我负责从厨房端菜到桌边。端走前核对桌号和要求，送到以后再确认一次，空碗也由我带回来。','I will carry the finished food from the kitchen to the tables. I will check the table number and requests before leaving, confirm them again when serving, and bring back the empty bowls.'),
 ('艾莉娅','顺序就是：吧台记单，厨房备餐，出餐后送到对应的桌子。门口和厨房通道都不能堆东西；忙不过来就先说。','The sequence is simple: record the order at the bar, prepare it in the kitchen, and serve it at the correct table. Keep the entrance and kitchen passage clear. Speak up if you need help.'),
 ('莉瑟','记住了。我先记清订单；忙的时候可以帮忙打包，但不会把还没核对的单子留在一边。','Understood. I will record each order carefully. When it gets busy, I can help pack the bread, but I will not leave an unchecked order behind.'),
 ('陆川','好。我先端汤，忙的时候我们再互相补位。今晚先把这几张桌子照顾好，账本等客人吃完再算。','Good. I will start by serving the soup, and we can help each other when needed. Tonight, let us take care of these tables first. We can settle the accounts after the guests have eaten.')]
async def main():
 c=json.loads(CHAPTER.read_text(encoding='utf-8-sig'))
 if any(l.get('scene')==SCENE for l in c['lines']):
  print('Meeting already inserted; unchanged.');return
 at=next(i for i,l in enumerate(c['lines']) if l['translation']=='光蝶不再绕着这个缺口打转了。至少今天，我们找对了一块。')+1
 BACKUP.mkdir(parents=True,exist_ok=True);CACHE.mkdir(parents=True,exist_ok=True)
 for p in [CHAPTER,OUT/'story.wav']:
  if not (BACKUP/p.name).exists():shutil.copy2(p,BACKUP/p.name)
 profiles=json.loads((OUT/'voices.json').read_text(encoding='utf-8-sig'));sem=asyncio.Semaphore(3)
 async def render(role,text):
  spec=profiles[role];key=hashlib.sha256((text+json.dumps(spec)).encode()).hexdigest()[:20];mp3=CACHE/(key+'.mp3');wav=CACHE/(key+'.wav')
  async with sem:
   if not wav.exists():
    for attempt in range(4):
     try:
      await edge_tts.Communicate(text,**spec,proxy=urllib.request.getproxies().get('https'),receive_timeout=30).save(str(mp3));break
     except Exception:
      mp3.unlink(missing_ok=True)
      if attempt==3:raise
      await asyncio.sleep(2)
    subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'),'-nostdin','-loglevel','error','-y','-i',str(mp3),'-ar','24000','-ac','1','-c:a','pcm_s16le',str(wav)],check=True)
   with wave.open(str(wav),'rb') as r:
    assert r.getparams()[:3]==(1,2,24000)
    return r.readframes(r.getnframes())
 pieces=await asyncio.gather(*(render(role,en) for role,zh,en in DIALOGUE))
 with wave.open(str(OUT/'story.wav'),'rb') as r:
  params=r.getparams();pcm=r.readframes(r.getnframes());assert params[:3]==(1,2,24000)
 cut=round(c['lines'][at]['start']*24000);frame=cut;added=[];blob=b''
 for (role,zh,en),data in zip(DIALOGUE,pieces):
  assert len(data)>4800
  added.append(dict(speaker=role,voiceRole=role,actor={'陆川':'luchuan','艾莉娅':'aelia','莉瑟':'lyse'}.get(role,''),text=en,translation=zh,scene=SCENE,sceneSingle=True,hidePortraits=True,timeOfDay='night',start=frame/24000,end=(frame+len(data)//2)/24000))
  blob+=data+bytes(12000);frame+=len(data)//2+6000
 shift=(frame-cut)/24000
 for l in c['lines'][at:]:l['start']=round(l['start']+shift,6);l['end']=round(l['end']+shift,6)
 c['lines'][at:at]=added
 for q in c['questions']:
  if q['afterLine']>=at:q['afterLine']+=len(added)
 for d in c['decisions']:
  if d.get('afterLine',-1)>=at:d['afterLine']+=len(added)
 with wave.open(str(OUT/'story.wav'),'wb') as w:w.setparams(params);w.writeframes(pcm[:cut*2]+blob+pcm[cut*2:])
 CHAPTER.write_text(json.dumps(c,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 # Keep the source generator consistent with the published chapter.
 path=ROOT/'tools/chapter-one-continuation.json';sections=json.loads(path.read_text(encoding='utf-8-sig'));section=sections[3]
 section['dialogue'][at:at]=[list(row) for row in DIALOGUE]
 section['lineOverrides']={str(at+i):dict(scene=SCENE,hidePortraits=True) for i in range(len(added))}
 for q in section['questions']:
  if q[0]>=at:q[0]+=len(added)
 path.write_text(json.dumps(sections,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 print('Inserted',len(added),'lines at',at,'added audio seconds',round(shift,2),'existing dialogue and PCM preserved')
asyncio.run(main())
