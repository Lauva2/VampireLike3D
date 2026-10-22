from pathlib import Path
from PIL import Image
import json,hashlib
r=Path('E:/UnityProject/VampireLike3D');o=r/'Docs/ArtReviews/Round50/UI'
read=lambda p:json.loads((r/p).read_text(encoding='utf-8-sig'))
m=read('Docs/ArtReviews/Round50/UI/SheetManifest.json')
files=[f for a in m for f in a['sources']]
opened=['Artifacts/UI50/Captures/'+n+'.png' for n in ['ui-selection-expansion-locked-1280x720','ui-selection-expansion-unlocked-1280x720','ui-hud-expansion-1280x720','ui-upgrade-1280x720','ui-upgrade-1920x1200']]
def item(p):
 f=r/p
 with Image.open(f) as im:dims=list(im.size)
 return {'path':p,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'pixels':dims}
v=read('Artifacts/UI50/Verification.json');b=read('Artifacts/UI50/BuildIdentity.json')
obs={'candidate':'Round50 / UI50','version':'1.2.0','reviewer':'/root/art_ui47','status':'static_precheck_only_candidate_replaced_expected','rubric':'Docs/ArtReview.md sections 1–6 original 20 items and 10 gates','documentsRead':['AGENTS.md','Docs/GameDesign.md including 1.1','Docs/ArtReview.md sections 1–6'],'observedSheets':[x['sheet'] for x in m],'staticSourcesObserved':[item(x) for x in files],'observedOriginals':[item(x) for x in opened],'sequenceFrames':{},'counts':{'staticDifferent':110,'staticOpenedAsSource':5,'dynamicDifferent':0,'auxiliarySheets':22},'runtimeReport':{'finished':v['finished'],'passed':v['passed'],'checkCount':len(v['checks']),'errors':v['errors'],'player':b['player'],'buildFileCount':len(b['files']),'exit':read('Artifacts/UI50/ProcessExit.json')},'identityBridge':read('Artifacts/BuildMatch50.json'),'limits':['All110 UI50 matrix images observed on complete-frame auxiliary sheets; five original source images additionally opened.','No dynamic50 frames observed and no full F2 score.','Primary agent reports Full50 diagnostic fixture assertion failure and expects candidate51; no complete50 art pass formed.','No51 sources or old47 scores included.','Added25 tasks/60 upgrades not each visually observed: progress matrix top rows only; skills matrix blessings top rows; supplementary scroll/category sources required for exhaustive added-text coverage.','No desktop controls, window/input automation, Player launch, build, runtime modifications or original image edits.']}
(o/'ObservedEvidence.json').write_text(json.dumps(obs,ensure_ascii=False,indent=2),encoding='utf-8')
print(len(v['checks']),len(b['files']))
