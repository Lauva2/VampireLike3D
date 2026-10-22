import json
from pathlib import Path
base=Path('Docs/ArtReviews/Round52/World');r=json.loads((base/'Review.json').read_text(encoding='utf-8-sig'));x=next(x for x in r['rows'] if x['id']=='B3');p=base/'IndependentWorldReview.md';s=p.read_text(encoding='utf-8-sig');s='\n'.join(f"| {x['id']} {x['title']} | {x['score']} | {x['observation']} 修改：{x['revision']} |" if line.startswith('| B3 ') else line for line in s.split('\n'));p.write_text(s,encoding='utf-8')
for n in ['Review.json','ObservedEvidence.json']:
 p=base/n;d=json.loads(p.read_text(encoding='utf-8-sig'));d['limitations']=[t.replace('总评本人本轮尚未看动态，B3完整动作待Combat独立观察佐证。','总评本人本轮没有看动态；B3完整动作由Combat独立观察佐证，未冒称本人看过。') for t in d['limitations']];p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
print('Synchronized World B3 independent motion attribution, no score change.')
