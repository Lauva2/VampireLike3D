# Round39 完整独立美术复评

日期：2026-10-22（Asia/Shanghai）。独立评审子代理：`/root/art_review38`。

**Round39 完整美术通过，92/100。A14、B14、C18、D20、E18、F8；20项最低4分，全部原硬门槛满足。** 本轮重新实际查看完整39证据，使用 [原100分量表与硬门槛](../../ArtReview.md)；分数与38相同是本轮逐项判断的结果，没有继承38的评分或观看记录。此结论不代表整项目、正式版完整功能、自然平衡或7200秒性能通过。

## 身份、范围与实际观看

来源：`Builds/Review39/WanXiangExpedition.exe` 的Windows development Player原始 `Camera.Render` 与同一实时Canvas。Unity 2022.3.62f2，默认quality=2（精致），本机RTX3060 Laptop/i7-10870H。未控制桌面、窗口、键盘或鼠标，未另起Player/构建。三份 [BuildIdentity](../../../Artifacts/Round39/BuildIdentity.json)、[UI身份](../../../Artifacts/UI39/BuildIdentity.json)、[Probe身份](../../../Artifacts/Probe39/BuildIdentity.json)的23文件名称/SHA256集合一致。

- Assembly-CSharp SHA256：`11171D7EDAA9D0E5316119F472801A8286D7CDC52D44EA3AF61081E1F74D5098`
- AA catalog SHA256：`C738005569E0EC902F91086D3C8ACB074A611BE5727619B11CC56F1CC14C21B1`
- [Release39BuildMatch](../../../Artifacts/Release39BuildMatch.json)：1068源码及18个AA包与开发版一致，仅作候选身份桥接；正式版自己的运行应另查正式流程报告。
- [Source38To39](../../../Artifacts/Source38To39.json)记录完整变动，不用于证明画面相同，也不增加美术分。
- 原图 [21-map](../../../Artifacts/Round39/Captures/21-map.png)可见种子 **705960096**、世界坐标21/15。其他阶段为不同受控局面，不能把此种子套用于全部采集；同名 `.world.json` 保留各图坐标、镜头、区块状态。角色、六能力及游戏时间由原图HUD与序列追溯。

| 同版运行报告 | finished UTC | 结果 |
| --- | --- | --- |
| [Round39 Full](../../../Artifacts/Round39/Verification.json) | 2026-10-21 18:09:00 | 95 checks，0 errors，passed |
| [UI39](../../../Artifacts/UI39/Verification.json) | 2026-10-21 18:11:32 | 72 checks，0 errors，passed |
| [Probe39](../../../Artifacts/Probe39/Verification.json) | 2026-10-21 18:11:05 | 8 checks，0 errors，passed |

这些运行检查补充版本/行为证据，未作为视觉评分的替代。

**本轮72张辅助表全部实际重看，覆盖190张静态原图、33条序列的571张表内帧。另打开55张原图（29 PNG、26 JPG），其中18张动态原帧不在表内，实际共看589张不同动态帧。** 原序列固定12fps，共1414帧；没有声称逐帧看完所有原帧。三角色侧前近景144帧、密战96帧和短UI反馈为全帧查看；其余长序列按 [ReviewSampling](ReviewSampling.json)跨阶段抽样。逐张观看表、完整原始路径和额外原图清单在 [ObservedEvidence.json](ObservedEvidence.json)，映射见 [SheetManifest](SheetManifest.json)。

覆盖3角色、4普通/精英族、四Boss、四单群系与Mixed；四群系各三处地貌的正常/广角、四挑战点前/近景/完成态、四处真实近水建筑与广角、四组混合边界（0-2、0-3、1-2、2-3）、熔岩岸尾、浮动原点与五探针。UI五画幅70张页面全部查看，包含菜单、选择锁定/解锁、成长任务、设置、HUD、技能、地图、升级、暂停/暂停设置、加载、结算/胜利；Probe两类错误恢复另有原图。画幅为1280×720、1920×1080、1920×1200、2560×1080、2560×1440。

## 新经验晶体与疑点复核

[高质量经验聚合](../../../Artifacts/Round39/Captures/experience-aggregates.png)和[低质量经验聚合](../../../Artifacts/Round39/Captures/experience-aggregates-low.png)均另开完整原图。对应quality元数据为Ultra=5、AA4与Very Low=0、AA1，均1600×900。青蓝晶体和场景/角色的遮挡关系成立；低质量边缘锯齿更显，未见粉色、矩形透明片、大片排序遮挡或地面漏底。密战原帧0016/0024/0055/0078与移动0038另看，拾取物、玩家、危险和持续发射仍可辨。精确保值合并及持续磁吸的算法正确性归技术报告；没有因功能检查通过而加美术分，也没有用静态图证明性能。

