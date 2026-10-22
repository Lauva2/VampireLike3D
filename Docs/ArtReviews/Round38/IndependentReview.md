# Round38 独立完整美术验收

评审日期：2026-10-22（Asia/Shanghai）。评审人：独立美术验收子代理 `/root/art_review38`。

**结论：Round38 完整美术通过，92/100。A14、B14、C18、D20、E18、F8；20个子项均不低于4分。** 本轮按 [原量表与硬门槛](../../ArtReview.md) 重新观察评分，未沿用 Round34 或 Round37 分数。结论仅适用于本轮有明确身份的美术证据，不代表正式版完整功能或2小时性能通过。

## 候选身份与证据

来源为 Windows development Player `Builds/Review38/WanXiangExpedition.exe` 的 `Camera.Render` 原始完整画幅与同一实时 Canvas。Unity 2022.3.62f2，quality=2（精致），GPU RTX 3060 Laptop，CPU i7-10870H。未控制桌面、窗口、鼠标或键盘，未启动新 Player 或构建。

Full / UI / Probe 三份 BuildIdentity 的23个文件 SHA256 一致，包含EXE、Assembly-CSharp、AA catalog/settings/link和18个bundle。Assembly-CSharp：`7058F372A40E10ECE6D36048486501FAB6C38761DEB2A0464EDBE5BB0CB52C38`。源/AA身份桥接见 [Release38BuildMatch](../../../Artifacts/Release38BuildMatch.json)：1066源码文件与18包无差异；这是身份佐证，正式版自身运行另行验收。37→38只改shader的事实没有用来继承任何美术分。

| 同版运行报告 | 结束 UTC | 结果 |
| --- | --- | --- |
| [Full](../../../Artifacts/Round38/Verification.json) | 2026-10-21 16:13:35 | 91 checks，0 errors，passed |
| [UI](../../../Artifacts/UI38/Verification.json) | 2026-10-21 16:15:09 | 72 checks，0 errors，passed |
| [Probe](../../../Artifacts/Probe38/Verification.json) | 2026-10-21 16:16:12 | 8 checks，0 errors，passed |

运行检查只补充行为及身份，未直接兑换为美术分。人物/能力/游戏时间由各完整画面HUD及同名world/bars资料追溯；各次采集为不同受控局面，不宣称统一随机种子。地图种子在21-map与UI map中保留，世界坐标/区块状态保存在同名 `.world.json`。现成证据的复核入口为 [SheetManifest](SheetManifest.json) 和 [ReviewSampling](ReviewSampling.json)，按各source打开原图或按帧号顺序查看，不需启动程序。

**实际观看：72张辅助表全部逐张查看，涵盖188张静态原图和33条序列的572张采样帧。另开56张原图（32 PNG、24 JPG），其中额外16张Boss相邻帧不在表内，最终实际看过588张不同序列帧。** 原序列共1433帧，固定12fps；不声称已逐帧看过全部记录。全部实际观看文件及逐序列帧号完整列于 [ObservedEvidence.json](ObservedEvidence.json)。

覆盖三角色、4普通/4精英、四Boss、四单群系与混合；四群系地貌正常/近景/广角、挑战前/近景/完成后、实际近水建筑及支路、混合四组边界/熔岩岸线、浮动原点和远距探针；菜单/选择/成长任务/设置/HUD/技能/升级/地图/暂停/加载/错误/死亡和胜利结算。UI38五画幅70页图均已查看，画幅为1280×720、1920×1080、1920×1200、2560×1080、2560×1440。

## 原20项逐项评分

表内链接指向现存证据；JSON中的路径相对工程根目录。每项只摘取代表证据，全部原始文件和观看清单见JSON。

