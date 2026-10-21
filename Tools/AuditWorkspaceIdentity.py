"""Check editable project files against the exact snapshot recorded at build time."""
import hashlib
import json
import sys
from pathlib import Path

root, snapshot, output = map(Path, sys.argv[1:4])
identity = json.loads(snapshot.read_text(encoding='utf-8-sig'))
missing, different = [], []
for entry in identity['files']:
    path = root / entry['path']
    if not path.is_file():
        missing.append(entry['path'])
    elif hashlib.sha256(path.read_bytes()).hexdigest().lower() != entry['sha256'].lower():
        different.append(entry['path'])
result = dict(passed=not missing and not different, checked_files=len(identity['files']),
              snapshot=str(snapshot.resolve()), missing=missing, different=different)
output.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(result, ensure_ascii=False))
