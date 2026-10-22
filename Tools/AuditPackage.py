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
    note_name = '版本说明.txt' if manifest['status'] == 'VERIFIED_RELEASE' else '候选版本说明.txt'
    expected_files = {prefix + entry['path'].replace('\\', '/') for entry in manifest['files']}
    expected_files.update((manifest_name, prefix + note_name))
    actual_files = [entry.filename for entry in archive.infolist() if not entry.is_dir()]
    if len(actual_files) != len(set(actual_files)) or set(actual_files) != expected_files:
        errors.append('Unexpected, duplicate or missing archive file entries')
    for entry in manifest['files']:
        name = prefix + entry['path'].replace('\\', '/')
        if name not in names:
            errors.append('Missing listed file: ' + name)
        elif archive.getinfo(name).file_size != entry['bytes']:
            errors.append('Size mismatch: ' + name)
        elif hashlib.sha256(archive.read(name)).hexdigest().upper() != entry['sha256'].upper():
            errors.append('Hash mismatch: ' + name)
    for required in ('WanXiangExpedition.exe', 'SourceIdentity.json', '游玩说明.md',
                     'Licenses/Noto-Sans-CJK-OFL.txt'):
        if prefix + required not in names:
            errors.append('Missing required distribution file: ' + required)
    corrupt = archive.testzip()
    if corrupt:
        errors.append('Archive CRC failed: ' + corrupt)
    # The manifest cannot contain its own digest. Record both it and the version
    # note in this separate audit alongside every payload file.
    entry_hashes = [dict(path=name, bytes=archive.getinfo(name).file_size,
                         sha256=hashlib.sha256(archive.read(name)).hexdigest().upper())
                    for name in sorted(actual_files)]
result = dict(passed=not errors, version=manifest['version'], status=manifest['status'],
              listed_files_checked=len(manifest['files']), bytes=path.stat().st_size,
              archive_sha256=hashlib.sha256(path.read_bytes()).hexdigest().upper(),
              all_archive_files=entry_hashes, errors=errors,
              scope='Exact archive file set, CRC, payload sizes/hashes and recorded SHA256 of every entry, '
                    'including manifest and version note. Release status is preserved; no test verdict is inferred.')
output.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({key: value for key, value in result.items() if key != 'all_archive_files'}, ensure_ascii=False))
