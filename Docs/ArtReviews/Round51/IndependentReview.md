# Round51 / 1.2.0 独立美术总评

**核心完整美术91/100，原20项最低4，原10门槛均在列明视觉范围内通过。** A14/B14/C18/D19/E18/F8。此为三名独立评审对51实际原图重新观察的合并；不是沿用47或50的91分，也不代表性能或整项目通过。独占九武器深构筑与实际7200秒末帧仍待追加，尚未更新共享ArtReview.md或ArtReviewResult.json。

原量表与门槛：`Docs/ArtReview.md` §1–6，100分20整数子项；对原“四群系/四Boss”条目按当前冻结范围扩展为六单群系+Mixed、六Boss，其余要求不变。三角色、八普通/八精英、九武器、新增联动与UI/任务/升级均纳入，不因扩容改变分母或降低门槛。

| 独立评审 | 实际范围 | 观看方式与数量 | 分报告 |
| --- | --- | --- | --- |
| art_lead47 | A/B/C，角色与世界 | 40辅助表内146不同静态；46原图另开；本人0动态帧 | [World](World/IndependentWorldReview.md) / [证据](World/Review.json) |
| art_ui47 | D1–D4及F2页面贡献 | 41表内185静态与42反馈帧；另开45静态/8动态源文件 | [UI](UI/IndependentUIReview.md) / [证据](UI/ObservedEvidence.json) |
| art_combat47 | E1–E4/F1及F2战斗贡献，B3完整表现佐证 | 74表内856/2567动态、13静态；另开65动态/13静态源文件，47序列均抽样 | [Combat](Combat/IndependentCombatReview.md) / [证据](Combat/ObservedEvidence.json) |

联合去重：336不同静态、856/2567不同动态帧、47序列、172个单独打开的原文件、155辅助表。总评本人只宣称World的实际观看；UI和Combat观察归属各自评审，读过他们的报告不等于本人看过全部原图。逐文件SHA/尺寸/观看归属、静态seed/角色/能力/游戏时间/site、动态帧号/时间与原sidecar哈希见[联合实际观看清单](ObservedEvidence.json)。

