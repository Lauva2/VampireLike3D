using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    internal sealed class BurningGround
    {
        sealed class Patch
        {
            public Vector3 point;
            public float radius, next, ends;
            public DamagePacket damage;
        }
        readonly List<Patch> patches = new List<Patch>();
        readonly List<EnemyActor> hits = new List<EnemyActor>();
        public int Count => patches.Count;
        public void Add(GameManager game, Vector3 point, float radius, DamagePacket packet)
        {
            var damage = packet; damage.damage = packet.burn * .5f; damage.burn = 0;
            damage.burning = true; damage.frost = 0; damage.lightning = 0; damage.shatter = false;
            patches.Add(new Patch { point = point, radius = radius, next = Time.time + .5f, ends = Time.time + 2.5f, damage = damage });
            game.Combat.Effect("vfx/flame", point + Vector3.up * .05f, 2.5f, radius * .75f);
        }
        public void Tick(GameManager game)
        {
            for (int i = patches.Count - 1; i >= 0; i--)
            {
                var patch = patches[i];
                while (Time.time >= patch.next && patch.next <= patch.ends)
                {
                    game.Combat.Query(patch.point, patch.radius, hits);
                    foreach (var target in hits) if (target.CanBeTargeted) target.Hit(patch.damage);
                    patch.next += .5f;
                }
                if (Time.time > patch.ends) patches.RemoveAt(i);
            }
        }
        public void ShiftOrigin(Vector3 shift) { foreach (var patch in patches) patch.point -= shift; }
        public void Dispose() { patches.Clear(); hits.Clear(); }
    }
}
