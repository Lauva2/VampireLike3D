from pathlib import Path
from PIL import Image
import json, hashlib, csv
root=Path('E:/UnityProject/VampireLike3D'); out=root/'Docs/ArtReviews/Round52/UI'
def read(p): return json.loads((root/p).read_text(encoding='utf-8-sig'))
def write(p,v): (out/p).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf-8')
def item(p):
    f=root/p
    with Image.open(f) as im: wh=list(im.size)
    return {'path':p,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'pixels':wh}
man=read('Docs/ArtReviews/Round52/UI/SheetManifest.json'); seqman=read('Docs/ArtReviews/Round52/UI/SequenceSheetManifest.json')
static=[f for m in man for f in m['sources']]
originals=[f for f in static if f.endswith('-1280x720.png')]+['Artifacts/UI52/Captures/'+n+'.png' for n in ['ui-upgrade-1920x1200','ui-upgrade-1920x1080','ui-selection-expansion-locked-2560x1080','ui-skills-expansion-联动-locked-1920x1200']]
extra=['Artifacts/Round52/Captures/'+n+'.png' for n in ['20-skills','22-upgrade','death-results']]+['Artifacts/Probe52/Captures/load-failure-'+n+'.png' for n in ['terrain-forest','weapon-gun']]
static+=extra; originals+=extra
seqOriginals=['Artifacts/Round52/Sequences/'+n+'.jpg' for n in ['feedback-pause-open/0001','feedback-pause-open/0002','feedback-skills-open/0001','feedback-upgrade-open/0001','feedback-upgrade-open/0002','feedback-upgrade-reward/0000','feedback-task-unlock/0000','feedback-task-unlock/0005']]
sequences={}
for m in seqman:
    p=Path(m['sources'][0]).parent
    sequences[p.name]={'framesObserved':list(range(len(m['sources']))),'sources':[item(f) for f in m['sources']],'metadata':read(str(p/'metadata.json')),'timestamps':list(csv.DictReader((root/p/'timestamps.csv').open())),'allFramesInThisSequenceObserved':True}
reports={}
for n in ['UI52','Round52','Probe52']:
    v=read('Artifacts/'+n+'/Verification.json');b=read('Artifacts/'+n+'/BuildIdentity.json');e=read('Artifacts/'+n+'/ProcessExit.json')
    reports[n]={'finished':v['finished'],'passed':v['passed'],'checkCount':len(v['checks']),'errors':v['errors'],'buildFileCount':len(b['files']),'player':b['player'],'exitCode':e['exitCode']}
pairs=lambda a:sorted((x['file'],x['sha256']) for x in a)
base=read('Artifacts/UI52/BuildIdentity.json')['files']
identity={'buildEntryCount':len(base),'allBuildEntriesMatch':all(pairs(base)==pairs(read('Artifacts/'+n+'/BuildIdentity.json')['files']) for n in ['Round52','Probe52']),'releaseBridge':read('Artifacts/BuildMatch52.json'),'workspace':read('Artifacts/Workspace52Identity.json'),'releaseAuditPassed':read('Artifacts/ReleaseAudit52.json')['passed']}
world={}
for p in ['Artifacts/UI52/Captures/ui-hud-1280x720.world.json','Artifacts/Round52/Captures/20-skills.world.json']:
    j=read(p);world[p]={k:j.get(k) for k in ['seed','character','abilities']}
