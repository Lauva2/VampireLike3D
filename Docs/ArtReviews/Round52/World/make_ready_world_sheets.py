from PIL import Image,ImageDraw
from pathlib import Path
import json
root=Path('Artifacts/Round52/Captures');out=Path('Docs/ArtReviews/Round52/World');mp=out/'WorldSheetManifest.json';manifest=json.loads(mp.read_text(encoding='utf-8-sig')) if mp.exists() else {};groups={}
for i in [0,1,2,3,5,6]:
 names=[f'10-biome-{i}',f'terrain-detail-{i}']+[f'geography-{i}-{j}' for j in range(3)]+[f'geography-wide-{i}-{j}' for j in range(3)]+[f'waterside-architecture-{i}',f'waterside-architecture-wide-{i}',f'sanctuary-{i}',f'sanctuary-close-{i}',f'sanctuary-completed-{i}',f'boss-{i}',f'boss-closeup-{i}']
 paths=[root/f'{n}.png' for n in names]
 if all(p.exists() for p in paths):groups[f'biome{i}']=paths
boundaries=sorted(root.glob('mixed-boundary-*.png'))
mixed=boundaries+[root/f'{s}.png' for s in ['mixed-lava-shore','mixed-lava-shore-close','mixed-region-crystal','mixed-region-desert','mixed-region-forest','mixed-region-snow','mixed-region-swamp','mixed-region-volcano','10-biome-4','terrain-detail-4','world-floating-origin']]
if len(boundaries)>=6 and all(p.exists() for p in mixed):groups['mixed']=mixed
probe=sorted(Path('Artifacts/Probe52/Captures').glob('*.png'))
if len(probe)==7:groups['probe']=probe
exper=sorted(root.glob('experience-*.png'))
if len(exper)==2:groups['experience']=exper
combat=sorted(root.glob('enemy-combat*.png'))
if len(combat)==8:groups['enemycombat']=combat
new=[]
for group,paths in groups.items():
 for i in range(0,len(paths),4):
  dst=out/f'{group}-{i//4:02d}.jpg'
  if dst.as_posix() in manifest:continue
  chunk=paths[i:i+4];sheet=Image.new('RGB',(1600,950),(32,35,38));d=ImageDraw.Draw(sheet)
  for j,p in enumerate(chunk):
   im=Image.open(p);im.thumbnail((800,445));x=(j%2)*800;y=(j//2)*475;sheet.paste(im,(x,y+25));d.text((x+8,y+5),p.name,fill='white')
  sheet.save(dst,quality=94);manifest[dst.as_posix()]=[p.as_posix() for p in chunk];new.append(dst.name)
mp.write_text(json.dumps(manifest,indent=2),encoding='utf-8');print('New ready52 sheets',new)
