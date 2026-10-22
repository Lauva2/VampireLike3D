from pathlib import Path
import json,hashlib
base=Path('Docs/ArtReviews/Round51');load=lambda p:json.loads(Path(p).read_text(encoding='utf-8-sig'))
raw=load('Artifacts/Round51/BuildIdentity.json');ref={x['file']:x['sha256'].lower() for x in raw['files']};ids={}
for n in ['Round51','UI51','Probe51']:
 b=load(f'Artifacts/{n}/BuildIdentity.json');f={x['file']:x['sha256'].lower() for x in b['files']};v=load(f'Artifacts/{n}/Verification.json');e=load(f'Artifacts/{n}/ProcessExit.json');ids[n]=dict(player=b['player'],fileCount=len(f),sameNameHashList=f==ref,finished=v['finished'],passed=v['passed'],checks=len(v['checks']),errors=v['errors'],exitCode=e['exitCode'])
seq={p.name:len(list(p.glob('*.jpg'))) for p in Path('Artifacts/Round51/Sequences').iterdir() if p.is_dir()}
src=load('Artifacts/Round51/SourceIdentity.json');o=dict(candidate='Round51',version=src['version'],sourceFiles=len(src['files']),identity=ids,sequenceCount=len(seq),sequenceFrames=sum(seq.values()),sequences=seq,workspace=load('Artifacts/Workspace51Identity.json'),releaseMatch=load('Artifacts/BuildMatch51.json'),releaseAuditPassed=load('Artifacts/ReleaseAudit51.json')['passed'],scope='Non-scoring identity/completeness corroboration only. No art score derived from assertions/hashes. Long test/final frame pending.')
(base/'IdentityAudit.json').write_text(json.dumps(o,ensure_ascii=False,indent=2),encoding='utf-8');print(json.dumps({k:o[k] for k in ['version','sourceFiles','identity','sequenceCount','sequenceFrames']},indent=2))
