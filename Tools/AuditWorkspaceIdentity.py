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
expected = {entry['path'] for entry in identity['files']}
actual = {path.relative_to(root).as_posix()
          for folder in ('Assets', 'Packages', 'ProjectSettings')
          for path in (root / folder).rglob('*') if path.is_file()
          and not path.relative_to(root).as_posix().startswith((
              'Assets/AddressableAssetsData/Windows/',
              'Assets/AddressableAssetsData/LinkerBuildProcessor/'))}
unexpected = sorted(actual - expected)
result = dict(passed=not missing and not different and not unexpected, checked_files=len(identity['files']),
              actual_files=len(actual), unexpected=unexpected,
              snapshot=str(snapshot.resolve()), missing=missing, different=different)
output.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(result, ensure_ascii=False))
