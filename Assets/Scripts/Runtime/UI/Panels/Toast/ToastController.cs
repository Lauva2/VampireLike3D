using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class ToastController : UIController<ToastView, ToastModel>
    {
        public ToastController(UIManager manager, GameManager game, ToastModel model, ToastView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            // Toasts never capture user input.
        }
        public override void Render()
        {
            Model.Refresh(game); View.Present(Model);
        }
        public override bool Expired => Time.unscaledTime > Model.expires;
    }
}
