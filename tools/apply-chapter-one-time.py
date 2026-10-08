"""Apply narrative phases without rebuilding or replacing any approved speech."""
import json
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MEDIA = ['chapter-one', 'chapter-two', 'chapter-three', 'chapter-four', 'chapter-five', 'chapter-six']
PHASES = ['morning', 'morning', 'dusk', 'dusk', 'night', 'night']

def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

def write(path, data):
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')

def main():
    for n, (media, phase) in enumerate(zip(MEDIA, PHASES), 1):
        path = ROOT / f'chapters/{10+n}-tavern-01-{n:02}.json'
        chapter = read(path)
        chapter['timeOfDay'] = phase
        chapter['title'] = chapter['title'].replace('午后：', '黄昏：').replace('傍晚：', '黄昏：')
        for i, line in enumerate(chapter['lines']):
            line['timeOfDay'] = 'night' if n == 4 and i >= 18 else phase
            if n == 3:
                line['scene'] = 'art/tavern/waystation-dusk.png'
            elif n == 4:
                line['scene'] = 'art/tavern/'+('chapter-one-road-night.png' if i == 23 else 'chapter-one-night.png' if i >= 18 else 'waystation-dusk.png')
            elif n == 5:
                line['scene'] = 'art/tavern/chapter-one-mine-night.png'
        chapter['background'] = chapter['lines'][0]['scene']
        write(path, chapter)
        cg_path = ROOT / 'chapters/audio/tavern' / media / 'cg.json'
        cg = read(cg_path)
        for kind in ['intro', 'outro']:
            cg[kind]['timeOfDay'] = 'night' if n == 4 and kind == 'outro' else phase
        if n == 3:
            cg['intro']['zh'] = cg['intro']['zh'].replace('午后的', '黄昏的')
        write(cg_path, cg)
        scenes = {3: ['chapter-three-preparation-dusk.png', 'chapter-three-ready-dusk.png'],
                  4: ['waystation-dusk.png', 'chapter-one-road-night.png'],
                  5: ['chapter-one-mine-night.png', 'chapter-one-mine-night.png']}.get(n)
        if scenes:
            for kind, scene in zip(['intro', 'outro'], scenes):
                image = ROOT / 'chapters/art/tavern' / scene
                assert image.exists(), image
                output = ROOT / 'assets/opening/tavern' / (media+'-'+kind+'.mp4')
                temporary = output.with_suffix('.rendering.mp4')
                subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'), '-nostdin', '-loglevel', 'error', '-y',
                                '-loop', '1', '-framerate', '24', '-i', str(image), '-t', str(cg[kind]['seconds']),
                                '-vf', 'scale=1280:720:force_original_aspect_ratio=increase:flags=neighbor,crop=1280:720,setsar=1',
                                '-an', '-c:v', 'libx264', '-preset', 'fast', '-crf', '18', '-pix_fmt', 'yuv420p',
                                '-movflags', '+faststart', str(temporary)], check=True)
                temporary.replace(output)
        print('Applied phase:', n, phase, flush=True)

if __name__ == '__main__':
    main()
