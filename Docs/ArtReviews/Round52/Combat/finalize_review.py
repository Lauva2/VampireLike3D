import json, hashlib
from pathlib import Path
from PIL import Image
ROOT=Path('E:/UnityProject/VampireLike3D'); OUT=ROOT/'Docs/ArtReviews/Round52/Combat'
def read(p):return json.loads((ROOT/p).read_text('utf-8-sig'))
def save(n,v):(OUT/n).write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf8')
def sha(p):return hashlib.sha256((ROOT/p).read_bytes()).hexdigest().upper()
e=read('Docs/ArtReviews/Round52/Combat/ObservedEvidence.json')
# Resolve preliminary wording against the actual opened originals. Preserve source/frame lists.
for b in e['batches']:
 n=b.get('note','')
 if n.startswith('52 frost boss adjacent18-21:'):
  b['note']='52 frost boss adjacent18-21 original: pink/red shock circle and smaller points visible on snow. Boss18 cream,19-21 blue; later original0030 confirms cream return. No individual EnemyActor pool lifecycle was tracked.'
 if n.startswith('52 lava original adjacent18-21:'):
  b['note']='52 lava original adjacent18-21: pink circles and exclamation precede orange eruption petals at19 onward; player cyan locator remains visible. Boss18 pale/grey hit shade and19-21 charcoal/orange, not a claimed blue freeze. Earlier frost note corrected against originals.'
 if n.startswith('52 giantfrog adjacent18-21:'):
  b['note']='52 giantfrog original18-21: large red shock circle and small left circle visible. Boss18 blue,19 pale/green,20-21 blue overlapping player; cyan locator remains visible though body is partly covered. No right-side green recovery at21 claimed. Local visual ordering only.'
 if n.startswith('52 crystal guardian adjacent18-21 original'):
  b['note']='52 crystal guardian original18-21: muted-red directional rectangles retained through player. Guardian18-19 rose,20-21 blue-grey overlay and turning pose. The adjacent set shows warning/turning, while later contact samples cover forward movement; do not claim completed lunge within18-21.'
paths=sorted({p for s in e['sheets'] for p in s['sources']}|{o['path'] for o in e['originals']})
files=[]
for p in paths:
 with Image.open(ROOT/p) as im:size=list(im.size)
 files.append({'path':p,'frame':int(Path(p).stem) if '/Sequences/' in p else None,'sha256':sha(p),'originalSize':size,'originalFileViewed':any(o['path']==p for o in e['originals'])})
save('ObservedFiles.sha256.json',files)
inventory=[]
for p in sorted((ROOT/'Artifacts/Round52/Sequences').iterdir()):
 if not p.is_dir():continue
 meta=json.loads((p/'metadata.json').read_text('utf-8-sig'));fs=list(p.glob('*.jpg'))
 seen=[f['frame'] for f in files if f['path'].startswith(p.relative_to(ROOT).as_posix()+'/')]
 inventory.append({'sequence':p.name,'availableFrames':len(fs),'metadataFrames':meta['frames'],'complete':len(fs)==meta['frames'],'observedFrames':seen,'observedCount':len(seen),'metadata':meta,'timestamps':str((p/'timestamps.csv').relative_to(ROOT)).replace('\\','/') if (p/'timestamps.csv').exists() else None})
