using System;
using System.Collections.Generic;
namespace VampireLike
{
    public static class UIRegistry
    {
        public sealed class Binding
        {
            public Type ViewType, ModelType, ControllerType;
            public Func<UIModel> NewModel;
            public Func<UIManager, GameManager, UIModel, UIView, UIController> NewController;
        }
        static readonly Dictionary<UIPanelId, Binding> bindings = new Dictionary<UIPanelId, Binding>();
        static UIRegistry()
        {
            Register<MenuView, MenuModel, MenuController>(UIPanelId.Menu, (m, g, model, view) => new MenuController(m, g, model, view));
            Register<SelectionView, SelectionModel, SelectionController>(UIPanelId.Selection, (m, g, model, view) => new SelectionController(m, g, model, view));
            Register<HUDView, HUDModel, HUDController>(UIPanelId.HUD, (m, g, model, view) => new HUDController(m, g, model, view));
            Register<UpgradeView, UpgradeModel, UpgradeController>(UIPanelId.Upgrade, (m, g, model, view) => new UpgradeController(m, g, model, view));
            Register<SkillsView, SkillsModel, SkillsController>(UIPanelId.Skills, (m, g, model, view) => new SkillsController(m, g, model, view));
            Register<PauseView, PauseModel, PauseController>(UIPanelId.Pause, (m, g, model, view) => new PauseController(m, g, model, view));
            Register<MapView, MapModel, MapController>(UIPanelId.Map, (m, g, model, view) => new MapController(m, g, model, view));
            Register<ProgressView, ProgressModel, ProgressController>(UIPanelId.Progress, (m, g, model, view) => new ProgressController(m, g, model, view));
            Register<SettingsView, SettingsModel, SettingsController>(UIPanelId.Settings, (m, g, model, view) => new SettingsController(m, g, model, view));
            Register<ResultsView, ResultsModel, ResultsController>(UIPanelId.Results, (m, g, model, view) => new ResultsController(m, g, model, view));
            Register<VictoryView, VictoryModel, VictoryController>(UIPanelId.Victory, (m, g, model, view) => new VictoryController(m, g, model, view));
            Register<LoadingView, LoadingModel, LoadingController>(UIPanelId.Loading, (m, g, model, view) => new LoadingController(m, g, model, view));
            Register<ToastView, ToastModel, ToastController>(UIPanelId.Toast, (m, g, model, view) => new ToastController(m, g, model, view));
            Register<DamageFlashView, DamageFlashModel, DamageFlashController>(UIPanelId.DamageFlash, (m, g, model, view) => new DamageFlashController(m, g, model, view));
        }
        static void Register<TView, TModel, TController>(UIPanelId id, Func<UIManager, GameManager, TModel, TView, TController> create)
            where TView : UIView where TModel : UIModel, new() where TController : UIController
        {
            bindings.Add(id, new Binding { ViewType = typeof(TView), ModelType = typeof(TModel), ControllerType = typeof(TController), NewModel = () => new TModel(),
                NewController = (manager, game, model, view) => create(manager, game, (TModel)model, (TView)view) });
        }
        public static Binding Get(UIPanelId id) => bindings[id];
    }
}
