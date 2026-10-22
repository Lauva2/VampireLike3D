from PIL import Image,ImageDraw
from pathlib import Path
import json
root=Path('Artifacts/Round47/Captures')
sets={'characters':[p for p in root.glob('character-*.png')], 'enemies':sorted(root.glob('enemy-*.png')), 'bosses':sorted(root.glob('boss*.png')), 'biomes':sorted(root.glob('10-biome*.png'))+sorted(root.glob('terrain-detail*.png')), 'geography':sorted(root.glob('geography-*-*.png')), 'geography-wide':sorted(root.glob('geography-wide*.png')), 'waterside':sorted(root.glob('waterside*.png')), 'sanctuary':sorted(root.glob('sanctuary*.png')), 'mixed':sorted(root.glob('mixed*.png'))+[root/'world-floating-origin.png'], 'probe':sorted(Path('Artifacts/Probe47/Captures').glob('*.png'))}
manifest={}
for group,paths in sets.items():
 for i in range(0,len(paths),4):
  chunk=paths[i:i+4]; sheet=Image.new('RGB',(1600,950),(32,35,38)); d=ImageDraw.Draw(sheet)
  for j,p in enumerate(chunk):
   im=Image.open(p);im.thumbnail((800,445));x=(j%2)*800;y=(j//2)*475;sheet.paste(im,(x,y+25));d.text((x+8,y+5),p.name,fill='white')
  dst=Path('Docs/ArtReviews/Round47/World')/f'{group}-{i//4:02d}.jpg';sheet.save(dst,quality=94);manifest[str(dst).replace('\\','/')]=[str(p).replace('\\','/') for p in chunk]
Path('Docs/ArtReviews/Round47/World/SheetManifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps(manifest,indent=2))
