"""Audition Lyse's voice using identical dialogue, without changing game assets."""
from pathlib import Path
import asyncio, json, subprocess, sys, urllib.request, wave

ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT / '.validation/tts-deps'))
import edge_tts

MORE = '--more' in sys.argv
OUT = ROOT / ('配音试听/公主莉瑟声线对比/第二组' if MORE else '配音试听/公主莉瑟声线对比')
OUT.mkdir(parents=True, exist_ok=True)
FF = ROOT / 'tools/ffmpeg/ffmpeg.exe'
VOICES = ['en-GB-MaisieNeural', 'en-US-JennyNeural', 'en-US-AvaNeural', 'en-US-EmmaNeural', 'en-GB-LibbyNeural']
if MORE:
    VOICES = ['en-US-AriaNeural', 'en-US-AnaNeural', 'en-IE-EmilyNeural', 'en-CA-ClaraNeural', 'en-AU-NatashaNeural', 'en-NZ-MollyNeural']
TEXT = "You can call me Lyse. As for the rest... today I am only here to buy lamp oil. Please keep that to yourself for now. I do not want everyone to hear only my title and ignore what I say. Thank you. Next time, I would still like this seat and a bowl of your soup."
ZH = '叫我莉瑟就好。至于其他的……今天我只是来买灯油。请先替我保密。我不想让每个人都只听见那个身份，而不听我说什么。谢谢。下次我还是想坐今天这个位置，喝你们的汤。'

async def main():
    proxy = urllib.request.getproxies().get('https')
    available = {v['ShortName'] for v in await edge_tts.list_voices(proxy=proxy)}
    missing = set(VOICES) - available
    if missing:
        raise RuntimeError('Voices unavailable: ' + ', '.join(sorted(missing)))
    sem = asyncio.Semaphore(3)
    async def render(i, voice):
        name = voice.split('-')[2].replace('Neural', '')
        number = i + (6 if MORE else 1)
        current = not MORE and i == 4
        mp3 = OUT / (f'{number:02}-{name}' + ('-当前' if current else '') + '.mp3')
        wav = mp3.with_suffix('.wav')
        async with sem:
            for attempt in range(4):
                try:
                    await edge_tts.Communicate(TEXT, voice, rate='-4%', pitch='-2Hz', proxy=proxy, receive_timeout=30).save(str(mp3))
                    break
                except Exception:
                    mp3.unlink(missing_ok=True)
                    if attempt == 3:
                        raise
                    await asyncio.sleep(2)
            subprocess.run([str(FF), '-nostdin', '-loglevel', 'error', '-y', '-i', str(mp3), '-af', 'loudnorm=I=-19:TP=-2:LRA=7', '-ar', '24000', '-ac', '1', '-c:a', 'pcm_s16le', str(wav)], check=True)
            with wave.open(str(wav), 'rb') as reader:
                data = reader.readframes(reader.getnframes())
                duration = reader.getnframes() / reader.getframerate()
            if duration < 10:
                raise RuntimeError('Incomplete voice sample: ' + voice)
            print(f'{number} {voice}: {duration:.1f}s', flush=True)
            return dict(number=number, voice=voice, file=wav.name, seconds=round(duration, 2), current=current), data
    results = await asyncio.gather(*(render(i, v) for i, v in enumerate(VOICES)))
    with wave.open(str(OUT / ('六种声线-连续对比.wav' if MORE else '五种声线-连续对比.wav')), 'wb') as writer:
        writer.setparams((1, 2, 24000, 0, 'NONE', 'not compressed'))
        for _, data in results:
            writer.writeframes(data)
            writer.writeframes(bytes(96000))
    (OUT / '声线清单.json').write_text(json.dumps(dict(text=TEXT, translation=ZH, rate='-4%', pitch='-2Hz', loudness='-19 LUFS', samples=[r[0] for r in results]), ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    description = '第二组新增六种声线，编号接续上一组。' if MORE else '四种候选加当前 Libby 对照。'
    (OUT / '试听说明.md').write_text('# 公主莉瑟声线试听\n\n' + description + '均使用相同的本游戏公主台词，语速 -4%、音调 -2Hz，响度统一为 -19 LUFS，无音乐、混响或音效。正式章节配音尚未替换。\n\n' + TEXT + '\n\n' + ZH + '\n\n' + '\n'.join(f"{r[0]['number']}. {r[0]['voice']}" for r in results) + '\n\n连续对比按照以上顺序，间隔两秒静音。\n', encoding='utf-8')

asyncio.run(main())
