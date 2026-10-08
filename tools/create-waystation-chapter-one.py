"""Build first section and fixed-voice offline audio; generated art is supplied separately."""
import asyncio, hashlib, json, os, subprocess, urllib.request, wave
from pathlib import Path
import edge_tts
ROOT = Path(__file__).resolve().parent.parent
WORK = ROOT / '.validation/chapter-one-voices'
WORK.mkdir(parents=True, exist_ok=True)
OUT = ROOT / 'chapters/audio/tavern/chapter-one'
OUT.mkdir(parents=True, exist_ok=True)
FF = ROOT / 'tools/ffmpeg/ffmpeg.exe'
VOICES = json.loads((ROOT/'chapters/audio/tavern/voices.json').read_text(encoding='utf-8-sig'))
dialogue = [
('旁白','晨光穿过新补好的窗沿。屋顶不再漏雨，楼下传来杯碟轻碰的声音。','Morning light falls through the repaired window. The roof is dry, and cups clink downstairs.'),
('艾莉娅','陆川，醒了吗？你的第一天老板生活开始了。','Lu Chuan, are you awake? Your first day as an innkeeper has begun.'),
('陆川','醒了。只是我的肩膀还以为我们在搬木板。','I am awake. My shoulders still think we are carrying boards.'),
('艾莉娅','我检查过横梁和楼梯，都很稳。昨晚没有白忙。','I checked the beams and stairs. They are secure. Last night was worth the effort.'),
('陆川','好消息。坏消息是，我答应的早餐好像还没着落。','Good news. The bad news is that the breakfast I promised is still missing.'),
('艾莉娅','有水，有锅，也有一点面包。不过，要营业还差得远。','We have water, a pot, and a little bread. Opening for guests will take more than that.'),
('陆川','原来一间能住的屋子，和一间能开门的酒馆，是两回事。','So a place to sleep and a tavern ready for business are two different things.'),
('艾莉娅','食材、菜单，还有招待客人用的东西。我们先列清单。','Ingredients, a menu, and supplies for our guests. Let us make a list first.'),
('陆川','屋顶已经修好，今天总算可以考虑锅里放什么了。','The roof is fixed. Today we can finally think about what goes into the pot.'),
('艾莉娅','先把早餐吃了。老板饿着肚子，可招待不好别人。','Have breakfast first. A hungry innkeeper will not take good care of anyone.'),
('旁白','两人来到厨房。炉灶已经清理干净，储藏架上却空了大半。','They enter the kitchen. The hearth is clean, but most of the pantry shelves are empty.'),
('陆川','这个柜子倒是很诚实，一眼就知道我们缺什么。','This cupboard is honest. It tells us exactly what we are missing.'),
('艾莉娅','先买面粉、鸡蛋和蔬菜，再补盐和灯油。','Buy flour, eggs, and vegetables first. Then get salt and lamp oil.'),
('陆川','菜单先做简单一点？热汤、面包，再加一份煎蛋。','Should we keep the menu simple? Hot soup, bread, and a plate of eggs.'),
('艾莉娅','适合赶路的人。做得到的，比写得漂亮的更重要。','That suits travelers. What we can serve matters more than a beautiful menu.'),
('陆川','那就不写十种招牌菜了。第一天，先把三样做好。','Then no list of ten special dishes. On the first day, we will do three things well.'),
('艾莉娅','我留下整理桌椅，检查杯碟。你去镇上的集市采购。','I will arrange the tables and check the cups. You can buy supplies at the town market.'),
('陆川','一个人去？我可能连路牌都还没认全。','On my own? I have not even learned all the road signs yet.'),
('艾莉娅','沿着门外的大路走，过石桥就是镇门。别走向北边的矿道。','Follow the main road outside. Cross the stone bridge to reach the town gate. Avoid the mine road to the north.'),
('陆川','大路、石桥、镇门。我记住了。矿道今天不在清单里。','Main road, stone bridge, town gate. Got it. The mine is not on today\'s shopping list.'),
('艾莉娅','把清单和钱收好。先买必需品，别被漂亮的招牌吸引走。','Keep the list and money safe. Buy the essentials first, before a pretty sign distracts you.'),
('陆川','放心。我擅长看说明，只是还没学会当老板。','Do not worry. Reading instructions is my strength. Being an innkeeper is the new part.'),
('旁白','陆川把采购清单放进挎包。门外的石路已经晒干，风里有青草的气息。','Lu Chuan slips the shopping list into his satchel. The stone road is dry, and the breeze smells of grass.'),
('艾莉娅','回来时，要是看见赶路的人，可以告诉他们这里快开门了。','If you meet travelers on your way back, tell them we will open soon.'),
('陆川','昨晚我们只是想找个不漏雨的地方，今天就要招待客人了。','Last night we only wanted a dry roof. Today we are preparing to welcome guests.'),
('艾莉娅','不用一口气做好所有事。先让第一位客人坐下，喝上一碗热汤。','We do not have to do everything at once. Start with a seat and hot soup for the first guest.'),
('陆川','那我去把热汤的材料买回来。驿站就先交给你了。','Then I will bring back the ingredients for that soup. The waystation is in your hands.'),
('艾莉娅','嗯，我在这里等你。路上小心。','I will be here when you return. Take care on the road.'),
('旁白','艾莉娅回到大厅，陆川沿着石路走向镇门。酒馆的第一份菜单，还只是挎包里的一张纸。','Aelia returns to the hall. Lu Chuan follows the road toward town. The tavern\'s first menu is still only a sheet of paper in his bag.'),
('陆川','拯救世界的第一步……先去买鸡蛋。','The first step to saving the world... buy some eggs.')]
lines=[]
for i,(speaker,zh,en) in enumerate(dialogue):
    scene='waystation-repair-morning.png' if i<10 else 'chapter-one-kitchen.png' if i<22 else 'chapter-one-road.png'
    lines.append(dict(speaker=speaker,voiceRole=speaker,actor={'陆川':'luchuan','艾莉娅':'aelia'}.get(speaker,''),text=en,translation=zh,scene='art/tavern/'+scene,sceneSingle=True,timeOfDay='morning',start=0,end=1))
