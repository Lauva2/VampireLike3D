using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class ProjectileActor : MonoBehaviour
    {
        GameManager game;
        DamagePacket packet;
        Vector3 velocity;
        float expires, splash;
        int pierce, bounces;
        bool enemy, returning, returnPhase;
        float turnAt, returnSpeed;
        public bool Returning => returning;
        public bool OnReturnLeg => returning && returnPhase;
        internal DamagePacket Damage => packet;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal string DiagnosticObstruction { get; private set; }
#endif
        readonly HashSet<long> hitIds = new HashSet<long>();
        readonly List<EnemyActor> candidates = new List<EnemyActor>(20);
        readonly List<EnemyActor> splashTargets = new List<EnemyActor>(24);
        public void Initialize(GameManager manager, Vector3 direction, float speed, DamagePacket damage, int penetration, int bounce, float area)
        {
            ResetSpecialMotion();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            DiagnosticObstruction = null;
#endif
            game = manager; packet = damage; velocity = direction.normalized * speed; enemy = false; returning = returnPhase = false;
            pierce = penetration; bounces = bounce; splash = area; expires = Time.time + 3; hitIds.Clear();
            transform.rotation = Quaternion.LookRotation(direction); transform.localScale = Vector3.one;
            foreach (var trail in GetComponentsInChildren<TrailRenderer>()) trail.Clear();
        }
        public void InitializeReturn(GameManager manager, Vector3 direction, float speed, DamagePacket damage, float reach)
        {
            direction.y = 0;
            Initialize(manager, direction, speed, damage, int.MaxValue, 0, 0);
            returning = true; returnSpeed = speed;
            float flight = Mathf.Max(2, reach) / Mathf.Max(1, speed);
            turnAt = Time.time + flight; expires = Time.time + flight * 2 + 4;
        }
        public void InitializeEnemy(GameManager manager, Vector3 direction, float damage)
        {
            Initialize(manager, direction, 7, new DamagePacket { damage = damage }, 0, 0, 0); enemy = true;
            expires = Time.time + 5; hitIds.Clear(); transform.localScale = Vector3.one;
        }
        void Update()
        {
            if (game == null || !game.Ready || game.IsPaused) return;
            if (Time.time >= expires) { game.Pool.Return(gameObject); return; }
            if (!AdvanceSpecialMotion(out bool specialStep, out Vector3 specialNext)) return;
            if (returning && Motion != ProjectileMotion.CurvedReturn)
            {
                if (!returnPhase && Time.time >= turnAt) { returnPhase = true; hitIds.Clear(); }
                if (returnPhase)
                {
                    Vector3 delta = game.Player.transform.position + Vector3.up * 1.05f - transform.position;
                    if (delta.magnitude <= returnSpeed * Time.deltaTime * 1.15f + .2f) { game.Pool.Return(gameObject); return; }
                    velocity = delta.normalized * returnSpeed * 1.15f;
                }
                transform.Rotate(Vector3.up, 720 * Time.deltaTime, Space.World);
            }
            Vector3 from = transform.position, next = specialStep ? specialNext : from + velocity * Time.deltaTime, step = next - from;
            transform.position = next;
            if (Physics.Linecast(from, next, out var obstruction, (1 << 10)|1))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (game.DiagnosticMode) DiagnosticObstruction = obstruction.collider.name + " layer=" + obstruction.collider.gameObject.layer + " at=" + obstruction.point;
#endif
                if (Motion == ProjectileMotion.FireArc || Motion == ProjectileMotion.Mortar) AreaImpact(obstruction.point, splash);
                game.Pool.Return(gameObject); return;
            }
            if (Motion == ProjectileMotion.Mortar)
            {
                if (SpecialArrivalReached()) { AreaImpact(next, splash); game.Pool.Return(gameObject); }
                return;
            }
            if (enemy)
            {
                if (SegmentDistance(game.Player.transform.position + Vector3.up * .7f, from, next) < .65f) { game.Player.Hurt(packet.damage); game.Pool.Return(gameObject); }
                return;
            }
            game.Combat.Query((from + next) * .5f, step.magnitude * .5f + 2, candidates);
            foreach (var target in candidates)
            {
                if (!target.IsAlive || hitIds.Contains(target.CollisionId) || SegmentDistance(target.transform.position + Vector3.up * .8f, from, next) > (target.Type == 4 ? 1.8f : .7f)) continue;
                hitIds.Add(target.CollisionId);
                Vector3 impact = target.transform.position;
                DirectImpact(target);
                if (bounces > 0)
                {
                    var newTarget = game.Combat.Target(impact, 10, hitIds);
                    if (newTarget != null) { bounces--; SetSeekingTarget(newTarget);
                        if (Motion == ProjectileMotion.FireArc) { Motion = ProjectileMotion.Standard; expires = Time.time + 2; }
                        velocity = (newTarget.transform.position + Vector3.up * .8f - transform.position).normalized * velocity.magnitude; transform.rotation = Quaternion.LookRotation(velocity); return; }
                }
                if (pierce-- <= 0) { FinishFlight(); return; }
                if (Motion == ProjectileMotion.SeekingSword)
                {
                    var nextTarget = game.Combat.Target(impact, 8, hitIds);
                    if (nextTarget == null) { BeginSwordReturn(); return; }
                    SetSeekingTarget(nextTarget);
                    return;
                }
            }
            if (Motion == ProjectileMotion.CurvedReturn && Time.time >= landedAt) { game.Pool.Return(gameObject); return; }
            if (specialStep && SpecialArrivalReached()) { AreaImpact(next, Motion == ProjectileMotion.SwordRain ? rainRadius : splash); game.Pool.Return(gameObject); }
        }
        static float SegmentDistance(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a; float t = d.sqrMagnitude > .00001f ? Mathf.Clamp01(Vector3.Dot(p - a, d) / d.sqrMagnitude) : 0;
            return Vector3.Distance(p, a + d * t);
        }
    }
}
