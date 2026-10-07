"""Keep authored chapter JSON and rebuild scripts in sync without regenerating voices."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
additions = [
    [('11-tavern-01-01.json'), 'create-waystation-chapter-one.py', [
        dict(afterLine=15, skill='菜单选择与原因', prompt='Why do they keep the first menu simple?', options=['They want to serve dishes they can prepare well.', 'Travelers never need hot food.', 'All ingredients are free.', 'Aelia refuses to use the kitchen.'], answer=0, explanation='第一天先把热汤、面包、煎蛋三样做好；能实际提供餐食比漂亮的长菜单重要。'),
        dict(afterLine=21, skill='采购优先级', prompt='What should Lu Chuan buy before looking at attractive signs?', options=['Expensive decorations.', 'A new roof.', 'Gifts for every traveler.', 'The essentials on the shopping list.'], answer=3, explanation='艾莉娅提醒他保管清单和钱，先买必需品，不被漂亮招牌带走。')]],
    ['12-tavern-01-02.json', 'create-waystation-chapter-two.py', [
        dict(afterLine=5, skill='采购数量与送货', prompt='What does Lu Chuan ask the seller to supply?', options=['A large barrel of oil.', 'One small bag of flour and twelve eggs.', 'Twelve bags of flour and one egg.', 'Only a new roof.'], answer=1, explanation='陆川先订一小袋面粉和十二个鸡蛋，摊主可送到驿站。'),
        dict(afterLine=27, skill='邀请与人物需求', prompt='What matters to Lyse more than a long menu?', options=['The size of the market.', 'An expensive sign.', 'A place to sit and some hot soup.', 'A room inside the mine.'], answer=2, explanation='莉瑟说赶路的人会喜欢热汤，比起一长串菜名，她更关心有没有座位。')]],
]
for filename, scriptname, extra in additions:
    path = ROOT / 'chapters' / filename
    chapter = json.loads(path.read_text(encoding='utf-8-sig'))
    for question in extra:
        if not any(q['afterLine'] == question['afterLine'] for q in chapter['questions']):
            chapter['questions'].append(question)
    chapter['questions'].sort(key=lambda q: q['afterLine'])
    chapter['source'] = '原创奇幻剧情与四道分段听力理解题，非考试原文。'
    path.write_text(json.dumps(chapter, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    script = ROOT / 'tools' / scriptname
    text = script.read_text(encoding='utf-8-sig')
    start = text.index('questions=[') + len('questions=')
    end = text.index('],ending=', start) + 1
    text = text[:start] + repr(chapter['questions']) + text[end:]
    text = text.replace('原创奇幻剧情及两道分段听力理解题', '原创奇幻剧情与四道分段听力理解题')
    script.write_text(text, encoding='utf-8')

source = ROOT / 'source/Game.cs'
text = source.read_text(encoding='utf-8-sig')
text = text.replace('c.questions.Count!=2||c.questions.Any', '(c.questions.Count<3||c.questions.Count>4)||c.questions.Any')
text = text.replace('c.questions.Select(q=>q.afterLine).Distinct().Count()!=2', 'c.questions.Select(q=>q.afterLine).Distinct().Count()!=c.questions.Count')
text = text.replace('驿站章节需有两道有效分段题', '驿站章节需有三至四道有效分段题')
old = 'if(save.lastChapter==WaystationChapterOne.SecondId&&!save.completed.Contains(WaystationChapterOne.SecondId)){EnterChapterOneSecond();return;}if(save.lastChapter==WaystationChapterOne.Id&&!save.completed.Contains(WaystationChapterOne.Id)){EnterChapterOne();return;}'
text = text.replace(old, 'if(WaystationChapterOne.Ids.Contains(save.lastChapter)&&!save.completed.Contains(save.lastChapter)){EnterWaystationSection(save.lastChapter);return;}')
source.write_text(text, encoding='utf-8')
for filename in ['ChapterOneChecks.cs', 'ChapterTwoChecks.cs']:
    path = ROOT / 'tools' / filename
    text = path.read_text(encoding='utf-8-sig').replace('c.questions.Count==2', 'c.questions.Count==4').replace('two questions', 'four questions')
    path.write_text(text, encoding='utf-8')
