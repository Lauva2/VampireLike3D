"""Compare release/test source snapshots and shipped AA content, without running either."""
import hashlib
import json
import sys
from pathlib import Path

development, release, output = map(Path, sys.argv[1:4])

def source(folder):
    identity = json.loads((folder / 'SourceIdentity.json').read_text(encoding='utf-8-sig'))
    return identity, {entry['path']: entry['sha256'] for entry in identity['files']}

def bundles(folder):
    files = {}
    for path in (folder / 'WanXiangExpedition_Data' / 'StreamingAssets').rglob('*.bundle'):
        files[path.name] = hashlib.sha256(path.read_bytes()).hexdigest()
    return files

dev_identity, dev_source = source(development)
release_identity, release_source = source(release)
dev_bundles, release_bundles = bundles(development), bundles(release)
source_differences = [path for path in sorted(dev_source.keys() | release_source.keys())
                      if dev_source.get(path) != release_source.get(path)]
bundle_differences = [path for path in sorted(dev_bundles.keys() | release_bundles.keys())
                      if dev_bundles.get(path) != release_bundles.get(path)]
result = dict(development=str(development.resolve()), release=str(release.resolve()),
              development_flag=dev_identity['development'], release_flag=release_identity['development'],
              source_files=len(dev_source), source_differences=source_differences,
              development_bundles=len(dev_bundles), release_bundles=len(release_bundles),
              bundle_differences=bundle_differences,
              exact_source_match=not source_differences,
              exact_bundle_match=bool(dev_bundles) and not bundle_differences,
              scope='Static identity only; gameplay, art and performance need their separate reports.')
output.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(result, ensure_ascii=False))
