using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class LoadingView : UIView
    {

        public void Present(LoadingModel model)
        {
            SetText("LoadingText", model.message); SetText("LoadingProgress", Mathf.RoundToInt(model.progress * 100) + "%");
        }
    }
}
