# 启动面板 CanvasGroup 修复

2026-10-23：用户报告 Loading(Clone)、Menu(Clone) 在 UIPanelMotion.Begin/Apply 中出现 MissingComponentException。

原因：独立面板预制体没有保存 CanvasGroup，动画构造函数使用 `GetComponent<CanvasGroup>() ?? AddComponent<CanvasGroup>()`。Unity 编辑器返回的缺组件引用可能是特殊空引用，C# 的 `??` 不调用 Unity 的空值比较，因此没有补加组件。此前后台 Player 测试未覆盖这种编辑器行为。

修复：动画构造函数显式用 Unity 的 `== null` 判断；UIView 基类声明 RectTransform 和 CanvasGroup 依赖，重新生成全部 14 个独立面板预制体。保持原有 GUID、四层结构、缓存设置和 AA 实例所有权。

回归：编辑器以未挂 View、未激活且缺少 CanvasGroup 的对象调用真实动画构造函数和 Begin/Reset，检查补加组件、透明度及输入恢复；逐个检查 14 个预制体序列化依赖和未激活具体 View 的依赖添加。后台框架回归增加 Menu/Loading 动画完成后的 CanvasGroup 状态检查。

修复前的检查已在后台编辑器复现相同 MissingComponentException，原日志保存在 `Artifacts/CanvasGroup59BeforeFix.log`。

修复后实际验证：

- `Tools/Build.ps1 -Mode UI` 成功，330 项编辑器检查通过；包含旧对象缺组件回归、14 个预制体依赖和14个未激活 View 的依赖检查。证据为 `Artifacts/EditorChecks59.json`、`Artifacts/CanvasGroup59Build.log`。
- 14 个面板预制体已比较、备份和同步到主项目，备份为 `Artifacts/GeneratedBackup-20261023-144703`。生成器恢复的 link.xml/meta 已移除，以保留原有删除状态。
- `Tools/RunVerification.ps1 -Stage UIFramework -Evidence Artifacts/CanvasGroupFramework59` 的 33 项检查通过，errors 为空，原 Player 14572 退出 0。覆盖 Menu/Loading 动画完成、缓存复用/销毁、异步取消/失败回收、进局、升级和结算返回菜单。

未操作用户桌面或编辑器，未执行美术评分、性能长测或本轮全画幅 UI 回归。让 Unity 完成脚本编译后，从 Boot 重新进入运行以替换此前已创建的旧实例。修复前的沙盒检查曾因许可证连接限制退出199，获准在沙盒外运行后台编辑器后才复现；未将该失败记作功能回归通过。
