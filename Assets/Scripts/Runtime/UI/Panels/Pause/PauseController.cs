using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class PauseController : UIController<PauseView, PauseModel>
    {
        public PauseController(UIManager manager, GameManager game, PauseModel model, PauseView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("ResumeButton", manager.CloseModal); On("PauseSettings", manager.ShowSettings);
            On("RetreatButton", () => { manager.CloseModal(); game.FinishRun(true); });
        }
        public override void Render()
        {
            Model.Refresh(game); View.Present(Model);
        }
    }
}
