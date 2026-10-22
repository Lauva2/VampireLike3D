import json
from pathlib import Path
u=json.loads(Path('Docs/ArtReviews/Round51/UI/ObservedEvidence.json').read_text(encoding='utf-8-sig'))
print(u['staticSourcesObserved'][0])
c=json.loads(Path('Docs/ArtReviews/Round51/Combat/ObservedEvidence.json').read_text(encoding='utf-8-sig'))
print(c['status'], c['uniqueDynamicFramesObserved'],len(c['originals']))
