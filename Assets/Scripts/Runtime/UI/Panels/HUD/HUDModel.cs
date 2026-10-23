using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class HUDModel : UIModel
    {
        public float health, experience, dodge, bossHealth, nextRefresh;
        public bool bossVisible;
        public string healthText, level, timer, biome, currency, kills, controls, interaction, bossName, retreat, mark;
        public override void Refresh(GameManager game)
        {
            var run = game.Run; var player = game.Player; var boss = game.Combat.Boss;
            health = player.Health / player.MaxHealth; experience = run.experience / (float)run.nextExperience; dodge = 1 - player.DodgeCooldown / 2.2f;
            healthText = Mathf.CeilToInt(player.Health) + " / " + Mathf.CeilToInt(player.MaxHealth); level = "Lv. " + run.level; timer = UIFormatting.Clock(run.seconds);
            biome = game.Catalog.Biome(game.World.CurrentBiome).name + "  ·  无限探索";
            currency = "定向点 " + run.skillPoints + "     晶石 " + game.Save.Profile.crystals; kills = "击败 " + run.kills + " · 能力 " + run.abilities.Count;
            controls = "WASD 移动   Shift 无敌闪避\n右键标记   E 交互" + (run.abilities.Count > 6 ? "\n武器栏可用滚轮或拖动浏览" : ""); interaction = game.World.InteractionText ?? "";
            bossVisible = boss != null; bossHealth = boss != null ? boss.Health / boss.MaxHealth : 0;
            bossName = boss != null ? game.Catalog.Biome(boss.Biome).bossName + (boss.IsEntering ? "  ·  正在现身" : boss.Health < boss.MaxHealth * .5f ? "  ·  狂怒阶段" : "  ·  守卫试炼") : "";
            retreat = game.Combat.RetreatProgress > 0 ? "即将脱战 · " + Mathf.Max(0, 2 - game.Combat.RetreatProgress).ToString("0.0") + " 秒后 Boss 重置" : "";
            var target = game.Combat.Marked;
            mark = target == null ? "右键敌人 · 标记集火" : "集火目标  " + (target.Type == 4 ? game.Catalog.Biome(target.Biome).bossName : target.Elite ? "精英" : game.Catalog.Enemy(target.Type).name) + "  " + Mathf.CeilToInt(target.Health);
        }
    }
}
