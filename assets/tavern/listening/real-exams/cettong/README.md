# CET通四六级真题听力音频

下载日期：2026-10-09。来源：[四级真题](https://www.cettong.cn/library/cet4)、[六级真题](https://www.cettong.cn/library/cet6)。

本目录保存该网站当前公开列出的全部听力 MP3：四级 29 份、六级 29 份，共 58 份，覆盖 2019—2025 年。列表中的另外 35 套试卷页面未提供单独听力文件，已在清单中记录为 `no_audio_listed`，没有人为补造或重复命名为新音频。

目录按级别、年份整理，例如：

- `cet4/2025/2025_12_1.mp3`：2025 年 12 月四级第 1 套。
- `cet6/2025/2025_12_2.mp3`：2025 年 12 月六级第 2 套。

`manifest.json` 记录全部 93 套试卷的页面、下载链接、文件大小和 SHA-256 校验值；成功解码后的文件还标记 `verified_full_decode`。原始 MP3 保留原样，没有转码、裁切或混入背景音乐。

这些音频已接入酒馆营业的「咒语练习」。真题按级别与年份分页，使用主线对话界面的逐句播放、中英字幕开关、点击单词和连续播放。练习进度与主线评分分开保存。

从游戏根目录运行以下命令可更新下载或检查完整性（Python 3.11+，需要 requests、beautifulsoup4；验证使用仓库中的 FFmpeg）：

```powershell
python tools/download-cettong-listening.py
python tools/verify-cettong-listening.py
```

来源网站标注资料仅供个人学习使用；本目录保留来源记录，不将下载视为取得公开再分发或商业使用授权。

## 原文、题目和逐句字幕

经用户提供并允许复用，公开题库 [ExamPace](https://117.72.200.49/#/library) 提供对应试卷的听力原文、分组、25 道题目、标准答案及中文解析。每套 `.resources.json` 保留来源页面、试卷 ID、分组时间和原始材料；只读取公开发布资源，不操作远程账户或数据库。

如果题库时间轴对应的音频与 CET通下载不同，读取对应的公开录音并生成 `.exampace-playback.mp3` 离线播放副本（48 kbps、单声道、24 kHz），缩小安装体积。额外源录音存入本地忽略的 `.validation/listening-source-audio`，大小、SHA-256、公开下载地址记录在 `.resources.json` 的 `source_audio`；游戏和 GitHub 只需播放副本。58 份原始 CET通文件仍保留原样。不要混用不同录音版本的时间轴。

`.captions.json` 使用网站已有逐句时间，或将网站原文与本地 faster-whisper 单词时间对齐。材料正文优先使用公开原文；录音说明及口播问题可能来自辅助识别。中文优先复用懒笔记公开中英原文中能逐句精确对应的译文，来源保存在 `.translations.json` 及句子 `translation_source`。其余句子由本地 OPUS-MT 辅助翻译，未逐句人工校对，不是官方译文。题目答案和中文解析来自题库，不由识别或翻译模型生成。自动对齐时间和辅助译文仍可能有误，后续可直接修订 JSON，不需重新生成配音。

处理脚本：`tools/import-exampace-listening.py`、`tools/import-listening-translations.py`、`tools/build-real-exam-captions.py`。识别和翻译均在本地运行，不调用 MiniMax 或其他收费 API。模型和运行依赖位于忽略的 `.validation`，游戏运行只需要音频、JSON 和现有 FFmpeg；首次打开某套音频会缓存成 WAV，以后复用。

运行 `python tools/verify-real-exam-materials.py --decode` 检查题数、答案索引、分组、字幕顺序及资源路径，并完整解码所有播放文件、检查字幕没有超出录音时长。`tools/prepare-listening-playback.py` 生成额外源录音的离线副本。`tools/check-spell-practice.ps1` 使用独立测试存档检查导航、字幕、播放、答题奖励、弹窗与厨房词库。
