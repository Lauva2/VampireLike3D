#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        [System.Serializable] sealed class ExpansionAttackFrame
        {
            public float gameSeconds,elapsed,visualHeight;
            public string phase;public int type,hazards,prismShots;public bool elite;
            public Vector3 position,player;
        }
        EnemyActor observedExpansionEnemy;
        float expansionObservedAt,nextExpansionObservation,maxLeapHeight;
        bool observedExpansionHazard,observedPrismShot;
        void ObserveExpansionAttack()
        {
            var enemy=observedExpansionEnemy;
            if(enemy==null||!game.Ready||Time.time<nextExpansionObservation)return;
            nextExpansionObservation=Time.time+.065f;
            int hazards=game.GetRunObjects<AttackHazard>().Length,prisms=0;
            foreach(var shot in game.GetRunObjects<ProjectileActor>())if(shot.name.StartsWith("prism"))prisms++;
            observedExpansionHazard|=hazards>0;observedPrismShot|=prisms>0;maxLeapHeight=Mathf.Max(maxLeapHeight,enemy.visual.localPosition.y);
            var data=new ExpansionAttackFrame{gameSeconds=game.Run.seconds,elapsed=Time.time-expansionObservedAt,visualHeight=enemy.visual.localPosition.y,phase=enemy.AttackPhase,type=enemy.Type,elite=enemy.Elite,hazards=hazards,prismShots=prisms,position=enemy.transform.position,player=game.Player.transform.position};
            File.AppendAllText(Path.Combine(output,"ExpansionAttackFrames.jsonl"),JsonUtility.ToJson(data)+"\n");
        }
        void ClearExpansionEffects()
        {
            foreach (var shot in game.GetRunObjects<ProjectileActor>()) game.Pool.Return(shot.gameObject);
            foreach (var hazard in game.GetRunObjects<AttackHazard>()) game.Pool.Return(hazard.gameObject);
        }
        EnemyActor ExpansionTarget(int type, Vector3 position)
        {
            var enemy = game.Combat.Spawn(type, position);
            if (enemy != null) { enemy.DiagnosticImmortal = true; enemy.DiagnosticStationary = true; }
            return enemy;
        }
        IEnumerator VerifyExpansionWeapons()
        {
            game.UI.CloseModal(); ClearEnemies(); ClearExpansionEffects(); Place(Vector3.zero);
            game.Inputs.ForVerification(Vector2.zero); game.Abilities.enabled = false;
            try
            {
                Vector3 origin = game.Player.transform.position + Vector3.up * 1.05f;
                var target = ExpansionTarget(0, game.Player.transform.position + Vector3.forward * 4);
                Check(target != null, "Returning weapon diagnostic has a real target");
                if (target != null)
                {
                    float health = target.Health;
                    var shot = game.Pool.Rent("projectile/boomerang", origin, Quaternion.identity).GetComponent<ProjectileActor>();
                    shot.InitializeReturn(game, Vector3.forward, 9, new DamagePacket { damage = 2, source = AbilityKind.Boomerang }, 9);
                    yield return AdvanceGameplay(.65f);
                    Check(Mathf.Abs(target.Health - (health - 2)) < .02f && shot.Returning && !shot.OnReturnLeg, "Boomerang outbound leg hits its target exactly once");
                    Place(game.Player.transform.position+Vector3.right*.6f);
                    yield return AdvanceGameplay(1.5f);
                    Check(Mathf.Abs(target.Health - (health - 4)) < .02f && !shot.gameObject.activeSelf, "Boomerang returns to the moving owner and hits once on each leg");
                    long firstLife = target.CollisionId;
                    target.Initialize(game, 0, false, BiomeId.Forest); target.DiagnosticImmortal = target.DiagnosticStationary = true;
                    Check(target.CollisionId != firstLife, "Reinitializing a pooled enemy changes its collision lifetime identity");
                    shot = game.Pool.Rent("projectile/boomerang", origin, Quaternion.identity).GetComponent<ProjectileActor>();
                    shot.Initialize(game, Vector3.forward, 20, new DamagePacket { damage = 2, source = AbilityKind.Crossbow }, 0, 0, 0);
                    Check(!shot.Returning && !shot.OnReturnLeg, "Ordinary projectile initialization clears pooled return state");
                    yield return AdvanceGameplay(.4f);
                    Check(Mathf.Abs(target.Health - (target.MaxHealth - 2)) < .02f, "Reused projectile can hit the new lifetime of the same enemy object");
                }
                ClearEnemies(); ClearExpansionEffects(); origin = game.Player.transform.position + Vector3.up * 1.05f;
                var line = new List<EnemyActor>();
                for (int i = 0; i < 3; i++) line.Add(ExpansionTarget(0, game.Player.transform.position + Vector3.forward * (3 + i * 2)));
                Check(line.TrueForAll(e => e != null), "Crossbow penetration targets spawn on the real world");
                if (line.TrueForAll(e => e != null))
                {
                    var shot = game.Pool.Rent("projectile/crossbow", origin, Quaternion.identity).GetComponent<ProjectileActor>();
                    shot.Initialize(game, Vector3.forward, 30, new DamagePacket { damage = 2, source = AbilityKind.Crossbow }, 1, 0, 0);
                    yield return AdvanceGameplay(.45f);
                    Check(line[0].Health < line[0].MaxHealth && line[1].Health < line[1].MaxHealth && Mathf.Approximately(line[2].Health, line[2].MaxHealth), "Base crossbow penetrates two targets and stops before the third");
                }
                ClearEnemies(); ClearExpansionEffects();
                var bounceA=ExpansionTarget(0,game.Player.transform.position+Vector3.forward*4);
                var bounceB=ExpansionTarget(0,game.Player.transform.position+Vector3.forward*4+Vector3.right*2.5f);
                if(bounceA!=null&&bounceB!=null)
                {
                    var bolt=game.Pool.Rent("projectile/crossbow",origin,Quaternion.identity).GetComponent<ProjectileActor>();
                    bolt.Initialize(game,Vector3.forward,30,new DamagePacket{damage=2,source=AbilityKind.Crossbow},0,1,0);
                    yield return AdvanceGameplay(.65f);
                    Check(bounceA.Health<bounceA.MaxHealth&&bounceB.Health<bounceB.MaxHealth,"Crossbow bounce turns toward and damages a second real target");
                }
                ClearEnemies();ClearExpansionEffects();
                var first = ExpansionTarget(0, game.Player.transform.position + Vector3.forward * 4);
                var beside = ExpansionTarget(0, game.Player.transform.position + Vector3.forward * 4 + Vector3.right * 1.7f);
                if (first != null && beside != null)
                {
                    var shot = game.Pool.Rent("projectile/froststaff", origin, Quaternion.identity).GetComponent<ProjectileActor>();
                    shot.Initialize(game, Vector3.forward, 15, new DamagePacket { damage = 2, source = AbilityKind.FrostStaff, frost = 1 }, 0, 0, 2.4f);
                    yield return AdvanceGameplay(.4f);
                    Check(first.Frozen && beside.Frozen && first.Health < first.MaxHealth && beside.Health < beside.MaxHealth, "Frost projectile explodes and freezes both direct and nearby targets");
                    float before = first.Health;
                    first.Hit(new DamagePacket { damage = 2, source = AbilityKind.FrostStaff, shatter = true });
                    Check(Mathf.Abs(first.Health - (before - 3.2f)) < .02f, "Frost shatter adds sixty percent damage against an already frozen enemy");
                }
                ClearEnemies(); ClearExpansionEffects();
                foreach (var kind in new[] { AbilityKind.Crossbow, AbilityKind.Boomerang, AbilityKind.FrostStaff }) yield return ExpansionWeaponPortrait(kind);
            }
            finally { game.Abilities.enabled = true; ClearEnemies(); ClearExpansionEffects(); }
        }
        IEnumerator ExpansionWeaponPortrait(AbilityKind kind)
        {
            var definition = game.Catalog.Ability(kind);
            var model = game.Pool.Rent(definition.prefabId, game.Player.transform.position + Vector3.up * 2, Quaternion.identity);
            game.Pause("weapon-portrait", true); var camera = Camera.main; var rig = CameraRig.Active;
            Vector3 previous = camera.transform.position; Quaternion rotation = camera.transform.rotation; float zoom = camera.orthographicSize; int mask = camera.cullingMask;
            var parts = model.GetComponentsInChildren<Transform>(true); var layers = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++) { layers[i] = parts[i].gameObject.layer; parts[i].gameObject.layer = 12; }
            rig.enabled = false; game.UI.View.hud.SetActive(false); camera.cullingMask = 1 << 12;
            camera.transform.position = model.transform.position + new Vector3(2.1f, 1.8f, 3); camera.transform.LookAt(model.transform.position); camera.orthographicSize = .95f;
            yield return Capture("weapon-" + definition.id, 1600, 1200);
            for (int i = 0; i < parts.Length; i++) parts[i].gameObject.layer = layers[i];
            camera.transform.SetPositionAndRotation(previous, rotation); camera.orthographicSize = zoom; camera.cullingMask = mask;
            rig.enabled = true; game.UI.View.hud.SetActive(true); game.Pause("weapon-portrait", false); game.Pool.Return(model);
        }
        IEnumerator VerifyExpansionEnemies()
        {
            ClearEnemies(); ClearExpansionEffects(); game.Abilities.enabled = false;
            var biome = game.World.CurrentBiome;
            var types = biome == BiomeId.Swamp ? new[] { 5, 6 } : new[] { 7, 8 };
            try
            {
                foreach (int type in types) for (int elite = 0; elite < 2; elite++)
                {
                    Place(game.World.LocalPosition(Vector2Int.zero) + Vector3.back * 3); ClearEnemies(); ClearExpansionEffects();
                    EnemyActor enemy = null;
                    for (int attempt = 0; attempt < 12 && enemy == null; attempt++) enemy = game.Combat.Spawn(type, game.Player.transform.position + Quaternion.Euler(0, attempt * 30, 0) * Vector3.forward * 8, elite == 1);
                    Check(enemy != null, "Expansion enemy spawns for attack evidence " + type + " / " + elite);
                    if (enemy == null) continue;
                    enemy.DiagnosticImmortal = true;
                    enemy.DiagnosticAttackAfter(.25f);
                    if (type == 7)
                    {
                        enemy.DiagnosticStationary=true;enemy.DiagnosticAttackAfter(20);
                        enemy.transform.rotation = Quaternion.LookRotation(game.Player.transform.position - enemy.transform.position);
                        float health=enemy.Health;enemy.Hit(new DamagePacket{damage=2,source=AbilityKind.Crossbow});
                        Check(Mathf.Abs(enemy.Health-(health-1.3f))<.02f,"Crystal beetle front armor reduces an actual hit by thirty-five percent");
                        enemy.transform.rotation*=Quaternion.Euler(0,180,0);health=enemy.Health;
                        enemy.Hit(new DamagePacket{damage=2,source=AbilityKind.Crossbow});
                        Check(Mathf.Abs(enemy.Health-(health-2))<.02f,"Crystal beetle unarmored rear takes full damage");
                        enemy.DiagnosticStationary=false;enemy.DiagnosticAttackAfter(.1f);
                        yield return AdvanceGameplay(1.2f);health=enemy.Health;
                        enemy.Hit(new DamagePacket{damage=2,source=AbilityKind.Crossbow});
                        Check(Mathf.Abs(enemy.Health-(health-2))<.02f,"Crystal beetle charge exposes it to full damage");
                    }
                    observedExpansionEnemy=enemy;expansionObservedAt=Time.time;nextExpansionObservation=0;observedExpansionHazard=observedPrismShot=false;maxLeapHeight=0;
                    yield return Sequence("enemy-attacks-" + type + (elite == 1 ? "-elite" : "-normal"), 8, false);
                    observedExpansionEnemy=null;
                    Check(type==8?observedPrismShot:observedExpansionHazard,"Expansion attack produces real hazard or prism projectiles "+type+" / "+elite);
                    if(type==6)Check(maxLeapHeight>.7f,"Swamp frog visibly leaves the ground during its real leap "+elite);
                    Check(enemy.IsAlive && enemy.Elite == (elite == 1), "Expansion attack and pooling preserve archetype and elite state " + type + " / " + elite);
                    yield return Capture("enemy-combat-" + type + (elite == 1 ? "-elite" : "-normal"));
                }
                // Metrics and validated task rewards use real kills, then reject duplicate grants.
                foreach (var kind in new[] { AbilityKind.Crossbow, AbilityKind.Boomerang, AbilityKind.FrostStaff })
                {
                    ClearEnemies(); var enemy = game.Combat.Spawn(types[0], game.Player.transform.position + Vector3.forward * 4);
                    if (enemy == null) { Check(false, "Expansion metric target spawns"); continue; }
                    var metric = kind == AbilityKind.Crossbow ? TaskMetric.CrossbowKills : kind == AbilityKind.Boomerang ? TaskMetric.BoomerangKills : TaskMetric.FrostStaffKills;
                    int before = game.Save.Profile.Metric(metric); enemy.Hit(new DamagePacket { damage = 100000, source = kind }); enemy.Hit(new DamagePacket { damage = 100000, source = kind });
                    Check(game.Save.Profile.Metric(metric) == before + 1, "Expansion weapon kill metric records once " + kind);
                }
            }
            finally { game.Abilities.enabled = true; ClearEnemies(); ClearExpansionEffects(); }
        }
    }
}
#endif
