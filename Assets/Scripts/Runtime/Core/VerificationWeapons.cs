#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        List<ProjectileActor> WeaponShots()
        { return new List<ProjectileActor>(game.GetRunObjects<ProjectileActor>()); }
        float ShotPower(List<ProjectileActor> shots, bool burn = false)
        { float total = 0; foreach (var shot in shots) total += burn ? shot.Damage.burn : shot.Damage.damage; return total; }
        IEnumerator WeaponTime(float seconds, WeaponAttack attack = null)
        {
            float end = Time.time + seconds, deadline = Time.realtimeSinceStartup + seconds + 20;
            while (game.Ready && Time.time < end && Time.realtimeSinceStartup < deadline)
            {
                attack?.Tick(); attack?.Present();
                foreach (var enemy in game.Combat.Enemies.ToArray())
                    if (enemy != game.Combat.Boss && !enemy.DiagnosticImmortal)
                    { game.Combat.Enemies.Remove(enemy); game.Pool.Return(enemy.gameObject); }
                yield return null;
            }
            Check(game.Ready && Time.time >= end, "Weapon diagnostic advances the gameplay clock");
        }
        void ResetWeaponProbe()
        {
            game.Run.upgrades.Clear(); ClearEnemies(); ClearExpansionEffects();
            game.Combat.SelectPriorityTarget(null); Place(Vector3.zero);
        }
        IEnumerator WeaponClip(string name, WeaponAttack attack, EnemyActor target, bool rain = false, int frames = 48)
        {
            string folder = Path.Combine(output, "Sequences", name); Directory.CreateDirectory(folder);
            int previous = Time.captureFramerate; Time.captureFramerate = 24;
            if (rain) ((SwordAttack)attack).Rain(target, 0); else attack.Cast(target);
            for (int i = 0; i < frames; i++)
            {
                attack.Tick(); attack.Present();
                RenderCapture(Path.Combine(folder, i.ToString("D4") + ".jpg"), 1280, 720, true);
                yield return null;
            }
            Time.captureFramerate = previous;
            File.WriteAllText(Path.Combine(folder, "metadata.json"), "{\"source\":\"background Player Camera.Render\",\"simulationFps\":24,\"frames\":" + frames + ",\"stationaryImmortalTarget\":true,\"performanceMeasurement\":false}");
            attack.Dispose();
        }
        IEnumerator WeaponIdentityRegression()
        {
            game.BeginRun("adventurer", BiomeId.Forest);
            float deadline = Time.realtimeSinceStartup + 90;
            while (!game.Ready && Time.realtimeSinceStartup < deadline) yield return null;
            Check(game.Ready, "Weapon identity probe enters a real run through AA"); if (!game.Ready) yield break;
            yield return game.UI.WaitForPanels(); game.Inputs.ForVerification(Vector2.zero); game.Abilities.enabled = false;
            game.Run.abilities.Add(AbilityKind.Fireball); game.Run.abilities.Add(AbilityKind.Sword); game.Abilities.Refresh();
            ResetWeaponProbe();
            var target = ExpansionTarget(2, Vector3.forward * 16);
            Check(target != null, "Controlled ranged target spawns on playable terrain"); if (target == null) yield break;
            yield return WeaponTime(.1f);
            game.Run.upgrades["burst"] = 2; game.Run.upgrades["spread"] = 1;
            var gun = new GunAttack(game, game.Abilities); gun.Cast(target); gun.Tick();
            Check(WeaponShots().Count == 3 && gun.Pending == 1, "Gun starts one three-lane volley and retains two timed rounds");
            game.Pause("weapon-probe", true); float pausedAt = Time.time;
            yield return Wait(.3f); gun.Tick();
            Check(Time.time == pausedAt && WeaponShots().Count == 3, "Pause freezes a burst without emitting its remaining rounds");
            game.Pause("weapon-probe", false);
            yield return WeaponTime(.105f, gun);
            Check(WeaponShots().Count == 6, "Second burst round emits a second independent three-lane fan");
            yield return WeaponTime(.105f, gun);
            Check(WeaponShots().Count == 9 && gun.Pending == 0 && Mathf.Abs(ShotPower(WeaponShots()) - 81) < .05f,
                "Three timed rounds times three lanes preserve all nine bullet damage packets");
            gun.Dispose(); ResetWeaponProbe(); target = ExpansionTarget(2, Vector3.forward * 16); yield return WeaponTime(.1f);
            game.Run.upgrades["projectiles"] = 39; game.Run.upgrades["burst"] = 19; game.Run.upgrades["spread"] = 1;
            gun = new GunAttack(game, game.Abilities); gun.Cast(target); gun.Tick();
            Check(WeaponShots().Count == 16 && Mathf.Abs(ShotPower(WeaponShots()) - 945) < .1f,
                "High-stack gun retains forty-two lanes times twenty rounds as weighted packets within the visual budget");
            gun.Dispose(); yield return WeaponTime(.2f, gun);
            Check(gun.Pending == 0, "Disposing a queued burst cancels every remaining emission");
            ResetWeaponProbe(); target = ExpansionTarget(2, Vector3.forward * 8); yield return WeaponTime(.1f);
            game.Run.upgrades["burn"] = 1;
            var fire = new FireballAttack(game, game.Abilities); fire.Cast(target); fire.Tick();
            Check(WeaponShots().Count == 0, "Fireball charges before producing a projectile");
            yield return WeaponTime(.32f, fire);
            var shots = WeaponShots(); var arc = shots.Count > 0 ? shots[0] : null;
            Check(arc != null && arc.Motion == ProjectileMotion.FireArc, "Charged fireball uses its curved flight instead of a straight bullet");
            float targetHealth = target.Health;
            yield return WeaponTime(1.1f, fire);
            Check(target.Health < targetHealth && game.Combat.BurningAreaCount > 0, "Fireball explodes on the real target and creates acquired burning ground");
            float afterImpact = target.Health;
            yield return WeaponTime(.6f);
            Check(target.Health < afterImpact, "Burning continues after the fireball has returned to the pool");
            float remainingHealth = target.Health; int patches = game.Combat.BurningAreaCount;
            game.Pause("weapon-probe", true); yield return Wait(.6f);
            Check(target.Health == remainingHealth && game.Combat.BurningAreaCount == patches, "Pause freezes both burning damage and patch expiration");
            game.Pause("weapon-probe", false); yield return WeaponTime(2.6f);
            Check(game.Combat.BurningAreaCount == 0, "All lingering fire patches expire instead of accumulating permanently"); fire.Dispose();

            ResetWeaponProbe(); target = ExpansionTarget(0, Vector3.forward * 16); target.DiagnosticStationary = false;
            yield return WeaponTime(.15f); targetHealth = target.Health;
            Check(target.Velocity.magnitude > 1, "Long-range fireball target moves through the normal enemy movement implementation");
            fire = new FireballAttack(game, game.Abilities); fire.Cast(target); yield return WeaponTime(2.1f, fire);
            Check(target.Health < targetHealth && game.Combat.BurningAreaCount == 0,
                "Predicted arc landing hits an approaching enemy at long range without granting unacquired burning ground"); fire.Dispose();

            ResetWeaponProbe(); target = ExpansionTarget(2, Vector3.forward * 8); yield return WeaponTime(.1f);
            game.Run.upgrades["projectiles"] = 39; game.Run.upgrades["burn"] = 1;
            fire = new FireballAttack(game, game.Abilities); fire.Cast(target); yield return WeaponTime(.31f, fire);
            shots = WeaponShots();
            Check(shots.Count == 16 && Mathf.Abs(ShotPower(shots) - 640) < .1f && Mathf.Abs(ShotPower(shots, true) - 140.8f) < .1f,
                "Fireball visual aggregation preserves both direct damage and burn strength at forty logical projectiles");
            var shift = Vector3.right * 32;
            Vector3 destination = shots[0].Destination;
            game.Combat.ShiftOrigin(shift); game.Player.ShiftOrigin(shift);
            Check(Vector3.Distance(shots[0].Destination, destination - shift) < .01f, "Floating origin shifts cached arc destinations with the projectiles");
            game.Combat.ShiftOrigin(-shift); game.Player.ShiftOrigin(-shift); fire.Dispose(); ClearExpansionEffects();

            ResetWeaponProbe(); target = ExpansionTarget(2, Vector3.forward * 7); yield return WeaponTime(.1f);
            var sword = new SwordAttack(game, game.Abilities); sword.Cast(target); sword.Tick();
            shots = WeaponShots(); var blade = shots.Count > 0 ? shots[0] : null; targetHealth = target.Health;
            Check(blade != null && blade.SwordHovering, "Seeking sword visibly hovers before launching");
            yield return WeaponTime(.1f, sword);
            Check(target.Health == targetHealth, "Hovering sword cannot damage an enemy before launch");
            yield return WeaponTime(.55f, sword);
            Check(target.Health < targetHealth && (blade == null || !blade.gameObject.activeSelf || blade.Motion == ProjectileMotion.SwordReturn),
                "Seeking sword reaches its enemy and begins a harmless return");
            float afterSword = target.Health; yield return WeaponTime(.6f, sword);
            Check(target.Health == afterSword && !blade.gameObject.activeSelf, "Returning sword reaches its owner without hitting the same enemy twice");
            var reused = game.Pool.Rent("projectile/sword", Vector3.up * 1.05f, Quaternion.identity).GetComponent<ProjectileActor>();
            reused.Initialize(game, Vector3.forward, 19, new DamagePacket { source = AbilityKind.Sword, damage = 2 }, 0, 0, 0);
            Check(reused.Motion == ProjectileMotion.Standard && !reused.SwordHovering && !reused.Returning,
                "Ordinary initialization clears pooled homing hover return and wave state");
            sword.Dispose(); ResetWeaponProbe(); target = ExpansionTarget(2, Vector3.forward * 7); yield return WeaponTime(.1f);
            sword = new SwordAttack(game, game.Abilities); targetHealth = target.Health; sword.Rain(target, 0);
            Check(sword.ActiveRainAreas == 1 && sword.FirstRainRing != null && WeaponShots().Count == 0,
                "Sword rain unfolds its target circle before emitting falling swords");
            yield return WeaponTime(.23f, sword);
            shots = WeaponShots();
            Check(shots.Count == 3 && shots[0].Motion == ProjectileMotion.SwordRain && shots[0].transform.position.y > target.transform.position.y + 7,
                "Sword rain begins above its target rather than at the player muzzle");
            yield return WeaponTime(.6f, sword);
            Check(target.Health < targetHealth, "Falling sword rain deals damage at its actual landing point"); sword.Dispose();

            ResetWeaponProbe(); target = ExpansionTarget(2, Vector3.forward * 9); yield return WeaponTime(.1f);
            game.Run.upgrades["burst"] = 2; game.Run.upgrades["spread"] = 1;
            yield return WeaponClip("gun-burst-spread", new GunAttack(game, game.Abilities), target);
            ResetWeaponProbe(); target = ExpansionTarget(2, Vector3.forward * 8); yield return WeaponTime(.1f);
            game.Run.upgrades["burn"] = 1;
            yield return WeaponClip("fireball-charge-arc", new FireballAttack(game, game.Abilities), target);
            yield return WeaponTime(2.6f);
            ResetWeaponProbe(); target = ExpansionTarget(2, Vector3.forward * 9); yield return WeaponTime(.1f);
            yield return WeaponClip("sword-seek-return", new SwordAttack(game, game.Abilities), target);
            ResetWeaponProbe(); target = ExpansionTarget(2, Vector3.forward * 9); yield return WeaponTime(.1f);
            yield return WeaponClip("sword-rain", new SwordAttack(game, game.Abilities), target, true);

            ResetWeaponProbe();
            foreach (var kind in ContentIds.Weapons) game.Run.abilities.Add(kind);
            game.Run.upgrades["orbit"] = 1; game.Run.upgrades["rain"] = 1; game.Run.upgrades["wave"] = 1;
            game.Run.upgrades["firesword"] = 1; game.Run.upgrades["electricammo"] = 1;
            game.Abilities.Refresh(); game.Abilities.enabled = true;
            target = ExpansionTarget(2, Vector3.forward * 7); yield return WeaponTime(.25f);
            bool seeking = false, rain = false;
            foreach (var shot in WeaponShots()) { seeking |= shot.Motion == ProjectileMotion.SeekingSword; rain |= shot.Motion == ProjectileMotion.SwordRain; }
            Check(game.Run.abilities.Count == 9 && seeking && rain && game.GetRunObjects<ProjectileActor>().Length > 0,
                "Nine weapons coexist while independent seeking swords and vertical sword rain remain active together");
            yield return Capture("weapon-combination"); game.Abilities.enabled = false;
            ResetWeaponProbe(); yield return VerifyExpansionWeapons(); game.Abilities.enabled = false;

            ClearExpansionEffects(); game.Run.upgrades["burst"] = 5;
            target = ExpansionTarget(2, Vector3.forward * 9); gun = new GunAttack(game, game.Abilities); gun.Cast(target); gun.Tick();
            game.Combat.LeaveFire(Vector3.zero, 2, new DamagePacket { burn = 2, source = AbilityKind.Fireball });
            var oldCombat = game.Combat; var oldAbilities = game.Abilities;
            game.ReturnToMenu(); deadline = Time.realtimeSinceStartup + 60;
            while (!game.CanBeginRun && Time.realtimeSinceStartup < deadline) yield return null;
            gun.Dispose(); yield return Wait(.5f);
            Check(game.CanBeginRun && game.Combat == null && game.Abilities == null && (oldCombat == null || oldCombat.BurningAreaCount == 0) && oldAbilities == null,
                "Returning to menu destroys the weapon owner and releases projectiles and lingering fire");
        }
    }
}
#endif
