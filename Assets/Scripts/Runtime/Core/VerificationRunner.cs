#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner : MonoBehaviour
    {
        [Serializable] public sealed class Report
        {
            public string unity, gpu, cpu, finished;
            public bool passed;
            public List<string> checks = new List<string>(), errors = new List<string>();
        }
        GameManager game;
        string output;
        readonly Report report = new Report();
        float started;
        IEnumerator Start()
        {
            game = GameManager.Instance;
            if (!game.DiagnosticMode) { Destroy(this); yield break; }
            started = Time.realtimeSinceStartup;
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--evidence");
            output = index >= 0 && index + 1 < args.Length ? args[index + 1] : Path.Combine(Application.persistentDataPath, "Evidence");
            Directory.CreateDirectory(output); Directory.CreateDirectory(Path.Combine(output, "Captures"));
            report.unity = Application.unityVersion; report.gpu = SystemInfo.graphicsDeviceName; report.cpu = SystemInfo.processorType;
            Application.logMessageReceived += OnLog;
            if(Array.IndexOf(args,"--terrain-probe")>=0){yield return TerrainProbe();Finish();yield break;}
            if(Array.IndexOf(args,"--ui-probe")>=0){yield return UIMatrix();Finish();yield break;}
            yield return Wait(3); yield return Capture("01-menu");
            foreach(var character in game.Catalog.characters)
            {MenuStage.Active?.SelectCharacter(character.id);yield return Wait(.3f);game.UI.View.menu.SetActive(false);yield return Capture("character-"+character.id);game.UI.View.menu.SetActive(true);}
            game.UI.ShowSelection(); yield return Wait(.5f); yield return Capture("02-selection"); game.UI.CloseModal();
            game.UI.ShowProgress(); yield return Wait(.5f); yield return Capture("03-progression"); game.UI.CloseModal();
            game.UI.ShowSettings(); yield return Wait(.3f); yield return Capture("04-settings"); game.UI.CloseModal();
            for (int biome = 0; biome < ContentIds.Maps.Length; biome++)
            {
                foreach (var task in game.Catalog.tasks) if (!game.Save.Profile.completedTasks.Contains(task.id)) game.Save.Profile.completedTasks.Add(task.id);
                game.BeginRun(game.Catalog.characters[biome % 3].id, (BiomeId)biome);
                float deadline = Time.realtimeSinceStartup + 70;
                while (!game.Ready && Time.realtimeSinceStartup < deadline) yield return null;
                Check(game.Ready, "Enter biome " + biome);
                if (!game.Ready) { Finish(); yield break; }
                if(biome<3)
                {
                    for(int i=0;i<3;i++){var target=game.Combat.Spawn(i%2,game.Player.transform.position+new Vector3((i-1)*4,0,8));if(target!=null){target.DiagnosticImmortal=true;target.DiagnosticStationary=true;}}
                    yield return RoleActionCloseup(game.Run.character.id);
                    yield return Sequence("role-action-"+game.Run.character.id,5,true);ClearEnemies();
                }
                foreach (var u in game.Catalog.upgrades)
                    if (u.effect == UpgradeEffect.NewAbility && game.Progression.Eligible(u)) game.Progression.GrantForDiagnostic(u.id);
                foreach (var u in game.Catalog.upgrades)
                    if (game.Progression.Eligible(u)) game.Progression.GrantForDiagnostic(u.id);
                game.UI.ShowHUD();
                Check(game.Run.abilities.Count == ContentIds.Weapons.Length, "All nine abilities coexist in biome " + biome);
                if(biome==0)yield return VerifyExperience();
                if(biome==0)yield return VerifyExpansionWeapons();
                if(biome>=5)yield return VerifyExpansionEnemies();
                if(biome==0)yield return EnemyGallery();
                if(biome==0)yield return EffectGallery();
                for (int i = 0; i < 55; i++)
                {
                    float a = i * 2.399f; float r = 9 + i % 9;
                    game.Combat.Spawn(game.Combat.ChooseEnemy(game.World.CurrentBiome), game.Player.transform.position + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * r, i % 15 == 0);
                }
                yield return AutoBattle(5);
                yield return Capture("10-biome-" + biome);
                if (biome == 0)
                {
                    yield return FeedbackEvidence();
                    game.UI.ToggleSkills(); yield return Wait(.4f); yield return Capture("20-skills"); game.UI.CloseModal();
                    game.UI.ToggleMap(); yield return Wait(.4f); yield return Capture("21-map"); game.UI.CloseModal();
                    Check(WorldService.SeedHash(42, -4, 7) == WorldService.SeedHash(42, -4, 7), "Stable world seed hash");
                    game.Run.skillPoints = 0; var damageUpgrade = game.Catalog.Upgrade("damage");
                    Check(!game.Progression.Apply(damageUpgrade.id, true), "Reject unaffordable upgrade");
                    game.Progression.Experience(game.Run.nextExperience);
                    var chosen = game.Progression.Choices(); game.UI.ShowUpgrade(chosen); yield return Wait(.3f); yield return Capture("22-upgrade");
                    if (chosen.Count > 0){game.Progression.Apply(chosen[0].id, false);Check(!game.Progression.Apply(chosen[0].id,false),"Reject duplicate or stale random-upgrade selection");}
                    int points=game.Run.skillPoints;game.Progression.SkipLevel();Check(game.Run.skillPoints==points,"Reject duplicate skip without an offered level");
                }
                yield return Scenarios(biome);
                if(!game.Ready){Check(false,"Run survives world streaming and presentation scenarios "+biome);Finish();yield break;}
                if(biome==4)yield return DefeatEvidence();
                Renderer fadingScenery=null;
                if(biome==0)yield return PrepareOcclusionExit(value=>fadingScenery=value);
                int crystals = game.Save.Profile.crystals;
                string runId = game.Run.id;
                game.FinishRun(true); yield return Wait(.3f); yield return Capture("30-results-" + biome);
                if(biome==0)Check(fadingScenery!=null&&!fadingScenery.HasPropertyBlock(),"Leaving Playing immediately clears an active scenery fade");
                Check(game.Run.settled && !game.Save.Settle(runId, game.Run.seconds), "Idempotent settlement " + biome);
                Check(game.Save.Profile.crystals >= crystals, "Retain earned currency " + biome);
                game.ReturnToMenu(); deadline = Time.realtimeSinceStartup + 40;
                while (game.Flow != GameFlow.Menu && Time.realtimeSinceStartup < deadline) yield return null;
                yield return Wait(1);
                Check(game.Flow == GameFlow.Menu && Mathf.Approximately(Time.timeScale, 1), "Return and clear pause " + biome);
            }
            Finish();
        }
        IEnumerator VerifyExperience()
        {
            foreach(var old in game.GetRunObjects<ExperienceOrb>())game.Pool.Return(old.gameObject);
            Vector3 point=game.Player.transform.position+Vector3.forward*(game.Player.PickupRadius+4)+Vector3.up*.4f;
            // These points are within the physical merge radius but two hash cells apart.
            point.x=Mathf.Floor(point.x/CombatWorld.ExperienceCellSize)*CombatWorld.ExperienceCellSize+1.49f;
            var first=game.Combat.DropExperience(point,7);
            var adjacent=game.Combat.DropExperience(point+Vector3.right*1.52f,11);
            Check(first==adjacent&&first.Amount==18,"Experience merges across neighboring hash-cell boundaries without losing value");
            foreach(var old in game.GetRunObjects<ExperienceOrb>())game.Pool.Return(old.gameObject);
            int expected=0;
            for(int i=0;i<1200;i++)
            {
                int value=1+i%4;expected+=value;
                game.Combat.DropExperience(point+new Vector3(i%18*.47f,0,i/18%18*.47f),value);
            }
            var orbs=game.GetRunObjects<ExperienceOrb>();int total=0;foreach(var orb in orbs)total+=orb.Amount;
            Check(total==expected&&orbs.Length<=64,"Dense experience drops conserve all value with bounded nearby aggregates");
            Vector3 shift=new Vector3(64,0,-96);
            int beforeCount=orbs.Length;Vector3 mergePoint=orbs[0].transform.position;
            game.Combat.ShiftOrigin(shift);game.Combat.DropExperience(mergePoint-shift,17);game.Combat.ShiftOrigin(-shift);
            orbs=game.GetRunObjects<ExperienceOrb>();total=0;foreach(var orb in orbs)total+=orb.Amount;
            Check(total==expected+17&&orbs.Length==beforeCount,"Floating origin rebuild preserves experience merge ownership and exact value");
            yield return Capture("experience-aggregates");
            int previousQuality=QualitySettings.GetQualityLevel();QualitySettings.SetQualityLevel(0,true);
            yield return Capture("experience-aggregates-low");QualitySettings.SetQualityLevel(previousQuality,true);
            foreach(var orb in game.GetRunObjects<ExperienceOrb>())game.Pool.Return(orb.gameObject);
            var collecting=game.Combat.DropExperience(game.Player.transform.position+Vector3.right*(game.Player.PickupRadius-.2f)+Vector3.up*.6f,3);
            yield return null;yield return null;
            Vector3 target=game.Player.transform.position+Vector3.up*.6f;
            collecting.transform.position=target+Vector3.right*(game.Player.PickupRadius+3);
            float previousDistance=Vector3.Distance(collecting.transform.position,target);
            yield return AutoBattle(.2f);
            Check(collecting.isActiveAndEnabled&&Vector3.Distance(collecting.transform.position,target)<previousDistance-.5f,"An attracted experience gem keeps homing after the player moves beyond the initial pickup radius");
            game.Pool.Return(collecting.gameObject);
        }
        IEnumerator AutoBattle(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                if (game.Player != null) game.Player.Heal(100);
                if (game.IsPaused && game.UI.ActiveModal == "upgrade")
                { var options = game.Progression.Choices(); if (options.Count > 0) game.Progression.Apply(options[0].id, false); else game.Progression.SkipLevel(); }
                yield return null;
            }
        }
        static IEnumerator Wait(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
        IEnumerator Capture(string name,int width=1600,int height=900)
        {
            float deadline=Time.realtimeSinceStartup+20;
            while(game.Ready&&!game.IsPaused&&!game.World.ViewReady&&Time.realtimeSinceStartup<deadline)yield return null;
            yield return null;
            RenderCapture(Path.Combine(output,"Captures",name+".png"),width,height,false);
            yield return Wait(.2f);
        }
        void RenderCapture(string path,int width,int height,bool jpeg)
        {
            var camera = Camera.main;
            if (camera == null) { report.errors.Add("Capture camera missing: " + path); return; }
            var canvas = game.UI.View.GetComponent<Canvas>();
            var previousMode = canvas.renderMode; var previousCamera = canvas.worldCamera; float previousDistance = canvas.planeDistance;
            var previousTarget = camera.targetTexture; var previousActive = RenderTexture.active;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            target.antiAliasing=Mathf.Max(1,QualitySettings.antiAliasing);
            target.Create(); camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
            var texture = new Texture2D(width,height,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
            File.WriteAllBytes(path,jpeg?texture.EncodeToJPG(88):texture.EncodeToPNG());
            if(!jpeg)File.WriteAllText(Path.ChangeExtension(path,"quality.json"),"{\"quality\":"+QualitySettings.GetQualityLevel()+",\"qualityName\":\""+QualitySettings.names[QualitySettings.GetQualityLevel()]+"\",\"antiAliasing\":"+target.antiAliasing+",\"width\":"+width+",\"height\":"+height+",\"originalCameraRender\":true}");
            if(auditLayout&&!jpeg)AuditLayout(path,width,height);
            if(!jpeg&&game.Ready)
            {
                var fill=game.UI.View.healthFill;
                File.WriteAllText(Path.ChangeExtension(path,"bars.json"),"{\"health\":"+game.Player.Health.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"maxHealth\":"+game.Player.MaxHealth.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"fillAmount\":"+fill.fillAmount.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"barWidth\":"+fill.rectTransform.rect.width.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}");
            }
            if(!jpeg&&game.World!=null)File.WriteAllText(Path.ChangeExtension(path,"world.json"),game.World.DiagnosticSnapshot());
            canvas.renderMode = previousMode; canvas.worldCamera = previousCamera; canvas.planeDistance = previousDistance;
            camera.targetTexture = previousTarget; RenderTexture.active = previousActive;
            target.Release(); Destroy(target); Destroy(texture); Canvas.ForceUpdateCanvases();
        }
        void Check(bool value, string name) { if (value) report.checks.Add(name); else report.errors.Add("FAIL: " + name); Write(); }
        void OnLog(string message, string stack, LogType type)
        { if ((type == LogType.Exception || type == LogType.Error || type == LogType.Assert)&&report.errors.Count<64) { string detail=message+"\n"+stack;if(!report.errors.Contains(detail))report.errors.Add(detail); Write(); } }
        void Write() { if (output != null) File.WriteAllText(Path.Combine(output, "Verification.json"), JsonUtility.ToJson(report, true)); }
        void Finish()
        {
            report.finished = DateTime.UtcNow.ToString("O"); report.passed = report.errors.Count == 0; Write();
            Application.logMessageReceived -= OnLog; Application.Quit(report.passed ? 0 : 1);
        }
        void Update()
        {
            if(output!=null)ObserveExpansionAttack();
            if (output == null || Time.realtimeSinceStartup - started < 1400) return;
            report.errors.Add("Verification timed out"); Finish();
        }
        void OnDestroy() { Application.logMessageReceived -= OnLog; }
    }
}

#endif
