using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class MapView : UIView
    {
        public MapGraphic largeMap;
        public void Present(MapModel model)
        {
            largeMap.offset = model.offset; SetText("MapCoordinates", model.coordinates);
        }
    }
}
