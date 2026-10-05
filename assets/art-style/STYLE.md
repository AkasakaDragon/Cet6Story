# 游戏图片默认画风

用户于 2026-10-05 指定六张参考图作为后续游戏图片的画风依据。原图保存在 `references/reference-1.png` 至 `reference-6.png`。生成时按场景选取相关原图作为视觉参考，不能只依赖文字提示。

## 核心特征

- 细腻的像素绘画：清楚的方形像素、阶梯状边缘，用有组织的像素色块描绘材质；细节丰富，但主体轮廓易读。不是极低分辨率的粗颗粒小精灵。
- 复古 RPG 冒险插画氛围，兼有童话感和史诗感。场景具有叙事性，人物、怪物和环境之间有明确关系。
- 前景、中景、远景分层，利用遮挡、明暗和远处降低对比度形成纵深。树林、山峰、建筑等环境承担主要气氛塑造。
- 人物轮廓简洁，服饰装备以像素块表达；避免光滑动漫脸、写实皮肤和塑料感 3D 材质。远景人物可以较小，近景人物仍保持像素绘画质感。
- 色彩丰富但每个局部的色阶克制。阴影常用深蓝、蓝绿、紫褐，亮部用淡黄、米白、嫩绿；不要给所有场景套同一色调。
- 光照通过像素色块和层次表现：阳光、林间光束、雾气、火焰和魔法都保留像素边缘，避免光滑渐变与模糊滤镜。
- 大面积冷色环境与少量暖色焦点形成对比，保持视觉中心清楚。不要让粒子和光效淹没主体。

## 六张图的场景侧重

1. 开阔草原：蓝天、奶白云、明亮绿色，人物在前景，远处城堡构成目标与纵深。
2. 酒馆群像：木质棕色与深青阴影，暖黄灯光，丰富生活细节，角色关系清晰。
3. 雪地战斗：冰蓝与深蓝，大怪物和小英雄形成尺度对比，橙色火焰承担视觉焦点。
4. 神秘森林：青绿薄雾、深色树木框景、层层植被，少量橙红发光点，安静而奇幻。
5. 林间旅途：自然绿、斑驳日光、土色小路，用道路和树木引导视线。
6. 山地史诗：蓝灰山脉、远景空气透视、深色角色剪影，宏大尺度与克制色彩。

## 生成基础提示词

Detailed pixel-painted illustration for a narrative adventure RPG, matching the supplied style reference images. Deliberate visible square pixel clusters, crisp stepped edges, rich handcrafted environmental detail, readable silhouettes, restrained local color ramps, layered foreground/midground/background, atmospheric perspective expressed through pixel colors, cinematic storytelling composition. Deep blue/teal or violet-brown shadows, scene-appropriate natural colors, carefully placed warm light accents. Render foliage, stone, wood, fabric, clouds, mist and light with structured pixel clusters. Preserve the game's established character identities, costumes and setting. No smooth anime rendering, no photorealism, no glossy 3D, no vector look, no blurred pixel filter, no smooth airbrushed gradients, no text, watermark, frame or UI unless explicitly requested.

追加具体场景、角色、用途、尺寸和构图要求。卡牌等小尺寸图片须适当简化细节并保持裁切后可读，仍使用上述画风。参考图中的既有作品角色只用于理解画风，不默认复制到本游戏。

## 使用与验收

- 本规范是后续新生成游戏图片的默认视觉方向，优先于旧美术说明中的粗像素风描述；既有角色设定、卡牌含义、索引和裁切约束继续遵守。
- 生成前查看所选参考图，并将原图传入图片生成工具。保持像素尺度协调，导出及显示缩放尽可能使用 nearest-neighbor，避免像素边缘被平滑。
- 检查主体辨识度、环境纵深、冷暖焦点、像素边缘以及角色设定是否一致。
