import json
from pathlib import Path
b={'id':4,'scope':'火山三近广景、熔河/柱组/建筑、熔像与完成回访','sheets':[f'Docs/ArtReviews/Round52/World/biome3-{j:02}.jpg' for j in range(4)],'originals':[f'Artifacts/Round52/Captures/{n}-3.png' for n in ['boss-closeup','waterside-architecture','sanctuary-completed','sanctuary-close']],'observation':'新看4表/15静态，补4原图。紫灰底与黑色柱束/环形岩台、火苗和亮橙熔河形成火山独立语言，桥横跨熔河且路径连贯；三处广景无可见空块或矩形硬拼缝。岩台/柱束与扁平小炉有尺度层次，柱组重复及平整建筑面仍宜精修。熔岩巨像原图见黑灰分块肩甲/手指/足、橙角/前臂金带/胸核，体量与普通蓝壳虫和玩家明确不同，静态无粉材/缺件。完成回访保留场景与金线柱，中心召唤圈暗化、E提示消失。sanctuary-close-3原尺寸深青字直接落于紫灰底仍可读，对比偏弱，记录交E4分域评审，不当中文缺字/硬裁切。只见四个原单群系，新增两地与Mixed待后续52原图。'}
Path('Docs/ArtReviews/Round52/World/batch4.json').write_text(json.dumps(b,ensure_ascii=False,indent=2),encoding='utf-8')
