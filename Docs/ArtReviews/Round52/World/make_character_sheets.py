from PIL import Image,ImageDraw
from pathlib import Path
import json
root=Path('Artifacts/Round52/Captures');out=Path('Docs/ArtReviews/Round52/World')
groups={'characters':sorted(root.glob('character-*.png')), 'enemies':sorted(p for p in root.glob('enemy-*.png') if not p.name.startswith('enemy-combat')), 'weapons':sorted(root.glob('weapon-*.png'))}
manifest={}
for group,paths in groups.items():
 for i in range(0,len(paths),4):
  chunk=paths[i:i+4];sheet=Image.new('RGB',(1600,950),(32,35,38));d=ImageDraw.Draw(sheet)
  for j,p in enumerate(chunk):
   im=Image.open(p);im.thumbnail((800,445));x=(j%2)*800;y=(j//2)*475;sheet.paste(im,(x,y+25));d.text((x+8,y+5),p.name,fill='white')
  dst=out/f'{group}-{i//4:02d}.jpg';sheet.save(dst,quality=94);manifest[dst.as_posix()]=[p.as_posix() for p in chunk]
(out/'CharacterSheetManifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print({k:len(v) for k,v in groups.items()})
