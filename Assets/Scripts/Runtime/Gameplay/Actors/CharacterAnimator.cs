using UnityEngine;

namespace VampireLike
{
    public sealed class CharacterAnimator : MonoBehaviour
    {
        public Transform visual, head, leftArm, rightArm, leftLeg, rightLeg, scarf;
        public Transform[] eyelids;
        public Transform mouth;
        float flashUntil;
        float attackUntil;
        Renderer[] renderers;
        MaterialPropertyBlock block;
        void Awake(){block=new MaterialPropertyBlock();renderers=visual!=null?visual.GetComponentsInChildren<Renderer>():new Renderer[0];}
        public void Flash() { flashUntil = Time.time + .12f; }
        public void Attack(){attackUntil=Time.time+.22f;}
        public void Animate(float movement, bool dodge, bool invulnerable)
        {
            float t = Time.time * 11;
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(Mathf.Sin(t) * 26 * movement, 0, -8);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(Time.time<attackUntil?-52:-Mathf.Sin(t) * 26 * movement, 0, 8);
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-Mathf.Sin(t) * 32 * movement, 0, 0);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(Mathf.Sin(t) * 32 * movement, 0, 0);
            if (head != null) head.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * 1.8f) * 3, Mathf.Sin(t * .5f) * 2 * movement);
            float blink=(Time.time+GetInstanceID()*.137f)%4.3f;float closed=blink<.16f?Mathf.Sin(blink/.16f*Mathf.PI):0;
            if(Time.time<flashUntil)closed=Mathf.Max(closed,.85f);else if(dodge)closed=Mathf.Max(closed,.55f);
            if(eyelids!=null)foreach(var lid in eyelids){lid.gameObject.SetActive(closed>.02f);lid.localScale=new Vector3(1,Mathf.Max(.01f,closed),1);}
            if(mouth!=null)mouth.localScale=new Vector3(1,Time.time<flashUntil?2.1f:dodge?1.5f:1,1);
            if (scarf != null) scarf.localRotation = Quaternion.Euler(10 + movement * 22 + Mathf.Sin(t * .7f) * 7, Mathf.Sin(t * .4f) * 8, 0);
            if (visual != null)
            {
                visual.localPosition = Vector3.up * (Mathf.Abs(Mathf.Sin(t)) * .055f * movement + Mathf.Sin(Time.time * 2) * .012f);
                visual.localRotation = Quaternion.Euler(dodge ? 28 : movement * 5, 0, 0);
                visual.localScale = Vector3.one * (Time.time < flashUntil ? 1.05f : 1);
            }
            float flash=Time.time<flashUntil?.85f:invulnerable?.35f+Mathf.Abs(Mathf.Sin(Time.time*42))*.45f:0;
            foreach(var renderer in renderers){if(renderer==null)continue;renderer.GetPropertyBlock(block);block.SetFloat("_Flash",flash);renderer.SetPropertyBlock(block);}
        }
        void Update()
        {
            if (GetComponent<PlayerController>() == null) Animate(.08f, false, false);
        }
    }
}
