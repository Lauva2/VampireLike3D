using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class LoadingModel : UIModel
    {
        public string message;
        public float progress;
        public override void Refresh(GameManager game)
        {
            progress = Mathf.Clamp01(progress);
        }
    }
}
