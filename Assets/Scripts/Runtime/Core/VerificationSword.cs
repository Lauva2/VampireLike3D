#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        [System.Serializable] sealed class SwordSample
        {
            public string phase, motion, obstruction;
            public int seed;
            public float time, elapsed, targetHealth;
            public bool active, curve;
            public Vector3 position, control, target;
        }
        void SampleSword(string phase, float began, System.Collections.Generic.List<ProjectileActor> blades, EnemyActor target)
        {
            foreach (var blade in blades)
                File.AppendAllText(Path.Combine(output, "SwordSamples.jsonl"), JsonUtility.ToJson(new SwordSample {
                    phase = phase, motion = blade.Motion.ToString(), seed = game.Run.seed, time = Time.time, elapsed = Time.time - began,
                    targetHealth = target.Health, active = blade.gameObject.activeInHierarchy, curve = blade.SwordCurveActive,
                    obstruction = blade.DiagnosticObstruction,
                    position = blade.transform.position, control = blade.SwordCurveControl, target = target.transform.position }) + "\n");
        }
        bool OrbitTipsLead()
        {
            for (int i = 0; i < game.Abilities.OrbitSwordCount; i++)
            {
                var blade = game.Abilities.OrbitSword(i).transform;
                Vector3 radius = blade.position - game.Player.transform.position; radius.y = 0;
                Vector3 tangent = Vector3.Cross(Vector3.up, radius).normalized;
                if (Vector3.Dot(blade.up, tangent) < .995f || Mathf.Abs(blade.up.y) > .01f) return false;
            }
            return game.Abilities.OrbitSwordCount > 0;
        }
        IEnumerator SwordOrbitClip()
        {
            string folder = Path.Combine(output, "Sequences", "sword-orbit-tip-first"); Directory.CreateDirectory(folder);
            int previous = Time.captureFramerate; Time.captureFramerate = 24;
            for (int i = 0; i < 72; i++)
            {
                game.Abilities.AnimateOrbit();
                RenderCapture(Path.Combine(folder, i.ToString("D4") + ".jpg"), 1280, 720, true);
                yield return null;
            }
            Time.captureFramerate = previous;
            File.WriteAllText(Path.Combine(folder, "metadata.json"), "{\"source\":\"background Player Camera.Render\",\"simulationFps\":24,\"frames\":72,\"performanceMeasurement\":false}");
        }
        IEnumerator SwordCurveRegression()
        {
            int previous = Time.captureFramerate; Time.captureFramerate = 60;
            try { yield return SwordCurveCases(); }
            finally { Time.captureFramerate = previous; }
        }
        IEnumerator SwordCurveCases()
        {
            float deadline = Time.realtimeSinceStartup + 40;
            while ((MenuStage.Active == null || !MenuStage.Active.Ready || !game.CanBeginRun) && Time.realtimeSinceStartup < deadline) yield return null;
            yield return game.UI.WaitForPanels();
            int menuPrefabs = game.Assets.LoadedPrefabCount, menuInstances = game.Assets.InstanceCount;
            game.BeginRun("adventurer", BiomeId.Forest); deadline = Time.realtimeSinceStartup + 90;
            while (game.Run == null && Time.realtimeSinceStartup < deadline) yield return null;
            Check(game.Run != null && game.World == null, "Sword fixture sets its reproducible seed before world generation");
            if (game.Run == null || game.World != null) yield break;
            game.Run.seed = 423533032;
            while (!game.Ready && Time.realtimeSinceStartup < deadline) yield return null;
            Check(game.Ready, "Sword probe enters a real AA-loaded run"); if (!game.Ready) yield break;
            yield return game.UI.WaitForPanels(); game.Inputs.ForVerification(Vector2.zero); game.Abilities.enabled = false;
            game.Run.abilities.Add(AbilityKind.Sword); game.Abilities.Refresh(); ResetWeaponProbe();

            var target = BatchTarget(Vector3.forward * 10); yield return WeaponTime(.1f);
            game.Run.upgrades["swordcount"] = 1;
            var sword = new SwordAttack(game, game.Abilities); sword.Cast(target); sword.Tick();
            var shots = WeaponShots(); float health = target.Health, castAt = Time.time;
            SampleSword("launch", castAt, shots, target);
            Check(shots.Count == 2 && shots[0].SwordHovering && Vector3.Distance(shots[0].transform.position, game.Player.transform.position) < 1.7f,
                "Seeking blades originate at the player body before their Bezier launch");
            game.Pause("sword-curve", true); Vector3 paused = shots[0].transform.position; yield return Wait(.3f);
            Check(shots[0].transform.position == paused && target.Health == health, "Pause freezes sword anticipation and its Bezier clock"); game.Pause("sword-curve", false);
            yield return WeaponTime(.36f, sword);
            SampleSword("curve-sample", castAt, shots, target);
            Check(shots[0].SwordCurveActive && shots[1].SwordCurveActive && shots[0].transform.position.x * shots[1].transform.position.x < -.16f,
                "Two launched blades fan out on opposite Bezier arcs rather than parallel straight lines");
            Vector3 control = shots[0].SwordCurveControl, beforeShift = shots[0].transform.position, shift = Vector3.right * 32;
            game.Combat.ShiftOrigin(shift); game.Player.ShiftOrigin(shift);
            Check(Vector3.Distance(shots[0].SwordCurveControl, control - shift) < .01f && Vector3.Distance(shots[0].transform.position, beforeShift - shift) < .01f,
                "Floating origin shifts the active cubic control point and blade exactly once");
            game.Combat.ShiftOrigin(-shift); game.Player.ShiftOrigin(-shift);
            Vector3 previous = shots[0].transform.position; yield return WeaponTime(.03f, sword);
            Vector3 step = shots[0].transform.position - previous;
            Check(Vector3.Dot(shots[0].transform.forward, step.normalized) > .97f, "Projectile sword tip follows the tangent of the curved flight");
            yield return WeaponTime(.8f, sword);
            SampleSword("after-impact", castAt, shots, target);
            Check(Mathf.Abs(health - target.Health - 28) < .1f, "Both curved blades converge on the real enemy and retain their full damage");
            health = target.Health; yield return WeaponTime(.7f, sword);
            Check(target.Health == health && WeaponShots().Count == 0, "Curved blades return harmlessly without repeat hits or lingering projectiles"); sword.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 10); target.DiagnosticStationary = false; yield return WeaponTime(.1f);
            health = target.Health; sword = new SwordAttack(game, game.Abilities); sword.Cast(target); yield return WeaponTime(1.2f, sword);
            Check(target.Health < health && target.Velocity.sqrMagnitude > 0, "Bezier endpoint follows a genuinely moving enemy instead of its stale cast position"); sword.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); var farther = BatchTarget(Vector3.forward * 6); yield return WeaponTime(.1f);
            sword = new SwordAttack(game, game.Abilities); sword.Cast(target); yield return WeaponTime(1.3f, sword);
            Check(Mathf.Abs(target.MaxHealth - target.Health - 14) < .1f && Mathf.Abs(farther.MaxHealth - farther.Health - 14) < .1f,
                "After penetration the sword builds another curved segment to a new enemy without double-hitting the first"); sword.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 7); yield return WeaponTime(.1f);
            sword = new SwordAttack(game, game.Abilities); sword.Cast(target); sword.Tick(); long oldLife = target.CollisionId;
            target.Initialize(game, 2, true, BiomeId.Forest); target.DiagnosticImmortal = target.DiagnosticStationary = true;
            target.DiagnosticAttackAfter(1000); target.transform.position = Vector3.forward * 30; health = target.Health;
            yield return WeaponTime(1, sword);
            Check(target.CollisionId != oldLife && target.Health == health, "A curved sword rejects its target's obsolete pooled lifetime"); sword.Dispose();
            var reused = game.Pool.Rent("projectile/sword", Vector3.up, Quaternion.identity).GetComponent<ProjectileActor>();
            reused.Initialize(game, Vector3.forward, 19, new DamagePacket { damage = 1, source = AbilityKind.Sword }, 0, 0, 0);
            Check(reused.Motion == ProjectileMotion.Standard && !reused.SwordCurveActive && reused.SwordCurveControl == Vector3.zero,
                "Ordinary projectile reuse clears the cubic control points and tracking state");

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 16); yield return WeaponTime(.1f);
            game.Run.upgrades["projectiles"] = 39;
            sword = new SwordAttack(game, game.Abilities); sword.Cast(target); sword.Tick(); shots = WeaponShots();
            bool atBody = shots.Count == 16;
            foreach (var blade in shots)
            {
                Vector3 offset = blade.transform.position - game.Player.transform.position; offset.y = 0;
                atBody &= offset.magnitude <= .451f;
            }
            Check(atBody && Mathf.Abs(ShotPower(shots) - 560) < .1f,
                "Forty logical swords retain their damage while every visible launch stays at the player torso"); sword.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 6); yield return WeaponTime(.1f);
            sword = new SwordAttack(game, game.Abilities); sword.Cast(target); sword.Tick(); shots = WeaponShots(); health = target.Health;
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "SwordProbeWall"; wall.layer = 10;
            wall.transform.position = game.Player.transform.position + Vector3.forward * 2 + Vector3.up * 1.5f;
            wall.transform.localScale = new Vector3(8, 4, .3f); Physics.SyncTransforms();
            yield return WeaponTime(.8f, sword);
            Check(shots.Count == 1 && shots[0].DiagnosticObstruction != null && shots[0].DiagnosticObstruction.StartsWith("SwordProbeWall") && target.Health == health,
                "A world obstacle appearing after launch blocks the real Bezier path without damaging the enemy behind it");
            sword.Dispose(); Destroy(wall); yield return null;

            ResetWeaponProbe(); game.Run.upgrades["orbit"] = 1; game.Abilities.Refresh(); game.Abilities.AnimateOrbit();
            Check(game.Abilities.OrbitSwordCount == 3 && OrbitTipsLead(), "All orbit sword tips point along the circular movement tangent on the horizontal plane");
            yield return WeaponTime(.4f); game.Player.transform.rotation = Quaternion.Euler(0, 143, 0); game.Abilities.AnimateOrbit();
            Check(OrbitTipsLead(), "Player turning does not reverse orbit blade orientation");
            game.Pause("sword-curve", true); paused = game.Abilities.OrbitSword(0).transform.position; yield return Wait(.2f); game.Abilities.AnimateOrbit();
            Check(paused == game.Abilities.OrbitSword(0).transform.position, "Pause freezes the orbit branch"); game.Pause("sword-curve", false);

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 5); yield return WeaponTime(.1f);
            sword = new SwordAttack(game, game.Abilities); sword.Rain(target, 0);
            Vector3 center = sword.FirstRainCenter; var ring = sword.FirstRainRing; health = target.Health;
            Check(sword.ActiveRainAreas == 1 && ring != null && WeaponShots().Count == 0 && Vector3.Distance(center, target.transform.position) < .4f,
                "Rain first creates a visible circle at the target position before any falling blade");
            Place(Vector3.back * 5); yield return WeaponTime(.23f, sword); shots = WeaponShots();
            Check(shots.Count == 3 && shots[0].Motion == ProjectileMotion.SwordRain && shots[0].transform.position.y > center.y + 7,
                "The first rain wave appears overhead inside the circle rather than at the moved player");
            Check(sword.FirstRainCenter == center, "Moving the player cannot move an already deployed rain circle");
            game.Pause("sword-curve", true); int waves = sword.EmittedRainWaves; paused = shots[0].transform.position; Quaternion rotation = ring.transform.rotation;
            yield return Wait(.4f); sword.Tick();
            Check(waves == sword.EmittedRainWaves && shots[0].transform.position == paused && ring.transform.rotation == rotation && target.Health == health,
                "Pause freezes rain scheduling falling blades circle animation and damage"); game.Pause("sword-curve", false);
            yield return WeaponTime(.8f, sword);
            Check(sword.EmittedRainWaves >= 3 && sword.ActiveRainAreas == 1 && target.Health < health,
                "The fixed circle continuously emits multiple damaging rain waves over time");
            health = target.Health; target.transform.position = center + Vector3.right * 10;
            var entering = BatchTarget(center); yield return WeaponTime(.1f, sword);
            shift = Vector3.right * 32; Vector3 oldRingPosition = ring.transform.position;
            game.Combat.ShiftOrigin(shift); game.Player.ShiftOrigin(shift); sword.ShiftOrigin(shift);
            Check(Vector3.Distance(sword.FirstRainCenter, center - shift) < .01f && Vector3.Distance(ring.transform.position, oldRingPosition - shift) < .01f,
                "Floating origin moves a fixed rain area and its managed circle exactly once");
            game.Combat.ShiftOrigin(-shift); game.Player.ShiftOrigin(-shift); sword.ShiftOrigin(-shift);
            yield return WeaponTime(.8f, sword);
            Check(target.Health == health && entering.Health < entering.MaxHealth && sword.FirstRainCenter == center,
                "Rain stays at its cast position damages enemies entering the circle and cannot chase an escaped target");
            target.Initialize(game, 2, true, BiomeId.Forest); target.DiagnosticImmortal = target.DiagnosticStationary = true;
            target.DiagnosticAttackAfter(1000); target.transform.position = center + Vector3.right * 10;
            yield return WeaponTime(2, sword);
            Check(sword.EmittedRainWaves == SwordAttack.RainWaves && sword.ActiveRainAreas == 0 && !ring.activeSelf && WeaponShots().Count == 0,
                "Rain survives source-target reuse then finishes every wave and releases the circle and falling swords"); sword.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 5); yield return WeaponTime(.1f);
            game.Run.upgrades["projectiles"] = 39; game.Run.upgrades["swordcount"] = 2; game.Run.upgrades["firesword"] = 1;
            sword = new SwordAttack(game, game.Abilities); sword.Rain(target, 0); yield return WeaponTime(.23f, sword); shots = WeaponShots();
            float power = 14 * SwordAttack.RainWavePower * 42;
            Check(shots.Count == 16 && Mathf.Abs(ShotPower(shots) - power) < .1f && Mathf.Abs(ShotPower(shots, true) - power * .3f) < .1f,
                "Rain visual budget preserves forty-two logical sword damage and acquired burn weights in every wave");
            yield return WeaponTime(3.5f, sword);
            Check(Mathf.Abs(sword.EmittedRainPower - power * SwordAttack.RainWaves) < 1,
                "All ten waves retain the complete accumulated rain damage instead of truncating upgraded swords"); sword.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 5); yield return WeaponTime(.1f);
            sword = new SwordAttack(game, game.Abilities); sword.Rain(target, 0); ring = sword.FirstRainRing; sword.Dispose();
            yield return WeaponTime(.8f, sword);
            Check(sword.ActiveRainAreas == 0 && sword.EmittedRainWaves == 0 && !ring.activeSelf && WeaponShots().Count == 0,
                "Disposing during circle anticipation returns its marker and cancels every future drop");

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 9); yield return WeaponTime(.1f);
            for (int i = 0; i < game.Abilities.OrbitSwordCount; i++) game.Abilities.OrbitSword(i).SetActive(false);
            game.Run.upgrades["swordcount"] = 3;
            yield return WeaponClip("sword-bezier-fan", new SwordAttack(game, game.Abilities), target);
            ResetWeaponProbe(); game.Run.upgrades["orbit"] = 1; game.Abilities.Refresh();
            for (int i = 0; i < game.Abilities.OrbitSwordCount; i++) game.Abilities.OrbitSword(i).SetActive(true);
            yield return SwordOrbitClip();
            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 5); entering = BatchTarget(new Vector3(1.4f, 0, 5.7f)); yield return WeaponTime(.1f);
            for (int i = 0; i < game.Abilities.OrbitSwordCount; i++) game.Abilities.OrbitSword(i).SetActive(false);
            yield return WeaponClip("sword-rain-area", new SwordAttack(game, game.Abilities), target, true, 120);

            ResetWeaponProbe(); foreach (var kind in ContentIds.Weapons) game.Run.abilities.Add(kind);
            game.Run.upgrades["orbit"] = game.Run.upgrades["rain"] = game.Run.upgrades["wave"] = game.Run.upgrades["firesword"] = 1;
            game.Abilities.Refresh(); target = BatchTarget(Vector3.forward * 6); yield return WeaponTime(.1f);
            for (int i = 0; i < game.Abilities.OrbitSwordCount; i++) game.Abilities.OrbitSword(i).SetActive(true);
            game.Abilities.enabled = true; yield return WeaponTime(.35f);
            bool curved = false, rain = false;
            foreach (var shot in WeaponShots()) { curved |= shot.SwordCurveActive; rain |= shot.Motion == ProjectileMotion.SwordRain; }
            Check(game.Run.abilities.Count == 9 && curved && rain && game.Abilities.OrbitSwordCount >= 3 && OrbitTipsLead(),
                "Bezier pursuit tangent-facing orbit sustained rain sword wave and fire-sword coexist in a real nine-weapon run");
            var oldOwner = game.Abilities; var oldPool = game.Pool;
            game.ReturnToMenu(); deadline = Time.realtimeSinceStartup + 70;
            while ((!game.CanBeginRun || MenuStage.Active == null || !MenuStage.Active.Ready) && Time.realtimeSinceStartup < deadline) yield return null;
            yield return game.UI.WaitForPanels(); game.UI.ClearCache(); yield return Wait(.5f);
            Check(game.CanBeginRun && oldOwner == null && oldPool.Count == 0 && game.Combat == null,
                "Returning with an active rain circle and cubic swords clears the entire original run pool");
            Check(game.Assets.LoadedPrefabCount == menuPrefabs && game.Assets.InstanceCount == menuInstances,
                "Sword cleanup restores the original menu AA prefab and instance leases");
        }
    }
}
#endif
