import json,sys,hashlib
from pathlib import Path
root=Path('E:/UnityProject/VampireLike3D');out=root/'Docs/ArtReviews/Round51/Combat';p=out/'ObservedEvidence.json'
data=json.loads(p.read_text('utf8')) if p.exists() else {'candidate':'Round51','version':'1.2.0','reviewer':'/root/art_combat47','rubric':'Docs/ArtReview.md sections 1-6','sheets':[],'originals':[],'batches':[],'status':'incomplete, ongoing observation; no score inherited'}
index=json.loads((out/'SheetSourceIndex.json').read_text('utf8'));batch=json.loads(sys.argv[1])
for s in batch.get('sheets',[]):
    e=next(e for e in index if Path(e['sheet']).name==s)
    if e not in data['sheets']:data['sheets'].append(e)
for f in batch.get('originals',[]):
    e={'path':f,'sha256':hashlib.sha256((root/f).read_bytes()).hexdigest().upper()}
    if e not in data['originals']:data['originals'].append(e)
data['batches'].append(batch);paths={s for e in data['sheets'] for s in e['sources']}|{e['path'] for e in data['originals']}
data['uniqueDynamicFramesObserved']=sum('/Sequences/' in x for x in paths);data['uniqueStaticImagesObserved']=sum('/Sequences/' not in x for x in paths)
p.write_text(json.dumps(data,ensure_ascii=False,indent=2),encoding='utf8');print('observed',len(data['sheets']),'sheets',len(data['originals']),'original files',data['uniqueDynamicFramesObserved'],'unique dynamic frames',data['uniqueStaticImagesObserved'],'statics')
