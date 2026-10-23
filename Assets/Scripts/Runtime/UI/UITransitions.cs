using UnityEngine;

namespace VampireLike
{
    internal sealed class UIPanelMotion
    {
        readonly GameObject panel;
        readonly CanvasGroup group;
        readonly Surface[] surfaces;
        readonly bool interactive, animate;
        float openedAt;
        bool opening;
        readonly struct Surface
        {
            public readonly RectTransform Rect;
            public readonly Vector2 Position;
            public readonly Vector3 Scale;
            public Surface(RectTransform rect) { Rect = rect; Position = rect.anchoredPosition; Scale = rect.localScale; }
        }
        public UIPanelMotion(GameObject panel, bool interactive, bool animate)
        {
            this.panel = panel; this.interactive = interactive; this.animate = animate;
            // Unity missing components can be fake-null in the Editor; ?? does not use Unity's null comparison.
            group = panel.GetComponent<CanvasGroup>();
            if (group == null) group = panel.AddComponent<CanvasGroup>();
            if (interactive)
            {
                surfaces = new Surface[panel.transform.childCount];
                for (int i = 0; i < surfaces.Length; i++) surfaces[i] = new Surface((RectTransform)panel.transform.GetChild(i));
            }
            else surfaces = new[] { new Surface((RectTransform)panel.transform) };
            Reset();
        }
        public void Begin()
        {
            if (!animate) { Reset(); return; }
            openedAt = Time.unscaledTime; opening = true;
            group.blocksRaycasts = interactive; group.interactable = false; Apply(0);
        }
        public void Tick()
        {
            if (!opening) return;
            if (panel == null || !panel.activeInHierarchy) { Reset(); return; }
            float t = Mathf.Clamp01((Time.unscaledTime - openedAt) / .24f);
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
                surface.Rect.anchoredPosition = surface.Position * scale + anchorOffset * (scale - 1) + Vector2.down * (interactive ? 18 : 10) * (1 - progress);
                surface.Rect.localScale = surface.Scale * scale;
            }
        }
        public void Reset()
        {
            opening = false;
            if (group != null) { group.alpha = 1; group.interactable = interactive; group.blocksRaycasts = interactive; }
            foreach (var surface in surfaces) if (surface.Rect != null) { surface.Rect.anchoredPosition = surface.Position; surface.Rect.localScale = surface.Scale; }
        }
    }
}
