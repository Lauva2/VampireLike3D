import json
from pathlib import Path
u=json.loads(Path('Docs/ArtReviews/Round54/UI54Fresh/ObservedEvidence.json').read_text(encoding='utf-8'));print([x['sheet'] for x in u['contactSheetsActuallyViewed']]);print(u.keys());print(u['sequences'][0])
c=json.loads(Path('Docs/ArtReviews/Round54/Combat54Fresh/ObservedEvidence.json').read_text(encoding='utf-8'));print(c['sequenceSamples'][0].keys());print(c['sequenceSamples'][0]['frames'][0]);print(type(c['observedDynamicPaths'][0]))
