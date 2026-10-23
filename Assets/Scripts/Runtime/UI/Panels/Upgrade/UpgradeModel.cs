using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class UpgradeModel : UIModel
    {
        public bool isBlessing;
        public int level, rerolls, blessingToken;
        public List<UpgradeDefinition> choices = new List<UpgradeDefinition>();
        public List<BlessingDefinition> blessings = new List<BlessingDefinition>();
        public ShrineBlessings blessingOwner;
        public override void Refresh(GameManager game)
        {
            level = game.Run.level; rerolls = game.Run.rerolls;
        }
        public override void OnClose() { choices.Clear(); blessings.Clear(); blessingOwner = null; blessingToken = 0; isBlessing = false; }
    }
}
