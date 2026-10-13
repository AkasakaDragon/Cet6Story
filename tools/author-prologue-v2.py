"""Author the new prologue independently of the discarded game script."""
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parent.parent
lines = []
segments = []

def section(name, scene, caption, phase, portraits, rows, cg=None):
    first = len(lines)
    for row in rows.strip().split('\n'):
        speaker, zh, en = row.split('|', 2)
        actor = {'陆川':'luchuan','艾莉娅':'aelia','伊瑟雅':'iserya'}.get(speaker, '')
        lines.append(dict(speaker=speaker, actor=actor, voiceRole='室友消息' if speaker=='室友' else '旁白' if speaker in ('时间字幕','演出字幕') else speaker,
                          text=en, translation=zh, scene='art/prologue-v2/'+scene+'.png', sceneSingle=True,
                          sceneCaption=caption, timeOfDay=phase, portraitIds=portraits,
                          hidePortraits=not portraits, start=0, end=0))
    if cg:
        lines[first]['cgBefore'] = cg
    segments.append(dict(name=name, first=first, last=len(lines)-1, caption=caption))

section('还有一页','dorm','原世界 · 晚上 · 下雨','night',[],'''
时间字幕|原世界，晚上。窗外的雨已经下了很久。|Original world. Late evening. Rain has been falling for hours.
室友|你那盏灯还不关？我睡醒一小觉了，睁眼看见你还在，以为天亮了。|Is your lamp still on? I woke up from a short nap and thought it was morning.
陆川|就剩这一页。答案对了，但这份解析写得绕，我想知道它究竟绕在哪里。|Just one page left. My answer is correct, but the explanation is confusing. I want to understand why.
室友|做对了还跟解析较劲？换我早把书合上了。|You got it right and still argue with the explanation? I would have closed the book.
陆川|做对是一回事，弄明白是另一回事。不然下次换个问法，我还得碰运气。|Getting it right is one thing; understanding it is another. Otherwise, I will depend on luck next time.
室友|那你顺便研究一下，为什么明天第一节课不能晚半小时。|Then investigate why tomorrow's first class cannot start half an hour later.
陆川|这个超出教材范围。你写信给排课老师，我帮你检查语法。|That is beyond the textbook. Write to the timetable office; I can check your grammar.
室友|算了，帮我带两个包子实际些。窗关严一点，刚才雨都飘进来了。|Never mind. Bring me two steamed buns instead. Close the window properly; some rain came in.
陆川|两个，记住了。你先睡，我看完这页就关灯。|Two. Noted. Get some sleep. I will switch off the lamp after this page.
旁白|床帘后传来含糊的答应。陆川把杯子挪开，桌上的电脑忽然闪了一下。|A sleepy reply comes from behind the curtain. Lu Chuan moves his cup aside. The laptop flickers.
''')

section('屏幕里的石门','dorm','原世界 · 晚上 · 下雨','night',[],'''
陆川|……刚才还不是这个页面。|That was not on the screen a moment ago.
旁白|屏幕中，一扇石门立在没有波纹的水面上。门缝透出的冷光，照到了他的手指。|On the screen, a stone doorway stands above motionless water. Cold light from its opening reaches his fingers.
陆川|鼠标没反应。不是视频？连退出都不行。|The mouse does nothing. Is this a video? I cannot even close it.
室友|又卡了？明天再修吧。|Frozen again? Fix it tomorrow.
陆川|嗯，你先睡。|Yes. Go back to sleep.
旁白|他伸手去确认电源。窗外雷声响起，屏幕里的水面却仍然纹丝不动。|He reaches for the power connection. Thunder sounds outside, but the water on the screen remains still.
伊瑟雅|陆川，请先听我说。|Lu Chuan, please listen to me.
陆川|谁？你是从电脑里在说话？|Who is there? Are you speaking through my laptop?
旁白|石门的光越过屏幕边缘。台灯、书页和雨声，依次消失在白光里。|Light crosses the screen's edge. The lamp, the pages, and the sound of rain disappear into white.
''', 'cg02')

