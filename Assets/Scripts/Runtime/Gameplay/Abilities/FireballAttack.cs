using UnityEngine;

namespace VampireLike
{
    internal sealed class FireballAttack : WeaponAttack
    {
        float chargedAt = -100, firesAt;
        public FireballAttack(GameManager game, AbilityController owner) : base(game, owner, AbilityKind.Fireball) { }
        public override void Cast(EnemyActor target)
        {
            chargedAt = Time.time;
            firesAt = Time.time + Mathf.Min(.28f, owner.AttackInterval(kind) * .3f);
            Queue(target, firesAt - Time.time);
            game.Combat.Effect("vfx/flame", Position + Vector3.up * 1.6f, firesAt - Time.time, .55f);
        }
        protected override void Fire(EnemyActor target, float weight)
        {
            Vector3 origin = Position + Vector3.up * 1.6f;
            Aim(target, origin);
            Vector3 landing = target.transform.position + Vector3.up * .8f;
            float speed = Definition.speed * .8f;
            Vector3 motion = target.Velocity; motion.y = 0;
            float travel = Vector3.Distance(origin, landing) / Mathf.Max(1, speed);
            // Iterate the intercept estimate because an approaching enemy shortens the flight.
            for (int i = 0; i < 3; i++) travel = Vector3.Distance(origin, landing + motion * travel) / Mathf.Max(1, speed);
            landing += motion * Mathf.Min(2, travel);
            Vector3 direction = (landing - origin).normalized;
            int count = Count, visible = Mathf.Min(16, count);
            for (int i = 0; i < visible; i++)
            {
                Vector3 offset = Quaternion.Euler(0, (i - (visible - 1) * .5f) * 9, 0) * direction;
                float reach = Vector3.Distance(origin, landing);
                Vector3 point = origin + offset * reach; point.y = landing.y;
                var shot = Spawn(origin, offset); if (shot == null) continue;
                shot.InitializeArc(game, point, speed, owner.Packet(kind, weight * count / visible),
                    Mathf.FloorToInt(Effect(UpgradeEffect.Pierce)), Mathf.FloorToInt(Effect(UpgradeEffect.Bounce)), 1.8f + Effect(UpgradeEffect.Explosion));
            }
            game.Audio.Play(SoundCue.Magic);
        }
        public override void Present()
        {
            var visual = Visual; if (visual == null) return;
            bool charging = Time.time >= chargedAt && Time.time < firesAt;
            float charge = charging ? Mathf.InverseLerp(chargedAt, firesAt, Time.time) : 0;
            visual.transform.position = Position + new Vector3(-.85f, 1.55f + Mathf.Sin(Time.time * 2) * .08f + charge * .25f, .15f);
            visual.transform.rotation = Quaternion.Euler(0, Time.time * 45, -15);
            visual.transform.localScale = Vector3.one * (1 + charge * .45f);
        }
    }
}
