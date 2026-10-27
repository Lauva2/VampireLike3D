using UnityEngine;

namespace VampireLike
{
    internal sealed partial class SwordAttack : WeaponAttack
    {
        public SwordAttack(GameManager game, AbilityController owner) : base(game, owner, AbilityKind.Sword) { }
        public override void Cast(EnemyActor target) { Queue(target, 0); }
        protected override void Fire(EnemyActor target, float weight)
        {
            Vector3 origin = Position + Vector3.up * ProjectileActor.SwordLaunchHeight;
            Vector3 direction = Aim(target, origin), side = Vector3.Cross(Vector3.up, direction).normalized;
            int count = Count, visible = Mathf.Min(16, count);
            for (int i = 0; i < visible; i++)
            {
                Vector3 offset = side * (visible > 1 ? Mathf.Lerp(-.45f, .45f, i / (float)(visible - 1)) : 0);
                var shot = Spawn(origin + offset, direction); if (shot == null) continue;
                DamagePacket? wave = Effect(UpgradeEffect.SwordWave) > 0 ? owner.Packet(kind, .35f * weight / visible) : (DamagePacket?)null;
                shot.InitializeSeeking(game, target, offset, Definition.speed, owner.Packet(kind, weight * count / visible),
                    1 + Mathf.FloorToInt(Effect(UpgradeEffect.Pierce)), Mathf.FloorToInt(Effect(UpgradeEffect.Bounce)), wave, i % 2 == 0 ? 1 : -1);
            }
            game.Audio.Play(SoundCue.Sword);
        }
        public override void Present()
        {
            var visual = Visual; if (visual == null) return;
            visual.transform.position = Position + new Vector3(.25f, 1.8f + Mathf.Sin(Time.time * 2.5f) * .13f, -.65f);
            visual.transform.rotation = Quaternion.Euler(0, Time.time * 18, -18);
        }
    }
}
