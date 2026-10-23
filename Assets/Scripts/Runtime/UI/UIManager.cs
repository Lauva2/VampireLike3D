using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class UIManager : MonoBehaviour
    {
        sealed class Entry
        {
            public AssetScope scope;
            public UIView panel;
            public UIController controller;
            public UIPanelMotion motion;
            public bool wanted, loading;
        }
        readonly Dictionary<UIPanelId, Entry> panels = new Dictionary<UIPanelId, Entry>();
        readonly Dictionary<UIPanelId, UIModel> models = new Dictionary<UIPanelId, UIModel>();
        readonly List<UIPanelId> expired = new List<UIPanelId>();
        string modal;
        public UIModel GetModel(UIPanelId id)
        {
            if (!models.TryGetValue(id, out var value)) { value = UIRegistry.Get(id).NewModel(); models.Add(id, value); }
            return value;
        }
        T Model<T>(UIPanelId id) where T : UIModel => (T)GetModel(id);
        internal readonly Dictionary<string, Texture2D> Portraits = new Dictionary<string, Texture2D>();
        internal readonly Dictionary<int, Texture2D> Landscapes = new Dictionary<int, Texture2D>();
        GameManager game;
        UIRoot root;
        bool disposed;
        public bool Ready => root != null;
        public bool BlocksGameplay => !string.IsNullOrEmpty(modal);
        public string ActiveModal => modal;
        public UIRoot Root => root;
        public T GetView<T>(UIPanelId id) where T : UIView => GetPanel(id) as T;
        public int PanelCount => panels.Count;
        public bool IsLoading { get { foreach (var e in panels.Values) if (e.loading && e.wanted) return true; return false; } }
        public UIView GetPanel(UIPanelId id) => panels.TryGetValue(id, out var e) ? e.panel : null;
        public IEnumerator WaitForPanels() { while (!disposed && IsLoading) yield return null; }
        public IEnumerator Initialize(GameManager manager, AssetScope scope)
        {
            game = manager;
            yield return game.Assets.Instantiate("ui/root", scope, null, instance =>
            {
                if (instance == null) return;
                DontDestroyOnLoad(instance); root = instance.GetComponent<UIRoot>();
            });
            if (!Ready) yield break;
            ShowLoading("正在整理远征行囊…"); yield return WaitForPanels();
            foreach (var c in game.Catalog.characters)
            {
                GameObject prefab = null;
                yield return game.Assets.LoadPrefab(c.prefabId, scope, p => prefab = p);
                if (prefab != null) Portraits[c.id] = PortraitRenderer.Create(prefab);
            }
            var previews = new AssetScope("selection-previews");
            GameObject pathPrefab = null; yield return game.Assets.LoadPrefab("path/network", previews, p => pathPrefab = p);
            foreach (var biome in game.Catalog.biomes)
            {
                GameObject terrain = null, decoration = null, architecture = null;
                yield return game.Assets.LoadPrefab(biome.terrainId, previews, p => terrain = p);
                yield return game.Assets.LoadPrefab(biome.decorationId, previews, p => decoration = p);
                yield return game.Assets.LoadPrefab("architecture/" + ContentIds.RegionKey(biome.id), previews, p => architecture = p);
                if (terrain != null && decoration != null) Landscapes[(int)biome.id] = PortraitRenderer.Landscape(terrain, decoration, architecture, pathPrefab, biome.id);
                yield return null;
            }
            if (Landscapes.Count == ContentIds.Regions.Length)
            {
                var mixed = new Texture2D(384, 160, TextureFormat.RGB24, false);
                for (int i = 0; i < ContentIds.Regions.Length; i++) mixed.SetPixels(i * 64, 0, 64, 160, Landscapes[(int)ContentIds.Regions[i]].GetPixels(160, 0, 64, 160));
                mixed.Apply(); Landscapes[(int)BiomeId.Mixed] = mixed;
            }
            game.Assets.Release(previews);
        }
        public void OpenPanel(UIPanelId id)
        {
            if (disposed || !Ready) return;
            if (panels.TryGetValue(id, out var e))
            {
                e.wanted = true; if (e.loading) return;
                bool opening = !e.panel.gameObject.activeSelf;
                e.panel.gameObject.SetActive(true);
                if (opening) { e.controller.Bind(); e.motion.Begin(); }
                e.controller.Render(); return;
            }
            e = new Entry { scope = new AssetScope("ui-" + id), wanted = true, loading = true };
            panels.Add(id, e); StartCoroutine(CreatePanel(id, e));
        }
        IEnumerator CreatePanel(UIPanelId id, Entry e)
        {
            var staging = new GameObject("UIStaging-" + id, typeof(RectTransform)); staging.SetActive(false);
            staging.transform.SetParent(root.Common, false);
            GameObject instance = null;
            yield return game.Assets.Instantiate(UIView.Address(id), e.scope, staging.transform, go => instance = go);
            if (disposed || !e.wanted || e.scope.Disposed) { if (staging != null) Destroy(staging); yield break; }
            if (instance == null || instance.GetComponent<UIView>() == null || instance.GetComponent<UIView>().id != id || !UIRegistry.Get(id).ViewType.IsInstanceOfType(instance.GetComponent<UIView>()))
            {
                Debug.LogWarning("UI panel load failed: " + id + " / " + game.Assets.LastError);
                e.loading = false; ClosePanel(id); Destroy(staging);
                if (modal == Reason(id) || (id == UIPanelId.Upgrade && modal == "blessing"))
                {
                    if (modal == "blessing") game.Progression?.Shrines.Cancel();
                    modal = null; game.Pause("ui", false);
                    if (game.Flow == GameFlow.Menu && id != UIPanelId.Menu) ShowMenu();
                    else if (id == UIPanelId.Menu) game.ReportUIFailure("主菜单加载失败，请检查Addressables内容后重新启动。");
                    else if (game.Ready) ShowHUD();
                }
                yield break;
            }
            e.panel = instance.GetComponent<UIView>();
            instance.SetActive(false); instance.transform.SetParent(root.Layer(e.panel.layer), false); Destroy(staging);
            e.controller = UIRegistry.Get(id).NewController(this, game, GetModel(id), e.panel);
            e.motion = new UIPanelMotion(instance, id != UIPanelId.Toast && id != UIPanelId.DamageFlash, id != UIPanelId.HUD && id != UIPanelId.DamageFlash);
            e.loading = false;
            e.controller.Bind(); e.controller.Render(); instance.SetActive(true); e.motion.Begin();
        }
        public void ClosePanel(UIPanelId id)
        {
            if (!panels.TryGetValue(id, out var e)) return;
            e.wanted = false;
            GetModel(id).OnClose();
            if (e.panel != null)
            {
                e.motion.Reset(); e.controller.Dispose(); e.panel.gameObject.SetActive(false);
                if (e.panel.cache) return;
            }
            panels.Remove(id); game.Assets.Release(e.scope);
        }
        public void ClearCache()
        {
            foreach (var id in new List<UIPanelId>(panels.Keys))
            {
                var e = panels[id]; if (e.wanted) continue;
                e.controller?.Dispose(); game.Assets.Release(e.scope); panels.Remove(id);
            }
        }
        static string Reason(UIPanelId id) => id == UIPanelId.HUD ? null : id.ToString().ToLowerInvariant();
        void Navigate(UIPanelId? id, string reason)
        {
            if (modal == "blessing" && reason != "blessing") game.Progression?.Shrines.Cancel();
            ClearToast(); modal = reason; game.Pause("ui", game.Ready && reason != null);
            foreach (var old in new List<UIPanelId>(panels.Keys))
                if (old != id && old != UIPanelId.HUD && old != UIPanelId.DamageFlash) ClosePanel(old);
            bool hud = game.Ready && (!id.HasValue || id == UIPanelId.Upgrade);
            if (hud) OpenPanel(UIPanelId.HUD); else ClosePanel(UIPanelId.HUD);
            if (id.HasValue) OpenPanel(id.Value);
        }
        public void ShowMenu() => Navigate(UIPanelId.Menu, "menu");
        public void ShowHUD() => Navigate(null, null);
        public void HideHUD() => ClosePanel(UIPanelId.HUD);
        public void ShowLoading(string message) { Model<LoadingModel>(UIPanelId.Loading).message = message; Model<LoadingModel>(UIPanelId.Loading).progress = 0; Navigate(UIPanelId.Loading, "loading"); }
        public void LoadingProgress(float progress) { Model<LoadingModel>(UIPanelId.Loading).progress = progress; Refresh(UIPanelId.Loading); }
        void Refresh(UIPanelId id) { if (panels.TryGetValue(id, out var e) && !e.loading && e.wanted) e.controller.Render(); }
        public void CloseModal()
        {
            if (modal == "results" || modal == "loading") return;
            game.Save.MarkDirty(); game.Save.Flush();
            if (modal == "settings" && Model<SettingsModel>(UIPanelId.Settings).returnModal == "pause") { Navigate(UIPanelId.Pause, "pause"); return; }
            if (game.Flow == GameFlow.Menu) ShowMenu(); else ShowHUD();
        }
        bool CanToggle() => game.Ready && modal != "upgrade" && modal != "victory" && modal != "loading";
        public void TogglePause() { if (!CanToggle()) return; if (modal != null) CloseModal(); else Navigate(UIPanelId.Pause, "pause"); }
        public void ToggleSkills() { if (!CanToggle()) return; if (modal == "skills") CloseModal(); else Navigate(UIPanelId.Skills, "skills"); }
        public void ToggleMap() { if (!CanToggle()) return; if (modal == "map") CloseModal(); else Navigate(UIPanelId.Map, "map"); }
        public void ShowSelection() => Navigate(UIPanelId.Selection, "selection");
        public void ShowProgress() => Navigate(UIPanelId.Progress, "progress");
        public void ShowSettings() { if (modal != "settings") Model<SettingsModel>(UIPanelId.Settings).returnModal = modal; Navigate(UIPanelId.Settings, "settings"); }
        public void ShowUpgrade(List<UpgradeDefinition> choices) { var state = Model<UpgradeModel>(UIPanelId.Upgrade); state.isBlessing = false; state.blessingOwner = null; state.choices = new List<UpgradeDefinition>(choices); Navigate(UIPanelId.Upgrade, "upgrade"); }
        public void ShowBlessings(List<BlessingDefinition> choices, int token)
        { var state = Model<UpgradeModel>(UIPanelId.Upgrade); state.isBlessing = true; state.blessings = new List<BlessingDefinition>(choices); state.blessingToken = token; state.blessingOwner = game.Progression.Shrines; Navigate(UIPanelId.Upgrade, "blessing"); }
        public void CloseUpgrade() => ShowHUD();
        public void ShowBossVictory(BiomeId biome) { Model<VictoryModel>(UIPanelId.Victory).biome = biome; Navigate(UIPanelId.Victory, "victory"); }
        public void ShowResults(bool extraction) { Model<ResultsModel>(UIPanelId.Results).extraction = extraction; Navigate(UIPanelId.Results, "results"); }
        public void Toast(string message, float duration = 3) { Model<ToastModel>(UIPanelId.Toast).message = message; Model<ToastModel>(UIPanelId.Toast).expires = Time.unscaledTime + duration; OpenPanel(UIPanelId.Toast); }
        void ClearToast() { Model<ToastModel>(UIPanelId.Toast).expires = 0; ClosePanel(UIPanelId.Toast); }
        public void DamageFlash() { Model<DamageFlashModel>(UIPanelId.DamageFlash).expires = Time.unscaledTime + .2f; OpenPanel(UIPanelId.DamageFlash); }
        public void RestoreSettings()
        {
            int quality = game.Save.Profile.quality;
            QualitySettings.shadows = quality == 0 ? ShadowQuality.Disable : ShadowQuality.All;
            QualitySettings.shadowDistance = quality == 2 ? 75 : 40; QualitySettings.antiAliasing = quality == 2 ? 4 : 0;
            game.Save.MarkDirty(); game.Audio.RefreshVolumes();
        }
        void Update()
        {
            expired.Clear();
            foreach (var pair in panels) if (!pair.Value.loading && pair.Value.wanted)
            {
                var entry = pair.Value; entry.motion.Tick(); entry.controller.Tick();
                if (entry.controller.Expired) expired.Add(pair.Key);
            }
            foreach (var id in expired) ClosePanel(id);
        }
        void OnDisable() { foreach (var e in panels.Values) e.motion?.Reset(); ClearToast(); }
        public void Shutdown()
        {
            if (disposed) return;
            disposed = true;
            foreach (var e in panels.Values) { e.controller?.Dispose(); game.Assets.Release(e.scope); }
            panels.Clear(); models.Clear();
            foreach (var texture in Portraits.Values) if (texture != null) Destroy(texture);
            foreach (var texture in Landscapes.Values) if (texture != null) Destroy(texture);
        }
        void OnDestroy() => Shutdown();
        public static string Icon(AbilityKind kind) => UIFormatting.Icon(kind);
    }
}
