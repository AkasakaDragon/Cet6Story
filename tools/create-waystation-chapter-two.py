"""Build chapter one section two with fixed role voices and offline CG media."""
import asyncio, hashlib, json, subprocess, sys, urllib.request, wave
from pathlib import Path
ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT / '.validation/tts-deps'))
import edge_tts
WORK = ROOT / '.validation/chapter-two-voices'
OUT = ROOT / 'chapters/audio/tavern/chapter-two'
WORK.mkdir(parents=True, exist_ok=True)
OUT.mkdir(parents=True, exist_ok=True)
FF = ROOT / 'tools/ffmpeg/ffmpeg.exe'
VOICES = json.loads((ROOT/'chapters/audio/tavern/voices.json').read_text(encoding='utf-8-sig'))
VOICES['莉瑟'] = dict(voice='en-GB-LibbyNeural', rate='-4%', pitch='-2Hz')
VOICES['摊主'] = dict(voice='en-US-EricNeural', rate='+0%', pitch='+0Hz')
DIALOGUE = [
('旁白','穿过镇门，陆川听见叫卖声。清单上的第一项是面粉，第二项是鸡蛋。','Beyond the town gate, Lu Chuan hears the market calls. Flour comes first on his list, followed by eggs.'),
('陆川','先买必需品。艾莉娅的话，得记在漂亮招牌前面。','Essentials first. Aelia\'s advice comes before any pretty sign.'),
('摊主','面粉在左边，鸡蛋在篮子里。开店的话，我可以帮你送到门口。','Flour on the left, eggs in the basket. If you run a shop, I can deliver them to your door.'),
('陆川','石桥外的驿站，今天准备开门。先给我一小袋面粉和十二个鸡蛋。','The waystation beyond the stone bridge is opening today. One small bag of flour and twelve eggs, please.'),
('摊主','那间驿站？昨晚我还以为它要塌了。你们把屋顶修好了？','That waystation? Last night I thought it would collapse. Have you repaired the roof?'),
('陆川','修好了。现在轮到厨房，不能让客人只吃屋顶。','We have. Now it is the kitchen\'s turn. We cannot serve our guests a roof.'),
('旁白','隔壁灯油摊前，一位银发少女停下了脚步。她手中的提灯没有熄灭，却忽明忽暗。','At the next stall, a silver-haired girl stops beside the lamp oil. Her lantern is still lit, but its light is unsteady.'),
('莉瑟','我要的是稳定魔力的灯油。这个瓶子，真的能用于符文提灯吗？','I need oil that keeps magical light steady. Is this bottle really suitable for a rune lantern?'),
('摊主','当然。瓶口有银线，价钱也比普通灯油高。','Certainly. There is silver around the neck, and it costs more than ordinary oil.'),
('陆川','抱歉打断一下。瓶底的说明，似乎说的不是这件事。','Sorry to interrupt. The note on the bottom seems to say something different.'),
('莉瑟','你能读懂这行旧文字？店里的人说它只是装饰。','Can you read that old writing? They told me it was only decoration.'),
('陆川','它写着“仅供普通照明，不可用于符文灯具”。银线只是包装。','It says, "For ordinary lighting only. Not for rune lamps." The silver is just decoration.'),
('莉瑟','我差一点就买下了。谢谢你，说明确实比包装有用。','I nearly bought it. Thank you. The instructions are more useful than the wrapping.'),
('摊主','等等……可能是搬货时混进来的。真正的符文灯油在下面一层。','Wait... Perhaps the bottles were mixed during delivery. The real rune oil is on the lower shelf.'),
('陆川','那就把说明一起拿出来。能做什么，应该先说清楚。','Then bring out the instructions too. What it can do should be clear before the sale.'),
('莉瑟','这瓶写着“稳定灯芯，远离明火”。这次没有把价格写成效果。','This one says, "Keeps the wick steady. Keep away from open flames." This time the price is not offered as proof.'),
('旁白','少女核对封口后才付款。新灯油滴入灯芯，提灯的光安静了下来。','The girl checks the seal before paying. A drop of new oil settles the light inside her lantern.'),
('莉瑟','你不是这里的常客吧？你看说明时，倒比摊主还熟练。','You are not a regular here, are you? Yet you read the instructions more carefully than the seller.'),
('陆川','陆川。刚接手石桥外的驿站，采购经验是今天开始积累的。','Lu Chuan. I have just taken over the waystation outside the stone bridge. My shopping experience starts today.'),
('莉瑟','叫我莉瑟就好。至于其他的……今天我只是来买灯油。','You can call me Lyse. As for the rest... today I am only here to buy lamp oil.'),
('陆川','那我今天也只是买鸡蛋的老板。你不用回答清单之外的问题。','Then today I am simply an innkeeper buying eggs. You need not answer questions beyond the shopping list.'),
('莉瑟','你的清单上，还有盐和蔬菜。前面那家比较新鲜，我带你过去。','Salt and vegetables are still on your list. The stall ahead has fresh produce. I can show you.'),
('旁白','两人走到集市另一侧。面粉和鸡蛋已经约好送货，陆川只需提着蔬菜篮。','They cross to the other side of the market. Flour and eggs will be delivered, leaving Lu Chuan with a basket of vegetables.'),
('陆川','热汤、面包、煎蛋。第一天的菜单，暂时就这三样。','Hot soup, bread, and eggs. Just those three things on our first menu.'),
('莉瑟','赶路的人会喜欢热汤。比起一长串菜名，我更想知道有没有地方坐。','Travelers will appreciate hot soup. More than a long menu, I would like to know if there is a seat.'),
('陆川','有，屋顶也不漏了。你路过时，可以来坐一会儿。','There is, and the roof no longer leaks. You are welcome to stop by when you pass.'),
('莉瑟','石桥外，大路旁。我记住了。如果下午有空，我去看看。','Beyond the stone bridge, beside the main road. I will remember. If I have time this afternoon, I will visit.'),
('旁白','镇门的风吹起莉瑟的发梢。一只淡紫蝴蝶掠过提灯，灯芯突然向北侧偏了一下。','Wind lifts Lyse\'s hair near the gate. A pale violet butterfly passes her lantern, and its light bends briefly toward the north.'),
('陆川','刚才那道光……不是被风吹的吧？','That light just now... That was not the wind, was it?'),
('莉瑟','不是。它有时会对不稳定的魔力产生反应。北边最近有什么变化吗？','No. It sometimes reacts to unstable magic. Has anything changed to the north recently?'),
('陆川','我只知道那边有旧矿道。艾莉娅提醒我，今天不要往那里走。','I only know there is an old mine road there. Aelia told me not to go that way today.'),
('莉瑟','那就听她的。买菜的人，没必要独自去调查矿道。','Then listen to her. Someone buying vegetables need not investigate a mine alone.'),
('陆川','我会把这件事告诉她。先把食材送回去，让驿站开门。','I will tell her what happened. First, I will bring the supplies home and open the waystation.'),
('莉瑟','好。谢谢你今天的帮忙，陆川。也替我留意那盏灯的方向。','Good. Thank you for your help today, Lu Chuan. Keep the direction of that light in mind.'),
('旁白','莉瑟收起提灯走进人群。陆川踏上回程，篮子里是第一锅热汤的材料，还有一个没问出口的问题。','Lyse disappears into the crowd with her lantern. Lu Chuan heads home with ingredients for the first soup, and one question left unasked.'),
('陆川','先开门，再问清楚。今天的清单，终于不只剩鸡蛋了。','Open the door first, ask questions later. Today\'s list finally contains more than eggs.')]
lines=[]
for i,(speaker,zh,en) in enumerate(DIALOGUE):
    scene='chapter-two-market.png' if i<22 else 'chapter-two-lantern.png' if i<34 else 'chapter-one-road.png'
    lines.append(dict(speaker=speaker,voiceRole=speaker,actor={'陆川':'luchuan','莉瑟':'lyse'}.get(speaker,''),text=en,translation=zh,scene='art/tavern/'+scene,sceneSingle=True,start=0,end=1))