chapter=dict(timeOfDay='morning',id='tavern-01-01',title='第一章：今天开始营业 · 第一节：老板的第一天',description='清晨醒来 · 检查厨房 · 商定菜单与采购 · 启程进城',source='原创奇幻剧情与四道分段听力理解题，非考试原文。',audio='audio/tavern/chapter-one/morning.wav',audioLabel='离线英文角色配音 · Brian / Michelle / Christopher',background='art/tavern/waystation-repair-morning.png',unlockLevel=1,sortOrder=1010,pixelArt=True,inlineQuestions=True,timeLimitSeconds=900,actors=[dict(id='luchuan',gender='male',side='left',image='art/tavern/luchuan-portrait.png'),dict(id='aelia',gender='female',side='right',image='art/tavern/aelia-portrait.png')],decisions=[],lines=lines,questions=[{'afterLine': 9, 'skill': '营业准备与因果关系', 'prompt': 'Why is the waystation not ready to welcome guests yet?', 'options': ['The roof is still leaking.', 'They need ingredients, a menu, and supplies.', 'Aelia has decided to leave.', 'The town gate is closed.'], 'answer': 1, 'explanation': '屋顶与楼梯已经修好，但食材、菜单和接待用品仍需准备。'}, {'afterLine': 15, 'skill': '菜单选择与原因', 'prompt': 'Why do they keep the first menu simple?', 'options': ['They want to serve dishes they can prepare well.', 'Travelers never need hot food.', 'All ingredients are free.', 'Aelia refuses to use the kitchen.'], 'answer': 0, 'explanation': '第一天先把热汤、面包、煎蛋三样做好；能实际提供餐食比漂亮的长菜单重要。'}, {'afterLine': 21, 'skill': '采购优先级', 'prompt': 'What should Lu Chuan buy before looking at attractive signs?', 'options': ['Expensive decorations.', 'A new roof.', 'Gifts for every traveler.', 'The essentials on the shopping list.'], 'answer': 3, 'explanation': '艾莉娅提醒他保管清单和钱，先买必需品，不被漂亮招牌带走。'}, {'afterLine': 29, 'skill': '分工与路线理解', 'prompt': 'What will Lu Chuan do while Aelia prepares the hall?', 'options': ['Explore the mine to the north.', 'Repair the roof again.', 'Follow the main road across the stone bridge to buy supplies.', 'Wait upstairs for the first guest.'], 'answer': 2, 'explanation': '艾莉娅留下整理桌椅与杯碟；陆川沿大路过石桥，到镇上集市采购，暂不探索北边矿道。'}],ending='菜单与采购分工已确定。陆川启程前往边境小镇，艾莉娅留在驿站准备大厅。第一章第二节“集市：不愿透露姓名的少女”已开放。')
cg=dict(intro=dict(zh='清晨，修好的驿站迎来新的一天。老板的第一天，从一声敲门开始。',en='Morning arrives at the repaired waystation. The innkeeper\'s first day begins with a knock at the door.'),outro=dict(zh='清单收进挎包，石桥通向城镇。今天，他们要让驿站真正开门。',en='With a shopping list in his bag, Lu Chuan sets out for town. Today, they will make the waystation ready for its first guests.'))
async def main():
    sem=asyncio.Semaphore(3)
    proxy=urllib.request.getproxies().get('https')
    async def render(key,text,role):
        spec=VOICES[role];digest=hashlib.sha256((text+json.dumps(spec)).encode()).hexdigest()[:12]
        mp3=WORK/(key+'-'+digest+'.mp3');wav=mp3.with_suffix('.wav')
        async with sem:
            if not mp3.exists():
                for attempt in range(3):
                    try:
                        await edge_tts.Communicate(text,**spec,proxy=proxy,receive_timeout=30).save(str(mp3));break
                    except Exception:
                        mp3.unlink(missing_ok=True)
                        if attempt==2:raise
                        await asyncio.sleep(2)
            subprocess.run([str(FF),'-loglevel','error','-y','-i',str(mp3),'-ar','24000','-ac','1','-c:a','pcm_s16le',str(wav)],check=True)
            with wave.open(str(wav),'rb') as reader:data=reader.readframes(reader.getnframes())
            if len(data)<4800:raise RuntimeError('Empty speech '+key)
            print('Rendered '+key+': '+spec['voice'],flush=True)
            return data
    pieces=await asyncio.gather(*(render(str(i),line['text'],line['voiceRole']) for i,line in enumerate(lines)))
    frames=0
    with wave.open(str(OUT/'morning.wav'),'wb') as writer:
        writer.setnchannels(1);writer.setsampwidth(2);writer.setframerate(24000)
        for line,data in zip(lines,pieces):
            line['start']=round(frames/24000,4);writer.writeframes(data);frames+=len(data)//2
            line['end']=round(frames/24000,4);writer.writeframes(bytes(12000));frames+=6000
    for key,spec in cg.items():
        data=await render(key,spec['en'],'旁白')
        with wave.open(str(OUT/(key+'.wav')),'wb') as writer:
            writer.setnchannels(1);writer.setsampwidth(2);writer.setframerate(24000);writer.writeframes(bytes(12000));writer.writeframes(data);writer.writeframes(bytes(48000))
        spec['seconds']=round(len(data)/48000+1.25,4)
    (ROOT/'chapters/11-tavern-01-01.json').write_text(json.dumps(chapter,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    (OUT/'cg.json').write_text(json.dumps(cg,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('Published 30 lines, two questions and opening/closing CG narration; '+str(round(frames/24000,2))+' seconds.',flush=True)
asyncio.run(main())
