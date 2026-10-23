using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class PauseModel : UIModel
    {
        public string title, subtitle;
        public override void Refresh(GameManager game)
        {
            title = "旅途稍歇"; subtitle = "暂停时世界停止。你可以随时携带已有收获撤离。";
        }
    }
}
