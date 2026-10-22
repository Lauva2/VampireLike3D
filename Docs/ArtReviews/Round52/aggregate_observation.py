import json, hashlib, csv
from pathlib import Path
from PIL import Image
from datetime import datetime,timezone
base=Path('Docs/ArtReviews/Round52')
def read(p): return json.loads(Path(p).read_text(encoding='utf-8-sig'))
def sha(p): return hashlib.sha256(Path(p).read_bytes()).hexdigest()
w=read(base/'World/Review.json'); u=read(base/'UI/ObservedEvidence.json'); c=read(base/'Combat/ObservedEvidence.json')
assert 'incomplete' not in c['status'].lower(), 'Wait for Combat actual completed report'
paths={}; originals={}; sheets={}
def add(path,who,mode): paths.setdefault(path,{}).setdefault(who,[]).append(mode)
def orig(path,who): originals.setdefault(path,[]).append(who);add(path,who,'source opened individually')
for path in w['actualViewing']['staticImages']: add(path,'art_lead47','full-frame contact sheet')
for path in w['actualViewing']['originalFiles']: orig(path,'art_lead47')
sheets['art_lead47']=w['actualViewing']['sheets']
for x in u['staticSourcesObserved']: add(x['path'],'art_ui47','full-frame contact sheet')
for seq,desc in u['sequenceFrames'].items():
 for x in desc['sources']: add(x['path'],'art_ui47','full-frame contact sheet')
for group in ['observedOriginals','observedSequenceOriginals']:
 for x in u[group]: orig(x['path'],'art_ui47')
sheets['art_ui47']=u['observedSheets']+u['observedSequenceSheets']
for x in c['sheets']:
 for path in x['sources']: add(path,'art_combat47','full-frame contact sheet')
for x in c['originals']: orig(x['path'],'art_combat47')
sheets['art_combat47']=[x['sheet'] for x in c['sheets']]
raw=[];seqs={}
for path,viewers in sorted(paths.items()):
 p=Path(path); pixels=list(Image.open(p).size)
 x={'path':path,'sha256':sha(p),'pixels':pixels,'reviewers':viewers}
 if '/Sequences/' in path:
  seq=p.parent.name; meta=p.parent/'metadata.json';md=read(meta) if meta.exists() else None
  x['sequenceMetadata']={'path':str(meta).replace('\\','/'),'sha256':sha(meta),'data':md} if md else None
  x['seed']=None;x['seedReason']='No per-frame seed in sequence metadata; adjacent PNG contexts are separately identified, not assigned to JPG frames.'
  frame=int(p.stem);x['frameIndex']=frame
  if md and md.get('fixedSimulationFps'):x['relativeSimulationSeconds']=frame/md['fixedSimulationFps']
  tp=p.parent/'timestamps.csv'
  if tp.exists():
   ts={int(t['frame']):float(t['wallSeconds']) for t in csv.DictReader(tp.open(encoding='utf-8-sig'))}
   x['relativeWallSeconds']=ts.get(frame);x['timestampsSidecar']={'path':str(tp).replace('\\','/'),'sha256':sha(tp)}
  if seq in u['sequenceFrames']:
   ts={int(t['frame']):float(t['wallSeconds']) for t in u['sequenceFrames'][seq]['timestamps']}
   x['relativeWallSeconds']=ts.get(frame)
  seqs.setdefault(seq,[]).append(frame)
 else:
  wp=p.with_suffix('.world.json');qp=p.with_suffix('.quality.json')
  if wp.exists():
   wd=read(wp); x['worldSidecar']={'path':str(wp).replace('\\','/'),'sha256':sha(wp)}
   for key in ['seed','character','seconds','abilities','global','current','origin','camera','zoom']:x[key]=wd.get(key)
   x['sites']=[{'id':s.get('id'),'type':s.get('type'),'chunk':s.get('chunk'),'complete':s.get('complete')} for s in wd.get('sites',[])]
  else:x['worldSidecar']=None;x['seed']=None;x['seedReason']='No world sidecar supplied; unknown rather than inferred.'
  x['qualitySidecar']={'path':str(qp).replace('\\','/'),'sha256':sha(qp),'data':read(qp)} if qp.exists() else None
 raw.append(x)
