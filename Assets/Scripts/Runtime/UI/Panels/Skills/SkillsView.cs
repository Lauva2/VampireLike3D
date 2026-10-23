using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class SkillsView : UIView
    {
        public Transform skillList;
        public UIWidget cardTemplate;
        public UIWidget rowTemplate;
        public void Present(SkillsModel model)
        {
            SetText("SkillPointsText", "定向点  " + model.skillPoints);
        }
    }
}
