using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class ResultsView : UIView
    {

        public void Present(ResultsModel model)
        {
            SetText("ResultsHeading", model.heading); SetText("ResultsBody", model.body);
        }
    }
}