section('女神的请求','boundary','交界空间 · 时间未明 · 无天气','', ['luchuan','iserya'],'''
时间字幕|交界空间。这里没有正常的昼夜，原世界的雨声已经消失。|The boundary between worlds. There is no ordinary day or night here. The rain is silent.
陆川|这不是我的衣服……刚才明明还在桌前。有人能告诉我，发生了什么吗？|These are not my clothes. I was at my desk. Can someone explain what happened?
伊瑟雅|先放松手，你没有受伤。我是伊瑟雅，守护艾瑟兰地脉联系的女神。是我发起了召唤。|Relax your hand. You are not injured. I am Iserya, guardian of Aetherlan's connected lifelines. I summoned you.
陆川|你用了“召唤”，但我还没有答应。我的家人和同学都在那边。|You say you summoned me, but I did not agree. My family and friends are still there.
伊瑟雅|你说得对。你的出现不等于同意。我不会要求你现在就宣誓。|You are right. Your presence is not consent. I will not ask for an oath.
陆川|那先说最实际的问题。我能回去吗？|Then let us start with the practical question. Can I go home?
伊瑟雅|能，但不是现在。归途经过大陆的地脉，污染正在侵蚀它。强行开启，会伤害你。|Yes, but not yet. The way home crosses the continent's lifelines. Pollution is damaging them. Opening it now could hurt you.
陆川|送我来时为什么可以？来和回，总该有个区别。|Why was bringing me here possible? There must be a difference between arriving and returning.
伊瑟雅|我仅存的跨界力量，只够建立一次单向召唤。现在，我也无法再召来第二个人。|My remaining power could create only one passage in one direction. I cannot summon another person now.
陆川|所以我先要了解这里出了什么事。别只告诉我，你需要一个英雄。|Then I need to understand what happened here. Please do not simply tell me you need a hero.
伊瑟雅|八十年前，魔王瓦尔莫斯从古老遗迹附近出现。他烧毁村庄，侵占城镇，制造自己的军队。|Eighty years ago, the demon king Valmors appeared near ancient ruins. He destroyed villages, occupied towns, and created an army.
陆川|既然他现在被封印，说明有人挡住过他。|If he is sealed now, someone must have stopped him.
伊瑟雅|奥德里克二世国王的魔术师赛维恩，用毕生研究铸成五颗宝石，交给五位将军。|King Oderic the Second's magician, Severn, used a lifetime of research to forge five gems for five generals.
伊瑟雅|他们打了十年的仗，收复土地，最后把瓦尔莫斯封印在他建于古庭之上的堡垒中。|They fought for ten years, reclaimed the land, and sealed Valmors in the fortress he built over the ancient court.
陆川|十年才换来的胜利。为什么今天又出了问题？|A victory that took ten years. Why is it failing now?
伊瑟雅|战后，宝石深埋在五处制造魔物的地区，压住污染。封印已经持续七十年，力量正在衰弱。|After the war, the gems were buried in five regions where monsters were made. They contained the pollution for seventy years. Their power is fading.
伊瑟雅|城镇传来失常的钟声，森林的动物开始发狂。码头、陨坑和矿洞，也陆续出现异常。|The town's bells sound wrongly. Forest animals turn violent. The docks, the crater, and the mines also show signs of danger.
陆川|这不是五场碰巧一起发生的灾害，而是同一条路出了问题。|These are not five unrelated disasters. Something connecting them is failing.
伊瑟雅|是的。归灯驿站曾是五条维护线路汇合的地方。它的设施，还能让宝石恢复稳定。|Yes. Guidelight Waystation once connected five maintenance routes. Its equipment can still restore the gems to stability.
陆川|我听见“驿站”，不是“军营”。我要先修一个能住人的地方？|You said a waystation, not a military camp. I need to repair a place where people can stay?
伊瑟雅|有人能休息、补给、交换消息，维护才不会只靠几个人苦撑。宝石需要设施，也需要懂得照料它们的人。|People need rest, supplies, and reliable information. Maintenance cannot depend on a few exhausted people. The gems need equipment and people who understand it.
陆川|我不会拿剑，也没有经营经验。这个计划里，你至少得留出让我学的时间。|I cannot fight with a sword or run an inn. Your plan must allow me time to learn.
伊瑟雅|我会给你雷电的力量。它通过吟唱释放，但不能替你判断安全的位置。|I will grant you the power of lightning. Chanting releases it, but it cannot choose a safe place for you.
陆川|雷电。听起来很有用，也很容易先把自己劈了。|Lightning. Useful, but it sounds rather easy to strike myself first.
伊瑟雅|先选目标，再保持呼吸，说清楚完整的咒句。紧张时可以停下重来，不能含糊地硬撑。|Choose your target, breathe steadily, and speak the whole chant clearly. If frightened, stop and begin again. Do not force it.
伊瑟雅|“Gather, lightning. Follow my voice.”先让它只在指尖亮起。|Gather, lightning. Follow my voice. First, let it shine only at your fingertips.
陆川|Gather, lightning. Follow my voice.……真的亮了。还好，你没让我第一下就打那扇门。|Gather, lightning. Follow my voice. It really shines. I am glad my first attempt was not aimed at that door.
伊瑟雅|水、金属和同伴的位置，会改变它经过的路线。记住，你可以先问，不必急着证明自己。|Water, metal, and nearby companions change its path. Remember: ask first. You do not need to prove yourself immediately.
陆川|你不能保证我一路顺利。但如果归途需要修复，我愿意先找到能一起做事的人。|You cannot promise an easy journey. But if the way home needs repair, I will look for people who can help.
伊瑟雅|到达以后，先找安全的屋檐。有人正在巡查那条路。你不会因为暂时不知道答案，就失去求助的资格。|When you arrive, find safe shelter first. Someone is patrolling that road. Not knowing the answer does not mean you cannot ask for help.
''', 'cg03')

