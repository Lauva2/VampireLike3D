import json
from pathlib import Path
r=json.loads(Path('Docs/ArtReviews/Round51/World/Review.json').read_text(encoding='utf-8-sig'))
print([(x['path'],x.get('worldSidecar'),x.get('missingWorldMetadataReason')) for x in r['sourceMetadata'] if x.get('seed') is None])
