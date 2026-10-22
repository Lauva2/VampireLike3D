import json,hashlib
from pathlib import Path
from PIL import Image
root=Path('E:/UnityProject/VampireLike3D');out=root/'Docs/ArtReviews/Round51/Combat'
def read(p):return json.loads((root/p).read_text('utf-8-sig'))
def sha(p):return hashlib.sha256((root/p).read_bytes()).hexdigest().upper()
frame='Artifacts/Soak51Preflight/FinalFrame.png'
with Image.open(root/frame) as im:size=list(im.size)
builds={s:{x['file']:x['sha256'] for x in read(f'Artifacts/{s}/BuildIdentity.json')['files']} for s in ['Soak51Preflight','Round51']}
record={'candidate':'Round51','version':'1.2.0','reviewer':'/root/art_combat47','reviewType':'Independent static deep-preflight supplement; no new scoring denominator',
 'actualObservation':{'path':frame,'sha256':sha(frame),'size':size,'viewed':'original full frame with UI, not cropped','staticImages':1,'dynamicFrames':0,'otherCandidateImagesUsed':False},
 'provenance':{'Summary':read('Artifacts/Soak51Preflight/Summary.json'),'CurrentBuild':read('Artifacts/Soak51Preflight/CurrentBuild.json'),'ImportedUpgradeSource':read('Artifacts/Soak51Preflight/ImportedUpgradeSource.json'),'ProcessExit':read('Artifacts/Soak51Preflight/ProcessExit.json'),
 'buildIdentityEntries':{s:len(d) for s,d in builds.items()},'exactBuildIdentityMatchesCore51':builds['Soak51Preflight']==builds['Round51'],
 'hashes':{p:sha(p) for p in ['Artifacts/Soak51Preflight/Summary.json','Artifacts/Soak51Preflight/CurrentBuild.json','Artifacts/Soak51Preflight/BuildIdentity.json','Artifacts/Soak51Preflight/SourceIdentity.json','Artifacts/Soak51Preflight/ImportedUpgradeSource.json','Artifacts/Soak51Preflight/ProcessExit.json']}},
 'observations':[
 {'subject':'player','finding':'中心棕色头部、脸和青色足环可定位；武器内圈覆盖躯干下部但该静态仍可辨人物锚点。','status':'readable_in_this_frame'},
 {'subject':'own_attacks','finding':'青白剑列/金柄、青白灵体弧、橙色火焰/大叶状爆发与细线弹道分组可辨。弧形轨迹数量大，内圈较拥挤；不能凭一帧确证每种武器均在此瞬间发射，九能力持有由同帧邻近CurrentBuild记录辅助。','status':'distinguishable_with_crowding'},
 {'subject':'enemies','finding':'左右及下缘聚集敌人，蓝圆甲/绿圆怪/紫孢囊的主体色和轮廓可辨；金精英冠与剑/火叠在一起，局部身体被部分覆盖。仍未在本帧形成无法辨认玩家的直接阻断。','status':'readable_with_local_crowding'},
 {'subject':'danger','finding':'本帧未清楚展示一轮红粉敌方预警与实际命中，不能单独判断危险时序、范围一致性或九能力深构筑全程危险可读性。','status':'not_established_by_single_frame'},
 {'subject':'HUD_and_materials','finding':'Lv.520、能力9、生命300/300、时间152:59、地图和下方按钮文字清楚；黄色沙地上青色掉落可见。未见粉色错误材质或整片白爆曝光；暖色大面积覆盖使局部地表层次变弱。','status':'no_direct_static_blocker_observed'}],
 'verdict':{'directStaticBlockerObserved':False,'persistentDangerReadabilityEstablished':False,'coreScoresChanged':False,'E2CoreScore':4,'scope':'未见本帧直接视觉阻断；保留核心E2=4的局部拥挤扣分。单静态不单独重判H07/H08，也不提高原核心91分。'},
 'limitations':['Only one final static image observed; no animation, attack timing, flicker, continuous player visibility or performance conclusion.',
 'Summary reports wallSeconds180.2/requested180,errorCount0,renderEveryFrame true,controlledHealingAndUnlockedBuild true; these are recorded runtime facts, not visual performance proof.',
 'fixedImportedBuild true; level520 and upgrades imported at test start, not 180 seconds of natural growth to520. Displayed game clock starts around9000 seconds and is not elapsed wall time.',
 'CurrentBuild contains nine ability IDs0-8, seed28354315, level520; renderResolution1920x1080 4x MSAA and quality5. This single desert frame cannot extend coverage to every map or all enemy attacks.',
 'No actual7200-second final frame or full long-duration sequence observed yet. No desktop control or Player launch performed.']}
(out/'DeepPreflightReview.json').write_text(json.dumps(record,ensure_ascii=False,indent=2),encoding='utf8')
p=out/'ObservedEvidence.json';e=json.loads(p.read_text('utf8'));e['supplements']=e.get('supplements',[])
entry={'type':'deepFixedBuildPreflightStatic','report':'Docs/ArtReviews/Round51/Combat/DeepPreflightReview.json','path':frame,'sha256':sha(frame),'observedStaticImages':1,'includedInCoreCounts':False}
if entry not in e['supplements']:e['supplements'].append(entry)
e['status']='core combat review complete; fixed level520 deep-preflight final static frame reviewed separately; actual7200-second final frame pending'
p.write_text(json.dumps(e,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'sha256':sha(frame),'size':size,'buildMatchesCore51':record['provenance']['exactBuildIdentityMatchesCore51'],'exitCode':record['provenance']['ProcessExit']['exitCode']},ensure_ascii=False))
