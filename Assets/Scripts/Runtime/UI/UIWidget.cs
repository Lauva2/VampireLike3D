using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace VampireLike
{
    public sealed class UIWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Text title, body, meta, icon;
        public Image accent, background;
        public Button button;
        public AbilityIcon symbol;
        public RawImage portrait, preview;
        public AbilityIcon lockIcon;
        public Outline selectionOutline;
        bool hover;
        public void OnPointerEnter(PointerEventData data) { hover = button != null && button.interactable; }
        public void OnPointerExit(PointerEventData data) { hover = false; }
        void OnDisable() { hover = false; transform.localScale = Vector3.one; }
        void Update() { transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * (hover ? 1.018f : 1), 1 - Mathf.Exp(-16 * Time.unscaledDeltaTime)); }
    }
}
