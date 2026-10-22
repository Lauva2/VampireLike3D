from pathlib import Path
import json,hashlib,datetime
load=lambda p:json.loads(Path(p).read_text(encoding='utf-8-sig'))
base=Path('Docs/ArtReviews/Round47');w=load(base/'World/Review.json');u=load(base/'UI/ObservedEvidence.json');c=load(base/'Combat/ObservedEvidence.json');cr=load(base/'Combat/Scores.json');ur=load(base/'UI/UIReviewRows.json');ident=load(base/'IdentityAudit.json')
rows=w['rows']+[dict(x,reviewer='art_ui47') for x in ur if x['id'].startswith('D')]+[dict(x,reviewer='art_combat47') for x in cr['rows'] if x['id']!='F2']
uf=next(x for x in ur if x['id']=='F2');cf=next(x for x in cr['rows'] if x['id']=='F2')
rows.append(dict(id='F2',score=4,max=5,title='页面动效、拾取升级解锁与整体一致性',reviewer='art_ui47 + art_combat47; art_lead47 synthesis',evidence=list(dict.fromkeys(uf['evidence']+cf['evidence'])),observation=uf['observation']+' '+cf['observation'],revision='细化拾取/升级/解锁的节奏差异与世界反应，补祭坛打开→选择→常规升级恢复的连续片段作后续专项回归。'))
rows.sort(key=lambda x:x['id']);dims={d:sum(x['score'] for x in rows if x['id'].startswith(d)) for d in 'ABCDEF'}
statics=set(w['actualViewing']['staticImages'])|{x['path'] for x in u['staticSourcesObserved']}|{x['path'] for x in c['originals'] if x['path'].endswith('.png')}
dynamic={f"Artifacts/Round47/Sequences/{s['name']}/{n}.jpg" for s in c['sequences'] for n in s['observedFrameIds']}|set(w['actualViewing']['originalSequenceFiles'])|{x['path'] for v in u['sequenceFrames'].values() for x in v['sources']}
originals=set(w['actualViewing']['originalFiles']+w['actualViewing']['originalSequenceFiles'])|{x['path'] for x in u['observedOriginals']+u['observedSequenceOriginals']}|{x['path'] for x in c['originals']}
sheets=set(w['actualViewing']['sheets'])|set(u['observedSheets'])|set(u['observedSequenceSheets'])|{x['sheet'] for x in c['sheets']}
limitations=[
'本轮只对同版Round47/UI47/Probe47实际视觉范围评分。未使用42/45/46图片或成绩作为47依据；46未形成总分，不补旧成绩。',
'三名独立评审分域实际观看；总评本人只看世界/角色89不同静态及3排查原帧，没有声称本人看过UI/Combat全部证据。联合计数去重，分域清单另存。',
'没有控制桌面、窗口或鼠标键盘；评审没有启动Player/构建，未亲自实时交互试玩，只看现成后台Player原始Camera.Render与实际runtime连续帧。',
'联合看33条序列的样本，未逐帧看完1420全部帧，也没有完整正常速度录像播放。原尺寸1280×720；角色/移动/Boss攻击/密战固定12fps且受控治疗，角色目标固定/不死，Boss攻击采集Boss不死。',
'效果/死亡/页面反馈是实际墙钟采样，requestedInterval不等于60FPS；H08/E3只针对已见起手-预警-生效区域的视觉一致，不声称独立量化全部亚帧伤害判定、碰撞体或脱战倒计时。功能断言不产生美术分。',
'六祝福/同种叠加/已获层数及普通升级稳定状态已覆盖；没有祭坛专属打开→取消/重开→选择→排队升级恢复全程连续视觉。这是新增转场的抽样限制，不以静态或功能断言声称已观察；H09依33条实际动作/反馈连续证据判定。',
'所有冻结类别已进入原20项，但未遍历无限地图全部种子、全部任务/强化滚动行和所有组合状态。UI矩阵/Full/Probe为同候选不同采集，不伪装成同一局。',
'辅助联系表完整画幅缩小仅供索引，已补原图；超过显示上限的源图工具可能缩到2048px，不声称2560图逐像素1:1人工检查。',
'Soak47Short末帧仅同版300.1秒短测静态补充（26级/六能力/受控治疗与解锁、非固定导入），不能证明动态、自然难度、60FPS、两小时或长期性能。',
'源码/构建身份/117功能检查/124 UI检查/8 Probe检查/Release静态审计仅作归属和完整性旁证，不代替视觉得分；美术通过不代表功能、音频人工试听、平衡、性能或整项目通过。']
gate_data=[
('total','总分至少91/100','20个原子项重新独立观察合计91/100。'),
('dimensions','A/B≥13、C/D/E≥18、F≥8','A14/B14/C18/D19/E18/F8，均达原下限。'),
('items','20个子项均≥4','20项各4或5分，原100分/20子项完整保留。'),
('frozen_content','全部冻结首发类别同版实机覆盖且无未评分类别','3角色、八普通/精英、四Boss、四单群系/Mixed/四边界/事件、六能力与联动、18类正式UI五画幅均进入分项；33序列含角色/四Boss/死亡/密战/反馈。全类别覆盖不等于穷举全部变体。'),
('finished_assets','无占位、粉材质、丢贴图或明显破损模型','世界角色89静态、UI97静态及战斗动态/末帧所看范围未见门槛级缺陷；自动悬浮枪与角色手持枪补3原帧区分，不属断挂点。'),
('chinese_layout','中文无缺字方框、关键裁切、不可读按钮、遮罩错误或核心UI出界','18类UI×五画幅90图均经UI评审实际看，另30静态原图；两处气。/命。孤字降D3，无字形丢失/关键裁切。'),
('dense_readability','密战持续定位玩家与危险，不靠停射删能力或关闭实际伤害造图','Combat实际看全部96密战帧、五地图密战原图与Boss样本；六能力持续发射/命中，玩家头帽/青脚圈与粉危险可追踪。采集有受控治疗，非关闭伤害，未由评审停止发射/删能力。'),
('danger_and_invulnerability','Boss危险有预警、闪避窗口有表现，无持续不可见范围掩盖严重错位','四Boss各25时间跨度样本并补各0018–0021相邻原帧，树根圆/虫冲条/冰震/火喷起手和范围相符；闪避连续原帧有俯身/亮身/青脚反馈，未见持续遮蔽。仅视觉范围，不扩大到亚帧穷尽判定。'),
('continuous_feedback','动画与反馈有实际运行连续证据','33序列存在且完成；角色/移动/四Boss攻击与死亡/玩家死亡/96密战/五页面反馈连续样本。新祭坛专属完整转场未抽样，保留局部限制，不用静态冒充该转场。'),
('same_candidate','最终证据同一可追溯候选不跨版拼图','Round47/UI47/Probe47/Soak47Short指向同Review47且23文件名/SHA256集合相同；1.1.0、1072源文件，Release47同1072源码与18AA。Release桥接仅归属旁证。')]
gates=[dict(id=id,requirement=req,passed=True,evidence=e,rubricCode=f'H{i:02d}') for i,(id,req,e) in enumerate(gate_data,1)]
view=dict(reviewers={
'art_lead47':dict(scope='A1-A3/B1-B3/C1-C4',staticDifferent=w['actualViewing']['staticCount'],dynamicDifferent=3,staticSourceOpened=w['actualViewing']['originalCount'],dynamicSourceOpened=3,auxiliarySheets=23,report='Docs/ArtReviews/Round47/World/Review.json'),
'art_ui47':dict(scope='D1-D4/F2页面',**u['counts'],report='Docs/ArtReviews/Round47/UI/ObservedEvidence.json'),
'art_combat47':dict(scope='E1-E4/F1/F2战斗',staticDifferent=8,dynamicDifferent=588,originalFilesOpened=60,auxiliarySheets=47,report='Docs/ArtReviews/Round47/Combat/ObservedEvidence.json')},uniqueStaticImagesViewed=len(statics),uniqueSequenceFramesViewed=len(dynamic),sequenceFramesAvailable=1420,sequenceCount=33,uniqueOriginalFilesOpened=len(originals),uniqueAuxiliarySheetsViewed=len(sheets))
result=dict(candidate='Round47',version='1.1.0',passed=True,status='完整美术通过（限定47同候选视觉范围）',total=sum(dims.values()),max=100,dimensions=dims,dimensionMaximum=dict(A=15,B=15,C=20,D=20,E=20,F=10),items=rows,hardGates=gates,actualViewing=view,limitations=limitations,missingRequiredArtEvidence=[],additionalUnverifiedSampling=['新祭坛专属完整连续转场','所有亚帧伤害范围/完整脱战倒计时','全部随机种子/候选/组合变体','长期性能与音频人工试听'],reviewedAt=datetime.datetime.now(datetime.timezone(datetime.timedelta(hours=8))).isoformat(),independentReview='Docs/ArtReviews/Round47/IndependentReview.md',observedEvidence='Docs/ArtReviews/Round47/ObservedEvidence.json',supplementaryVisualEvidence=[dict(path='Artifacts/Soak47Short/FinalFrame.png',observer='art_combat47',wallSeconds=300.1,level=26,scope='Static readability only; no long-duration/performance conclusion',observation='玩家头帽/青脚圈及六能力HUD可辨，左下金冠成簇；维持E2=4，无新增静态硬阻断。')])
assert len(rows)==20 and result['total']==91 and min(x['score'] for x in rows)==4
Path('Docs/ArtReviewResult47.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
obs=dict(candidate='Round47',reviewers=view['reviewers'],jointCounts={k:v for k,v in view.items() if k!='reviewers'},sourceReports=['Docs/ArtReviews/Round47/World/Review.json','Docs/ArtReviews/Round47/UI/ObservedEvidence.json','Docs/ArtReviews/Round47/UI/IndependentUIReview.md','Docs/ArtReviews/Round47/Combat/ObservedEvidence.json','Docs/ArtReviews/Round47/Combat/IndependentCombatReview.md'],identityAudit='Docs/ArtReviews/Round47/IdentityAudit.json',uniqueStaticFiles=sorted(statics),uniqueDynamicFiles=sorted(dynamic),uniqueOriginalFiles=sorted(originals),uniqueSheets=sorted(sheets),originalSHA256=[dict(path=p,sha256=hashlib.sha256(Path(p).read_bytes()).hexdigest()) for p in sorted(originals)],limitations=limitations)
(base/'ObservedEvidence.json').write_text(json.dumps(obs,ensure_ascii=False,indent=2),encoding='utf-8')
md=['# Round47 独立美术总评','',f"**同版1.1.0独立美术91/100，A14/B14/C18/D19/E18/F8；20子项最低4，原10硬门槛全部通过。** 本结论只指所列候选视觉范围，不代表整项目、性能或全部功能通过。旧42/45/46历史保留，未将旧图片/成绩作为本轮评分依据。",'','## 独立分工和真实范围','',f"- art_lead47（本总评）：A/B/C，89不同静态、31静态原图与3排查原帧、23表。见[世界角色观察](World/Review.json)和[逐批记录](World/BatchNotes.md)。",'- art_ui47：D1–D4与F2页面，97不同静态、37反馈帧、30静态/8动态原图、23表；见[独立UI报告](UI/IndependentUIReview.md)。','- art_combat47：E1–E4/F1与F2战斗，588不同动态/8静态、52JPG/8PNG原文件、47表；见[独立战斗报告](Combat/IndependentCombatReview.md)。',f"- 三人联合去重：{len(statics)}不同静态、{len(dynamic)}/1420不同动态帧、33条序列；{len(originals)}不同原文件、{len(sheets)}张实际看过的辅助表。总评没有声称本人看过另两人的全部图片。完整清单与SHA见[ObservedEvidence.json](ObservedEvidence.json)。",'','## 候选与采集','', '只看本轮现成后台Windows开发Player `Builds/Review47/WanXiangExpedition.exe` 自身完整Camera.Render；版本1.1.0、Unity2022.3.62f2、RTX3060 Laptop/i7-10870H。未控制桌面/窗口/输入、未启动Player/构建、未修改原图。静态主图1600×900 Ultra/4xMSAA；UI覆盖1280×720、1920×1080、1920×1200、2560×1080、2560×1440；序列1280×720。地图种子/位置/能力等按各原图.world/.quality与序列metadata/timestamps保留，未将不同采集拼成同一局。', '', 'Full117/UI124/Probe8均finished/passed、errors=[]、原进程退出0。33序列1420帧存在；四套23文件名/SHA256清单相同，1072源码与Release47/18AA匹配。只用作归属与采集完整性，不增加美术分，详见[IdentityAudit.json](IdentityAudit.json)。', '', '## 原100分20原子项','', '| 子项 | 分/5 | 实看来源/观察与扣分 | 修订方向 |','| --- | ---: | --- | --- |']
for x in rows:
 e='；'.join(x['evidence'][:2]);md.append(f"| {x['id']} {x['title']} | {x['score']} | {x['reviewer']}；{e}。{x['observation']} | {x['revision']} |")
md+=['','完整每项证据列表见[ArtReviewResult47.json](../../ArtReviewResult47.json)。F2为页面与战斗两名实际观察的共同4分建议合并，未重复计分。','','## 原10硬门槛','','| ID | 原条件 | 结论与本轮证据 |','| --- | --- | --- |']
for g in gates:md.append(f"| {g['rubricCode']} | {g['requirement']} | 通过。{g['evidence']} |")
md+=['','H09要求实际动作/反馈连续证据，本轮33条满足；它不等于逐条页面转场穷尽。六祝福与叠加已见稳定页面，但祭坛专属完整连续转场仍未验证，明确留在限制中，不能引用本报告声称该转场已经人工看完。','','## 扣分与后续精修','','本轮九处单分扣分：A2角色局部细节、B2精英差异、C2建筑层次、C3组群重复、D3两处孤字、E2密战局部拥挤、E4火山交互低对比、F1短循环/直接收招、F2反馈节奏。均有上表原图/原帧；未发现必须先修的门槛级阻断。建议回归这些图及受影响页面/战斗，不能以删能力或停伤害清场。','','## 限制与未验证抽样','']
md+=['- '+s for s in limitations]
md+=['','缺失原门槛必需类别：无。仍未验证的局部/扩展范围：新祭坛专属完整转场、精密亚帧伤害与脱战倒计时、全种子/候选穷举、长期性能和音频人工试听；不作为已完成交付宣称。']
(base/'IndependentReview.md').write_text('\n'.join(md)+'\n',encoding='utf-8')
print(json.dumps(dict(total=result['total'],dimensions=dims,counts=view),ensure_ascii=False,indent=2))
