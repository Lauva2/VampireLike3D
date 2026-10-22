from pathlib import Path
p=Path('Docs/ArtReviews/Round51/write_core_review.py')
s=p.read_text(encoding='utf-8-sig').replace('import json','import json, os',1)
s=s.replace("w['rows'][5]['evidence'].append('Docs/ArtReviews/Round51/Combat/IndependentCombatReview.md')", "if 'Docs/ArtReviews/Round51/Combat/IndependentCombatReview.md' not in w['rows'][5]['evidence']: w['rows'][5]['evidence'].append('Docs/ArtReviews/Round51/Combat/IndependentCombatReview.md')")
s=s.replace("w['rows'][5]['observation']+=' 六套", "w['rows'][5]['observation']=w['rows'][5]['observation'].split(' 六套攻击与141/141死亡帧由art_combat47')[0]+' 六套")
s=s.replace("[dict(x,reviewer='/root/art_ui47') for x in ui]", "[dict(x,reviewer='/root/art_ui47') for x in ui if x['id'].startswith('D')]")
s=s.replace("f'[{Path(x).name}](../../../{x.removeprefix(\"Docs/\")})'", "f'[{Path(x).name}]({os.path.relpath(x,base).replace(chr(92),chr(47))})'")
p.write_text(s,encoding='utf-8')
