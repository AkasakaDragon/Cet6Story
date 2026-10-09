"""Check archive hashes, sizes and full MP3 decoding without changing audio."""
import concurrent.futures, hashlib, json, subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / 'assets/tavern/listening/real-exams/cettong/manifest.json'
FFMPEG = ROOT / 'tools/ffmpeg/ffmpeg.exe'

def verify(file):
    path = ROOT / file['path']
    assert path.stat().st_size == file['bytes'], 'Size mismatch: ' + file['path']
    with path.open('rb') as stream:
        assert hashlib.file_digest(stream, 'sha256').hexdigest() == file['sha256'], 'Hash mismatch: ' + file['path']
    result = subprocess.run([str(FFMPEG), '-nostdin', '-hide_banner', '-v', 'error', '-xerror',
                             '-i', str(path), '-map', '0:a:0', '-f', 'null', '-'],
                            stdout=subprocess.DEVNULL, stderr=subprocess.PIPE, timeout=120)
    if result.returncode:
        raise RuntimeError(file['path'] + ': ' + result.stderr.decode(errors='replace'))
    file['verified_full_decode'] = True
    return file

def main():
    manifest = json.loads(MANIFEST.read_text(encoding='utf-8'))
    assert manifest['entries'], 'Empty archive'
    assert all(entry['status'] in ('downloaded', 'no_audio_listed') for entry in manifest['entries']), 'Download failures remain'
    files = [file for entry in manifest['entries'] for file in entry.get('files', [])]
    assert len(files) == sum(len(entry.get('audio_urls', [])) for entry in manifest['entries']), 'Incomplete audio download'
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        for count, file in enumerate(pool.map(verify, files), 1):
            if count % 10 == 0:
                print('Verified ' + str(count) + '/' + str(len(files)), flush=True)
    manifest['verification'] = 'All downloaded MP3s passed size, SHA-256 and full FFmpeg audio decoding.'
    MANIFEST.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print('PASS: ' + str(len(files)) + ' complete, decodable MP3 files.', flush=True)

if __name__ == '__main__':
    main()
