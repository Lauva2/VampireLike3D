import json,re,hashlib
from pathlib import Path
base=Path('Docs/ArtReviews/Round52');p=base/'World/IndependentWorldReview.md';s=p.read_text(encoding='utf-8-sig').replace('B3完整表现须等Combat连续观察佐证，尚不形成完整100分结论。','B3现由Combat六攻击及130/130死亡帧独立观察佐证；完整20项与门槛见上级总评。').replace('其观察等独立Combat报告。','其实际观察已由独立Combat报告提供。');p.write_text(s,encoding='utf-8')
p=base/'World/ObservedEvidence.json';d=json.loads(p.read_text(encoding='utf-8-sig'));d['status']='assigned52static domain complete; B3 motion corroborated independently by Combat; preflight static supplement reviewed; actual7200 final pending';p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
r=json.loads(Path('Docs/ArtReviewResult52.json').read_text(encoding='utf-8-sig'));o=json.loads((base/'ObservedEvidence.json').read_text(encoding='utf-8-sig'))
assert len(r['items'])==20 and len(r['hardGates'])==10
assert sum(x['score'] for x in r['items'])==r['total']==91
assert min(x['score'] for x in r['items'])==4
for x in r['items']:
 assert x['observation'] and x['revision'] and x['reviewer']
 for f in x['evidence']:assert Path(f).exists(),f
assert len(o['rawSources'])==337+838 and len({x['path'] for x in o['rawSources']})==1175
for x in o['rawSources']:
 assert x['path'].startswith(('Artifacts/Round52/','Artifacts/UI52/','Artifacts/Probe52/','Artifacts/Soak52Preflight/'))
 assert hashlib.sha256(Path(x['path']).read_bytes()).hexdigest().lower()==x['sha256'].lower(),x['path']
for p in [base/'IndependentReview.md',base/'World/IndependentWorldReview.md',base/'World/DeepPreflightReview.md']:
 for dest in re.findall(r'\]\(([^)]+)\)',p.read_text(encoding='utf-8-sig')):
  if not dest.startswith(('http:','https:')):assert (p.parent/dest).resolve().exists(),str(p)+' -> '+dest
assert r['supplementStatus']['sharedFinalConclusionUpdated'] is False
print('Verified52:20 rows/10 gates/min4/total91;1175 raw source hashes match;all evidence/Markdown links exist;shared unchanged.')