save('SequenceInventory.json',inventory)
builds={s:{x['file']:x['sha256'] for x in read(f'Artifacts/{s}/BuildIdentity.json')['files']} for s in ['Round52','UI52','Probe52']}
ver=read('Artifacts/Round52/Verification.json')
assert ver['finished'] and ver['passed'] and not ver['errors']
assert len(inventory)==47 and all(s['complete'] and s['observedCount']>0 for s in inventory)
e.update({'status':'Round52 core combat independent visual review complete; deep preflight and actual7200-second final frame pending separate supplement',
 'scope':'Fresh52 E1-E4/F1 plus F2 combat contribution; no score/image inherited from51 or older rounds',
 'sheetCount':len(e['sheets']),'originalFilesViewed':len(e['originals']),'uniqueDynamicFramesObserved':sum('/Sequences/' in p for p in paths),'uniqueStaticImagesObserved':sum('/Sequences/' not in p for p in paths),
 'availableDynamicFrames':sum(s['availableFrames'] for s in inventory),'sequenceCount':len(inventory),'allSequenceMetadataComplete':True,'allSequencesSampled':True,
 'observationMethod':'76 complete-frame contact sheets and80 selected original files including adjacent source frames; not every original-size frame or normal-speed video playback',
 'fullVerification':{'finished':ver['finished'],'passed':ver['passed'],'checks':len(ver['checks']),'errors':ver['errors']},'originalProcessExit':read('Artifacts/Round52/ProcessExit.json'),
 'identity':{'sourceVersion':read('Artifacts/Round52/SourceIdentity.json')['version'],'sourceFileCount':len(read('Artifacts/Round52/SourceIdentity.json')['files']),
  'buildIdentityFileCounts':{k:len(v) for k,v in builds.items()},'Round52_UI52_Probe52_exactBuildFilesEqual':builds['Round52']==builds['UI52']==builds['Probe52'],
  'workspace':read('Artifacts/Workspace52Identity.json'),'developmentReleaseBridge':read('Artifacts/BuildMatch52.json'),'releaseAudit':read('Artifacts/ReleaseAudit52.json')},
 'captureQuality':read('Artifacts/Round52/Captures/late-density-5.quality.json'),
 'sevenMapWorldMetadata':{str(i):read(f'Artifacts/Round52/Captures/late-density-{i}.world.json') for i in range(7)},
 'limitations':[
  'Fixed simulation12fps/controlled healing establishes sampled visual ordering and shapes, not actual60fps, normal survival, balance or long performance.',
  'Role closeups use stationary immortal targets; boss attack capture uses boss immortality. Death/feedback/effect sequences have actual timestamps, requested interval0.03333 does not establish30fps. For example frost frame0=.0009,1=.0950,2=.1662wallSeconds.',
  'VerificationExpansion.cs126-138 explicitly disables Abilities and makes isolated type5-8 enemy immortal. HUD still says9acquired. These eight isolated enemy sequences are NOT dense-combat proof; density evidence is all96 live combination frames plus seven live density PNGs.',
  'Ordinary/elite base colours, blue status overlays and pale hit feedback were visibly sampled, including blue ordinary mushrooms/pods in density0030/0086. No identified same-enemy full freeze/burn->despawn->reinitialize lifecycle was independently tracked visually; functional pool assertions do not fill that visual limitation.',
  'Full boss disengagement countdown/reset and dedicated shrine choice->queued-upgrade->resume continuous transition not independently viewed. Start instruction, warning areas, other five page/reward transitions are observed.',
  'No desktop control, Player launch/build, audio audition, deep fixed-build frame or actual7200-second final frame in this report. No long-duration conclusion.'
 ]})
save('ObservedEvidence.json',e)
rows=[
 {'id':'E1','score':5,'max':5,'title':'九武器/能力与联动的形色、命中层次','evidence':['13 effect-* sequences,12 samples each; originals frost/flame/lightning/crystalburst/spore0002','weapon-crossbow/boomerang/froststaff current52 original PNGs','late-density-combinations0018,0030,0072,0086; all96frames in full-frame sheets'],
  'observation':'本轮原图中橙焰/暖色爆瓣、细金弹、青白剑列、青白灵体、紫金折雷、蓝冰碎片、金符环与紫乳白孢点/粉晶棱各有形状。新弩青金弓形、青色折刃、蓝晶杖可区分；密战实际同时出现多类发射/命中，未只凭HUD能力9计分。','revision':'本项无必修扣分；后续继续保持敌我形色、发射体和命中体层次。'},
 {'id':'E2','score':4,'max':5,'title':'密战中的玩家、标记、敌弹和拾取物','evidence':['late-density-combinations0000-0095 all96 full-frame sheet frames; originals0018/0030/0072/0086','late-density-0..6 seven current52 original PNGs'],
  'observation':'玩家头帽/青足环在96帧中可跟随，七地图密图中心均可找到人物；粉敌弹/红粉危险与青经验、金目标圈有区分。0018/0030的剑列/冰环，0072/0086和左侧敌群的精英冠叠加，挤压身体及地面可用空隙，扣1分；未观察到玩家持续完全不可见。新怪关闭能力的隔离图不计密战证明。','revision':'减少重复精英冠和己方冰圈/剑列的视觉权重；保持九能力、实际发射和伤害后回归中心脚环与红危险。'},
 {'id':'E3','score':5,'max':5,'title':'六Boss/普通精英的前摇、范围与命中视觉','evidence':['boss-attacks0/1/2/3/5/6各25表样本并补0018-0021相邻原图；5/0054和6/0054原图','enemy-attacks5..8-normal/elite各17表样本；5elite0018、6normal0067-0069、7normal/elite0054、8normal/elite0018原图'],
  'observation':'树根三圆、虫向长条、寒兽大震圈/小冰点、熔岩多爆点、菌蛙大圈/多孢点、晶卫多方向条/落晶圈的危险形状与作用区域相符。原树18-21看见警示→圈内爆发→受击边缘，熔岩18-21看见圈内先后爆瓣。新怪孢点、跃蛙落点、甲虫直条、晶妖扇弹能区分；甲虫/晶卫条线早段偏淡，但原0054轴线和之后移动仍可辨。没有严重范围错位或长时间遮没危险，时间精度限于采样。','revision':'保持红粉危险专用形状；若改范围/动作必须回归相邻原帧和多能力密战。'},
 {'id':'E4','score':4,'max':5,'title':'闪避、受击、状态、边界及交互提示','evidence':['character-move-dodge0001-0003 original adjacent','boss-attacks2 original0018-0021 and0030; density0030/0086','player-defeat0004/0009; sanctuary-close-3 current52 original'],
  'observation':'闪避浅色闪身、倾身位移、青足环与红边受击可见；普通怪部分蓝色状态覆盖与邻居基础色并存，寒兽18奶油色→19-21蓝色→30恢复奶油色已看。未见整片错误染色/严重过曝。火山深青交互字直接压紫灰地，偏暗但可读，扣1分。没有把Boss变色当作普通怪完整出池重设证明。','revision':'给世界交互字浅底/描边，先回归火山地面；若继续改染色，补同一个普通/精英敌人状态结束及重新出池的连续原帧。'},
 {'id':'F1','score':4,'max':5,'title':'三角色动作与六Boss起收、受击及死亡衔接','evidence':['three role-action sequences12 samples each and three closeup sequences16 each; adventurer0019 original','character-move-dodge12samples+0001-0003 original','player-defeat all14 sheetframes+0004/0009 originals','all six boss-death sequences full available frames in sheets;0010 original each; six attack sequences'],
  'observation':'三角色走转/出手/施法可辨，闪避有身体和位置反馈；玩家倾倒后进入结算。六Boss确有前摇与死亡连续表现，树倾、虫压缩/消退、寒兽翻倒、岩像压平、菌蛙侧倒、晶卫下沉。短循环和收招仍较直接，多用整体旋转/缩放，重心/肢体延迟层次不足，扣1分；无已见明显断肢/整段缺动画。','revision':'增加角色回收缓冲与Boss分段重心、肢体延迟，三角色六Boss循环一起回归。'},
 {'id':'F2-combat','score':4,'max':5,'title':'拾取、升级、解锁与页面反馈的战斗侧建议','evidence':['all five feedback sequences full frames; task-unlock0000 original','player-defeat all14 and settlement0009 original; density all96'],
  'observation':'奶油深青页面从淡入至稳定，任务完成/晶石+15提示清楚；拾取、升级和解锁与世界色系统一，结算原0009无开战Toast挡返回按钮。世界奖励多依靠通用环和Toast，重大解锁的动作层次偏薄，建议4分。最终F2应与UI评审合并，不另增评分项。祭坛专属选择后带待升级恢复转场未抽样。','revision':'为重大升级/首次解锁加短促且不遮挡玩家的世界反馈，并补祭坛专属连续转场。'}]