| 原子项 | 得分 | 代表证据 | 实际观察、扣分与修改方向 |
| --- | ---: | --- | --- |
| A1 大头短肢与俯视轮廓清晰 | 5/5 | [character-adventurer.png](../../../Artifacts/Round38/Captures/character-adventurer.png)、[character-elementalist.png](../../../Artifacts/Round38/Captures/character-elementalist.png)、[character-swordsman.png](../../../Artifacts/Round38/Captures/character-swordsman.png)；完整索引见JSON | 三角色头身比、短肢和饱满发型/帽檐在近景与正常俯视均清楚；帽子、发型、武器和足圈共同保持位置辨识。 本项无扣分。 |
| A2 面部、服饰、装备设计 | 4/5 | [character-adventurer.png](../../../Artifacts/Round38/Captures/character-adventurer.png)、[character-elementalist.png](../../../Artifacts/Round38/Captures/character-elementalist.png)、[character-swordsman.png](../../../Artifacts/Round38/Captures/character-swordsman.png)；完整索引见JSON | 眼睛、眉睫、发型、围巾、披衣、帽饰与枪/杖/剑建立身份。近景手掌与袖口、腰带附件仍简化，面部细节层级相近。 **扣分修订：**细化握持手型、袖口转折和各角色不同的面部小表情，保留现有俯视清楚的大轮廓。 |
| A3 卡通造型、材质、色光协调 | 5/5 | [10-biome-0.png](../../../Artifacts/Round38/Captures/10-biome-0.png)、[10-biome-1.png](../../../Artifacts/Round38/Captures/10-biome-1.png)、[10-biome-2.png](../../../Artifacts/Round38/Captures/10-biome-2.png)；完整索引见JSON | 明亮奶油/青绿体系贯穿人物和UI，各群系有独立色域；火山暗岩与橙光、雪原白蓝保持层次。冻结蓝染、闪光和多实例原色并存，未见串色、粉材或持续白片。 本项无扣分。 |
| B1 全部首发角色模型、材质、挂点 | 4/5 | [character-adventurer.png](../../../Artifacts/Round38/Captures/character-adventurer.png)、[character-elementalist.png](../../../Artifacts/Round38/Captures/character-elementalist.png)、[character-swordsman.png](../../../Artifacts/Round38/Captures/character-swordsman.png)；完整索引见JSON | 三角色均有完整头发/帽子、衣物、手脚与对应枪/杖/剑，动态中挂点跟随。近景握柄与手掌关系偏简单，共用身体转折较明显。 **扣分修订：**为三类握持增加适合武器的手型与腕部角度，补衣摆/肩部的角色化转折。 |
| B2 普通怪与精英辨识 | 5/5 | [enemy-0-normal.png](../../../Artifacts/Round38/Captures/enemy-0-normal.png)、[enemy-0-elite.png](../../../Artifacts/Round38/Captures/enemy-0-elite.png)、[enemy-1-normal.png](../../../Artifacts/Round38/Captures/enemy-1-normal.png)；完整索引见JSON | 绿叶软体、橙色尖角奔跑体、蓝色甲壳足肢、紫色菌帽施法体有不同轮廓和运动；精英金冠/角饰、血条与尺寸建立第二层辨识，密战中仍能找到精英。 本项无扣分。 |
| B3 四Boss独立造型与体量表现 | 5/5 | [boss-closeup-0.png](../../../Artifacts/Round38/Captures/boss-closeup-0.png)、[boss-attacks-0](../../../Artifacts/Round38/Sequences/boss-attacks-0)、[boss-death-0](../../../Artifacts/Round38/Sequences/boss-death-0)；完整索引见JSON | 古树树冠木躯根手、沙虫节段巨口、霜原角/白毛/爪、熔岩岩甲与裂隙光各自完整；与普通敌体量有级差，起手、攻击、死亡皆有实录。 本项无扣分。 |
| C1 四群系地貌与装饰语言 | 5/5 | [geography-0-0.png](../../../Artifacts/Round38/Captures/geography-0-0.png)、[geography-0-1.png](../../../Artifacts/Round38/Captures/geography-0-1.png)、[geography-0-2.png](../../../Artifacts/Round38/Captures/geography-0-2.png)；完整索引见JSON | 林地树簇/河桥/花木、荒漠沙丘台地/绿洲棕榈遗迹、雪原冰湖冰脊/白松、火山熔岩流/玄武岩柱/岩桥分别成立。火山主坑与不规则开口野地已有主从。 本项无扣分。 |
| C2 地面植被岩石建筑层次尺度 | 4/5 | [waterside-architecture-0.png](../../../Artifacts/Round38/Captures/waterside-architecture-0.png)、[sanctuary-0.png](../../../Artifacts/Round38/Captures/sanctuary-0.png)、[waterside-architecture-1.png](../../../Artifacts/Round38/Captures/waterside-architecture-1.png)；完整索引见JSON | 近水建筑全座位于干地平基，支路接入；挑战点有连续后左地形与前中后尺度。部分大块平地和三柱/石簇仍偏疏、模块间细层次较薄。 **扣分修订：**在不侵占战斗走位面的前提下，用少量地表高度/密度变化连接大块空地与岩簇，丰富柱簇主次。 |
| C3 接缝遮挡重复感与碰撞视觉 | 4/5 | [mixed-lava-shore-close.png](../../../Artifacts/Round38/Captures/mixed-lava-shore-close.png)、[mixed-boundary-0-3.png](../../../Artifacts/Round38/Captures/mixed-boundary-0-3.png)、[world-floating-origin.png](../../../Artifacts/Round38/Captures/world-floating-origin.png)；完整索引见JSON | 边界与远距探针未见底色裂口、无效地块；移动/密战遮挡时头部与足圈仍在。地表小斑/三件石簇/柱簇的重复节奏在广角仍可察觉。 **扣分修订：**增加小簇间距、朝向和组团轮廓变化，优先打散长距离同类重复，继续保持遮挡消隐和道路通畅。 |
| C4 混合过渡地标路径交互探索 | 5/5 | [mixed-boundary-0-1.png](../../../Artifacts/Round38/Captures/mixed-boundary-0-1.png)、[mixed-boundary-0-2.png](../../../Artifacts/Round38/Captures/mixed-boundary-0-2.png)、[mixed-boundary-0-3.png](../../../Artifacts/Round38/Captures/mixed-boundary-0-3.png)；完整索引见JSON | 四组实际混合边界有宽度与植被过渡，熔岩终点有收束岸线；地图符号与实景宝箱/精英/挑战/房屋呼应。侧路绕阵心、踏石接路，完成后阵纹熄灭与提示清除、景物留存，探索前后状态清楚。 本项无扣分。 |
| D1 完整页面视觉体系 | 5/5 | [ui-menu-1280x720.png](../../../Artifacts/UI38/Captures/ui-menu-1280x720.png)、[ui-selection-unlocked-1280x720.png](../../../Artifacts/UI38/Captures/ui-selection-unlocked-1280x720.png)、[ui-progress-1280x720.png](../../../Artifacts/UI38/Captures/ui-progress-1280x720.png)；完整索引见JSON | 菜单、选择、永久成长/任务、设置、暂停、加载、错误、死亡/胜利结算均有奶油卡片、青绿按钮、金色强调和图形图标体系；错误能表达内容失败及恢复状态。 本项无扣分。 |
| D2 HUD升级技能信息层级 | 5/5 | [ui-hud-1280x720.png](../../../Artifacts/UI38/Captures/ui-hud-1280x720.png)、[ui-upgrade-2560x1080.png](../../../Artifacts/UI38/Captures/ui-upgrade-2560x1080.png)、[ui-skills-1280x720.png](../../../Artifacts/UI38/Captures/ui-skills-1280x720.png)；完整索引见JSON | 生命、时间、Boss条、资源、小地图和六能力各有固定区域；升级卡描述/机制标签/稀有度/选择按钮分层，技能树点数/节点/锁因清楚。列表滚动区域与页脚可分辨。 本项无扣分。 |
| D3 中文完整与长文排版 | 5/5 | [ui-selection-locked-1280x720.png](../../../Artifacts/UI38/Captures/ui-selection-locked-1280x720.png)、[ui-progress-1280x720.png](../../../Artifacts/UI38/Captures/ui-progress-1280x720.png)、[ui-skills-1280x720.png](../../../Artifacts/UI38/Captures/ui-skills-1280x720.png)；完整索引见JSON | 原图复核长中文、解锁原因、价格和任务行，未见方框、关键字裁切或文字跑出按钮。底部列表的部分行受正常滚动遮罩约束，页脚与滚动条清楚；小字号可读。 本项无扣分。 |
| D4 要求画幅与交互状态 | 5/5 | [UI38-00.png](UI38-00.png)、[UI38-11.png](UI38-11.png)、[Verification.json](../../../Artifacts/UI38/Verification.json)；完整索引见JSON | 实际查看UI38全部70张五画幅页面（1280×720、1920×1080、1920×1200、2560×1080、2560×1440），含锁定/解锁、选中、禁用、价格/图标；主模态未拉伸、越界或覆盖必要操作。 本项无扣分。 |
| E1 武器与联动区分命中层次 | 5/5 | [effect-flame](../../../Artifacts/Round38/Sequences/effect-flame)、[effect-slash](../../../Artifacts/Round38/Sequences/effect-slash)、[effect-lightning](../../../Artifacts/Round38/Sequences/effect-lightning)；完整索引见JSON | 橙色火焰/余烬、枪弹电弧、青白飞剑/斩弧、蓝色冻结/碎冰、圆形符阵有不同轨迹和扩散尺度；联动形成命中、扩散、余波层次，不靠同一白闪代替。 本项无扣分。 |
| E2 高密度玩家目标弹丸拾取辨识 | 4/5 | [late-density-0.png](../../../Artifacts/Round38/Captures/late-density-0.png)、[late-density-4.png](../../../Artifacts/Round38/Captures/late-density-4.png)、[late-density-combinations](../../../Artifacts/Round38/Sequences/late-density-combinations)；完整索引见JSON | 密战中玩家帽/头、青色足圈、金色精英冠/目标圈、掉落及红色危险边线仍能持续定位，全部六能力可见。飞剑和冰弧集中穿过脚下时有短时局部拥挤。 **扣分修订：**收窄交叠冰弧/剑尾的中心亮带，按屏幕覆盖密度调整纯视觉透明度，保留全部能力、弹丸与伤害。 |
| E3 四Boss及精英预警时序范围 | 5/5 | [boss-attacks-0](../../../Artifacts/Round38/Sequences/boss-attacks-0)、[boss-attacks-1](../../../Artifacts/Round38/Sequences/boss-attacks-1)、[boss-attacks-2](../../../Artifacts/Round38/Sequences/boss-attacks-2)；完整索引见JSON | 四Boss录制均可见起手、红圈/长矩形逐步建立、攻击落在相同区域、回收；逐张补看各Boss0018–0021及0026/0039确认早段和兑现段。沙虫矩形顺身向，古树/雪怪/熔岩圈阵与动作对应，未见严重范围偏离。精英密战边线仍可辨。 本项无扣分。 |
| E4 无敌受击状态边界交互清晰 | 4/5 | [character-move-dodge](../../../Artifacts/Round38/Sequences/character-move-dodge)、[effect-frost](../../../Artifacts/Round38/Sequences/effect-frost)、[late-density-combinations](../../../Artifacts/Round38/Sequences/late-density-combinations)；完整索引见JSON | 闪避足圈/亮度变化、冻结蓝染、命中短闪、边缘受击反馈、红危险边线和E交互提示均有证据，未见持续过曝或实例染色污染。火山挑战近景提示与暖色岩面接近，局部对比还有提升空间。 **扣分修订：**为世界交互提示补稳定浅/深描边或小底板，特别保证火山暖色背景中的文字/按键与景物分离。 |
| F1 移动闪避攻击受击死亡衔接 | 4/5 | [character-move-dodge](../../../Artifacts/Round38/Sequences/character-move-dodge)、[role-action-adventurer](../../../Artifacts/Round38/Sequences/role-action-adventurer)、[role-action-elementalist](../../../Artifacts/Round38/Sequences/role-action-elementalist)；完整索引见JSON | 记录中移动摆动、转向/闪避、攻击、命中与倒地、Boss起手/回收都有时间推进；无固定姿态冒充整段。关节转动和回中仍偏程序化，肩臂/腰身的柔和过渡有限。 **扣分修订：**为攻击起手和收招增加少量身体跟随与缓动，改善腕肩回中，保持预警兑现时点不变。 |
| F2 页面反馈与世界一致性 | 4/5 | [feedback-pause-open](../../../Artifacts/Round38/Sequences/feedback-pause-open)、[feedback-skills-open](../../../Artifacts/Round38/Sequences/feedback-skills-open)、[feedback-task-unlock](../../../Artifacts/Round38/Sequences/feedback-task-unlock)；完整索引见JSON | 页面淡入可到完整不透明终态，领取/解锁/升级与拾取光点沿用相同色彩语言；未见粉材、破网格、持续悬浮或明显跳闪。奖励和世界小反馈仍以简单缩放/亮点为主，节奏层次较平。 **扣分修订：**给任务领取、解锁与世界拾取增加不同的短促缓动节奏和尾部收束，避免扩大遮挡范围。 |

