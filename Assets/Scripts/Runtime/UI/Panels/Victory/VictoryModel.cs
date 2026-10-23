using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class VictoryModel : UIModel
    {
        public BiomeId biome;
        public string bossName, message;
        public override void Refresh(GameManager game)
        {
            bossName = "击败「" + game.Catalog.Biome(biome).bossName + "」";
            message = biome == BiomeId.Volcano && game.Save.MapUnlocked(BiomeId.Mixed) ? "四方试炼完成 · 万象交界与两处新群系开放！\n首胜与任务成果已经保存。" : "首胜与任务成果已经保存。\n你可以继续探索，也可以携带收获撤离。";
        }
    }
}
