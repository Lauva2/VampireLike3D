"""Archive original evidence with hashes; never invent or complete missing reports."""
import argparse
import hashlib
import json
import zipfile
from datetime import datetime, timezone
from pathlib import Path


def digest(path):
    value = hashlib.sha256()
    with path.open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            value.update(block)
    return value.hexdigest().upper()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--candidate', type=int, required=True)
    parser.add_argument('--version', required=True)
    args = parser.parse_args()
    if not args.version or any(c not in 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._-' for c in args.version):
        raise SystemExit('Invalid version')
    root = Path(__file__).resolve().parent.parent
    number = args.candidate
    folders = [f'Artifacts/{prefix}{number}' for prefix in
               ('Round', 'UI', 'Probe', 'Campaign', 'Soak', 'FormalGameplay', 'ReleaseSmoke')]
    folders += [f'Docs/ArtReviews/Round{number}']
    required = [f'Artifacts/{prefix}{number}/{name}' for prefix, name in
                [('Round', 'Verification.json'), ('UI', 'Verification.json'), ('Probe', 'Verification.json'),
                 ('Campaign', 'Summary.json'), ('Soak', 'Summary.json'), ('Soak', 'Analysis.json'),
                 ('Soak', 'Acceptance.json'), ('Soak', 'ProcessExit.json'), ('Soak', 'Errors.txt'),
                 ('FormalGameplay', 'Summary.json'), ('ReleaseSmoke', 'Summary.json')]]
    required += [f'Docs/ArtReviewResult{number}.json']
    for relative in required:
        if not (root / relative).is_file():
            raise SystemExit(f'Missing finished evidence: {relative}')
    paths = set()
    for relative in folders:
        paths.update(p for p in (root / relative).rglob('*') if p.is_file())
    paths.update(root / p for p in required)
    # Keep failed performance history and the short exploration explicitly separate.
    historical = [p for p in (root / 'Artifacts').glob('Soak*') if p.is_dir()
                  and ((p / 'Interrupted.json').is_file() or (p / 'Cancelled.json').is_file()
                       or (p / 'Limitations.json').is_file())]
    historical.append(root / f'Artifacts/Soak{number}Preflight')
    for directory in historical:
        if directory.exists():
            paths.update(p for p in directory.rglob('*') if p.is_file())
    for pattern in (f'*{number}*.json', f'*{number}*.txt', f'*{number}*.log'):
        paths.update(p for p in (root / 'Artifacts').glob(pattern) if p.is_file())
    for name in ('NewMachineAudioAudit.json',):
        if (root / 'Artifacts' / name).is_file():
            paths.add(root / 'Artifacts' / name)
    for name in ('ReleaseAcceptance.md', 'SessionHandoff.md', 'ImplementationStatus.md',
                 'ArtReview.md', 'ContentScope.md', 'PlayerGuide.md'):
        paths.add(root / 'Docs' / name)
    soak_review = root / 'Docs' / f'Soak{number}Review.md'
    if soak_review.is_file():
        paths.add(soak_review)
    files = []
    for path in sorted(paths):
        if not path.resolve().is_relative_to(root) or not path.is_file():
            raise SystemExit(f'Invalid evidence path: {path}')
        files.append(dict(path=path.relative_to(root).as_posix(), bytes=path.stat().st_size, sha256=digest(path)))
    manifest = dict(version=args.version, candidate=f'Round{number}', createdUtc=datetime.now(timezone.utc).isoformat(),
                    scope='Raw Player evidence, independent art observation, identities and build records. '
                          'Previous interrupted soak and short preflight are historical, not final passes. '
                          'This archive does not change any test or art verdict.', files=files)
    destination = root / 'Builds' / 'EvidenceArchives' / f'WanXiangExpedition-Evidence-{args.version}.zip'
    destination.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(destination, 'x', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
        for entry in files:
            archive.write(root / entry['path'], entry['path'])
        archive.writestr('EvidenceArchiveManifest.json', json.dumps(manifest, ensure_ascii=False, indent=2))
    with zipfile.ZipFile(destination) as archive:
        if len(archive.namelist()) != len(files) + 1:
            raise SystemExit('Evidence archive entry count mismatch')
        for entry in files:
            if hashlib.sha256(archive.read(entry['path'])).hexdigest().upper() != entry['sha256']:
                raise SystemExit(f"Archive hash mismatch: {entry['path']}")
    sidecar = dict(archive=str(destination), sha256=digest(destination), files=len(files),
                   allEntryHashesVerified=True, bytes=destination.stat().st_size)
    destination.with_suffix('.zip.audit.json').write_text(json.dumps(sidecar, indent=2), encoding='utf-8')
    print(json.dumps(sidecar))


if __name__ == '__main__':
    main()
