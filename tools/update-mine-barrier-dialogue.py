"""Replace the mine barrier narration and its audio, preserving other recorded dialogue."""
import asyncio, json, shutil, subprocess, urllib.request, wave
from pathlib import Path
import edge_tts
ROOT = Path(__file__).resolve().parent.parent
SCRIPT = ROOT / 'chapters/15-tavern-01-05.json'
WORK = ROOT / '.validation/mine-barrier-dialogue'
WORK.mkdir(parents=True, exist_ok=True)
FF = ROOT / 'tools/ffmpeg/ffmpeg.exe'
REPLACEMENTS = {
    14: ('石台下的守卫兽撞向盾面。艾莉娅退半步站稳，陆川立马发动屏障。', 'The guardian beneath the ledge strikes the shield. Aelia steadies herself a step back as Lu Chuan immediately casts a barrier.')
}

async def main():
    chapter = json.loads(SCRIPT.read_text(encoding='utf-8-sig'))
    if all(chapter['lines'][i]['text'] == pair[1] for i, pair in REPLACEMENTS.items()):
        print('Already updated; no repeated audio splice.')
        return
    audio = ROOT / 'chapters' / chapter['audio']
    backup = ROOT / '.git/audio-backups/mine-barrier-dialogue'
    backup.mkdir(parents=True, exist_ok=True)
    for original in (SCRIPT, audio):
        if not (backup / original.name).exists():
            shutil.copy2(original, backup / original.name)
    with wave.open(str(audio)) as reader:
        params = reader.getparams()
        source = reader.readframes(reader.getnframes())
    rate, width = params.framerate, params.nchannels * params.sampwidth
    voices = json.loads((ROOT / 'chapters/audio/tavern/chapter-five/voices.json').read_text(encoding='utf-8-sig'))
    pieces = {}
    for i, (zh, en) in REPLACEMENTS.items():
        mp3, wav = WORK / f'line-{i}.mp3', WORK / f'line-{i}.wav'
        spec = voices[chapter['lines'][i]['voiceRole']]
        for attempt in range(3):
            try:
                await edge_tts.Communicate(en, **spec, proxy=urllib.request.getproxies().get('https'), receive_timeout=30).save(str(mp3))
                break
            except Exception:
                if attempt == 2:
                    raise
                await asyncio.sleep(2)
        subprocess.run([str(FF), '-hide_banner', '-loglevel', 'error', '-y', '-i', str(mp3), '-ar', str(rate), '-ac', str(params.nchannels), '-c:a', 'pcm_s16le', str(wav)], check=True)
        with wave.open(str(wav)) as reader:
            pieces[i] = reader.readframes(reader.getnframes())
        if len(pieces[i]) < rate * width:
            raise RuntimeError('Incomplete speech')
    output, cursor, shift = bytearray(), 0, 0
    old_lines = [dict(line) for line in chapter['lines']]
    for i, line in enumerate(chapter['lines']):
        old = old_lines[i]
        start, end = round(old['start'] * rate), round(old['end'] * rate)
        if i in pieces:
            output.extend(source[cursor * width:start * width])
            output.extend(pieces[i])
            length = len(pieces[i]) // width
            line['start'], line['end'] = (start + shift) / rate, (start + shift + length) / rate
            shift += length - (end - start)
            cursor = end
            line['translation'], line['text'] = REPLACEMENTS[i]
        else:
            line['start'], line['end'] = (start + shift) / rate, (end + shift) / rate
    output.extend(source[cursor * width:])
    # Verify all unaffected utterances are byte-for-byte unchanged.
    for i, line in enumerate(chapter['lines']):
        if i in pieces:
            continue
        old = old_lines[i]
        before = source[round(old['start'] * rate) * width:round(old['end'] * rate) * width]
        after = output[round(line['start'] * rate) * width:round(line['end'] * rate) * width]
        assert before == after, f'Other dialogue changed at {i}'
    staged = WORK / 'updated.wav'
    with wave.open(str(staged), 'wb') as writer:
        writer.setparams(params)
        writer.writeframes(output)
    shutil.copy2(staged, audio)
    SCRIPT.write_text(json.dumps(chapter, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    rebuild = ROOT / 'tools/chapter-one-continuation.json'
    text = rebuild.read_text(encoding='utf-8-sig')
    for i, (zh, en) in REPLACEMENTS.items():
        text = text.replace(old_lines[i]['text'], en).replace(old_lines[i]['translation'], zh)
    rebuild.write_text(text, encoding='utf-8')
    print('PASS barrier narration, bilingual text, aligned timestamps; other utterances unchanged')

asyncio.run(main())
