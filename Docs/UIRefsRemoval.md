# 移除UIRefs

> 后续启动问题已修复：独立面板缺 CanvasGroup，动画构造函数的 `??` 无法识别编辑器特殊空引用。现已用 Unity 空值比较，并在基类与14个预制体补齐依赖；本轮330编辑器/33框架检查通过。详见[启动修复](CanvasGroupStartupFix.md)，下方58结果不包含此修复。

2026-10-23用户要求移除UIRefs。具体View已经接管控件引用，UIManager不再汇总或反射注册控件。

## 实现

- 删除UIRefs.cs及其meta，生成的UIRoot和14个面板不再挂该组件；其余既有脚本和预制体meta/GUID保持。
- SelectionView直接持有角色/群系列表和模板，HUDView持有血条/地图/能力栏，UpgradeView、SkillsView、ProgressView分别持有自己的列表与模板，SettingsView持有滑块，MapView持有地图，DamageFlashView持有受击Image。
- UIView不再RequireComponent(UIRefs)或提供Refs属性；UIController不再持有共享UIRefs。通用列表创建工具显式接收当前View的模板。
- UIManager移除View聚合属性、反射字段集合和Register/Unregister。GetView<TView>(id)只查询已经创建且类型正确的View，未创建/类型不符返回null；Root保留Canvas和四层访问。
- UIFactory直接给具体View赋值，UIPrefabFactory只保存各View及其本地模板。编辑器按具体View的实际字段检查非空与局部归属，不在生成时创建替代的统一引用组件。
- 游戏诊断、完整UI/地形/截图/自然推进/长测驱动和外置正式配置测试工具都改为查询具体View；未启动长测或模拟器。

## 本轮验证

已完成本次移除与功能验证，不沿用57结果。用户要求暂不评分，无桌面/模拟器操作，性能验收仍暂缓。

|实际证据|结果|
|---|---|
|开发构建|成功，错误0；300项编辑器检查通过，含无全局引用代理、具体View所有控件非空且归属于自身预制体、缺脚本和CanvasRenderer检查；Artifacts/EditorChecks58.json|
|NoRefsFramework58|31项通过、errors为空，原Player35780退出0；类型化查询、缓存/销毁/迟到回收、Model独立状态、关闭数据清理、真实升级/结算/返回菜单|
|NoRefsUI58|273项通过、errors为空，原Player18456退出0；五画幅175张原始截图及连续动效，选择/设置/成长/构筑/地图、真实祭坛/升级、胜利/结算/死亡流程|
|旧GUID扫描|UIRefs脚本和meta均不存在，Assets下所有prefab/unity/asset没有旧组件GUID引用；Artifacts/NoRefs58AssetAudit.json|
|外置工具编译|FormalFlowHarness.cs针对当前程序集编译退出0；Artifacts/NoRefs58HarnessCompile.txt。仅编译兼容性，不宣称本轮执行正式包自然流程|

框架和UI回归使用同一开发Player。1456个主树现存源码/资源/配置与构建快照完全一致，无不同或额外文件。NoRefs58WorkspaceIdentity完整判定仍为false：快照含主树开工前已删除的自动生成link.xml/meta及隔离副本中的旧生成暂存文件三个缺失项；不改写快照，不将现存文件核对冒充全文件身份通过。

300项检查后的15个UI预制体与两个生成文件均先比较和备份，再同步至主树，备份目录Artifacts/GeneratedBackup-20261023-142319。保留主树原有link.xml/meta删除状态；其它既有meta/GUID未改变。

首轮编译发现迁移脚本误改preview名称，以及旧诊断中不合法的可空赋值/布尔判断，修订后成功；失败日志保留RemoveUIRefs58CompileAttempt1.log。本次未运行正式包自然长流程、两小时压力或美术评分，也未操作用户编辑器。
