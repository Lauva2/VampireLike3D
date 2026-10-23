from pathlib import Path
import json
out=Path('Docs/ArtReviews/Round54');data={}
for group in ['Round54','UI54']:
 entries=[]
 for d in sorted((Path('Artifacts')/group/'Sequences').iterdir()):
  if not d.is_dir():continue
  frames=sorted(p for p in d.iterdir() if p.suffix.lower() in ['.jpg','.png','.jpeg']);entries.append({'sequence':d.as_posix(),'count':len(frames)})
 data[group]={'sequences':entries,'sequenceCount':len(entries),'frameCount':sum(e['count'] for e in entries)}
(out/'Sequence54Inventory.json').write_text(json.dumps(data,indent=2),encoding='utf-8');print({k:{x:v[x] for x in ['sequenceCount','frameCount']} for k,v in data.items()})
