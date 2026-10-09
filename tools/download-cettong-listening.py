"""Download publicly linked CET listening MP3s, with a provenance manifest.
Run from any directory. Existing verified files are reused; no game/save changes.
"""
import concurrent.futures, hashlib, json, re, threading, time
from datetime import datetime, timezone
from pathlib import Path
from urllib.parse import urljoin
import requests
from bs4 import BeautifulSoup

ROOT = Path(__file__).resolve().parents[1]
DEST = ROOT / 'assets/tavern/listening/real-exams/cettong'
BASE = 'https://www.cettong.cn'
LOCAL = threading.local()

def session():
    if not hasattr(LOCAL, 'session'):
        LOCAL.session = requests.Session()
        LOCAL.session.headers['User-Agent'] = 'Cet6Story-ListeningArchive/1.0'
    return LOCAL.session

def get(url, **kwargs):
    for attempt in range(3):
        try:
            response = session().get(url, timeout=(20, 120), **kwargs)
            response.raise_for_status()
            return response
        except requests.RequestException:
            if attempt == 2:
                raise
            time.sleep(2 * (attempt + 1))

def discover(item):
    level, path = item
    url = urljoin(BASE, path)
    try:
        response = get(url)
        soup = BeautifulSoup(response.content, 'html.parser')
        audio = sorted(set(urljoin(url, node.get('src') or node.get('href'))
                           for node in soup.select('source[src], audio[src], a[href]')
                           if (node.get('src') or node.get('href') or '').split('?')[0].endswith('.mp3')))
        title = soup.find('h1')
        return dict(level=level, exam_id=path.rsplit('/', 1)[-1],
                    title=title.get_text(' ', strip=True) if title else '',
                    page_url=url, audio_urls=audio,
                    status='pending' if audio else 'no_audio_listed')
    except Exception as error:
        return dict(level=level, exam_id=path.rsplit('/', 1)[-1], page_url=url,
                    status='discovery_failed', error=str(error))

def download(entry):
    if entry['status'] != 'pending':
        return entry
    files = []
    try:
        for index, url in enumerate(entry['audio_urls']):
            year = entry['exam_id'].split('_')[0]
            name = entry['exam_id'] + ('' if index == 0 else '-' + str(index + 1)) + '.mp3'
            target = DEST / entry['level'] / year / name
            target.parent.mkdir(parents=True, exist_ok=True)
            if not target.exists():
                temporary = target.with_suffix('.mp3.part')
                digest = hashlib.sha256()
                with get(url, stream=True) as response, temporary.open('wb') as output:
                    if 'text/html' in response.headers.get('Content-Type', ''):
                        raise ValueError('Audio URL returned HTML')
                    total = 0
                    for chunk in response.iter_content(128 * 1024):
                        if chunk:
                            output.write(chunk)
                            digest.update(chunk)
                            total += len(chunk)
                    expected = response.headers.get('Content-Length')
                    if expected and not response.headers.get('Content-Encoding') and total != int(expected):
                        raise ValueError('Incomplete audio download')
                if temporary.stat().st_size < 1024:
                    raise ValueError('Audio file unexpectedly small')
                temporary.replace(target)
            with target.open('rb') as stream:
                checksum = hashlib.file_digest(stream, 'sha256').hexdigest()
            files.append(dict(path=target.relative_to(ROOT).as_posix(), source_url=url,
                              bytes=target.stat().st_size, sha256=checksum))
        entry.update(status='downloaded', files=files)
    except Exception as error:
        entry.update(status='download_failed', files=files, error=str(error))
    return entry

def save(entries):
    DEST.mkdir(parents=True, exist_ok=True)
    manifest = dict(source=BASE, fetched_at=datetime.now(timezone.utc).isoformat(),
                    scope='All publicly listed CET4 and CET6 listening MP3s; papers without audio recorded separately.',
                    entries=sorted(entries, key=lambda e: (e['level'], e['exam_id'])))
    (DEST / 'manifest.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

def main():
    items = []
    for level in ('cet4', 'cet6'):
        response = get(BASE + '/library/' + level)
        soup = BeautifulSoup(response.content, 'html.parser')
        paths = sorted(set(a['href'] for a in soup.select('a[href]')
                           if re.fullmatch('/library/' + level + r'/[^/?#]+', a['href'])))
        items.extend((level, path) for path in paths)
        print(level + ': ' + str(len(paths)) + ' exam pages', flush=True)
    entries = []
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        for entry in pool.map(discover, items):
            entries.append(entry)
            if len(entries) % 15 == 0:
                print('Discovered ' + str(len(entries)) + '/' + str(len(items)), flush=True)
    save(entries)
    print('Audio-bearing pages: ' + str(sum(e['status'] == 'pending' for e in entries)), flush=True)
    completed = []
    with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
        for entry in pool.map(download, entries):
            completed.append(entry)
            save(completed + entries[len(completed):])
            if entry['status'] in ('downloaded', 'download_failed', 'discovery_failed'):
                print(entry['level'] + '/' + entry['exam_id'] + ': ' + entry['status'], flush=True)
    summary = {state: sum(e['status'] == state for e in completed)
               for state in sorted(set(e['status'] for e in completed))}
    print(json.dumps(summary), flush=True)
    if any(e['status'].endswith('failed') for e in completed):
        raise SystemExit(1)

if __name__ == '__main__':
    main()
