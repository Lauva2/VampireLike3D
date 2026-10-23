using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class MenuModel : UIModel
    {
        public int crystals;
        public override void Refresh(GameManager game)
        {
            crystals = game.Save.Profile.crystals;
        }
    }
}
