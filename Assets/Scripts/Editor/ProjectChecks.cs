using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VampireLike.Editor
{
    public static class ProjectChecks
    {
        [Serializable] sealed class Results { public bool passed; public List<string> checks = new List<string>(); }
        public static void Run()
        {
            var result = new Results();
            Action<bool,string> check = (ok,name) => { if(!ok)throw new InvalidOperationException("CHECK FAILED: "+name);result.checks.Add(name); };
            ProjectBuilder.Validate();
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Config/Catalogs/GameCatalog.asset");
            check(catalog.characters.Length==3,"Three characters");check(catalog.biomes.Length==4,"Four biomes");check(catalog.abilities.Length==6,"Six abilities");check(catalog.upgrades.Length>=30,"At least thirty upgrades");
            var tasks=new HashSet<string>();foreach(var task in catalog.tasks)check(tasks.Add(task.id),"Unique task "+task.id);
            foreach(var upgrade in catalog.upgrades)check(string.IsNullOrEmpty(upgrade.taskId)||tasks.Contains(upgrade.taskId),"Reachable upgrade reference "+upgrade.id);
            foreach(var task in catalog.tasks)
            {
                bool exists=task.rewardKind==UnlockKind.Character?Array.Exists(catalog.characters,c=>c.id==task.rewardId):catalog.Upgrade(task.rewardId)!=null;
                check(exists,"Reward exists "+task.id);
            }
            var run=new RunState();run.upgrades["damage"]=3;run.upgrades["swordcount"]=4;
            check(Mathf.Approximately(run.Effect(catalog,UpgradeEffect.Damage,AbilityKind.Gun),.36f),"Generic modifier stacks");
            check(Mathf.Approximately(run.Effect(catalog,UpgradeEffect.Count,AbilityKind.Gun),0),"Specific modifier does not leak");
            check(Mathf.Approximately(run.Effect(catalog,UpgradeEffect.Count,AbilityKind.Sword),4),"Sword modifier applies");
            run.upgrades["orbit"]=1;run.upgrades["rain"]=1;check(run.Level("orbit")==1&&run.Level("rain")==1,"Parallel routes coexist");
            check(WorldService.SeedHash(13,-99,45)==WorldService.SeedHash(13,-99,45),"Stable chunk hash");
            check(WorldService.SeedHash(13,-99,45)!=WorldService.SeedHash(13,-98,45),"Distinct chunk coordinates");
            string dir=Path.Combine(ProjectBuilder.Workspace,"Artifacts/Tests");Directory.CreateDirectory(dir);string path=Path.Combine(dir,"profile-"+Guid.NewGuid().ToString("N")+".json");
            var save=new SaveService(path);save.Profile.crystals=42;save.MarkDirty();save.Flush();
            check(new SaveService(path).Profile.crystals==42,"Save roundtrip");
            save.Profile.crystals=90;save.MarkDirty();save.Flush();File.WriteAllText(path,"{broken");
            var recovered=new SaveService(path);check(recovered.Profile.crystals==42,"Corruption restores backup");
            recovered.MarkDirty();recovered.Flush();check(new SaveService(path+".bak").Profile.crystals==42,"Recovery preserves valid backup");
            check(recovered.Settle("run-a",30),"Initial settlement");check(!recovered.Settle("run-a",30),"Duplicate settlement rejected");
            check(recovered.MapUnlocked(BiomeId.Forest)&&!recovered.MapUnlocked(BiomeId.Desert),"Initial biome gate");
            recovered.Profile.bossWins[0]=true;check(recovered.MapUnlocked(BiomeId.Desert)&&!recovered.MapUnlocked(BiomeId.Mixed),"Sequential biome gate");
            for(int i=0;i<4;i++)recovered.Profile.bossWins[i]=true;check(recovered.MapUnlocked(BiomeId.Mixed),"All-boss mixed gate");
            var journal=new WorldJournal("test-"+Guid.NewGuid().ToString("N"));var key=new Vector2Int(-2,5);journal.Load(key);journal.Complete(key,"chest");journal.Unload(key);
            check(journal.Load(key).completed.Contains("chest"),"World event survives chunk eviction");journal.Dispose();
            result.passed=true;File.WriteAllText(Path.Combine(ProjectBuilder.Workspace,"Artifacts/EditorChecks.json"),JsonUtility.ToJson(result,true));Debug.Log("EDITOR_CHECKS_PASSED "+result.checks.Count);
        }
    }
}
