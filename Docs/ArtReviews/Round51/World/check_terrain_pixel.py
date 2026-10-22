from PIL import Image
from pathlib import Path
import hashlib,json
p=Path('Artifacts/Round51/Captures/mixed-region-snow.png');im=Image.open(p).convert('RGB');target=(220,232,207);matches=[]
for y in range(im.height):
 for x in range(im.width):
  if im.getpixel((x,y))==target:matches.append([x,y])
box=[[[x,y],list(im.getpixel((x,y)))] for y in range(738,743) for x in range(1319,1324)]
o=dict(image=str(p).replace('\\','/'),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),imageOpenedInFull=True,matchingRGB=list(target),allExactRGBMatches=matches,neighborhood5x5=box,observation='Original1600x900 full-frame visible snow/transition ground is continuous. The reported pixel is at a pale-gold projectile edge near bottom-right. No visible contiguous opening/rectangular missing chunk or broken terrain edge was observed. A single exact RGB match cannot independently establish a real terrain hole.',inference='Antialias/interpolated edge color coincidence is plausible but not proven by a static image.',scope='Visual inspection plus exact pixel read, not exhaustive geometry/rasterization proof. Existing TerrainCoverage51Audit.json failure preserved without changing pixels or its counts.')
Path('Docs/ArtReviews/Round51/World/TerrainPixelReview.json').write_text(json.dumps(o,ensure_ascii=False,indent=2),encoding='utf-8')
p=Path('Docs/ArtReviews/Round51/World/Review.json');r=json.loads(p.read_text(encoding='utf-8'));r['actualViewing']['originalFiles'].append(o['image']);r['actualViewing']['originalCount']+=1;r['terrainPixelReview']='Docs/ArtReviews/Round51/World/TerrainPixelReview.json';r['limitations'].append('mixed-region-snow original has one exact CameraClear RGB pixel at1321,740; full-frame visible ground is continuous. Color coincidence plausible, not proven; original heuristic failure retained.');p.write_text(json.dumps(r,ensure_ascii=False,indent=2),encoding='utf-8')
print('ExactRGB matches:',matches)
