from pathlib import Path
import json
base=Path('Docs/ArtReviews/Round47')
seedLimit='Full47/UI47的world、RuntimeStatus、Player.log未记录可用具体地图seed，本轮这些画面的seed未知，不臆造；位置/角色/能力仅据各原图sidecar/metadata已有字段。RuntimeStatus是周期快照，非最终状态。Soak47Short末帧seed=442071442有CurrentBuild/BuildSamples记录，不能移作Full/UI种子。'
for path in ['Docs/ArtReviewResult47.json',str(base/'ObservedEvidence.json')]:
 p=Path(path);o=json.loads(p.read_text(encoding='utf-8'));o['limitations'].append(seedLimit)
 if 'supplementaryVisualEvidence' in o:o['supplementaryVisualEvidence'][0]['seed']=442071442
 o['captureSeedAvailability']={'Round47':None,'UI47':None,'Soak47Short':442071442,'note':'Null means unavailable in current capture metadata, not seed zero.'}
 if 'items' in o:
  for row in o['items']:
   clean=[]
   for e in row['evidence']:
    if e.startswith('SheetManifest.json'):e='Docs/ArtReviews/Round47/UI/SheetManifest.json'
    elif e.startswith('SequenceSheetManifest.json'):e='Docs/ArtReviews/Round47/UI/SequenceSheetManifest.json'
    elif e.startswith('Combat/ObservedEvidence.json'):e='Docs/ArtReviews/Round47/Combat/ObservedEvidence.json'
    clean.append(e)
   row['evidence']=list(dict.fromkeys(clean))
 p.write_text(json.dumps(o,ensure_ascii=False,indent=2),encoding='utf-8')
p=base/'IndependentReview.md';s=p.read_text(encoding='utf-8').replace('地图种子/位置/能力等按各原图.world/.quality与序列metadata/timestamps保留','位置/角色/能力/采样时间仅按各原图.world/.quality及序列metadata/timestamps已有字段保留；Full/UI具体seed没有记录，保持未知；短测末帧seed=442071442另有CurrentBuild/BuildSamples记录');s+='\n- '+seedLimit+'\n';p.write_text(s,encoding='utf-8')
p=Path('Docs/ArtReview.md');s=p.read_text(encoding='utf-8');s+='\n补充复现元数据限制：'+seedLimit+'\n';p.write_text(s,encoding='utf-8')
Path('Docs/ArtReviewResult.json').write_bytes(Path('Docs/ArtReviewResult47.json').read_bytes())
p=base/'IdentityAudit.json';o=json.loads(p.read_text(encoding='utf-8'));su=json.loads(Path('Artifacts/Soak47Short/Summary.json').read_text(encoding='utf-8-sig'));o['identity']['Soak47Short'].update(finishedUtc=su['finishedUtc'],wallSeconds=su['wallSeconds'],errorCount=su['errorCount'],summaryUsesDifferentSchema=True);p.write_text(json.dumps(o,ensure_ascii=False,indent=2),encoding='utf-8')
res=json.loads(Path('Docs/ArtReviewResult47.json').read_text(encoding='utf-8'));missing=[e for r in res['items'] for e in r['evidence'] if not Path(e).exists()]
print('Missing row evidence paths:',missing)
print('20 rows:',len(res['items']),'total:',sum(r['score'] for r in res['items']),'minimum:',min(r['score'] for r in res['items']),'gates:',len(res['hardGates']),all(g['passed'] for g in res['hardGates']))
print('Shared identical:',Path('Docs/ArtReviewResult.json').read_bytes()==Path('Docs/ArtReviewResult47.json').read_bytes())
print('Original paths exist:',all(Path(p).exists() for p in json.loads((base/'ObservedEvidence.json').read_text(encoding='utf-8'))['uniqueOriginalFiles']))
