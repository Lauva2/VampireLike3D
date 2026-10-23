using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class DamageFlashModel : UIModel
    {
        public float expires, alpha;
        public override void Refresh(GameManager game)
        {
            alpha = Mathf.Clamp01((expires - Time.unscaledTime) / .2f) * .18f;
        }
    }
}
