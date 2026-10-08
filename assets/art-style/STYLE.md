# 游戏图片默认画风

## 酒馆时段与天气一致性（2026-10-08）

- 用户最新确认的颜色参考原图为 `references/tavern-palette-approved.png`。营业准备各天气版本按此图保持棕色木材、深绿色织物与金色灯光的浓度；消除整幅偏红，但不要过度去饱和变成灰淡画面。该图提供配色依据，六张既定画风参考仍适用。

- 早上、黄昏、晚上及营业准备的各天气背景，后门右侧、吧台左侧均须保留同一位置的木框菱格玻璃窗。窗是固定玻璃窗，窗台下为墙面，不能画成第二道门或通道，也不能在移除多余门时填掉窗。
- 室内基础配色以 `assets/waystation/business-preparation-night-preview.png` 的自然棕色木材、中性米灰墙面、灰蓝石地板及克制绿色为准；减少整幅红橙偏色与过高饱和度。灯火保留局部柔和琥珀暖光，各时段通过窗外天色、曝光及光照方向体现差异。
- 早上背景不绘制公告栏右侧、后门左侧的竖版食品菜单牌；保留公告栏。营业准备背景中的独立落地菜单板仍保留。

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

- 2026-10-07 用户指定所有后续剧情 CG 默认使用人物主导的动画分镜思维：通常人物占 60%～80%，根据剧情采用人物中近景、表情/手/道具特写、剪影、环境局部或情绪背景。环境保持 1～2 个标志物，避免每张都展示完整房间。背景弱化通过像素色块、降低对比和暗化实现，避免模糊滤镜。
- 每张 CG 增强像素感觉：明显可辨的方形像素簇、阶梯轮廓、节制的局部调色板和有取舍的细节。角色身份、服饰、肤色与基础色保持一致；不要光滑动漫/3D，也不默认采用极粗低分辨率马赛克。用户对单张提出更强像素化或全景要求时按具体要求执行。

- `references/tavern-structure.png` 当前为用户否决的结构草稿，不能作为已确认参考。新版须满足二层完整环绕挑空大厅、后门外后院卸货区、吧台厨房门廊及厨房下到酒窖独立楼梯，待用户确认后再作为酒馆 CG 固定结构依据。

- 本规范是后续新生成游戏图片的默认视觉方向，优先于旧美术说明中的粗像素风描述；既有角色设定、卡牌含义、索引和裁切约束继续遵守。
- 生成前查看所选参考图，并将原图传入图片生成工具。保持像素尺度协调，导出及显示缩放尽可能使用 nearest-neighbor，避免像素边缘被平滑。
- 检查主体辨识度、环境纵深、冷暖焦点、像素边缘以及角色设定是否一致。
