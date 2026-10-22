import json, hashlib
from pathlib import Path
from PIL import Image

ROOT=Path('E:/UnityProject/VampireLike3D')
OUT=ROOT/'Docs/ArtReviews/Round51/Combat'
def read(path): return json.loads((ROOT/path).read_text('utf-8-sig'))
def save(name,value): (OUT/name).write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding='utf8')
def digest(path): return hashlib.sha256((ROOT/path).read_bytes()).hexdigest().upper()
e=read('Docs/ArtReviews/Round51/Combat/ObservedEvidence.json')
observed=sorted({p for s in e['sheets'] for p in s['sources']}|{o['path'] for o in e['originals']})
frames=[]
for p in observed:
    with Image.open(ROOT/p) as im: size=list(im.size)
    frames.append({'path':p,'frame':Path(p).stem if '/Sequences/' in p else None,'sha256':digest(p),'originalSize':size,'viewedAtOriginalSize':any(o['path']==p for o in e['originals'])})
save('ObservedFiles.sha256.json',frames)
seq=[]
for p in sorted((ROOT/'Artifacts/Round51/Sequences').iterdir()):
    if not p.is_dir():continue
    meta=json.loads((p/'metadata.json').read_text('utf-8-sig'))
    allframes=list(p.glob('*.jpg'))
    seen=[f for f in frames if f['path'].startswith(p.relative_to(ROOT).as_posix()+'/')]
    seq.append({'sequence':p.name,'availableFrames':len(allframes),'metadataFrames':meta['frames'],'complete':len(allframes)==meta['frames'],'observedFrames':[int(f['frame']) for f in seen],'observedCount':len(seen),'metadata':meta})
save('SequenceInventory.json',seq)
verification=read('Artifacts/Round51/Verification.json')
builds={s:{x['file']:x['sha256'] for x in read(f'Artifacts/{s}/BuildIdentity.json')['files']} for s in ['Round51','UI51','Probe51']}
buildmatch=read('Artifacts/BuildMatch51.json')
e.update({'status':'core combat review complete; later deep-build preflight and actual 7200-second final frame remain supplemental pending evidence',
 'scope':'Fresh Round51 / 1.2.0 E1-E4, F1 and combat F2; no score or original image inherited from Round47/50',
 'observationMethod':'74 complete-frame thumbnail sheets, then selected original-size frames including adjacent originals; not a normal-speed video playback or every original frame',
 'uniqueDynamicFramesObserved':sum('/Sequences/' in p for p in observed),'uniqueStaticImagesObserved':sum('/Sequences/' not in p for p in observed),
 'originalFilesViewed':len(e['originals']),'sheetCount':len(e['sheets']),'sequenceCount':len(seq),'availableDynamicFrames':sum(s['availableFrames'] for s in seq),
 'allSequenceMetadataComplete':all(s['complete'] for s in seq),'allSequencesSampled':all(s['observedCount'] for s in seq),
 'fullVerification':{'finished':verification['finished'],'passed':verification['passed'],'checks':len(verification['checks']),'errors':verification['errors']},
 'originalProcessExit':read('Artifacts/Round51/ProcessExit.json'),
 'identity':{'buildIdentityFileCount':{s:len(v) for s,v in builds.items()},'Round51_UI51_Probe51_exactBuildFilesEqual':builds['Round51']==builds['UI51']==builds['Probe51'],
 'sourceVersion':read('Artifacts/Round51/SourceIdentity.json')['version'],'sourceFileCount':len(read('Artifacts/Round51/SourceIdentity.json')['files']),
 'workspaceIdentity':read('Artifacts/Workspace51Identity.json'),'developmentReleaseMatch':buildmatch,'releaseAudit':read('Artifacts/ReleaseAudit51.json')},
 'staticCaptureQuality':read('Artifacts/Round51/Captures/late-density-5.quality.json'),
 'staticWorldMetadata':{str(i):read(f'Artifacts/Round51/Captures/late-density-{i}.world.json') for i in range(7)},
 'fixtureLimitations':[
 'Fixed-step 12fps sequences are visual timing/shape evidence, not evidence of 60fps or sustained performance. Controlled healing prevents natural defeat in those captures.',
 'Role captures use stationary immortal targets; boss attack captures use boss immortality. They establish motion and warnings, not normal balance or natural survival.',
 'New enemy 5-8 normal/elite isolation explicitly disables game.Abilities and uses DiagnosticImmortal (VerificationExpansion.cs:126-138). HUD still lists nine acquired abilities. These sequences are NOT dense-combat proof. Density instead uses all 96 late-density-combinations frames plus seven current live density PNGs with nine abilities.',
 'Realtime effect/death/feedback metadata requests 0.03333 seconds, but wall timestamps govern actual capture intervals. No assumed 30fps playback or subframe invulnerability/damage precision.',
 'No complete dedicated shrine open/choice/resume-with-pending-upgrade continuous sequence was observed. Other page/upgrade/unlock sequences were observed.',
 'Boss leaving-boundary complete countdown/reset transition was not independently viewed here; visible danger zones and start instruction only.',
 'No audio audition, native desktop interaction, Player launch, long-duration dynamic performance test or actual 7200-second final frame reviewed in this core report.'
 ]})
