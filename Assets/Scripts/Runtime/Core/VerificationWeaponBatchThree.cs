#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        IEnumerator WeaponBatchThreeRegression()
        {
            float deadline = Time.realtimeSinceStartup + 40;
            while ((MenuStage.Active == null || !MenuStage.Active.Ready || !game.CanBeginRun) && Time.realtimeSinceStartup < deadline) yield return null;
            yield return game.UI.WaitForPanels();
            int menuPrefabs = game.Assets.LoadedPrefabCount, menuInstances = game.Assets.InstanceCount;
            game.BeginRun("adventurer", BiomeId.Forest); deadline = Time.realtimeSinceStartup + 90;
            while (!game.Ready && Time.realtimeSinceStartup < deadline) yield return null;
            Check(game.Ready, "Third weapon batch enters an AA-loaded run"); if (!game.Ready) yield break;
            yield return game.UI.WaitForPanels(); game.Inputs.ForVerification(Vector2.zero); game.Abilities.enabled = false;
            game.Run.abilities.Add(AbilityKind.Crossbow); game.Run.abilities.Add(AbilityKind.Boomerang); game.Run.abilities.Add(AbilityKind.FrostStaff);
            game.Abilities.Refresh(); ResetWeaponProbe();

            var target = BatchTarget(Vector3.forward * 5); var behind = BatchTarget(Vector3.forward * 7); var third = BatchTarget(Vector3.forward * 9);
            yield return WeaponTime(.1f);
            var bow = new CrossbowAttack(game, game.Abilities); float health = target.Health;
            bow.Cast(target); bow.Tick(); bow.Present();
            Check(bow.Drawing && WeaponShots().Count == 0, "Crossbow draws before releasing its bolt");
            game.Pause("third-weapons", true); yield return Wait(.3f); bow.Tick();
            Check(bow.Drawing && target.Health == health && WeaponShots().Count == 0, "Pause preserves the bow draw without firing");
            game.Pause("third-weapons", false); yield return WeaponTime(.7f, bow);
            Check(Mathf.Abs(health - target.Health - 12) < .05f && Mathf.Abs(behind.MaxHealth - behind.Health - 12) < .05f && third.Health == third.MaxHealth,
                "A charged bow bolt pierces exactly two aligned enemies at its base penetration"); bow.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 5); behind = BatchTarget(Vector3.forward * 7); third = BatchTarget(Vector3.forward * 9);
            game.Run.upgrades["crossbow.pierce"] = 1; game.Run.upgrades["crossbow.frost"] = 3; yield return WeaponTime(.1f);
            bow = new CrossbowAttack(game, game.Abilities); bow.Cast(target); yield return WeaponTime(.8f, bow);
            Check(third.Health < third.MaxHealth && target.Frozen && behind.Frozen && third.Frozen,
                "The charged bow combines acquired penetration and frost on all three aligned targets"); bow.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 5); behind = BatchTarget(new Vector3(2.2f, 0, 5)); yield return WeaponTime(.1f);
            game.Run.upgrades["crossbow.bounce"] = 1;
            bow = new CrossbowAttack(game, game.Abilities); bow.Cast(target); yield return WeaponTime(.8f, bow);
            Check(Mathf.Abs(behind.MaxHealth - behind.Health - 12) < .05f, "Charged bow retains its acquired angled bounce after the primary contact"); bow.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 16); yield return WeaponTime(.1f);
            game.Run.upgrades["projectiles"] = 39; game.Run.upgrades["crossbow.volley"] = 2;
            float interval = game.Abilities.AttackInterval(AbilityKind.Crossbow);
            game.Run.upgrades["crossbow.reload"] = 1;
            bow = new CrossbowAttack(game, game.Abilities); bow.Cast(target); yield return WeaponTime(.3f, bow);
            Check(WeaponShots().Count == 16 && Mathf.Abs(ShotPower(WeaponShots()) - 504) < .1f,
                "Forty-two logical bow bolts preserve all damage inside sixteen parallel visual lanes");
            Check(game.Abilities.AttackInterval(AbilityKind.Crossbow) < interval && !bow.Drawing, "Reload upgrade speeds both casting and the draw stage");
            bow.Dispose(); ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 5); yield return WeaponTime(.1f);
            bow = new CrossbowAttack(game, game.Abilities); bow.Cast(target);
            target.Initialize(game, 2, true, BiomeId.Forest); target.DiagnosticStationary = target.DiagnosticImmortal = true;
            target.DiagnosticAttackAfter(1000); target.transform.position = Vector3.forward * 30; health = target.Health;
            yield return WeaponTime(.8f, bow);
            Check(target.Health == health && WeaponShots().Count == 0, "A draw rejects an obsolete pooled target lifetime outside its range"); bow.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); yield return WeaponTime(.1f);
            var go = game.Pool.Rent("projectile/boomerang", Vector3.up * 1.05f, Quaternion.identity);
            var blade = go.GetComponent<ProjectileActor>();
            blade.InitializeCurvedReturn(game, target.transform.position + Vector3.up * .8f, 8, game.Abilities.Packet(AbilityKind.Boomerang), 8, 1);
            health = target.Health; yield return WeaponTime(.2f);
            Check(blade.Motion == ProjectileMotion.CurvedReturn && Mathf.Abs(blade.transform.position.x) > .5f,
                "Boomerang visibly follows a curved outbound path");
            game.Pause("third-weapons", true); Vector3 paused = blade.transform.position; yield return Wait(.3f);
            Check(blade.transform.position == paused && target.Health == health, "Pause freezes the curved blade and its contact damage"); game.Pause("third-weapons", false);
            yield return WeaponTime(.45f);
            Check(Mathf.Abs(health - target.Health - 21) < .05f && !blade.OnReturnLeg, "Curved outbound leg contacts its chosen enemy once");
            yield return WeaponTime(1.4f);
            Check(Mathf.Abs(health - target.Health - 42) < .05f && !go.activeSelf,
                "Static-player curved return hits once again then returns the blade to its pool");

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); yield return WeaponTime(.1f);
            go = game.Pool.Rent("projectile/boomerang", Vector3.up * 1.05f, Quaternion.identity); blade = go.GetComponent<ProjectileActor>();
            blade.InitializeCurvedReturn(game, target.transform.position + Vector3.up * .8f, 8, game.Abilities.Packet(AbilityKind.Boomerang), 8, 1);
            yield return WeaponTime(1.1f); Place(Vector3.right * 3); yield return WeaponTime(.3f);
            Check(blade.OnReturnLeg && blade.transform.position.x > .5f, "Moving the player bends the return path toward the new player position");
            Vector3 destination = blade.Destination, beforeShift = blade.transform.position, shift = Vector3.right * 32;
            game.Combat.ShiftOrigin(shift); game.Player.ShiftOrigin(shift);
            Check(Vector3.Distance(blade.Destination, destination - shift) < .01f && Vector3.Distance(blade.transform.position, beforeShift - shift) < .01f,
                "Floating origin shifts the cached boomerang curve and visible blade exactly once");
            game.Combat.ShiftOrigin(-shift); game.Player.ShiftOrigin(-shift); yield return WeaponTime(.7f);
            Check(!go.activeSelf && Vector3.Distance(blade.transform.position, game.Player.transform.position + Vector3.up * 1.05f) < .1f,
                "A moving-player return ends at the current owner and releases its pooled instance");
            go = game.Pool.Rent("projectile/boomerang", Vector3.up, Quaternion.identity); blade = go.GetComponent<ProjectileActor>();
            blade.Initialize(game, Vector3.forward, 8, game.Abilities.Packet(AbilityKind.Boomerang), 0, 0, 0);
            Check(blade.Motion == ProjectileMotion.Standard && !blade.Returning, "Ordinary pool reuse clears all curved return state");

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 10); yield return WeaponTime(.1f);
            game.Run.upgrades["projectiles"] = 39; game.Run.upgrades["boomerang.count"] = 2; game.Run.upgrades["boomerang.reach"] = 2;
            var boomerang = new BoomerangAttack(game, game.Abilities); boomerang.Cast(target); yield return WeaponTime(.16f, boomerang);
            var curvedShots = WeaponShots();
            Vector3 reachDelta = curvedShots.Count > 0 ? curvedShots[0].Destination - game.Player.transform.position : Vector3.zero; reachDelta.y = 0;
            Check(curvedShots.Count == 16 && Mathf.Abs(ShotPower(curvedShots) - 882) < .1f && Mathf.Abs(reachDelta.magnitude - 19) < .05f,
                "Boomerang count and reach upgrades preserve forty-two damage weights on longer curved paths"); boomerang.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 6); var beside = BatchTarget(new Vector3(2.2f, 0, 6)); yield return WeaponTime(.1f);
            game.Run.upgrades["boomerang.lightning"] = 1;
            boomerang = new BoomerangAttack(game, game.Abilities); boomerang.Cast(target); yield return WeaponTime(.75f, boomerang);
            Check(Mathf.Abs(target.MaxHealth - target.Health - 21) < .1f && Mathf.Abs(beside.MaxHealth - beside.Health - 9.45f) < .1f,
                "Curved boomerang retains the acquired electric arc on a nearby off-path target"); boomerang.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); behind = BatchTarget(Vector3.back * 3);
            third = BatchTarget(new Vector3(3, 0, 1)); beside = BatchTarget(new Vector3(1, 0, 3)); var outside = BatchTarget(Vector3.forward * 9);
            yield return WeaponTime(.1f); health = target.Health;
            var frost = new FrostStaffAttack(game, game.Abilities); frost.Cast(target); frost.Tick();
            Check(frost.ActiveBreaths == 1 && WeaponShots().Count == 0 && target.Health == health,
                "Frost staff starts a directional breath rather than another generic projectile");
            yield return WeaponTime(.2f, frost);
            Check(!target.Frozen && target.Cold > .3f && Mathf.Abs(health - target.Health - 5.75f) < .05f,
                "First cone contact chills and deals one quarter of a full cast without instantly freezing");
            game.Pause("third-weapons", true); int waves = frost.EmittedWaves; float cold = target.Cold;
            yield return Wait(.3f); frost.Tick();
            Check(frost.EmittedWaves == waves && target.Cold == cold, "Pause preserves cold buildup and pending cone stages"); game.Pause("third-weapons", false);
            yield return WeaponTime(.15f, frost);
            Check(!target.Frozen && target.Cold > .6f, "Second contact continues deterministic cold buildup");
            yield return WeaponTime(.2f, frost);
            Check(target.Frozen && beside.Frozen, "Third consecutive cone contact freezes enemies in the fan");
            yield return WeaponTime(.4f, frost);
            Check(frost.EmittedWaves == 4 && frost.ActiveBreaths == 0 && Mathf.Abs(health - target.Health - 23) < .05f && Mathf.Abs(beside.MaxHealth - beside.Health - 23) < .05f,
                "Four timed cone stages retain the full base damage for every covered enemy");
            Check(behind.Health == behind.MaxHealth && third.Health == third.MaxHealth && outside.Health == outside.MaxHealth,
                "Cold cone excludes enemies behind the player outside its angle and beyond its reach"); frost.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); yield return WeaponTime(.1f);
            game.Run.upgrades["froststaff.shatter"] = 1; int freezes = game.Save.Profile.freezes;
            frost = new FrostStaffAttack(game, game.Abilities); frost.Cast(target); health = target.Health; yield return WeaponTime(.8f, frost);
            Check(Mathf.Abs(health - target.Health - 26.45f) < .1f && game.Save.Profile.freezes == freezes + 1,
                "Acquired shatter amplifies the post-freeze fourth wave by sixty percent and counts one actual freeze"); frost.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); yield return WeaponTime(.1f);
            game.Run.upgrades["froststaff.freeze"] = 1;
            frost = new FrostStaffAttack(game, game.Abilities); frost.Cast(target); yield return WeaponTime(.36f, frost);
            Check(target.Frozen && frost.EmittedWaves == 2, "Existing deep-cold upgrade freezes with two contacts instead of three"); frost.Dispose();
            target.Initialize(game, 2, true, BiomeId.Forest); target.DiagnosticStationary = target.DiagnosticImmortal = true; target.DiagnosticAttackAfter(1000);
            Check(!target.Frozen && target.Cold == 0, "A reused enemy lifetime clears both freeze and accumulated cold");
            target.Hit(new DamagePacket { source = AbilityKind.FrostStaff, chill = .4f }); yield return WeaponTime(2.1f);
            Check(target.Cold == 0 && !target.Frozen, "Interrupted cold exposure decays instead of remaining indefinitely");

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); yield return WeaponTime(.1f);
            frost = new FrostStaffAttack(game, game.Abilities); frost.Cast(target); yield return WeaponTime(.2f, frost); frost.Dispose(); health = target.Health;
            yield return WeaponTime(.8f, frost);
            Check(frost.ActiveBreaths == 0 && target.Health == health && frost.EmittedWaves == 1, "Disposing an active frost breath cancels all later damage waves");

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); yield return WeaponTime(.1f);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "DiagnosticConeOccluder"; wall.layer = 10;
            wall.transform.position = new Vector3(0, 1.5f, 1.5f); wall.transform.localScale = new Vector3(3, 4, .3f); Physics.SyncTransforms();
            frost = new FrostStaffAttack(game, game.Abilities); frost.Cast(target); health = target.Health; yield return WeaponTime(.8f, frost);
            Check(target.Health == health && target.Cold == 0, "Frost cone cannot hit through a world obstruction"); frost.Dispose(); Destroy(wall); yield return null;

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 9); beside = BatchTarget(new Vector3(5.5f, 0, 6.5f)); yield return WeaponTime(.1f);
            game.Run.upgrades["projectiles"] = 39; game.Run.upgrades["froststaff.count"] = 2; game.Run.upgrades["froststaff.radius"] = 2;
            frost = new FrostStaffAttack(game, game.Abilities); frost.Cast(target); yield return WeaponTime(.8f, frost);
            Check(Mathf.Abs(frost.EmittedPower - 966) < .2f && target.Health < target.MaxHealth && beside.Health < beside.MaxHealth && WeaponShots().Count == 0,
                "Forty-two logical frost channels retain all damage and combine with wider longer cold coverage"); frost.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 5); behind = BatchTarget(Vector3.forward * 7); yield return WeaponTime(.1f);
            yield return WeaponClip("crossbow-draw-pierce", new CrossbowAttack(game, game.Abilities), target);
            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 6); yield return WeaponTime(.1f);
            game.Run.upgrades["boomerang.count"] = 1;
            yield return WeaponClip("boomerang-curved-return", new BoomerangAttack(game, game.Abilities), target);
            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); beside = BatchTarget(new Vector3(1.5f, 0, 4)); yield return WeaponTime(.1f);
            game.Run.upgrades["froststaff.shatter"] = 1;
            yield return WeaponClip("frost-cone-freeze-shatter", new FrostStaffAttack(game, game.Abilities), target);

            ResetWeaponProbe(); foreach (var kind in ContentIds.Weapons) game.Run.abilities.Add(kind);
            game.Run.upgrades["orbit"] = game.Run.upgrades["rain"] = game.Run.upgrades["runepulse"] = 1;
            game.Abilities.Refresh(); target = BatchTarget(Vector3.forward * 5); yield return WeaponTime(.1f);
            game.Abilities.enabled = true; yield return WeaponTime(.6f);
            bool curved = false, bowShot = false;
            foreach (var shot in WeaponShots()) { curved |= shot.Motion == ProjectileMotion.CurvedReturn; bowShot |= shot.Damage.source == AbilityKind.Crossbow; }
            Check(game.Run.abilities.Count == 9 && curved && bowShot && (target.Cold > 0 || target.Frozen),
                "Automatic controller mixes all nine independent weapons including curved return charged bolts and cold breath");
            var oldOwner = game.Abilities; var oldPool = game.Pool;
            game.ReturnToMenu(); deadline = Time.realtimeSinceStartup + 70;
            while ((!game.CanBeginRun || MenuStage.Active == null || !MenuStage.Active.Ready) && Time.realtimeSinceStartup < deadline) yield return null;
            yield return game.UI.WaitForPanels(); game.UI.ClearCache(); yield return Wait(.5f);
            Check(game.CanBeginRun && oldOwner == null && oldPool.Count == 0 && game.Combat == null,
                "Returning with all nine weapon behaviors active destroys their owner and clears all pooled run objects");
            Check(game.Assets.LoadedPrefabCount == menuPrefabs && game.Assets.InstanceCount == menuInstances,
                "Third-batch cleanup restores the original menu prefab and instance lease counts");
        }
    }
}
#endif
