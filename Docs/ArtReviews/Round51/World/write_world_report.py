import json
from pathlib import Path
p=Path('Docs/ArtReviews/Round51/World')
r=json.loads((p/'Review.json').read_text(encoding='utf-8-sig'))
lines=['# Round51 独立角色与世界评审','', '候选：Round51 / 1.2.0；评审人：`/root/art_lead47`。按 `Docs/ArtReview.md` 原100分20子项与10门槛，不修改分母；新增六地图、八敌种及六Boss均纳入原项。这里仅提交A/B/C分域；六Boss完整表现还需Combat独立连续观察佐证，未形成完整总评。','', '实际观察：40张辅助联系表内146张不同静态，并另开46张原图；本评审人没有观看本轮动态帧。逐源SHA256、尺寸、seed/角色/能力/时间和缺失元数据见[Review.json](Review.json)；逐批事实见[BatchNotes.md](BatchNotes.md)。辅助表保留全画面，只作索引；原图未改。','', '| 子项 | 分数/5 | 实际观察与修改方向 |','| --- | ---: | --- |']
for row in r['rows']:
    ev='；'.join('`'+x+'`' for x in row['evidence'])
    lines.append(f"| {row['id']} {row['title']} | {row['score']} | {row['observation']} 修改：{row['revision']} 证据：{ev} |")
lines += ['', '**分域合计：A14/15，B14/15，C18/20，46/50。** 所有扣分来自本轮实际观察；未使用或迁移47/50及更旧图片和成绩作为51评分依据。B3完整动作由另一名独立评审补充，总评会明确归属。','', '三角色近景与正常俯视、八普通/八精英、新三武器、六Boss近景、六单群系各三处近广景/建筑水岸/挑战前后、Mixed六区域和六边界、Probe远坐标/最大缩放、经验高低质量及新增四敌普通/精英正常战斗静态均覆盖。完整可点击原源路径在Review.json，表到原图映射在CharacterSheetManifest.json和WorldSheetManifest.json。','', '玩家周围独立悬浮能力展示体与手持装备同时存在；本轮原图可见手持枪/杖/背剑，不把外侧能力体的位置间距定性为断挂点。六Boss主题与体量层级可辨，但本报告不以静态替代其起手、攻击和死亡。','', '地形专项：`mixed-region-snow.png`唯一等底色像素(1321,740)位于浅金弹丸边缘附近，完整原图未见可见连续洞/缺块/裂边。色彩巧合只是推断，不能确定像素来源。保留 `Artifacts/TerrainCoverage51Audit.json` 原失败与1像素结果；独立原图观察、SHA和5×5邻域见[TerrainPixelReview.json](TerrainPixelReview.json)。不据此宣布旧启发式审计通过。','', '同版旁证见[IdentityAudit.json](../IdentityAudit.json)：Round51/UI51/Probe51三套29文件名/SHA一致；1309源文件与24AA核对，243/215/8项完成通过、原退出0。这些日志不增加视觉分。','', '限制：只审查已列实际视角和种子；不能证明所有随机地图/碰撞/流送状态。静态无法证明连续遮挡与动作。角色/武器展示图若无world sidecar则seed/游戏时间记未知；有sidecar的每图独立保留，不泛称同一seed。原图通常1600×900、Ultra/4x；最大Probe为2560×1080，工具显示时可能缩至2048×864，未声称每张全图1:1显示。长测与独占九武器深构筑末帧尚待父代理提供，未算完成或性能通过。', '']
(p/'IndependentWorldReview.md').write_text('\n'.join(lines),encoding='utf-8')
print('World report written; rows',len(r['rows']))
