using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace VampireLike.Editor
{
    public static class UIPrefabChecks
    {
        public static void Validate(GameCatalog catalog, Action<bool, string> check)
        {
            ValidateMotionDependency(check);
            check(typeof(UIManager).GetProperty("View") == null && typeof(UIView).GetProperty("Refs") == null && typeof(UIView).Assembly.GetType("VampireLike.UIRefs") == null,
                "UI uses concrete View fields without a global reference component or compatibility facade");
            var root = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Prefabs/UI/UIRoot.prefab");
            var layers = root.GetComponent<UIRoot>();
            check(layers != null && root.transform.childCount == 4 && root.GetComponentsInChildren<UIView>(true).Length == 0,
                "UIRoot serializes only four layers and no eagerly instantiated panels");
            if (layers == null) return;
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
                check(layers.Layer(layer) != null && layers.Layer(layer).name == layer.ToString() && layers.Layer(layer).GetSiblingIndex() == (int)layer,
                    "Authored UI layer order: " + layer);
            foreach (UIPanelId id in Enum.GetValues(typeof(UIPanelId)))
            {
                GameObject prefab = null;
                try { prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(catalog.Prefab(UIView.Address(id)).AssetGUID)); } catch { }
                var panel = prefab != null ? prefab.GetComponent<UIView>() : null;
                check(panel != null && panel.id == id, "Independent addressable UI prefab with its base settings: " + id);
                if (panel == null) continue;
                check(prefab.GetComponent<CanvasGroup>() != null, "Panel prefab serializes its CanvasGroup before activation: " + id);
                var binding = UIRegistry.Get(id);
                var inactive = new GameObject("Inactive" + id + "DependencyProbe", typeof(RectTransform));
                inactive.SetActive(false);
                try
                {
                    inactive.AddComponent(binding.ViewType);
                    check(inactive.GetComponent<CanvasGroup>() != null, "Inactive concrete View creates its required CanvasGroup: " + id);
                }
                finally { UnityEngine.Object.DestroyImmediate(inactive); }
                check(panel.GetType() == binding.ViewType && binding.ViewType.Name == id + "View", "Prefab mounts its own concrete UIView subclass: " + id);
                var model = binding.NewModel();
                check(model.GetType() == binding.ModelType && model.GetType().BaseType == typeof(UIModel) && model.GetType().Name == id + "Model",
                    "Panel owns its own concrete UIModel subclass: " + id);
                check(typeof(UIController).IsAssignableFrom(binding.ControllerType) && binding.ControllerType.Name == id + "Controller",
                    "Panel registers its own concrete UIController subclass: " + id);
                bool local = true;
                foreach (var field in panel.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)) continue;
                    var value = field.GetValue(panel) as UnityEngine.Object;
                    if (value == null) { local = false; continue; }
                    Transform target = value is GameObject go ? go.transform : value is Component component ? component.transform : null;
                    if (target != null && target != panel.transform && !target.IsChildOf(panel.transform)) local = false;
                }
                check(local, "UI View has no serialized references into other panels: " + id);
                check(panel.layer >= UILayer.Bottom && panel.layer <= UILayer.Top, "UI base layer setting is valid: " + id);
            }
        }
        static void ValidateMotionDependency(Action<bool, string> check)
        {
            var probe = new GameObject("MissingCanvasGroupMotionProbe", typeof(RectTransform));
            probe.SetActive(false);
            try
            {
                // Exercise the runtime constructor in the Editor, where missing components can be Unity fake-null objects.
                var motionType = typeof(UIView).Assembly.GetType("VampireLike.UIPanelMotion", true);
                var motion = Activator.CreateInstance(motionType, new object[] { probe, true, true });
                motionType.GetMethod("Begin").Invoke(motion, null);
                var group = probe.GetComponent<CanvasGroup>();
                check(group != null && group.alpha == 0 && !group.interactable && group.blocksRaycasts,
                    "Motion adds a usable CanvasGroup to an inactive legacy panel before opening in the Editor");
                motionType.GetMethod("Reset").Invoke(motion, null);
                check(group.alpha == 1 && group.interactable && group.blocksRaycasts,
                    "Motion restores interaction and alpha after opening is canceled");
            }
            finally { UnityEngine.Object.DestroyImmediate(probe); }
        }
    }
}
