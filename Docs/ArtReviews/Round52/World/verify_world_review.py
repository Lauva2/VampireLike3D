import json,hashlib,re
from pathlib import Path
out=Path('Docs/ArtReviews/Round52/World');d=json.loads((out/'Review.json').read_text(encoding='utf-8-sig'));v=d['actualViewing'];assert (v['sheetCount'],v['staticCount'],v['originalCount'])==(len(v['sheets']),len(v['staticImages']),len(v['originalFiles']))
assert len(set(v['staticImages']))==146
for x in d['sourceMetadata']:
 assert x['path'].startswith(('Artifacts/Round52/','Artifacts/Probe52/'))
 assert hashlib.sha256(Path(x['path']).read_bytes()).hexdigest()==x['sha256']
for row in d['rows']:
 assert row['revision'] and row['observation']
 for f in row['evidence']:assert Path(f).exists(),f
assert len(d['rows'])==10 and sum(x['score'] for x in d['rows'])==46
print('World52 actual146 SHA verified;40 sheets/46 originals;10 domain rows46/50;no cross-version raw paths.')
print('Unknown seed paths:',[x['path'] for x in d['sourceMetadata'] if x['seed'] is None])
