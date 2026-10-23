from pathlib import Path
from PIL import Image
import json,hashlib,datetime
r=Path('E:/UnityProject/VampireLike3D');o=r/'Docs/ArtReviews/Round54/Combat54Fresh'; prep=json.loads((o/'PreparedEvidence.json').read_text(encoding='utf8'))
raw=['Artifacts/Round54/Sequences/late-density-combinations/0032.jpg','Artifacts/Round54/Sequences/boss-attacks-0/0019.jpg']
raw += [f'Artifacts/Round54/Sequences/boss-attacks-{b}/{f:04}.jpg' for b in [0,1,2,3,5,6] for f in [18,20,21]]
raw += [f'Artifacts/Round54/Sequences/role-action-adventurer/{f:04}.jpg' for f in [12,13,14,36,37,38]]
raw += [f'Artifacts/Round54/Sequences/effect-{n}/0004.jpg' for n in ['flame','frost','explosion']]
raw += [f'Artifacts/Round54/Sequences/late-density-combinations/{f:04}.jpg' for f in [64,95]]
raw += [f'Artifacts/Round54/Captures/late-density-{i}.png' for i in range(7)]
raw += [f'Artifacts/Round54/Captures/weapon-{s}.png' for s in ['boomerang','crossbow','froststaff']]
raw += [f'Artifacts/UI54/Sequences/{s}/{f:04}.jpg' for s,f in [('popup-upgrade',1),('popup-upgrade',2),('popup-upgrade',4),('popup-blessing',1),('popup-pause',1),('popup-toast',1)]]
assert len(raw)==len(set(raw))==47
sha=lambda p:hashlib.sha256((r/p).read_bytes()).hexdigest()
rawentries=[{'path':p,'sha256':sha(p),'width':Image.open(r/p).width,'height':Image.open(r/p).height,'viewMode':'original-file-opened'} for p in raw]
observed=set(x['path'] for rec in prep['records'] for x in rec['frames']) | set(p for p in raw if '/Sequences/' in p)
metadata=[]
for rec in prep['records']:
 p=f"Artifacts/{rec['source']}/Sequences/{rec['sequence']}/metadata.json"
 if (r/p).exists():metadata.append({'path':p,'sha256':sha(p),'data':json.loads((r/p).read_text(encoding='utf-8-sig'))})
reportchecks=[]
for source in ['Round54','UI54']:
 p=f'Artifacts/{source}/Verification.json';v=json.loads((r/p).read_text(encoding='utf-8-sig'));ep=f'Artifacts/{source}/ProcessExit.json';e=json.loads((r/ep).read_text(encoding='utf-8-sig')); reportchecks.append({'path':p,'sha256':sha(p),'finished':v.get('finished'),'passed':v.get('passed'),'checks':len(v.get('checks',[])),'errors':v.get('errors'),'exitPath':ep,'exitSha256':sha(ep),'exitCode':e['exitCode']})
limitations=['仅评E1-E4/F1-F2，不形成100分完整总分；没有迁用52/53成绩或图。','以现成Windows开发Player Camera.Render/runtime原帧及逐序列联系表观察；未控制桌面、另开Player、构建或修改原图。','未按正常实时速度试玩，固定12fps动作只作记录采样；死亡/反馈/UI按真实墙钟记录，requestedInterval不等于实际帧率。','角色近景/动作目标可能固定且不死；Boss攻击采集Boss不死；持续治疗不证明自然生存难度。','预警匹配限所见视觉范围与时序，不穷尽所有种子、全部组合、亚帧命中、完整脱战倒计时。','不据此声明性能、60FPS、7200秒长测、音频试听或整项目通过。','武器三近景为运行时展示视角，仅补充形状；战斗评分依靠正常动作与密战。']
obs={'candidate':'Round54','reviewer':'popup54_combat','reviewDate':'2026-10-23','scope':['E1','E2','E3','E4','F1','F2'],'rubric':'Docs/ArtReview.md §3 原100分20项，每项5分','independentFreshReview':True,'counts':{'sequences':63,'round54Sequences':48,'ui54Sequences':15,'availableDynamicFrames':sum(rec['availableFrames'] for rec in prep['records']),'viewedContactSheets':77,'differentFramesViaSheets':724,'differentDynamicFramesObserved':len(observed),'differentStaticOriginals':10,'originalFilesOpened':47,'originalJpgOpened':37,'originalPngOpened':10},'sheetsViewed':[{**s,'sha256':sha(s['path']),'viewed':True} for s in prep['sheets']],'sequenceSamples':prep['records'],'originalFilesOpened':rawentries,'observedDynamicPaths':sorted(observed),'metadata':metadata,'reportsRead':reportchecks,'identityFiles':[{'path':f'Artifacts/{src}/BuildIdentity.json','sha256':sha(f'Artifacts/{src}/BuildIdentity.json')} for src in ['Round54','UI54']],'observations':[{'code':'E1','text':'普通枪线/橙火球/青白飞剑、蓝冰圈、橙焰花瓣、符阵/爆破脉冲在13效果与密战样本中可分辨；无大片过曝。'},{'code':'E2','text':'96密战帧顺序全看，玩家帽/头和青脚圈持续可跟随，拾取青晶及标记仍可辨；0032、0064、0095周边怪群/金冠/飞剑短时重叠。'},{'code':'E3','text':'六Boss攻击的12帧时间分布样本，另六条0018/0020/0021原尺寸补看，圆圈落点/环形/直线冲击阶段与实际展开区域视觉一致；普通精英5-8也有预警→攻击→恢复。'},{'code':'E4','text':'0036→0038闪避由实色转浅色倾身及尾迹，Boss蓝冻结/红闪光、屏边受击色可见；赤晶地面细红直线框局部偏淡。'},{'code':'F1','text':'三角色移动转向、浅色闪避、近景射击/施法/挥剑，六Boss起收招/全部死亡帧及玩家倒地14帧均有连续变化；整体肢体倾转/臂摆/缩倒较简化。'},{'code':'F2','text':'15组UI原序列全看，底板/文字/卡片/按钮一起淡入/位移；遮罩世界稳定、暂停时仍进入。升级奖励/任务与拾取反馈稳定，仍主要用通用环/提示条，差异层次有限。'}],'hardBlockersObserved':[],'limitations':limitations}
(o/'ObservedEvidence.json').write_text(json.dumps(obs,ensure_ascii=False,indent=2),encoding='utf8')
scores={'candidate':'Round54','reviewer':'popup54_combat','scope':'Independent combat and feedback domain only','scores':{'E1':5,'E2':4,'E3':5,'E4':4,'F1':4,'F2':4},'dimensionTotals':{'E':18,'F':8},'domainTotal':26,'domainMaximum':30,'complete100PointTotal':None,'minimumItem':4,'domainThresholdsMet':True,'hardBlockersObserved':[],'relatedHardGates':{'H07_denseCombatPlayerAndDanger':'passed_in_observed_samples','H08_bossWarningsAndDodge':'passed_in_observed_visual_scope','H09_continuousAnimationFeedback':'passed_with_recorded_sampling_limits','H10_sameCandidate':'Round54 and UI54 BuildIdentity read; full source/release bridge delegated to lead identity audit'},'fullArtPassDeclared':False,'performanceVerified':False,'evidence':'ObservedEvidence.json','limitations':limitations}
(o/'Scores.json').write_text(json.dumps(scores,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps(obs['counts'],ensure_ascii=False))

