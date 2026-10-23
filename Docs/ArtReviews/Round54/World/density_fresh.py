from pathlib import Path
from PIL import Image,ImageDraw
import json
out=Path('Docs/ArtReviews/Round54/World');root=Path('Artifacts/Round54/Captures')
paths=sorted(root.glob('late-density-*.png'));mp=out/'DensitySheetManifest.json';m={}
for i in range(0,len(paths),4):
 chunk=paths[i:i+4];im=Image.new('RGB',(1600,950),(32,35,38));d=ImageDraw.Draw(im)
 for j,p in enumerate(chunk):
  q=Image.open(p);q.thumbnail((800,445));x=j%2*800;y=j//2*475;im.paste(q,(x,y+25));d.text((x+8,y+5),p.name,fill='white')
 dst=out/f'density-{i//4:02d}.jpg';im.save(dst,quality=94);m[dst.as_posix()]=[p.as_posix() for p in chunk]
mp.write_text(json.dumps(m,indent=2))
