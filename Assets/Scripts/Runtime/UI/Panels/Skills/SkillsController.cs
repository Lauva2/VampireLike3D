using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class SkillsController : UIController<SkillsView, SkillsModel>
    {
        public SkillsController(UIManager manager, GameManager game, SkillsModel model, SkillsView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("SkillsClose", manager.CloseModal);
            foreach (var branch in new[] { "全部", "元素", "召唤", "弹道", "爆破", "剑修", "符阵", "通用", "联动" })
            { string value = branch; On("Filter" + branch, () => { Model.filter = value; Render(); }); }
        }
        public override void Render()
        {
            Model.Refresh(game); PopulateSkills(); View.Present(Model);
        }
        void PopulateSkills()
        {
            Clear(View.skillList);
            if (Model.filter == "全部" || Model.filter == "通用")
                foreach (var blessing in game.Catalog.blessings)
                {
                    int count = game.Run.BlessingLevel(blessing.id); if (count == 0) continue;
                    var widget = Widget(View.skillList, View.cardTemplate);
                    Fill(widget, blessing.name, blessing.description, "祭坛祝福 ×" + count + " · 本局生效", blessing.icon, blessing.color);
                    widget.button.interactable = false;
                }
            foreach (var u in game.Catalog.upgrades)
            {
                if (Model.filter != "全部" && u.branch != Model.filter) continue;
                var upgrade = u; bool eligible = game.Progression.Eligible(u); var widget = Widget(View.skillList, View.cardTemplate);
                string status = !game.Save.TaskUnlocked(u.taskId) ? TaskHint(u.taskId) : u.unique && game.Run.Level(u.id) > 0 ? "已掌握" : u.effect == UpgradeEffect.NewAbility && game.Run.abilities.Contains(u.target) ? "已获得" : !eligible ? "需要先获得对应能力" : "定向点 " + game.Progression.Cost(u) + "  ·  等级 " + game.Run.Level(u.id);
                Fill(widget, u.name, u.description, status, Icon(u), u.color);
                widget.lockIcon.gameObject.SetActive(!game.Save.TaskUnlocked(u.taskId));
                if(eligible&&game.Run.skillPoints<game.Progression.Cost(u))widget.meta.text+=" · 不足";
                widget.button.interactable = eligible && game.Run.skillPoints >= game.Progression.Cost(u);
                widget.button.onClick.AddListener(() => { if (game.Progression.Apply(upgrade.id, true)) Render(); });
            }
        }
    }
}
