using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public abstract class UIView : MonoBehaviour
    {
        public UILayer layer = UILayer.Common;
        public bool cache = true;
        public UIPanelId id;
        readonly Dictionary<string, Text> textIndex = new Dictionary<string, Text>();
        protected void SetText(string name, string value)
        {
            if (textIndex.Count == 0) foreach (var text in GetComponentsInChildren<Text>(true))
                if (!textIndex.ContainsKey(text.name)) textIndex.Add(text.name, text);
            if (textIndex.TryGetValue(name, out var target)) target.text = value;
        }
        protected void SetTextVisible(string name, bool visible)
        {
            SetText(name, textIndex.TryGetValue(name, out var current) ? current.text : "");
            if (textIndex.TryGetValue(name, out var target)) target.gameObject.SetActive(visible);
        }
        protected static void SetBar(Image fill, float value)
        {
            value = Mathf.Clamp01(value); fill.fillAmount = value; fill.enabled = value > .001f;
            fill.rectTransform.anchorMax = new Vector2(value, 1); fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;
        }
        public static string Address(UIPanelId id) => "ui/" + id.ToString().ToLowerInvariant();
    }
}
