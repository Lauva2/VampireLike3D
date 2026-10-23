using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class ResultsModel : UIModel
    {
        public bool extraction;
        public string heading, body;
        public override void Refresh(GameManager game)
        {
            var run = game.Run;
            heading = extraction ? "满载而归" : "旅途未完";
            body = "生存时间   " + UIFormatting.Clock(run.seconds) + "\n击败敌人   " + run.kills + "\n成长等级   " + run.level + "\n本局晶石   " + run.earnedCrystals + "\n已掌握能力   " + run.abilities.Count + "\n\n永久资源、任务进度与解锁均已保存。";

        }
    }
}
