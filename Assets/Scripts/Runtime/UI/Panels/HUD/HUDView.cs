using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class HUDView : UIView
    {
        public Image healthFill;
        public Image experienceFill;
        public Image dodgeFill;
        public Image bossFill;
        public GameObject bossBar;
        public MapGraphic miniMap;
        public Transform abilityList;
        public UIWidget abilityTemplate;
        public void Present(HUDModel model)
        {
            SetBar(healthFill, model.health); SetBar(experienceFill, model.experience); SetBar(dodgeFill, model.dodge);
            SetText("HealthText", model.healthText); SetText("LevelText", model.level); SetText("TimerText", model.timer); SetText("BiomeText", model.biome);
            SetText("RunCurrencyText", model.currency); SetText("KillsText", model.kills); SetText("ControlsText", model.controls); SetText("InteractionText", model.interaction);
            SetTextVisible("BiomeText", !model.bossVisible); bossBar.SetActive(model.bossVisible);
            if (model.bossVisible) { SetBar(bossFill, model.bossHealth); SetText("BossNameText", model.bossName); }
            SetText("RetreatWarningText", model.retreat); SetText("MarkText", model.mark);
        }
    }
}
