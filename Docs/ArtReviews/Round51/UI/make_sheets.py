from PIL import Image,ImageDraw
from pathlib import Path
import json
root=Path('E:/UnityProject/VampireLike3D')
out=root/'Docs/ArtReviews/Round51/UI'
files=sorted((root/'Artifacts/UI51/Captures').glob('*.png'))
groups={}
for f in files:
    typ=f.stem.rsplit('-',1)[0]
    groups.setdefault(typ,[]).append(f)
manifest=[]
for typ,fs in groups.items():
    sheet=Image.new('RGB',(1920,860),'#263339'); d=ImageDraw.Draw(sheet)
    for i,f in enumerate(fs):
        im=Image.open(f).convert('RGB'); im.thumbnail((632,380))
        x=(i%3)*640+(632-im.width)//2; y=(i//3)*430+30
        sheet.paste(im,(x,y));d.text(((i%3)*640+5,(i//3)*430+5),f.name,fill='white')
    dest=out/(typ+'-matrix.jpg');sheet.save(dest,quality=94)
    manifest.append({'sheet':str(dest.relative_to(root)).replace('\\','/'),'sources':[str(f.relative_to(root)).replace('\\','/') for f in fs]})
(out/'SheetManifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
(out/'ObservedEvidence.json').write_text(json.dumps({'candidate':'Round51/UI51','reviewer':'/root/art_ui47','status':'in_progress','sheetManifest':'SheetManifest.json','observedSheets':[],'observedOriginals':[],'sequenceFrames':{},'limits':['Only observed entries count; auxiliary sheets do not modify originals.']},indent=2),encoding='utf-8')
print(len(files),len(groups))