section('雨中的归灯','outside','艾瑟兰 · 第1天 · 晚上 · 下雨','night',['luchuan'],'''
时间字幕|艾瑟兰，第1天，晚上。召唤结束，雨再次落在陆川的肩上。|Aetherlan. Day one, evening. The summoning ends. Rain falls on Lu Chuan's shoulders.
陆川|至少这雨是真的。刚才那点雷光，先别拿来照整条路。|At least the rain is real. I should not use that little spark to light the entire road.
旁白|道路尽头，一座两层驿站的招牌斜挂在屋檐下。门没有锁，却被湿木板卡住了。|At the end of the road, a two-storey waystation has a crooked sign. Its unlocked door is caught against wet timber.
陆川|归灯……名字挺像个好去处。希望里面比外面干一点。|Guidelight. That sounds welcoming. I hope it is drier inside.
''')

section('旧设施的声音','ruined','归灯驿站 · 第1天 · 晚上 · 下雨','night',['luchuan'],'''
旁白|左侧壁炉已经熄灭，吧台蒙着灰。右侧楼梯断了一阶，二层走廊隐没在黑暗里。|The left fireplace is cold. Dust covers the counter. A step is broken on the right staircase; the upper corridor lies in darkness.
陆川|厨房、炉子、能坐的椅子……先检查屋顶。女神没说这工作还附送拆迁风险。|A kitchen, a hearth, and chairs. First, check the roof. The goddess did not mention the risk of demolition.
旁白|一声金属摩擦从横梁下传来。灰背裂牙兽正拖拽嵌在木槽里的旧铜线。|Metal scrapes below a beam. A grey-backed rift hound is pulling an old copper wire from a wooden channel.
陆川|它不在找吃的。那根线有什么值得它这么费劲？|It is not looking for food. Why is that wire worth so much effort?
旁白|怪物转过头。陆川后退一步，脚跟碰翻了接雨的木盆。门外传来急促的脚步声。|The creature turns. Lu Chuan steps back and knocks over a bucket. Quick footsteps approach outside.
''')

