import json,hashlib
from pathlib import Path
r=Path('Artifacts');out=Path('Docs/ArtReviews/Round54');folders=['Round54','UI54','Probe54'];sets={};reports={}
for n in folders:
 identity=json.loads((r/n/'BuildIdentity.json').read_text(encoding='utf-8-sig'));sets[n]={e['file']:e['sha256'].upper() for e in identity['files']}
 report=json.loads((r/n/'Verification.json').read_text(encoding='utf-8-sig'));ex=json.loads((r/n/'ProcessExit.json').read_text(encoding='utf-8-sig'))
 reports[n]={'passed':report.get('passed'),'finished':report.get('finished'),'checks':len(report.get('checks',[])),'errors':report.get('errors'),'exitCode':ex.get('exitCode'),'buildFiles':len(sets[n]),'buildIdentitySha256':hashlib.sha256((r/n/'BuildIdentity.json').read_bytes()).hexdigest().upper(),'sourceIdentitySha256':hashlib.sha256((r/n/'SourceIdentity.json').read_bytes()).hexdigest().upper()}
base=sets['Round54'];match=all(s==base for s in sets.values())
audit={'candidate':'Round54','reviewer':'/root/popup54_world','sameBuildIdentityAcrossFullUIProbe':match,'reports':reports,'buildMatch':json.loads((r/'BuildMatch54.json').read_text(encoding='utf-8-sig')),'workspaceIdentity':json.loads((r/'Workspace54Identity.json').read_text(encoding='utf-8-sig')),'releaseAuditPassed':json.loads((r/'ReleaseAudit54.json').read_text(encoding='utf-8-sig')).get('passed'),'scope':'Read-only identity/report audit; does not add visual points, certify whole main tree exact identity, performance or final product.'}
for file in ['BuildMatch54.json','Workspace54Identity.json','ReleaseAudit54.json','Functional54EvidenceAudit.json','PopupMotion54Audit.json']:
 audit.setdefault('sourceAuditFiles',[]).append({'path':(r/file).as_posix(),'sha256':hashlib.sha256((r/file).read_bytes()).hexdigest().upper()})
(out/'Identity54FreshAudit.json').write_text(json.dumps(audit,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'sameBuild':match,'reports':reports},ensure_ascii=False,indent=2))