static=[x for x in raw if '/Sequences/' not in x['path']];dynamic=[x for x in raw if '/Sequences/' in x['path']]
identity=read(base/'IdentityAudit.json')
out={'candidate':'Round52','version':'1.2.0','recordedAt':datetime.now(timezone.utc).isoformat(),'status':'Independent domain observations complete; final supplement pending when supplied','rubric':'Docs/ArtReview.md original sections1-6;100 points/20 integer items/original10 hard gates','scope':'Expanded frozen candidate:3 characters,8 ordinary/8 elite,6 bosses,6 single biomes+Mixed,9 weapons,formal UI and sampled motions/feedback. New60 upgrades/25 tasks included as categories and new targeted rows; not every permutation exhausted.',
'reports':{'World':'Docs/ArtReviews/Round52/World/IndependentWorldReview.md','WorldEvidence':'Docs/ArtReviews/Round52/World/Review.json','UI':'Docs/ArtReviews/Round52/UI/IndependentUIReview.md','UIEvidence':'Docs/ArtReviews/Round52/UI/ObservedEvidence.json','Combat':'Docs/ArtReviews/Round52/Combat/IndependentCombatReview.md','CombatEvidence':'Docs/ArtReviews/Round52/Combat/ObservedEvidence.json'},
'counts':{'uniqueStaticImagesViewed':len(static),'uniqueSequenceFramesViewed':len(dynamic),'sequenceFramesAvailable':identity['sequenceFrames'],'sequenceCount':len(seqs),'uniqueOriginalFilesOpened':len(originals),'uniqueAuxiliarySheetsViewed':len(set(y for x in sheets.values() for y in x))},
'reviewerActualViewing':{'art_lead47':w['actualViewing'],'art_ui47':u['counts'],'art_combat47':{'sheets':len(c['sheets']),'originals':len(c['originals']),'dynamic':c['uniqueDynamicFramesObserved'],'static':c['uniqueStaticImagesObserved']}},
'authorship':'Lead personally viewed only listed World images, plus reads of UI/Combat reports. UI/Combat source viewing remains their actual independent observation, not represented as lead viewing every image.',
'sheetsByReviewer':sheets,'originalsByPath':originals,'sequenceFramesObserved':{k:sorted(set(v)) for k,v in seqs.items()},'rawSources':raw,
'identityAudit':'Docs/ArtReviews/Round52/IdentityAudit.json','terrainAssessment':'Artifacts/TerrainCoverage52Assessment.json',
'limitations':['Only listed52 observations; no older screenshots/scores/counts migrated.','Contact sheets preserve full frames but are reduced indexes; originals additionally opened are explicit. Larger original files may be downscaled by image transport; not pixel-perfect1:1 display of every file.','Fixed12fps controlled-healing/immortal capture flags are retained per sequence; do not prove subframe damage timing, natural difficulty or normal-speed hands-on play. Realtime effects/death/feedback instead use actual wall-clock timestamps, not the requested interval as measured FPS.','Some presentation/load-failure PNGs lack world sidecars; seed remains unknown. Dynamic metadata usually lacks per-frame seed/role/abilities: adjacent PNG world snapshots remain separate contexts.','Shrine-specific complete open/cancel/reopen/choose/restore regular upgrade sequence and HUD continuous scrolling were not sampled. Stable state pairs and functional logs do not replace those continuous observations.','Not every60 upgrade/25 task row and all state permutations or all random terrain seeds exhaustively viewed.','Actual52 terrain-selected60-image assessment reports zero exact camera-clear pixels and no inactive chunks; broad194-image audit with intentional studio clear backgrounds is separately retained and not a world-hole verdict. Neither audit awards art points.','Long7200s run and exclusive nine-weapon deep-build final frames not supplied/observed at core review time; no long-run/performance conclusion.','No audio listening, desktop control, Player launch, build, runtime edit or original image modification by these reviewers.']}
(base/'ObservedEvidence.json').write_text(json.dumps(out,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(out['counts']))