原图曾出现两个需要避免误判的投影细节：

- 火山近水建筑圆基完整位于熔岩北岸并接右侧支路；人物/投影覆盖河道不等于基座压河，同版placement资料为foundationClear=true、forcedBuildingPlacement=false。
- 火山挑战完成图中角色旁的小金环内部是三道符纹，属于符阵武器（已对照其他原图和模型构成），不是残留的E交互提示。四完成图真正的挑战文字均已清除，阵纹转暗、地貌留存。

## 20项正式评分

每项5分。表内链接指向本轮实际证据；完整证据列表在JSON中。

| 原子项 | 得分 | 代表证据 | 观察、扣分理由与具体修订方向 |
| --- | ---: | --- | --- |
| A1 Q版比例与俯视轮廓 | 5/5 | [character-adventurer.png](../../../Artifacts/Round39/Captures/character-adventurer.png)、[character-elementalist.png](../../../Artifacts/Round39/Captures/character-elementalist.png)、[character-swordsman.png](../../../Artifacts/Round39/Captures/character-swordsman.png) | 39三角色的大头、短肢、宽发型/帽型和小躯干比例一致；正常俯视中的紫帽、深发和青色足圈提供可靠锚点，近景也未失去Q版体积。 本项无扣分。 |
| A2 面部服饰装备设计 | 4/5 | [character-adventurer.png](../../../Artifacts/Round39/Captures/character-adventurer.png)、[character-elementalist.png](../../../Artifacts/Round39/Captures/character-elementalist.png)、[character-swordsman.png](../../../Artifacts/Round39/Captures/character-swordsman.png) | 护目镜/围巾与短夹克、紫帽金扣法袍、青衣背剑分别可辨；连续近景有睁闭眼。手掌和袖口的转接、腰带附件仍较概括，三张脸的细节语言接近。 **扣分与修订：**调整握持手型及袖口厚度，给各角色增加少量不同的眉眼/配件细节。 |
| A3 明快卡通材质色光 | 5/5 | [10-biome-0.png](../../../Artifacts/Round39/Captures/10-biome-0.png)、[10-biome-1.png](../../../Artifacts/Round39/Captures/10-biome-1.png)、[10-biome-2.png](../../../Artifacts/Round39/Captures/10-biome-2.png) | 林地绿、荒漠金、雪地蓝白、火山紫灰橙各有清楚色域；人物与奶油UI协调。冻结染色和短闪能回到原色；晶体在Ultra/Very Low原图均有青蓝实体轮廓，无粉材或大片透明遮挡。 本项无扣分。 |
| B1 三首发角色模型材质挂点 | 4/5 | [character-adventurer.png](../../../Artifacts/Round39/Captures/character-adventurer.png)、[role-action-adventurer](../../../Artifacts/Round39/Sequences/role-action-adventurer)、[role-closeup-adventurer](../../../Artifacts/Round39/Sequences/role-closeup-adventurer) | 三角色都有完整衣物、手脚和对应武器，抬臂、朝向变化及发射时挂点跟随，背剑/法杖/手枪未丢失。小手与柄的握持、共用躯干关节转折仍简单。 **扣分与修订：**按枪、杖、剑分别校准掌腕角度和持柄接触，补肩肘转折与衣摆的角色差异。 |
| B2 普通与精英辨识 | 5/5 | [enemy-0-normal.png](../../../Artifacts/Round39/Captures/enemy-0-normal.png)、[enemy-0-elite.png](../../../Artifacts/Round39/Captures/enemy-0-elite.png)、[enemy-1-normal.png](../../../Artifacts/Round39/Captures/enemy-1-normal.png) | 叶芽软体、尖角橙兽、蓝甲虫、紫菌帽法杖的主体和动作不同；金冠、角甲和增大体型让精英形成第二层辨识。在39的密战横越火山/林地过程中仍能分辨各族及精英。 本项无扣分。 |
| B3 四Boss完整表现 | 5/5 | [boss-closeup-0.png](../../../Artifacts/Round39/Captures/boss-closeup-0.png)、[boss-attacks-0](../../../Artifacts/Round39/Sequences/boss-attacks-0)、[boss-death-0](../../../Artifacts/Round39/Sequences/boss-death-0) | 四Boss原图分别呈现根木/树冠叶须、节体口牙、冰角白毛蓝爪、暗岩甲橙裂隙，体量明显大于普通敌；四条攻击和四条死亡记录均有阶段变化，未见缺件或材质破损。 本项无扣分。 |
| C1 四群系地貌装饰语言 | 5/5 | [geography-0-0.png](../../../Artifacts/Round39/Captures/geography-0-0.png)、[geography-0-1.png](../../../Artifacts/Round39/Captures/geography-0-1.png)、[geography-0-2.png](../../../Artifacts/Round39/Captures/geography-0-2.png) | 河桥与林簇苔岩、沙丘台地绿洲、冰湖冰壁雪松、熔岩岸桥与玄武岩形成不同地貌组织。火山广角中单处主坑和较低开口岩地有主从，没有把每处野地处理成同样的大圆环。 本项无扣分。 |
| C2 场景层次与尺度 | 4/5 | [waterside-architecture-0.png](../../../Artifacts/Round39/Captures/waterside-architecture-0.png)、[sanctuary-0.png](../../../Artifacts/Round39/Captures/sanctuary-0.png)、[waterside-architecture-1.png](../../../Artifacts/Round39/Captures/waterside-architecture-1.png) | 四种近水建筑/遗迹的完整地基与支路同框，火山基底在熔岩北岸干地；挑战点高构件在后侧，前沿低踏石有尺度层次。荒漠、火山的大平地与小石/柱组之间仍有较宽的平淡区。 **扣分与修订：**用低起伏或少量疏密变化连接大平地和主景，强化岩柱与石簇之间的大小级差，保留战斗空间。 |
| C3 生成衔接遮挡重复感 | 4/5 | [mixed-boundary-2-3.png](../../../Artifacts/Round39/Captures/mixed-boundary-2-3.png)、[mixed-lava-shore-close.png](../../../Artifacts/Round39/Captures/mixed-lava-shore-close.png)、[world-floating-origin.png](../../../Artifacts/Round39/Captures/world-floating-origin.png) | 混合道路跨界连续，熔岩圆尾和岸线完整；五探针图未见底色裂口。移动和96帧密战中玩家可持续追踪，未见模型长期被消隐。远景三石、三柱、圆点地表纹样仍有可察觉的复用节奏。 **扣分与修订：**打散小簇的间距、方向和形状组合，优先减少同一广角内等权重复，保持现有消隐/道路边界。 |
| C4 混合过渡与探索目的地 | 5/5 | [mixed-boundary-0-2.png](../../../Artifacts/Round39/Captures/mixed-boundary-0-2.png)、[mixed-boundary-0-3.png](../../../Artifacts/Round39/Captures/mixed-boundary-0-3.png)、[mixed-boundary-1-2.png](../../../Artifacts/Round39/Captures/mixed-boundary-1-2.png) | 39实际四组边界有渐变带、跨界植被和连续路线。地图事件/Boss符号可对应实景；四挑战点侧路绕阵心，右踏石接路，后左连续地貌围合。四完成原图均显示阵纹转暗、挑战文字清除且主体景观留存，目的地前后状态清楚。 本项无扣分。 |
| D1 全页面视觉体系 | 5/5 | [ui-menu-1280x720.png](../../../Artifacts/UI39/Captures/ui-menu-1280x720.png)、[ui-selection-unlocked-1280x720.png](../../../Artifacts/UI39/Captures/ui-selection-unlocked-1280x720.png)、[ui-progress-1280x720.png](../../../Artifacts/UI39/Captures/ui-progress-1280x720.png) | 菜单、选择、成长任务、设置、暂停、加载55%、两种结算及失败恢复均使用奶油卡片/深青文字/金色强调和统一图标语言。失败提示有稳定底板和明确返回菜单状态。 本项无扣分。 |
| D2 HUD升级技能信息层级 | 5/5 | [ui-hud-1280x720.png](../../../Artifacts/UI39/Captures/ui-hud-1280x720.png)、[ui-skills-1280x720.png](../../../Artifacts/UI39/Captures/ui-skills-1280x720.png)、[ui-upgrade-2560x1440.png](../../../Artifacts/UI39/Captures/ui-upgrade-2560x1440.png) | 生命、时间、Boss名称/阶段、资源、小地图和六能力有分区；升级四卡的标题、效果、等级和新机制提示分层；技能点数、价格、分类与滚动提示清楚。密战HUD没有被额外能力文字挤出。 本项无扣分。 |
| D3 中文长文本排版 | 5/5 | [ui-selection-locked-1280x720.png](../../../Artifacts/UI39/Captures/ui-selection-locked-1280x720.png)、[ui-progress-1280x720.png](../../../Artifacts/UI39/Captures/ui-progress-1280x720.png)、[ui-skills-1280x720.png](../../../Artifacts/UI39/Captures/ui-skills-1280x720.png) | 39原图重看锁定原因、任务条件/奖励、构筑说明、地图说明及长机制文字，未见方框、关键字裁切或越界。列表底端部分卡片受正常滚动遮罩约束，滚动条与页脚明确；字号/行距稳定。 本项无扣分。 |
| D4 画幅状态图标适配 | 5/5 | [UI39-00.png](UI39-00.png)、[UI39-11.png](UI39-11.png)、[Verification.json](../../../Artifacts/UI39/Verification.json) | 五画幅70张UI图已全部实看：1280×720、1920×1080、1920×1200、2560×1080、2560×1440。锁定/解锁/已选、灰态价格、返回及主按钮可辨，未见拉伸、主模态出界或必要操作被遮挡。 本项无扣分。 |
| E1 技能联动与命中层次 | 5/5 | [effect-arc](../../../Artifacts/Round39/Sequences/effect-arc)、[effect-embers](../../../Artifacts/Round39/Sequences/effect-embers)、[effect-explosion](../../../Artifacts/Round39/Sequences/effect-explosion) | 火球橙瓣与余烬、金色细枪弹、电弧折线、青白飞剑斩弧、蓝色冰环/碎片、金色符阵有不同轨迹与扩散方式；密战中可见命中碎光和后续余波，随时间消退，未用大白闪覆盖所有命中。 本项无扣分。 |
| E2 高密度玩家危险与拾取辨识 | 4/5 | [late-density-combinations](../../../Artifacts/Round39/Sequences/late-density-combinations)、[0016.jpg](../../../Artifacts/Round39/Sequences/late-density-combinations/0016.jpg)、[0024.jpg](../../../Artifacts/Round39/Sequences/late-density-combinations/0024.jpg) | 全部96帧中紫帽/青足圈、金冠和危险红边持续可定位，蓝晶体和粉色敌弹区别成立；聚合没有造成铺满屏幕的光片。0016/0024附近剑尾、冰弧和敌群在脚下交叠，局部读取仍拥挤。 **扣分与修订：**按中心屏幕覆盖密度收窄剑尾/冰弧亮带或调整纯视觉透明度，保留能力数量、发射和伤害。 |
| E3 Boss精英预警与时序 | 5/5 | [boss-attacks-0](../../../Artifacts/Round39/Sequences/boss-attacks-0)、[boss-attacks-1](../../../Artifacts/Round39/Sequences/boss-attacks-1)、[boss-attacks-2](../../../Artifacts/Round39/Sequences/boss-attacks-2) | 四Boss的起手、红圈/条带、临界强调、生效和回收都有39记录；另逐张开各0018–0021/0026原帧。沙虫条带与朝向一致，古树三圈、雪原多圈/近身圈、熔岩五圈的实际特效仍在相应区域，精英短条边线与敌弹保持可辨，未见严重范围错位。 本项无扣分。 |
| E4 状态无敌危险交互提示 | 4/5 | [character-move-dodge](../../../Artifacts/Round39/Sequences/character-move-dodge)、[0038.jpg](../../../Artifacts/Round39/Sequences/character-move-dodge/0038.jpg)、[boss-attacks-2](../../../Artifacts/Round39/Sequences/boss-attacks-2) | 闪避的淡亮/足圈、受击边缘渐变、冻结蓝染与恢复、红边界及交互文字均能对应状态，未见持续过曝。火山挑战提示深青字直接压在灰紫地表，局部对比偏弱；完成态金色三纹小圆是符阵武器，非残留E提示。 **扣分与修订：**给世界交互提示加入简洁稳定底板或对比描边，特别提高火山地表下的文字与按键辨识。 |
| F1 动作起手收招与死亡 | 4/5 | [character-move-dodge](../../../Artifacts/Round39/Sequences/character-move-dodge)、[role-action-adventurer](../../../Artifacts/Round39/Sequences/role-action-adventurer)、[role-action-elementalist](../../../Artifacts/Round39/Sequences/role-action-elementalist) | 三角色近景144帧可见开火/抬杖/剑诀和眨眼；正常镜头有转向、闪避，玩家倒地到结算及四Boss死亡有连续变化。肩腕回中和身体跟随仍偏短促、程序循环感可见。 **扣分与修订：**为腕肩回中和躯干跟随补少量缓动与重量变化，不改变预警兑现/无敌判定时点。 |
| F2 UI拾取奖励反馈与一致性 | 4/5 | [feedback-pause-open](../../../Artifacts/Round39/Sequences/feedback-pause-open)、[feedback-skills-open](../../../Artifacts/Round39/Sequences/feedback-skills-open)、[feedback-task-unlock](../../../Artifacts/Round39/Sequences/feedback-task-unlock) | 淡入页面能收敛到完整不透明，任务/升级奖励Toast与世界金青色语言统一；密战和移动中晶体可吸入、经验等级推进，未见大片排序错误或明显材质跳失。各类奖励主要靠同类光点/Toast，反馈节奏差异还不够丰富。 **扣分与修订：**分别细化任务、能力升级、拾取的短缓动与收束节奏，保持光效局部、避免新增大面积遮挡。 |