维度合计：**A14/15、B14/15、C18/20、D20/20、E18/20、F8/10；总计92/100。**

C的通过来自本轮实际画面：四挑战点的侧路避开阵心，踏石接路、后左连续地貌形成场地围合；完成态阵纹关闭、提示清除、实体地貌留存。四群系实际近水建筑完整落在干地平基且有支路；混合过渡和熔岩岸尾都有收束。场景C2/C3仍因大块空地、簇状资产层次和广角重复节奏扣分，没有把“未见裂缝”直接当作场景满分。

新shader重点回归：三角色原色/近景面部、普通与精英并列、冻结蓝染、受击短闪、密战96帧、树冠遮挡和四Boss预警。未见实例间串色、错误ID染色、粉色/空材质、持续消隐、玩家长期不可见或预警被大片闪白覆盖。局部剑/冰弧交叠和火山世界提示对比各扣一分，不扩大为不存在的阻断。

## 全部硬门槛

| 原门槛 | 本轮结论与证据 |
| --- | --- |
| 总分≥91；A/B≥13，C/D/E≥18，F≥8；20项各≥4 | **通过**：92；14/14/18/20/18/8；最小4 |
| 冻结首发内容全实机覆盖，无未评分类别 | **通过**：三角色、四普通/精英族、四Boss、四群系+混合、完整UI和五画幅均有本轮原图/运行序列，见观看清单 |
| 无占位资产、粉材、缺贴图、明显破损模型 | **通过**：已观看全范围未见；程序化造型已有完整主题/层次，不以制作方式代替完成度判断 |
| 中文不缺字、不裁关键文字、按钮可读、核心UI不出界 | **通过**：五画幅页面矩阵及720长中文原图复核；滚动列表局部行遮罩属于有效滚动区域 |
| 密战/Boss持续定位玩家和危险；不得靠停火/删能力/关闭伤害造干净画面 | **通过**：96帧密战全部查看，六能力持续可见；Full同版断言六能力共存和伤害联动。玩家受控治疗与动作对象不死限制另列，未作自然生存证明 |
| 所有Boss预警/前摇与闪避窗口有对应表现；无持续遮挡、范围严重错位 | **通过**：四攻击序列、各18–21相邻帧及26/39兑现帧；角色移动闪避序列及窗口内外伤害断言。视觉没有严重偏移或持续掩盖 |
| 动画反馈有连续证据，非单张姿态推断 | **通过**：33条12fps运行帧记录；三角色近景、密战、短UI反馈全帧与其余动作抽样，四Boss与玩家死亡均覆盖 |
| 同一可追溯候选，不跨版本拼分 | **通过**：只评分Round38/UI38/Probe38，23文件身份一致；旧轮画面没有进入本轮观察清单 |

