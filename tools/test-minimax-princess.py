"""Generate one princess audition using the locally configured MiniMax key."""
import json
from pathlib import Path
import urllib.request
import urllib.error

ROOT = Path(__file__).resolve().parents[1]
config = {}
for line in (ROOT / '.env').read_text(encoding='utf-8-sig').splitlines():
    if '=' in line and not line.lstrip().startswith('#'):
        name, value = line.split('=', 1)
        config[name.strip()] = value.strip().strip('\"\'')
key = config.get('MINIMAX_API_KEY', '')
base = config.get('MINIMAX_BASE_URL', 'https://api.minimax.cn').rstrip('/')
if not key or base != 'https://api.minimax.cn':
    raise SystemExit('Missing API key or unexpected API host.')
out = ROOT / '配音试听/公主莉瑟声线对比/MiniMax'
out.mkdir(parents=True, exist_ok=True)
target = out / '02-Warm-Girl-公主试听.mp3'
if target.exists():
    raise SystemExit('Sample already exists; skipped to avoid duplicate billing.')
text = 'I nearly bought it. Thank you. The instructions are more useful than the wrapping.'
payload = {
    'model': 'speech-2.8-hd', 'text': text, 'stream': False,
    'voice_setting': {'voice_id': 'Chinese (Mandarin)_Warm_Girl', 'speed': 1, 'vol': 1, 'pitch': 0},
    'audio_setting': {'sample_rate': 32000, 'bitrate': 128000, 'format': 'mp3', 'channel': 1},
    'language_boost': 'English', 'output_format': 'hex',
}
request = urllib.request.Request(base + '/v1/t2a_v2', data=json.dumps(payload).encode(),
    headers={'Authorization': 'Bearer ' + key, 'Content-Type': 'application/json'})
try:
    with urllib.request.urlopen(request, timeout=120) as response:
        result = json.load(response)
except urllib.error.HTTPError as error:
    raise SystemExit(f'API HTTP error {error.code}; no automatic retry.')
except Exception as error:
    raise SystemExit(f'API request failed ({type(error).__name__}); no automatic retry.')
status = result.get('base_resp', {})
if status.get('status_code') != 0:
    message = str(status.get('status_msg', '')).replace(key, '[redacted]')
    raise SystemExit(f"API error {status.get('status_code')}: {message[:300]}")
audio = result.get('data', {}).get('audio')
if not audio:
    raise SystemExit('API returned no audio; no automatic retry.')
target.write_bytes(bytes.fromhex(audio))
metadata = {'text': text, 'model': payload['model'], 'voice_setting': payload['voice_setting'],
            'extra_info': result.get('extra_info', {}), 'file': target.name}
target.with_suffix('.json').write_text(json.dumps(metadata, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(metadata, ensure_ascii=False))
