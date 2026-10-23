#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        IEnumerator UIFrameworkRegression()
        {
            var ui = game.UI; yield return ui.WaitForPanels();
            Check(ui.Root.transform.childCount == 4 && ui.Root.Bottom.GetSiblingIndex() == 0 && ui.Root.Top.GetSiblingIndex() == 3,
                "UIRoot contains exactly Bottom Common Pop Top in rendering order");
            Check(ui.GetPanel(UIPanelId.Menu) != null && ui.PanelCount == 1 && ui.GetPanel(UIPanelId.Selection) == null && ui.GetPanel(UIPanelId.HUD) == null,
                "Startup instantiates only the requested menu, not every panel");
            yield return Wait(.3f);
            var menuGroup = ui.GetPanel(UIPanelId.Menu).GetComponent<CanvasGroup>();
            Check(menuGroup != null && menuGroup.alpha == 1 && menuGroup.interactable && menuGroup.blocksRaycasts,
                "Startup menu completes its CanvasGroup animation and accepts input");
            int baseline = game.Assets.InstanceCount;
            Check(ui.GetView<MenuView>(UIPanelId.Menu) != null && ui.GetView<HUDView>(UIPanelId.Menu) == null && typeof(UIManager).GetProperty("View") == null,
                "Typed lookup rejects a different panel type and no global widget reference facade remains");
            var selectionModel = (SelectionModel)ui.GetModel(UIPanelId.Selection);
            var skillsModel = (SkillsModel)ui.GetModel(UIPanelId.Skills);
            selectionModel.selectedCharacter = "adventurer"; skillsModel.filter = "全部";
            Check(ui.GetPanel(UIPanelId.Menu) is MenuView && ui.GetModel(UIPanelId.Menu) is MenuModel && !ReferenceEquals(selectionModel, skillsModel),
                "Concrete panel View and independently typed Models are used at runtime");
            ui.ShowSettings(); ui.ShowSettings(); ui.ShowSettings(); yield return ui.WaitForPanels(); yield return Wait(.3f);
            var settings = ui.GetPanel(UIPanelId.Settings);
            Check(settings != null && settings.transform.parent == ui.Root.Pop && game.Assets.InstanceCount == baseline + 1,
                "Concurrent opens coalesce into one AA instance in the authored Pop layer");
            ui.CloseModal(); yield return ui.WaitForPanels();
            Check(settings != null && !settings.gameObject.activeSelf && ui.GetPanel(UIPanelId.Settings) == settings,
                "cache true retains a hidden panel and its AA lease");
            ui.ShowSettings(); yield return ui.WaitForPanels(); yield return Wait(.3f);
            Check(ui.GetPanel(UIPanelId.Settings) == settings, "Cached reopening reuses the original instance");
            Check(settings is SettingsView && ui.GetModel(UIPanelId.Settings) is SettingsModel, "Settings uses its own typed View and Model through the registry");
            int quality = game.Save.Profile.quality;
            settings.transform.Find("QualityButton").GetComponent<Button>().onClick.Invoke();
            Check(game.Save.Profile.quality == (quality + 1) % 3, "Repeated cached opens bind each controller action exactly once");
            settings.cache = false; ui.CloseModal(); yield return null;
            Check(settings == null && ui.GetPanel(UIPanelId.Settings) == null && game.Assets.InstanceCount == baseline,
                "cache false destroys the panel and releases the matching AA instance lease");
            ui.ShowSettings(); yield return ui.WaitForPanels();
            Check(ui.GetPanel(UIPanelId.Settings) != null, "A destroyed panel can load and bind again");
            ui.CloseModal(); ui.ClearCache(); yield return null;
            Check(game.Assets.InstanceCount == baseline, "Explicit cache clearing releases closed panel leases");
            ui.ShowProgress(); ui.CloseModal(); yield return Wait(.7f);
            Check(ui.GetPanel(UIPanelId.Progress) == null && game.Assets.InstanceCount == baseline && ui.ActiveModal == "menu",
                "Close during AA creation discards late results without a modal or instance leak");
            game.Assets.DiagnosticFailNextPrefab(UIView.Address(UIPanelId.Progress));
            ui.ShowProgress(); yield return ui.WaitForPanels(); yield return Wait(.2f);
            Check(ui.GetPanel(UIPanelId.Progress) == null && ui.ActiveModal == "menu" && game.Assets.InstanceCount == baseline,
                "Failed panel load returns to a usable menu and releases the failed scope");
            ui.ShowSelection(); yield return ui.WaitForPanels(); yield return Wait(.3f);
            Check(ui.GetPanel(UIPanelId.Selection)?.gameObject.transform.parent == ui.Root.Common && ui.GetView<SelectionView>(UIPanelId.Selection)?.characterList.childCount == 3 && ui.GetView<SelectionView>(UIPanelId.Selection)?.biomeList.childCount == 7,
                "Independent selection View renders model content in Common");
            Check(ReferenceEquals(selectionModel, ui.GetModel(UIPanelId.Selection)), "Panel Model state survives View creation and cache changes independently");
            yield return Capture("mvc-selection"); ui.CloseModal(); ui.ClearCache();
            ui.ShowLoading("独立加载面板"); yield return ui.WaitForPanels();
            var loading = ui.GetPanel(UIPanelId.Loading);
            Check(loading.transform.parent == ui.Root.Top && !loading.cache, "Loading is independently authored on Top with destruction on close");
            yield return Wait(.3f);
            var loadingGroup = loading.GetComponent<CanvasGroup>();
            Check(loadingGroup != null && loadingGroup.alpha == 1 && loadingGroup.interactable && loadingGroup.blocksRaycasts,
                "Loading completes its CanvasGroup animation without a missing component");
            ui.ShowMenu(); yield return ui.WaitForPanels(); yield return null;
            Check(loading == null && game.Assets.InstanceCount == baseline, "Replacing loading with menu releases the old panel");
            game.BeginRun("adventurer", BiomeId.Forest);
            float deadline = Time.realtimeSinceStartup + 90;
            while (!game.Ready && Time.realtimeSinceStartup < deadline) yield return null;
            Check(game.Ready, "MVC regression enters real gameplay"); if (!game.Ready) yield break;
            yield return ui.WaitForPanels();
            Check(ui.GetPanel(UIPanelId.HUD)?.gameObject != null && ui.GetPanel(UIPanelId.HUD)?.gameObject.transform.parent == ui.Root.Bottom, "HUD loads on first gameplay entry in Bottom");
            ui.TogglePause(); yield return ui.WaitForPanels(); yield return Wait(.3f);
            Check(game.IsPaused && ui.ActiveModal == "pause", "Lazy popup reserves gameplay input and pause while loading");
            ui.ShowSettings(); yield return ui.WaitForPanels(); ui.CloseModal(); yield return ui.WaitForPanels();
            Check(ui.ActiveModal == "pause" && game.IsPaused, "Settings return recreates or restores the parent pause modal");
            ui.CloseModal(); Check(!game.IsPaused, "Closing the nested popup releases UI pause immediately");
            ui.ToggleSkills(); yield return ui.WaitForPanels(); yield return Wait(.3f); yield return Capture("mvc-skills"); ui.CloseModal();
            ui.ToggleMap(); yield return ui.WaitForPanels(); yield return Wait(.3f); yield return Capture("mvc-map"); ui.CloseModal();
            game.Progression.Experience(game.Run.nextExperience); yield return ui.WaitForPanels(); yield return Wait(.3f);
            Check(ui.ActiveModal == "upgrade" && ui.GetView<UpgradeView>(UIPanelId.Upgrade)?.upgradeList.childCount > 0 && !ui.GetPanel(UIPanelId.Upgrade).cache,
                "Real XP creates an independent noncached upgrade View");
            yield return Capture("mvc-upgrade");
            var upgrade = ui.GetPanel(UIPanelId.Upgrade); game.Progression.SkipLevel(); yield return null;
            Check(upgrade == null && ui.ActiveModal == null && !game.IsPaused, "Choosing or skipping closes and destroys the upgrade panel");
            var upgradeModel = (UpgradeModel)ui.GetModel(UIPanelId.Upgrade);
            Check(upgradeModel.choices.Count == 0 && upgradeModel.blessings.Count == 0 && upgradeModel.blessingOwner == null,
                "Closing the concrete upgrade controller clears transient model offers and run owner references");
            ui.Toast("独立Top提示", .7f); yield return ui.WaitForPanels();
            Check(ui.GetPanel(UIPanelId.Toast)?.gameObject.transform.parent == ui.Root.Top && ui.GetPanel(UIPanelId.Toast)?.gameObject.GetComponent<CanvasGroup>().blocksRaycasts == false,
                "Toast is lazy in Top and never intercepts gameplay input");
            ui.DamageFlash(); yield return ui.WaitForPanels();
            Check(ui.GetPanel(UIPanelId.DamageFlash)?.gameObject.transform.parent == ui.Root.Top, "Damage feedback has its own lazy Top View");
            yield return Wait(.8f); Check(ui.GetPanel(UIPanelId.Toast) == null, "Expired noncached toast releases its AA instance");
            game.FinishRun(true); yield return ui.WaitForPanels(); yield return Wait(.3f); yield return Capture("mvc-results");
            Check(ui.GetPanel(UIPanelId.Results)?.gameObject != null && ui.ActiveModal == "results", "Settlement renders through its independent controller");
            game.ReturnToMenu(); deadline = Time.realtimeSinceStartup + 90;
            while (!game.CanBeginRun && Time.realtimeSinceStartup < deadline) yield return null;
            yield return ui.WaitForPanels(); ui.ClearCache(); yield return null;
            Check(game.CanBeginRun && ui.ActiveModal == "menu" && !game.IsPaused && game.Assets.InstanceCount == baseline,
                "Return to menu and cache disposal leave only the root and active menu leases");
            yield return Capture("mvc-return-menu");
        }
    }
}
#endif
