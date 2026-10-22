import json
from pathlib import Path
u=json.loads(Path('Docs/ArtReviews/Round51/UI/ObservedEvidence.json').read_text(encoding='utf-8-sig'))
c=json.loads(Path('Docs/ArtReviews/Round51/Combat/ObservedEvidence.json').read_text(encoding='utf-8-sig'))
print('UI observed original sample',u['observedOriginals'][0])
print('UI sequence context',[(k,list(v.keys())) for k,v in u['sequenceFrames'].items()])
print('Combat sheet keys',c['sheets'][0].keys())
print('Combat sheet first all',str(c['sheets'][0])[-2200:])
print('Combat original last',str(c['originals'][-1]))
