from pathlib import Path
import json
p=Path('Docs/ArtReviews/Round47/World/Review.json');o=json.loads(p.read_text(encoding='utf-8'));v=o['actualViewing'];v['originalSequenceFiles']=[f'Artifacts/Round47/Sequences/role-closeup-adventurer/{n}.jpg' for n in ['0019','0000','0035']];v['dynamicFramesPersonallyViewed']=3;v['totalOriginalFilesOpened']=32;o['rows'][3]['evidence']+=v['originalSequenceFiles'];o['rows'][3]['observation']+=' 外侧自动能力枪独立悬浮，三帧补查看到手持灰枪仍在，不属断挂点；实现只辅助归因。';p.write_text(json.dumps(o,ensure_ascii=False,indent=2),encoding='utf-8')
