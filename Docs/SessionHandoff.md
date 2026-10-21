# 跨电脑接续记录

记录时间：2026-10-21 09:34（Asia/Shanghai）。这是开发中检查点，**不是成品验收通过**。

## 接续时先读

依次阅读根目录 AGENTS.md，以及 Docs/GameDesign.md、Architecture.md、DevelopmentWorkflow.md、ImplementationStatus.md、ArtReview.md。用户要求依照工作流直到成品，并由独立美术子代理评分超过90且通过原量表全部门槛。

**禁止控制桌面、窗口、鼠标或键盘。** 只使用独立后台 Player、程序化游戏输入、日志、原始 Camera.Render 截图。不要恢复早期窗口自动化授权。

## 工程与玩法约束

- Unity 2022.3.62f2，Windows x64，内置渲染管线，UGUI中文界面，Addressables 1.22.3 已安装并锁定。
- 运行时加载统一经 AssetService，正确保持和释放 AA 租约。全部自制 prefab 位于 Assets/Art/Prefabs；保留所有 .meta/GUID。
- GameManager 管整体流程，UIManager 管UI生命周期与模态输入。
- 俯视自动攻击；WASD移动，Shift短暂无敌闪避，右键标记，E交互，Tab构筑，M地图，Esc暂停。
- 无武器/被动槽限制，同武器路线并存。任务解锁角色、能力和选项；永久属性可成长。
- 无限种子地图：林地→荒漠→雪原→火山，四Boss首胜后开放混合。主动挑战Boss、可脱战、自由撤离或无限生存。
- 首发冻结：3角色、6能力、42成长选项、20任务、4Boss、4群系+混合、3事件，明快卡通Q版。

## 已有实际验证

- 69个prefab；106项编辑器检查通过。
- Round33/UI33/Probe33均已finished、passed=true、errors=[]。完整回归含真实场景遮挡渐隐→结算清理检查。
- Campaign31独立新档自然推进通过：1408.7秒、6局、1次真实死亡、20任务、0错误；不强制属性、生命、生成、时钟或解锁。它使用Review31，不能冒充后续版本同版自然回归。
- Campaign28曾在801.5秒因OcclusionFader退出阶段空引用失败。当前已修复，失败原始记录保留。
- Release31与Review31的950源码/资源文件和18个AA包完全匹配；正式程序集测试驱动剔除检查通过。
- rc31压缩包已做哈希/CRC检查，并在新目录后台冷启动进入正常菜单。该范围仅正式包启动与本地AA初始化，不是完整人工试玩。
- Builds/Packages/WanXiangExpedition-1.0.0-rc31.zip是**较旧候选包**；最新开发Player在Builds/Review33。Builds/Windows目前仍为Release31。

## 美术当前状态与真正下一步

- 最近已写入报告的完整正式评分为Round24的85/100，未通过；最终以ArtReview.md后续实际记录为准，不用口头预计分数冒充通过。
- 子代理对Round31的局部实际结论：C1/C2/C3/C4各4，C16/20；密战交网和全屏红罩已实质改善，完整总分仍待正式记录。
- Round32/33已修建筑避水、平整地基、岸边装饰避让、道路连接圆角及建筑门面朝向道路。
- **以下仅已讨论，尚未实施**：将火山台地/火山口/熔岩岸组成连贯主景；四主题挑战点做顺地貌的不对称场地边缘与侧入口；挑战完成后只改变阵纹状态，避免大块场地跟随旧逻辑缩小。
- 具体检查：PathNetwork.Configure的跨河向北道路仍使用固定末段控制点(0,0,13)，在共享端点发生横向偏移时可能形成接缝小三角；应统一末段切线并用实机复核。
- Boss地标生成在Assets/Scripts/Editor/ArtFactoryLandmarks.cs；当前仍是六个较等距构件。地形生成在ArtFactoryTerrain.cs。世界落位在Runtime/Gameplay/World/WorldService.cs。
- 场景C与战斗E需各≥18，总分≥91，A/B≥13，D≥18，F≥8，每子项≥4，同一候选版完整覆盖。继续让独立子代理实际看图评分，不能自己改分或放宽量表。

## 正在运行的测试（仅原电脑）

- Soak09：Review33真实7200秒长测，原电脑PID37580，目录Artifacts/Soak09。记录时只有约1230秒采样、没有Summary.json，**尚未通过**。
- 原电脑关闭、睡眠或终止进程后，测试进程不会随工程文件迁移。新电脑先检查Summary.json/Errors.txt；未完成时重新运行，不能把两台机器的分段时间相加冒充连续两小时。
- Soak05/06/07/08都未完成，原数据及取消/中断记录保留，不计通过。
- 其他用户Unity进程不得结束。只操作能够核对可执行文件路径、测试输出目录与启动时间的自有后台进程。

## 新电脑环境与复现

1. 用Unity Hub安装同版本和Windows Build Support，加入项目根目录；首次重建Library需要等待。
2. 迁移Assets（含全部.meta）、Packages、ProjectSettings、Docs、Tools、AGENTS.md、README.md。若保留Git历史，还需迁移隐藏.git；当前有大量未提交修改，仅clone旧HEAD会缺失最新工作。
3. 新源码迁移包不含Library/Temp/Logs/.build/Player和Artifacts原始证据。需要保留旧验收证据时，另外迁移Artifacts/Round33、UI33、Probe33、Campaign31、Soak09及对应BuildIdentity/SourceIdentity；美术原帧在Captures/Sequences中。
4. Tools/Build.ps1使用固定Unity安装路径，若新电脑不同请调整。Python审计工具需Python与Pillow；PackageCandidate.ps1里的Git路径当前为原机器Codex运行时路径，换机需改为可用Git或接受源码提交信息不可用。
5. 构建示例：`powershell -ExecutionPolicy Bypass -File Tools/Build.ps1 -Mode Development`。更新生成内容用GenerateDevelopment；生成文件同步必须先比较、备份，禁止整目录强制覆盖主工作树。
6. 验证示例：`powershell -ExecutionPolicy Bypass -File Tools/RunVerification.ps1 -Stage Full -Evidence Artifacts/NewMachineFull -Background`；其他Stage为UI/Terrain/Campaign/Soak，长测SoakSeconds=7200。每次使用全新证据目录。
7. 最终完成美术、同版回归/自然推进/长测后，再构建正式版、核对源码/AA内容、冷启动、打包与归档。PackageCandidate.ps1的Final开关会检查门槛，不能以候选ZIP代替成品。

## 新会话可直接发送

> 继续这个Unity项目。先阅读AGENTS.md和Docs/SessionHandoff.md，再核对文档所列最新实际证据。沿既定工作流直到完成成品，美术由独立子代理按原量表评分超过90并通过全部门槛。禁止控制桌面；只做后台Player、程序化游戏输入、日志与原始渲染验证。不要把计划、旧版或未完成长测当作当前版本已通过。
