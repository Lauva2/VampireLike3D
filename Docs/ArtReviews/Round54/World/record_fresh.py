import json,hashlib
from pathlib import Path
from PIL import Image
out=Path('Docs/ArtReviews/Round54')
record={'candidate':'Round54','reviewer':'/root/popup54_world','rubric':'Docs/ArtReview.md §3–4 original 100 points / 20 items / 10 gates','status':'in progress; first batch actually viewed','sheetsViewed':[],'originalFilesViewed':['Artifacts/Round54/Captures/character-adventurer.png'],'batches':[{'id':1,'observations':'Actually viewed all three character closeups, 16 normal/elite closeups and three extra weapon closeups via six full-frame sheets plus adventurer original. Clear large heads/short limbs and coordinated warm/teal materials; distinct hat/sword/gun identities. Hand/grip geometry remains simplified. Slime/rabbit/beetle/mage and swamp/crystal silhouettes are distinct; elites show crowns plus shape additions. No missing/pink material in these views.'}],'limits':['No52/53 scores or image observations reused.','No desktop, emulator, Player launch/build or source-image modification.','Performance deferred by user.']}
manifest=json.loads((out/'World/CharacterSheetManifest.json').read_text())
record['sheetsViewed']=list(manifest)
record['staticImagesViewed']=sorted(set(sum(manifest.values(),[])+record['originalFilesViewed']))
def fileinfo(p):
 q=Path(p)
 with Image.open(q) as im: size=list(im.size)
 return {'path':p,'sha256':hashlib.sha256(q.read_bytes()).hexdigest().upper(),'size':size}
record['sourceFiles']=[fileinfo(p) for p in record['staticImagesViewed']]
(out/'World54FreshObserved.json').write_text(json.dumps(record,ensure_ascii=False,indent=2),encoding='utf-8')