save('ObservedEvidence.json',e)

rows=[
 {'id':'E1','score':5,'max':5,'title':'武器、能力与联动形色及命中层次','evidence':['Artifacts/Round51/Captures/weapon-crossbow.png','Artifacts/Round51/Captures/weapon-boomerang.png','Artifacts/Round51/Captures/weapon-froststaff.png','13 effect-* sequences: 12 frames each; originals flame/explosion/spore/crystalburst/lightning/frost 0002 or 0003','Artifacts/Round51/Sequences/late-density-combinations/0018.jpg'],
  'observation':'火球橙焰、金色细弹、青白飞剑、紫金折线雷、蓝冰碎片与符环具有独立轮廓；孢子紫色乳白点和晶爆粉色棱片可辨。连弩青金弓形、往返回旋刃青色折角轮廓、冰杖蓝晶杆体与冰迹实际出现。密战仍见多种发射与命中，并非只凭能力9的HUD计数判断。','revision':'无本项必修扣分；新增能力继续沿用独立形状和敌我色组。'},
 {'id':'E2','score':4,'max':5,'title':'高密叠加中的玩家、危险、目标与掉落辨识','evidence':['Artifacts/Round51/Sequences/late-density-combinations/0000.jpg through 0095.jpg: all 96 in six full-frame sheets','Original frames 0018,0024,0030,0072','Artifacts/Round51/Captures/late-density-0.png through late-density-6.png: seven originals'],
  'observation':'九能力持续密战中，玩家头帽与青色足环能连续追踪，粉红敌弹/红危险范围与青色经验有区分，目标金环可见。0018/0024/0030及0072的飞剑、蓝冰环、黄精英冠在中心和左侧拥聚，短时压缩怪物身体及地面预警的可用空隙，故扣1分。新怪隔离片段关闭能力，未用于满足本门槛。','revision':'降低精英冠重复叠加和己方冰环/剑雨覆盖的视觉权重，保留所有能力、实际发射与伤害；用同九能力高密回归玩家足环和红预警。'},
 {'id':'E3','score':5,'max':5,'title':'六Boss及精英预警形状、时序和命中区域','evidence':['boss-attacks-0,1,2,3,5,6: 25 samples each over complete 144-frame source','All six bosses original adjacent 0018-0021','Additional originals boss3/0066,boss5/0054,boss6/0054 and0114','enemy-attacks-5..8-normal/elite: 17 samples each; frog6-normal0067-0069 adjacent'],
  'observation':'树根多圆、沙虫方向长条、寒兽环震/冰点、熔岩多点爆发、菌蛙大圈/多孢圈、晶卫方向条/多晶落点具备可辨前摇，红粉圈线由淡至明确并出现条纹/感叹号，爆发在此前范围内。相邻原帧可见树根警示→命中花瓣→受击边缘的局部顺序。新孢囊/跃蛙/前甲冲锋/扇晶弹的普通精英轮廓和动作危险可辨。未见严重错位或被长时间完全遮盖；不据12fps推断精确判定毫秒。','revision':'维持敌方专用红粉形状；后续若改攻击范围，应以同版相邻原帧与正常实战回归。'},
 {'id':'E4','score':4,'max':5,'title':'闪避、受击、状态、边界与交互提示','evidence':['Artifacts/Round51/Sequences/character-move-dodge/0001.jpg through0003.jpg adjacent','boss-attacks-2/0018.jpg and0021.jpg','player-defeat/0004.jpg and0009.jpg','Artifacts/Round51/Captures/sanctuary-close-3.png','Artifacts/Round51/Captures/sanctuary-close-5.png','Artifacts/Round51/Captures/sanctuary-close-6.png'],
  'observation':'闪避弯身/浅色闪现、青色足部定位，受击红边与冻结蓝色状态在连续证据中有对应表现；目标金环与血条明确。未见严重过曝。火山祭坛深青色E开启挑战文字直接落在紫灰地面，对比偏弱但仍可读，扣1分。只观察到开战脱离提示，未看完整脱战倒计时，闪避无敌起止亦不作毫秒精度结论。','revision':'为世界交互文字加浅底或外描边，先回归火山紫灰地面，并保留祭坛5/6当前清晰度。'},
 {'id':'F1','score':4,'max':5,'title':'移动、攻击、闪避、受击、死亡与Boss起收动作','evidence':['role-action-adventurer/elementalist/swordsman: 12 samples each','role-closeup-adventurer/elementalist/swordsman: 16 samples each; original0019 each','character-move-dodge:12 samples plus adjacent0001-0003','player-defeat: all14 frames','boss-death-0,1,2,3,5,6: all141 frames, original0010 each','six boss attack sequences and new enemy frog adjacent0067-0069'],
  'observation':'三角色走转、手持动作、闪避倾身和受击死亡可辨；玩家倒下后进入结算，六Boss分别倾倒、压低或收缩后消退，树/虫/兽/岩像/菌蛙/晶卫都有实际连续死亡画面。跃蛙有离地与落点变化。短动作多以整体旋转/缩放完成，攻击后回正和Boss收招较直接，肢体重心衔接尚少层次，扣1分。青金悬浮能力枪与手中灰枪分离，不定性为断挂点。','revision':'延长角色攻击回收的缓冲，给Boss死亡/收招增加分段重心与肢体延迟，在现有三角色和六Boss原循环回归。'},
 {'id':'F2-combat','score':4,'max':5,'title':'战斗侧拾取、升级、解锁和界面反馈一致性（合并建议）','evidence':['feedback-pause-open all9','feedback-skills-open all7','feedback-task-unlock all10, original0000','feedback-upgrade-open all8','feedback-upgrade-reward all8','player-defeat all14; late-density-combinations all96'],
  'observation':'五反馈共42帧中奶油/深青页面淡入后稳定，任务完成与解锁提示清楚，升级奖励与回到世界的色彩一致，结算未见开战Toast遮住按钮。密战掉落与吸收反馈可辨。奖励主要依赖通用环和Toast，世界侧庆祝动作层次偏薄，建议4分；这是战斗侧贡献，最终F2须由总评与UI评审合并。未抽到祭坛专属打开→选项→升级恢复的完整转场。','revision':'为重大升级/首次解锁增加短促且不遮挡玩家的世界反馈；补祭坛专属连续转场用于后续回归。'}]
