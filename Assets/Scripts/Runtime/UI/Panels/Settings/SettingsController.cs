using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class SettingsController : UIController<SettingsView, SettingsModel>
    {
        public SettingsController(UIManager manager, GameManager game, SettingsModel model, SettingsView view) : base(manager, game, model, view) { }
        public override void Bind()
        {
            On("SettingsClose", manager.CloseModal);
            On("QualityButton", () => { game.Save.Profile.quality = (game.Save.Profile.quality + 1) % 3; manager.RestoreSettings(); Render(); });
            On("FullscreenButton", () => { Screen.fullScreen = !Screen.fullScreen; manager.RestoreSettings(); Render(); });
            On(View.musicSlider, v => { game.Save.Profile.musicVolume = v; game.Audio.RefreshVolumes(); game.Save.MarkDirty(); Model.Refresh(game); });
            On(View.soundSlider, v => { game.Save.Profile.soundVolume = v; game.Audio.RefreshVolumes(); game.Save.MarkDirty(); Model.Refresh(game); });
        }
        public override void Render()
        {
            Model.Refresh(game); View.Present(Model);
        }
    }
}
