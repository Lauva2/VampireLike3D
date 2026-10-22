import json
from pathlib import Path
p=Path('Docs/ArtReviews/Round51/World/Review.json')
d=json.loads(p.read_text(encoding='utf-8-sig'))
d['limitations'][1]='Static146 via40 complete-frame sheets,46 original files opened; large2560 image may transport at2048px, not pixel-perfect1:1 inspection.'
d['limitations'][2]='Three character presentation PNGs and two Probe load-failure PNGs have no world JSON; unavailable fields stay null. Gameplay PNG world includes seed/character/seconds/abilities/sites when available.'
p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
print('Corrected manifest limitation count46 and explicit five missing world sidecars')
