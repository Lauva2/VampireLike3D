using UnityEngine;

namespace VampireLike
{
    public sealed partial class UIManager
    {
        const float PopupOpenSeconds = .24f;
        PopupMotion[] panelMotions;
        PopupMotion activeMotion, toastMotion;

        // UIManager owns these presentation states; they never own AA resources or flow.
        sealed class PopupMotion
        {
            public readonly GameObject Panel;
            readonly CanvasGroup group;
            readonly Surface[] surfaces;
            readonly bool interactive;
            float openedAt;
            bool opening;

            readonly struct Surface
            {
                public readonly RectTransform Rect;
                public readonly Vector2 Position;
                public readonly Vector3 Scale;
                public Surface(RectTransform rect) { Rect = rect; Position = rect.anchoredPosition; Scale = rect.localScale; }
            }
            public PopupMotion(GameObject panel, bool interactive)
            {
                Panel = panel; this.interactive = interactive;
                group = panel.GetComponent<CanvasGroup>();
                if (group == null) group = panel.AddComponent<CanvasGroup>();
                if (interactive)
                {
                    // Preserve authored hierarchy/refs and the full-screen raycast backdrop.
                    // Every top-level surface receives the same transform around panel center.
                    surfaces = new Surface[panel.transform.childCount];
                    for (int i = 0; i < surfaces.Length; i++) surfaces[i] = new Surface((RectTransform)panel.transform.GetChild(i));
                }
                else surfaces = new[] { new Surface((RectTransform)panel.transform) };
                Reset();
            }
            public void Begin()
            {
                openedAt = Time.unscaledTime; opening = true;
                group.blocksRaycasts = interactive; group.interactable = false;
                Apply(0);
            }
            public void Tick()
            {
                if (!opening) return;
                if (Panel == null || !Panel.activeInHierarchy) { Reset(); return; }
                // Do not count the opening frame's pre-existing delta time.
                float t = Mathf.Clamp01((Time.unscaledTime - openedAt) / PopupOpenSeconds);
                // Cubic ease-out: immediate response, gentle arrival, no overshoot.
                Apply(1 - (1 - t) * (1 - t) * (1 - t));
                if (t >= 1) Reset();
            }
            void Apply(float progress)
            {
                group.alpha = progress;
                float scale = Mathf.Lerp(interactive ? .965f : .985f, 1, progress);
                foreach (var surface in surfaces)
                {
                    if (surface.Rect == null) continue;
                    var parent = (RectTransform)surface.Rect.parent;
                    var anchor = surface.Rect.anchorMin + Vector2.Scale(surface.Rect.anchorMax - surface.Rect.anchorMin, surface.Rect.pivot);
                    var anchorOffset = Vector2.Scale(parent.rect.size, anchor - parent.pivot);
                    surface.Rect.anchoredPosition = surface.Position * scale + anchorOffset * (scale - 1)
                        + Vector2.down * (interactive ? 18 : 10) * (1 - progress);
                    surface.Rect.localScale = surface.Scale * scale;
                }
            }
            public void Reset()
            {
                opening = false;
                if (group != null) { group.alpha = 1; group.interactable = interactive; group.blocksRaycasts = interactive; }
                foreach (var surface in surfaces)
                    if (surface.Rect != null) { surface.Rect.anchoredPosition = surface.Position; surface.Rect.localScale = surface.Scale; }
            }
        }
        void PrepareTransitions()
        {
            var panels = new[] { view.menu, view.selection, view.upgrade, view.skills, view.pause,
                view.map, view.progress, view.settings, view.results, view.victory, view.loading };
            panelMotions = new PopupMotion[panels.Length];
            for (int i = 0; i < panels.Length; i++) panelMotions[i] = new PopupMotion(panels[i], true);
            if (texts.TryGetValue("ToastText", out var text)) toastMotion = new PopupMotion(text.transform.parent.gameObject, false);
        }
        void BeginPanelTransition(GameObject panel)
        {
            foreach (var motion in panelMotions)
                if (motion.Panel == panel) { activeMotion = motion; motion.Begin(); return; }
        }
        void ResetPanelTransition() { activeMotion?.Reset(); activeMotion = null; }
        void TickTransitions() { activeMotion?.Tick(); toastMotion?.Tick(); }
        void OnDisable() { ResetPanelTransition(); ClearToast(); }
    }
}
