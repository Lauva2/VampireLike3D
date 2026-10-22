using UnityEngine;

namespace VampireLike
{
    public sealed class BossSiteAppearance : MonoBehaviour
    {
        public GameObject[] themes;
        public GameObject[] invocationSeals;
        public void Configure(BiomeId biome)
        {
            for(int i=0;i<themes.Length;i++)themes[i].SetActive(i==(int)biome);
            SetCompleted(false);
        }
        public void SetCompleted(bool completed)
        {
            if(invocationSeals==null)return;
            foreach(var seal in invocationSeals)if(seal!=null)seal.SetActive(!completed);
        }
    }
}
