"""Build sections 3-6 with cached fixed-role speech and existing game art."""
import asyncio
import hashlib
import json
import subprocess
import sys
import shutil
import urllib.request
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT / '.validation/tts-deps'))
import edge_tts

FF = ROOT / 'tools/ffmpeg/ffmpeg.exe'
WORK = ROOT / '.validation/chapter-continuation-voices'
WORK.mkdir(parents=True, exist_ok=True)
VOICES = json.loads((ROOT / 'chapters/audio/tavern/voices.json').read_text(encoding='utf-8-sig'))
VOICES.update({
    '莉瑟': dict(voice='en-GB-LibbyNeural', rate='-4%', pitch='-2Hz'),
    '送货人': dict(voice='en-US-EricNeural', rate='+0%', pitch='+0Hz'),
    '守卫': dict(voice='en-US-EricNeural', rate='-3%', pitch='-3Hz'),
    '侍从': dict(voice='en-GB-RyanNeural', rate='-3%', pitch='+0Hz'),
})
ROLES = {'陆川': 'luchuan', '艾莉娅': 'aelia', '莉瑟': 'lyse'}
MEDIA = ['chapter-three', 'chapter-four', 'chapter-five', 'chapter-six']

async def main():
    sem = asyncio.Semaphore(3)
    proxy = urllib.request.getproxies().get('https')
    async def render(text, role):
        spec = VOICES[role]
        digest = hashlib.sha256((text + json.dumps(spec)).encode()).hexdigest()[:20]
        mp3 = WORK / (digest + '.mp3')
        wav = mp3.with_suffix('.wav')
        async with sem:
            if not wav.exists():
                if not mp3.exists():
                    for attempt in range(4):
                        try:
                            await edge_tts.Communicate(text, **spec, proxy=proxy, receive_timeout=30).save(str(mp3))
                            break
                        except Exception:
                            mp3.unlink(missing_ok=True)
                            if attempt == 3:
                                raise
                            await asyncio.sleep(2)
                subprocess.run([str(FF), '-nostdin', '-loglevel', 'error', '-y', '-i', str(mp3), '-ar', '24000', '-ac', '1', '-c:a', 'pcm_s16le', str(wav)], check=True)
            with wave.open(str(wav), 'rb') as reader:
                data = reader.readframes(reader.getnframes())
            if len(data) < 4800:
                raise RuntimeError('Empty speech: ' + text)
            return data

    sections = json.loads((ROOT / 'tools/chapter-one-continuation.json').read_text(encoding='utf-8'))
    for number, (section, media) in enumerate(zip(sections, MEDIA), 3):
        if '--opening-revision' in sys.argv and number == 5:
            continue
        out = ROOT / 'chapters/audio/tavern' / media
        out.mkdir(parents=True, exist_ok=True)
        chapter_path = ROOT / f'chapters/{10+number}-tavern-01-{number:02}.json'
        preserved = {}
        old_voices = {}
        backup = ROOT / '.git/audio-backups/first-opening-revision' / media
        if chapter_path.exists() and (out / 'story.wav').exists():
            original_chapter = backup / chapter_path.name if (backup / chapter_path.name).exists() else chapter_path
            original_audio = backup / 'story.wav' if (backup / 'story.wav').exists() else out / 'story.wav'
            old = json.loads(original_chapter.read_text(encoding='utf-8-sig'))
            with wave.open(str(original_audio), 'rb') as reader:
                assert reader.getparams()[:3] == (1, 2, 24000)
                pcm = reader.readframes(reader.getnframes())
            for line in old['lines']:
                preserved[(line['speaker'], line['text'])] = pcm[round(line['start']*24000)*2:round(line['end']*24000)*2]
            if (out / 'voices.json').exists():
                old_voices = json.loads((out / 'voices.json').read_text(encoding='utf-8-sig'))
            backup.mkdir(parents=True, exist_ok=True)
            for original in [chapter_path, out / 'story.wav', out / 'cg.json', out / 'intro.wav', out / 'outro.wav', out / 'voices.json']:
                if original.exists() and not (backup / original.name).exists():
                    shutil.copy2(original, backup / original.name)
        lines = [dict(speaker=speaker, voiceRole=speaker, actor=ROLES.get(speaker, ''), text=en, translation=zh,
                      scene='art/tavern/' + section['scene'], sceneSingle=True, start=0, end=1)
                 for speaker, zh, en in section['dialogue']]
        for i, line in enumerate(lines):
            line['timeOfDay'] = section.get('timeOfDay')
            line.update(section.get('lineOverrides', {}).get(str(i), {}))
            if number == 4 and i >= section.get('nightFromLine', 999):
                line['timeOfDay'] = 'night'
                line['scene'] = 'art/tavern/' + ('chapter-one-road-night.png' if i == 23 else 'chapter-one-night.png')
        async def line_audio(line):
            key = (line['speaker'], line['text'])
            return preserved[key] if key in preserved else await render(line['text'], line['speaker'])
        pieces = await asyncio.gather(*(line_audio(line) for line in lines))
        frames = 0
        with wave.open(str(out / 'story.wav'), 'wb') as writer:
            writer.setparams((1, 2, 24000, 0, 'NONE', 'not compressed'))
            for line, data in zip(lines, pieces):
                line['start'] = round(frames / 24000, 6)
                writer.writeframes(data)
                frames += len(data) // 2
                line['end'] = round(frames / 24000, 6)
                writer.writeframes(bytes(12000))
                frames += 6000
        cg = {}
        for kind in ['intro', 'outro']:
            zh, en = section[kind]
            data = await render(en, '旁白')
            with wave.open(str(out / (kind + '.wav')), 'wb') as writer:
                writer.setparams((1, 2, 24000, 0, 'NONE', 'not compressed'))
                writer.writeframes(bytes(12000) + data + bytes(48000))
            seconds = len(data) / 48000 + 1.25
            cg[kind] = dict(zh=zh, en=en, seconds=round(seconds, 4), timeOfDay=section.get(kind+'TimeOfDay', section.get('timeOfDay')))
            video = ROOT / 'assets/opening/tavern' / (media + '-' + kind + '.mp4')
            subprocess.run([str(FF), '-nostdin', '-loglevel', 'error', '-y', '-loop', '1', '-framerate', '24',
                            '-i', str(ROOT / 'chapters/art/tavern' / section.get(kind + 'Scene', section['scene'])), '-t', str(seconds),
                            '-vf', 'scale=1280:720:force_original_aspect_ratio=increase:flags=neighbor,crop=1280:720,setsar=1',
                            '-an', '-c:v', 'libx264', '-preset', 'fast', '-crf', '18', '-pix_fmt', 'yuv420p', '-movflags', '+faststart', str(video)], check=True)
        questions = [dict(afterLine=at, skill=skill, prompt=prompt, options=options, answer=answer, explanation=explanation)
                     for at, skill, prompt, options, answer, explanation in section['questions']]
        chapter = dict(id=f'tavern-01-{number:02}', title=f'第一章：今天开始营业 · 第{number}节：' + section['name'],
                       timeOfDay=section.get('timeOfDay'), description=section['ending'], source='原创奇幻剧情与四道分段听力题，非考试原文。',
                       audio=f'audio/tavern/{media}/story.wav', audioLabel='离线固定角色英文配音',
                       background=lines[0]['scene'], unlockLevel=1, sortOrder=1000 + number * 10,
                       pixelArt=True, inlineQuestions=True, timeLimitSeconds=1200,
                       actors=[dict(id=ROLES[name], gender='male' if name == '陆川' else 'female',
                                    side='left' if name == '陆川' else 'right', image=f'art/tavern/{ROLES[name]}-portrait.png')
                               for name in section['actors']], decisions=[], lines=lines, questions=questions, ending=section['ending'])
        (ROOT / f'chapters/{10+number}-tavern-01-{number:02}.json').write_text(json.dumps(chapter, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        (out / 'cg.json').write_text(json.dumps(cg, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        voice_metadata = dict(VOICES)
        voice_metadata.update(old_voices)
        (out / 'voices.json').write_text(json.dumps(voice_metadata, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        print(f'Published section {number}: {len(lines)} lines, {len(questions)} questions, {frames/24000:.1f}s', flush=True)

asyncio.run(main())
