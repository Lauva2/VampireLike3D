using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class ToastModel : UIModel
    {
        public string message;
        public float expires;
        public override void Refresh(GameManager game)
        {
            if (message == null) message = string.Empty;
        }
    }
}
