using UnityEngine;

namespace VampireLike
{
    public sealed class TerrainAppearance : MonoBehaviour
    {
        public GameObject bridge;
        public Transform landform;
        public bool HasBridge=>bridge!=null&&bridge.activeSelf;
        public void Orient(int quarterTurns){if(landform!=null)landform.localRotation=Quaternion.Euler(0,quarterTurns*90,0);}
        public void Configure(bool crossing,float offset=0){if(bridge!=null){bridge.SetActive(crossing);bridge.transform.localPosition=Vector3.forward*offset;}}
    }
}
