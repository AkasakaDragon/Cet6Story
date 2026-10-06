# 林间驿站主界面

2026-10-05；使用内置 image_gen 工具。用户选择独立的林间驿站场景，最终要求严格匹配项目参考图的画风，不采用大色块卡通风。

画风原图输入：`../art-style/references/reference-5.png`（主要画法）、`reference-4.png`（森林层次）、`reference-2.png`（木质材质）。未传入旧主界面或用户后来提供的遗迹构图图。

## 最终生成提示词

Paint an ORIGINAL forest waystation game main menu background in landscape 16:9, strictly in the exact PIXEL-PAINTED STYLE of the supplied reference images. Image 1 (forest trail) is the dominant style target: faithfully match its pixel cluster size relative to objects, organic tree silhouettes, natural proportions, muted green and blue-gray layered shade, pale dappled sun, ochre earth, hand-placed staggered pixel brushwork. Image 2 guides layered forest depth; image 3 guides credible timber materials only. Do NOT simplify into cute cartoon, chibi, toy cottage, oversized tiles or flat vector shapes. Do NOT invent a new pixel aesthetic, restrict palette artificially, or force a tiny canvas. Keep the same maturity and painterly pixel craftsmanship as reference 1 with deliberately selected detail, not scattered noise. New scene: a quiet forest trail curves through a clearing toward a modest practical wooden waystation on the RIGHT middle distance, tall trees framing edges, a small creek across lower RIGHT, a patch of open pale sky upper center. LEFT third is a quiet shaded clearing for overlaid menu, visually balanced with the cabin. Bright relaxed hopeful afternoon, natural leaf greens and blue-teal shadows, modest warm cream sunlight, no bloom. Original composition, no riders or characters copied from references. No ruined castle or arch bridge, no previous bedroom scene. No people, text, logo, buttons, watermark or UI. Rich but not fussy, no gratuitous particles, magical ornaments or oversaturated shiny foliage. Maintain the reference's visible square pixel clusters and crisp stepped edges, no smooth gradient/anime/3D or pixel-filtered photograph.

## 菜单与动态

主菜单加载 `forest-waystation.png`，左侧暖米色纸质按钮、青绿边框，右侧保留驿站与小溪。运行时叠加低强度云气漂移和溪水移动波光，背景静态帧缓存复用，离开主页时销毁动画计时器。

## 原创音乐设计

64 秒循环，90 BPM，大调和声；柔和的钢琴/竖琴式分解和弦、合成短笛、稀疏钟琴回应和轻薄持续和声。以「初到异世界、林间休憩、好奇地踏上旅程」为情绪方向，不借用作品旋律或录音。程序合成音色，非实录乐器。实现见 `source/MenuMusic.cs`，输出到 `assets/rogue/audio/menu-orbit.wav`，继续遵循游戏音乐音量设置。
