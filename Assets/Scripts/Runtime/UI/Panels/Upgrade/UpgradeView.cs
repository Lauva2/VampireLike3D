using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class UpgradeView : UIView
    {
        public Transform upgradeList;
        public UIWidget cardTemplate;
        public UIWidget rowTemplate;
        public void Present(UpgradeModel model)
        {
            SetText("UpgradeTitle", model.isBlessing ? "祭坛回响" : "灵感涌现");
            SetText("UpgradeSubtitle", model.isBlessing ? "选择后恢复45%生命，获得3定向点、4晶石与18经验。祝福持续到本局结束。" : "从四种可能中选择一种。其他路线仍可在之后获得。");
            SetText("UpgradeHeading", model.isBlessing ? "三选一 · 后续祭坛仍可叠加" : "等级 " + model.level + " · 选择新的力量");
            SetText("SkipUpgradeLabel", model.isBlessing ? "暂不领取 · 保留祭坛" : "改领 2 个定向点");
            SetText("RerollLabel", "重新抽取  " + model.rerolls);
            var reroll = transform.Find("RerollButton").GetComponent<Button>(); reroll.gameObject.SetActive(!model.isBlessing); reroll.interactable = model.rerolls > 0;
            var grid = upgradeList.GetComponent<GridLayoutGroup>(); grid.constraintCount = model.isBlessing ? 3 : 4;
            grid.cellSize = model.isBlessing ? new Vector2(360, 335) : new Vector2(290, 335); grid.childAlignment = model.isBlessing ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
        }
    }
}
