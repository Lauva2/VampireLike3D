#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        bool entryHazardObserved;
        float nextEntryObservation;
        [System.Serializable] sealed class BossEntryFrame
        {
            public float time, health, arrivalRemaining;
            public bool pending, entering, bossHazard;
            public Vector3 player, center;
        }
        IEnumerator WaitForBossReady(bool immortal = false)
        {
            float deadline = Time.realtimeSinceStartup + 15;
            while (game.Ready && Time.realtimeSinceStartup < deadline)
            {
                var boss = game.Combat.Boss;
                if (boss != null)
                {
                    boss.DiagnosticImmortal = immortal;
                    if (!boss.IsEntering) yield break;
                }
                else if (!game.Combat.BossChallengeActive) break;
                yield return null;
            }
            Check(false, "Requested boss finishes summoning and entrance before diagnostic combat actions");
        }
        IEnumerator BossEntryWait(float seconds)
        {
            float until = Time.time + seconds, deadline = Time.realtimeSinceStartup + seconds + 20;
            while (game.Ready && Time.time < until && Time.realtimeSinceStartup < deadline)
            {
                // Isolate Boss damage without healing, dodging, invulnerability or modifying Boss attack timers.
                ClearEnemies();
                bool hazard = false;
                foreach (var h in game.GetRunObjects<AttackHazard>()) if (h.BossOwned) hazard = true;
                entryHazardObserved |= hazard;
                if (Time.time >= nextEntryObservation)
                {
                    nextEntryObservation = Time.time + .1f;
                    var frame = new BossEntryFrame { time = Time.time, health = game.Player.Health,
                        arrivalRemaining = game.Combat.BossArrivalRemaining,
                        pending = game.Combat.BossChallengeActive && game.Combat.Boss == null,
                        entering = game.Combat.Boss != null && game.Combat.Boss.IsEntering, bossHazard = hazard,
                        player = game.Player.transform.position, center = game.Combat.BossCenter };
                    File.AppendAllText(Path.Combine(output, "BossEntryFrames.jsonl"), JsonUtility.ToJson(frame) + "\n");
                }
                yield return null;
            }
            Check(game.Ready && Time.time >= until, "Boss entry probe advances gameplay clock without player healing");
        }
        IEnumerator BossEntryRegression()
        {
            game.BeginRun("adventurer", BiomeId.Forest);
            float deadline = Time.realtimeSinceStartup + 90;
            while (!game.Ready && Time.realtimeSinceStartup < deadline) yield return null;
            Check(game.Ready, "Boss entry probe enters a real Forest run"); if (!game.Ready) yield break;
            game.Inputs.ForVerification(Vector2.zero); game.Abilities.enabled = false;
            Place(game.World.LocalPosition(new Vector2Int(0, 2)));
            deadline = Time.realtimeSinceStartup + 35;
            while (!game.World.ViewReady && Time.realtimeSinceStartup < deadline) { ClearEnemies(); yield return null; }
            WorldSite site = null;
            foreach (var s in game.World.Sites) if (s.type == 3 && s.chunk == new Vector2Int(0, 2)) { site = s; break; }
            Check(site != null, "Actual generated Boss site is available for E interaction"); if (site == null) yield break;
            ClearEnemies(); Place(site.position + Vector3.back * .5f);
            float health = game.Player.Health;
            game.Inputs.InteractForVerification(); yield return null; yield return null;
            Check(game.Combat.Boss == null && game.Combat.BossChallengeActive && game.Combat.BossSiteId == site.id,
                "Normal E interaction reserves the challenge without spawning an immediate contact attacker");
            RenderCapture(Path.Combine(output, "Captures", "boss-summoning.png"), 1600, 900, false);
            float remaining = game.Combat.BossArrivalRemaining;
            game.Combat.ActivateBoss("duplicate", site.position, site.biome, game.Catalog.Biome(site.biome).bossId);
            Check(game.Combat.BossSiteId == site.id && Mathf.Abs(game.Combat.BossArrivalRemaining - remaining) < .01f,
                "Repeated interaction cannot duplicate the challenge or restart its countdown");
            game.Pause("boss-entry-probe", true); yield return Wait(2.3f);
            Check(game.Combat.Boss == null && Mathf.Abs(game.Combat.BossArrivalRemaining - remaining) < .05f && game.Player.Health == health,
                "Pausing longer than the summon delay freezes the countdown without damage");
            game.Pause("boss-entry-probe", false);
            yield return BossEntryWait(.8f);
            Check(game.Combat.Boss == null && game.Player.Health == health && !entryHazardObserved,
                "Standing on the summoning center stays safe before Boss arrival");
            yield return BossEntryWait(game.Combat.BossArrivalRemaining + .08f);
            var boss = game.Combat.Boss;
            Check(boss != null && boss.IsEntering && game.Player.Health == health, "Boss arrives after two seconds with a harmless entrance phase");
            if (boss == null) yield break;
            float bossHealth = boss.Health;
            boss.Hit(new DamagePacket { damage = 10000, burn = 1000, source = AbilityKind.Gun });
            Check(boss.Health == bossHealth && !boss.CanBeTargeted && game.Combat.Target(boss.transform.position, 3) == null,
                "Emerging Boss rejects damage and auto targeting until its entrance finishes");
            RenderCapture(Path.Combine(output, "Captures", "boss-emerging.png"), 1600, 900, false);
            Vector3 spawnPosition = boss.transform.position;
            yield return BossEntryWait(.5f);
            Check(boss.IsEntering && boss.transform.position == spawnPosition && game.Player.Health == health && !entryHazardObserved,
                "Emerging Boss remains stationary and cannot deal contact or skill damage");
            yield return BossEntryWait(.6f);
            Check(!boss.IsEntering && boss.CanBeTargeted && game.Player.Health < health,
                "Contact damage resumes only after the one-second entrance when the player deliberately stays adjacent");
            Place(game.Combat.BossCenter + Vector3.back * 8);
            yield return BossEntryWait(2);
            Check(entryHazardObserved, "Boss skill telegraphs resume after entrance with the normal attack windup");
            RenderCapture(Path.Combine(output, "Captures", "boss-combat.png"), 1600, 900, false);
            Vector3 center = game.Combat.BossCenter;
            Place(center + Vector3.back * 35); yield return BossEntryWait(2.2f);
            Check(!game.Combat.BossChallengeActive && game.Combat.Boss == null, "Active Boss disengages and clears challenge reservation");

            // A second request leaves before arrival; it must not produce a delayed Boss outside the arena.
            Place(center + Vector3.back * 3);
            game.Combat.ActivateBoss(site.id, center, BiomeId.Forest, game.Catalog.Biome(BiomeId.Forest).bossId);
            Place(center + Vector3.back * 35); yield return BossEntryWait(2.2f);
            Check(!game.Combat.BossChallengeActive && game.Combat.Boss == null, "Leaving during summoning cancels it after the exit buffer without a late spawn");
            Place(center + Vector3.back * .5f);
            game.Combat.ActivateBoss(site.id, center, BiomeId.Forest, game.Catalog.Biome(BiomeId.Forest).bossId);
            Vector3 shift = Vector3.right * 32;
            game.Combat.ShiftOrigin(shift); Place(center - shift + Vector3.back * .5f);
            yield return BossEntryWait(CombatWorld.BossSummonSeconds + .08f);
            boss = game.Combat.Boss;
            Check(boss != null && Vector3.Distance(boss.transform.position, center - shift) < .1f && boss.Health == boss.MaxHealth,
                "Reactivation restores full health and delayed spawning uses the shifted world position");
            game.Combat.ShiftOrigin(-shift); Place(center + Vector3.back * 35); yield return BossEntryWait(2.2f);

            foreach (var biome in ContentIds.Regions)
            {
                if (biome == BiomeId.Forest) continue;
                string id = game.Catalog.Biome(biome).bossId;
                yield return game.Assets.LoadPrefab(id, game.RunAssets, p => game.Pool.Register(id, p));
                Place(center + Vector3.back * .5f); health = game.Player.Health; entryHazardObserved = false;
                game.Combat.ActivateBoss("entry-" + biome, center, biome, id);
                Check(game.Combat.Boss == null && game.Combat.BossChallengeActive, "Biome Boss waits before spawning: " + biome);
                yield return BossEntryWait(CombatWorld.BossSummonSeconds + .08f);
                Check(game.Combat.Boss != null && game.Combat.Boss.IsEntering && game.Player.Health == health && !entryHazardObserved,
                    "Biome Boss shares the harmless entrance and full-health initialization: " + biome);
                Place(center + Vector3.back * 35); yield return BossEntryWait(2.2f);
            }
            Place(center + Vector3.back * 3);
            game.Combat.ActivateBoss("missing-prefab", center, BiomeId.Forest, "boss/missing-entry-probe");
            yield return BossEntryWait(CombatWorld.BossSummonSeconds + .1f);
            Check(!game.Combat.BossChallengeActive && game.Combat.BossSiteId == null, "Missing Boss prefab clears the pending request so the site can be retried");
            game.Combat.ActivateBoss(site.id, center, BiomeId.Forest, game.Catalog.Biome(BiomeId.Forest).bossId);
            var disposedCombat = game.Combat;
            game.ReturnToMenu(); deadline = Time.realtimeSinceStartup + 60;
            while (!game.CanBeginRun && Time.realtimeSinceStartup < deadline) yield return null;
            yield return Wait(2.4f);
            Check(game.CanBeginRun && game.Combat == null && (disposedCombat == null || !disposedCombat.BossChallengeActive),
                "Returning to menu during summoning disposes the request with no delayed spawn");
            game.BeginRun("adventurer", BiomeId.Forest); deadline = Time.realtimeSinceStartup + 60;
            while (!game.Ready && Time.realtimeSinceStartup < deadline) yield return null;
            Check(game.Ready && !game.Combat.BossChallengeActive && game.Combat.Boss == null, "New run starts without a previous summoning request");
        }
    }
}
#endif
