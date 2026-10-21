using UnityEngine;

namespace VampireLike
{
    public sealed class BossSiteAppearance : MonoBehaviour
    {
        public GameObject[] themes;
        public void Configure(BiomeId biome)
        {
            for(int i=0;i<themes.Length;i++)themes[i].SetActive(i==(int)biome);
        }
    }
}
