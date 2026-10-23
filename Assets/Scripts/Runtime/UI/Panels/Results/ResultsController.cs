using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class ResultsController : UIController<ResultsView, ResultsModel>
    {
        public ResultsController(UIManager manager, GameManager game, ResultsModel model, ResultsView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("ResultsContinue", game.ReturnToMenu);
        }
        public override void Render()
        {
            Model.Refresh(game); View.Present(Model);
        }
    }
}
