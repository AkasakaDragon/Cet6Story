"""Add the established CG narrator to the sanctuary arrival without changing earlier clips."""
import asyncio, json, subprocess, urllib.request, wave
from pathlib import Path
import edge_tts

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / 'assets/opening/tavern'
WORK = ROOT / '.validation/sanctuary-narration'
WORK.mkdir(parents=True, exist_ok=True)
FF = ROOT / 'tools/ffmpeg/ffmpeg.exe'
EN = 'The white light fades. Lu Chuan stands in an ancient sanctuary above the clouds, where a goddess awaits beside a shattered golden crystal.'
ZH = '白光散去，陆川站在云海间的古老神殿中。破碎的金色晶核悬浮在石台上，一位女神正等待着他。'
VOICE = json.loads((ROOT / 'chapters/audio/tavern/voices.json').read_text(encoding='utf-8-sig'))['旁白']

def run(*args):
    subprocess.run([str(FF), '-hide_banner', '-loglevel', 'error', '-y', *map(str, args)], check=True)

async def main():
    mp3 = WORK / 'arrival-narrator.mp3'
    if not mp3.exists():
        for attempt in range(3):
            try:
                await edge_tts.Communicate(EN, **VOICE, proxy=urllib.request.getproxies().get('https'), receive_timeout=30).save(str(mp3))
                break
            except Exception:
                mp3.unlink(missing_ok=True)
                if attempt == 2:
                    raise
                await asyncio.sleep(2)
    speech = OUT / 'sanctuary-narration.wav'
    run('-i', mp3, '-af', 'loudnorm=I=-19:TP=-2:LRA=7', '-ar', '44100', '-ac', '2', '-c:a', 'pcm_s16le', speech)
    with wave.open(str(speech)) as reader:
        speech_seconds = reader.getnframes() / reader.getframerate()
    if speech_seconds < 3:
        raise RuntimeError('Narration incomplete')
    onset = 14.65
    duration = round(max(18, onset + speech_seconds + 1.0), 4)
    mixed = OUT / 'opening-narrated.wav'
    filters = f'[0:a]apad,atrim=0:{duration}[bed];[1:a]adelay=14650|14650[voice];[bed][voice]amix=inputs=2:normalize=0,alimiter=limit=0.95,atrim=0:{duration}[out]'
    run('-i', OUT / 'opening.wav', '-i', speech, '-filter_complex', filters, '-map', '[out]', '-ar', '44100', '-ac', '2', '-c:a', 'pcm_s16le', mixed)
    run('-i', OUT / 'opening.mp4', '-i', mixed, '-map', '0:v:0', '-map', '1:a:0', '-vf', f'tpad=stop_mode=clone:stop_duration={duration - 18}', '-t', duration,
        '-c:v', 'libx264', '-crf', '18', '-pix_fmt', 'yuv420p', '-c:a', 'aac', '-b:a', '160k', '-movflags', '+faststart', OUT / 'opening-narrated.mp4')
    (OUT / 'opening-narrated.json').write_text(json.dumps(dict(seconds=duration, zh=ZH, en=EN, voice=VOICE, narrationStart=onset, narrationSeconds=speech_seconds), ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(f'Built narrated arrival: {VOICE["voice"]}, speech {speech_seconds:.2f}s, CG {duration:.2f}s')

asyncio.run(main())
