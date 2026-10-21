# 万象远征

Unity 2022.3.62f2 制作的中文、单人离线 3D 俯视自动攻击游戏。魔法、枪械与剑修自由混搭，不设武器/被动槽位；包含任务解锁、四群系与混合无限地图、主动 Boss 挑战和自由撤离。

**当前为发布候选，尚未完成成品验收。** 最新实际测试和未完成项见 [实施记录](Docs/ImplementationStatus.md)，独立评分见 [美术评审](Docs/ArtReview.md)。不得将候选包当作已达标成品。

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
.\Tools\RunVerification.ps1 -Stage Soak -SoakSeconds 7200
```

构建工具的 Unity 路径默认是 `C:\Program Files\Unity 2022.3.62f2\Editor\Unity.exe`，可在工具脚本中修改。构建输出在 `Builds/Development` 和 `Builds/Windows`；原始日志、截图、连续帧和报告在 `Artifacts`。后台验证仅对开发版有效，真实玩法与受控诊断的边界在实施记录中说明。

## 文档

- [游玩与存档说明](Docs/PlayerGuide.md)
- [确认的设计](Docs/GameDesign.md)
- [工程约束](Docs/Architecture.md)
- [完整工作流](Docs/DevelopmentWorkflow.md)
- [首发内容范围](Docs/ContentScope.md)
- [实际验证状态](Docs/ImplementationStatus.md)
- [独立美术验收](Docs/ArtReview.md)

字体的 SIL OFL 许可证随源文件保存在 `Assets/Art/UI/Fonts/OFL.txt`。项目保留所有生成资源、配置、Addressables 设置与 `.meta`，可重新构建。