save('Scores.json',{'candidate':'Round52','version':'1.2.0','reviewer':'/root/art_combat47','rubricUnchanged':True,'freshReview':True,'rows':rows,'E':18,'F1':4,'F2CombatSuggestion':4,'complete100PointTotal':None,'status':'core combat visual review complete; no long-duration conclusion'})
gates=[
 {'id':'H07','status':'pass_in_observed_core_visual_scope','evidence':'Current52 all96 live combination frames and7 map density originals','limitation':'Abilities-disabled eight enemy isolation sequences excluded; healing fixture is not natural survival; no future deep/long frame claim.'},
 {'id':'H08','status':'pass_in_observed_core_visual_scope','evidence':'SixBoss150 sheet attack samples plus24 adjacent originals and other original windups; eight new normal/elite attack sequences; dodge adjacent0001-0003','limitation':'12fps visible ordering/range only, no subframe invulnerability/damage timing. Full disengagement countdown not independently viewed.'},
 {'id':'H09','status':'pass_in_observed_core_visual_scope','evidence':'All47 actual runtime sequences sampled; all six death available frames and14 playerdeath frames and five feedback fullframes in sheets; selected original files','limitation':'Not original-size every frame or normal-speed playback; dedicated shrine pending-upgrade transition not sampled.'},
 {'id':'H10','status':'pass_traceability_checked','evidence':'Current52 only;29build entries identical acrossRound52/UI52/Probe52;1309source and24AA development/release exact bridge; each observed sourceSHA256 recorded','limitation':'Identity/function assertions do not earn visual points.'}
]
save('GateContributions.json',{'candidate':'Round52','reviewer':'/root/art_combat47','gates':gates,'dimensionContribution':{'E':18,'F1':4,'F2CombatSuggestion':4,'assignedItemsMinimum':4},'fullTenGateVerdict':None,'noLongDurationClaim':True})
print(json.dumps({'sheets':len(e['sheets']),'originals':len(e['originals']),'dynamic':e['uniqueDynamicFramesObserved'],'static':e['uniqueStaticImagesObserved'],'totalDynamic':e['availableDynamicFrames'],'sequences':len(inventory),'buildEqual':e['identity']['Round52_UI52_Probe52_exactBuildFilesEqual']},ensure_ascii=False))
print([(s['sequence'],s['observedCount'],s['availableFrames']) for s in inventory])