section('骑士的规矩','ruined','归灯驿站 · 第1天 · 晚上 · 下雨','night',['luchuan','aelia'],'''
艾莉娅|别跑向楼梯。那一阶承不住人。站到壁炉旁边的干地上。|Do not run to the stairs. That step cannot bear your weight. Move to the dry floor beside the hearth.
陆川|你对这里很熟？我刚想把楼梯当退路。|Do you know this place? I was about to use the stairs as an escape route.
艾莉娅|我巡查这条路。先听指令，名字等安全了再说。|I patrol this road. Follow my instructions first; names can wait until we are safe.
旁白|红发骑士用双手剑拦住裂牙兽。她没有追着它乱挥，而是一步步隔开它与陆川的距离。|The red-haired knight blocks the hound with a two-handed sword. She moves carefully, keeping it away from Lu Chuan.
陆川|我有一点雷电魔法。能帮忙，但我还不太熟练。|I have some lightning magic. I can help, but I have little practice.
艾莉娅|打哪里？先告诉我位置。|Where will you strike? Tell me the position first.
陆川|横梁下，那根铜线旁边——等等，地上有水。|Below the beam, beside that copper wire. Wait. There is water on the floor.
艾莉娅|对。铜线连着积水，你现在放电，我也在那条路上。不要施法。|Yes. The wire reaches the water. If you release it now, I am in its path. Do not cast.
陆川|我差点把“帮你”变成“连你一起”。我换个目标，等你信号。|I nearly turned helping you into striking you too. I will choose another target and wait for your signal.
艾莉娅|看那片干木板。我把它逼过去。它低头咬东西时，你打断它，不要追着它放电。|Watch those dry boards. I will drive it there. When it lowers its head, interrupt it. Do not chase it with lightning.
陆川|明白。先位置，再呼吸，再吟唱。|Understood. Position first, then breath, then the chant.
艾莉娅|我数到三。别急，一、二——三。|On three. Stay calm. One, two, three.
''','cg04')

section('战斗暂跳','ruined','归灯驿站 · 第1天 · 晚上 · 下雨','night',[],'''
演出字幕|战斗演出暂未制作，本段跳过。陆川按约定吟唱，打断裂牙兽的扑咬，艾莉娅随后击退了它。|The battle sequence is not implemented yet and is skipped. Lu Chuan interrupts the hound's attack with his chant; Aelia drives it back.
旁白|喧闹退去，雨仍在落。艾莉娅先看陆川是否受伤，再确认门外已经没有魔物。|The noise fades. Rain continues. Aelia checks Lu Chuan for injuries, then confirms that the road is clear.
''')

section('先把灯点起来','ruined','归灯驿站 · 第1天 · 晚上 · 下雨','night',['luchuan','aelia'],'''
艾莉娅|手给我看一下。不是检查你的魔法，看看有没有被木刺划伤。|Let me see your hand. Not your magic; I am checking for splinters.
陆川|没有伤。我刚才说话挺稳的，现在手才开始抖。|No injury. My voice was steady then. My hand is only shaking now.
艾莉娅|害怕不影响你刚才做对了什么。我叫艾莉娅，边境巡防骑士。|Being afraid does not change what you did correctly. I am Aelia, a border patrol knight.
陆川|陆川。关于我为什么在这里，解释会比“迷路”长不少。|Lu Chuan. Explaining why I am here will take longer than saying I got lost.
艾莉娅|那就坐下说。先别碰那根铜线，我还没有确认它连去哪里。|Then sit and explain. Do not touch that copper wire; I have not checked where it leads.
旁白|陆川说起召唤与五颗宝石。艾莉娅听完没有立即点头，先看向他刚才发出雷光的手。|Lu Chuan describes the summoning and the five gems. Aelia listens without rushing to agree, then looks at his hand.
艾莉娅|你说的名字，我在旧巡防档案里见过。可档案不会告诉我眼前的人靠不可靠。|I have seen those names in old patrol records. But records cannot tell me whether the person before me is reliable.
陆川|所以你先看我会不会听人说“停下”。这个考核比让我证明女神存在合理。|So you first checked whether I could listen when told to stop. That is more reasonable than asking me to prove a goddess exists.
艾莉娅|今晚至少能确认这一点。其他事，天亮后再找人核实。|Tonight, I can confirm that much. We will verify the rest in daylight.
陆川|那我能先问，这个铜槽是干什么的？怪物宁可挨剑，也想靠近它。|Then what is this copper channel for? The creature tried to reach it even with your sword in the way.
旁白|铜槽边缘刻着五道不同的纹路。雨水沿着其中一道，慢慢渗进墙内。|Five different marks are cut into the channel. Rain follows one of them into the wall.
艾莉娅|我不知道它的用途。今晚把漏水止住，明天找懂旧设施的人。别为了试它，再把手伸进去。|I do not know its purpose. Stop the leak tonight; find someone who knows the equipment tomorrow. Do not put your hand in to test it.
陆川|好。拯救世界的第一步：别在不懂的地方乱伸手。|Agreed. The first step towards saving a world: keep your hands out of equipment you do not understand.
艾莉娅|第一步是把那块湿木板搬走。你坐的位置还在漏雨。|The first step is moving that wet board. Rain is still falling where you are sitting.
陆川|你说得对。先解决眼前的事。|You are right. Start with the problem in front of us.
''')

