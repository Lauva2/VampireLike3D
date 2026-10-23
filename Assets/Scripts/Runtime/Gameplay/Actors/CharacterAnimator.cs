using UnityEngine;

namespace VampireLike
{
    public sealed class CharacterAnimator : MonoBehaviour
    {
        public Transform visual, head, leftArm, rightArm, leftLeg, rightLeg, scarf;
        public Transform[] eyelids;
        public Transform mouth,eyes;
        float flashUntil;
        float attackUntil;
        AbilityKind attackKind;
        Vector3 aim;
        Renderer[] renderers;
        MaterialPropertyBlock block;
        void Awake(){block=new MaterialPropertyBlock();renderers=visual!=null?visual.GetComponentsInChildren<Renderer>():new Renderer[0];}
        public void Flash() { flashUntil = Time.time + .12f; }
        public void Attack(AbilityKind kind,Vector3 direction){attackUntil=Time.time+.26f;attackKind=kind;aim=direction;}
        public void Animate(float movement, bool dodge, bool invulnerable)
        {
            float t = Time.time * 11;
            if (leftArm != null) leftArm.localRotation = Quaternion.Euler(Mathf.Sin(t) * 26 * movement, 0, -8);
            if (rightArm != null) rightArm.localRotation = Quaternion.Euler(Time.time<attackUntil?-52:-Mathf.Sin(t) * 26 * movement, 0, 8);
            if(Time.time<attackUntil)
            {
                Vector3 localAim=transform.InverseTransformDirection(aim);
                float swing=Mathf.Sin((1-(attackUntil-Time.time)/.26f)*Mathf.PI),yaw=Mathf.Clamp(Mathf.Atan2(localAim.x,localAim.z)*Mathf.Rad2Deg,-55,55);
                if(attackKind==AbilityKind.Fireball||attackKind==AbilityKind.Spirit||attackKind==AbilityKind.FrostStaff){if(leftArm!=null)leftArm.localRotation=Quaternion.Euler(-72-swing*18,-yaw*.35f,-18);if(rightArm!=null)rightArm.localRotation=Quaternion.Euler(-45,yaw*.3f,25);}
                else if(attackKind==AbilityKind.Sword||attackKind==AbilityKind.Boomerang){if(rightArm!=null)rightArm.localRotation=Quaternion.Euler(-105+45*swing,yaw,-25+45*swing);if(leftArm!=null)leftArm.localRotation=Quaternion.Euler(-24,0,-30);}
                else if(rightArm!=null)rightArm.localRotation=Quaternion.Euler(-68-swing*8,yaw,8);
            }
            if (leftLeg != null) leftLeg.localRotation = Quaternion.Euler(-Mathf.Sin(t) * 32 * movement, 0, 0);
            if (rightLeg != null) rightLeg.localRotation = Quaternion.Euler(Mathf.Sin(t) * 32 * movement, 0, 0);
            if (head != null) head.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * 1.8f) * 3, Mathf.Sin(t * .5f) * 2 * movement);
            // Instance IDs can be negative: % would keep a negative phase and repeatedly oscillate the eyelids.
            float blink = Mathf.Repeat(Time.time + GetInstanceID() * .137f, 5.2f);
            float closed = blink < .22f ? Mathf.Sin(blink / .22f * Mathf.PI) : 0;
            if(Time.time<flashUntil)closed=Mathf.Max(closed,.85f);else if(dodge)closed=Mathf.Max(closed,.55f);
            if(eyes!=null){eyes.gameObject.SetActive(closed<.55f);eyes.localScale=new Vector3(1,1-closed*.9f,1);}
            if(eyelids!=null)foreach(var lid in eyelids)lid.gameObject.SetActive(closed>=.55f);
            if(mouth!=null)mouth.localScale=new Vector3(1,Time.time<flashUntil?2.1f:dodge?1.5f:1,1);
            if (scarf != null) scarf.localRotation = Quaternion.Euler(10 + movement * 22 + Mathf.Sin(t * .7f) * 7, Mathf.Sin(t * .4f) * 8, 0);
            if (visual != null)
            {
                visual.localPosition = Vector3.up * (Mathf.Abs(Mathf.Sin(t)) * .055f * movement + Mathf.Sin(Time.time * 2) * .012f);
                visual.localRotation = Quaternion.Euler(dodge ? 28 : movement * 5, 0, 0);
                visual.localScale = Vector3.one * (Time.time < flashUntil ? 1.05f : 1);
            }
            float flash=Time.time<flashUntil?.24f:invulnerable?.12f+Mathf.Abs(Mathf.Sin(Time.time*30))*.16f:0;
            foreach(var renderer in renderers){if(renderer==null)continue;renderer.GetPropertyBlock(block);block.SetFloat("_Flash",flash);renderer.SetPropertyBlock(block);}
        }
        void Update()
        {
            if (GetComponent<PlayerController>() == null) Animate(.08f, false, false);
        }
    }
}
