using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class ProgressView : UIView
    {
        public Transform taskList;
        public Transform metaList;
        public UIWidget cardTemplate;
        public UIWidget rowTemplate;
        public void Present(ProgressModel model)
        {
            SetText("ProgressCrystalText", "旅途晶石  " + model.crystals);
        }
    }
}
