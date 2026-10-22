using UnityEngine;

namespace VampireLike
{
    public sealed class TerrainAppearance : MonoBehaviour
    {
        public GameObject bridge;
        public Transform landform;
        public Vector2 waterCenter,waterRadii;
        public bool HasBridge=>bridge!=null&&bridge.activeSelf;
        public void Orient(int quarterTurns){if(landform!=null)landform.localRotation=Quaternion.Euler(0,quarterTurns*90,0);}
        public void Configure(bool crossing,float offset=0){if(bridge!=null){bridge.SetActive(crossing);bridge.transform.localPosition=Vector3.forward*offset;}}
        public bool ContainsWater(Vector3 world,float margin)
        {
            if(landform==null||waterRadii.x<=0||waterRadii.y<=0)return false;
            Vector3 p=landform.InverseTransformPoint(world);
            Vector2 d=new Vector2((p.x-waterCenter.x)/(waterRadii.x+margin),(p.z-waterCenter.y)/(waterRadii.y+margin));return d.sqrMagnitude<1;
        }
    }
}
