"""Move the first service to section six without changing sections one and two."""
import json
from pathlib import Path

root = Path(__file__).resolve().parents[1]
path = root / 'tools/chapter-one-continuation.json'
sections = json.loads(path.read_text(encoding='utf-8'))
if sections[0]['name'] == '午后：开门前的准备':
    raise SystemExit('Revision already applied.')
s = sections[0]
s.update(name='午后：开门前的准备', introScene='chapter-three-preparation.png',
         outroScene='chapter-three-ready.png',
         intro=['午后的驿站还未开门。食材送到了，空白的菜单和采购单一起放在桌上。', 'The waystation is not open yet. The supplies have arrived, and a blank menu lies beside the shopping list.'],
         outro=['菜单与分工已经确定，门牌仍写着尚未营业。晶石的裂痕提醒他们，准备还没有结束。', 'The menu and duties are agreed, but the sign still says not yet open. The cracked crystal reminds them that their preparations are not complete.'],
         ending='完成食材核对、试菜与分工；驿站尚未正式营业。守卫带来矿道线索，晶石仍需碎片修复。')
s['dialogue'] = [
 ['旁白','陆川把买来的蔬菜放上厨房桌。门牌还写着尚未营业，艾莉娅展开采购单。','Lu Chuan sets the vegetables on the kitchen table. The sign still says not yet open, and Aelia unfolds the shopping list.'],
 ['陆川','带去三十枚铜币，食材花了十八枚。剩下十二枚，先留作下一次采购钱。','I took thirty copper coins and spent eighteen on ingredients. The remaining twelve are for our next supplies.'],
 ['艾莉娅','先把备用金单独放好。今天还没有客人付钱，采购后的余额不能算利润。','Keep that reserve separate. No customer has paid us yet, so the money left after shopping is not profit.'],
 ['陆川','记住了。先把店准备好，再想营业的事。','Understood. First we prepare the inn, then we think about opening.'],
 ['艾莉娅','菜单先定三样：蔬菜汤、面包和煎蛋。不接我们做不出来的菜。','Start with three dishes: vegetable soup, bread, and fried eggs. Do not offer food we cannot prepare.'],
 ['旁白','后门传来敲门声。送货人把面粉袋和鸡蛋篮放在门边，货车停在卸货区。','A knock comes at the back door. The delivery worker leaves flour and eggs beside it, with his cart in the unloading yard.'],
 ['送货人','面粉两袋，鸡蛋一篮。请先核对数量，签字以后我还要赶下一趟。','Two sacks of flour and one basket of eggs. Please check the quantities before signing. I have another delivery to make.'],
 ['陆川','两袋面粉、一篮鸡蛋。艾莉娅，先看鸡蛋有没有破，再核对采购单。','Two sacks of flour and a basket of eggs. Aelia, check for broken eggs first, then compare the delivery with our list.'],
 ['艾莉娅','数量对得上。这几只裂了，先换好再签收，不能混进厨房。','The quantities match. These eggs are cracked; replace them before we sign. They must not go into the kitchen.'],
 ['送货人','说得对，我从备用篮里换。你们还没开门？','You are right. I will replace them from the spare basket. Are you not open yet?'],
 ['陆川','还在准备。第一次营业得让人安心，不能先挂招牌再补漏洞。','We are still preparing. Our first service should be reliable. We should not open first and fix problems later.'],
 ['旁白','送货人换好鸡蛋，带走签过字的收货单。艾莉娅把面粉移到干燥的架子上。','The worker replaces the eggs and takes the signed receipt. Aelia moves the flour to a dry shelf.'],
 ['艾莉娅','现在试菜。你盛汤，我准备面包，出菜前把要求再说一遍。','Now we test the dishes. You serve the soup, and I prepare the bread. Repeat the request before serving.'],
 ['陆川','那我先出一道题：一碗不加鸡蛋的汤，两片面包打包。','Here is a practice order: one bowl of soup without eggs, and two slices of bread to take away.'],
 ['艾莉娅','汤在这里喝，面包打包。先听清楚，比急着端出去更重要。','The soup is eaten here, and the bread is packed. Listening carefully matters more than serving quickly.'],
 ['陆川','厨房门和柜台之间留出通道，热汤端出来时别碰到搬食材的人。','Keep the passage between the kitchen and counter clear, so hot soup does not collide with someone carrying supplies.'],
 ['旁白','两人试过一遍出菜路线。桌椅擦净，四张圆桌间留出了足够的通道。','They test the serving route. The furniture is clean, and clear passages remain between the four round tables.'],
 ['艾莉娅','准备和营业分开记。今晚正式开门前，再检查门锁、灯和食材。','Record preparation separately from service. Before opening tonight, check the locks, lights, and ingredients again.'],
 ['旁白','巡逻守卫在门口停下，没有进来用餐。他将一张矿道附近的记录交给艾莉娅。','A patrol guard stops at the door without coming in for a meal. He hands Aelia a note about the area near the mine.'],
 ['守卫','昨晚路障旁出现了陌生脚印。我已经报告值守官，但还没确认来源。','Unfamiliar tracks appeared beside the barrier last night. I have reported them to the watch officer, but their source is still unknown.'],
 ['艾莉娅','谢谢。我们会提醒路过的人别抄近道，不把猜测当结论。','Thank you. We will warn travelers against the shortcut, without treating a guess as a conclusion.'],
 ['陆川','晶石还没有亮起来。把桌子摆好，不能直接修好它，对吧？','The crystal has not brightened yet. Arranging the tables cannot repair it, can it?'],
 ['言契系统','相聚可以稳定封印的能量，碎片才能修复晶石的结构。当前裂痕仍然存在。','Gathering can stabilize the energy of the seal. Fragments are needed to repair the crystal itself. Its crack remains.'],
 ['旁白','陆川合上采购记录，留下空白的营业页。门牌没有翻动，第一笔订单还在等他们准备好。','Lu Chuan closes the supply record, leaving the sales page blank. The sign stays unchanged; their first order must wait until they are ready.']
]
s['questions'] = [
 [5,'预算与营业准备','Why must the twelve remaining coins not be counted as profit?',['They were paid by a customer.','They are a reserve from the supply budget, and sales have not begun.','They belong to the delivery cart.','They will repair the crystal.'],1,'十二枚是采购预算的余额，尚无营业收入，不是利润。'],
 [11,'收货核对','What must happen before they sign the delivery receipt?',['They must sell the flour.','They must open the front door.','The cracked eggs must be replaced.','The worker must order soup.'],2,'先检查数量与品质，更换破损鸡蛋后再签收。'],
 [17,'试菜与分工','Why do they test the route between kitchen and counter?',['To keep the serving passage clear and safe.','To hide the ingredients.','To start selling before they are ready.','To move the fireplace.'],0,'试走出菜路线，避免热汤与搬运人员碰撞。'],
 [23,'晶石修复规则','What is needed to repair the crystal itself?',['A larger dining table.','More coins in the ledger.','A new sign by the door.','The missing crystal fragments.'],3,'相聚稳定能量，碎片修复结构；此时尚未营业。']
]
s = sections[1]
s['intro'] = ['试菜刚结束，莉瑟提着灯来到尚未营业的驿站。', 'The practice meal has just ended when Lyse arrives with her lantern. The waystation is not open yet.']
s['dialogue'][11] = ['陆川','你是来帮忙确认晶石的，不必用自己的秘密交换一碗试菜的汤。','You came to help us examine the crystal. A bowl of our practice soup does not require you to share your secrets.']
s['dialogue'][18] = ['旁白','艾莉娅检查水袋和盾带。陆川把门牌留在尚未营业的一面，并写下预计返回的时间。','Aelia checks the water flask and her shield strap. Lu Chuan leaves the sign at not yet open and writes down their expected return time.']
s['dialogue'][20] = ['陆川','先把晶石的线索查清，再回来开门。备用金和食材都留好了。','We will check the crystal clue before returning to open. Our reserve and supplies are ready.']
s = sections[3]
s['name'] = '夜晚：第一次营业'
s['intro'] = ['三人带着碎晶回到尚未开门的驿站。修复晶石后，他们将迎来第一批客人。','The three return to the unopened waystation with a crystal fragment. After repairing the crystal, they will welcome their first customers.']
s['outro'] = ['第一次营业留下了六枚铜币的盈余。晶石的光稳定下来，更深的裂痕仍等待修复。','Their first service leaves a profit of six copper coins. The crystal shines steadily, but its deeper cracks still need repair.']
s['outroScene'] = 'chapter-six-first-profit.png'
s['dialogue'][1] = ['艾莉娅','门牌还没翻。先把碎片放到裂口旁，确认安全以后再开门。','The sign has not been turned yet. Place the fragment beside the crack, and we will open once it is safe.']
service = [
 ['旁白','陆川洗净手，艾莉娅重新热好汤。莉瑟点亮桌灯，门牌第一次翻到营业的一面。','Lu Chuan washes his hands, and Aelia reheats the soup. Lyse lights the table lamps, and the sign turns to open for the first time.'],
 ['旁白','送货人收工后回来，两名巡逻守卫随后进门。准备好的四张圆桌终于迎来了客人。','The delivery worker returns after his work, followed by two patrol guards. The four prepared round tables finally welcome customers.'],
 ['送货人','一碗不加鸡蛋的汤，两片面包带走。我记得你说过，准备好了才开门。','One bowl of soup without eggs, and two slices of bread to take away. I remember you said you would open only when ready.'],
 ['陆川','汤不加鸡蛋，面包打包。今晚是第一次营业，谢谢你愿意来。','Soup without eggs, bread packed to go. This is our first service. Thank you for coming.'],
 ['艾莉娅','我来装面包，你先端汤。门口和厨房的通道都留空，照我们试过的顺序来。','I will pack the bread while you serve the soup. Keep the entrance and kitchen passage clear, and follow the order we practiced.'],
 ['旁白','碗勺声和谈笑声渐渐填满大厅。晶石的微光稳定下来，但没有新的裂缝自行消失。','Spoons and conversation gradually fill the hall. The crystal glows steadily, but no further crack disappears on its own.']
]
s['dialogue'][6:6] = service
s['dialogue'][27] = ['旁白','账本里夹着采购单、第一批订单和矿道记录。直到这一晚，驿站才真正完成第一次营业。','The ledger holds the shopping list, the first paid orders, and the mine notes. Only tonight has the waystation completed its first service.']
for q in s['questions'][1:]:
 q[0] += 6
path.write_text(json.dumps(sections, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print('Revised sections 3, 4 and 6; preserved section 5 and all Lyse dialogue.')
