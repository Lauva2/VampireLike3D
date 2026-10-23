using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class DamageFlashView : UIView
    {
        public Image damageFlash;
        public void Present(DamageFlashModel model)
        {
            damageFlash.color = new Color(1, .24f, .18f, model.alpha);
        }
    }
}
