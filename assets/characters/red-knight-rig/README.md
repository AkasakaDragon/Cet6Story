# 女骑士骨骼动画原型

已按用户提供的朝右八帧图测量并拆层，未使用示意图上的关节坐标。
本目录包含可修改的骨架、部件 PNG、原始八帧和三种动画轨道。

## 打开和操作

双击项目根目录的 `打开女骑士骨骼预览.bat`。首次启动自动编译独立预览工具。
也可打开 `output/knight-rig/KnightRigPreview.exe`；请保留它在项目目录内。
该工具独立运行，不加载或写入玩家存档。

- 顶部选择待机呼吸、双手挥剑、受击后仰。攻击/受击播放完自动回到待机。
- 可开关骨架、风动、原八帧叠加，以及双臂 IK。
- 选择身体、头发或披风，再用前一个/后一个按钮选择关节。
- 点击角度 ±5° 会暂停，修改该骨骼的额外角度。子骨骼随父骨骼运动。
- 逐步播放每次推进 0.12 秒；暂停后时间和风动也暂停。
- 保存调节写入 `output/knight-rig/pose-offsets.json`，下次打开自动读取。
- 重置姿势清除本次角度调节；选动作也会清除调节，便于查看标准动作。
- 空格暂停/继续，ESC 关闭。

## 骨架与图层

`rig.json` 使用逻辑像素坐标 200 × 272，角度单位为度。
38 个骨骼包括 18 个身体/武器节点、12 个头发节点、8 个披风节点。
头发为上、中、下三条链，每条四段；披风为两条链，每条四段。
身体包含根、骨盆、胸、颈、头、双侧上臂/前臂/手、双侧大腿/小腿/脚和剑。

18 个原图拆层 PNG 加 6 个遮挡补绘层，共 24 层。
部件都保留统一的全画布坐标，透明区域允许重叠；运行时按 z 排序。
头发与披风用三节点权重的三角网格蒙皮，发梢相位滞后于根部。
原八帧经过剑/腿区域对齐，只有头发和披风图层切帧；身体保持使用同一组部件。
原图的近透明内部像素转成二值 alpha，保持清晰像素边缘并避免半透明接缝。

剑是共用目标。双臂两段 IK 计算肘部和腕部，手部角度跟随剑。
目标先限制在两条手臂共同可达的范围内，避免拉长手臂或手离开剑柄。
额外手动角度在 IK 之后应用；自由调臂时可关闭双手贴合剑柄。

## 接入现有桌面游戏

`source/PixelSkeletalRig.cs` 与现有 System.Drawing/.NET Framework 构建兼容。
骨骼工具已经编译了全部游戏源码，但没有替换战斗角色或启用首页动画。

```csharp
var rig = new PixelSkeletalRig(Path.Combine(root, "assets", "characters", "red-knight-rig"));
rig.Animation = "attack"; // idle / attack / hit
rig.AnimationTime = elapsedSeconds; // 动作局部时间；攻击和受击结束由调用方切回 idle
rig.WindTime = totalElapsedSeconds; // 连续风动时间，切换动作时不要归零
using (var pixels = rig.Render(400, 340, 55, 36)) {
    graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
    graphics.DrawImage(pixels, targetRectangle);
}
```

修改 `rig.json` 的骨骼 x/y、parent、动画 keys 即可调关节和动作。
所有父骨骼必须先于子骨骼定义。动作键之间使用平滑插值。
工作台导出的角度字典可通过 `rig.AngleOffsets[name] = degrees` 载入。
当前为项目自有格式，并非 Spine 或 DragonBones 工程文件。

## 检查与导出

```powershell
./tools/preview-knight-rig.ps1 -Check
./tools/preview-knight-rig.ps1 -Export
```

检查内容：绑定姿势逐像素还原、三种动作各 61 个采样点的双手握柄约束、
有效骨骼变换、发梢运动与脚部稳定。导出包含三种动作各 24 张 PNG 和工作台截图。

重做分层使用 Pillow + numpy：

```text
python tools/prepare-knight-rig.py --source assets/characters/red-knight-rig/source-sheet.png
python tools/repair-knight-rig-underlayers.py
```

## 素材与适用范围

`source-sheet.png` 保留用户本次附件原文件，`original-0.png` 至 `original-7.png`
保留对齐后的独立帧。`part-mask.png` 保存部件归属，可用于以后细化拆层。

`underpainting-source.png` 由内置 imagegen 编辑工具补绘，
`underpainting-aligned.png` 是与角色画布对应的版本。只提取被原剑、前臂和手
遮住的区域作为底层，原图可见部件不使用重画后的角色替换。
补绘提示词核心：移除剑与前臂/手，补全遮住的白金甲胄、腰带、裙甲和腿脚，
保留角色位置、朝向、装备、像素画法与透明背景；项目 reference-1 原图作为画风参考。

这是可运行、可继续调节的三分之四视角骨骼原型。八帧原图存在顶端裁切，
原姿势本身的遮挡和部件轮廓也限制了大幅肘部翻转。补绘解决了主要剑后缺口，
极端姿势仍可能出现接缝；大幅转身、背面视角和完整跑步需要额外方向素材及
更精细的肩/髋关节补绘，不能仅靠旋转当前像素图获得。
