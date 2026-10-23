using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class LoadingController : UIController<LoadingView, LoadingModel>
    {
        public LoadingController(UIManager manager, GameManager game, LoadingModel model, LoadingView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            // Loading has no user commands.
        }
        public override void Render()
        {
            Model.Refresh(game); View.Present(Model);
        }
    }
}
