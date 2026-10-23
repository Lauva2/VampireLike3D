using UnityEngine;

namespace VampireLike
{
    internal sealed class SwordAttack : WeaponAttack
    {
        public SwordAttack(GameManager game, AbilityController owner) : base(game, owner, AbilityKind.Sword) { }
        public override void Cast(EnemyActor target) { Queue(target, 0); }
        protected override void Fire(EnemyActor target, float weight)
        {
            Vector3 origin = Position + Vector3.up * 1.65f;
            Vector3 direction = Aim(target, origin), side = Vector3.Cross(Vector3.up, direction).normalized;
            int count = Count, visible = Mathf.Min(16, count);
            for (int i = 0; i < visible; i++)
            {
                Vector3 offset = side * ((i - (visible - 1) * .5f) * .28f);
                var shot = Spawn(origin + offset, direction); if (shot == null) continue;
                DamagePacket? wave = Effect(UpgradeEffect.SwordWave) > 0 ? owner.Packet(kind, .35f * weight / visible) : (DamagePacket?)null;
                shot.InitializeSeeking(game, target, offset, Definition.speed, owner.Packet(kind, weight * count / visible),
                    1 + Mathf.FloorToInt(Effect(UpgradeEffect.Pierce)), Mathf.FloorToInt(Effect(UpgradeEffect.Bounce)), wave);
            }
            game.Audio.Play(SoundCue.Sword);
        }
        public void Rain(EnemyActor target, int order)
        {
            int count = Count, visible = Mathf.Min(16, count);
            Vector3 center = target.transform.position + Vector3.up * .8f;
            for (int i = 0; i < visible; i++)
            {
                Vector3 point = center + new Vector3((i - (visible - 1) * .5f) * .32f, 0, 0);
                var shot = Spawn(point + Vector3.up * 8, Vector3.down); if (shot == null) continue;
                DamagePacket? wave = Effect(UpgradeEffect.SwordWave) > 0 ? owner.Packet(kind, .35f / visible) : (DamagePacket?)null;
                shot.InitializeRain(game, point, .12f + order * .035f, owner.Packet(kind, .65f * count / visible),
                    1 + Mathf.FloorToInt(Effect(UpgradeEffect.Pierce)), wave);
            }
        }
        public override void Present()
        {
            var visual = Visual; if (visual == null) return;
            visual.transform.position = Position + new Vector3(.25f, 1.8f + Mathf.Sin(Time.time * 2.5f) * .13f, -.65f);
            visual.transform.rotation = Quaternion.Euler(0, Time.time * 18, -18);
        }
    }
}
