"""Make smaller offline speech copies without changing the downloaded originals."""
import concurrent.futures,hashlib,json,subprocess,time
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];FOLDER=ROOT/'assets/tavern/listening/real-exams/cettong'
def run(file):
    source=json.loads(file.read_text(encoding='utf-8'));audio=ROOT/source['audio']
    if '.exampace-audio.' not in audio.name:return 0
    playback=audio.with_name(audio.name.replace('.exampace-audio.','.exampace-playback.'))
    if not playback.exists():
        temporary=playback.with_suffix('.tmp.mp3')
        subprocess.run([str(ROOT/'tools/ffmpeg/ffmpeg.exe'),'-nostdin','-hide_banner','-loglevel','error','-y','-i',str(audio),'-vn','-ar','24000','-ac','1','-c:a','libmp3lame','-b:a','48k',str(temporary)],check=True,creationflags=subprocess.CREATE_NO_WINDOW)
        temporary.replace(playback)
    # Keep the original download locally for provenance; only the speech copy is
    # needed by the game. Each exact file is checked to stay within these folders.
    archive=ROOT/'.validation/listening-source-audio'/audio.relative_to(FOLDER)
    assert FOLDER.resolve() in audio.resolve().parents
    assert (ROOT/'.validation').resolve() in archive.resolve().parents
    cached=ROOT/'.validation/exampace'/(source['material_paper_id']+'.json')
    origin=json.loads(cached.read_text(encoding='utf-8'))['paper']['audio'] if cached.exists() else {}
    source['source_audio']=dict(url='https://117.72.200.49'+origin.get('url','/#/library'),original_bytes=audio.stat().st_size,sha256=hashlib.sha256(audio.read_bytes()).hexdigest(),playback_encoding='MP3 48 kbps mono 24 kHz; unchanged content and timing')
    source['audio']=playback.relative_to(ROOT).as_posix()
    captions=file.with_name(file.name.replace('.resources.json','.captions.json'))
    if captions.exists():
        doc=json.loads(captions.read_text(encoding='utf-8'));doc['audio']=source['audio'];captions.write_text(json.dumps(doc,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    file.write_text(json.dumps(source,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    archive.parent.mkdir(parents=True,exist_ok=True);audio.replace(archive)
    print(source['id']+': offline playback '+str(playback.stat().st_size)+' bytes',flush=True)
    return playback.stat().st_size
if __name__=='__main__':
    with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:list(pool.map(run,FOLDER.rglob('*.resources.json')))
