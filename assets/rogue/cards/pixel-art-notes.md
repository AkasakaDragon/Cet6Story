# 卡牌像素插画

使用内置 imagegen 工具，根据原 illustrations.png 图集及游戏男女主像素角色参考重绘。最终素材为 illustrations.png，16 个单元按四行四列排列；行优先顺序对应 CardBattle.Cards 的 art 索引 0–15。卡框、名称、数值和技能规则由游戏继续绘制。

## 最终生成提示词

Redraw image1 (entire 4x4 square card art atlas, sixteen equal square cells, no gutters) into genuine chunky PIXEL ART matching image2's sprites. Image2 is the character identity and pixel style reference: male short black hair, black coat CYAN trim and gray pants; female long silver hair, purple top, black coat PURPLE lining, black pants/boots. Retain exactly all sixteen scene meanings and positions from image1: row1 tactical duo aim, blue shield duo, red crosshair on stone armor, gold duo attack; row2 male focus star, female cleanse purple spores, gold armor breaking shot, blue emergency cover; row3 duo charging shot, piercing gold beam, strong blue-violet dome, male manipulating cards; row4 duo connected by stars, duo star burst, male healing wounded female (no gore), female final pistol shot with male and purple rune. Visible square pixels, dark stepped outlines, limited palette, pixel sprite faces, 3 tone shading, blocky energy sparks. Each square resembles hand drawn 96x96 game art enlarged sharply. NO smooth anime painting, realistic rendering, soft gradients, text, borders or UI. Composition centered for oval card crop. Muted night ruins, subtle gold/cyan/violet spell effects. Clean perfectly uniform 4 by 4 atlas grid. Preserve character clothing from image2, not gold outfits from image1.

## 显示验证

使用实际 SupportCardView 渲染全部 16 张牌，检查椭圆裁切、角色配色、技能图案与原索引对应关系。插画采用 nearest-neighbor 缩放保留像素轮廓；卡框和文字保持原来的绘制方式。
