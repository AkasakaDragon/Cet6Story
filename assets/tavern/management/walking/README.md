# 酒馆人物走路素材

使用 image_gen.imagegen 生成，参考各人物原图和 assets/art-style/references/reference-2.png。

- aelia-walk.png：艾莉娅
- lyse-walk.png：莉瑟
- luchuan-walk.png：陆川
- townsperson-walk.png：酒馆客人

每张为 4 × 4 透明精灵表。运行时使用第一行背面左上、第三行正面左下；右上与右下直接水平镜像，保证左右一致。循环使用 0 → 1 → 2 → 1 三个关键姿势，回到起始帧；不使用第四列中朝向不一致的姿势。移动距离驱动帧序，暂停营业时冻结动作。路线以四个斜向段连接。

接入代码：source/TavernHallWalking.cs。
