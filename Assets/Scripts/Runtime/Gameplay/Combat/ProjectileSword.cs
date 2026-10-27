using UnityEngine;

namespace VampireLike
{
    public sealed partial class ProjectileActor
    {
        internal const float SwordLaunchHeight = 1.4f;
        Vector3 swordControl, swordArrivalOffset;
        float swordCurveBegan, swordCurveDuration, swordArcSign;
        bool swordCurveReady;
        internal Vector3 SwordCurveControl => swordControl;
        internal bool SwordCurveActive => Motion == ProjectileMotion.SeekingSword && swordCurveReady;

        void ResetSwordCurve()
        {
            swordControl = swordArrivalOffset = Vector3.zero;
            swordCurveBegan = swordCurveDuration = 0; swordArcSign = 1; swordCurveReady = false;
        }
        void BeginSwordCurve()
        {
            flightStart = transform.position;
            Vector3 end = seekingTarget.transform.position + Vector3.up * .8f;
            Vector3 delta = end - flightStart, direction = delta.normalized;
            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized * swordArcSign;
            float bend = Mathf.Clamp(delta.magnitude * .32f, 1.4f, 4);
            swordControl = flightStart + direction * delta.magnitude * .12f + side * bend + Vector3.up * .5f;
            swordArrivalOffset = -direction * Mathf.Min(2, delta.magnitude * .25f) + side * bend * .65f + Vector3.up * .35f;
            swordCurveBegan = Time.time;
            swordCurveDuration = Mathf.Clamp(delta.magnitude * 1.2f / Mathf.Max(1, flightSpeed), .28f, 1.35f);
            swordCurveReady = true;
        }
        bool AdvanceSeekingSword(out bool specialStep, out Vector3 next)
        {
            specialStep = false; next = transform.position;
            if (Time.time < launchAt)
            {
                transform.position = game.Player.transform.position + Vector3.up * SwordLaunchHeight + hoverOffset;
                return false;
            }
            if (Time.time > seekUntil) { BeginSwordReturn(); return false; }
            if (seekingTarget == null || seekingTarget.CollisionId != seekingLife || !seekingTarget.CanBeTargeted)
                SetSeekingTarget(game.Combat.Target(transform.position, 12, hitIds));
            if (seekingTarget == null) { BeginSwordReturn(); return false; }
            if (!swordCurveReady) BeginSwordCurve();
            flightEnd = seekingTarget.transform.position + Vector3.up * .8f;
            Vector3 arrival = flightEnd + swordArrivalOffset;
            float t = Mathf.Clamp01((Time.time - swordCurveBegan) / swordCurveDuration), u = 1 - t;
            next = u * u * u * flightStart + 3 * u * u * t * swordControl + 3 * u * t * t * arrival + t * t * t * flightEnd;
            Vector3 tangent = 3 * u * u * (swordControl - flightStart) + 6 * u * t * (arrival - swordControl) + 3 * t * t * (flightEnd - arrival);
            if (tangent.sqrMagnitude > .0001f) { velocity = tangent.normalized * flightSpeed; transform.rotation = Quaternion.LookRotation(tangent); }
            specialStep = true;
            return true;
        }
    }
}
