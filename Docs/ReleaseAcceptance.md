# 1.0.0 验收记录（冻结候选42，同版门槛通过）

当前冻结Review42/Release42。实际测试为i7-10870H / RTX3060 Laptop 6GB、Unity2022.3.62f2、内置管线、Addressables1.22.3；不沿用旧基准PC结果。全部操作均为后台Player、进程内程序输入、日志及原始Camera.Render，无桌面控制。全部同版运行验收及独立美术门槛已通过。最终游戏/源码/原始证据ZIP均已实际完成并通过逐文件审计，见Artifacts/Delivery42.json和ZIP侧车。包内文档保留归档前快照，本机此处追加实际交付结果。

## 候选42当前证据

| 检查 | 实际结果 | 证据 |
| --- | --- | --- |
| 编辑器配置/组合/世界/存档 | 113项通过，10000次预热成长查表0字节分配 | Artifacts/EditorChecks42.json |
| 完整后台回归 | 95项finished/passed、0错误 | Artifacts/Round42/Verification.json |
| 五画幅中文UI | 72项finished/passed、0错误 | Artifacts/UI42/Verification.json |
| 资源异常/取消回收/地形 | 8项finished/passed、0错误 | Artifacts/Probe42/Verification.json |
| 地形原图像素 | 5张底色0、inactive为空 | Artifacts/Probe42/TerrainPixelAudit.json |
| 主树/开发身份 | 1068文件一致 | Artifacts/Workspace42Identity.json |
| 开发/正式内容身份 | 1068源文件、18AA包一致 | Artifacts/Release42BuildMatch.json |
| 正式配置/本地依赖 | 18本地包、剔除3个诊断驱动 | Artifacts/Release42Audit.json |
| 正式便携菜单启动/重启 | 两次通过，仅菜单范围 | Artifacts/ReleaseSmoke42/Summary.json |
| 正式外置输入玩法/读档 | 191.8秒林地Boss首胜结算，5.9秒重启荒漠；非开发/非诊断、0错误 | Artifacts/FormalGameplay42/Summary.json、Instrumentation.json及两个Phase原图 |
| 音频PCM技术审计 | 14文件通过；未人工试听 | Artifacts/AudioAudit42.json |
| 固定深构筑预检 | 180.2秒/16586真实渲染/退出0；87.04–96.15FPS，p95最高15.27ms；属性/强化匹配 | Artifacts/Soak42Preflight/PreflightAssessment.json；仅固定新种子短预检，不代持续成长长测 |
| 新档自然全部解锁 | 1486.7秒、7局、2死亡、全部20任务、四Boss/Mixed、0错误 | Artifacts/Campaign42/Summary.json、ProgressionCrossCheck.json、verification-profile.json |
| 独立原量表美术 | 91分、A14/B14/C18/D19/E18/F8、20项min4、原10门槛全通过 | Docs/ArtReviewResult42.json、ArtReviews/Round42/IndependentReview.md、ObservedEvidence.json |
| 连续7200秒压力 | 实际7200.2秒/原PID退出0/0错误；240完整窗口/712734渲染；最低72.24FPS、末15分钟中位93.74；记录范围内性能/内存验收通过 | Artifacts/Soak42/Summary.json、ProcessExit.json、Acceptance.json及Docs/Soak42Review.md |
| 最终归档 | 三ZIP实际完成；游戏221清单文件、源码1846文件、原始证据2497文件均审计通过，整包SHA再次核对；游戏ZIP解压副本两次正式菜单启动/重启通过 | [交付清单](../Artifacts/Delivery42.json)、Package42Audit、Package42SmokeAudit及三个ZIP侧车 |

独立末段补充实看394级FinalFrame，未见新增直接美术阻断，保留91分/原10门槛；单帧不代替33动态序列复评。见ArtReviews/Round42/SoakFinalFrameReview.md。

21/240压力窗口p95超过16.67ms，最高20.27ms，最长单帧78.53ms；不宣称每帧60FPS或默认限速节奏通过。Windows私有峰1172.34MB/最后1165.43MB，Unity分配203.05–294.71MB；池逐帧峰3799随构筑扩大，句柄56–68/区块28–36。完整分段、生命周期判断及边界见[压力专项](Soak42Review.md)。冻结后再次核对1068源码/18AA，34项证据身份检查通过（Final42系列报告）。

正式玩法为隔离副本增加外置输入/观察程序集及两份启动登记JSON，其余原文件逐一哈希一致。只提交进程内输入和有效购买，不覆盖属性、生命、生成、时钟或解锁；原正式分发目录未修改。这不是人工试玩，正常视角美术证据来自完整回归/UI/Probe原图。