section('临时屋檐','patched','归灯驿站 · 第1天 · 晚上 · 下雨','night',['luchuan','aelia'],'''
时间字幕|同一晚，约半小时后。两人只修出一小片能安全避雨的地方。|The same evening, about half an hour later. They have made only a small area safe enough for shelter.
艾莉娅|扶住这一边，我来固定。人别站在横梁正下方。|Hold this side while I secure it. Do not stand directly under the beam.
陆川|你连屋顶都会修。巡防训练里还包括这个？|You can repair roofs too. Is that part of patrol training?
艾莉娅|巡防时，能睡的屋子不总比敌人好找。基本的事，不会也得学。|On patrol, shelter can be harder to find than enemies. You must learn the basics.
陆川|我以前总想先弄清所有原理，再动手。今天看来，有时候得先把盆接上。|I usually want to understand everything before acting. Today, I should start by putting a bucket under the leak.
艾莉娅|原理没错，顺序要看时候。你把干柴递过来。|Understanding matters. The order depends on the situation. Pass me the dry wood.
旁白|壁炉燃起小小一团火。右侧楼梯仍用绳子拦着，厨房和客房还没有检查。|A small fire catches in the hearth. A rope still blocks the stairs. The kitchen and guest rooms remain unchecked.
陆川|所以我们有一个炉子，两把椅子，还有半个屋顶。离开店还差得很远。|We have a hearth, two chairs, and half a roof. We are a long way from opening an inn.
艾莉娅|够今晚休息就好。明天去村庄找木匠和粮食，正式修缮要几天。|Enough to rest tonight. Tomorrow, find a carpenter and food in the village. Proper repairs will take days.
陆川|你不是还要继续巡查吗？别为了这里，把你的工作都耽误了。|Do you still have a patrol to finish? I do not want this place to delay all your work.
艾莉娅|雨夜巡线暂时改为守点，我会留下记录。天亮去村庄交接，顺路带你过去。|The rain has changed tonight's patrol into a watch here. I will record it. At dawn, I will report in the village and take you along.
陆川|谢谢。你把“顺路”说得这么认真，我就不跟你争是不是帮忙了。|Thank you. You make that sound so official that I will not argue whether it counts as helping.
艾莉娅|需要感谢，可以先把你的湿斗篷挪开。它盖住了最后一根干柴。|If you want to thank me, move your wet cloak. It is covering the last dry piece of wood.
陆川|好的，骑士。我发现你的规矩多数都跟保住今晚有关。|Of course, knight. Most of your rules seem to be about surviving tonight.
艾莉娅|规矩应该有理由。你不明白时可以问，紧急的时候先照做。|Rules should have reasons. Ask when you do not understand. In an emergency, act first.
陆川|那也约定一条：你撑不住的时候告诉我，别把所有事都当成你的职责。|Then let us agree on one more: tell me when you are tired. Not everything has to be your responsibility.
艾莉娅|……可以。先休息两个时辰，我守前半段，后半段叫你。|All right. Rest for a few hours. I will take the first watch and wake you for the next.
''','cg05')

