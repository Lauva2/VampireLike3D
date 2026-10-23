from PIL import Image,ImageOps,ImageDraw
from pathlib import Path
import json, hashlib
root=Path('E:/UnityProject/VampireLike3D'); out=root/'Docs/ArtReviews/Round54/Combat54Fresh'; sheets=out/'Sheets'; sheets.mkdir(exist_ok=True)
records=[]; cards=[]
for source in ['Round54','UI54']:
 for d in sorted((root/'Artifacts'/source/'Sequences').iterdir()):
  if not d.is_dir(): continue
  fs=sorted(d.glob('*.jpg')); n=len(fs)
  if not n: continue
  if source=='UI54' or 'death' in d.name or d.name in ['player-defeat','late-density-combinations'] or d.name.startswith('feedback-') or d.name=='popup-blessing': ids=list(range(n))
  elif d.name.startswith('effect-'): ids=sorted(set([0,n//4,n//2,3*n//4,n-1]))
  else: ids=sorted(set(round(i*(n-1)/11) for i in range(12)))
  rec={'source':source,'sequence':d.name,'availableFrames':n,'sampleIndices':ids,'frames':[]}
  for i in ids:
   p=fs[i]; rec['frames'].append({'index':i,'path':p.relative_to(root).as_posix(),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}); cards.append((p,f'{source}/{d.name}/{p.name}',rec))
  records.append(rec)
# Separate each sequence into pages; keeps action order obvious.
manifest=[]
for rec in records:
 entries=rec['frames']
 for start in range(0,len(entries),12):
  chunk=entries[start:start+12]; canvas=Image.new('RGB',(1920,1164),'#222222'); draw=ImageDraw.Draw(canvas)
  for j,e in enumerate(chunk):
   im=Image.open(root/e['path']).convert('RGB'); im.thumbnail((480,270)); x=(j%4)*480;y=(j//4)*388
   canvas.paste(im,(x,y+28)); draw.text((x+5,y+5),f"{rec['source']}/{rec['sequence']} {e['index']:04d}",fill='white')
  p=sheets/f"{rec['source']}_{rec['sequence']}_{start//12:02}.jpg"; canvas.save(p,quality=90)
  manifest.append({'path':p.relative_to(root).as_posix(),'source':rec['source'],'sequence':rec['sequence'],'indices':[e['index'] for e in chunk]})
(out/'PreparedEvidence.json').write_text(json.dumps({'records':records,'sheets':manifest},ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'sequenceCount':len(records),'frameSamples':sum(len(x['frames']) for x in records),'sheets':len(manifest),'sequences':[{k:v for k,v in r.items() if k!='frames'} for r in records]},indent=2))