| 原子项 | 分/5 | 观察、扣分与修改方向 |
| --- | ---: | --- |
| A1 大头短肢与俯视轮廓清晰 | 5 | 三角色大头短肢，鞋手分离，发型/帽子/长袍在正常镜头保持清楚，九能力实体围绕时头部仍可定位。 修改：无本项必修。 主要证据：`Artifacts/Round51/Captures/character-adventurer.png`；`Artifacts/Round51/Captures/character-elementalist.png`；`Artifacts/Round51/Captures/character-swordsman.png`；完整证据见机器结果对应项。 |
| A2 面部服饰装备设计与辨识度 | 4 | 棕发橙围巾/紫尖帽白发/深发绿袍形成身份，眼白瞳孔眉嘴鼻颊色和带扣/鞋/服装分区有设计；纵长大眼偏夸张、指肘衣褶仍简。 修改：细化手指/肘/衣褶与眼比例，保持Q版。 主要证据：`Artifacts/Round51/Captures/character-adventurer.png`；`Artifacts/Round51/Captures/character-elementalist.png`；`Artifacts/Round51/Captures/character-swordsman.png`；完整证据见机器结果对应项。 |
| A3 明快卡通造型材质色光协调 | 5 | 六单群系青翠、金黄、冰蓝、灰紫橙火、灰绿紫菌、玫瑰赤晶均有独立色组，统一描边/柔和影与明快角色一致；雪与晶谷没有全屏泛白。 修改：无本项必修。 主要证据：`Artifacts/Round51/Captures/terrain-detail-0.png`；`Artifacts/Round51/Captures/terrain-detail-1.png`；`Artifacts/Round51/Captures/terrain-detail-2.png`；完整证据见机器结果对应项。 |
| B1 全部首发角色模型材质与挂点完成 | 5 | 三角色有完整发脸、鞋手、服装、手持灰枪/法杖/背剑，正常九能力画面角色不缺件，悬浮自动武器为独立视觉，不作手持挂点缺失；连续动作由Combat复核。 修改：无本项必修。 主要证据：`Artifacts/Round51/Captures/character-adventurer.png`；`Artifacts/Round51/Captures/character-elementalist.png`；`Artifacts/Round51/Captures/character-swordsman.png`；完整证据见机器结果对应项。 |
| B2 八普通敌种与精英轮廓动作标识区分 | 4 | 旧四基型与新高孢囊、低蛙、晶甲黑足、长赤晶金弧各可辨；精英金冠及侧菌/肩甲/角/晶有差异，正常新地图镜头成立。精英仍较依赖共用金冠，差异化可精修。 修改：强化各类精英独有盔甲/姿态，保留共同等级语汇。 主要证据：`Artifacts/Round51/Captures/enemy-0-normal.png`；`Artifacts/Round51/Captures/enemy-0-elite.png`；`Artifacts/Round51/Captures/enemy-1-normal.png`；完整证据见机器结果对应项。 |
| B3 六Boss独立主题造型体量与表现 | 5 | 古树根手/分节虫/白毛蓝角兽/橙核黑岩像/深青冠蛙背菌/头肩赤晶石卫各有主题轮廓，正常镜头比小怪和玩家更大。蛙沿同类基型但深青冠背菌清楚，石卫与熔像同巨像方向有独立头肩晶/核心。动态完整性另引Combat本轮报告。 六套攻击与141/141死亡帧由art_combat47独立观察支撑完整表现；总评人未声称本人看过这些动态。 修改：无本项必修；后续可进一步拉开蛙王与普通蛙细部。 主要证据：`Artifacts/Round51/Captures/boss-closeup-0.png`；`Artifacts/Round51/Captures/boss-closeup-1.png`；`Artifacts/Round51/Captures/boss-closeup-2.png`；完整证据见机器结果对应项。 |
| C1 六单群系独立地貌装饰语言 | 5 | 六地各有三处近广景：林河树岩、沙丘绿洲岩台、冰湖雪脊冰门、熔河桥柱、菌伞莲点池栈板菌屋、赤晶簇碎晶环门柱，保持开阔战斗面并有独立地貌。 修改：无本项必修。 主要证据：`Artifacts/Round51/Captures/geography-0-0.png`；`Artifacts/Round51/Captures/geography-0-1.png`；`Artifacts/Round51/Captures/geography-0-2.png`；完整证据见机器结果对应项。 |
| C2 地面植被岩石建筑遗迹层次尺度 | 4 | 大地貌/中景树石晶/小点草菌、建筑基台/门柱/桥与道路分层，六主题尺度一致；屋侧面/门柱仍平整，木板重复、晶谷留白较多。 修改：补少量屋檐门框/材质分区和遗迹边缘，不用散点堆叠填空。 主要证据：`Artifacts/Round51/Captures/waterside-architecture-0.png`；`Artifacts/Round51/Captures/waterside-architecture-1.png`；`Artifacts/Round51/Captures/waterside-architecture-2.png`；完整证据见机器结果对应项。 |
| C3 区块接缝遮挡重复与视觉一致 | 4 | 六地广景、浮动原点、Probe远坐标/超宽最大缩放无空块或硬裂面，路径桥岸衔接；菌伞开圆口后中心玩家可辨。重复树团/柱束/棱晶组方向接近，圆头岸线与消隐口边缘较硬。 修改：增加非对称组群/大小方向变化，平滑遮挡边缘；不以静态证明所有种子或碰撞。 主要证据：`Artifacts/Round51/Captures/world-floating-origin.png`；`Artifacts/Round51/Captures/geography-5-2.png`；`Artifacts/Round51/Captures/geography-wide-6-1.png`；完整证据见机器结果对应项。 |
| C4 Mixed六区过渡地标路径交互清楚 | 5 | Mixed六主体与六边界包括菌沼/赤晶，宽颜色渐变/地物混入、路径连续；熔河与雪侧分离。旗/箱/碧滴祭坛/主题挑战圈分工明确，六完成回访保留完整景物并熄圈清提示。 修改：无本项必修；火山开启文字偏暗归E4局部精修。 主要证据：`Artifacts/Round51/Captures/mixed-boundary-0-1.png`；`Artifacts/Round51/Captures/mixed-boundary-0-2.png`；`Artifacts/Round51/Captures/mixed-boundary-0-6.png`；完整证据见机器结果对应项。 |
| D1 完整UI视觉体系 | 5 | 主菜单、三角色/七地图选择、任务/永久成长、设置、加载、暂停/暂停设置、地图、构筑、普通升级、六祝福三选一、胜利及撤离/死亡结算完整保持奶油底/青色主操作/彩色图标体系；新增地图缩略图、任务/技能也纳入。加载失败返回菜单有明确提示，启程主按钮可见。 修改：无本项必修；新增页面继续保持当前主题、层级与资源完成度。 主要证据：[SheetManifest.json](UI/SheetManifest.json)；`Artifacts/Probe51/Captures/load-failure-terrain-forest.png`；`Artifacts/Probe51/Captures/load-failure-weapon-gun.png`；完整证据见机器结果对应项。 |
| D2 HUD/升级/技能树层级与密度 | 5 | 生命/计时/资源/地图/能力9摘要分层；九武器条初段和后段均实看，有滚轮/拖动提示，不引入槽位上限。普通四选和祭坛三选有名称、效果、等级/层数/机制叠加说明。任务名称/目标/进度/奖励与技能分类/价格/锁定原因/已掌握区分清楚，祝福×2卡说明本局生效。 修改：无本项必修；新增量继续通过可滚动容器表达，保留能力总数与滚动引导。 主要证据：`Artifacts/UI51/Captures/ui-hud-1280x720.png`；`Artifacts/UI51/Captures/ui-hud-expansion-1280x720.png`；`Artifacts/UI51/Captures/ui-skills-expansion-联动-locked-1280x720.png`；完整证据见机器结果对应项。 |
| D3 中文字形、长文本、字号行距对齐 | 4 | 已看稳定页面、新五任务与新武器/三联动文字无缺字方框或关键裁切；本轮生机短文在1080原图完整一行。扣1来自本版2560×1440普通升级碎霜领域：前行到“冰晶对冻结目标额外造成60%伤害”，下一行仅句号。原图独立打开确认；非沿用50。 修改：使碎霜描述句末标点跟随正文，重排短句或调整卡内排版；用该候选在五画幅回归，不能只检查自动fits断言。 主要证据：`Artifacts/UI51/Captures/ui-upgrade-2560x1440.png`；`Artifacts/UI51/Captures/ui-upgrade-1920x1080.png`；`Artifacts/UI51/Captures/ui-task-expansion-boomerang-1280x720.png`；完整证据见机器结果对应项。 |
| D4 五画幅与交互状态、价格、锁定原因 | 5 | 720p/1080p/16:10/21:9/1440p均实看，核心面板未见拉伸/出界。新两地图锁定灰卡/锁标与原四Boss条件、开放白卡/点击选择清楚。两武器锁定任务原因、三联动灰锁与解锁后4点价格明确；已获九武器始终保留，仅任务完成可见状态被临时切换。祭坛0→1/1→2与取消保留清楚。 修改：无本项必修；继续区分已获、可购、任务锁定三种状态。 主要证据：[SheetManifest.json](UI/SheetManifest.json)；`Artifacts/UI51/Captures/ui-selection-expansion-locked-2560x1080.png`；`Artifacts/UI51/Captures/ui-skills-expansion-联动-locked-1280x720.png`；完整证据见机器结果对应项。 |
| E1 武器、能力与联动形色及命中层次 | 5 | 火球橙焰、金色细弹、青白飞剑、紫金折线雷、蓝冰碎片与符环具有独立轮廓；孢子紫色乳白点和晶爆粉色棱片可辨。连弩青金弓形、往返回旋刃青色折角轮廓、冰杖蓝晶杆体与冰迹实际出现。密战仍见多种发射与命中，并非只凭能力9的HUD计数判断。 修改：无本项必修扣分；新增能力继续沿用独立形状和敌我色组。 主要证据：`Artifacts/Round51/Captures/weapon-crossbow.png`；`Artifacts/Round51/Captures/weapon-boomerang.png`；`Artifacts/Round51/Captures/weapon-froststaff.png`；完整证据见机器结果对应项。 |
| E2 高密叠加中的玩家、危险、目标与掉落辨识 | 4 | 九能力持续密战中，玩家头帽与青色足环能连续追踪，粉红敌弹/红危险范围与青色经验有区分，目标金环可见。0018/0024/0030及0072的飞剑、蓝冰环、黄精英冠在中心和左侧拥聚，短时压缩怪物身体及地面预警的可用空隙，故扣1分。新怪隔离片段关闭能力，未用于满足本门槛。 修改：降低精英冠重复叠加和己方冰环/剑雨覆盖的视觉权重，保留所有能力、实际发射与伤害；用同九能力高密回归玩家足环和红预警。 主要证据：`Artifacts/Round51/Sequences/late-density-combinations/0000.jpg`；`Artifacts/Round51/Sequences/late-density-combinations/0018.jpg`；`Artifacts/Round51/Sequences/late-density-combinations/0024.jpg`；完整证据见机器结果对应项。 |
| E3 六Boss及精英预警形状、时序和命中区域 | 5 | 树根多圆、沙虫方向长条、寒兽环震/冰点、熔岩多点爆发、菌蛙大圈/多孢圈、晶卫方向条/多晶落点具备可辨前摇，红粉圈线由淡至明确并出现条纹/感叹号，爆发在此前范围内。相邻原帧可见树根警示→命中花瓣→受击边缘的局部顺序。新孢囊/跃蛙/前甲冲锋/扇晶弹的普通精英轮廓和动作危险可辨。未见严重错位或被长时间完全遮盖；不据12fps推断精确判定毫秒。 修改：维持敌方专用红粉形状；后续若改攻击范围，应以同版相邻原帧与正常实战回归。 主要证据：`Artifacts/Round51/Sequences/boss-attacks-0/0018.jpg`；`Artifacts/Round51/Sequences/boss-attacks-0/0019.jpg`；`Artifacts/Round51/Sequences/boss-attacks-0/0020.jpg`；完整证据见机器结果对应项。 |
| E4 闪避、受击、状态、边界与交互提示 | 4 | 闪避弯身/浅色闪现、青色足部定位，受击红边与冻结蓝色状态在连续证据中有对应表现；目标金环与血条明确。未见严重过曝。火山祭坛深青色E开启挑战文字直接落在紫灰地面，对比偏弱但仍可读，扣1分。只观察到开战脱离提示，未看完整脱战倒计时，闪避无敌起止亦不作毫秒精度结论。 修改：为世界交互文字加浅底或外描边，先回归火山紫灰地面，并保留祭坛5/6当前清晰度。 主要证据：`Artifacts/Round51/Sequences/character-move-dodge/0001.jpg`；`Artifacts/Round51/Sequences/character-move-dodge/0002.jpg`；`Artifacts/Round51/Sequences/character-move-dodge/0003.jpg`；完整证据见机器结果对应项。 |
| F1 移动、攻击、闪避、受击、死亡与Boss起收动作 | 4 | 三角色走转、手持动作、闪避倾身和受击死亡可辨；玩家倒下后进入结算，六Boss分别倾倒、压低或收缩后消退，树/虫/兽/岩像/菌蛙/晶卫都有实际连续死亡画面。跃蛙有离地与落点变化。短动作多以整体旋转/缩放完成，攻击后回正和Boss收招较直接，肢体重心衔接尚少层次，扣1分。青金悬浮能力枪与手中灰枪分离，不定性为断挂点。 修改：延长角色攻击回收的缓冲，给Boss死亡/收招增加分段重心与肢体延迟，在现有三角色和六Boss原循环回归。 主要证据：`Artifacts/Round51/Sequences/role-closeup-adventurer/0019.jpg`；`Artifacts/Round51/Sequences/role-closeup-elementalist/0019.jpg`；`Artifacts/Round51/Sequences/role-closeup-swordsman/0019.jpg`；完整证据见机器结果对应项。 |
| F2 页面、拾取/升级/解锁反馈与世界整体一致性 | 4 | UI与Combat分别实看五反馈42/42帧，页面淡入后稳定，任务解锁、升级奖励、掉落吸收和世界色系协调，结算未见开战Toast遮挡按钮。重要奖励仍主要用通用环/Toast，世界庆祝动作层次偏薄，扣1分。祭坛专属完整打开/取消/重开/选择/恢复普通升级及HUD连续滚动未抽样，不将静态对照称完整动态。 修改：增加不遮挡玩家的重大升级/首次解锁短反馈；后续专项补祭坛完整转场和HUD连续滚动。 主要证据：`Artifacts/Round51/Sequences/feedback-upgrade-open/0007.jpg`；`Artifacts/Round51/Sequences/feedback-upgrade-reward/0007.jpg`；`Artifacts/Round51/Sequences/feedback-task-unlock/0000.jpg`；完整证据见机器结果对应项。 |

