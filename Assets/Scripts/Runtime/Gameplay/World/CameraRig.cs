using UnityEngine;

namespace VampireLike
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraRig : MonoBehaviour
    {
        public static CameraRig Active { get; private set; }
        public bool menu;
        Transform target;
        Camera cam;
        float zoom = 13.5f, shake;
        Vector3 smooth;
        void Awake() { cam = GetComponent<Camera>(); Active = this; }
        public void SetTarget(Transform value) { target = value; smooth = value.position; }
        public void Shake(float amount) { shake = Mathf.Max(shake, amount); }
        public void DiagnosticZoom(float value){if(GameManager.Instance!=null&&GameManager.Instance.DiagnosticMode){zoom=Mathf.Clamp(value,9.5f,23);cam.orthographicSize=zoom;}}
        void LateUpdate()
        {
            if (menu) return;
            var gm = GameManager.Instance;
            if (gm == null || target == null) return;
            zoom = Mathf.Clamp(zoom - gm.Inputs.Zoom * .8f, 9.5f, 23);
            smooth = Vector3.Lerp(smooth, target.position, 1 - Mathf.Exp(-8 * Time.unscaledDeltaTime));
            Vector3 offset = new Vector3(0, 22, -16);
            transform.position = smooth + offset;
            if (shake > 0) { transform.position += Random.insideUnitSphere * shake; shake = Mathf.MoveTowards(shake, 0, Time.unscaledDeltaTime); }
            transform.rotation = Quaternion.LookRotation(-offset);
            cam.orthographic = true; cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoom, 1 - Mathf.Exp(-8 * Time.unscaledDeltaTime));
        }
        public void ShiftOrigin(Vector3 shift) { smooth -= shift; transform.position -= shift; }
        void OnDestroy() { if (Active == this) Active = null; }
    }
}