未限速诊断吞吐与正常游戏的60帧限速节奏是不同测量范围。固定预检不恢复旧世界种子、不持续强化，不能作严格A/B或最终7200秒。GPU帧时间和Player私有内存不可用时保留NA；Windows进程内存另表记录。已知未执行范围为人工操作手感/主观试听和旧基准机复测；无限世界仅声明实际种子与路线覆盖。

## 前候选39及更早历史（不继承）

| 检查 | 实际结果 | 证据 |
| --- | --- | --- |
| 编辑器配置、组合、世界与存档检查 | 110项通过 | Artifacts/EditorChecks39.json |
| 后台完整回归 | 95项完成通过，0错误；含新增经验保值/聚合/原点/磁吸检查 | Artifacts/Round39/Verification.json |
| 五画幅中文UI矩阵 | 72项完成通过，0错误 | Artifacts/UI39/Verification.json |
| 资源失败、取消回收与地形专项 | 8项完成通过，0错误 | Artifacts/Probe39/Verification.json |
| 专项原图相机底色与无效区块 | 5张均未检出 | Artifacts/Probe39/TerrainPixelAudit.json |
| 可编辑主树与开发快照 | 1068文件完全匹配 | Artifacts/Workspace39Identity.json |
| 正式/开发源码与AA内容 | 1068文件、18AA包完全匹配 | Artifacts/Release39BuildMatch.json |
| 正式包本地依赖、剔除三个诊断驱动 | 通过 | Artifacts/Release39Audit.json |
| 全新便携目录正式启动及重启 | 两次通过；仅菜单/本地AA/便携存档 | Artifacts/ReleaseSmoke39/Summary.json |
| 正式配置外置输入玩法及重启 | 192.7秒、1局、0死亡、林地Boss首胜并结算；重启5.8秒进入已解锁荒漠，均0错误 | Artifacts/FormalGameplay39/Summary.json、Instrumentation.json、Phase1/Phase2原图 |
| 独立美术 | 92分，A14/B14/C18/D20/E18/F8；20项均≥4，原10门槛全通过 | Docs/ArtReviewResult39.json、ArtReviews/Round39/IndependentReview.md |
| 全新档自然解锁 | 完成1162.4秒、5局、0死亡、20任务、四Boss首胜与Mixed、0错误 | Artifacts/Campaign39/Summary.json、verification-profile.json |
| 连续7200秒压力与长距离探索 | 4110秒/137样本/245241实际渲染后停止，后段约59并多次低59，性能不予接受；未完成7200秒 | Artifacts/Soak39/Interrupted.json、Analysis.json；没有通过记录 |
| 最终ZIP/源码/原图证据归档 | 等待全部门槛后执行 | 尚无最终交付包 |

回归全部使用后台 Player、程序化游戏输入、日志和原始 Camera.Render 图像，未控制桌面。自然推进与受控压力分开：前者不强制属性、回血、生成、时钟或解锁；后者从后期组合开始、持续治疗并增加压力生成，真实时间与每帧1080p渲染单独记录。

实际测试机为 i7-10870H / RTX3060 Laptop 6GB；旧基准机i5-14400F/RTX5060未在本次重新测试。压力批处理Player显式使用多线程图形参数，在BuildIdentity/ProcessIdentity与日志留证；正式项目配置m_MTRendering=1。无法取得的GPU计时与Player私有内存保留NA，另用Windows Get-Process采样，不混为同一项。

正式玩法范围：隔离副本添加外置输入/观察程序集及两份启动登记JSON，原有游戏程序集、AA和其它原文件逐一哈希一致；development=false、diagnosticMode=false。只提交进程内移动/闪避/交互和合法购买，不强制属性、生命、生成、时钟或解锁。这是带外置验证组件的正式配置玩法；分发目录不含该组件。原始Camera.Render近景用于流程佐证，不能替正常游玩视角的美术覆盖。

历史：38完整独立美术92分且原门槛通过、功能/自然/正式配置通过，但Soak38后段反复54–58FPS，在4230秒/141样本/249432实际渲染后停止，未完成7200秒，无Summary/Acceptance通过。Soak37在1470秒后因性能停止。两者保留Interrupted与Analysis，不合并时长，也不作为39通过依据。

限制：人工操作手感和音频主观试听未执行；14个PCM技术审计不能代替试听。无限世界仅覆盖所列种子、采样与压力路线，不宣称穷尽地图。现有shader编译警告保留，构建无编译错误；是否影响表现由实际原图和独立美术记录判断。
