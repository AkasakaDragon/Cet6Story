# 出牌施法动作

模式：内置 imagegen，透明图像编辑；参考 support-sprites.png 的男主身份。三个独立 4 列 × 2 行动作图保存为 cast-0.png、cast-1.png、cast-2.png。

公共提示：transparent PNG sprite atlas, EXACT 4 columns by 2 rows, eight evenly spaced full-body frames, read left-to-right then bottom row. Pixel art black-haired male, cyan eyes, black coat with cyan edging, gray trousers, black boots; faces right. Identical proportions and feet baseline. Complete head, hands, coat tails and boots inside each cell with generous margins. No weapons, woman, monsters, painted magic, labels, grid or background. Sharp pixel edges, limited palette.

- cast-0: One handed rune casting: hand rises from waist, fingers draw sigil, palm held forward, release, returns to relaxed stance.
- cast-1: Two handed gathering: hands lift near chest, cups hands together, separates hands to frame an orb, both palms extend, returns to relaxed stance.
- cast-2: Overhead invocation: hand moves chest to above head, other hand balances, raised palm opens, sweeping hand down in release, returns to relaxed stance.

运行时按透明行分开上下动作，保留所有 alpha 部件，统一脚底位置。施法特效在男主身边绘制；不产生攻击弹道。三种合成音效经游戏音效音量与总音量控制。
