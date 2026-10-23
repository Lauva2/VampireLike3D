using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class UpgradeController : UIController<UpgradeView, UpgradeModel>
    {
        public UpgradeController(UIManager manager, GameManager game, UpgradeModel model, UpgradeView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("RerollButton", () => { if (!Model.isBlessing) game.Progression.Reroll(); });
            On("SkipUpgrade", () => { if (Model.isBlessing) manager.CloseModal(); else game.Progression.SkipLevel(); });
        }
        public override void Render()
        {
            Model.Refresh(game); View.Present(Model);
            if (Model.isBlessing) RenderBlessings(Model.blessings, Model.blessingToken); else RenderUpgrade(Model.choices);
        }
        void RenderUpgrade(List<UpgradeDefinition> choices)
        {
            Clear(View.upgradeList);
            foreach (var u in choices)
            {
                var upgrade = u; var widget = Widget(View.upgradeList, View.cardTemplate);
                Fill(widget, u.name, u.description, u.unique ? "新机制 · 可与其他路线叠加" : "当前等级 " + game.Run.Level(u.id) + " → " + (game.Run.Level(u.id) + 1), Icon(u), u.color);
                LayoutChoice(widget);
                widget.button.onClick.AddListener(() => game.Progression.Apply(upgrade.id, false));
            }
        }
        void RenderBlessings(List<BlessingDefinition> choices, int token)
        {
            var owner = Model.blessingOwner;
            Clear(View.upgradeList);
            foreach (var blessing in choices)
            {
                var choice = blessing; var widget = Widget(View.upgradeList, View.cardTemplate);
                Fill(widget, choice.name, choice.description, "本局层数 " + game.Run.BlessingLevel(choice.id) + " → " + (game.Run.BlessingLevel(choice.id) + 1), choice.icon, choice.color);
                LayoutChoice(widget);
                widget.button.onClick.AddListener(() => owner.Select(token, choice.id));
            }
        }
    }
}
