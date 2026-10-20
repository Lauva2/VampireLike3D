using UnityEngine;
using UnityEngine.EventSystems;

namespace VampireLike
{
    public sealed class InputService
    {
        public bool Blocked;
        public bool HardwareEnabled=true;
        bool simulated;
        Vector2 simulatedMove;
        int dodgeFrame=-1;
        int interactFrame=-1;
        public void ForVerification(Vector2 move,bool dodge=false)
        {
            if(GameManager.Instance==null||!GameManager.Instance.DiagnosticMode)return;
            simulated=true;simulatedMove=Vector2.ClampMagnitude(move,1);if(dodge)dodgeFrame=Time.frameCount+1;
        }
        public void InteractForVerification(){if(GameManager.Instance!=null&&GameManager.Instance.DiagnosticMode)interactFrame=Time.frameCount+1;}
        public Vector2 Move => Blocked ? Vector2.zero : simulated?simulatedMove:HardwareEnabled?Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1):Vector2.zero;
        public bool Dodge => !Blocked && (dodgeFrame==Time.frameCount || HardwareEnabled&&(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)));
        public bool Interact => !Blocked && (interactFrame==Time.frameCount||HardwareEnabled&&Input.GetKeyDown(KeyCode.E));
        public bool Mark => !Blocked && HardwareEnabled&&Input.GetMouseButtonDown(1) && !OverUI;
        public float Zoom => Blocked || !HardwareEnabled||OverUI ? 0 : Input.mouseScrollDelta.y;
        public bool OverUI => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
