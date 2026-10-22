from PIL import Image,ImageDraw
from pathlib import Path
import json
root=Path('Artifacts/Round51/Captures'); out=Path('Docs/ArtReviews/Round51/World');groups={}
for i in [0,1,2,3,5,6]:
 names=[f'10-biome-{i}',f'terrain-detail-{i}']+[f'geography-{i}-{j}' for j in range(3)]+[f'geography-wide-{i}-{j}' for j in range(3)]+[f'waterside-architecture-{i}',f'waterside-architecture-wide-{i}',f'sanctuary-{i}',f'sanctuary-close-{i}',f'sanctuary-completed-{i}',f'boss-{i}',f'boss-closeup-{i}']
 groups[f'biome{i}']=[root/f'{n}.png' for n in names if (root/f'{n}.png').exists()]
groups['mixed']=sorted(root.glob('mixed*.png'))+[root/'10-biome-4.png',root/'terrain-detail-4.png',root/'world-floating-origin.png']
groups['probe']=sorted(Path('Artifacts/Probe51/Captures').glob('*.png'))
groups['experience']=sorted(root.glob('experience-*.png'))
groups['enemycombat']=sorted(root.glob('enemy-combat*.png'))
manifest={}
for group,paths in groups.items():
 for i in range(0,len(paths),4):
  chunk=paths[i:i+4]; sheet=Image.new('RGB',(1600,950),(32,35,38)); d=ImageDraw.Draw(sheet)
  for j,p in enumerate(chunk):
   im=Image.open(p);im.thumbnail((800,445));x=(j%2)*800;y=(j//2)*475;sheet.paste(im,(x,y+25));d.text((x+8,y+5),p.name,fill='white')
  dst=out/f'{group}-{i//4:02d}.jpg';sheet.save(dst,quality=94);manifest[str(dst).replace('\\','/')]=[str(p).replace('\\','/') for p in chunk]
(out/'WorldSheetManifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8');print(json.dumps({k:len(v) for k,v in groups.items()}))
