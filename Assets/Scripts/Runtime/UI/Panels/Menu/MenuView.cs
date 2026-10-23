using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class MenuView : UIView
    {

        public void Present(MenuModel model)
        {
            SetText("MenuCrystalText", "旅途晶石  " + model.crystals);
        }
    }
}
