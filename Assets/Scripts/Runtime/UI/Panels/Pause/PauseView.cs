using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class PauseView : UIView
    {

        public void Present(PauseModel model)
        {
            SetText("PauseTitle", model.title); SetText("PauseSubtitle", model.subtitle);
        }
    }
}
