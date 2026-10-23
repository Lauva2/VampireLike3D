using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class SelectionController : UIController<SelectionView, SelectionModel>
    {
        public SelectionController(UIManager manager, GameManager game, SelectionModel model, SelectionView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("StartButton", () => game.BeginRun(Model.selectedCharacter, Model.selectedBiome));
            On("SelectionClose", manager.CloseModal);
        }
        public override void Render()
        {
            Model.Refresh(game); PopulateSelection(); View.Present(Model);
        }
        void PopulateSelection()
        {
            Clear(View.characterList); Clear(View.biomeList);
            foreach (var character in game.Catalog.characters)
            {
                var c = character; bool unlocked = game.Save.TaskUnlocked(c.taskId);
                var widget = Widget(View.characterList, View.rowTemplate);
                Fill(widget, c.name, c.subtitle + " · " + c.description, unlocked ? c.id == Model.selectedCharacter ? "已选择" : "点击选择" : TaskHint(c.taskId), "旅", c.color);
                if (manager.Portraits.TryGetValue(c.id, out var portrait)) { widget.portrait.texture = portrait; widget.portrait.gameObject.SetActive(true); widget.symbol.gameObject.SetActive(false); }
                widget.button.interactable = unlocked;
                widget.lockIcon.gameObject.SetActive(!unlocked);Highlight(widget,unlocked&&c.id==Model.selectedCharacter,c.color);
                widget.button.onClick.AddListener(() => { Model.selectedCharacter = c.id; MenuStage.Active?.SelectCharacter(c.id); manager.ShowSelection(); });
            }
            for (int i = 0; i < ContentIds.Maps.Length; i++)
            {
                BiomeId biome = ContentIds.Maps[i]; var config = game.Catalog.Biome(biome); bool unlocked = game.Save.MapUnlocked(biome);
                var widget = Widget(View.biomeList, View.cardTemplate);
                string title = i == 4 ? "万象交界" : config.name;
                string description = i == 4 ? "所有群系交汇，无限探索与自由组合。" : config.subtitle;
                string requirement = i == 0 ? "初始开放" : i >= 4 ? "首次击败原四群系 Boss 后开放" : "首次击败" + game.Catalog.Biome((BiomeId)(i - 1)).bossName;
                Fill(widget, title, description, unlocked ? Model.selectedBiome == biome ? "已选择 · 无限地图" : "点击选择" : requirement, new[] { "林", "沙", "雪", "焰", "象", "菌", "晶" }[i], i == 4 ? new Color(.55f, .4f, .85f) : config.accent);
                if(manager.Landscapes.TryGetValue(i,out var landscape))
                {
                    widget.preview.gameObject.SetActive(true);widget.preview.texture=landscape;
                    widget.body.rectTransform.anchoredPosition=new Vector2(0,-136);widget.body.rectTransform.sizeDelta=new Vector2(-36,47);widget.body.fontSize=16;
                    widget.meta.fontSize=15;widget.meta.rectTransform.sizeDelta=new Vector2(-28,26);widget.meta.rectTransform.anchoredPosition=new Vector2(0,8);
                }
                widget.button.interactable = unlocked;
                widget.lockIcon.gameObject.SetActive(!unlocked);Highlight(widget,unlocked&&Model.selectedBiome==biome,config.accent);
                widget.button.onClick.AddListener(() => { Model.selectedBiome = biome; manager.ShowSelection(); });
            }
            var selected = game.Catalog.Character(Model.selectedCharacter);
            if (buttons.TryGetValue("StartButton", out var start)) start.interactable = game.Save.TaskUnlocked(selected.taskId) && game.Save.MapUnlocked(Model.selectedBiome);
        }
    }
}
