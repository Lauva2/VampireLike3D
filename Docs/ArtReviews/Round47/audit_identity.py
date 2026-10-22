from pathlib import Path
import json,hashlib
root=Path('.')
load=lambda p:json.loads(Path(p).read_text(encoding='utf-8-sig'))
base=load('Artifacts/Round47/BuildIdentity.json'); baseline={x['file']:x['sha256'].lower() for x in base['files']}
identity={}
for n in ['Round47','UI47','Probe47','Soak47Short']:
 b=load(f'Artifacts/{n}/BuildIdentity.json'); f={x['file']:x['sha256'].lower() for x in b['files']}
 v=load(f'Artifacts/{n}/Verification.json') if Path(f'Artifacts/{n}/Verification.json').exists() else load(f'Artifacts/{n}/Summary.json')
 e=load(f'Artifacts/{n}/ProcessExit.json')
 identity[n]=dict(player=b['player'],sameFileNameHashList=f==baseline,fileCount=len(f),passed=v.get('passed'),finished=v.get('finished'),checks=len(v.get('checks',[])),errors=v.get('errors'),exitCode=e['exitCode'])
seq={p.name:len(list(p.glob('*.jpg'))) for p in Path('Artifacts/Round47/Sequences').iterdir() if p.is_dir()}
source=load('Artifacts/Round47/SourceIdentity.json')
obj=dict(version=source['version'],sourceFiles=len(source['files']),identity=identity,sequenceDirectories=len(seq),sequenceFrames=sum(seq.values()),sequences=seq,releaseMatch=load('Artifacts/Release47BuildMatch.json'),scope='Non-scoring identity/completeness corroboration; no visual scores derived from checks or hashes.')
Path('Docs/ArtReviews/Round47/IdentityAudit.json').write_text(json.dumps(obj,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(obj,ensure_ascii=False,indent=2))
