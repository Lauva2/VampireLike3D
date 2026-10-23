using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    internal sealed class RuneAttack : WeaponAttack
    {
        sealed class Seal
        {
            public Vector3 center;
            public float began, radius, next;
            public int waves, emitted;
            public DamagePacket packet;
            public GameObject ring;
            public TransientEffect effect;
        }
        readonly List<Seal> seals = new List<Seal>();
        readonly List<EnemyActor> hits = new List<EnemyActor>();
        public int ActiveSeals => seals.Count;
        internal int EmittedPulses { get; private set; }
        internal float EmittedPulsePower { get; private set; }
        internal Vector3 FirstCenter => seals.Count > 0 ? seals[0].center : Position;
        public RuneAttack(GameManager game, AbilityController owner) : base(game, owner, AbilityKind.Rune) { }
        public override void Cast(EnemyActor target)
        {
            if (Disposed) return;
            float power = Mathf.Max(0, Effect(UpgradeEffect.RunePulse));
            int waves = Mathf.Min(6, 1 + Mathf.FloorToInt(power / .35f + .0001f));
            var seal = new Seal { center = Position, began = Time.time, next = Time.time + .3f,
                radius = 4.2f + Effect(UpgradeEffect.Range), waves = waves, packet = owner.Packet(kind, (1 + power) / waves) };
            if (Physics.Raycast(seal.center + Vector3.up * 3, Vector3.down, out var ground, 6, 1 << 0)) seal.center.y = ground.point.y;
            seal.ring = game.Pool.Rent("vfx/rune", seal.center + Vector3.up * .06f, Quaternion.identity);
            if (seal.ring != null) { seal.effect = seal.ring.GetComponent<TransientEffect>(); seal.effect.BeginManaged(game.Pool, seal.radius * 2); }
            seals.Add(seal); game.Audio.Play(SoundCue.Magic);
        }
        protected override void Fire(EnemyActor target, float weight) { }
        public override void Tick()
        {
            if (Disposed || !game.Ready || game.IsPaused) return;
            for (int i = seals.Count - 1; i >= 0; i--)
            {
                var seal = seals[i];
                while (seal.emitted < seal.waves && Time.time >= seal.next)
                {
                    game.Combat.Query(seal.center, seal.radius, hits);
                    foreach (var enemy in hits) if (enemy.CanBeTargeted) enemy.Hit(seal.packet);
                    seal.emitted++; EmittedPulses++; EmittedPulsePower += seal.packet.damage; seal.next += .2f;
                    game.Combat.Effect("vfx/rune", seal.center + Vector3.up * .1f, .22f, seal.radius * 2);
                }
                if (seal.ring != null)
                {
                    float unfold = Mathf.SmoothStep(.1f, 1, Mathf.Clamp01((Time.time - seal.began) / .3f));
                    float pulse = seal.emitted > 0 ? Mathf.Clamp01(1 - (Time.time - (seal.next - .2f)) / .2f) : 0;
                    seal.ring.transform.localScale = new Vector3(seal.radius * 2 * unfold, 1, seal.radius * 2 * unfold);
                    seal.effect.PresentManaged(pulse, .65f + pulse * .35f);
                    seal.ring.transform.Rotate(Vector3.up, Time.deltaTime * (10 + pulse * 25), Space.World);
                }
                if (seal.emitted == seal.waves && Time.time > seal.next + .12f)
                { game.Pool.Return(seal.ring); seals.RemoveAt(i); }
            }
        }
        public override void Present()
        {
            var visual = Visual; if (visual == null) return;
            visual.transform.position = Position + new Vector3(-.7f, 1.3f + Mathf.Sin(Time.time * 2) * .08f, -.8f);
            visual.transform.rotation = Quaternion.Euler(0, Time.time * 20, seals.Count > 0 ? 25 : 0);
        }
        public override void ShiftOrigin(Vector3 shift) { foreach (var seal in seals) seal.center -= shift; }
        public override void Dispose()
        {
            foreach (var seal in seals) game.Pool.Return(seal.ring);
            seals.Clear(); hits.Clear(); base.Dispose();
        }
    }
}
