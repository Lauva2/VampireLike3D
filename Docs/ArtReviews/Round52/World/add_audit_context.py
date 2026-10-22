import json
from pathlib import Path
p=Path('Docs/ArtReviews/Round52/World/Review.json');d=json.loads(p.read_text(encoding='utf-8-sig'))
d['terrainAuditContext']={'assessment':'Artifacts/TerrainCoverage52Assessment.json','selectedRawAudit':'Artifacts/TerrainCoverage52Audit.json','broadRawAudit':'Artifacts/AllCaptureClear52Audit.json','selected60ImagesReportedClearPixels':0,'selected60ImagesReportedInactiveChunks':0,'scope':'Read actual52 assessment only. Broad194-image result includes intentional studio camera-clear backgrounds, retained separately; not treated as world-hole verdict. Non-scoring corroboration; C3 stays4 from actual repetitions/occlusion-edge observations.'}
p.write_text(json.dumps(d,ensure_ascii=False,indent=2),encoding='utf-8')
with Path('Docs/ArtReviews/Round52/World/BatchNotes.md').open('a',encoding='utf-8') as f:f.write('\n## 非评分采集/地形旁证核对\n\n从52实际JSON核对Full244/UI222/Probe8均finished/passed/0错误/原退出0，29构建文件名/SHA一致。47条源序列2505帧只是存在，不称本人全看。52 TerrainCoverage52Assessment：真实地理近远/六区域六实际边界/水岸建筑60图等底色像素总0、inactive0；广域194图含模型工作室故意保留底色，AllCaptureClear52Audit原失败单独保留，不当作世界洞。该旁证不提高C3，仍按实际重复组群/硬遮挡开口局部扣分。三位UI/Combat未合并，不提前写全局通过；深构筑/7200末帧仍待。\n')
print('52 terrain/collection context appended without scoring change')
