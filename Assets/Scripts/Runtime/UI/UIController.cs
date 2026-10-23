using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VampireLike
{
    // Controllers bind only their own View and remove their own subscriptions on close.
    public abstract class UIController : IDisposable
    {
        protected readonly UIManager manager;
        protected readonly GameManager game;
        public UIModel Model { get; }
        public UIView Panel { get; }
        protected UIView panel => Panel;
        protected readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        readonly List<Action> unbind = new List<Action>();
        public UIController(UIManager manager, GameManager game, UIModel model, UIView panel)
        {
            this.manager = manager; this.game = game; Model = model; Panel = panel;
            foreach (var button in panel.GetComponentsInChildren<Button>(true)) if (!buttons.ContainsKey(button.name)) buttons.Add(button.name, button);
        }
        protected void On(string name, Action action)
        {
            if (!buttons.TryGetValue(name, out var button)) return;
            UnityAction handler = () => { game.Audio.Play(SoundCue.Click); action(); };
            button.onClick.AddListener(handler); unbind.Add(() => { if (button != null) button.onClick.RemoveListener(handler); });
        }
        protected void On(Slider slider, UnityAction<float> action)
        {
            if (slider == null) return;
            slider.onValueChanged.AddListener(action); unbind.Add(() => { if (slider != null) slider.onValueChanged.RemoveListener(action); });
        }
        protected static void LayoutChoice(UIWidget widget)
        {
            var accent=(RectTransform)widget.accent.transform;accent.anchorMin=accent.anchorMax=new Vector2(.5f,1);accent.anchoredPosition=new Vector2(0,-65);accent.sizeDelta=new Vector2(86,86);
            widget.symbol.rectTransform.sizeDelta=new Vector2(60,60);
            widget.title.rectTransform.anchoredPosition=new Vector2(0,-122);widget.title.rectTransform.sizeDelta=new Vector2(-36,48);widget.title.alignment=TextAnchor.MiddleCenter;
            widget.body.rectTransform.anchoredPosition=new Vector2(0,-180);widget.body.rectTransform.sizeDelta=new Vector2(-42,80);widget.body.alignment=TextAnchor.UpperLeft;
            widget.meta.rectTransform.sizeDelta=new Vector2(-40,48);widget.meta.alignment=TextAnchor.MiddleCenter;
        }

        protected string TaskHint(string taskId)
        {
            var task = Array.Find(game.Catalog.tasks, t => t.id == taskId);
            return task == null ? "未解锁" : "解锁任务 · " + task.name;
        }

        protected UIWidget Widget(Transform parent, UIWidget template)
        {
            var widget = UnityEngine.Object.Instantiate(template, parent, false);
            widget.gameObject.SetActive(true); widget.button.onClick.RemoveAllListeners(); return widget;
        }

        protected static void Clear(Transform parent)
        {
            foreach (Transform child in parent) { child.gameObject.SetActive(false); UnityEngine.Object.Destroy(child.gameObject); }
        }

        protected static void Fill(UIWidget widget, string title, string body, string meta, string icon, Color color)
        {
            widget.title.text = title; widget.body.text = body; widget.meta.text = meta; widget.icon.text = icon;
            widget.accent.color = color;
            if (widget.symbol != null) { widget.symbol.gameObject.SetActive(true); widget.symbol.Set(icon); widget.icon.gameObject.SetActive(false); }
            if (widget.portrait != null) widget.portrait.gameObject.SetActive(false);
            if(widget.preview!=null)widget.preview.gameObject.SetActive(false);
        }

        protected static void Highlight(UIWidget widget,bool selected,Color color)
        {
            widget.selectionOutline.enabled=selected;widget.selectionOutline.effectColor=color;
            widget.background.color=selected?Color.Lerp(Color.white,color,.10f):Color.white;
        }
        protected static string Icon(AbilityKind kind) => UIFormatting.Icon(kind);
        protected static string Icon(UpgradeDefinition upgrade) => UIFormatting.Icon(upgrade);
        public abstract void Bind();
        public abstract void Render();
        public virtual void Tick() { }
        public virtual bool Expired => false;
        public virtual void Dispose()
        {
            foreach (var remove in unbind) remove(); unbind.Clear();
            foreach (var widget in panel.GetComponentsInChildren<UIWidget>(true))
                if (widget.button != null) widget.button.onClick.RemoveAllListeners();
        }
    }
    public abstract class UIController<TView, TModel> : UIController where TView : UIView where TModel : UIModel
    {
        protected TView View { get; }
        protected new TModel Model => (TModel)base.Model;
        protected UIController(UIManager manager, GameManager game, TModel model, TView view) : base(manager, game, model, view) { View = view; }
    }
}
