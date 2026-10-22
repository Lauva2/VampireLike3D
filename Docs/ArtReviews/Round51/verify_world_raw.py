import json, hashlib
from pathlib import Path
r=json.loads(Path('Docs/ArtReviews/Round51/World/Review.json').read_text(encoding='utf-8-sig'))
fail=[]
for x in r['sourceMetadata']:
 p=Path(x['path'])
 if hashlib.sha256(p.read_bytes()).hexdigest()!=x['sha256']: fail.append(x['path'])
print('World raw SHA matches:',len(r['sourceMetadata'])-len(fail),'mismatches:',fail)
print('World missing seed:',sum(x.get('seed') is None for x in r['sourceMetadata']))
print('World seeds:',sorted(set(str(x.get('seed')) for x in r['sourceMetadata'])))
u=json.loads(Path('Docs/ArtReviews/Round51/UI/ObservedEvidence.json').read_text(encoding='utf-8-sig'))
print('UI sequence structure:',str(u['sequenceFrames'])[:800])
c=json.loads(Path('Docs/ArtReviews/Round51/Combat/ObservedEvidence.json').read_text(encoding='utf-8-sig'))
print('Combat sheet sample:',str(c['sheets'][0])[:800])
print('Combat original sample:',str(c['originals'][0])[:600])
f=json.loads(Path('Docs/ArtReviewResult47.json').read_text(encoding='utf-8-sig'))
print('gate structure:',str(f['hardGates'])[:1800])
