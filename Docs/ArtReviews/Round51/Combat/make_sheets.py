from pathlib import Path
from PIL import Image, ImageDraw
import json,math
root=Path('E:/UnityProject/VampireLike3D');out=root/'Docs/ArtReviews/Round51/Combat';out.mkdir(parents=True,exist_ok=True)
entries=[]
def sheet(name,paths,ids):
    for page in range(math.ceil(len(ids)/16)):
        subset=ids[page*16:(page+1)*16];im=Image.new('RGB',(1280,808),(24,29,33));draw=ImageDraw.Draw(im)
        for j,i in enumerate(subset):
            p=paths[i];frame=Image.open(p);frame.thumbnail((320,180));x=j%4*320;y=j//4*202
            im.paste(frame,(x,y+22));draw.text((x+3,y+4),name+' '+p.stem,fill='white')
        target=out/f'{name}-{page:02}.jpg';im.save(target,quality=92)
        entries.append({'sheet':str(target.relative_to(root)).replace('\\','/'),'sources':[str(paths[i].relative_to(root)).replace('\\','/') for i in subset]})
for d in sorted((root/'Artifacts/Round51/Sequences').iterdir()):
    if not d.is_dir() or not (d/'metadata.json').exists():continue
    paths=sorted(d.glob('*.jpg'));n=len(paths);m=json.loads((d/'metadata.json').read_text())
    if n!=m['frames']:continue
    if d.name=='late-density-combinations' or d.name=='player-defeat' or d.name.startswith(('feedback-','boss-death-')):ids=list(range(n))
    elif d.name.startswith(('boss-attacks-','enemy-attacks-')):ids=sorted(set(list(range(0,n,6))+[n-1]))
    elif d.name.startswith('role-closeup-'):ids=sorted(set(round(i*(n-1)/15) for i in range(16)))
    else:ids=sorted(set(round(i*(n-1)/11) for i in range(12)))
    sheet(d.name,paths,ids)
for prefix in ['late-density-','weapon-']:
    paths=sorted((root/'Artifacts/Round51/Captures').glob(prefix+'*.png'))
    if paths:sheet('static-'+prefix.strip('-'),paths,list(range(len(paths))))
(out/'SheetSourceIndex.json').write_text(json.dumps(entries,indent=2),encoding='utf8');print('prepared',len(entries),'sheets')
