using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class VictoryController : UIController<VictoryView, VictoryModel>
    {
        public VictoryController(UIManager manager, GameManager game, VictoryModel model, VictoryView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("VictoryContinue", manager.CloseModal); On("VictoryRetreat", () => { manager.CloseModal(); game.FinishRun(true); });
        }
        public override void Render()
        {
            Model.Refresh(game); View.Present(Model);
        }
    }
}
