import json,hashlib
from pathlib import Path
from PIL import Image
out=Path('Docs/ArtReviews/Round54');d=json.loads((out/'World54FreshObserved.json').read_text(encoding='utf-8'))
m={}
for n in ['CharacterSheetManifest.json','WorldSheetManifest.json','DensitySheetManifest.json']: m.update(json.loads((out/'World'/n).read_text(encoding='utf-8-sig')))
d['sheetsViewed']=list(m)
d['originalFilesViewed']=['Artifacts/Round54/Captures/'+n+'.png' for n in ['character-adventurer','character-elementalist','character-swordsman','geography-wide-5-2','waterside-architecture-wide-6','sanctuary-close-3','mixed-boundary-1-5','boss-closeup-5','boss-closeup-6','waterside-architecture-wide-0','waterside-architecture-wide-1','waterside-architecture-wide-2','waterside-architecture-wide-3','geography-wide-3-2','mixed-lava-shore-close','enemy-8-elite','10-biome-0','geography-wide-6-2','boss-closeup-0','boss-closeup-1','boss-closeup-2','boss-closeup-3']]+['Artifacts/Probe54/Captures/probe-max-zoom-ultrawide.png']
d['staticImagesViewed']=sorted(set(sum(m.values(),[])+d['originalFilesViewed']))
d['sheetManifest']=m
d['batches'] += [{'id':2,'observations':'Actually viewed24 biome sheets: six standalone maps each normal/detail, three geography positions at normal/wide zoom, architecture normal/wide, challenge normal/close/completed, Boss normal/close. River/pond/ice/lava, canopy/dune/ice ridge/rock pillar and mushroom/crystal languages are distinct. Side paths and stepping stones leave challenge centers clear; completed captures remove activation text. Dry foundations connect to paths; crystal architecture frame contains no water, so its filename alone is not evidence of waterside placement. Large flat ground and repeated small vegetation/rock/mushroom assemblies remain visible, especially geography-wide-5-2 and geography-wide-6-2.'},{'id':3,'observations':'Actually viewed six Mixed boundary pairs (0-1,0-3,0-6,1-2,1-5,3-5), all six Mixed regions, lava end/bridge and floating-origin captures;7 Probe captures and2 experience quality captures;8 new-enemy combat captures. Blended transitions retain both biome prop languages, paths and rounded lava shoreline end are clear; no world hole/clear background or obvious material failure in observed frames. Probe maximum ultrawide personally opened; original2560x1080 was tool-resized to2048x864, full frame retained.'},{'id':4,'observations':'Actually viewed seven late-density static captures via two sheets. Player head/teal foot ring can be found amid enemies; gold crowns and dense silhouettes crowd edges. No continuous motion claim from these static captures. Also personally opened23 original PNG files, all six Boss closeups and all three characters included; none counted twice.'}]
d['status']='A/B/C fresh visual review complete; B3 full-action contribution to be joined from combat reviewer'
d['counts']={'sheets':len(m),'differentStaticImages':len(d['staticImagesViewed']),'originalFiles':len(d['originalFilesViewed']),'dynamicFramesPersonallyViewed':0}
d['scores']={'A1':5,'A2':4,'A3':5,'B1':4,'B2':5,'B3':5,'C1':5,'C2':4,'C3':4,'C4':5}
d['dimensionTotals']={'A':14,'B':14,'C':18}
d['sourceFiles']=[]
for p in d['staticImagesViewed']:
 q=Path(p)
 with Image.open(q) as im: size=list(im.size)
 e={'path':p,'sha256':hashlib.sha256(q.read_bytes()).hexdigest().upper(),'size':size}
 for suffix in ['quality','world','model','placement']:
  s=q.with_suffix('.'+suffix+'.json')
  if s.exists():
   z=json.loads(s.read_text(encoding='utf-8-sig'));e[suffix+'File']=s.as_posix();e[suffix+'Sha256']=hashlib.sha256(s.read_bytes()).hexdigest().upper()
   if suffix=='quality':e['quality']=z
   if suffix=='world':e['world']={k:z[k] for k in ['seed','character','abilities','seconds','global','current','origin','zoom'] if k in z}
 d['sourceFiles'].append(e)
d['metadataNote']='World metadata fields are retained only when actually supplied; character closeups have quality but no world sidecar. No invented seed/time/abilities. Auxiliary sheets preserve entire frame at reduced size, originals unchanged.'
(out/'World54FreshObserved.json').write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
print(d['counts'])