limits=['Only candidate52 images and observations; no prior scores or viewed counts migrate.','All180 UI source frames observed through36 full-frame five-aspect contact sheets, with all36 720p originals separately opened plus4 higher-resolution originals and5 additional static originals. Large source transport may resize to2048px.','All32 original frames in five feedback sequences observed through five complete contact sheets, with8 source originals separately opened. This is not normal-speed live play or original-size review of every dynamic frame.','Actual wall-clock timestamps are included. fixedSimulationFps=null; capture frequency is not performance FPS.','Shrine-specific full open/cancel/reopen/choose/return-to-normal-upgrade continuous sequence and weapon-HUD continuous scrolling remain unobserved. Functional assertions and static pairs do not replace those dynamic observations.','Five new tasks and four expanded skill categories including three linkages were individually read; not all60 upgrades/25 tasks in every scroll position/state were exhaustively viewed.','Skill lock fixtures temporarily alter task completion visibility. Nine acquired weapons remain; these screenshots are not dense-combat capability-removal evidence. Crossbow in projectile-locked is already acquired, same as unlocked, and is not claimed as an observed locked weapon.','A/B/C/E/F1 and complete world F2, damage timing, natural difficulty, performance and long-run stability are outside this review.','No desktop control, Player launch, build, runtime edit or original-image modification.']
obs={'candidate':'Round52/UI52/Probe52','version':'1.2.0','reviewer':'/root/art_ui47','status':'complete_for_assigned_scope','rubric':'Docs/ArtReview.md original sections1–6;100 points20 items;10 gates unchanged','observedSheets':[m['sheet'] for m in man],'observedSequenceSheets':[m['sheet'] for m in seqman],'staticSourcesObserved':[item(p) for p in static],'observedOriginals':[item(p) for p in originals],'observedSequenceOriginals':[item(p) for p in seqOriginals],'sequenceFrames':sequences,'counts':{'staticDifferent':len(set(static)),'staticOpenedAsSource':len(set(originals)),'dynamicDifferent':sum(len(v['sources']) for v in sequences.values()),'dynamicOpenedAsSource':len(seqOriginals),'auxiliarySheets':len(man)+len(seqman)},'runtimeReports':reports,'identity':identity,'worldContext':world,'quality':read('Artifacts/UI52/Captures/ui-hud-1280x720.quality.json'),'limits':limits}
write('ObservedEvidence.json',obs)
rows=[
 {'id':'D1','score':5,'max':5,'title':'菜单/选择/任务/成长/设置/加载/结算体系','evidence':['SheetManifest.json:36类×五画幅180图','Artifacts/Probe52/Captures/load-failure-terrain-forest.png','Artifacts/Probe52/Captures/load-failure-weapon-gun.png','Artifacts/Round52/Captures/death-results.png'],'observation':'本轮180图及错误恢复/死亡原图均独立看过，奶油底、青色主操作、彩色图标和白/灰卡统一；三角色七地图、新任务/技能与六祝福页面完整纳入。加载失败Toast说明返回主菜单，启程主按钮保持可读。','revision':'无本项必修；错误Toast可继续细化，当前暂遮次操作但不阻断主操作。'},
 {'id':'D2','score':5,'max':5,'title':'HUD/候选/技能层级与密度','evidence':['Artifacts/UI52/Captures/ui-hud-1280x720.png','Artifacts/UI52/Captures/ui-hud-expansion-1280x720.png','Artifacts/UI52/Captures/ui-blessing-owned-1280x720.png','Artifacts/UI52/Captures/ui-skills-expansion-联动-locked-1280x720.png','Artifacts/Round52/Captures/20-skills.png'],'observation':'能力9摘要和横向栏初/后段同时实看，生命/计时/资源/地图层级清楚，滚轮/拖动提示可见。普通四选、祭坛三选、层数变化、任务进度奖励及构筑价格/任务锁定/已掌握均分层。已获祝福区本轮出现×1/×2/×4并说明本局生效。','revision':'无本项必修；持续保留能力总数及滚动引导。'},
 {'id':'D3','score':4,'max':5,'title':'中文完整/长文本/字号/行距/对齐','evidence':['Artifacts/UI52/Captures/ui-upgrade-1920x1200.png','Artifacts/UI52/Captures/ui-upgrade-1920x1080.png','Artifacts/UI52/Captures/ui-task-expansion-froststaff.shatter-1280x720.png','Artifacts/UI52/Captures/ui-skills-expansion-联动-locked-1920x1200.png'],'observation':'本轮稳定五画幅未见缺字方框/关键裁切。扣1为原图1920×1200普通升级碎霜领域描述仅句号独占第二行；1920×1080普通升级剑气前行至“范围剑”，末行仅“气。”。两处均本轮源图独立确认；生机短句在720p完整可读。','revision':'缩短或重排碎霜领域/剑气描述，避免孤标点和单字末行；实际五画幅回归，不只依赖text fits断言。'},
 {'id':'D4','score':5,'max':5,'title':'五画幅/状态/价格/锁定原因/图标','evidence':['SheetManifest.json:180图','Artifacts/UI52/Captures/ui-selection-expansion-locked-2560x1080.png','Artifacts/UI52/Captures/ui-skills-expansion-联动-locked-1280x720.png','Artifacts/UI52/Captures/ui-skills-expansion-联动-unlocked-1280x720.png','Artifacts/UI52/Captures/ui-blessing-variety-3-1280x720.png'],'observation':'720p、1080p、16:10、21:9、1440p核心面板和控件未见出界/拉伸。新地图锁标/灰卡/条件与开放白卡明确；两武器锁因和三联动解锁后4点价格可辨。祭坛0→1和1→2、取消保留、已获本局生效均完整。弹道locked中连弩已掌握，如实作为已掌握状态。','revision':'无本项必修；锁态夹具与真实已获状态需继续清楚区分。'},
 {'id':'F2','score':4,'max':5,'scope':'page-feedback portion recommendation only','title':'页面动效/升级及解锁反馈（页面部分）','evidence':['SequenceSheetManifest.json:五序列32帧','Artifacts/Round52/Sequences/feedback-pause-open/0001.jpg','Artifacts/Round52/Sequences/feedback-pause-open/0002.jpg','Artifacts/Round52/Sequences/feedback-upgrade-open/0001.jpg','Artifacts/Round52/Sequences/feedback-upgrade-open/0002.jpg','Artifacts/Round52/Sequences/feedback-upgrade-reward/0000.jpg','Artifacts/Round52/Sequences/feedback-task-unlock/0000.jpg'],'observation':'本轮五反馈32帧从开始至末帧均看，8原帧补看；暂停/构筑/普通升级早期透明后稳定，获得叠阵及任务完成/新内容解锁/晶石+15在左下避开中心角色与主操作，未见持续闪烁/材质跳变。反馈仍主要是短淡入、通用Toast和环形变化，节奏与奖励类别差异可进一步细化，页面部分建议4。','revision':'细化不同选择确认/奖励及层数增加的视觉节奏；祭坛完整转场和HUD连续滚动待专属连续证据。完整F2交总评结合世界反馈确定。'}]
write('UIReviewRows.json',rows)
print(obs['counts']);print(reports);print(world);print(identity['allBuildEntriesMatch'])
