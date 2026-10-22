from pathlib import Path
from PIL import Image
import json,hashlib,csv
root=Path('E:/UnityProject/VampireLike3D'); out=root/'Docs/ArtReviews/Round47/UI'
read=lambda p:json.loads((root/p).read_text(encoding='utf-8-sig'))
man=read('Docs/ArtReviews/Round47/UI/SheetManifest.json'); seqman=read('Docs/ArtReviews/Round47/UI/SequenceSheetManifest.json')
static=[f for g in man for f in g['sources']]
orig=[f for f in static if f.endswith('-1280x720.png')]
orig += ['Artifacts/UI47/Captures/'+n+'.png' for n in ['ui-blessing-1920x1080','ui-blessing-variety-0-1920x1200','ui-blessing-variety-1-2560x1080','ui-blessing-owned-2560x1440','ui-upgrade-1920x1200']]
extra=['Artifacts/Round47/Captures/'+n+'.png' for n in ['20-skills','resolution-skills-1280x720','22-upgrade','death-results','ui-blessing-variety-5-1280x720']]+['Artifacts/Probe47/Captures/load-failure-'+n+'.png' for n in ['terrain-forest','weapon-gun']]
orig+=extra; static+=extra
seqorig=['Artifacts/Round47/Sequences/'+f+'.jpg' for f in ['feedback-pause-open/0001','feedback-pause-open/0002','feedback-skills-open/0002','feedback-upgrade-open/0002','feedback-task-unlock/0000','feedback-task-unlock/0009','feedback-upgrade-reward/0000','feedback-upgrade-reward/0007']]
def item(p):
    f=root/p
    with Image.open(f) as im: wh=list(im.size)
    return {'path':p,'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'pixels':wh}
seq={}
for m in seqman:
    name=Path(m['sources'][0]).parent.name
    prefix=Path(m['sources'][0]).parent
    rows=list(csv.DictReader((root/prefix/'timestamps.csv').open()))
    seq[name]={'framesObserved':list(range(len(m['sources']))),'sources':[item(f) for f in m['sources']],'metadata':read(str(prefix/'metadata.json')),'timestamps':rows,'allFramesInThisSequenceObserved':True}
versions={}
for name in ['UI47','Round47','Probe47']:
    v=read('Artifacts/'+name+'/Verification.json'); b=read('Artifacts/'+name+'/BuildIdentity.json')
    versions[name]={'finished':v['finished'],'passed':v['passed'],'checkCount':len(v['checks']),'errors':v['errors'],'buildFileCount':len(b['files']),'player':b['player'],'exitCode':read('Artifacts/'+name+'/ProcessExit.json')['exitCode']}
base=read('Artifacts/UI47/BuildIdentity.json')['files']; pairs=lambda a:sorted((x['file'],x['sha256']) for x in a)
identity={'all23BuildEntriesMatch': all(pairs(base)==pairs(read('Artifacts/'+n+'/BuildIdentity.json')['files']) for n in ['Round47','Probe47']),'releaseBridge':read('Artifacts/Release47BuildMatch.json'),'workspace':read('Artifacts/Workspace47Identity.json')}
limits=['Only Round47/UI47/Probe47 runtime Camera.Render sources are used; no old images or historical scores migrate.','18 UI sheets cover all 90 matrix sources; 30 static source files and 8 dynamic source files additionally opened. Large source files may be downscaled by display transport to 2048px.','All 37 frames in five feedback sequences were visually observed in full-frame auxiliary sheets; this does not mean original-size opening of every frame or normal-speed live play.','Feedback sequences use wall-clock timestamps with fixedSimulationFps=null, requested interval 0.03333; actual last timestamps 0.4480–0.5931 seconds.','No shrine-specific continuous open/choose/return-to-normal-upgrade sequence was present among the five feedback directories; shrine animations and the complete new transition remain unverified here. Stable states and functional assertions do not substitute for continuous visuals.','Not a full 100-point review; A/B/C/E/F1, full-world F2, natural progression, damage timing, performance and 7200-second stability remain outside this assignment.','No desktop/window/input automation, Player launches, builds, original edits or gameplay code changes were performed.']
obs={'candidate':'Round47/UI47/Probe47','version':'1.1.0','reviewer':'/root/art_ui47','status':'complete_for_assigned_scope','rubric':'Docs/ArtReview.md sections 1–6 original 20-item 100-point rubric','documentsRead':['AGENTS.md','Docs/GameDesign.md including 1.1','Docs/ArtReview.md sections 1–6'],'observedSheets':[x['sheet'] for x in man],'observedSequenceSheets':[x['sheet'] for x in seqman],'staticSourcesObserved':[item(f) for f in static],'observedOriginals':[item(f) for f in orig],'observedSequenceOriginals':[item(f) for f in seqorig],'sequenceFrames':seq,'counts':{'staticDifferent':len(set(static)),'staticOpenedAsSource':len(set(orig)),'dynamicDifferent':37,'dynamicOpenedAsSource':len(seqorig),'auxiliarySheets':23},'runtimeReports':versions,'identity':identity,'limits':limits}
(out/'ObservedEvidence.json').write_text(json.dumps(obs,ensure_ascii=False,indent=2),encoding='utf-8')
rows=[
{'id':'D1','score':5,'max':5,'title':'菜单、选择、任务、成长、设置、加载、结算视觉体系完整','evidence':['Artifacts/UI47/Captures/ui-menu-1280x720.png','Artifacts/UI47/Captures/ui-progress-1280x720.png','Artifacts/UI47/Captures/ui-results-1280x720.png','Artifacts/Probe47/Captures/load-failure-terrain-forest.png','Artifacts/Probe47/Captures/load-failure-weapon-gun.png','SheetManifest.json (18 page types, 90 sources)'],'observation':'主菜单、三角色/五地图选择、任务/永久成长、双设置、加载、HUD、地图、构筑、普通升级、祭坛三选一、暂停、胜利与撤离/死亡结算均实看；奶油底板、深青标题/主操作、白/灰卡片与彩色图标统一。两个加载失败返回菜单提示清楚，未见默认按钮、粉材或缺图混入UI。','revision':'无本项必修；保持新增祭坛与现有页面共用容器、图标和模态层级。'},
{'id':'D2','score':5,'max':5,'title':'HUD、升级候选与技能树信息层级清晰、密度合理','evidence':['Artifacts/UI47/Captures/ui-hud-1280x720.png','Artifacts/UI47/Captures/ui-blessing-variety-1-1280x720.png','Artifacts/UI47/Captures/ui-blessing-owned-1280x720.png','Artifacts/Round47/Captures/20-skills.png','Artifacts/UI47/Captures/ui-upgrade-1280x720.png'],'observation':'生命/计时/资源/地图/能力摘要各有容器。普通四选与祭坛三选明确区分，候选有名称、效果、当前→新等级/层数；新机制有可叠加说明。已获六祝福位于构筑页前两行，疾风×2与其余×1清楚；可购卡、价格/等级、分类和滚动提示可辨，普通20-skills已独立补看。','revision':'无本项必修；继续保留已获祝福、购买价格与滚动指引的区别。'},
{'id':'D3','score':4,'max':5,'title':'中文字形完整、长文本无溢出、字号/行距/对齐稳定','evidence':['Artifacts/UI47/Captures/ui-upgrade-1920x1200.png','Artifacts/Round47/Captures/22-upgrade.png','Artifacts/UI47/Captures/ui-blessing-variety-0-1280x720.png','Artifacts/UI47/Captures/ui-blessing-owned-2560x1440.png'],'observation':'已看五画幅稳定页面未见缺字方框或关键字裁切，新祝福中文、每层数值、取消说明与层数完整。扣1分来自本版原图的长描述局部孤字：1920×1200普通升级“剑气”末行仅“气。”；1600×900 Full22-upgrade“生机”末行仅“命。”。属于排版精修问题，内容仍完整可读，不是硬门槛裁切。没有采用旧45图或历史扣分。','revision':'对“剑气/生机”等候选描述做短句重排或适当增宽/微调字号，避免末行仅一个汉字加标点；用本版实际候选在五画幅回归。'},
{'id':'D4','score':5,'max':5,'title':'要求画幅适配、交互状态、价格、锁定原因与图标明确','evidence':['SheetManifest.json (all five sizes)','Artifacts/UI47/Captures/ui-selection-locked-1280x720.png','Artifacts/UI47/Captures/ui-selection-unlocked-1280x720.png','Artifacts/UI47/Captures/ui-blessing-variety-1-2560x1080.png','Artifacts/UI47/Captures/ui-blessing-owned-2560x1440.png','Artifacts/Round47/Captures/20-skills.png'],'observation':'18种页面的1280×720、1920×1080、1920×1200、2560×1080、2560×1440矩阵均实际查看。核心面板不出界/拉伸；角色和群系的选中边框、可选白卡、锁定灰卡/锁标/原因清楚。主操作、返回、价格和三选一“暂不领取·保留祭坛”可辨。已获祝福灰卡写明本局生效，未与可购卡混淆。','revision':'无本项必修；新增卡片继续回归超宽、16:10与720p。'},
{'id':'F2','score':4,'max':5,'title':'页面动效、拾取/升级/解锁反馈与世界表现统一（仅页面部分建议）','scope':'page feedback observation only; full F2 score requires lead combination','evidence':['SequenceSheetManifest.json (all 37 feedback frames)','Artifacts/Round47/Sequences/feedback-pause-open/0001.jpg','Artifacts/Round47/Sequences/feedback-pause-open/0002.jpg','Artifacts/Round47/Sequences/feedback-task-unlock/0000.jpg','Artifacts/Round47/Sequences/feedback-upgrade-reward/0000.jpg'],'observation':'五条本轮连续序列全37帧可见暂停/构筑/升级淡入后稳定，任务完成/新内容解锁/晶石+15与获得“叠阵”Toast出现在左下且避开中心玩家和主要操作。未见页面持续透明、闪烁或场景材质突变。局部反馈以简短淡入、通用Toast和少量环形变化为主，层次/仪式感可继续精修，建议页面部分4。未看见祭坛专用连续选择/叠加/恢复升级序列，不据静态状态声称该新流程完整动态已验证。','revision':'在不遮住玩家和按钮的前提下增强奖励种类差异、层数变化与选择确认的细节节奏；补祭坛取消/重开/选择/常规升级恢复连续证据供完整F2复核。'}]
(out/'UIReviewRows.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8')
print('observed',obs['counts'])