section('把门留给明天','morning','归灯驿站 · 第2天 · 早上 · 多云','morning',['luchuan','aelia'],'''
时间字幕|一夜过去。第2天早上，雨停了，厚云仍遮着天空。|A night passes. Day two, morning. The rain has stopped, but thick clouds remain.
旁白|临时补板撑过了夜雨。壁炉边的地面干了，屋顶缺口和封住的楼梯仍清楚可见。|The temporary boards held through the rain. The floor by the hearth is dry; the roof damage and blocked stairs remain.
陆川|我昨晚梦见自己回去上课。醒来发现不用迟到，又觉得好像没那么值得高兴。|I dreamed I was going to class. Waking up without needing to rush is less comforting than I expected.
艾莉娅|想家很正常。先喝水，我们吃一点干粮，再去村庄。|Missing home is normal. Drink some water. We will eat a little and go to the village.
陆川|这里没有突然变成一家好酒馆。但至少这盏灯还在。|This has not suddenly become a good inn. But at least the lamp is still here.
艾莉娅|灯油够到今晚。别先承诺接待客人，厨房、用水和屋顶都要验收。|There is oil until tonight. Do not promise rooms yet. The kitchen, water, and roof need inspection.
陆川|明白。今天先去找能一起修的人，老板的招牌等事情做成再挂。|Understood. Today, find people who can help repair it. The owner's sign can wait until the work is done.
艾莉娅|带上那根铜线的拓印。村里有人见过旧维护设施。|Bring a copy of the marks on that copper wire. Someone in the village knows the old equipment.
陆川|走吧。先弄清五道刻痕，也先弄清早餐能在哪里买。|Let us go. Find out about those five marks, and where to get breakfast.
旁白|两人拉好门栓，朝村庄走去。归灯驿站的第一夜结束，真正的重建才刚要开始。|They secure the door and walk towards the village. Guidelight's first night is over. Its real rebuilding is about to begin.
''')

chapter = dict(id='tavern-prologue', title='序章 · 雨里的一盏灯',
 description='全新序章 · 雨夜召唤 · 雷电吟唱 · 女骑士相遇 · 战斗暂跳 · 临时屋檐',
 source='原创双语叙事。无主线听力选择题。战斗演出暂未制作，以过渡字幕跳过。',
 audio='audio/tavern/prologue-v2.wav', audioLabel='离线英文角色配音', background='art/prologue-v2/dorm.png',
 unlockLevel=1, sortOrder=1000, pixelArt=True, inlineQuestions=True, timeLimitSeconds=0,
 actors=[dict(id='luchuan',image='art/prologue-v2/luchuan.png',side='left',gender='male'),
         dict(id='iserya',image='art/prologue-v2/iserya.png',side='right',gender='female'),
         dict(id='aelia',image='art/prologue-v2/aelia.png',side='right',gender='female')],
 decisions=[],questions=[],lines=lines,ending='第2天早上，多云。雨夜应急避险完成；厨房、客房和正式营业尚未开放。前往村庄，开始第一章的重建。')
(ROOT/'chapters/10-tavern-prologue.json').write_text(json.dumps(chapter,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
(ROOT/'assets/story/prologue-v2/segments.json').write_text(json.dumps(segments,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
doc = ['# 序章重制：雨里的一盏灯\n','实际接入版。CG 800 × 800；对话背景无人物；立绘独立显示；战斗暂跳。\n']
for seg in segments:
    doc.append('## '+seg['name']+'\n\n'+seg['caption']+'\n')
    if lines[seg['first']].get('cgBefore'):doc.append('【CG】'+lines[seg['first']]['cgBefore']+'\n')
    for n in range(seg['first'],seg['last']+1):
        line=lines[n];doc.append(f"**序-{n+1:03d}｜{line['speaker']}：** {line['translation']}\n\n{line['text']}\n")
(ROOT/'项目文档/序章重制交付/序章-逐句中英演出剧本.md').write_text('\n'.join(doc),encoding='utf-8')
print(json.dumps(dict(lines=len(lines),segments=segments,cg=[(i,l['cgBefore']) for i,l in enumerate(lines) if l.get('cgBefore')]),ensure_ascii=False))