**A14/15 + B14/15 + C18/20 + D20/20 + E18/20 + F8/10 = 92/100。** 八个4分项的局部问题均保留扣分。场景C的两个5分分别来自四群系清楚的地貌语言和完整的过渡/探索目的地组织；大平地和小簇复用仍让C2/C3各扣1分，没有按“没有裂缝”把场景满分化。

## 原硬门槛逐项检查

| 检查 | 结论 |
| --- | --- |
| 总分≥91 | 通过：92 |
| A/B≥13，C/D/E≥18，F≥8 | 通过：14/14/18/20/18/8 |
| 20子项各≥4 | 通过：最低4，逐项列明 |
| 全冻结首发角色/四群系/四Boss/Mixed/正式UI均实机覆盖 | 通过：本轮完整观看清单，无未评分类别 |
| 无占位角色/敌人/场景/默认UI，粉材、缺贴图或明显破损模型 | 通过：所看全部范围未见，造型与材质有完整主题和层次 |
| 中文无缺字、关键裁切/不可读按钮、遮罩错误或核心UI出界 | 通过：五画幅矩阵与中文原图复核；滚动列表底端部分卡片有有效滚动区域 |
| 密战/Boss能持续定位玩家和危险，不靠停火/删能力/关伤害造图 | 通过：96帧密战全看，六能力持续可见，角色头/足圈和红危险边线可追踪；Full补充六能力/伤害断言 |
| 四Boss危险起手/预警、闪避窗口有对应表现，无严重遮挡/范围错位 | 通过：四攻击序列及各0018–0021/0026原帧；移动闪避、冻结与短闪；窗口内外伤害有同版断言。判断范围限于实际观察 |
| 动画反馈有连续运行证据，非单姿态代替 | 通过：33条12fps运行记录，571表内帧及18额外原帧；四Boss/玩家死亡和五反馈均已看 |
| 同一可追溯候选，无跨版拼分 | 通过：仅Round39/UI39/Probe39，23文件身份相同；FormalGameplay近景和38画面未补入 |

