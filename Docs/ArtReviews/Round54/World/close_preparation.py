import json
from pathlib import Path
out=Path('Docs/ArtReviews/Round54')
d=json.loads((out/'World54FreshObserved.json').read_text(encoding='utf-8'))
(out/'World/ObservedEvidence.json').write_text(json.dumps({'candidate':'Round54','status':'Preparation superseded by completed fresh actual world review; no former scores reused.','reviewer':'/root/popup54_world','authoritativeEvidence':'Docs/ArtReviews/Round54/World54FreshObserved.json','report':'Docs/ArtReviews/Round54/World54Fresh.md','counts':d['counts']},ensure_ascii=False,indent=2),encoding='utf-8')
(out/'World/ReviewScope.md').write_text('# Round54 角色与世界范围\n\n原准备状态已由54真实新观察替代，正式分评见 [World54Fresh.md](../World54Fresh.md)，逐图SHA/尺寸/元数据与42张实际查看辅助表清单见 [World54FreshObserved.json](../World54FreshObserved.json)。\n\n此域A14、B14、C18。153不同静态，23原PNG，0个人动态；六Boss/八敌动作由独立Combat54贡献，完整原20项/10门槛等待UI+Combat分评后合并。原量表不变；52/53图、分和计数均未沿用。性能按用户要求延后，不要求7200末帧；未控制桌面/模拟器、启动Player/构建、改运行时或原图。\n',encoding='utf-8')
