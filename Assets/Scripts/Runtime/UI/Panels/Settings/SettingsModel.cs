using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class SettingsModel : UIModel
    {
        public string returnModal;
        public float music, sound;
        public int quality;
        public bool fullscreen;
        public override void Refresh(GameManager game)
        {
            music = game.Save.Profile.musicVolume; sound = game.Save.Profile.soundVolume; quality = game.Save.Profile.quality; fullscreen = Screen.fullScreen;
        }
    }
}
