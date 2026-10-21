"""Audit the packaged local Addressables files and compiled verification exclusion."""
import hashlib
import json
import sys
from pathlib import Path

player = Path(sys.argv[1]).resolve()
data = player / 'WanXiangExpedition_Data'
aa = data / 'StreamingAssets' / 'aa'
settings = json.loads((aa / 'settings.json').read_text(encoding='utf-8-sig'))
catalog = json.loads((aa / 'catalog.json').read_text(encoding='utf-8-sig'))
errors = []
locations = settings.get('m_CatalogLocations', [])
if len(locations) != 1 or not locations[0]['m_InternalId'].startswith('{UnityEngine.AddressableAssets.Addressables.RuntimePath}'):
    errors.append('The startup catalog is not the bundled local catalog')
bundle_files = []
for internal_id in catalog.get('m_InternalIds', []):
    if internal_id.lower().startswith(('http://', 'https://')):
        errors.append('Remote dependency: ' + internal_id)
    if not internal_id.endswith('.bundle'):
        continue
    prefix = '{UnityEngine.AddressableAssets.Addressables.RuntimePath}'
    if not internal_id.startswith(prefix):
        errors.append('Unexpected bundle location: ' + internal_id)
        continue
    relative = internal_id[len(prefix):].strip('/\\').replace('\\', '/')
    path = aa / relative
    if not path.is_file():
        errors.append('Missing bundled dependency: ' + relative)
    else:
        bundle_files.append(dict(file=relative, bytes=path.stat().st_size,
                                 sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
assembly = (data / 'Managed' / 'Assembly-CSharp.dll').read_bytes()
diagnostics = {name: name.encode() in assembly for name in ('VerificationRunner', 'CampaignRunner', 'SoakRunner')}
if any(diagnostics.values()):
    errors.append('A development verification driver remains in the Release assembly')
if not bundle_files:
    errors.append('No bundled dependencies found')
result = dict(passed=not errors, scope='Static Release package audit; does not substitute for gameplay or art acceptance',
              unity='2022.3.62f2', addressables=settings.get('m_AddressablesVersion'),
              player=str(player), diagnostic_types_present=diagnostics,
              assembly_sha256=hashlib.sha256(assembly).hexdigest(),
              catalog_sha256=hashlib.sha256((aa / 'catalog.json').read_bytes()).hexdigest(),
              bundled_dependencies=bundle_files, errors=errors)
output = Path(sys.argv[2])
output.write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(dict(passed=result['passed'], bundled_dependencies=len(bundle_files), errors=errors), ensure_ascii=False))
raise SystemExit(0 if result['passed'] else 1)
