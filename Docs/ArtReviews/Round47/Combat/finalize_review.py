from pathlib import Path
import json,hashlib
from PIL import Image
root=Path('E:/UnityProject/VampireLike3D');out=root/'Docs/ArtReviews/Round47/Combat'
e=json.loads((out/'ObservedEvidence.json').read_text('utf8'))
sources={s for sheet in e['sheets'] for s in sheet['sources']}|{x['path'] for x in e['originals']}
e['uniqueDynamicFramesObserved']=sum('/Sequences/' in p for p in sources)
e['uniqueStaticImagesObserved']=sum('/Sequences/' not in p for p in sources)
e['sheetCount']=len(e['sheets']);e['originalFileCount']=len(e['originals'])
e['sequences']=[]
for d in sorted((root/'Artifacts/Round47/Sequences').iterdir()):
    if not d.is_dir():continue
    files=sorted(d.glob('*.jpg'));actual=[p.stem for p in files if str(p.relative_to(root)).replace('\\','/') in sources]
    meta=json.loads((d/'metadata.json').read_text('utf8'))
    e['sequences'].append({'name':d.name,'availableFrames':len(files),'metadata':meta,'observedFrameIds':actual,'sampled':len(actual)<len(files),'originalSize':list(Image.open(files[0]).size)})
