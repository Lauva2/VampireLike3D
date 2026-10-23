from pathlib import Path
from PIL import Image,ImageOps,ImageDraw,ImageFont
import json,hashlib,re,math
root=Path('E:/UnityProject/VampireLike3D'); out=root/'Docs/ArtReviews/Round54/UI54Fresh'; sheets=out/'Sheets'; sheets.mkdir(exist_ok=True)
font=ImageFont.truetype('C:/Windows/Fonts/msyh.ttc',15)
manifest=[]
def sheet(key,files):
    cols=2 if len(files)==5 else 3
    tw,th=640,400
    result=Image.new('RGB',(cols*tw,math.ceil(len(files)/cols)*th),'#1e2427');d=ImageDraw.Draw(result)
    entry={'sheet':str((sheets/(key+'.jpg')).relative_to(root)).replace('\\','/'),'files':[]}
    for n,p in enumerate(files):
        im=Image.open(p).convert('RGB'); im.thumbnail((tw,th-30));x=(n%cols)*tw;y=(n//cols)*th
        result.paste(im,(x+(tw-im.width)//2,y+30));d.text((x+4,y+4),p.name if len(files)==5 else key+'/'+p.name,font=font,fill='white')
        entry['files'].append({'path':str(p.relative_to(root)).replace('\\','/'),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'width':Image.open(p).width,'height':Image.open(p).height})
    result.save(sheets/(key+'.jpg'),quality=94);manifest.append(entry)
groups={}
for p in sorted((root/'Artifacts/UI54/Captures').glob('*.png')):
    key=re.sub(r'-\d+x\d+$','',p.stem);groups.setdefault(key,[]).append(p)
for key,files in groups.items():sheet(key,files)
for folder in sorted((root/'Artifacts/UI54/Sequences').iterdir()):
    files=sorted(folder.glob('*.jpg'))
    if files:sheet(folder.name,files)
(out/'ContactManifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
print(len(groups),'groups',sum(len(v) for v in groups.values()),'PNGs',len(manifest),'sheets')
