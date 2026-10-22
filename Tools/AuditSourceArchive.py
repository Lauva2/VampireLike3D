"""Verify every editable source archive entry against its recorded digest."""
import hashlib
import json
import sys
import zipfile
from pathlib import Path

path = Path(sys.argv[1])
with zipfile.ZipFile(path) as archive:
    manifest = json.loads(archive.read('SourceArchiveManifest.json').decode('utf-8-sig'))
    entries = manifest['files']
    expected = {entry['path'] for entry in entries} | {'SourceArchiveManifest.json'}
    if len(expected) != len(entries) + 1 or len(archive.namelist()) != len(expected) or set(archive.namelist()) != expected:
        raise SystemExit('Unexpected, duplicate or missing source archive entries')
    for entry in entries:
        content = archive.read(entry['path'])
        if len(content) != entry['bytes'] or hashlib.sha256(content).hexdigest().upper() != entry['sha256']:
            raise SystemExit(f"Source archive hash mismatch: {entry['path']}")
audit = dict(archive=str(path.resolve()), files=len(entries), allEntryHashesVerified=True,
             sha256=hashlib.sha256(path.read_bytes()).hexdigest().upper())
path.with_suffix('.zip.audit.json').write_text(json.dumps(audit, indent=2), encoding='utf-8')
print(json.dumps(audit))