e['availableDynamicFrames']=sum(s['availableFrames'] for s in e['sequences'])
e['verification']=json.loads((root/'Artifacts/Round47/Verification.json').read_text('utf8'))
e['processExit']=json.loads((root/'Artifacts/Round47/ProcessExit.json').read_text('utf8'))
e['identityEvidence']=[{'path':str(p.relative_to(root)).replace('\\','/'),'sha256':hashlib.sha256(p.read_bytes()).hexdigest().upper()} for p in [root/'Artifacts/Round47/BuildIdentity.json',root/'Artifacts/Round47/SourceIdentity.json']]
e['supplementalShortSoak']={'image':'Artifacts/Soak47Short/FinalFrame.png','summary':json.loads((root/'Artifacts/Soak47Short/Summary.json').read_text('utf8')),'scope':'单张原图的静态可读性补充；没有由末帧判断动态/60FPS/两小时性能。'}
e['limitations']=['只读本轮现成后台Player原始Camera.Render图/运行序列；未控制桌面、窗口、输入，未启动Player或构建。','联系表保留完整画幅并缩小，用于索引和顺序观察；细节以另开原始帧判断。没有播放完整正常速度录像，没有声称33条逐帧全看。','移动/密战/角色/四Boss攻击为固定12fps模拟采集且受控治疗；角色动作对象固定且不死，Boss攻击对象不死。不能据此判断自然难度、60fps或性能。','effect、死亡和feedback使用实际runtime timestamps，requestedIntervalSeconds不是实际帧率；原CSV保留，实际间隔存在约0.06秒等变化。','E3只判断所见视觉形状/阶段/落点与效果区域，不穷尽随机种子或独立量化亚帧伤害判定；技术断言不转换为美术分。','本子报告负责E1-E4、F1及F2战斗部分；全部20项/10门槛由独立总评汇总。','祭坛20图稳定态及新增UI由另一评审负责；本子报告没有见祭坛专属打开-选择-排队升级恢复完整连续片段。','Soak47Short仅单张末帧，持续治疗/解锁内容；HUD34:59不等于实测墙钟300.1秒。']
(out/'ObservedEvidence.json').write_text(json.dumps(e,ensure_ascii=False,indent=2),encoding='utf8')
S='Artifacts/Round47/Sequences/';C='Artifacts/Round47/Captures/'
rows=[
{'id':'E1','score':5,'max':5,'title':'三武器及联动区分与命中层次','evidence':[S+'effect-flame/0002.jpg',S+'effect-explosion/0002.jpg',S+'effect-lightning/0008.jpg',S+'effect-iceburst/0002.jpg',S+'effect-arc/0003.jpg',S+'effect-embers/0003.jpg',S+'effect-rune/0003.jpg',S+'effect-sand/0003.jpg','Combat/ObservedEvidence.json: all 11 effect sequences, 12 samples each'], 'observation':'11效果各12样帧及各一原帧均实看：火球橙色投射与瓣状冲击、枪械细金色弹道及星状命中、青白实体剑刃/成列剑雨、雷击折线、冰片爆发、青色弧、符圈、砂点、叶片及暗余烬有形色差异；展开到消退阶段可见。正常视距同时存在六能力，没有只靠HUD判定发射。','revision':'本项无扣分；保留短寿命和有限屏幕覆盖。'},
{'id':'E2','score':4,'max':5,'title':'高密战斗辨识','evidence':[S+'late-density-combinations/0018.jpg',S+'late-density-combinations/0037.jpg',S+'late-density-combinations/0045.jpg',S+'late-density-combinations/0072.jpg',S+'late-density-combinations/0095.jpg']+[C+f'late-density-{i}.png' for i in range(5)]+['Artifacts/Soak47Short/FinalFrame.png'], 'observation':'96密战连续原帧全部经6张联系表观察，五地图密战另开原图；玩家帽/头部和青脚圈可持续追踪，透明遮挡物前后能继续定位，粉敌弹、青晶体、实体剑和金色子弹可辨。0018冰圈/剑雨/精英冠挤在玩家附近，0045树与建筑附近冠环和尾迹相压，短测末帧左下金冠成簇，类别层级尚非5分的稳定清爽。','revision':'减少冠环和己方尾迹局部视觉权重，给玩家脚圈保留更稳定的明度间隔；保留全部获得能力及实际伤害。'},
{'id':'E3','score':5,'max':5,'title':'四Boss及精英危险预警','evidence':[S+f'boss-attacks-{i}/0018.jpg' for i in range(4)]+[S+'boss-attacks-0/0030.jpg',S+'boss-attacks-1/0030.jpg',S+'boss-attacks-2/0024.jpg',S+'boss-attacks-3/0066.jpg','Combat/ObservedEvidence.json: four attacks, 25 samples each; 0018-0021 adjacent originals each'], 'observation':'四Boss每条25样帧覆盖12秒起手/预警/生效/余波；树根三圆、沙虫定向条、霜兽主震击圆及周围落点、巨像多喷发圆均有范围形状，临近生效增强边线/斜纹/叹号，攻击余波落在已见区域。精英矩形冲锋条在五地图/密战可辨。巨像0066多圈重叠仍能区分粉红危险边、橙爆发与青己方弧，玩家脚圈可见，未见持续掩盖危险。','revision':'本项在所见视觉范围无扣分；仍可精修最早淡线的对比。此5分不声称亚帧判定/全部种子已量化验证。'},
{'id':'E4','score':4,'max':5,'title':'闪避、受击、状态、危险边界与交互提示','evidence':[S+'character-move-dodge/0001.jpg',S+'character-move-dodge/0002.jpg',S+'character-move-dodge/0003.jpg',S+'character-move-dodge/0037.jpg',S+'character-move-dodge/0038.jpg',S+'boss-attacks-2/0030.jpg',S+'late-density-combinations/0072.jpg',C+'boss-0.png',C+'sanctuary-close-3.png'], 'observation':'相邻闪避样本有俯身/浅色变化和青脚部反馈，受击是边缘渐变、中央保持可读；冰冻蓝染与暖火状态区别可见。Boss集火标记/血条、危险圈/条和开战脱战说明可读。火山sanctuary-close-3的深青开启提示直接落在紫灰背景，虽仍可读，局部对比弱于其他UI，扣1分。没有据开战文字宣称已独立看见完整脱战倒计时。','revision':'给世界交互文字添加浅底或细描边，统一不同群系背景的对比；保留轻量受击与无敌闪避表现。'},
{'id':'F1','score':4,'max':5,'title':'角色与Boss动画衔接','evidence':[S+'role-closeup-adventurer/0019.jpg',S+'role-closeup-elementalist/0019.jpg',S+'role-closeup-swordsman/0019.jpg',S+'character-move-dodge/0037.jpg',S+'player-defeat/0004.jpg']+[S+f'boss-death-{i}/0010.jpg' for i in range(4)]+['Combat/ObservedEvidence.json: role-action, role-closeup, move/dodge, player-defeat and boss attacks/deaths'], 'observation':'三职业侧前各16/48及正常行动各12/60样帧能见转身、步态、后坐/施法/剑诀，配移动闪避与受击样本；玩家15帧倒地到结果、四Boss全部95死亡帧及起手/收招已见，树/虫/霜兽/巨像有独立侧倾、缩低或压倒并消隐。动作功能齐全一致，但攻击仍偏小幅短循环，收招回中性较直接，死亡多为整体倾倒/缩放，未达更自然重量转移的5分。','revision':'增加攻击预备/回落的重心、肢体跟随与死亡分段，而非加大白闪。'},
{'id':'F2','score':4,'max':5,'title':'战斗与整体反馈一致性（战斗部分建议分，汇总需合并UI）','evidence':[S+'feedback-task-unlock/0000.jpg',S+'feedback-upgrade-reward/0000.jpg',S+'player-defeat/0009.jpg','Combat/ObservedEvidence.json: all five feedback sequences, 37 frames'], 'observation':'五反馈37帧全部观察，面板从淡入到稳定，任务解锁/升级后Toast、经验聚合与倒地结算沿用奶油底/深青/短环色系，所见没有明显闪烁、粉材或大片排序错误。奖励主要依赖通用Toast/环，世界庆祝和页面节奏较直接，建议4分。未以程序断言代替祭坛新转场的视觉观看。','revision':'提升拾取/升级/解锁的轻重层次和世界反应；补祭坛打开-选择-升级恢复连续片段作为今后完整转场回归。'}]
(out/'Scores.json').write_text(json.dumps({'candidate':'Round47','version':'1.1.0','scope':'E1-E4/F1 and F2 combat contribution','rows':rows,'dimensionE':18,'F1':4,'F2CombatRecommendation':4,'completeArtVerdict':None},ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'sheets':e['sheetCount'],'originals':e['originalFileCount'],'observedDynamicFrames':e['uniqueDynamicFramesObserved'],'availableDynamicFrames':e['availableDynamicFrames'],'staticImages':e['uniqueStaticImagesObserved'],'sequences':len(e['sequences']),'checks':len(e['verification']['checks'])},ensure_ascii=False))
