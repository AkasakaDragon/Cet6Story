# 厨房工作试玩（2026-10-08）

入口：晚上点击酒馆营业 → 营业准备 → 开始营业 → 右侧「进入厨房开始工作」。

沿用厨房原布局：左侧储物架与通往大厅的门，后墙蒸煮锅、石烤炉与煎台，中央切配岛台，右侧装盘区，前右侧独立楼梯通往酒窖。斜上方俯视，WASD 移动，靠近工作台按 E 交互，Esc 返回。

陆川沿用工作服展示图：棕发、米白卷袖衬衫、深蓝马甲与星形扣、亚麻围裙、深裤、棕靴。当前只有胡萝卜，不加入调味品。拿取 → 三次拼写切配 → 拿取 → 任选蒸煮 / 烘烤 / 煎制并完成三次拼写 → 拿取 → 装盘。加工状态保存到存档，错误输入不推进，不重复消耗或重复装盘。

上方显示中文释义与英文拼写输入，下方显示切刀、蒸汽冒泡、烘烤上色或煎锅翻动过程。三次正确输入对应三个加工阶段。初始词组：carrot / slice / chop；water / steam / tender；oven / roast / golden；pan / fry / crisp。

美术由内置图片生成工具制作，原始输入包括 `../waystation/kitchen-ingredient-ledger.png`、`../characters/work/tavern-work-outfits.png`、`../art-style/references/reference-2.png` 与 `../art-style/references/tavern-palette-approved.png`。

最终提示要求：保持既有厨房空间关系，改为高位斜俯视2.5D厨房；清晰细腻像素簇与阶梯边缘，自然棕木、灰石、深绿和局部金色灯光；无遮挡可行走通道、中央岛台与六个交互点；保留陆川工作服身份；加工近景分别呈现切菜板、铁锅、石烤炉和煎锅，不生成文字或界面。

资源：`work-room.png` 场景；`luchuan-work-directions.png` 角色方向图；`process-stations.png` 四格工作台近景；`carrot-stages.png` 胡萝卜状态素材。角色左向通过游戏绘制时镜像右向素材显示。

验证：`tools/KitchenWorkChecks.cs` 检查三条烹饪流程、错题与重复输入、存档恢复、拿取装盘、碰撞及六个工作台可达性，并在隔离存档目录运行实际加工弹窗的三次拼写和拿取流程，输出界面截图。

## 2026-10-08 点击工作台
厨房工作背景采用 assets/waystation/kitchen-woodfire-pixel-preview.png。暂时隐藏主角，移除 WASD 移动和 E 距离交互；食材架、切菜板、蒸煮锅、烘烤炉、煎制台、装盘台均通过对应位置的深绿金边按钮直接操作。保留每轮三次拼写、拿取和装盘流程。已验证六个按钮派发、缩放及三种完整烹饪流程。
