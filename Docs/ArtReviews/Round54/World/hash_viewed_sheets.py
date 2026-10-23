from pathlib import Path
from PIL import Image
import json,hashlib
p=Path('Docs/ArtReviews/Round54/World54FreshObserved.json');d=json.loads(p.read_text(encoding='utf-8'));d['reviewDate']='2026-10-23';d['sheetFiles']=[]
for path in d['sheetsViewed']:
 q=Path(path)
 with Image.open(q) as im:size=list(im.size)
 d['sheetFiles'].append({'path':path,'sha256':hashlib.sha256(q.read_bytes()).hexdigest().upper(),'size':size,'viewed':True,'sourcePaths':d['sheetManifest'][path]})
p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
