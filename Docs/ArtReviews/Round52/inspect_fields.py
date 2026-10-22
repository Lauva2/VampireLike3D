import json
from pathlib import Path
for f in ['UI/ObservedEvidence.json','Combat/ObservedEvidence.json']:
 d=json.loads((Path('Docs/ArtReviews/Round52')/f).read_text(encoding='utf-8-sig'));print(f,[(k,len(v) if isinstance(v,(dict,list)) else str(v)[:90]) for k,v in d.items()])
