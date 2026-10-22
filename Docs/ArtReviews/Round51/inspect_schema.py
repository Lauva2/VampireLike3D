import json
from pathlib import Path
for file in ['Docs/ArtReviews/Round51/World/Review.json','Docs/ArtReviews/Round51/UI/ObservedEvidence.json','Docs/ArtReviews/Round51/Combat/ObservedEvidence.json','Docs/ArtReviewResult47.json']:
 d=json.loads(Path(file).read_text(encoding='utf-8-sig'))
 print(file)
 print([(k, len(v) if isinstance(v,(list,dict)) else str(v)[:100]) for k,v in d.items()])
 if 'actualViewing' in d: print('actualViewing',[(k,len(v) if isinstance(v,(list,dict)) else v) for k,v in d['actualViewing'].items()])
 if 'gates' in d: print('gates',d['gates'])
