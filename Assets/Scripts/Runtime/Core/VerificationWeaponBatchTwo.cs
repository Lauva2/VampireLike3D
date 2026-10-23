#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        EnemyActor BatchTarget(Vector3 position)
        {
            var target = game.Combat.Spawn(2, position, true);
            if (target != null) { target.DiagnosticImmortal = target.DiagnosticStationary = true; target.DiagnosticAttackAfter(1000); }
            return target;
        }
        IEnumerator WeaponBatchTwoRegression()
        {
            float deadline = Time.realtimeSinceStartup + 40;
            while ((MenuStage.Active == null || !MenuStage.Active.Ready || !game.CanBeginRun) && Time.realtimeSinceStartup < deadline) yield return null;
            yield return game.UI.WaitForPanels();
            int menuPrefabs = game.Assets.LoadedPrefabCount, menuInstances = game.Assets.InstanceCount;
            game.BeginRun("adventurer", BiomeId.Forest); deadline = Time.realtimeSinceStartup + 90;
            while (!game.Ready && Time.realtimeSinceStartup < deadline) yield return null;
            Check(game.Ready, "Second weapon batch enters a real AA-loaded run"); if (!game.Ready) yield break;
            yield return game.UI.WaitForPanels(); game.Inputs.ForVerification(Vector2.zero); game.Abilities.enabled = false;
            game.Run.abilities.Add(AbilityKind.Spirit); game.Run.abilities.Add(AbilityKind.Cannon); game.Run.abilities.Add(AbilityKind.Rune);
            game.Abilities.Refresh(); ResetWeaponProbe();

            var target = BatchTarget(Vector3.forward * 8); Check(target != null, "Summon fixture has a stationary elite target"); if (target == null) yield break;
            yield return WeaponTime(.1f);
            var spirit = new SpiritAttack(game, game.Abilities); spirit.Refresh(); float health = target.Health;
            spirit.Cast(target); spirit.Tick();
            Check(spirit.ModelCount == 1 && WeaponShots().Count == 0 && target.Health == health,
                "Spirit owns a persistent familiar and winds up instead of firing a spirit projectile");
            game.Pause("weapon-batch-two", true); Vector3 pausedPosition = spirit.FirstPosition; yield return Wait(.3f); spirit.Tick();
            Check(spirit.FirstPosition == pausedPosition && target.Health == health, "Pause freezes familiar movement and its pending strike");
            game.Pause("weapon-batch-two", false); yield return WeaponTime(1.2f, spirit);
            Check(Mathf.Abs(health - target.Health - 13) < .05f && Mathf.Abs(spirit.EmittedStrikePower - 13) < .05f,
                "A familiar physically reaches its enemy and resolves one Spirit damage packet");
            health = target.Health; yield return WeaponTime(.5f, spirit);
            Check(target.Health == health && Vector3.Distance(spirit.FirstPosition, game.Player.transform.position) < 2.5f,
                "Familiar returns to follow without dealing a second return hit");
            Vector3 independentPosition = spirit.FirstPosition;
            game.Player.transform.rotation *= Quaternion.Euler(0, 110, 0);
            Place(Vector3.back * 4);
            Check(Vector3.Distance(spirit.FirstPosition, independentPosition) < .001f,
                "Player turning and movement cannot drag a world-space familiar around the player pivot");
            Place(Vector3.zero); Vector3 familiarShift = Vector3.right * 32;
            game.Player.ShiftOrigin(familiarShift); spirit.ShiftOrigin(familiarShift);
            Check(Vector3.Distance(spirit.FirstPosition, independentPosition - familiarShift) < .001f,
                "Floating origin moves a persistent world-space familiar exactly once");
            game.Player.ShiftOrigin(-familiarShift); spirit.ShiftOrigin(-familiarShift); spirit.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 4.5f); yield return WeaponTime(.1f);
            game.Run.upgrades["spiritcount"] = 39;
            spirit = new SpiritAttack(game, game.Abilities); spirit.Cast(target); spirit.Cast(target);
            Check(spirit.LogicalCount == 40 && spirit.ModelCount == 8 && Mathf.Abs(spirit.PendingWeight - 80) < .01f,
                "Forty logical familiars and two casts retain eighty strike weights within the model budget");
            yield return WeaponTime(1.2f, spirit);
            Check(Mathf.Abs(spirit.EmittedStrikePower - 1040) < .2f && spirit.PendingWeight == 0,
                "Busy familiar aggregation delivers all queued strike damage without growing an order list");
            spirit.Dispose(); yield return WeaponTime(.1f, spirit);
            Check(spirit.ModelCount == 0, "Disposed summons cannot recreate their pooled familiar views");

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 8); yield return WeaponTime(.1f);
            float oldInterval = game.Abilities.AttackInterval(AbilityKind.Spirit);
            game.Run.upgrades["spiritspeed"] = 5; game.Run.upgrades["spiritdamage"] = 2;
            spirit = new SpiritAttack(game, game.Abilities); spirit.Cast(target); yield return WeaponTime(.85f, spirit);
            Check(game.Abilities.AttackInterval(AbilityKind.Spirit) < oldInterval && Mathf.Abs(spirit.EmittedStrikePower - 19.5f) < .05f,
                "Existing summon speed and spirit damage upgrades improve the new familiar behavior"); spirit.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 8); yield return WeaponTime(.1f);
            spirit = new SpiritAttack(game, game.Abilities); spirit.Cast(target); spirit.Tick(); long oldLife = target.CollisionId;
            target.Initialize(game, 2, true, BiomeId.Forest); target.DiagnosticImmortal = target.DiagnosticStationary = true;
            target.DiagnosticAttackAfter(1000); target.transform.position = Vector3.forward * 24; health = target.Health;
            yield return WeaponTime(1.2f, spirit);
            Check(target.CollisionId != oldLife && target.Health == health && spirit.EmittedStrikePower == 0,
                "A familiar rejects an obsolete target lifetime and never hits its out-of-range pooled replacement"); spirit.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 10); var beside = BatchTarget(Vector3.forward * 10 + Vector3.right * 2);
            var outside = BatchTarget(Vector3.forward * 10 + Vector3.right * 6); yield return WeaponTime(.1f);
            Check(target != null && beside != null && outside != null, "Mortar AoE fixtures spawn around the landing point");
            var cannon = new CannonAttack(game, game.Abilities); cannon.Cast(target); cannon.Tick();
            Check(WeaponShots().Count == 0 && cannon.Pending == 1, "Cannon visibly loads before launching a shell");
            yield return WeaponTime(.4f, cannon);
            var shots = WeaponShots(); var shell = shots.Count > 0 ? shots[0] : null;
            Check(shell != null && shell.Motion == ProjectileMotion.Mortar && shell.HasLandingMarker,
                "Cannon launches a dedicated mortar arc with an owned friendly landing indicator"); if (shell == null) yield break;
            health = target.Health; game.Pause("weapon-batch-two", true); pausedPosition = shell.transform.position; yield return Wait(.3f);
            Check(shell.transform.position == pausedPosition && target.Health == health && shell.HasLandingMarker,
                "Pause freezes the shell and keeps its landing indicator alive"); game.Pause("weapon-batch-two", false);
            yield return WeaponTime(.22f, cannon);
            Check(shell.transform.position.y > 3 && target.Health == health, "Mortar climbs high and cannot deal damage while flying over the enemy");
            yield return WeaponTime(1.1f, cannon);
            Check(Mathf.Abs(health - target.Health - 36) < .05f && Mathf.Abs(beside.MaxHealth - beside.Health - 36) < .05f && outside.Health == outside.MaxHealth,
                "Mortar lands once and damages only enemies within its explosion radius");
            Check(!shell.gameObject.activeSelf && !shell.HasLandingMarker, "Finished mortar returns both shell and owned landing indicator"); cannon.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 10); beside = BatchTarget(Vector3.forward * 10 + Vector3.right * 4.2f); yield return WeaponTime(.1f);
            game.Run.upgrades["cannoncount"] = 2; game.Run.upgrades["cannonradius"] = 2;
            cannon = new CannonAttack(game, game.Abilities); cannon.Cast(target); yield return WeaponTime(.4f, cannon);
            shots = WeaponShots();
            Check(shots.Count == 3 && Mathf.Abs(ShotPower(shots) - 108) < .05f, "Multi-shell upgrade produces three mortar landings with full damage");
            yield return WeaponTime(1.4f, cannon);
            Check(beside.Health < beside.MaxHealth, "Explosive payload upgrade extends the actual mortar damage radius"); cannon.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 10); yield return WeaponTime(.1f);
            game.Run.upgrades["cannoncount"] = 39;
            cannon = new CannonAttack(game, game.Abilities); cannon.Cast(target); yield return WeaponTime(.4f, cannon);
            shots = WeaponShots();
            Check(shots.Count == 16 && Mathf.Abs(ShotPower(shots) - 1440) < .2f, "Forty logical mortar shells retain their damage within sixteen visible shells");
            Vector3 destination = shots[0].Destination, shift = Vector3.right * 32;
            Vector3 indicatorPosition = shots[0].LandingMarkerPosition;
            game.Combat.ShiftOrigin(shift); game.Player.ShiftOrigin(shift);
            Check(Vector3.Distance(shots[0].Destination, destination - shift) < .01f && shots[0].HasLandingMarker &&
                Vector3.Distance(shots[0].LandingMarkerPosition, indicatorPosition - shift) < .01f,
                "Floating origin preserves mortar destination and landing indicator ownership");
            game.Combat.ShiftOrigin(-shift); game.Player.ShiftOrigin(-shift); cannon.Dispose(); ClearExpansionEffects();
            int markers = 0; foreach (var effect in game.GetRunObjects<TransientEffect>()) if (effect.name.StartsWith("mark")) markers++;
            Check(markers == 0, "Canceling in-flight shells releases every manually managed landing indicator");

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); outside = BatchTarget(Vector3.forward * 6); yield return WeaponTime(.1f);
            game.Run.upgrades["runepulse"] = 2;
            var rune = new RuneAttack(game, game.Abilities); rune.Cast(null); rune.Tick(); health = target.Health;
            Check(rune.ActiveSeals == 1 && rune.EmittedPulses == 0, "Rune unfolds a stationary seal before its first damage pulse");
            yield return WeaponTime(.15f, rune); Check(target.Health == health, "Rune deploy anticipation cannot damage an enemy early");
            game.Pause("weapon-batch-two", true); yield return Wait(.4f); rune.Tick();
            Check(target.Health == health && rune.EmittedPulses == 0, "Paused rune preserves its seal without advancing pulse damage"); game.Pause("weapon-batch-two", false);
            Place(Vector3.back * 10); yield return WeaponTime(.85f, rune);
            Check(rune.EmittedPulses == 3 && Mathf.Abs(health - target.Health - 40.8f) < .1f && outside.Health == outside.MaxHealth,
                "Two stacked rune upgrades emit three pulses at the original seal with exactly the existing total damage bonus");
            yield return WeaponTime(.15f, rune);
            Check(rune.ActiveSeals == 0, "Completed layered rune releases its stationary seal"); rune.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 5.5f); yield return WeaponTime(.1f);
            game.Run.upgrades["runerange"] = 2;
            rune = new RuneAttack(game, game.Abilities); rune.Cast(null); health = target.Health; yield return WeaponTime(.4f, rune);
            Check(Mathf.Abs(health - target.Health - 24) < .05f, "Rune range upgrade expands the real pulse coverage");
            shift = Vector3.right * 32; Vector3 center = rune.FirstCenter;
            game.Combat.ShiftOrigin(shift); game.Player.ShiftOrigin(shift); rune.ShiftOrigin(shift);
            Check(Vector3.Distance(rune.FirstCenter, center - shift) < .01f, "Floating origin shifts an active stationary rune seal");
            game.Combat.ShiftOrigin(-shift); game.Player.ShiftOrigin(-shift); rune.ShiftOrigin(-shift); rune.Dispose();

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); yield return WeaponTime(.1f);
            game.Run.upgrades["runepulse"] = 20;
            rune = new RuneAttack(game, game.Abilities); rune.Cast(null); yield return WeaponTime(1.7f, rune);
            Check(rune.EmittedPulses == 6 && Mathf.Abs(rune.EmittedPulsePower - 192) < .1f,
                "High-stack rune retains all accumulated damage while grouping visible pulse stages"); rune.Dispose();
            rune = new RuneAttack(game, game.Abilities); rune.Cast(null); health = target.Health; rune.Dispose(); yield return WeaponTime(.8f, rune);
            Check(rune.ActiveSeals == 0 && rune.EmittedPulses == 0 && target.Health == health, "Disposing a rune cancels its untriggered pulses");
            var reusedEffect = game.Pool.Rent("vfx/rune", Vector3.zero, Quaternion.identity);
            reusedEffect.GetComponent<TransientEffect>().Initialize(game.Pool, .1f, 1); yield return WeaponTime(.2f);
            Check(!reusedEffect.activeSelf, "A managed rune effect can be reused as a normally expiring transient effect");

            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 8); yield return WeaponTime(.1f);
            game.Run.upgrades["spiritcount"] = 2;
            yield return WeaponClip("spirit-familiar-dash", new SpiritAttack(game, game.Abilities), target);
            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 9); yield return WeaponTime(.1f);
            yield return WeaponClip("cannon-mortar", new CannonAttack(game, game.Abilities), target);
            ResetWeaponProbe(); target = BatchTarget(Vector3.forward * 3); yield return WeaponTime(.1f);
            game.Run.upgrades["runepulse"] = 3;
            yield return WeaponClip("rune-layered-pulses", new RuneAttack(game, game.Abilities), target);

            ResetWeaponProbe(); foreach (var kind in ContentIds.Weapons) game.Run.abilities.Add(kind);
            game.Run.upgrades["runepulse"] = 2; game.Run.upgrades["orbit"] = game.Run.upgrades["rain"] = 1;
            game.Abilities.Refresh(); target = BatchTarget(Vector3.forward * 7); yield return WeaponTime(.1f);
            game.Abilities.enabled = true; yield return WeaponTime(.5f);
            bool mortar = false; foreach (var shot in WeaponShots()) mortar |= shot.Motion == ProjectileMotion.Mortar;
            Check(game.Run.abilities.Count == 9 && mortar && game.Abilities.WeaponVisual(AbilityKind.Spirit) != null,
                "Real automatic controller mixes all nine weapons and dispatches the second batch independently");
            var oldOwner = game.Abilities; var oldPool = game.Pool;
            game.ReturnToMenu(); deadline = Time.realtimeSinceStartup + 70;
            while (!game.CanBeginRun && Time.realtimeSinceStartup < deadline) yield return null;
            while ((MenuStage.Active == null || !MenuStage.Active.Ready) && Time.realtimeSinceStartup < deadline) yield return null;
            yield return game.UI.WaitForPanels(); game.UI.ClearCache(); yield return Wait(.5f);
            Check(game.CanBeginRun && oldOwner == null && oldPool.Count == 0 && game.Combat == null,
                "Returning with live summons mortar indicators and layered seals clears the original run pool without callback mutation");
            Check(game.Assets.LoadedPrefabCount == menuPrefabs && game.Assets.InstanceCount == menuInstances,
                "After second-batch cleanup the original menu prefab and instance lease counts are restored");
        }
    }
}
#endif
