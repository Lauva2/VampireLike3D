"""Reviewer-only full-frame contact sheets; source evidence is never modified."""
from pathlib import Path
from PIL import Image, ImageOps, ImageDraw
import json, math, sys

root = Path(__file__).resolve().parents[2]
candidate = sys.argv[1]
num = candidate.removeprefix('Round')
dest = root / 'Docs' / 'ArtReviews' / candidate
dest.mkdir(parents=True, exist_ok=True)
manifest = []

def pages(label, entries, size=(600,338), cols=3, rows=2):
    w,h = size
    for page in range(math.ceil(len(entries)/(cols*rows))):
        group = entries[page*cols*rows:(page+1)*cols*rows]
        used_rows = math.ceil(len(group)/cols)
        out = Image.new('RGB', (w*cols,(h+28)*used_rows),(24,24,24))
        draw = ImageDraw.Draw(out)
        for i,p in enumerate(group):
            im = Image.open(p).convert('RGB')
            im.thumbnail((w,h))
            x,y = (i%cols)*w,(i//cols)*(h+28)
            out.paste(im,(x+(w-im.width)//2,y+(h-im.height)//2))
            draw.text((x+4,y+h+4),p.name,fill='white')
        fn = f'{label}-{page:02}.png'
        out.save(dest/fn)
        manifest.append({'sheet':fn,'sources':[str(p.relative_to(root)).replace('\\','/') for p in group]})

for group in (candidate,'UI'+num,'Probe'+num):
    pages(group, sorted((root/'Artifacts'/group/'Captures').glob('*.png')))

sampleinfo=[]
for seq in sorted((root/'Artifacts'/candidate/'Sequences').iterdir()):
    if not seq.is_dir(): continue
    frames=sorted([*seq.glob('*.jpg'),*seq.glob('*.png')])
    if not frames: continue
    full=seq.name.startswith(('role-closeup','feedback-')) or seq.name=='late-density-combinations'
    ids=list(range(len(frames))) if full else sorted(set(round(i*(len(frames)-1)/11) for i in range(min(12,len(frames)))))
    meta=json.loads((seq/'metadata.json').read_text(encoding='utf-8-sig')) if (seq/'metadata.json').exists() else {}
    sampleinfo.append({'sequence':seq.name,'totalFrames':len(frames),'reviewedFrames':[frames[i].name for i in ids],'metadata':meta})
    pages(seq.name,[frames[i] for i in ids],(400,225),4,6)
(dest/'ReviewSampling.json').write_text(json.dumps(sampleinfo,indent=2),encoding='utf-8')
(dest/'SheetManifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps({'sheets':len(manifest),'staticImages':sum(len(x['sources']) for x in manifest if '/Captures/' in x['sources'][0]),'sequenceSamples':sum(len(x['reviewedFrames']) for x in sampleinfo),'sequenceCount':len(sampleinfo)}))