chapter=dict(id='tavern-01-02',title='第一章：今天开始营业 · 第二节：集市，不愿透露姓名的少女',description='进城采购 · 识破灯油说明 · 初遇莉瑟 · 提灯的异常反应',source='原创奇幻剧情与分段听力题，非考试原文。',audio='audio/tavern/chapter-two/market.wav',audioLabel='离线固定角色英文配音 · Brian / Libby / Eric / Christopher',background='art/tavern/chapter-two-market.png',unlockLevel=1,sortOrder=1020,pixelArt=True,inlineQuestions=True,timeLimitSeconds=1200,actors=[dict(id='luchuan',gender='male',side='left',image='art/tavern/luchuan-portrait.png'),dict(id='lyse',gender='female',side='right',image='art/tavern/lyse-portrait.png')],decisions=[],lines=lines,questions=[{'afterLine': 5, 'skill': '采购数量与送货', 'prompt': 'What does Lu Chuan ask the seller to supply?', 'options': ['A large barrel of oil.', 'One small bag of flour and twelve eggs.', 'Twelve bags of flour and one egg.', 'Only a new roof.'], 'answer': 1, 'explanation': '陆川先订一小袋面粉和十二个鸡蛋，摊主可送到驿站。'}, {'afterLine': 16, 'skill': '商品说明与用途', 'prompt': 'Why does Lu Chuan warn Lyse about the first bottle?', 'options': ['It is meant for ordinary lighting, not rune lamps.', 'It contains no oil at all.', 'Its silver decoration is damaged.', 'It is too heavy to carry.'], 'answer': 0, 'explanation': '瓶底说明明确写着仅供普通照明，不适用于符文灯；银线装饰和更高价格不能证明用途。'}, {'afterLine': 27, 'skill': '邀请与人物需求', 'prompt': 'What matters to Lyse more than a long menu?', 'options': ['The size of the market.', 'An expensive sign.', 'A place to sit and some hot soup.', 'A room inside the mine.'], 'answer': 2, 'explanation': '莉瑟说赶路的人会喜欢热汤，比起一长串菜名，她更关心有没有座位。'}, {'afterLine': 35, 'skill': '异常线索与行动安排', 'prompt': 'What will Lu Chuan do after seeing the lantern react?', 'options': ['Explore the northern mine alone.', 'Follow Lyse into the crowd.', 'Return with supplies and tell Aelia about the reaction.', 'Cancel the opening and return every purchase.'], 'answer': 2, 'explanation': '先带食材返回驿站，告知艾莉娅提灯的异常，暂不独自进入矿道。'}],ending='采购完成，陆川认识了化名莉瑟的少女。符文提灯指向北侧旧矿道，他决定先返回驿站，与艾莉娅分享线索。')
cg=dict(intro=dict(zh='石桥尽头，集市已经热闹起来。陆川带着清单，走进第一次采购的早晨。',en='Beyond the stone bridge, the market is already awake. Lu Chuan enters town with a shopping list and a tavern to prepare.'),outro=dict(zh='食材已经买齐，少女的名字却仍是谜。陆川沿石桥回去，记住了提灯指向的方向。',en='The supplies are ready, but the girl remains a mystery. Lu Chuan heads back across the bridge, remembering where her lantern pointed.'))
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
    with wave.open(str(OUT/'market.wav'),'wb') as writer:
        writer.setnchannels(1);writer.setsampwidth(2);writer.setframerate(24000)
        for line,data in zip(lines,pieces):
            line['start']=round(frames/24000,4);writer.writeframes(data);frames+=len(data)//2
            line['end']=round(frames/24000,4);writer.writeframes(bytes(12000));frames+=6000
    for key,spec in cg.items():
        data=await render(key,spec['en'],'旁白')
        with wave.open(str(OUT/(key+'.wav')),'wb') as writer:
            writer.setnchannels(1);writer.setsampwidth(2);writer.setframerate(24000);writer.writeframes(bytes(12000));writer.writeframes(data);writer.writeframes(bytes(48000))
        spec['seconds']=round(len(data)/48000+1.25,4)
    (ROOT/'chapters/12-tavern-01-02.json').write_text(json.dumps(chapter,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    (OUT/'voices.json').write_text(json.dumps(VOICES,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    (OUT/'cg.json').write_text(json.dumps(cg,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('Published section two: '+str(len(lines))+' lines, '+str(round(frames/24000,2))+' seconds.',flush=True)
asyncio.run(main())
import importlib.util
override_spec = importlib.util.spec_from_file_location('princess_override', ROOT / 'tools/install-princess-chapter-two.py')
if override_spec.origin and Path(override_spec.origin).exists():
    override = importlib.util.module_from_spec(override_spec)
    override_spec.loader.exec_module(override)
    override.install()
