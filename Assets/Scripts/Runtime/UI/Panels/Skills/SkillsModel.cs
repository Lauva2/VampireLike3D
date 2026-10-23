using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class SkillsModel : UIModel
    {
        public string filter = "全部";
        public int skillPoints;
        public override void Refresh(GameManager game)
        {
            skillPoints = game.Run.skillPoints;
        }
    }
}
