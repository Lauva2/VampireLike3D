from pathlib import Path
import json,re
p=Path('Docs/ArtReview.md');s=p.read_text(encoding='utf-8-sig')
start=s.index('**状态：',s.index('## 1. 当前结论'));end=s.index('\n\n选中的是',start)
s=s[:start]+'**状态：Round47 完整美术通过（1.1.0同候选视觉范围）。三名独立评审分别实看角色/世界、UI、战斗，重新按原100分20子项评为91/100：A14、B14、C18、D19、E18、F8；20项最低4，原10硬门槛全部通过。** 详见§24、[独立总评](ArtReviews/Round47/IndependentReview.md)、[联合实际观看清单](ArtReviews/Round47/ObservedEvidence.json)和[机器结果](ArtReviewResult47.json)。联合190不同静态、589/1420不同动态帧、33序列、128原文件；总评没有声称本人看过另两位全部图。六祝福/叠加层数稳定UI已覆盖，新祭坛专属完整连续转场未抽样，保留限制；同版300.1秒末帧只作静态补充，不代表长期性能、60FPS、音频人工试听或整项目通过。42/45/46记录保留，46未形成总分，不补旧成绩；不跨版拼分。'+s[end:]
row='| Round47 | 1.1.0同版完整独立复评 | Round47/UI47/Probe47；三名分域实看，短测末帧静态补充；见§24 | 91/100 | 原20项最低4、全部10门槛通过；只限视觉范围 |\n'
pos=s.index('\n## 8. ');s=s[:pos].rstrip()+'\n'+row+s[pos:]
s+='''

## 24. Round47 — 2026-10-22 / 1.1.0 三名独立评审完整复评

**91/100：A14/15、B14/15、C18/20、D19/20、E18/20、F8/10，20项均≥4，原H01–H10全部通过。** 本轮没有沿用42/45/46画面或成绩。Round46因评审网络/上下文中断未形成总分，保留其观察档案而不补旧成绩。

- art_lead47：A/B/C，89不同静态、31静态原图+3动态排查原帧，23表；三角色、八普通/精英、四Boss、四单群系近广景/水岸建筑/三处地貌、四Mixed边界/桥岸、挑战完成回访及Probe最大缩放/远坐标。自动悬浮能力枪与角色右手灰枪经三原帧区分，不认作断挂点。
- art_ui47：D1–D4与F2页面，97静态/37反馈帧、30静态+8动态原图、23表；18类页面×五画幅90矩阵全看。六祭坛祝福图标/描述、0→1、同种1→2、构筑已获×2与普通升级稳定态均清楚；两处新种子孤字“气。”/“命。”扣D3，不是缺字或关键裁切。
- art_combat47：E/F1/F2战斗，588不同动态/8静态、52JPG+8PNG原文件、47表；33序列均有实际样本，四Boss95死亡帧、玩家15倒地帧、96密战帧、五反馈37帧全经表看。补300.1秒短测26级末帧，玩家/六能力HUD可辨、左下金冠拥挤，维持E2=4。
- 联合去重190不同静态、589/1420不同动态帧、33序列、128不同原文件、93张实际查看辅助表。总评只汇总另两人的独立报告，不宣称亲自看过其全部图片；联系表仅完整画幅缩小索引，原图保持不变。
- Round47 Full117、UI124、Probe8均finished/passed/errors=[]/原进程退出0。四套23项构建文件名/SHA256集合一致，1072源码与正式Release47/18AA一致；仅作归属旁证，不增加美术分。

完整20行/扣分/修订方向与全部10门槛：[IndependentReview.md](ArtReviews/Round47/IndependentReview.md)。逐人清单/联合去重/原图SHA：[ObservedEvidence.json](ArtReviews/Round47/ObservedEvidence.json)；身份完整性：[IdentityAudit.json](ArtReviews/Round47/IdentityAudit.json)；机器结果：[ArtReviewResult47.json](ArtReviewResult47.json)，共享结果同步同内容。

限制：三人均没有桌面控制、启动Player/构建或修改原图；只看现成后台Player Camera.Render/连续runtime帧。部分动作固定12fps、受控治疗，角色目标固定/不死、Boss攻击采集Boss不死；其余效果/死亡/反馈按墙钟采样，不等于60FPS。H08只指所见视觉时序/范围匹配，不声称穷尽亚帧伤害、所有碰撞或完整脱战倒计时。新祭坛完整打开→取消/重开→选择→排队升级恢复连续流尚未抽样，H09根据已有33条连续动作/反馈判定，不能引用通过结论声称该新转场已完整人工验证。未穷举全随机种子/全部任务强化滚动行/所有组合。300.1秒末帧不证明持续动态、自然难度、两小时或长期性能；本节只记录47视觉通过，不是整项目验收。
'''
p.write_text(s,encoding='utf-8')
Path('Docs/ArtReviewResult.json').write_bytes(Path('Docs/ArtReviewResult47.json').read_bytes())
print('Updated current conclusion, appended history and section24; shared result matches47.')
