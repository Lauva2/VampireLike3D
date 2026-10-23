using UnityEngine;

namespace VampireLike
{
    public sealed class UIRoot : MonoBehaviour
    {
        public RectTransform Bottom, Common, Pop, Top;
        public RectTransform Layer(UILayer layer)
        {
            switch (layer)
            {
                case UILayer.Bottom: return Bottom;
                case UILayer.Common: return Common;
                case UILayer.Pop: return Pop;
                case UILayer.Top: return Top;
                default: throw new System.ArgumentOutOfRangeException(nameof(layer));
            }
        }
    }
}
