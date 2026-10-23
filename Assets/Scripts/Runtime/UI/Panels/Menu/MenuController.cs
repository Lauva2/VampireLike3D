using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class MenuController : UIController<MenuView, MenuModel>
    {
        public MenuController(UIManager manager, GameManager game, MenuModel model, MenuView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("PlayButton", manager.ShowSelection); On("ProgressButton", manager.ShowProgress);
            On("SettingsButton", manager.ShowSettings); On("QuitButton", () => { game.Save.Flush(); Application.Quit(); });
        }
        public override void Render()
        {
            Model.Refresh(game); View.Present(Model);
        }
    }
}
