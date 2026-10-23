using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    internal sealed class FrostStaffAttack : WeaponAttack
    {
        sealed class Breath
        {
            public Vector3 direction;
            public float next, began, range, halfAngle;
            public int emitted;
            public DamagePacket packet;
        }
        readonly List<Breath> breaths = new List<Breath>();
        readonly List<EnemyActor> targets = new List<EnemyActor>();
        internal int ActiveBreaths => breaths.Count;
        internal int EmittedWaves { get; private set; }
        internal float EmittedPower { get; private set; }
        public FrostStaffAttack(GameManager game, AbilityController owner) : base(game, owner, AbilityKind.FrostStaff) { }
        public override void Cast(EnemyActor target)
        {
            if (Disposed || target == null) return;
            Vector3 direction = Aim(target, Position); direction.y = 0; direction.Normalize();
            var packet = owner.Packet(kind, Count / 4f);
            // Cone contact accumulates cold deterministically; other weapons retain their existing freeze chance.
            packet.chill = packet.frost; packet.frost = 0;
            breaths.Add(new Breath { direction = direction, began = Time.time, next = Time.time + .12f,
                range = Definition.range + Effect(UpgradeEffect.Range) + Effect(UpgradeEffect.Explosion),
                halfAngle = Mathf.Min(65, 32 + (Count - 1) * 2 + Effect(UpgradeEffect.Explosion) * 8), packet = packet });
            game.Audio.Play(SoundCue.Magic);
        }
        protected override void Fire(EnemyActor target, float weight) { }
        public override void Tick()
        {
            if (Disposed || !game.Ready || game.IsPaused) return;
            for (int i = breaths.Count - 1; i >= 0; i--)
            {
                var breath = breaths[i];
                while (breath.emitted < 4 && Time.time >= breath.next)
                {
                    float cosine = Mathf.Cos(breath.halfAngle * Mathf.Deg2Rad);
                    game.Combat.Query(Position, breath.range, targets);
                    foreach (var target in targets)
                    {
                        Vector3 delta = target.transform.position - Position; delta.y = 0;
                        if (!target.CanBeTargeted || delta.sqrMagnitude > breath.range * breath.range ||
                            Vector3.Dot(delta.normalized, breath.direction) < cosine || !game.Combat.Visible(Position, target)) continue;
                        bool frozen = target.Frozen;
                        target.Hit(breath.packet);
                        if (frozen && breath.packet.shatter) game.Combat.Effect("vfx/iceburst", target.transform.position, .3f, 1.2f);
                    }
                    // Small authored ice clusters trace an expanding fan, while damage uses the complete cone.
                    float distance = breath.range * (breath.emitted + 1) / 4;
                    for (int lane = -2; lane <= 2; lane++)
                    {
                        Vector3 ray = Quaternion.Euler(0, breath.halfAngle * lane / 2, 0) * breath.direction;
                        game.Combat.Effect("vfx/frost", Position + ray * distance + Vector3.up * .15f, .32f, .65f + distance * .1f);
                    }
                    breath.emitted++; breath.next += .18f; EmittedWaves++; EmittedPower += breath.packet.damage;
                }
                if (breath.emitted == 4 && Time.time > breath.began + .85f) breaths.RemoveAt(i);
            }
        }
        public override void Present()
        {
            var visual = Visual; if (visual == null) return;
            float cast = Mathf.Clamp01(1 - (Time.time - lastShot) / .85f);
            visual.transform.position = Position + new Vector3(.75f, 1.3f, -.35f);
            visual.transform.rotation = Quaternion.LookRotation(facing) * Quaternion.Euler(-25 * cast, 0, 15);
        }
        public override void Dispose() { breaths.Clear(); targets.Clear(); base.Dispose(); }
    }
}
