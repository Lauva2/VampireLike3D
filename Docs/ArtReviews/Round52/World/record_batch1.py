import json,hashlib
from pathlib import Path
from PIL import Image
from datetime import datetime,timezone
out=Path('Docs/ArtReviews/Round52/World');m=json.loads((out/'CharacterSheetManifest.json').read_text(encoding='utf-8-sig'))
originals=['character-adventurer','character-elementalist','character-swordsman','enemy-1-normal','enemy-5-elite','enemy-6-normal','enemy-7-elite','enemy-8-normal','weapon-crossbow','weapon-boomerang','weapon-froststaff']
raw=[]
for path in sorted(set(x for v in m.values() for x in v)):
 p=Path(path);wp=p.with_suffix('.world.json');qp=p.with_suffix('.quality.json');world=json.loads(wp.read_text(encoding='utf-8-sig')) if wp.exists() else None
 raw.append({'path':path,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'pixels':list(Image.open(p).size),'quality':json.loads(qp.read_text(encoding='utf-8-sig')) if qp.exists() else None,'worldSidecar':wp.as_posix() if wp.exists() else None,'world':world,'seed':world.get('seed') if world else None,'seedUnknownReason':None if world else 'No world sidecar; do not infer from other captures.'})
d={'candidate':'Round52','version':'1.2.0','reviewer':'/root/art_lead47','status':'partial ongoing observation; no complete score or gate conclusion','recordedAt':datetime.now(timezone.utc).isoformat(),'actualViewing':{'sheets':list(m),'staticImages':[x['path'] for x in raw],'originalFiles':['Artifacts/Round52/Captures/'+n+'.png' for n in originals],'sheetCount':len(m),'staticCount':len(raw),'originalCount':len(originals),'dynamicFramesPersonallyViewed':0},'sourceMetadata':raw,'batches':[{'id':1,'scope':'Three characters,8normal/8elite,three new weapon closeups','newSheets':6,'newStatic':22,'newOriginal':11,'notes':'Docs/ArtReviews/Round52/World/BatchNotes.md'}],'limitations':['Only current52 raw images actually inspected; no51 image/score/count migrated.','Full52 still running, world/Mixed/Probe and final motion completeness not yet viewed. No core100-point result.','Full-frame contact sheets are reduced indexes; selected original files additionally opened.','No desktop control, Player launch, build, runtime edit or original-image modification.','Deep fixed build and actual7200sec final frame pending, not a performance pass.']}
(out/'ObservedEvidence.json').write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
(out/'BatchNotes.md').write_text('''# Round52 角色与世界实际观察批记

只使用52原始Camera.Render；不将51图片、观看计数或分数迁入本轮。原100分20项/原10门槛保留，新增六单群系/Mixed、六Boss、八普通/八精英、九武器全部归原项。本文仅记已经看的范围，尚未形成完整A/B/C或总分。

## 批次1：角色、八敌种及精英、三新武器

实际看characters-00/enemies-00–03/weapons-00共6张完整画幅辅助表、22不同静态；另开三角色、enemy-1-normal/5-elite/6-normal/7-elite/8-normal、三武器共11张原图。逐文件SHA/像素/quality/world/seed在ObservedEvidence.json，表源映射CharacterSheetManifest.json。

三角色头大短肢、鞋手和发帽轮廓分离，冒险者橙围巾/青短衣/枪、元素学徒紫尖帽与白发/金边袍/杖、御剑者深蓝发/绿长衣/背剑可分。原图完整看到手持灰枪、法杖和背剑柄，没有粉材或明显缺件。冒险者纵长大眼仍夸张，手指/肘部与衣褶处理简洁；元素学徒本帧闭眼，静态不将闭眼视为缺眼模型。

八敌种以绿团、黄长耳、蓝角壳、紫菌伞、竖长紫孢囊、低伏巨眼跃蛙、黑足晶甲和高赤晶精灵区分。金冠是共同精英识别，额外金角/侧晶/肩块/菌朵补体量；近景精英与普通能分，但八类仍高度依赖共用金冠，精英局部盔甲/姿态可加强，不能只因颜色区分给满。孢囊与菌伞都用紫绿，但上下体比例与宽伞/竖囊轮廓不同；晶甲与晶灵一个低足一个竖身，正常镜头还需后续实看。

连弩棕体/金横弓/白弦箭与青色上盒、回旋刃青弯体/乳白端/小金片、霜杖蓝长棱晶/银托/深杆三金环各有形色。上盒/接合与银托偏简，后续正常运行效果/联动由Combat实看；单近景不代表全部九能力在运行，也不代替动态命中/状态。

Full52仍在采集，六群系/六Boss/地标回访/Mixed/Probe未在本批观察，所有全局结论保留未验证。没有改runtime或启动Player/操作桌面。
''',encoding='utf-8')
print('Batch1 recorded',len(raw),'statics',len(originals),'originals')
