# 万象远征

Unity 2022.3.62f2 制作的中文、单人离线 3D 俯视自动攻击游戏。魔法、枪械与剑修自由混搭，不设武器/被动槽位；包含任务解锁、四群系与混合无限地图、主动 Boss 挑战和自由撤离。

**1.0.0冻结42的同版验收已通过。** Full95/UI72/Probe8、自然全部解锁、正式配置玩法/重启及实际7200.2秒后台成长压力完成；独立美术91分、全部原门槛通过。测试范围与限制见[验收记录](Docs/ReleaseAcceptance.md)和[压力专项](Docs/Soak42Review.md)。三个交付ZIP已实际生成、逐文件审计通过，并验证游戏ZIP解压后的正式启动/重启；完整路径、SHA256与测试范围见[交付清单](Artifacts/Delivery42.json)。ZIP内文档为归档前快照，本机此处补齐归档后结果。

交付：

- [游戏ZIP](Builds/Packages/WanXiangExpedition-1.0.0.zip)：完整解压后运行WanXiangExpedition.exe。
- [可编辑源码ZIP](Builds/SourceArchives/WanXiangExpedition-Source-1.0.0.zip)：1846文件及.meta/包锁/配置/工具。
- [原始证据ZIP](Builds/EvidenceArchives/WanXiangExpedition-Evidence-1.0.0.zip)：2497文件，含原图、日志、独立评审和历史失败。

三个ZIP旁均有完整性侧车。工作树保留未提交修改，源码归档提供精确可恢复快照。

## 打开与构建

使用指定 Unity 版本打开本目录。运行 `Assets/Scenes/Boot.unity`，确保 Addressables 使用已构建的本地内容。正常运行所有内容统一经 AssetService 加载；所有作者预制体位于 `Assets/Art/Prefabs`。GameManager 负责整体流程，UIManager 负责界面。

从 PowerShell 在工程根目录执行：

```powershell
# 在 .build/Project 副本生成内容、执行检查并构建开发版，再同步资源与 .meta
.\Tools\Build.ps1 -Mode GenerateDevelopment

# 检查并构建正式配置；自动测试驱动不参与正式编译
.\Tools\Build.ps1 -Mode Release

# 独立存档的后台 Player 回归，不操作桌面
.\Tools\RunVerification.ps1 -Stage Full
.\Tools\RunVerification.ps1 -Stage UI
.\Tools\RunVerification.ps1 -Stage Terrain
.\Tools\RunVerification.ps1 -Stage Campaign
.\Tools\RunVerification.ps1 -Stage Soak -SoakSeconds 7200 -MultithreadedGraphics

# 正式配置：全新便携目录菜单启动/重启；外置输入/观察组件只加入隔离副本
.\Tools\RunReleaseSmoke.ps1
.\Tools\RunFormalFlow.ps1
```

压力测试默认限速60。显式增加 `-SoakFrameRateLimit -1` 测量1080p实际未限速吞吐，报告记录 `frameRateLimit=-1`，不能称作正常游戏限速下的帧节奏。`-SoakUpgrades <快照>` 仅用于固定深构筑短预检，Final拒绝用它代替正常持续成长7200秒。

长测在另一个后台终端用 `Tools/MonitorPlayerMemory.ps1 -Evidence <本次证据目录>` 约45秒采集Windows内存；启动器已在进程启动后立即采第一点。监测脚本逐次核对PID、启动时间和路径，不能将观察器正常结束当作Player退出码或性能通过。

构建工具支持 `-UnityPath` 参数或 `UNITY_EDITOR_PATH` 环境变量，否则检查工程内 `.build/UnityEditor/Editor/Unity.exe`、旧安装目录和 Unity Hub 的同版本目录，并校验版本必须为 `2022.3.62f2`。构建输出在 `Builds/Development` 和 `Builds/Windows`；原始日志、截图、连续帧和报告在 `Artifacts`。后台验证仅对开发版有效，真实玩法与受控诊断的边界在实施记录中说明。

正式验证工具默认读取 `Builds/Windows`，自动创建带时间戳的全新隔离副本和证据目录；也可显式传 `-PlayerFolder`、`-ColdFolder`、`-Evidence`。外置玩法工具同样支持 `-UnityPath`，仅使用该版本附带的编译器，不启动编辑器或控制桌面。压力性能测试应在其他验证 Player 与构建退出后单独运行；`-Background` 只表示启动后返回，不能作为通过证据。

`Tools/PackageCandidate.ps1` 要求显式提供本次 `-VerificationFolder`、`-UIFolder`、`-TerrainFolder`；`-Final` 还要求同版本的 `-CampaignFolder`、`-SoakFolder`、`-FormalGameplayFolder` 和独立 `-ArtReviewRecord`。最终包检查源码身份、原美术门槛、实际长测时长及独立性能/内存结论，不默认引用历史候选报告。源码与原始证据分别由 `ArchiveSource.ps1`、`ArchiveEvidence.py` 归档，并逐文件核验哈希；报告通过之前不制作最终交付包。

## 文档

- [跨电脑接续记录与下一步](Docs/SessionHandoff.md)
- [游玩与存档说明](Docs/PlayerGuide.md)
- [确认的设计](Docs/GameDesign.md)
- [工程约束](Docs/Architecture.md)
- [完整工作流](Docs/DevelopmentWorkflow.md)
- [首发内容范围](Docs/ContentScope.md)
- [实际验证状态](Docs/ImplementationStatus.md)
- [独立美术验收](Docs/ArtReview.md)

字体的 SIL OFL 许可证随源文件保存在 `Assets/Art/UI/Fonts/OFL.txt`。项目保留所有生成资源、配置、Addressables 设置与 `.meta`，可重新构建。
