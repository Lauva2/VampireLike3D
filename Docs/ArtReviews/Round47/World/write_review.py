from pathlib import Path
import json,datetime
r=Path('Docs/ArtReviews/Round47/World')
m=json.loads((r/'SheetManifest.json').read_text())
seen={k:v for k,v in m.items() if not any(k.endswith(f'geography-0{i}.jpg') for i in (3,4,5))}
originals=['character-adventurer','character-elementalist','character-swordsman']+[f'boss-closeup-{i}' for i in range(4)]+['enemy-3-elite','enemy-0-elite','enemy-1-normal']+[f'waterside-architecture-{i}' for i in range(4)]+['mixed-boundary-0-1','mixed-boundary-0-3','mixed-boundary-1-2','mixed-boundary-1-3']+[f'sanctuary-completed-{i}' for i in range(4)]+['mixed-lava-shore-close','geography-3-0','geography-2-2','sanctuary-close-3']
originals=[f'Artifacts/Round47/Captures/{n}.png' for n in originals]+[f'Artifacts/Probe47/Captures/{n}.png' for n in ['probe-max-zoom-ultrawide','probe-20-1','load-failure-terrain-forest']]
data=[
('A1',5,'大头短肢与俯视轮廓清晰',['character-adventurer','character-elementalist','character-swordsman','10-biome-0','10-biome-1','10-biome-2'],'三角色头身关系、短肢和分离鞋手明确，尖帽/发型在正常俯视仍可辨。','无本项必修。'),
('A2',4,'面部、服饰、装备有设计与辨识度',['character-adventurer','character-elementalist','character-swordsman'],'瞳孔高光/眉嘴颊色与围巾、腰带、扣件、袍摆、法杖/手枪/背剑完整；冒险者纵长眼较夸张，手指/肘与褶皱仍简。','微调眼比例并补少量关节与衣褶层次。'),
('A3',5,'明快卡通的造型材质色光协调',['character-adventurer','terrain-detail-0','terrain-detail-1','terrain-detail-2','terrain-detail-3','mixed-boundary-1-3'],'绿金冰蓝灰紫四环境与角色明快配色、统一描边/柔和阴影协调，雪白底仍有轮廓，无全屏泛白。','无本项必修。'),
('B1',5,'全部首发角色模型材质挂点完成',['character-adventurer','character-elementalist','character-swordsman'],'三角色面部/发型/服装/鞋/持械均有完成造型，角色在菜单近景和正常场景保持身份，没有缺件或粉材质；动态另引Combat。','无本项必修。'),
('B2',4,'普通怪与精英轮廓动作标识区分',[f'enemy-{i}-{k}' for i in range(4) for k in ['normal','elite']],'绿团/长耳/蓝壳角/紫蘑菇四基型轮廓与色彩可分，精英金冠、附角或额外球体清晰；精英主要共享同一冠语言，差异化仍可加强。','给各类精英补独有盔甲或姿态细节，保持冠作为共同标识。'),
('B3',5,'四Boss独立造型体量与完整表现',[f'boss-closeup-{i}' for i in range(4)]+[f'boss-{i}' for i in range(4)],'古树树冠根手、沙虫分节尖牙、冰兽白毛蓝角、熔像玄武岩橙熔核各有独立轮廓；正常镜头体量高于小怪且不混同地标；动态表现由Combat报告补足。','无本项必修。'),
('C1',5,'四单群系独立地貌与装饰语言',[f'geography-{i}-{j}' for i in range(4) for j in range(3)],'林地河流/树丛岩脊、荒漠沙丘/岩台绿洲、雪原冰湖/白脊、火山熔河/岩桥/柱束均在三处近广景明确，四种语言可辨。','无本项必修。'),
('C2',4,'地面植被岩石建筑遗迹层次尺度',[f'waterside-architecture-{i}' for i in range(4)]+['geography-3-0','geography-2-2'],'地表斑块、大小树石、桥、屋顶/基座、门柱和主题遗迹形成层次；局部建筑可见面细节少、门柱和平整地表偏简。','补少量屋檐/门框、材质分区和遗迹边缘层次。'),
('C3',4,'区块接缝遮挡重复感与视觉一致',['world-floating-origin','geography-wide-3-0','geography-wide-1-1','mixed-lava-shore-close'],'近广景及Probe远处/最大缩放未见空洞或硬断面，桥路连贯、遮挡后玩家可辨；沙丘岩台棕榈/火山柱组的重复感及规整圆头河岸仍可见。','增加组群方向/大小变化与非对称岸线；静态不证明所有种子碰撞。'),
('C4',5,'混合过渡地标路径与可交互物',['mixed-boundary-0-1','mixed-boundary-0-3','mixed-boundary-1-2','mixed-boundary-1-3','mixed-lava-shore-close']+[f'sanctuary-completed-{i}' for i in range(4)],'四边界有宽色带及地物混入，路跨界连续；水火终点分离，桥岸可读。主题挑战圈/门柱、旗/箱/碧色祭坛分工明确，完成回访保留场景并熄圈清提示。','无本项必修；火山直接文字对比局部问题归E4。')]
rows=[]
for id,score,title,e,obs,rev in data:
 rows.append(dict(id=id,score=score,max=5,title=title,evidence=[f'Artifacts/Round47/Captures/{n}.png' for n in e],observation=obs,revision=rev,reviewer='art_lead47'))
obj=dict(round=47,reviewer='art_lead47',scope='A1-A3/B1-B3/C1-C4 independent static inspection',rows=rows,dimensions={'A':14,'B':14,'C':18},subtotal=46,actualViewing=dict(sheets=list(seen),staticImages=sorted(set(p for v in seen.values() for p in v)),originalFiles=originals,sheetCount=len(seen),staticCount=len(set(p for v in seen.values() for p in v)),originalCount=len(originals),dynamicFramesPersonallyViewed=0),limitations=['联系表是索引，补29原图；view_image对2560图可能缩放，不声称逐像素1:1。','无限地图仅本轮种子和采集位置抽样，没有遍历全部随机种子、地物摆放与碰撞。','未控制桌面/启动Player/构建；未亲自交互试玩。','动态E/F由另一独立评审实际观察；不声称本人查看其全部动态帧。'],notes='Docs/ArtReviews/Round47/World/BatchNotes.md')
(r/'Review.json').write_text(json.dumps(obj,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({k:obj['actualViewing'][k] for k in ['sheetCount','staticCount','originalCount']}))
