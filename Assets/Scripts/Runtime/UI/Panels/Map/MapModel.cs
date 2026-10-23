using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class MapModel : UIModel
    {
        public Vector2Int offset;
        public string coordinates;
        public override void Refresh(GameManager game)
        {
            coordinates = game.Ready ? "世界坐标  " + game.World.GlobalPosition.x.ToString("0") + " / " + game.World.GlobalPosition.y.ToString("0") + "    种子 " + game.Run.seed : "";
        }
    }
}