每项完整原图路径、观察人及修改方向见[ArtReviewResult51.json](../../ArtReviewResult51.json)。上述全部分数来自本轮观察，功能检查数与资源哈希只作来源核对，不增加视觉分。

| 原门槛 | 结论 | 依据与边界 |
| --- | --- | --- |
| H01 总分至少91/100 | 通过 | 20项本轮实际独立观察合计91/100。 |
| H02 A/B≥13，C/D/E≥18，F≥8 | 通过 | A14/B14/C18/D19/E18/F8，满足原门槛。 |
| H03 20子项均≥4 | 通过 | 20项均4或5；保持原100分、整数20项及分母。 |
| H04 全部冻结内容同版实机覆盖，无未评分内容 | 通过 | 3角色、8普通/8精英、6Boss、6单群系/Mixed六区域六边界、9武器、新3联动、60升级/25任务所属页面与新增目标行及全部正式UI纳入原项；47序列均抽样。未穷尽所有技能/任务滚动状态组合。 |
| H05 无占位、粉材、缺贴图或明显破损模型 | 通过 | World146、UI185、Combat13不同静态与动态抽样均未见该类硬缺陷。手持灰枪与外侧悬浮能力体可分，不据间距定性断挂点。 |
| H06 中文无缺字/关键裁切/不可读按钮/遮罩错误/核心UI出界 | 通过 | 36页面/状态×五画幅180矩阵及额外5静态，UI独立观察；1440碎霜领域句号孤行属D3局部4分，文字未丢失。 |
| H07 密战持续定位玩家和危险，不靠停射/删能力/关闭伤害制造干净图 | 通过 | 96/96九能力密战帧+七地图密战原图见实际发射/命中、玩家头帽青圈和粉红敌危；受控治疗不等于关闭伤害。新怪隔离明确关闭Abilities且不死，仅评动作，绝不用于此门槛。 |
| H08 全部Boss危险前摇可辨、闪避有表现，无持续遮挡或严重范围错位 | 通过 | 六Boss攻击168不同帧含六组0018–0021相邻原帧，新怪140帧及闪避相邻0001–0003支撑所见视觉阶段/范围；未见长期全遮挡。不推断亚帧伤害或无敌毫秒，未声称完整脱战倒计时已看。 |
| H09 动画反馈有真实运行连续证据 | 通过 | 47条序列均抽样；141/141六Boss死亡、14/14玩家死亡、42/42五反馈、96/96密战全部表看，另相邻原帧。不是静态代替动作；祭坛专属完整转场/HUD连续滚动仍未验证。 |
| H10 同一可追溯候选，禁止跨版拼图 | 通过 | Round51/UI51/Probe51同Review51的29文件名/SHA逐一一致；1.2.0、1309源码、24AA与Release51精确匹配，ReleaseAudit通过。仅使用51实际观察，不迁旧图旧分。 |