本轮没有阻断美术通过的必修问题。下一轮可按八项扣分方向精修，任何渲染、模型、场景、特效或UI改动均应按原量表回归，当前结论不自动延续到新候选。

## 观察限制

1. 离线查看现成development Windows Player Camera.Render原始完整画幅及同一Canvas；未控制桌面、未启动Player/构建、未独立交互试玩。辅助表是全部原图的缩略索引；另开原图55张，2560×1440图由查看器适配为2048×1152显示，不声称逐像素原比例检查。
2. 33序列固定12fps、共1414帧，实际571表内帧加18张额外原帧，共589张不同动态帧；未逐帧看全部1414帧。144帧三角色近景、96帧密战及短UI反馈全帧覆盖，其余跨阶段抽样；不能用此判断60fps或正常速度流畅度。
3. 序列有controlledPlayerHealing=true；角色动作/近景的敌人静止且不死，Boss动作录制中Boss不死。它们保证可观察动作，不能证明自然生存/平衡。密战所见六能力持续、击败数与等级推进，未以停止发射/删除能力/关闭伤害求干净画面。
4. 预警与攻击范围判断是所见视觉阶段/区域的一致性及同版窗口断言，不是穷举所有随机种子、每个碰撞体或亚帧判定；经验精确保值/磁吸算法正确性属于技术报告，不因源码修改或95功能检查增加美术分。
5. 只评价Round39冻结首发美术。FormalGameplay39近景、旧38画面/分数、Campaign或Soak运行进度均未用于补美术覆盖；不代表整项目、正式版完整功能、音频或连续7200秒性能通过。

机器结果为 [ArtReviewResult39.json](../../ArtReviewResult39.json)，`candidate=Round39`、`passed=true`。本次仅写39专属记录，没有同步共享结论索引。

