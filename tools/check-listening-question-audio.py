"""Find answer pauses in the original recordings so replays retain whole questions."""
import concurrent.futures
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
FOLDER = ROOT/'assets/tavern/listening/real-exams/cettong'
CACHE = ROOT/'.validation/listening-audio-silences'
CACHE.mkdir(parents=True,exist_ok=True)


def check(file):
    exam = json.loads(file.read_text(encoding='utf-8'))
    audio = ROOT/exam['audio']
    out = CACHE/(exam['id']+'.json')
    identity = [audio.stat().st_size,audio.stat().st_mtime_ns]
    if out.exists() and json.loads(out.read_text(encoding='utf-8'))['audio_identity'] == identity:
        return
    result = subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'),'-nostdin','-hide_banner','-i',str(audio),
                             '-af','silencedetect=noise=-35dB:d=2.5','-f','null','-'],capture_output=True,text=True,check=True,
                            creationflags=subprocess.CREATE_NO_WINDOW)
    events = [(kind,float(value)) for kind,value in re.findall(r'silence_(start|end): ([0-9.]+)',result.stderr)]
    pauses = []
    beginning = None
    for kind,value in events:
        if kind == 'start':beginning=value
        elif beginning is not None:
            pauses.append([beginning,value]);beginning=None
    if beginning is not None:pauses.append([beginning,beginning+2.5])
    out.write_text(json.dumps(dict(audio=exam['audio'],audio_identity=identity,pauses=pauses),indent=2)+'\n',encoding='utf-8')


if __name__ == '__main__':
    files = list(FOLDER.rglob('*.resources.json'))
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:list(pool.map(check,files))
    print('PASS: original audio answer pauses checked for',len(files),'recordings.')
