using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class DamageFlashController : UIController<DamageFlashView, DamageFlashModel>
    {
        public DamageFlashController(UIManager manager, GameManager game, DamageFlashModel model, DamageFlashView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            // Feedback never captures user input.
        }
        public override void Render()
        {
            Tick();
        }
        public override void Tick() { Model.Refresh(game); View.Present(Model); }
        public override bool Expired => Time.unscaledTime > Model.expires;
    }
}
