using UnityEngine;

namespace VampireLike
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float DodgeCooldown => Mathf.Max(0, nextDodge - Time.time);
        public bool Invulnerable => Time.time < invulnerableUntil;
        public float Speed { get; private set; }
        public float PickupRadius { get; private set; }
        GameManager game;
        CharacterController controller;
        Vector3 lastDirection = Vector3.back, dodgeDirection;
        float nextDodge, dodgeUntil, invulnerableUntil, nextHit, gravity;
        CharacterAnimator animator;
        Vector3 combatFacing;float combatFacingUntil;
        public void FaceAttack(Vector3 direction){direction.y=0;if(direction.sqrMagnitude>.01f){combatFacing=direction.normalized;combatFacingUntil=Time.time+.4f;}}
        public void Initialize(GameManager manager)
        {
            game = manager; controller = GetComponent<CharacterController>(); animator = GetComponent<CharacterAnimator>();
            RefreshStats(false); Health = MaxHealth;
            var locator=game.Pool.Rent("vfx/player",transform.position,Quaternion.identity);if(locator!=null){locator.transform.SetParent(transform,false);locator.transform.localPosition=Vector3.up*.06f;}
        }
        public void RefreshStats(bool healDifference)
        {
            float oldMax = MaxHealth;
            MaxHealth = 100 + game.Save.Profile.healthLevel * 10 + game.Run.character.healthBonus + game.Run.Effect(game.Catalog, UpgradeEffect.Health);
            Speed = 6.2f * (1 + game.Save.Profile.speedLevel * .025f + game.Run.character.speedBonus + game.Run.Effect(game.Catalog, UpgradeEffect.MoveSpeed));
            PickupRadius = 2.6f + game.Save.Profile.pickupLevel * .25f + game.Run.Effect(game.Catalog, UpgradeEffect.Pickup);
            if (healDifference) Health = Mathf.Min(MaxHealth, Health + Mathf.Max(0, MaxHealth - oldMax));
        }
        void Update()
        {
            if (game == null || !game.Ready || game.IsPaused || Health <= 0) return;
            Vector2 input = game.Inputs.Move;
            Vector3 direction = new Vector3(input.x, 0, input.y);
            if (direction.sqrMagnitude > .01f) lastDirection = direction.normalized;
            if (game.Inputs.Dodge && Time.time >= nextDodge)
            {
                nextDodge = Time.time + 2.2f; dodgeUntil = Time.time + .3f; invulnerableUntil = Time.time + .23f;
                dodgeDirection = lastDirection; game.Audio.Play(SoundCue.Dodge);
                game.Combat.Effect("vfx/dodge", transform.position + Vector3.up * .5f, .4f, .8f);
            }
            bool dodging = Time.time < dodgeUntil;
            if (controller.isGrounded) gravity = -2; else gravity = Mathf.Max(-30, gravity - 25 * Time.deltaTime);
            Vector3 velocity = (dodging ? dodgeDirection * 16 : direction * Speed) + Vector3.up * gravity;
            controller.Move(velocity * Time.deltaTime);
            if (direction.sqrMagnitude > .01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 14);
            else if(!dodging&&Time.time<combatFacingUntil)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(combatFacing),Time.deltaTime*18);
            animator?.Animate(direction.magnitude, dodging, Invulnerable);
            Shader.SetGlobalVector("_PlayerFocus",new Vector4(transform.position.x,transform.position.y,transform.position.z,1));
            if (Health < MaxHealth) Health = Mathf.Min(MaxHealth, Health + game.Run.Effect(game.Catalog, UpgradeEffect.Regeneration) * Time.deltaTime);
            if (transform.position.y < -5) { controller.enabled = false; transform.position = new Vector3(transform.position.x, 2, transform.position.z); controller.enabled = true; }
        }
        public void Hurt(float damage)
        {
            if (Invulnerable || Time.time < nextHit || !game.Ready) return;
            nextHit = Time.time + .65f;
            float armor = game.Run.Effect(game.Catalog, UpgradeEffect.Armor);
            Health = Mathf.Max(0, Health - damage / (1 + armor));
            animator?.Flash(); game.Audio.Play(SoundCue.Hurt);
            game.UI.DamageFlash(); CameraRig.Active?.Shake(.12f);
            if (Health <= 0) game.FinishRun(false);
        }
        public void Heal(float value) { Health = Mathf.Min(MaxHealth, Health + value); }
        public System.Collections.IEnumerator PlayDefeat()
        {
            if(animator==null||animator.visual==null)yield break;float begin=Time.unscaledTime;
            while(Time.unscaledTime-begin<.42f){float t=Mathf.SmoothStep(0,1,(Time.unscaledTime-begin)/.42f);animator.visual.localRotation=Quaternion.Euler(8*t,0,84*t);animator.visual.localPosition=Vector3.up*(.15f*t);yield return null;}
        }
        public void ShiftOrigin(Vector3 shift) { controller.enabled = false; transform.position -= shift; controller.enabled = true; }
    }
}
