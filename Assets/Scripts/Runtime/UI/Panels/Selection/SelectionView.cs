using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class SelectionView : UIView
    {
        public Transform characterList;
        public Transform biomeList;
        public UIWidget cardTemplate;
        public UIWidget rowTemplate;
        public void Present(SelectionModel model)
        {
            SetText("SelectionDescription", model.description); SetText("SelectionCrystalText", "晶石  " + model.crystals);
        }
    }
}
