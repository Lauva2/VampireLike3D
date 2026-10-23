using System;
using System.Collections.Generic;
using UnityEngine;

namespace VampireLike.Editor
{
    public static class UIPrefabFactory
    {
        public static readonly Dictionary<UIPanelId, GameObject> Panels = new Dictionary<UIPanelId, GameObject>();
        public static GameObject Split(GameObject source, IReadOnlyList<UIView> views, UIWidget card, UIWidget row, UIWidget ability)
        {
            Panels.Clear();
            foreach (var panel in views)
            {
                var id = panel.id = IdFor(panel);
                AddTemplates(panel, card, row, ability);
                panel.layer = id == UIPanelId.HUD ? UILayer.Bottom : id == UIPanelId.Toast || id == UIPanelId.DamageFlash || id == UIPanelId.Loading ? UILayer.Top : id == UIPanelId.Pause || id == UIPanelId.Settings || id == UIPanelId.Upgrade || id == UIPanelId.Victory || id == UIPanelId.Results ? UILayer.Pop : UILayer.Common;
                panel.cache = id != UIPanelId.Upgrade && id != UIPanelId.Results && id != UIPanelId.Victory && id != UIPanelId.Loading && id != UIPanelId.Toast;
                var go = panel.gameObject; go.transform.SetParent(null, false); go.SetActive(true);
                Panels.Add(id, ArtFactory.SavePrefab(go, "UI", id.ToString()));
            }
            for (int i = source.transform.childCount - 1; i >= 0; i--) UnityEngine.Object.DestroyImmediate(source.transform.GetChild(i).gameObject);
            var root = source.AddComponent<UIRoot>();
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var go = new GameObject(layer.ToString(), typeof(RectTransform)); var rect = (RectTransform)go.transform;
                rect.SetParent(source.transform, false); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                switch (layer)
                {
                    case UILayer.Bottom: root.Bottom = rect; break;
                    case UILayer.Common: root.Common = rect; break;
                    case UILayer.Pop: root.Pop = rect; break;
                    case UILayer.Top: root.Top = rect; break;
                }
            }
            return ArtFactory.SavePrefab(source, "UI", "UIRoot");
        }
        static UIPanelId IdFor(UIView view)
        {
            foreach (UIPanelId id in Enum.GetValues(typeof(UIPanelId)))
                if (UIRegistry.Get(id).ViewType == view.GetType()) return id;
            throw new InvalidOperationException("Unregistered View: " + view.GetType().Name);
        }
        static void AddTemplates(UIView view, UIWidget card, UIWidget row, UIWidget ability)
        {
            if (!(view is SelectionView || view is UpgradeView || view is SkillsView || view is ProgressView || view is HUDView)) return;
            var templates = new GameObject("Templates", typeof(RectTransform)); templates.transform.SetParent(view.transform, false); templates.SetActive(false);
            if (view is HUDView hud) { hud.abilityTemplate = UnityEngine.Object.Instantiate(ability, templates.transform, false); return; }
            var localCard = UnityEngine.Object.Instantiate(card, templates.transform, false);
            var localRow = UnityEngine.Object.Instantiate(row, templates.transform, false);
            if (view is SelectionView selection) { selection.cardTemplate = localCard; selection.rowTemplate = localRow; }
            else if (view is UpgradeView upgrade) { upgrade.cardTemplate = localCard; upgrade.rowTemplate = localRow; }
            else if (view is SkillsView skills) { skills.cardTemplate = localCard; skills.rowTemplate = localRow; }
            else if (view is ProgressView progress) { progress.cardTemplate = localCard; progress.rowTemplate = localRow; }
        }
    }
}
