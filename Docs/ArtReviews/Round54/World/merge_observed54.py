import json,hashlib
from pathlib import Path
out=Path('Docs/ArtReviews/Round54');w=json.loads((out/'World54FreshObserved.json').read_text(encoding='utf-8'));u=json.loads((out/'UI54Fresh/ObservedEvidence.json').read_text(encoding='utf-8'));c=json.loads((out/'Combat54Fresh/ObservedEvidence.json').read_text(encoding='utf-8'))
by={};original={};sheets={};errors=[]
def add(path,reviewer,target=by,sha=None):
 p=path.replace('\\','/');q=Path(p);actual=hashlib.sha256(q.read_bytes()).hexdigest().upper()
 if sha and actual!=sha.upper():errors.append({'path':p,'expected':sha,'actual':actual})
 if p not in target:target[p]={'path':p,'sha256':actual,'reviewers':[]}
 if reviewer not in target[p]['reviewers']:target[p]['reviewers'].append(reviewer)
for e in w['sourceFiles']:add(e['path'],'world',sha=e['sha256'])
for p in w['originalFilesViewed']:add(p,'world',original)
for p in w['sheetsViewed']:add(p,'world',sheets)
for e in u['contactSheetsActuallyViewed']:
 add(e['sheet'],'ui',sheets)
 for f in e['files']:add(f['path'],'ui',sha=f['sha256'])
for e in u['originalFilesActuallyOpened']:add(e['path'],'ui',original,sha=e['sha256']);add(e['path'],'ui',sha=e['sha256'])
for e in c['sequenceSamples']:
 for f in e['frames']:add(f['path'],'combat',sha=f['sha256'])
for e in c['originalFilesOpened']:add(e['path'],'combat',original,sha=e['sha256']);add(e['path'],'combat',sha=e['sha256'])
for e in c['sheetsViewed']:add(e['path'],'combat',sheets,sha=e['sha256'])
sta=[e for p,e in by.items() if '/Sequences/' not in p];dyn=[e for p,e in by.items() if '/Sequences/' in p]
counts={'differentStaticFilesByPathAndSha':len(sta),'differentDynamicFrameFilesByPathAndSha':len(dyn),'differentOriginalFilesOpenedByPathAndSha':len(original),'differentSheetsViewed':len(sheets),'differentSequencesRepresented':len(set(str(Path(e['path']).parent).replace('\\','/') for e in dyn)),'staticDistinctContentSha256':len(set(e['sha256'] for e in sta)),'dynamicDistinctContentSha256':len(set(e['sha256'] for e in dyn)),'originalDistinctContentSha256':len(set(e['sha256'] for e in original.values())),'inventoryDynamicFrameCount':2629}
record={'candidate':'Round54','reviewDate':'2026-10-23','independentReviewers':['/root/popup54_world','/root/popup54_ui','/root/popup54_combat'],'rubric':'Docs/ArtReview.md original100/20items/10hard gates, unchanged','aggregationRule':'Union of actually viewed source file paths with SHA256 verified against disk; a source viewed by multiple reviewers counted once. Separate distinct-content SHA counts supplied. Available inventory is not actual viewing. Lead personally viewed only World54 domain; UI and Combat remain attributed independent contributions.','counts':counts,'reviewerCounts':{'world':w['counts'],'ui':u['counts'],'combat':c['counts']},'sourceReports':['World54Fresh.md','UI54Fresh/IndependentUIReview.md','Combat54Fresh/IndependentCombatReview.md'],'sourceEvidence':['World54FreshObserved.json','UI54Fresh/ObservedEvidence.json','Combat54Fresh/ObservedEvidence.json'],'staticFilesActuallyObserved':sta,'dynamicFramesActuallyObserved':dyn,'originalFilesActuallyOpened':list(original.values()),'contactSheetsActuallyViewed':list(sheets.values()),'shaVerificationErrors':errors,'identityAudit':'Identity54FreshAudit.json','inventoryOnly':'Sequence54Inventory.json','limitations':['No52/53image/scores/viewing counts reused.','No reviewer operated desktop/emulator,launchedPlayer/build,changedruntime/originalimages.','All stored popup frames observed, not every runtime frame; discrete sampling not60FPS or exact0.24s visual measurement.','Finite seeds/text rows/combinations; action capture controlled healing/fixed or immortal targets does not establish natural difficulty.','Opening clips do not fully visually retest every close/reopen/refresh/disable/expiry path; behavioral reports are supporting evidence only.','Performance explicitly deferred; no7200frame required or performance/finalproduct pass claimed.','Main tree has two pre-existing missing generatedlink files;1311present match while frozen builds1313source/24AAmatch.']}
(out/'ObservedEvidence.json').write_text(json.dumps(record,ensure_ascii=False,indent=2),encoding='utf-8');print(json.dumps(counts,indent=2));print('SHA errors',errors)
