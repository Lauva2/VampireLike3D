from pathlib import Path
p=Path('Docs/ArtReviews/Round52/World/make_ready_world_sheets.py')
s=p.read_text(encoding='utf-8-sig')
s=s.replace("mixed=[root/f'mixed-boundary-{s}.png' for s in ['0-1','0-2','0-6','1-2','2-3','3-5']]+", "boundaries=sorted(root.glob('mixed-boundary-*.png'))\nmixed=boundaries+")
s=s.replace("if all(p.exists() for p in mixed):groups['mixed']=mixed", "if len(boundaries)>=6 and all(p.exists() for p in mixed):groups['mixed']=mixed")
p.write_text(s,encoding='utf-8')
