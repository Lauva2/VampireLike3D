"""Verify packaged file hashes, archive integrity and absence of bundled user saves."""
import hashlib
import json
import sys
import zipfile
from pathlib import Path, PurePosixPath

path, output = map(Path, sys.argv[1:3])
errors = []
with zipfile.ZipFile(path) as archive:
    names = archive.namelist()
    for name in names:
        parts = PurePosixPath(name.replace('\\', '/')).parts
        if '..' in parts or PurePosixPath(name).is_absolute():
            errors.append('Unsafe archive path: ' + name)
        if 'Saves' in parts:
            errors.append('User save directory included: ' + name)
    manifest_names = [name for name in names if name.endswith('/BuildManifest.json')]
    if len(manifest_names) != 1:
        raise SystemExit('Expected exactly one packaged build manifest')
    manifest_name = manifest_names[0]
    prefix = manifest_name.rsplit('/', 1)[0] + '/'
    manifest = json.loads(archive.read(manifest_name).decode('utf-8-sig'))
    for entry in manifest['files']:
        name = prefix + entry['path'].replace('\\', '/')
        if name not in names:
            errors.append('Missing listed file: ' + name)
        elif hashlib.sha256(archive.read(name)).hexdigest().upper() != entry['sha256'].upper():
            errors.append('Hash mismatch: ' + name)
    for required in ('WanXiangExpedition.exe', 'SourceIdentity.json', '游玩说明.md',
                     'Licenses/Noto-Sans-CJK-OFL.txt'):
        if prefix + required not in names:
            errors.append('Missing required distribution file: ' + required)
    corrupt = archive.testzip()
    if corrupt:
        errors.append('Archive CRC failed: ' + corrupt)
result = dict(passed=not errors, version=manifest['version'], status=manifest['status'],
              listed_files_checked=len(manifest['files']), bytes=path.stat().st_size,
              errors=errors, scope='Archive integrity and listed-file hashes only; release status is preserved.')
output.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(result, ensure_ascii=False))