没有需要阻断本轮美术通过的必修问题。表中8个4分项列出下一轮可改进方向；这些扣分保持保留，未按通过压力补成5分。

## 观察限制

1. 只离线查看现成 Windows development Player 的 Camera.Render 原始采集与同一 Canvas；未控制桌面、未启动新 Player、未独立游玩。图表仅作索引，32张PNG和24张JPG另开原图；较大原图由查看器适配显示，未声称2560图逐像素原比例。
2. 33条原始12fps运行序列共1433帧，实际查看572张表内帧及16张额外Boss相邻帧，共588张不同序列帧。三角色近景、96帧密战及短UI反馈有全帧覆盖；其余长动作抽样，未逐帧检查全部1433帧，也不是实时60fps流畅度评估。
3. 采集使用程序化输入及受控局面；sequence元数据明确controlledPlayerHealing=true，role记录敌人静止/不死，Boss动作记录Boss不死。这用于保持动作可见，不能代表自然通关/生存难度；所见密战仍保留六能力持续发射与伤害，非清空技能截图。
4. 视觉范围/预警与命中一致性依据记录中的起手—区域—兑现与同版运行断言，未做每个碰撞体、所有随机种子或每个子帧的穷举测量。
5. 此结论只为Round38美术验收；不评音频、资源架构、正式版完整功能、冷启动或2小时性能。Release38的源码/AA一致性报告是身份桥接，不替代正式版自身运行验证。

完整原始证据已经具备，本轮必需美术类别没有缺失。后续如改人物/场景/渲染/特效或UI，应使用同一量表回归受影响内容；当前结论不自动延续到新候选。机器可读结果见 [ArtReviewResult38.json](../../ArtReviewResult38.json)。

