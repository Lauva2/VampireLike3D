using System.Collections;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class EnemyActor
    {
        bool leapActive;
        float leapHeight;
        Vector3 leapStart, leapTarget;
        public void ShiftOrigin(Vector3 shift) { transform.position -= shift; leapStart -= shift; leapTarget -= shift; }
        void ExpandedAttacks(float distance, Vector3 direction, ref float movement)
        {
            if (leapActive) { movement = 0; return; }
            if (Type == 5 && distance < 16 && Time.time >= nextAttack)
            {
                nextAttack = Time.time + (Elite ? 3.4f : 4.5f); attackAnimation = Time.time;
                var target = game.Player.transform.position;
                for (int i = 0; i < (Elite ? 3 : 1); i++) game.Combat.Hazard(target + Quaternion.Euler(0, i * 120, 0) * Vector3.forward * (i == 0 ? 0 : 2.2f), 1.5f, .9f + i * .12f, 2, damage, false);
            }
            else if (Type == 6 && distance < 13 && distance > 2 && Time.time >= nextAttack)
            {
                nextAttack = Time.time + (Elite ? 3.3f : 4.5f); attackAnimation = Time.time;
                BeginLeap(game.Player.transform.position, Elite ? 1.9f : 1.4f, false);
            }
            else if (Type == 7 && distance < 15 && Time.time >= nextAttack)
            {
                nextAttack = Time.time + 4.8f; attackAnimation = Time.time;
                chargeDirection = direction; chargeAt = Time.time + 1; chargeEnd = chargeAt + .65f;
                game.Combat.HazardLine(transform.position, transform.position + direction * 9, Elite ? 1.05f : .75f, 1, .65f, damage, false);
            }
            else if (Type == 8 && distance < 19 && Time.time >= nextAttack)
            {
                nextAttack = Time.time + (Elite ? 2.6f : 3.5f); attackAnimation = Time.time;
                StartCoroutine(PrismFan(direction));
            }
            if ((Type == 5 || Type == 8) && distance < 9) movement *= .2f;
            if (Time.time - attackAnimation < .65f && Type != 7) movement = 0;
        }
        IEnumerator PrismFan(Vector3 direction)
        {
            yield return new WaitForSeconds(.65f);
            if (!IsAlive || !game.Ready) yield break;
            int count = Elite ? 5 : 3;
            for (int i = 0; i < count; i++) game.Combat.EnemyShot(transform.position + Vector3.up, Quaternion.Euler(0, (i - (count - 1) * .5f) * 13, 0) * direction, damage, "projectile/prism");
        }
        void BeginLeap(Vector3 target, float radius, bool boss)
        {
            Vector3 start = transform.position;
            target.y = start.y;
            if (Physics.Linecast(start + Vector3.up * .8f, target + Vector3.up * .8f, 1 << 10)) return;
            game.Combat.Hazard(target, radius, .95f, .3f, damage, boss);
            StartCoroutine(Leap(start, target));
        }
        IEnumerator Leap(Vector3 start, Vector3 target)
        {
            leapActive = true; leapHeight = 0; leapStart = start; leapTarget = target;
            yield return new WaitForSeconds(.35f);
            float began = Time.time;
            while (Time.time - began < .6f && IsAlive && game.Ready)
            {
                float t = Mathf.Clamp01((Time.time - began) / .6f);
                var at = Vector3.Lerp(leapStart, leapTarget, t); at.y = transform.position.y; transform.position = at;
                leapHeight = Mathf.Sin(t * Mathf.PI) * (Type == 4 ? 3.2f : 1.6f);
                yield return null;
            }
            leapActive = false; leapHeight = 0;
        }
        void AnimateExpansion()
        {
            if (leapActive) visual.localPosition += Vector3.up * leapHeight;
            if (Type == 8) { visual.localPosition += Vector3.up * (.35f + Mathf.Sin(Time.time * 2) * .12f); visual.localRotation *= Quaternion.Euler(0, Mathf.Sin(Time.time * 1.7f) * 16, 0); }
            if (Type == 5 && Time.time - attackAnimation < .9f) visual.localScale = new Vector3(1.15f, .8f, 1.15f);
        }
        void ExpandedBossAttack(Vector3 target)
        {
            bool enraged = Health < MaxHealth * .5f;
            if (Biome == BiomeId.Swamp)
            {
                if (attacks % 2 == 1) BeginLeap(target, enraged ? 3.3f : 2.7f, true);
                else
                {
                    int count = enraged ? 6 : 4;
                    for (int i = 0; i < count; i++) game.Combat.Hazard(target + Quaternion.Euler(0, i * 360f / count, 0) * Vector3.forward * 4.4f, 1.6f, 1.05f + i * .08f, 3, damage * .7f, true);
                    if (attacks % 4 == 0) for (int i = 0; i < 2; i++) game.Combat.Spawn(5, transform.position + Quaternion.Euler(0, i * 180, 0) * Vector3.forward * 4);
                }
            }
            else
            {
                int lanes = enraged ? 4 : 3;
                Vector3 direction = target - transform.position; direction.y = 0; direction.Normalize();
                for (int i = 0; i < lanes; i++)
                {
                    Vector3 along = Quaternion.Euler(0, (i - (lanes - 1) * .5f) * 32, 0) * direction;
                    game.Combat.HazardLine(transform.position + along * 3, transform.position + along * 19, .75f, 1.1f + i * .08f, .5f, damage, true);
                }
                if (attacks % 2 == 0) for (int i = 0; i < 4; i++) game.Combat.Hazard(target + Quaternion.Euler(0, i * 90, 0) * Vector3.forward * 3.8f, 1.3f, 1.3f, .4f, damage * .8f, true);
            }
        }
    }
}