save('Scores.json',{'candidate':'Round51','version':'1.2.0','reviewer':'/root/art_combat47','rubricUnchanged':True,'rows':rows,'E':18,'F1':4,'F2CombatSuggestion':4,'complete100PointTotal':None,'scope':'独立战斗侧核心复评，不替代总评或后续长测补充'})
save('GateContributions.json',{'candidate':'Round51','version':'1.2.0','reviewer':'/root/art_combat47','scope':'Only combat-relevant gate contributions; full ten-gate conclusion belongs to art lead','gates':[
 {'id':'H07','status':'pass_in_observed_visual_scope','description':'九能力实际密战持续定位玩家和危险','evidence':'96/96 density source frames on full-frame sheets + originals0018/0024/0030/0072 and seven full-size map density PNGs','limitation':'New enemy isolated abilities-disabled sequences explicitly excluded from density proof; controlled healing not natural survival test.'},
 {'id':'H08','status':'pass_in_observed_visual_scope','description':'六Boss及新普通/精英前摇、预警与闪避对应表现','evidence':'150 boss attack sampled frames plus six adjacent0018-0021 original sets; enemy136 sampled frames; dodge adjacent0001-0003','limitation':'12fps controlled sequence supports visible ordering/range, not subframe hit timing or quantified invulnerability. Full disengagement countdown not viewed.'},
 {'id':'H09','status':'pass_in_observed_visual_scope','description':'动画及反馈有连续实机证据','evidence':'All47 sequences sampled, all six141 death frames and14 player death frames and42 feedback frames viewed via complete-frame sheets; selected originals','limitation':'Not every original frame or normal-speed playback; no complete dedicated shrine pending-upgrade transition.'},
 {'id':'H10','status':'pass_traceability_checked','description':'仅用51原图；Round51/UI51/Probe51的29条build identity完全相同','evidence':'ObservedEvidence.identity, ObservedFiles.sha256.json, sequence metadata, BuildMatch51, Workspace51Identity and ReleaseAudit51','limitation':'Functional pass assertions establish provenance and fixture facts, do not award visual points.'}],
 'dimensionContribution':{'E':18,'Eminimum':18,'F1':4,'F2CombatSuggestion':4,'allAssignedItemsAtLeast4':True},'notClaimed':['full100pointscore','alltenoverallgatesindependentlyreviewed','7200secondtestcompleted','60fps','audioaudition']})
print(json.dumps({'sheets':e['sheetCount'],'originals':e['originalFilesViewed'],'dynamic':e['uniqueDynamicFramesObserved'],'static':e['uniqueStaticImagesObserved'],'sequences':e['sequenceCount'],'available':e['availableDynamicFrames'],'buildEqual':e['identity']['Round51_UI51_Probe51_exactBuildFilesEqual'],'E':18,'F1':4,'F2combat':4},ensure_ascii=False))
