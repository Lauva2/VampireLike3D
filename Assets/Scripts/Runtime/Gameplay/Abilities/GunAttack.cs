using UnityEngine;

namespace VampireLike
{
    internal sealed class GunAttack : WeaponAttack
    {
        public GunAttack(GameManager game, AbilityController owner) : base(game, owner, AbilityKind.Gun) { }
        public override void Cast(EnemyActor target)
        {
            int rounds = Mathf.Max(1, 1 + Mathf.FloorToInt(Effect(UpgradeEffect.Burst)));
            int stages = Mathf.Min(8, rounds);
            float spacing = Mathf.Min(.09f, owner.AttackInterval(kind) * .65f / Mathf.Max(1, stages - 1));
            // Visual grouping retains the full number of rounds as damage weight, including high-stack builds.
            Queue(target, 0, stages, spacing, (float)rounds / stages);
        }
        protected override void Fire(EnemyActor target, float weight)
        {
            Vector3 origin = Position + Vector3.up * 1.15f + owner.transform.right * .65f;
            Vector3 direction = Aim(target, origin);
            origin += direction * .65f;
            int count = Count, visible = Mathf.Min(16, count);
            for (int i = 0; i < visible; i++)
            {
                Vector3 shotDirection = Quaternion.Euler(0, (i - (visible - 1) * .5f) * 9, 0) * direction;
                var shot = Spawn(origin, shotDirection); if (shot == null) continue;
                shot.Initialize(game, shotDirection, Definition.speed * 1.35f, owner.Packet(kind, weight * count / visible),
                    Mathf.FloorToInt(Effect(UpgradeEffect.Pierce)), Mathf.FloorToInt(Effect(UpgradeEffect.Bounce)), Effect(UpgradeEffect.Explosion));
            }
            game.Combat.Effect("vfx/explosion", origin, .075f, .24f, rotation: Quaternion.LookRotation(direction));
            game.Audio.Play(SoundCue.Gun);
            Present();
        }
        public override void Present()
        {
            var visual = Visual; if (visual == null) return;
            float recoil = Mathf.Clamp01(1 - (Time.time - lastShot) / .13f);
            visual.transform.position = Position + Vector3.up * 1.15f + owner.transform.right * .65f - facing * recoil * .16f;
            visual.transform.rotation = Quaternion.LookRotation(facing) * Quaternion.Euler(-recoil * 12, 0, 0);
        }
    }
}
