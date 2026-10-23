# UI MVC、四层与独立面板（最终功能回归56）

> 本页保留初轮实现和56回归历史。用户随后要求每个面板独立三份脚本，当前基类为UIView/UIController/UIModel，各面板具体类型和57新回归见[PanelMVC.md](PanelMVC.md)。下方UIPanel/GamePanelController等类名只描述已替换的初版。

用户于2026-10-23要求：UIRoot拆分Bottom/Common/Pop/Top，面板独立预制体，按需创建，基类可选择layer，以cache控制关闭是否销毁。

## 编辑方式

打开Assets/Art/Prefabs/UI中的具体面板预制体，在UIPanel组件上设置：

|字段|含义|
|---|---|
|id|面板稳定标识，对应AA地址ui/<名称小写>|
|layer|创建后挂载的UIRoot层级：Bottom/Common/Pop/Top|
|cache|true关闭后隐藏并保留实例；false关闭后销毁、释放AA实例租约，下次重新加载|

默认Bottom放HUD，Common放菜单/选择/构筑/地图/成长，Pop放暂停/设置/升级/胜利/结算，Top放加载/提示/受击反馈。默认升级、胜利、结算、加载、提示不缓存，其他面板缓存；这些是可在预制体覆盖的初始配置。

UIRoot不持有面板或共享模板的序列化引用。每个需要列表的面板自带其隐藏模板，因此关闭销毁时没有跨面板控件依赖。模板只属于该面板内容，不是初始化时创建的其他页面。

## 代码入口

UIManager保留ShowMenu、ShowSelection、ToggleSkills等流程入口。首次调用异步创建该独立AA预制体，后续显示缓存或重新加载；旧调用方不得立即访问UIManager.View中尚未加载的字段，需要yield return UI.WaitForPanels()。主逻辑无需通过View直接控制其他面板。

UIModel保存展示上下文；UIPanel/UIRefs构成View；UIPanelController处理生命周期与订阅，GamePanelController处理各实例的控件绑定、内容展示和请求。控制器不自行发放奖励或绕过服务校验。UIManager统一管理四层挂载、模态输入、暂停、AA作用域、缓存与释放。

ClearCache只销毁已关闭的缓存；活动页面保留。Shutdown在GameManager释放AssetService之前统一回收所有页面，避免根销毁后仍存在面板租约。关闭加载中的面板将scope作废，AA完成后回收迟到实例。

保留原0.24秒打开动效与非缩放时钟。当前页内容刷新不重启动画；面板关闭时复位动效并解除自己的UI暂停；再次打开缓存重新绑定监听，避免一次点击执行多次。

## 复现

`Tools/Build.ps1 -Mode UI`在.build/Project隔离生成和构建，只更新UI及目录登记，不重新生成其他世界资源。生成成功后逐文件比较并备份，再同步允许的生成文件。普通Development/Release构建直接使用已保存预制体。

`Tools/RunVerification.ps1 -Stage UIFramework -Evidence Artifacts/UIFramework55`运行后台新档专项；UI阶段保留五画幅、真实祭坛、升级、结算和动效检查。均不操作桌面、不启动或关闭MuMu，不进行暂缓的性能长测。

## 当前实际状态

本次目标已实现。用户随后明确本次暂不进行子代理评分，已停止该评分任务；不将54分数用于本次重构，不进行暂缓的性能长测或模拟器操作。

实际开发构建成功，错误0；257项编辑器检查全部通过，包含四层顺序、14个独立AA面板、面板本地引用和原CanvasRenderer依赖检查。UIRoot仅四个层级，无预先创建的面板；AA UI组登记root及14个面板，共15个地址。

|本次最终实际回归|结果|
|---|---|
|Artifacts/UIFramework56/Verification.json|26项通过，errors为空；原Player34956退出0|
|Artifacts/UI56/Verification.json|274项通过，errors为空；原Player20684退出0|
|五画幅UI矩阵|175张实际截图；覆盖选择、永久成长、构筑、地图、设置、暂停、真实祭坛和升级、胜利、结算，以及打开动效|
|运行范围|新档进入真实玩法、缓存重开、升级发放、祭坛取消/领取/陈旧回调、致命伤害、返回菜单及清理；使用程序输入与Camera.Render，不操作桌面|

两个最终回归的BuildIdentity文件集合和SHA一致，使用同一开发Player。1355个主树现存源码/资源/设置文件与该构建快照完全一致，无不同或额外文件；完整Workspace56Identity仍为passed=false：快照还列出隔离项目中自动生成的link.xml/meta（主树开工前已删除，保持该状态）及一次资源同步留下、随后已清理的暂存文件。该暂存文件不参与运行内容；不把此有限核对写成完整工作树身份通过，不改写原始快照。正式配置、整局长流程与性能不在本次验收范围。

保留首轮失败：UIFramework55Attempt1因沙箱拒绝原子替换测试存档，以及加载暂存节点成为第五个根子节点而失败；节点现放Common内，最终获准的后台Player以独立测试档重测通过。UI55旧诊断未等待菜单加载便访问按钮而中断，已补等待，保留Interrupted及原退出记录；UI56重新完整通过。

生成文件同步均先逐文件核对并备份到Artifacts/GeneratedBackup-*，未覆盖修改中的源码。同步脚本修正Windows PowerShell 5.1的JSON数组嵌套枚举问题；第一次同步报告0文件不当作成功，本次实际完成48项生成资源同步。主树原有.idea目录和用户编辑器/模拟器保持原状。Unity生成YAML保留其默认空字段格式，不为消除尾空白改写生成资源。
