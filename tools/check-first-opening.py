"""Check revised timeline, quiz placement and exact preservation of existing speech."""
import json
import subprocess
import wave
from pathlib import Path

root = Path(__file__).resolve().parents[1]
def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))
def pcm(path):
    with wave.open(str(path), 'rb') as w:
        assert w.getparams()[:3] == (1, 2, 24000)
        return w.readframes(w.getnframes())
for number, media in [(3,'chapter-three'), (4,'chapter-four'), (6,'chapter-six')]:
    name = f'{10+number}-tavern-01-{number:02}.json'
    chapter = read(root/'chapters'/name)
    audio = root/'chapters/audio/tavern'/media
    data = pcm(audio/'story.wav')
    lines = chapter['lines']
    assert len(chapter['questions']) == 4
    assert [q['afterLine'] for q in chapter['questions']] == ([5,17,23,29] if number == 6 else [5,11,17,23])
    last = 0
    for line in lines:
        assert last <= line['start'] < line['end'] <= len(data)/48000
        assert (root/'chapters'/line['scene']).exists()
        last = line['end']
    for q in chapter['questions']:
        assert q['afterLine'] < len(lines) and len(q['options']) == 4 and 0 <= q['answer'] < 4
    backup = root/'.git/audio-backups/first-opening-revision'/media
    old = read(backup/name)
    old_data = pcm(backup/'story.wav')
    old_lines = {(l['speaker'],l['text']):l for l in old['lines']}
    for line in lines:
        original = old_lines.get((line['speaker'],line['text']))
        if original:
            assert data[round(line['start']*24000)*2:round(line['end']*24000)*2] == old_data[round(original['start']*24000)*2:round(original['end']*24000)*2], 'Changed preserved voice'
        if line['speaker'] == '莉瑟':
            assert original, 'Princess dialogue should remain intact'
    specs = read(audio/'cg.json')
    for kind in ['intro','outro']:
        with wave.open(str(audio/(kind+'.wav')),'rb') as w:
            assert abs(w.getnframes()/w.getframerate()-specs[kind]['seconds']) < .002
        subprocess.run([str(root/'tools/ffmpeg/ffmpeg.exe'),'-v','error','-i',str(root/'assets/opening/tavern'/(media+'-'+kind+'.mp4')),'-f','null','-'],check=True)
    print(f'PASS section {number}: timing, 4 quizzes, exact retained speech, narration and both CG videos')
assert all('营业收了' not in l['translation'] for l in read(root/'chapters/13-tavern-01-03.json')['lines'])
assert any('第一次营业' in l['translation'] for l in read(root/'chapters/16-tavern-01-06.json')['lines'])
print('PASS first service occurs in section six')