候选与采集追溯：Round51 243项、UI51 215项、Probe51 8项全部finished/passed、errors=[]、原退出0；三套29条运行文件名/SHA相同。Workspace51Identity核对1309源文件，开发/发行24AA包及源码精确一致，ReleaseAudit51通过。详见[身份核对](IdentityAudit.json)。未启动Player、构建、修改运行时代码或控制桌面。

画面与元数据：主要世界PNG1600×900 Ultra/4x；武器展示1600×1200；UI五画幅为1280×720、1920×1080、1920×1200、2560×1440、2560×1080；动态JPG1280×720。大图显示运输可能缩至2048像素，未声称每张1:1逐像素审看。PNG按各自world sidecar记seed与实际site；三个角色展示和两个加载失败图没有world sidecar，字段未知。动态metadata通常无逐帧seed，不把相邻静态上下文赋给每个动态帧。

时间与夹具：角色/移动/Boss攻击等固定12fps序列含受控治疗、静止不死目标或Boss不死；效果/死亡/反馈有墙钟timestamp，不能把请求0.03333秒当实测30fps。新怪隔离关闭Abilities并不死，只用于动作与敌种识别；H07依据另行九能力密战96帧及七地图原图。所见范围支持局部预警/命中顺序与形状，不能证明亚帧判定、自然承伤平衡、持续60FPS或长测性能。

地形单像素：`Artifacts/TerrainCoverage51Audit.json`原审计失败保留，60图有59为0、mixed-region-snow在(1321,740)恰1像素等CameraClear色。另开原图所见地面连续，无可见连续洞/缺块/裂边；该点位于浅金弹丸边缘附近。抗锯齿巧合是合理推断，未确定原因，不把1改0、不改原图或将审计失败改写通过，见[独立专项](World/TerrainPixelReview.json)。

采样限制：未逐项穷尽60升级/25任务全部状态与所有种子。祭坛完整专属转场、HUD连续滚动、完整脱战倒计时未独立看全；稳定对照与日志不替代这些连续证据。暂无音频人工试听或实时正常速度手动试玩。现有序列足以覆盖原动画/反馈门槛，具体未采样流程仍单列未知。

后续补充：独占九武器深构筑与实际7200秒末帧收到后，须核对同版并另开原图，追加实际静态观察。单末帧只能证明当帧可读性，不证明整段长期画面或性能；在收到前不得记完成。共享当前结论与旧42/45/46/47/50历史保持不动，46未形成总分，不补旧成绩。
