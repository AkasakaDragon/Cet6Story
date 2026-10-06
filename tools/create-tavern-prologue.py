"""Author the independent prologue; audio timings are filled by the audio builder."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
scenes = ['goddess-sanctuary', 'tavern-ruins', 'tavern-defense']
segments = [
[
('伊瑟雅', 'Welcome, Lu Chuan. I am Iserya. I need your help to protect another world.', '欢迎你，陆川。我是伊瑟雅。我需要你帮助守护另一个世界。'),
('陆川', 'A goddess? Then I suppose this is more serious than falling asleep at my desk.', '女神？看来这比趴在桌上睡着严重多了。'),
('伊瑟雅', 'A demon king is sealed beneath a border town. The seal is weakening.', '一位魔王被封印在边境小镇地下。如今，封印正在衰弱。'),
('伊瑟雅', 'He cannot escape yet, but his soldiers can cross the cracks and attack our people.', '他还无法脱身，但他的士兵已经能穿过裂隙，袭击这里的人们。'),
('陆川', 'So you need a hero with a legendary sword?', '所以，你需要一位拿着传说之剑的勇者？'),
('伊瑟雅', 'Actually, I need someone who can run a tavern. Its crystal holds the surface seal together.', '其实，我需要一个能经营酒馆的人。那里的晶石维系着地面封印。'),
('陆川', 'Saving the world now requires experience in hospitality? That was not in my study plan.', '现在拯救世界还要求餐饮经验？我的复习计划可没这项。'),
('伊瑟雅', 'Repair the tavern, welcome guests, and recover crystal fragments from the dungeon below.', '修好酒馆，迎接客人，再到下面的地下城找回晶石碎片。'),
('伊瑟雅', 'Their shared warmth will power the crystal. The fragments will repair its broken structure.', '人们相聚的温暖会为晶石供能，碎片则能修复它破损的结构。'),
('言契系统', 'Language contract connected. Answer vocabulary questions to activate support cards for your allies.', '语言契约已连接。回答词汇题，即可为同伴发动支援卡牌。'),
('陆川', 'The instructions make sense. A sword is beyond me, but I can work with these cards.', '指令我看懂了。用剑我不在行，不过，这些卡牌我能试试。'),
('伊瑟雅', 'Once the seal is stable, I can open a safe path home. Until then, you will have help.', '等封印稳定下来，我就能打开安全的归途。在那之前，你也会有同伴。'),
],
[
('旁白', 'The light fades. Rain drips through a broken roof onto the floor of an abandoned tavern.', '光芒消退。雨水穿过破损的屋顶，滴在废弃酒馆的地板上。'),
('陆川', 'Good news: I own a business. Bad news: the business might collapse before the world does.', '好消息：我有自己的店了。坏消息：这家店可能比世界先垮。'),
('旁白', 'A cracked crystal sits behind the counter. Its faint light flickers with every drop of rain.', '吧台后放着一块开裂的晶石。每滴雨落下，它的微光都会晃动。'),
('言契系统', 'Seal core located. Structural damage is severe. Begin with shelter, food, and a working hearth.', '已定位封印核心。结构损伤严重。请先恢复遮雨、食物与炉火。'),
('陆川', 'A roof, hot meals, and somewhere to sit. At least the first steps are understandable.', '能挡雨，有热饭，还有地方坐。至少第一步听起来很明白。'),
('旁白', 'Something strikes the door. A wounded knight stumbles inside, followed by a spiny creature covered in purple mushrooms.', '有什么撞上大门。一名受伤的骑士跌进来，身后追着一只长满紫色蘑菇的荆棘孢子兽。'),
('艾莉娅', 'Get behind me! Stay away from the door, and do not try to fight it!', '躲到我身后！离门远一点，不要试着和它打！'),
('旁白', 'She sees my empty hands and raises her shield, placing herself between me and the monster.', '她看见我两手空空，立刻举起盾牌，挡在我和魔物之间。'),
('陆川', 'I cannot use a sword, but I can help. Keep your shield up for a moment.', '我不会用剑，但我能帮忙。请再举盾撑一会儿。'),
('艾莉娅', 'Then help by staying alive. I will draw its attention away from you.', '那就先保住自己。我会把它的注意力引开。'),
],
[
('言契系统', 'Ally identified: Aelia, a holy knight. Support link ready. The spore beast is gathering poisonous spores.', '已识别同伴：圣骑士艾莉娅。支援连接就绪。荆棘孢子兽正在聚集毒孢子。'),
('陆川', 'I can see its next move. Give me a moment to strengthen your defense.', '我能看见它下一步的行动。给我一点时间，我来增强你的防御。'),
('旁白', 'Two cards glow before me: a protective barrier and a tactical guide. The contract waits for my answer.', '两张卡牌在我面前亮起：守护屏障与战术指引。契约等待我的回答。'),
('艾莉娅', 'There is light around my shield. Was that your magic?', '我的盾牌周围出现了光。这是你的魔法？'),
('陆川', 'Yes. You handle the sword. I will make sure you have the opening you need.', '是。你来挥剑，我来给你创造机会。'),
('言契系统', 'Choose your support cards, then answer. Your ally will strike when the contract is complete.', '选择支援卡牌，再作答。契约完成后，同伴将发动攻击。'),
('旁白', 'Her blade cuts through the purple spores. The creature falls, and the room becomes quiet again.', '她的剑锋穿过紫色孢子。魔物倒下，屋里终于重新安静。'),
('艾莉娅', 'I thought you were an ordinary traveler. I am Aelia. Thank you for standing with me.', '我还以为你是普通旅人。我叫艾莉娅。谢谢你和我一起战斗。'),
('陆川', 'Lu Chuan. Apparently, I am the new owner. You have seen the state of the place.', '陆川。看来，我是这里的新老板。这家店的情况，你也看见了。'),
('艾莉娅', 'Then we should start with the roof. I can guard the tavern while you make it a home.', '那就先从屋顶开始吧。你把这里变成归处，我来守护它。'),
('旁白', 'We clear a table and light the old hearth. The crystal answers with a small, steady glow.', '我们清理出一张桌子，点燃旧炉火。晶石回应了一点微弱而稳定的光。'),
('陆川', 'Tomorrow, we open for business. Tonight, you can finally put down that shield.', '明天，我们开始营业。今晚，你终于可以把盾牌放下了。'),
('艾莉娅', 'Only if you promise to wake me when you need help.', '那你得答应，需要帮助的时候就叫醒我。'),
('旁白', 'Outside, the rain continues. Inside, our first lamp stays lit. A new story has begun.', '门外，雨还在下。门内，我们的第一盏灯没有熄灭。新的故事开始了。'),
],
]
lines = []
for scene, segment in zip(scenes, segments):
    for speaker, text, zh in segment:
        lines.append(dict(speaker=speaker, actor='', voiceRole=speaker, text=text, translation=zh,
                          scene=f'art/tavern/{scene}.png', sceneSingle=True, start=0, end=0))
for index, line in enumerate(lines):
    if 17 <= index <= 21:
        line['scene'] = 'art/tavern/tavern-entrance-spore.png'
    elif 22 <= index <= 27:
        line['scene'] = 'art/tavern/tavern-defense-spore.png'
    elif index >= 28:
        line['scene'] = 'art/tavern/tavern-first-light.png'
questions = [
dict(afterLine=1, skill='人物身份与动机', prompt='Why does Iserya welcome Lu Chuan?', options=['She needs his help to protect another world.', 'She wants him to finish his exam.', 'She wants him to return to his dormitory.', 'She is asking him to repair her computer.'], answer=0, explanation='女神说明，她需要陆川帮助守护另一个世界。'),
dict(afterLine=11, skill='因果关系', prompt='What must Lu Chuan do to strengthen the seal?', options=['Buy a legendary sword and leave the town.', 'Close the tavern to keep everyone away.', 'Bring the crystal back to his dormitory.', 'Welcome guests and recover crystal fragments.'], answer=3, explanation='女神说明：客人相聚提供能量，地下城碎片修复晶石结构。经营和探索缺一不可。'),
dict(afterLine=21, skill='行为推断', prompt='Why does Aelia stand in front of Lu Chuan?', options=['She thinks he needs protection.', 'She wants him to repair her shield.', 'She believes he controls the monster.', 'She is asking him to leave the tavern.'], answer=0, explanation='她看到男主没有武器，把他当作需要保护的普通人，举盾挡在他身前。'),
dict(afterLine=35, skill='分工与主旨', prompt='How do Lu Chuan and Aelia plan to work together?', options=['Both will abandon the damaged tavern.', 'Aelia will answer every question for him.', 'He will restore the tavern, and she will help protect it.', 'He will guard the town while she returns to his world.'], answer=2, explanation='艾莉娅说：你把这里变成归处，我来守护它。两人建立经营与保护的分工。'),
]
chapter=dict(id='tavern-prologue', title='封印酒馆 · 序幕：第一盏灯', description='CG开场 · 三幕原创剧情 · 独立新主线 · 字幕自由切换 · 卡牌支援教学', source='原创奇幻剧情及四道分段听力理解题，非考试原文。', audio='audio/tavern/prologue.wav', audioLabel='离线英文角色合成配音 · 原创非考试原音', background='art/tavern/goddess-sanctuary.png', unlockLevel=1, sortOrder=1000, pixelArt=True, inlineQuestions=True, timeLimitSeconds=900, actors=[], decisions=[], questions=questions, lines=lines, ending='序幕完成。陆川与艾莉娅点亮归灯酒馆；第一章“今天开始营业”将在后续开放。')
target=ROOT/'chapters/10-tavern-prologue.json'
target.write_text(json.dumps(chapter, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print(f'Authored {len(lines)} lines, {len(questions)} questions: {target}')
