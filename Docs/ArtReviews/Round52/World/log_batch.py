import json,hashlib,sys
from pathlib import Path
from PIL import Image
from datetime import datetime,timezone
out=Path('Docs/ArtReviews/Round52/World');p=out/'ObservedEvidence.json';d=json.loads(p.read_text(encoding='utf-8-sig'));b=json.loads(Path(sys.argv[1]).read_text(encoding='utf-8-sig'))
manifest={}
for name in ['CharacterSheetManifest.json','WorldSheetManifest.json']:
 q=out/name
 if q.exists():manifest.update(json.loads(q.read_text(encoding='utf-8-sig')))
view=d['actualViewing'];oldSheets=set(view['sheets']);oldSources=set(view['staticImages']);oldOriginals=set(view['originalFiles'])
assert b['id'] not in [x['id'] for x in d['batches']], 'Already recorded batch'
for sheet in b['sheets']:
 assert sheet in manifest,sheet
 for f in manifest[sheet]:oldSources.add(f)
 oldSheets.add(sheet)
for path in b['originals']:
 oldOriginals.add(path);oldSources.add(path)
raw=[]
for path in sorted(oldSources):
 q=Path(path);wp=q.with_suffix('.world.json');qp=q.with_suffix('.quality.json');world=json.loads(wp.read_text(encoding='utf-8-sig')) if wp.exists() else None
 raw.append({'path':path,'sha256':hashlib.sha256(q.read_bytes()).hexdigest(),'pixels':list(Image.open(q).size),'quality':json.loads(qp.read_text(encoding='utf-8-sig')) if qp.exists() else None,'worldSidecar':wp.as_posix() if wp.exists() else None,'world':world,'seed':world.get('seed') if world else None,'seedUnknownReason':None if world else 'No world sidecar; unknown rather than inferred.'})
view.update({'sheets':sorted(oldSheets),'staticImages':sorted(oldSources),'originalFiles':sorted(oldOriginals),'sheetCount':len(oldSheets),'staticCount':len(oldSources),'originalCount':len(oldOriginals)})
d['sourceMetadata']=raw;d['batches'].append(b);d['recordedAt']=datetime.now(timezone.utc).isoformat();p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
with (out/'BatchNotes.md').open('a',encoding='utf-8') as f:f.write('\n## 批次'+str(b['id'])+'：'+b['scope']+'\n\n'+b['observation']+'\n\n累计实际'+str(len(oldSheets))+'表/'+str(len(oldSources))+'不同静态/'+str(len(oldOriginals))+'原图；本批动态0，所有未看到范围继续未验证。\n')
print('Recorded batch',b['id'],'totals:',view['sheetCount'],view['staticCount'],view['originalCount'])
