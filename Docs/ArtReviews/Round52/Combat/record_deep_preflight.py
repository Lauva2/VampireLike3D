import json,hashlib
from pathlib import Path
from PIL import Image
ROOT=Path('E:/UnityProject/VampireLike3D'); OUT=ROOT/'Docs/ArtReviews/Round52/Combat'
def read(p):return json.loads((ROOT/p).read_text('utf-8-sig'))
def sha(p):return hashlib.sha256((ROOT/p).read_bytes()).hexdigest().upper()
frame='Artifacts/Soak52Preflight/FinalFrame.png'
with Image.open(ROOT/frame) as im:size=list(im.size)
builds={s:{x['file']:x['sha256'] for x in read(f'Artifacts/{s}/BuildIdentity.json')['files']} for s in ['Soak52Preflight','Round52']}
r={'candidate':'Round52','version':'1.2.0','reviewer':'/root/art_combat47','scope':'Independent new52 fixed-depth final static frame supplement only; no inherited51 frame or score',
 'observation':{'path':frame,'sha256':sha(frame),'originalSize':size,'viewedOriginalFullFrame':True,'staticImagesObserved':1,'dynamicFramesObserved':0},
 'provenance':{'Summary':read('Artifacts/Soak52Preflight/Summary.json'),'ProcessExit':read('Artifacts/Soak52Preflight/ProcessExit.json'),'CurrentBuild':read('Artifacts/Soak52Preflight/CurrentBuild.json'),'ImportedUpgradeSource':read('Artifacts/Soak52Preflight/ImportedUpgradeSource.json'),
 'buildIdentityCounts':{s:len(v) for s,v in builds.items()},'buildIdentityExactlyMatchesCore52':builds['Soak52Preflight']==builds['Round52'],
 'hashes':{p:sha(p) for p in ['Artifacts/Soak52Preflight/Summary.json','Artifacts/Soak52Preflight/CurrentBuild.json','Artifacts/Soak52Preflight/BuildIdentity.json','Artifacts/Soak52Preflight/SourceIdentity.json','Artifacts/Soak52Preflight/ProcessExit.json','Artifacts/Soak52Preflight/ImportedUpgradeSource.json']}},
 'visualFindings':[
 {'subject':'player','observation':'中央棕发/头脸与青色足环可定位，围绕身体的橙色内圈和青白武器遮住部分躯干；附近大树冠没有在这张原图里完全挡住定位锚点。','status':'readable_in_this_frame'},
 {'subject':'attacks','observation':'右侧青白剑刃/金柄列、青白灵体弧、橙色内圈和远处橙焰可凭形色分组；青经验与细弹道可见。层数多且大面积金黄/橙色爆发压低地面细节，局部视觉拥挤。','status':'distinguishable_with_crowding'},
 {'subject':'enemy_density','observation':'左右及下方敌群拥密，绿色圆怪、蓝甲虫、紫色菌怪和金精英冠仍能区分；边缘部分身体被攻击/树冠/其他敌人遮盖，冠与暖色攻击相互争夺注意力。中央人物仍可见。','status':'local_occlusion_without_direct_static_player_loss'},
 {'subject':'danger','observation':'本图没有足够明确的一轮红粉敌方预警起止/命中过程，不能独立判深构筑中危险时序或全程可躲避性。不能将大金黄自身攻击区域当成已核实的敌方预警。','status':'not_established_by_single_frame'},
 {'subject':'HUD_materials','observation':'Lv520/生命300/300/能力9/时间152:59、地图与底部按钮可读。未见粉材、整幅白爆或明显模型缺件；金黄攻击大片覆盖部分森林地表。','status':'no_direct_static_blocker_seen'}],
 'verdict':{'directStaticVisualBlockerObserved':False,'coreScoresChanged':False,'E2CoreScore':4,'persistentVisibilityOrDangerEstablished':False,'scope':'仅该帧未见直接阻断，局部拥挤保持核心E2扣1；不提高分数、不单独重判完整H07/H08或性能。'},
 'limitations':['Only one original1920x1080 static image observed; no animation/flicker/timing/full180secondreadability/FPS inference.',
 'Summary reports180.2 wallSeconds/requested180/errorCount0/renderEveryFrame true and original process exit0. These runtime records do not turn one image into performance proof.',
 'fixedImportedBuild=true, controlledHealingAndUnlockedBuild=true; level520 and upgrades imported at start, not natural growth to520 in180seconds. HUD152:59 is preloaded game clock, not wall elapsed.',
 'CurrentBuild seed438899693, level520, quality5, abilityIDs0-8; cannot establish every ability firing in this exact instant merely from nine acquired IDs.',
 'This is current52 mixed Forest/Desert region image, not51 desert final frame. Does not cover every biome or all enemy warnings.',
 'Actual7200-second growing soak final frame not observed yet; no long-duration verdict. No desktop or Player control performed.']}
(OUT/'DeepPreflightReview.json').write_text(json.dumps(r,ensure_ascii=False,indent=2),encoding='utf8')
p=OUT/'ObservedEvidence.json';e=json.loads(p.read_text('utf8'));e['supplements']=e.get('supplements',[])
item={'type':'fixed520DeepPreflightStatic','report':'Docs/ArtReviews/Round52/Combat/DeepPreflightReview.json','path':frame,'sha256':sha(frame),'staticImagesObserved':1,'includedInCoreCounts':False}
if item not in e['supplements']:e['supplements'].append(item)
e['status']='Round52 core complete; new52 fixed520deep preflight final static independently observed; actual7200secondgrowing final frame pending'
p.write_text(json.dumps(e,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'sha256':sha(frame),'size':size,'sameBuild':r['provenance']['buildIdentityExactlyMatchesCore52'],'exitCode':r['provenance']['ProcessExit']['exitCode']}))
