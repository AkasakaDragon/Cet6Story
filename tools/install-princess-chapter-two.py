"""Install approved, manually generated MiniMax lines without changing other roles."""
import json
import shutil
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'chapters/audio/tavern/chapter-two'
ASSETS = OUT / 'lyse-minimax'

def read_pcm(path):
    with wave.open(str(path), 'rb') as reader:
        assert reader.getparams()[:3] == (1, 2, 24000)
        return reader.readframes(reader.getnframes())

def install():
    manifest_path = ASSETS / 'manifest.json'
    if not manifest_path.exists():
        return
    manifest = json.loads(manifest_path.read_text(encoding='utf-8'))
    chapter_path = ROOT / 'chapters/12-tavern-01-02.json'
    chapter = json.loads(chapter_path.read_text(encoding='utf-8-sig'))
    princess = [line for line in chapter['lines'] if line['speaker'] == '莉瑟']
    assert [line['text'] for line in princess] == [line['text'] for line in manifest['lines']]
    original = read_pcm(OUT / 'market.wav')
    work = ROOT / '.validation/princess-chapter-two'
    work.mkdir(parents=True, exist_ok=True)
    for path in [chapter_path, OUT / 'market.wav', OUT / 'voices.json']:
        backup = work / ('original-' + path.name)
        if not backup.exists():
            shutil.copy2(path, backup)
    pieces = []
    cursor = 0
    for line in chapter['lines']:
        if line['speaker'] == '莉瑟':
            item = manifest['lines'][cursor]
            pieces.append(read_pcm(ASSETS / item['file']))
            cursor += 1
        else:
            pieces.append(original[round(line['start'] * 24000) * 2:round(line['end'] * 24000) * 2])
    temporary = work / 'market-new.wav'
    frames = 0
    with wave.open(str(temporary), 'wb') as writer:
        writer.setparams((1, 2, 24000, 0, 'NONE', 'not compressed'))
        for line, pcm in zip(chapter['lines'], pieces):
            assert len(pcm) > 4800
            line['start'] = round(frames / 24000, 4)
            writer.writeframes(pcm)
            frames += len(pcm) // 2
            line['end'] = round(frames / 24000, 4)
            writer.writeframes(bytes(12000))
            frames += 6000
    chapter['audioLabel'] = '离线英文配音 · 莉瑟：MiniMax 温暖少女 · 其他角色：固定角色配音'
    voices = json.loads((OUT / 'voices.json').read_text(encoding='utf-8-sig'))
    voices['莉瑟'] = {'provider': 'MiniMax', 'voice': manifest['voice_id'], 'language': 'English',
                    'source': 'lyse-minimax/manifest.json', 'generation': '用户提供的网页生成录音'}
    shutil.copy2(temporary, OUT / 'market.wav')
    chapter_path.write_text(json.dumps(chapter, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    (OUT / 'voices.json').write_text(json.dumps(voices, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(f'Installed {cursor} princess lines; chapter duration {frames / 24000:.2f}s')

if __name__ == '__main__':
    install()
