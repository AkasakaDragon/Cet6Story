"""Build closing dialogue using the chapter's established voice profiles."""
import asyncio, hashlib, json, re, subprocess, sys, urllib.request, wave
from pathlib import Path
ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT / '.validation/tts-deps'))
import edge_tts
OUT = ROOT / 'chapters/audio/tavern/chapter-six'
CACHE = ROOT / '.validation/stay-voices'
CACHE.mkdir(parents=True, exist_ok=True)
source = (ROOT / 'source/ChapterOneStayEpilogue.cs').read_text(encoding='utf-8-sig')
def strings(name):
    return re.findall(r'"([^"\n]*)"', re.search(r'string\[\] '+name+r'=\{(.*?)\};', source, re.S).group(1))
roles, texts, translations = strings('speakers'), strings('en'), strings('zh')
profiles = json.loads((OUT / 'voices.json').read_text(encoding='utf-8-sig'))
async def main():
    sem = asyncio.Semaphore(3)
    async def render(text, role):
        profile = profiles[role]
        key = hashlib.sha256((text + json.dumps(profile)).encode()).hexdigest()[:20]
        mp3, wav = CACHE / (key+'.mp3'), CACHE / (key+'.wav')
        async with sem:
            if not wav.exists():
                for attempt in range(4):
                    try:
                        await edge_tts.Communicate(text, **profile, proxy=urllib.request.getproxies().get('https'), receive_timeout=30).save(str(mp3))
                        break
                    except Exception:
                        mp3.unlink(missing_ok=True)
                        if attempt == 3: raise
                        await asyncio.sleep(2)
                subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'), '-nostdin', '-loglevel', 'error', '-y', '-i', str(mp3), '-ar', '24000', '-ac', '1', '-c:a', 'pcm_s16le', str(wav)], check=True)
            with wave.open(str(wav), 'rb') as reader:
                assert reader.getparams()[:3] == (1, 2, 24000)
                return reader.readframes(reader.getnframes())
    pieces = await asyncio.gather(*(render(text, role) for text, role in zip(texts, roles)))
    frames, lines = 0, []
    with wave.open(str(OUT/'stay.wav'), 'wb') as writer:
        writer.setparams((1, 2, 24000, 0, 'NONE', 'not compressed'))
        for role, text, zh, pcm in zip(roles, texts, translations, pieces):
            assert len(pcm)>4800
            start=frames/24000
            writer.writeframes(pcm); frames+=len(pcm)//2
            lines.append(dict(speaker=role, text=text, translation=zh, start=start, end=frames/24000))
            writer.writeframes(bytes(12000)); frames+=6000
    (OUT/'stay.json').write_text(json.dumps(lines, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    (OUT/'stay-voices.json').write_text(json.dumps({role:profiles[role] for role in set(roles)}, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print('Built 8 lines with existing narrator/Brian/Michelle profiles:', round(frames/24000,2), 'seconds')
asyncio.run(main())
