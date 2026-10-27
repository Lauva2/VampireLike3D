using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    internal sealed partial class SwordAttack
    {
        internal const int RainWaves = 10;
        internal const float RainWavePower = .3f, RainDuration = 3.5f;
        sealed class RainArea
        {
            public Vector3 center;
            public float began, next, radius;
            public int emitted;
            public GameObject ring;
            public TransientEffect effect;
        }
        readonly List<RainArea> rainAreas = new List<RainArea>();
        internal int ActiveRainAreas => rainAreas.Count;
        internal int EmittedRainWaves { get; private set; }
        internal float EmittedRainPower { get; private set; }
        internal Vector3 FirstRainCenter => rainAreas.Count > 0 ? rainAreas[0].center : Vector3.zero;
        internal GameObject FirstRainRing => rainAreas.Count > 0 ? rainAreas[0].ring : null;

        public void Rain(EnemyActor target, int order)
        {
            if (Disposed || target == null || !target.CanBeTargeted) return;
            var area = new RainArea { center = target.transform.position, began = Time.time,
                next = Time.time + .2f, radius = Mathf.Max(3 + Effect(UpgradeEffect.Range) * .3f,
                    1.1f + (1 + Mathf.FloorToInt(Effect(UpgradeEffect.Pierce))) * .12f) };
            if (Physics.Raycast(area.center + Vector3.up * 4, Vector3.down, out var ground, 8, 1 << 0)) area.center.y = ground.point.y;
            area.ring = game.Pool.Rent("vfx/rune", area.center + Vector3.up * .06f, Quaternion.identity);
            if (area.ring != null)
            {
                area.effect = area.ring.GetComponent<TransientEffect>(); area.effect.BeginManaged(game.Pool, area.radius * 2);
                area.ring.transform.localScale = new Vector3(area.radius * .2f, 1, area.radius * .2f);
                area.effect.PresentManaged(.15f, 1, false);
            }
            rainAreas.Add(area);
        }
        public override void Tick()
        {
            base.Tick();
            if (Disposed || !game.Ready || game.IsPaused) return;
            for (int i = rainAreas.Count - 1; i >= 0; i--)
            {
                var area = rainAreas[i];
                while (area.emitted < RainWaves && Time.time >= area.next)
                {
                    DropWave(area); area.emitted++; EmittedRainWaves++; area.next += .3f;
                }
                float elapsed = Time.time - area.began;
                if (area.ring != null)
                {
                    float unfold = Mathf.SmoothStep(.1f, 1, Mathf.Clamp01(elapsed / .2f));
                    float fade = Mathf.Clamp01((RainDuration - elapsed) / .25f);
                    area.ring.transform.localScale = new Vector3(area.radius * 2 * unfold, 1, area.radius * 2 * unfold);
                    area.effect.PresentManaged(.15f + .2f * Mathf.PingPong(elapsed * 2, 1), fade, false);
                    area.ring.transform.Rotate(Vector3.up, Time.deltaTime * 20, Space.World);
                }
                if (elapsed >= RainDuration) { game.Pool.Return(area.ring); rainAreas.RemoveAt(i); }
            }
        }
        void DropWave(RainArea area)
        {
            int count = Count, visible = Mathf.Min(16, count + 2);
            int penetration = 1 + Mathf.FloorToInt(Effect(UpgradeEffect.Pierce));
            float scatter = Mathf.Max(0, area.radius - (1.1f + penetration * .12f));
            for (int i = 0; i < visible; i++)
            {
                float angle = area.emitted * 2.399f + i * 2.399f;
                float radius = i == 0 ? 0 : scatter * Mathf.Sqrt(i / (float)visible);
                Vector3 point = area.center + new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
                if (Physics.Raycast(point + Vector3.up * 4, Vector3.down, out var ground, 8, 1 << 0)) point.y = ground.point.y;
                var shot = Spawn(point + Vector3.up * 8, Vector3.down); if (shot == null) continue;
                float weight = RainWavePower * count / visible;
                DamagePacket? wave = Effect(UpgradeEffect.SwordWave) > 0 ? owner.Packet(kind, .35f * weight) : (DamagePacket?)null;
                var packet = owner.Packet(kind, weight); EmittedRainPower += packet.damage;
                shot.InitializeRain(game, point + Vector3.up * .05f, .05f, packet, penetration, wave);
            }
            // Keep audio per wave, independently of the number of weighted visuals.
            game.Audio.Play(SoundCue.Sword);
        }
        public override void ShiftOrigin(Vector3 shift) { foreach (var area in rainAreas) area.center -= shift; }
        public override void Dispose()
        {
            if (game.Pool != null) foreach (var area in rainAreas) game.Pool.Return(area.ring);
            rainAreas.Clear(); base.Dispose();
        }
    }
}
