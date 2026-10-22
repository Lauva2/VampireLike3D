# Round42 最终长测末帧独立视觉补充

日期：2026-10-22 06:37（Asia/Shanghai）。评审人：独立美术子代理。依据原 [ArtReview量表及门槛](../../ArtReview.md) 与本候选 [完整独立复评](IndependentReview.md)，只读现成记录及原图；未启动Player/构建，未操作桌面。

**结论：这张末帧没有显示可直接确认的新增美术硬阻断。保留Round42原91/100及原10门槛结论，分数不变。** 中心玩家、中文HUD及可见资源完整；周缘暖色效果拥挤、细红预警对比弱的局部问题仍在。单帧不重新证明持续危险识读、连续动态、性能或整项目通过。

## 原图与同候选身份

实际以原始1920×1080整幅查看：[Artifacts/Soak42/FinalFrame.png](../../../Artifacts/Soak42/FinalFrame.png)。未裁去HUD或周缘效果。

SHA256：`E6A62C86D3820C0ED19D3BEF6EDD2048878D9AF0415109A57025B06602DF45F5`，独立计算与提供值相同。

将 [Soak42/BuildIdentity](../../../Artifacts/Soak42/BuildIdentity.json) 与 [Round42/BuildIdentity](../../../Artifacts/Round42/BuildIdentity.json) 的文件名/SHA256映射逐项比较，23项完全相同；两者均指Review42。将两套 [Soak42/SourceIdentity](../../../Artifacts/Soak42/SourceIdentity.json) / [Round42/SourceIdentity](../../../Artifacts/Round42/SourceIdentity.json) 的路径/SHA256映射逐项比较，1068项完全相同。此处比较保存的候选快照，不以身份记录推导视觉质量。

## 先读的真实构筑与采集范围

- [Summary](../../../Artifacts/Soak42/Summary.json)：实际墙钟7200.2秒，240样本、errorCount=0；1920×1080/4xMSAA，每帧实际渲染，frameRateLimit=-1，fixedImportedBuild=false，controlledHealingAndUnlockedBuild=true。
- [ProcessExit](../../../Artifacts/Soak42/ProcessExit.json)：原PID38564，启动UTC20:35:19.2819364、观察退出UTC22:35:23.6955904，exitCode=0。[Errors.txt](../../../Artifacts/Soak42/Errors.txt) 实存0字节。这些只记录本图的来源/完成上下文，不是本评审的性能验收。
- [CurrentBuild](../../../Artifacts/Soak42/CurrentBuild.json) 与 [BuildSamples末条](../../../Artifacts/Soak42/BuildSamples.jsonl) 完全一致：墙钟7200.0137秒、游戏时间8999.9629秒、394级、quality=5、实际拾取半径12.4、seed=1106767528；六能力入口[0,1,2,3,4,5]，41个强化ID、强化等级总和434。
- 读取并核对240条BuildSamples：均六能力、未导入固定构筑、frameRateLimit=-1；首样本30.026秒为6级/强化等级总和46，末样本394级/434。它们是构筑记录，**没有当作240张视觉帧观看**。
- [WorkloadAnnotation](../../../Artifacts/Soak42/WorkloadAnnotation.json) 的“RUNNING_NOT_A_PASS”是启动期说明，其列出的持续治疗、解锁混合图/能力、额外刷怪和未限速范围仍须保留；实际结束以Summary/ProcessExit为准。未导入固定构筑不等于自然难度或人工试玩。
- 本图HUD149:59是游戏内时间，不是实际墙钟时长。未限速吞吐也不等于已验收帧率、帧时间或限速后的帧节奏。

## 实际可见观察与相关门槛

| 范围 | 本帧实际观察 | 判断及限制 |
| --- | --- | --- |
| E2玩家定位 | 中心棕发头部、躯干和青脚圈清楚；左前飞剑/灵体扇形很密，但未把玩家完全遮掉。 | 本帧可定位；不证明前后时刻持续可见。E2维持4，不加分。 |
| E2/E3/E4危险与效果 | 周缘大片黄橙弧/爆破形状与精英金冠相挤；中心上方细红直线预警只见局部边线，对比偏弱。己方暖色效果不是敌方危险范围证明。没有四Boss完整预警—释放周期。 | 保留拥挤/对比扣分。单帧不足以断言所有有效危险范围连续无遮挡，也不能据此确认持续掩盖或严重时序错位；没有追加“持续危险通过”证明。 |
| D2/D3/D4中文HUD | 394级、300/300、149:59、资源672777/762569、击败634051和六能力中文名称/图标可读，右上地图、右下暂停/构筑/地图按钮均在画幅内。 | 本帧没有缺字方框、关键裁切或核心HUD出界。原720p句号独行扣分仍保留。 |
| 完成资产/资源缺失门槛 | 可见雪地、冰脊/雪树、湖面、各色敌人与精英、青色拾取物、武器/灵体和效果有材质。 | 本帧未见粉材质、大片缺面、相机底色裂洞或UI失效；不能覆盖画外区域/全部随机状态。 |
| 原连续动画、命中及闪避门槛 | 新增证据只有一张静态末帧。 | 本次不重验时间维度；原42的33序列/589不同帧完整复评结论保留，不以末帧替代。 |

未从本帧直接确认新的门槛阻断，**不调整原20项、A14/B14/C18/D19/E18/F8或91分**。对危险连续时序的保留是单帧证据边界，不是放宽原门槛；若另有连续证据显示持续遮挡或严重预警错位，须按原门槛重新判定。

## 记录范围

原完整复评和177级预检补图维持其历史观看清单。此次实际新增1张静态原图，累计打开原文件 **81个（47 PNG、34 JPG）**，辅助表及补图合计实际不同静态图192张；不同动态帧仍 **589/1415**，没有增加连续观看量。身份、原图哈希和逐项观察已写入 [ObservedEvidence.json](ObservedEvidence.json) 的 `postSoakFinalFrameReview` / `supplementaryVisualEvidence`，最新累计量见 `latestViewingTotals`。

只记录末段静态视觉复核。帧率、帧时间、内存、长测性能与整项目验收由各自独立记录负责，本报告不作通过结论。
