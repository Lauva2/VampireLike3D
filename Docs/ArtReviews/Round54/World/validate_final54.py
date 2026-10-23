import json,hashlib
from pathlib import Path
r=json.loads(Path('Docs/ArtReviewResult54.json').read_text(encoding='utf-8'));o=json.loads(Path('Docs/ArtReviews/Round54/ObservedEvidence.json').read_text(encoding='utf-8'));errors=[]
assert len(r['items'])==20 and len(r['hardGates'])==10 and r['total']==sum(x['score'] for x in r['items'])==91
assert all(x['passed'] for x in r['hardGates']) and min(x['score'] for x in r['items'])==4
for x in r['items']:
 assert x['evidence']
 for p in x['evidence']:
  if not Path(p).is_file():errors.append(p)
assert not errors, errors
assert not o['shaVerificationErrors']
for key in ['staticFilesActuallyObserved','dynamicFramesActuallyObserved','originalFilesActuallyOpened','contactSheetsActuallyViewed']:
 assert len(o[key])==len({x['path'] for x in o[key]})
print('PASS:20 unique scored items,10 passing original gates,total91,evidence paths exist,source SHA verified,unions have no repeatedpaths.')
for p in ['Docs/ArtReviewResult54.json','Docs/ArtReviews/Round54/IndependentReview.md','Docs/ArtReviews/Round54/ObservedEvidence.json']:
 q=Path(p);print(p,q.stat().st_size,hashlib.sha256(q.read_bytes()).hexdigest().upper())
