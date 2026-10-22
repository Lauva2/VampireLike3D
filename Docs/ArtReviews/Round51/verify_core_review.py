import json,re
from pathlib import Path
p=Path('Docs/ArtReviews/Round51/World/IndependentWorldReview.md')
s=p.read_text(encoding='utf-8-sig')
s=s.replace('这里仅提交A/B/C分域；六Boss完整表现还需Combat独立连续观察佐证，未形成完整总评。','这里提交A/B/C分域；六Boss完整表现现由Combat独立连续观察佐证，完整20项与门槛见上级IndependentReview.md。')
s=s.replace('B3完整动作由另一名独立评审补充，总评会明确归属。','B3完整动作由art_combat47独立实看六套攻击及141/141死亡帧佐证；总评明确归属，本人不冒称观看过其动态。')
p.write_text(s,encoding='utf-8')
r=json.loads(Path('Docs/ArtReviewResult51.json').read_text(encoding='utf-8-sig'))
o=json.loads(Path('Docs/ArtReviews/Round51/ObservedEvidence.json').read_text(encoding='utf-8-sig'))
assert len(r['items'])==20 and len(r['hardGates'])==10
assert sum(i['score'] for i in r['items'])==91 and min(i['score'] for i in r['items'])==4
for i in r['items']:
 assert i['evidence'] and i['revision'] and i['observation']
 for f in i['evidence']:assert Path(f).exists(),f
assert len(o['rawSources'])==336+856 and len({x['path'] for x in o['rawSources']})==1192
assert all(x['path'].startswith(('Artifacts/Round51/','Artifacts/UI51/','Artifacts/Probe51/')) for x in o['rawSources'])
for md in [Path('Docs/ArtReviews/Round51/IndependentReview.md'),p]:
 for dest in re.findall(r'\]\(([^)]+)\)',md.read_text(encoding='utf-8-sig')):
  if not dest.startswith(('http:','https:')): assert (md.parent/dest).resolve().exists(),str(md)+' -> '+dest
assert r['supplementStatus']['sharedFinalConclusionUpdated'] is False
print('Verified:20 rows/10 gates/min4/total91; all row evidence and Markdown links exist;1192 distinct current51 raw sources;shared untouched.')
