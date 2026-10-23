# 每个面板独立的MVC脚本

2026-10-23用户要求三个基类UIView、UIController、UIModel，每个面板各有三份派生脚本。此前共享的GamePanelController和包含全部页面数据的UIModel被替换；本文优先于UIFramework55.md中的旧类名说明。

## 三个基类

- UIView：MonoBehaviour，挂在预制体根节点，保留序列化id、layer、cache，负责展示Model数据和控件表现。各具体View直接序列化自己的控件，例如SettingsView.musicSlider和SelectionView.characterList；UIRefs组件及全局引用适配集合均已移除。
- UIModel：普通C#数据基类，Refresh读取展示快照，OnClose清理临时数据，不持有预制体、Canvas控件或AA租约。各面板的数据互相独立；Model可独立于View实例生存，选择和筛选不会因为销毁View而丢失。
- UIController：普通C#控制器基类，负责绑定/解绑、刷新和用户请求。泛型UIController<TView,TModel>把每个具体Controller绑定到对应具体View/Model，避免跨页面误用。购买、发奖及解锁仍由原业务模块校验。

UIManager只编排打开/关闭、层级、缓存、暂停与AA作用域。UIRegistry集中登记每一组具体类型和构造函数，运行时与编辑器均检查登记类型和预制体组件一致。

## 查询已加载的面板

Controller通过自己的强类型View访问控件。外部诊断在等待UI.WaitForPanels()后，可用UI.GetView<SettingsView>(UIPanelId.Settings)查询具体View，查询不会触发加载，未创建或类型不匹配返回null。Canvas与整体层级通过UI.Root访问，不保留全部面板控件的聚合代理。UIFactory在生成时直接绑定各具体View字段；列表模板归属对应View，无跨面板引用。

移除UIRefs后的新一轮验证另见[移除记录](UIRefsRemoval.md)，下方57结果保留为此次删除前的MVC拆分历史。

## 文件布局

每个面板位于Assets/Scripts/Runtime/UI/Panels/<名称>/，包含<名称>View.cs、<名称>Controller.cs、<名称>Model.cs，共14组三份脚本。预制体仍位于Assets/Art/Prefabs/UI，并挂载对应具体View。

|面板|具体职责|
|---|---|
|Menu|晶石展示；启程、成长、设置、退出请求|
|Selection|角色/地图选择状态、列表刷新、解锁信息和开始请求|
|HUD|生命、经验、闪避、Boss、目标、武器栏与HUD按钮|
|Upgrade|升级四选一与祭坛三选一数据、独立布局、重抽/跳过/领取请求，关闭清理旧offer和owner|
|Skills|筛选、定向点、祝福层数与购买请求|
|Pause|暂停文案与继续、设置、撤离请求|
|Map|地图偏移、定位和坐标展示|
|Progress|永久成长、任务列表与属性购买请求|
|Settings|声音、画质、全屏展示状态和返回上下文|
|Results|结算展示快照与返回菜单请求|
|Victory|Boss胜利信息及继续/撤离请求|
|Loading|加载提示与进度展示|
|Toast|轻量提示和到期关闭，不阻挡输入|
|DamageFlash|受击边缘颜色、非缩放时钟衰减和到期关闭，不阻挡输入|

四层挂载、AA异步按需实例化、缓存复用、关闭中加载失效回收、0.24秒打开动效保持原约定。过期面板在Controller更新结束后统一关闭，避免枚举过程中删除集合；关闭会解绑当前Controller，再调用Model.OnClose清理临时数据。

## 后续新增面板

1. 新建NameView : UIView、NameModel : UIModel、NameController : UIController<NameView,NameModel>。
2. 在UIPanelId与UIRegistry加入登记；预制体挂NameView，配置layer/cache及本地控件。
3. 预制体放Art/Prefabs/UI，登记稳定AA地址ui/<名称小写>，通过UIManager入口填入对应Model后打开。
4. 在Controller.Bind绑定请求，Render更新Model并交给View.Present；关闭由基类解绑和Model.OnClose清理。不要在View直接发放奖励或写进度。

## 本轮验证状态

已实现并通过本轮功能验证，未沿用旧56结果。用户明确本次不进行子代理评分；无桌面/模拟器操作，无性能长测。

|实际验证|结果|
|---|---|
|编辑器检查|299项通过；包含14组具体类型登记、对应View预制体组件、Model继承和Controller注册，Artifacts/EditorChecks.json|
|MVCFramework57|30项通过，errors为空；原Player20408正常退出0。检查具体类型、独立Model、跨View生命周期保留状态、缓存/销毁/AA回收、升级Model临时数据清理|
|MVCUI57|275项通过，errors为空；原Player34428正常退出0。五画幅175张原始截图、弹窗动效、选择/筛选/设置、祭坛取消/领取/陈旧回调、真实升级、胜利/结算、死亡与返回菜单流程|
|版本归属|两个回归使用同一开发Player；BuildIdentity文件集合和SHA一致。1458个主树现存源码/资源/配置与构建快照一致，无不同或额外文件|

完整MVC57WorkspaceIdentity仍为passed=false，保留三个缺失项：隔离项目自动生成的link.xml/meta（主树开工前已有删除，保留该状态），以及隔离副本中旧的生成暂存文件。暂存文件不参与运行内容；不将有限现存文件核对宣称为全文件精确身份通过，不改原始快照。生成比较工具现忽略.tmp，防止把暂存文件当作正式生成资源。

14个面板预制体逐项核对后备份并同步，目录Artifacts/GeneratedBackup-20261023-135645，原prefab.meta/GUID保持。旧脚本被重命名/移除后，构建工具会先备份并清理验证副本中的过期脚本，避免Copy-Item遗留旧集中控制器；不删除主工程脚本来迁就验证副本。

本次范围是UI实现与功能验证，不是新版整体成品、正式包、性能或美术评分验收。旧GamePanelController和UIPanelController已经从运行时源码移除；旧UIPanel的枚举迁至UIIds，控制器基类改为UIController并保留对应脚本meta/GUID。
