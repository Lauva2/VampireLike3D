using UnityEngine;

namespace VampireLike
{
    public sealed class DecorationVariant : MonoBehaviour
    {
        public Transform[] variants;
        public void Select(int seed)
        {
            if (variants == null || variants.Length == 0) return;
            int selected = (seed & int.MaxValue) % variants.Length;
            for (int i = 0; i < variants.Length; i++) variants[i].gameObject.SetActive(i == selected);
        }
    }
}
