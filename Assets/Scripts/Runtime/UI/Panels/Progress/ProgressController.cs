using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class ProgressController : UIController<ProgressView, ProgressModel>
    {
        public ProgressController(UIManager manager, GameManager game, ProgressModel model, ProgressView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("ProgressClose", manager.CloseModal);
        }
        public override void Render()
        {
            Model.Refresh(game); PopulateProgress(); View.Present(Model);
        }
        void PopulateProgress()
        {
            Clear(View.metaList); Clear(View.taskList);
            var p = game.Save.Profile;
            string[] names = { "锋芒", "活力", "轻盈", "引力" };
            string[] descriptions = { "永久伤害 +6% / 级", "永久最大生命 +10 / 级", "永久移动速度 +2.5% / 级", "永久拾取范围 +0.25 / 级" };
            int[] levels = { p.damageLevel, p.healthLevel, p.speedLevel, p.pickupLevel };
            for (int i = 0; i < 4; i++)
            {
                int index = i; var widget = Widget(View.metaList, View.rowTemplate); int cost = game.Save.MetaCost(levels[i]);
                Fill(widget, names[i] + "  Lv." + levels[i], descriptions[i], cost + " 晶石 · 点击提升", new[] { "攻", "生", "速", "引" }[i], new Color(.2f, .62f, .52f));
                widget.button.interactable = p.crystals >= cost;
                widget.button.onClick.AddListener(() => { if (game.Save.BuyMeta(index)) { game.Audio.Play(SoundCue.Upgrade); manager.ShowProgress(); } });
            }
            foreach (var task in game.Catalog.tasks)
            {
                bool done = p.completedTasks.Contains(task.id); var widget = Widget(View.taskList, View.rowTemplate);
                Fill(widget, (done ? "已完成 · " : "") + task.name, task.description, (done ? task.target : Mathf.Min(task.target, p.Metric(task.metric))) + " / " + task.target + "  ·  晶石 +" + task.crystals, done ? "成" : "途", done ? new Color(.2f, .65f, .46f) : new Color(.84f, .58f, .24f));
                widget.button.interactable = false;
            }
        }
    }
}
