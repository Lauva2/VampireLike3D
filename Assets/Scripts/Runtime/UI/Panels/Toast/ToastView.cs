using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class ToastView : UIView
    {

        public void Present(ToastModel model)
        {
            SetText("ToastText", model.message);
        }
    }
}
