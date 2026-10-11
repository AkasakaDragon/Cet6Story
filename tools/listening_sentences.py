"""English sentence boundaries shared by listening alignment and translations."""
import re

def abbreviation_continues(prefix, following):
    prefix = prefix.strip()
    following = following.lstrip()
    if not following:
        return False
    token = prefix.split()[-1] if prefix else ''
    if re.fullmatch(r'(?:Mr|Mrs|Ms|Dr|Prof|Sr|Jr|St|vs|e\.g|i\.e)\.', token, re.I):
        return True
    if re.fullmatch(r'(?:[A-Za-z]\.){2,}', token):
        # Acronyms can also finish a real sentence: "He lives in the U.S."
        # A lowercase continuation or a short noun-phrase fragment is unambiguous.
        return following[0].islower() or len(prefix.split()) <= 3
    if re.fullmatch(r'[A-Z]\.', token):
        return bool(re.match(r'[A-Z][a-z]', following))
    return False

def sentences(text):
    result = []
    start = 0
    for boundary in re.finditer(r'(?<=[.!?])\s+|\n+', text):
        prefix = text[start:boundary.start()].strip()
        if abbreviation_continues(prefix, text[boundary.end():]):
            continue
        if prefix:
            result.append(prefix)
        start = boundary.end()
    if text[start:].strip():
        result.append(text[start:].strip())
    return result

def merge_abbreviation_rows(rows):
    """Retain the first start and last end, including fragments from source data."""
    result = []
    for row in rows:
        if result and abbreviation_continues(result[-1]['text'], row['text']) and 0 <= row['start']-result[-1]['end'] < 2.5 and result[-1].get('speaker') == row.get('speaker'):
            result[-1]['text'] += ' ' + row['text']
            result[-1]['end'] = row['end']
            result[-1]['translation'] = ''
            result[-1].pop('translation_source', None)
        else:
            result.append(dict(row))
    return result
