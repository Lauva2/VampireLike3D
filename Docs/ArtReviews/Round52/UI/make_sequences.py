from PIL import Image,ImageDraw
from pathlib import Path
import json
root=Path('E:/UnityProject/VampireLike3D');out=root/'Docs/ArtReviews/Round52/UI'
manifest=[]
for seq in sorted((root/'Artifacts/Round52/Sequences').glob('feedback-*')):
    fs=sorted(seq.glob('*.jpg')); cols=3;rows=(len(fs)+2)//3
    sheet=Image.new('RGB',(1920,rows*390),'#263339');d=ImageDraw.Draw(sheet)
    for i,f in enumerate(fs):
        im=Image.open(f).convert('RGB');im.thumbnail((632,356));x=i%3*640+(632-im.width)//2;y=i//3*390+28
        sheet.paste(im,(x,y));d.text((i%3*640+5,i//3*390+5),seq.name+'/'+f.name,fill='white')
    target=out/(seq.name+'-all.jpg');sheet.save(target,quality=95)
    manifest.append({'sheet':str(target.relative_to(root)).replace('\\','/'),'sources':[str(f.relative_to(root)).replace('\\','/') for f in fs]})
(out/'SequenceSheetManifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(sum(len(x['sources']) for x in manifest))
