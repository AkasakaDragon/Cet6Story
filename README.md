# 六级物语 · Cet6Story

Windows 桌面英语学习游戏，结合剧情听力、词汇练习和卡牌战斗。

## 运行

在 Windows 上启动 `Cet6Story.exe`。请保留同目录的 `assets`、`chapters` 和 `tools`。个人进度保存在本地 `save.json`，不上传到仓库。

## 编译

在项目根目录运行：

```powershell
powershell -ExecutionPolicy Bypass -File tools/build-game.ps1
```

构建脚本使用 Windows .NET Framework C# 编译器、Windows Forms、System.Drawing 与 System.Speech，输出根目录的 `Cet6Story.exe`。

## 打包

运行 `一键打包.bat` 或 `一键打包.ps1`，生成的压缩包位于 `打包输出`。

## 目录

- `source`：游戏源码。
- `assets`、`chapters`：游戏运行资源、词库、剧情及音频。
- `tools`：编译脚本及 FFmpeg（其许可证位于 `tools/ffmpeg/LICENSE`）。
- `项目文档`：使用说明和玩法文档。
- `导入模板`：内容导入模板。
- `角色概念稿`、`剧情概念稿`、`配音试听`、`预览`：制作素材与预览。

本地存档、缓存、验证临时目录和生成的输出文件由 `.gitignore` 排除。
