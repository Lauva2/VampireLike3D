from pathlib import Path
import json,hashlib
r=Path('.'); p=r/'Docs/ArtReviews/Round54/Combat54Fresh/ObservedEvidence.json';e=json.loads(p.read_text(encoding='utf8'))
motion=[]
for m in sorted((r/'Artifacts/UI54/Sequences').glob('*/motion.jsonl')):
 rows=[json.loads(s) for s in m.read_text(encoding='utf-8-sig').splitlines() if s.strip()]
 motion.append({'path':m.as_posix(),'sha256':hashlib.sha256(m.read_bytes()).hexdigest(),'frameCount':len(rows),'first':rows[0],'last':rows[-1],'monotoneAlpha':all(a['alpha']<=b['alpha'] for a,b in zip(rows,rows[1:]))})
e['motionLogsRead']=motion
p.write_text(json.dumps(e,ensure_ascii=False,indent=2),encoding='utf8')
for n in ['IndependentCombatReview.md','Scores.json','ObservedEvidence.json']:
 q=p.parent/n; print(n,hashlib.sha256(q.read_bytes()).hexdigest())
print('motion',len(motion),sum(x['frameCount'] for x in motion))
