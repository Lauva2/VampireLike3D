from pathlib import Path
from PIL import Image, ImageDraw
import json, math
root=Path('E:/UnityProject/VampireLike3D')
out=root/'Docs/ArtReviews/Round47/Combat'
entries=[]
def sheet(name, paths, selected):
    for page in range(math.ceil(len(selected)/16)):
        ids=selected[page*16:(page+1)*16]
        canvas=Image.new('RGB',(1280,4*202),(24,29,33))
        draw=ImageDraw.Draw(canvas)
        for j,idx in enumerate(ids):
            p=paths[idx]; im=Image.open(p); im.thumbnail((320,180))
            x=(j%4)*320;y=(j//4)*202
            canvas.paste(im,(x,y+22));draw.text((x+4,y+4),f'{name} {p.stem}',fill='white')
        target=out/f'{name}-{page:02}.jpg';canvas.save(target,quality=92)
        entries.append({'sheet':str(target.relative_to(root)).replace('\\','/'),'sources':[str(paths[i].relative_to(root)).replace('\\','/') for i in ids]})
seq=root/'Artifacts/Round47/Sequences'
for d in sorted(seq.iterdir()):
    if not d.is_dir():continue
    paths=sorted(d.glob('*.jpg')); n=len(paths)
    if d.name=='late-density-combinations' or d.name=='player-defeat' or d.name.startswith('feedback-'):
        ids=list(range(n))
    elif d.name.startswith('boss-attacks-'):
        ids=sorted(set(list(range(0,n,6))+[n-1]))
    elif d.name.startswith('boss-death-'):
        ids=list(range(n))
    elif d.name.startswith('role-closeup-'):
        ids=sorted(set(round(i*(n-1)/15) for i in range(16)))
    else:
        ids=sorted(set(round(i*(n-1)/11) for i in range(12)))
    sheet(d.name,paths,ids)
paths=sorted((root/'Artifacts/Round47/Captures').glob('late-density-*.png'))
sheet('static-late-density',paths,list(range(len(paths))))
(out/'SheetSourceIndex.json').write_text(json.dumps(entries,indent=2),encoding='utf8')
print(json.dumps({'sheets':len(entries),'sourceFrames':sum(len(e['sources']) for e in entries)},indent=2))
