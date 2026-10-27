using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public enum ProjectileMotion { Standard, FireArc, SeekingSword, SwordReturn, SwordRain, Mortar, CurvedReturn }

    public sealed partial class ProjectileActor
    {
        public ProjectileMotion Motion { get; private set; }
        public bool SwordHovering => Motion == ProjectileMotion.SeekingSword && Time.time < launchAt;
        internal Vector3 Destination => flightEnd;
        EnemyActor seekingTarget;
        long seekingLife;
        Vector3 flightStart, flightEnd, hoverOffset;
        float launchAt, landedAt, seekUntil, flightSpeed, rainRadius;
        DamagePacket? swordWave;
        readonly List<EnemyActor> waveTargets = new List<EnemyActor>();
        GameObject landingMarker;
        Vector3 curveThrough, curveSide;
        internal bool HasLandingMarker => landingMarker != null;
        internal Vector3 LandingMarkerPosition => landingMarker != null ? landingMarker.transform.position : Vector3.zero;

        void ResetSpecialMotion()
        {
            if (landingMarker != null && game != null && game.Pool != null) game.Pool.Return(landingMarker);
            landingMarker = null;
            Motion = ProjectileMotion.Standard; seekingTarget = null; seekingLife = 0;
            flightStart = flightEnd = hoverOffset = Vector3.zero;
            launchAt = landedAt = seekUntil = flightSpeed = rainRadius = 0; swordWave = null;
            curveThrough = curveSide = Vector3.zero;
            ResetSwordCurve();
        }
        public void InitializeCurvedReturn(GameManager manager, Vector3 through, float speed, DamagePacket damage, float reach, float handedness)
        {
            Vector3 direction = through - transform.position; direction.y = 0; direction.Normalize();
            Initialize(manager, direction, speed, damage, int.MaxValue, 0, 0);
            Motion = ProjectileMotion.CurvedReturn; returning = true;
            flightStart = transform.position; curveThrough = through;
            flightEnd = flightStart + direction * Mathf.Max(2, reach); flightEnd.y = through.y;
            curveSide = Vector3.Cross(Vector3.up, direction) * handedness * Mathf.Min(1.8f, reach * .12f);
            launchAt = Time.time; turnAt = launchAt + Mathf.Max(.3f, reach / Mathf.Max(1, speed));
            landedAt = turnAt + (turnAt - launchAt) * .9f; expires = landedAt + .2f;
        }
        Vector3 OutwardCurve(float t)
        {
            if (t <= .55f)
            {
                float u = t / .55f;
                return Vector3.Lerp(flightStart, curveThrough, u) + curveSide * Mathf.Sin(u * Mathf.PI);
            }
            float v = (t - .55f) / .45f;
            return Vector3.Lerp(curveThrough, flightEnd, v) - curveSide * Mathf.Sin(v * Mathf.PI);
        }
        public void InitializeMortar(GameManager manager, Vector3 target, float travel, DamagePacket damage, float area)
        {
            Initialize(manager, target - transform.position, Vector3.Distance(target, transform.position) / Mathf.Max(.1f, travel), damage, 0, 0, area);
            Motion = ProjectileMotion.Mortar; flightStart = transform.position; flightEnd = target;
            if (Physics.Raycast(target + Vector3.up * 4, Vector3.down, out var ground, 8, 1 << 0)) flightEnd.y = ground.point.y + .05f;
            launchAt = Time.time; landedAt = launchAt + Mathf.Max(.1f, travel); expires = landedAt + .3f;
            transform.localScale = Vector3.one * 1.4f;
            landingMarker = game.Pool.Rent("vfx/mark", flightEnd + Vector3.up * .03f, Quaternion.identity);
            if (landingMarker != null)
            {
                // The priority marker is a persistent authored indicator, without a TransientEffect component.
                landingMarker.transform.localScale = new Vector3(area * 2, 1, area * 2);
            }
        }
        public void InitializeArc(GameManager manager, Vector3 target, float speed, DamagePacket damage, int penetration, int bounce, float area)
        {
            Initialize(manager, (target - transform.position).normalized, speed, damage, penetration, bounce, area);
            Motion = ProjectileMotion.FireArc; flightStart = transform.position; flightEnd = target; flightSpeed = speed;
            launchAt = Time.time; landedAt = launchAt + Mathf.Max(.25f, Vector3.Distance(flightStart, target) / Mathf.Max(1, speed));
            expires = landedAt + .2f;
        }
        public void InitializeSeeking(GameManager manager, EnemyActor target, Vector3 offset, float speed, DamagePacket damage, int penetration, int bounce, DamagePacket? wave = null, float curveSign = 1)
        {
            Initialize(manager, target.transform.position + Vector3.up * .8f - transform.position, speed, damage, penetration, bounce, 0);
            Motion = ProjectileMotion.SeekingSword; SetSeekingTarget(target); hoverOffset = offset;
            launchAt = Time.time + .16f; seekUntil = launchAt + 2.4f; flightSpeed = speed; swordWave = wave; swordArcSign = curveSign;
            expires = seekUntil + 3;
        }
        public void InitializeRain(GameManager manager, Vector3 target, float delay, DamagePacket damage, int penetration, DamagePacket? wave = null)
        {
            Initialize(manager, Vector3.down, 28, damage, penetration, 0, 0);
            Motion = ProjectileMotion.SwordRain; flightStart = transform.position; flightEnd = target;
            launchAt = Time.time + delay; landedAt = launchAt + .3f; expires = landedAt + .2f;
            swordWave = wave; rainRadius = 1.1f + penetration * .12f;
        }
        void SetSeekingTarget(EnemyActor target) { seekingTarget = target; seekingLife = target != null ? target.CollisionId : 0; swordCurveReady = false; }
        void BeginSwordReturn()
        { Motion = ProjectileMotion.SwordReturn; seekingTarget = null; expires = Time.time + 3; }
        void FinishFlight()
        {
            if (Motion == ProjectileMotion.SeekingSword) BeginSwordReturn();
            else game.Pool.Return(gameObject);
        }
        bool AdvanceSpecialMotion(out bool specialStep, out Vector3 next)
        {
            specialStep = false; next = transform.position;
            if (Motion == ProjectileMotion.CurvedReturn)
            {
                if (!returnPhase && Time.time >= turnAt) { returnPhase = true; hitIds.Clear(); }
                if (returnPhase)
                {
                    float t = Mathf.Clamp01((Time.time - turnAt) / (landedAt - turnAt));
                    Vector3 home = game.Player.transform.position + Vector3.up * 1.05f;
                    next = OutwardCurve(1 - t) + (home - flightStart) * t;
                }
                else next = OutwardCurve(Mathf.Clamp01((Time.time - launchAt) / (turnAt - launchAt)));
                specialStep = true;
                transform.Rotate(Vector3.up, 900 * Time.deltaTime, Space.World);
                return true;
            }
            if (Motion == ProjectileMotion.SwordReturn)
            {
                Vector3 home = game.Player.transform.position + Vector3.up * 1.65f;
                Vector3 delta = home - transform.position;
                if (delta.magnitude < flightSpeed * Time.deltaTime * 1.35f + .2f) game.Pool.Return(gameObject);
                else
                {
                    transform.position += delta.normalized * flightSpeed * 1.35f * Time.deltaTime;
                    transform.rotation = Quaternion.LookRotation(delta);
                }
                return false;
            }
            if (Motion == ProjectileMotion.SeekingSword)
                return AdvanceSeekingSword(out specialStep, out next);
            if (Motion == ProjectileMotion.FireArc || Motion == ProjectileMotion.SwordRain || Motion == ProjectileMotion.Mortar)
            {
                if (Time.time < launchAt) return false;
                float t = Mathf.Clamp01((Time.time - launchAt) / Mathf.Max(.01f, landedAt - launchAt));
                next = Vector3.Lerp(flightStart, flightEnd, t);
                if (Motion == ProjectileMotion.FireArc) next += Vector3.up * (4 * t * (1 - t)) * Mathf.Min(3, 1 + Vector3.Distance(flightStart, flightEnd) * .08f);
                if (Motion == ProjectileMotion.Mortar)
                {
                    next += Vector3.up * (4 * t * (1 - t) * 5.5f);
                    if (landingMarker != null) landingMarker.transform.Rotate(Vector3.up, Time.deltaTime * (20 + t * 30), Space.World);
                }
                Vector3 tangent = next - transform.position;
                if (tangent.sqrMagnitude > .0001f) { transform.rotation = Quaternion.LookRotation(tangent); velocity = tangent.normalized * Mathf.Max(1, flightSpeed); }
                specialStep = true;
            }
            return true;
        }
        bool SpecialArrivalReached() => (Motion == ProjectileMotion.FireArc || Motion == ProjectileMotion.SwordRain || Motion == ProjectileMotion.Mortar) && Time.time >= landedAt;

        void DirectImpact(EnemyActor target)
        {
            Vector3 point = target.transform.position;
            target.Hit(packet);
            game.Combat.Effect("vfx/impact", point + Vector3.up * .7f, .18f, .45f);
            if (splash > 0)
            {
                game.Combat.Query(point, splash, splashTargets);
                foreach (var other in splashTargets) if (other != target && other.CanBeTargeted) other.Hit(packet);
                PresentExplosion(point, splash);
            }
            ChainImpact(target, point);
            WaveImpact(point);
        }
        void AreaImpact(Vector3 point, float radius)
        {
            if (Physics.Raycast(point + Vector3.up * 2, Vector3.down, out var ground, 6, 1 << 0)) point.y = ground.point.y + .05f;
            game.Combat.Query(point, radius, splashTargets);
            EnemyActor first = null;
            foreach (var target in splashTargets)
            {
                if (!target.CanBeTargeted || hitIds.Contains(target.CollisionId)) continue;
                target.Hit(packet); if (first == null) first = target;
            }
            if (first != null) ChainImpact(first, point);
            if (Motion == ProjectileMotion.SwordRain)
                game.Combat.Effect("vfx/slash", point, .25f, 1.1f);
            else PresentExplosion(point, radius);
            WaveImpact(point);
        }
        void PresentExplosion(Vector3 point, float radius)
        {
            if (packet.source == AbilityKind.Cannon)
            {
                game.Combat.Effect("vfx/explosion", point, .55f, radius * 2);
                game.Combat.Effect("vfx/sand", point, .35f, radius);
                return;
            }
            game.Combat.Effect(packet.source == AbilityKind.Fireball ? "vfx/flame" : packet.source == AbilityKind.FrostStaff ? "vfx/frost" : "vfx/explosion", point, .4f, radius);
            if (packet.source == AbilityKind.Fireball)
            {
                game.Combat.Effect("vfx/explosion", point, .28f, radius * .8f);
                if (packet.burn > 0) game.Combat.LeaveFire(point, radius, packet);
            }
        }
        void WaveImpact(Vector3 point)
        {
            if (!swordWave.HasValue) return;
            var wave = swordWave.Value; swordWave = null;
            game.Combat.Query(point, 3, waveTargets);
            foreach (var other in waveTargets) if (other.CanBeTargeted) other.Hit(wave);
            Vector3 direction = velocity.sqrMagnitude > .01f ? velocity.normalized : Vector3.forward;
            game.Combat.Effect("vfx/slash", point, .35f, 2.5f, rotation: Quaternion.LookRotation(direction));
        }
        void ChainImpact(EnemyActor primary, Vector3 point)
        {
            if (packet.lightning <= 0) return;
            game.Combat.Query(point, 5, waveTargets); int left = packet.lightning;
            var proc = packet; proc.damage *= .45f; proc.lightning = 0;
            foreach (var other in waveTargets)
            {
                if (left <= 0) break;
                if (other == primary || !other.CanBeTargeted) continue;
                other.Hit(proc); game.Combat.Chain(point + Vector3.up, other.transform.position + Vector3.up); left--;
            }
        }
        public void ShiftOrigin(Vector3 shift)
        {
            transform.position -= shift; flightStart -= shift; flightEnd -= shift;
            curveThrough -= shift;
            swordControl -= shift;
            // Unlike transient effects, the authored static marker is shifted by its owning projectile.
            if (landingMarker != null) landingMarker.transform.position -= shift;
        }
        void OnDisable()
        {
            ResetSpecialMotion(); game = null; candidates.Clear(); splashTargets.Clear(); waveTargets.Clear(); hitIds.Clear();
            returning = returnPhase = false;
        }
    }
}
