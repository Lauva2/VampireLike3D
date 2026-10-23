using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class VictoryView : UIView
    {

        public void Present(VictoryModel model)
        {
            SetText("VictoryName", model.bossName); SetText("VictoryBody", model.message);
        }
    }
}
