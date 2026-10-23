using UnityEngine;

namespace VampireLike
{
    internal sealed class CrossbowAttack : WeaponAttack
    {
        float began = -100, releaseAt;
        internal bool Drawing => Pending > 0 && Time.time < releaseAt;
        public CrossbowAttack(GameManager game, AbilityController owner) : base(game, owner, AbilityKind.Crossbow) { }
        public override void Cast(EnemyActor target)
        {
            if (Disposed || target == null) return;
            facing = (target.transform.position - Position).normalized;
            began = Time.time; releaseAt = began + Mathf.Min(.32f, owner.AttackInterval(kind) * .38f);
            Queue(target, releaseAt - began);
        }
        protected override void Fire(EnemyActor target, float weight)
        {
            Vector3 origin = Position + Vector3.up * 1.05f;
            Vector3 direction = Aim(target, origin);
            int count = Count, visible = Mathf.Min(16, count);
            for (int i = 0; i < visible; i++)
            {
                // Parallel tracks keep the weapon's straight-line clearing identity.
                Vector3 lane = Vector3.Cross(Vector3.up, direction).normalized * (i - (visible - 1) * .5f) * .24f;
                var shot = Spawn(origin + lane, direction); if (shot == null) continue;
                shot.Initialize(game, direction, Definition.speed, owner.Packet(kind, weight * count / visible),
                    1 + Mathf.FloorToInt(Effect(UpgradeEffect.Pierce)), Mathf.FloorToInt(Effect(UpgradeEffect.Bounce)), Effect(UpgradeEffect.Explosion));
            }
            game.Combat.Effect("vfx/impact", origin + direction * .6f, .1f, .35f);
            game.Audio.Play(SoundCue.Sword);
        }
        public override void Present()
        {
            var visual = Visual; if (visual == null) return;
            float draw = Drawing ? Mathf.Clamp01((Time.time - began) / Mathf.Max(.01f, releaseAt - began)) : 0;
            float recoil = Mathf.Clamp01(1 - (Time.time - lastShot) / .18f);
            visual.transform.position = Position + Vector3.up * 1.25f + owner.transform.right * .75f - facing * (.22f * draw + .3f * recoil);
            visual.transform.rotation = Quaternion.LookRotation(facing) * Quaternion.Euler(-8 * draw + 14 * recoil, 0, 0);
            visual.transform.localScale = new Vector3(1 + draw * .15f, 1, 1 - draw * .12f);
        }
    }
}
