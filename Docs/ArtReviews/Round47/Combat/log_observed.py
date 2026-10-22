import json,sys,hashlib
from pathlib import Path
root=Path('E:/UnityProject/VampireLike3D'); out=root/'Docs/ArtReviews/Round47/Combat'
p=out/'ObservedEvidence.json'
data=json.loads(p.read_text('utf8')) if p.exists() else {'candidate':'Round47','reviewer':'/root/art_combat47','rubric':'Docs/ArtReview.md sections 1-6','sheets':[],'originals':[],'batches':[]}
index=json.loads((out/'SheetSourceIndex.json').read_text('utf8'))
batch=json.loads(sys.argv[1])
for s in batch.get('sheets',[]):
    entry=next(e for e in index if Path(e['sheet']).name==s)
    if entry not in data['sheets']:data['sheets'].append(entry)
for f in batch.get('originals',[]):
    entry={'path':f,'sha256':hashlib.sha256((root/f).read_bytes()).hexdigest().upper()}
    if entry not in data['originals']:data['originals'].append(entry)
data['batches'].append(batch)
paths={s for e in data['sheets'] for s in e['sources']}|{e['path'] for e in data['originals']}
data['uniqueDynamicFramesObserved']=sum('/Sequences/' in x for x in paths)
data['uniqueStaticImagesObserved']=sum('/Sequences/' not in x for x in paths)
p.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf8')
print(data['uniqueDynamicFramesObserved'], data['uniqueStaticImagesObserved'])
