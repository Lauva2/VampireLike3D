using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class ProjectileActor : MonoBehaviour
    {
        GameManager game;
        DamagePacket packet;
        Vector3 velocity;
        float expires, splash;
        int pierce, bounces;
        bool enemy;
        readonly HashSet<int> hitIds = new HashSet<int>();
        readonly List<EnemyActor> candidates = new List<EnemyActor>(20);
        readonly List<EnemyActor> splashTargets = new List<EnemyActor>(24);
        public void Initialize(GameManager manager, Vector3 direction, float speed, DamagePacket damage, int penetration, int bounce, float area)
        {
            game = manager; packet = damage; velocity = direction.normalized * speed; enemy = false;
            pierce = penetration; bounces = bounce; splash = area; expires = Time.time + 3; hitIds.Clear();
            transform.rotation = Quaternion.LookRotation(direction); transform.localScale = Vector3.one;
            foreach (var trail in GetComponentsInChildren<TrailRenderer>()) trail.Clear();
        }
        public void InitializeEnemy(GameManager manager, Vector3 direction, float damage)
        {
            game = manager; velocity = direction.normalized * 7; packet.damage = damage; enemy = true;
            expires = Time.time + 5; hitIds.Clear(); transform.localScale = Vector3.one;
        }
        void Update()
        {
            if (game == null || !game.Ready || game.IsPaused) return;
            if (Time.time >= expires) { game.Pool.Return(gameObject); return; }
            Vector3 from = transform.position, step = velocity * Time.deltaTime, next = from + step;
            transform.position = next;
            if (Physics.Linecast(from, next, 1 << 10)) { game.Pool.Return(gameObject); return; }
            if (enemy)
            {
                if (SegmentDistance(game.Player.transform.position + Vector3.up * .7f, from, next) < .65f) { game.Player.Hurt(packet.damage); game.Pool.Return(gameObject); }
                return;
            }
            game.Combat.Query((from + next) * .5f, step.magnitude * .5f + 2, candidates);
            foreach (var target in candidates)
            {
                if (!target.IsAlive || hitIds.Contains(target.GetInstanceID()) || SegmentDistance(target.transform.position + Vector3.up * .8f, from, next) > (target.Type == 4 ? 1.8f : .7f)) continue;
                hitIds.Add(target.GetInstanceID());
                Vector3 impact = target.transform.position;
                target.Hit(packet);
                game.Combat.Effect("vfx/impact", impact + Vector3.up * .7f, .18f, .45f);
                if (splash > 0)
                {
                    game.Combat.Query(impact, splash, splashTargets);
                    foreach (var other in splashTargets) if (other != target && other.IsAlive) other.Hit(packet);
                    game.Combat.Effect(packet.source == AbilityKind.Fireball ? "vfx/flame" : "vfx/explosion", impact, .4f, splash);
                }
                if (packet.lightning > 0)
                {
                    game.Combat.Query(impact, 5, splashTargets); int left = packet.lightning;
                    var proc = packet; proc.damage *= .45f; proc.lightning = 0;
                    foreach (var other in splashTargets)
                    {
                        if (left <= 0) break;
                        if (other == target || !other.IsAlive) continue;
                        other.Hit(proc); game.Combat.Effect("vfx/lightning", other.transform.position + Vector3.up, .24f, 1); left--;
                    }
                }
                if (bounces > 0)
                {
                    var newTarget = game.Combat.Target(impact, 10, hitIds);
                    if (newTarget != null) { bounces--; velocity = (newTarget.transform.position + Vector3.up * .8f - transform.position).normalized * velocity.magnitude; transform.rotation = Quaternion.LookRotation(velocity); return; }
                }
                if (pierce-- <= 0) { game.Pool.Return(gameObject); return; }
            }
        }
        static float SegmentDistance(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a; float t = d.sqrMagnitude > .00001f ? Mathf.Clamp01(Vector3.Dot(p - a, d) / d.sqrMagnitude) : 0;
            return Vector3.Distance(p, a + d * t);
        }
    }
}
