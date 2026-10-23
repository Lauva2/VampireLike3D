import json
from pathlib import Path
from datetime import datetime,timezone
out=Path('Docs/ArtReviews/Round54/World')
initial={'candidate':'Round54','version':None,'reviewer':'/root/art_lead47','status':'prepared; no54raw image has been observed yet; no score or complete gate conclusion','recordedAt':datetime.now(timezone.utc).isoformat(),'actualViewing':{'sheets':[],'staticImages':[],'originalFiles':[],'sheetCount':0,'staticCount':0,'originalCount':0,'dynamicFramesPersonallyViewed':0},'sourceMetadata':[],'batches':[],'rubric':'Docs/ArtReview.md original100points/20integer items/10hard gates unchanged; all expanded content included','performanceDeferredByUser':True,'limits':['No52/53 images/scores/viewing counts used as54 evidence.','Wait for actual54sources and traceable same-candidate identity, then inspect and log each batch.','Performance is explicitly deferred by user; do not wait for or require7200-final-frame for this UI/art round and do not claim performance completion.','No runtime/original-image changes,desktop control,emulator operation,Player launch or build by reviewers.']}
(out/'ObservedEvidence.json').write_text(json.dumps(initial,ensure_ascii=False,indent=2),encoding='utf-8')
(out/'ReviewScope.md').write_text('''# Round54 独立角色与世界评审范围

当前仅准备评审流程，尚未看过54原图，不计分、不作通过结论。52/53分数、原图和观看数量均不迁移。原ArtReview.md §1–6的100分20项/10门槛保持；当前扩容内容全部归原项。

本域A1–A3/B1–B3/C1–C4需实际覆盖三角色近景/正常视角、八普通与八精英、六Boss、九武器、六单群系各三近广景、地标建筑与水岸/道路、六处挑战完成回访、Mixed六区域与本轮实际边界及熔岸、Probe远坐标/最大视野、浮动原点与经验高低质。辅助表只作全画幅索引，另开关键原图；每批实际观察落盘，逐源记录SHA/尺寸/quality/world/seed/角色/能力/时间/site，未提供的元数据明确未知。

UI由art_ui47独立D/F2页面观察，重点为本轮五画幅及13类popup连续原帧，按实际时间戳判断起点、底板/文字/控件同步、背景、结束稳定状态，功能断言不替代视觉。Combat独立E/F1及F2战斗贡献，六Boss完整表现也用于B3佐证。合并总评明确三人各自实际观看，不能称总评本人看过全部其他分域原图。

本轮实现说明仅作为检查线索：统一0.24秒unscaled/cubic缓出、内容整体轻缩放上移淡入，全屏背景不动，动画时禁交互且挡射线，同页刷新不重播，切换/关闭/禁用恢复，toast输入透明。是否实际顺滑与一致须看54新序列，不从源码或52/53通过推定。

用户明确暂缓性能测试，本会话不操作MuMu或桌面。本轮不要求、不等待7200秒末帧，也不将UI功能/美术通过称性能通过或最终性能交付。共享ArtReview.md/ArtReviewResult.json只在54实际证据合并形成结论后按父代理后续范围收尾，所有旧历史保留。
''',encoding='utf-8')
for name in ['make_character_sheets.py','make_ready_world_sheets.py']:
 s=(Path('Docs/ArtReviews/Round52/World')/name).read_text(encoding='utf-8-sig').replace('Round52','Round54').replace('Probe52','Probe54').replace('fresh52','fresh54').replace('ready52','ready54')
 (out/name).write_text(s,encoding='utf-8')
print('Prepared54scope and empty evidence record;0viewed,score unset;performance deferred.')
