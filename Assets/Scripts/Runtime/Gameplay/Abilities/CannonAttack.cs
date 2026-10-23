using UnityEngine;

namespace VampireLike
{
    internal sealed class CannonAttack : WeaponAttack
    {
        public CannonAttack(GameManager game, AbilityController owner) : base(game, owner, AbilityKind.Cannon) { }
        public override void Cast(EnemyActor target)
        {
            facing = (target.transform.position - Position).normalized;
            Queue(target, Mathf.Min(.35f, owner.AttackInterval(kind) * .3f));
        }
        protected override void Fire(EnemyActor target, float weight)
        {
            Vector3 origin = Position + Vector3.up * 1.5f + owner.transform.right * 1.05f;
            Aim(target, origin);
            Vector3 point = target.transform.position + Vector3.up * .05f;
            Vector3 motion = target.Velocity; motion.y = 0;
            float travel = Mathf.Clamp(Vector3.Distance(origin, point) / Definition.speed, .55f, 1.6f);
            for (int n = 0; n < 3; n++) travel = Mathf.Clamp(Vector3.Distance(origin, point + motion * travel) / Definition.speed, .55f, 1.6f);
            point += motion * travel;
            int count = Count, visible = Mathf.Min(16, count);
            for (int i = 0; i < visible; i++)
            {
                float angle = i * 2.399f, radius = i == 0 ? 0 : Mathf.Sqrt(i) * .7f;
                Vector3 landing = point + new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
                var shot = Spawn(origin, (landing - origin).normalized); if (shot == null) continue;
                shot.InitializeMortar(game, landing, travel, owner.Packet(kind, weight * count / visible), 3.3f + Effect(UpgradeEffect.Explosion));
            }
            game.Combat.Effect("vfx/explosion", origin, .12f, .65f); game.Audio.Play(SoundCue.Gun); Present();
        }
        public override void Present()
        {
            var visual = Visual; if (visual == null) return;
            float recoil = Mathf.Clamp01(1 - (Time.time - lastShot) / .5f);
            visual.transform.position = Position + Vector3.up * 1.3f + owner.transform.right * 1.05f - facing * recoil * .3f;
            visual.transform.rotation = Quaternion.LookRotation(facing) * Quaternion.Euler(-35 - recoil * 15, 0, 0);
        }
    }
}
