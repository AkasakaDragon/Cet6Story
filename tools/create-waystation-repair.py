"""Append the approved post-battle repair sequence without changing earlier dialogue."""
from pathlib import Path
import json, shutil
ROOT=Path(__file__).resolve().parent.parent
chapter_path=ROOT/'chapters/10-tavern-prologue.json'
chapter=json.loads(chapter_path.read_text(encoding='utf-8-sig'))
images=[
('codex-clipboard-44299c32-4dff-43eb-8aa9-34fbeb395076.png','waystation-repair-0.png'),
('codex-clipboard-71763211-268a-47f9-986f-c29595360dac.png','waystation-repair-1.png'),
('codex-clipboard-7cfc7324-4659-4b0c-a7cd-fd4bf4d8ce8f.png','waystation-repair-2.png'),
('codex-clipboard-aad22d0b-0d7b-4c38-8a34-0a39501c9db8.png','waystation-repair-morning.png')]
for original,name in images:
    shutil.copy2(Path('C:/Users/33370/AppData/Local/Temp')/original,ROOT/'chapters/art/tavern'/name)
dialogue=[
('艾莉娅','It is over. Are you hurt?','结束了……你没受伤吧？'),
('陆川','No. I only just noticed that my legs are still shaking.','没有。只是现在才发现，我的腿一直在抖。'),
('艾莉娅','Thank you. Your barrier stopped its claws from reaching me.','谢谢你。刚才那道屏障替我挡住了它的爪子。'),
('陆川','You stood in front of me. I could not just watch.','你挡在我前面，我总不能只站着看。'),
('艾莉娅','This roof will not keep you dry. You helped me with the beast. Let me help you repair the waystation.','这里已经不能遮雨了。你帮我解决了魔物，我也帮你把驿站修起来。'),
('旁白','They recover usable boards and support the damaged beam. The first patch keeps the rain away.','他们找出还能用的木板，撑住受损的横梁。第一块补板挡住了头顶的雨。'),
('艾莉娅','Hold this side. I will secure it. Keep clear of the beam.','扶住这一边。我来固定，别站到横梁下面。'),
('陆川','You know how to repair a roof, too?','你连修屋顶都会？'),
('艾莉娅','On border patrol, finding a place to sleep can be harder than finding an enemy.','边境巡防时，找到能睡的屋子比找到敌人还难。'),
('陆川','Then I will pass you the boards. I can start by being a useful helper.','那我负责递木板。今天先从合格的帮手做起。'),
('旁白','Rain no longer reaches the hall. They clear the rubble and bring old rugs, lamps, and potted plants out of storage.','雨终于不再落进大厅。两人清走碎石，把储藏室里的旧地毯、灯具和盆栽搬了出来。'),
('陆川','I thought I would have to figure everything out alone after arriving here.','我本来以为，来到这里之后，所有事都得自己想办法。'),
('艾莉娅','On the border, no one can manage alone forever.','在边境，没人能一直靠自己撑下去。'),
('陆川','When we can welcome guests, I want travelers to have somewhere to rest.','等这里能接待客人，我想让路过的人都能有个歇脚的地方。'),
('艾莉娅','Then let us light the hearth. A place to return to should be warm.','那就先把炉火点起来。归处应该是暖的。'),
('旁白','By the time they secure the final board, morning has arrived. Sunlight fills the waystation, and the floor is dry at last.','翌日清晨。最后一块木板固定时，天已亮了。晨光照进驿站，昨夜的积水终于退去。'),
('陆川','We actually fixed it. Last night, I thought it might collapse.','真修好了……昨晚这里还像随时会塌。'),
('艾莉娅','The roof and stairs are secure. We can use the hall now and improve the rest gradually.','屋顶和楼梯已经稳住，能先使用了。剩下的地方可以慢慢整理。'),
('陆川','Breakfast is on me. You are our first guest.','第一份早餐我请。你可是这里的第一位客人。'),
('艾莉娅','A guest? After working all night, surely I count as your helper.','客人？我干了一整夜的活，至少该算你的帮手吧。')]
chapter['lines']=chapter['lines'][:25]
for i,(speaker,en,zh) in enumerate(dialogue):
    chapter['lines'].append(dict(speaker=speaker,voiceRole=speaker,actor={'陆川':'luchuan','艾莉娅':'aelia'}.get(speaker,''),text=en,translation=zh,scene='art/tavern/'+images[i//5][1],sceneSingle=True,start=0,end=1))
chapter['description']='穿越异界 · 初遇巡防骑士 · 卡牌支援战斗 · 一起修复驿站'
chapter['ending']='驿站基础修复完成。陆川与艾莉娅一起迎来天亮，经营基础已解锁。第一章“今天开始营业”后续开放。'
chapter_path.write_text(json.dumps(chapter,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
