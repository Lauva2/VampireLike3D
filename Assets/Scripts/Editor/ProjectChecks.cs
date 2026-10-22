using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VampireLike.Editor
{
    public static class ProjectChecks
    {
        [Serializable] sealed class Results { public bool passed; public long warmedUpgradeLookupAllocatedBytes; public List<string> checks = new List<string>(); }
        public static void Run()
        {
            var result = new Results();
            Action<bool,string> check = (ok,name) => { if(!ok)throw new InvalidOperationException("CHECK FAILED: "+name);result.checks.Add(name); };
            ProjectBuilder.Validate();
            foreach(var graphicType in new[]{typeof(AbilityIcon),typeof(MapGraphic)})
            {
                var inactiveGraphic=new GameObject("InactiveGraphicDependencyProbe",typeof(RectTransform));
                inactiveGraphic.SetActive(false);
                try
                {
                    inactiveGraphic.AddComponent(graphicType);
                    check(inactiveGraphic.GetComponent<CanvasRenderer>()!=null,"Inactive "+graphicType.Name+" creates its required CanvasRenderer before activation");
                }
                finally{UnityEngine.Object.DestroyImmediate(inactiveGraphic);}
            }
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Config/Catalogs/GameCatalog.asset");
            bool lookupMatches=true;
            foreach(var definition in catalog.upgrades)if(!ReferenceEquals(catalog.Upgrade(definition.id),definition))lookupMatches=false;
            check(lookupMatches&&catalog.Upgrade("missing-upgrade")==null&&catalog.Upgrade(null)==null,"Upgrade lookup preserves every authored definition and rejects unknown IDs");
            var lookupProbe=ScriptableObject.CreateInstance<GameCatalog>();
            try
            {
                var before=new UpgradeDefinition{id="before",value=1};var after=new UpgradeDefinition{id="after",value=2};
                lookupProbe.upgrades=new[]{before};bool first=ReferenceEquals(lookupProbe.Upgrade("before"),before);
                lookupProbe.upgrades=new[]{after};
                check(first&&lookupProbe.Upgrade("before")==null&&ReferenceEquals(lookupProbe.Upgrade("after"),after),"Replacing catalog definitions invalidates earlier upgrade lookup results");
            }
            finally{UnityEngine.Object.DestroyImmediate(lookupProbe);}
            long allocatedBefore=GC.GetAllocatedBytesForCurrentThread();int lookupChecksum=0;
            for(int n=0;n<10000;n++)lookupChecksum+=catalog.Upgrade(catalog.upgrades[n%catalog.upgrades.Length].id).cost;
            result.warmedUpgradeLookupAllocatedBytes=GC.GetAllocatedBytesForCurrentThread()-allocatedBefore;
            check(lookupChecksum>0&&result.warmedUpgradeLookupAllocatedBytes==0,"Warmed upgrade lookup performs ten thousand queries without managed allocations");
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
            bool finiteMeshes=true;int meshCount=0;
            foreach(string guid in AssetDatabase.FindAssets("t:Mesh",new[]{"Assets/Art/Models/Generated"}))
            {
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath(guid));meshCount++;
                foreach(var v in mesh.vertices)if(float.IsNaN(v.x)||float.IsNaN(v.y)||float.IsNaN(v.z)||float.IsInfinity(v.x)||float.IsInfinity(v.y)||float.IsInfinity(v.z)){finiteMeshes=false;Debug.LogError("Invalid generated mesh "+mesh.name);break;}
            }
            check(finiteMeshes&&meshCount>50,"All generated mesh vertices are finite");
            bool collisionMeshes=true;
            foreach(var entry in catalog.prefabs)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(entry.reference.AssetGUID));
                foreach(var collider in prefab.GetComponentsInChildren<MeshCollider>(true))if(collider.sharedMesh==null||!AssetDatabase.Contains(collider.sharedMesh))collisionMeshes=false;
            }
            check(collisionMeshes,"All authored mesh colliders retain saved mesh assets");
            var pickupMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Generated/CartoonPickup.mat");
            check(pickupMaterial!=null&&pickupMaterial.enableInstancing&&pickupMaterial.renderQueue<=2500&&pickupMaterial.GetFloat("_ZWrite")==1&&pickupMaterial.GetFloat("_SrcBlend")==1&&pickupMaterial.GetFloat("_DstBlend")==0&&pickupMaterial.GetFloat("_AlphaCoverage")==1,"Experience gems use instanced depth-writing coverage instead of transparent sorting");
            var dune=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Models/Generated/sand-dune.asset");check(dune!=null&&dune.bounds.size.y>.8f,"Generated slope has valid nonzero relief");
            bool network=true,anchors=true;
            for(int x=-16;x<=16;x++)for(int z=-16;z<=16;z++)
            {
                var key2=new Vector2Int(x,z);var parent=PathNetwork.Parent(71,key2);
                if(key2!=Vector2Int.zero&&Mathf.Abs(parent.x)+Mathf.Abs(parent.y)>=Mathf.Abs(x)+Mathf.Abs(z))network=false;
                foreach(var d in new[]{Vector2Int.right,Vector2Int.up})
                {
                    if(PathNetwork.Connected(71,key2,key2+d)!=PathNetwork.Connected(71,key2+d,key2))network=false;
                    Vector3 a=PathNetwork.EdgeAnchor(71,key2,d),b=PathNetwork.EdgeAnchor(71,key2+d,-d)+new Vector3(d.x*32,0,d.y*32);
                    if((a-b).sqrMagnitude>.000001f)anchors=false;
                }
            }
            check(network,"Road graph shares undirected edges and every node has a finite route to origin");
            check(anchors,"Curved road endpoints match across positive and negative chunk boundaries");
            var roadObject=new GameObject("Road seam verification");
            try
            {
                var road=roadObject.AddComponent<PathNetwork>();road.SendMessage("Awake");bool flushEdges=true;
                foreach(int x in new[]{-9,-3,0,2,7})
                {
                    road.Configure(71,new Vector2Int(x,2),4,BiomeId.Forest,new[]{BiomeId.Forest,BiomeId.Forest,BiomeId.Forest,BiomeId.Forest},true,true,true,false);
                    int edgeVertices=0;
                    foreach(var vertex in roadObject.GetComponent<MeshFilter>().sharedMesh.vertices)
                    {
                        if(vertex.z>16.0001f)flushEdges=false;
                        if(Mathf.Abs(vertex.z-16)<.0001f)edgeVertices++;
                    }
                    if(edgeVertices<12)flushEdges=false;
                }
                check(flushEdges,"River crossing road spans meet the north chunk edge without angled overhangs");
                bool clearing=true;
                foreach(bool river in new[]{false,true})
                {
                    road.Configure(71,new Vector2Int(0,2),15,BiomeId.Forest,new[]{BiomeId.Forest,BiomeId.Forest,BiomeId.Forest,BiomeId.Forest},river,true,true,true,true);
                    if(road.NearPath(Vector3.zero,2.5f))clearing=false;
                }
                check(clearing,"Challenge roads preserve the central ritual clearing with and without a river");
            }
            finally{UnityEngine.Object.DestroyImmediate(roadObject);}
            var waterTest=new GameObject("Rotated water footprint verification");
            try
            {
                var appearance=waterTest.AddComponent<TerrainAppearance>();appearance.landform=waterTest.transform;appearance.waterCenter=new Vector2(-9,8);appearance.waterRadii=new Vector2(3.5f,2.5f);
                bool footprints=true;
                foreach(int quarter in new[]{0,1,2,3})
                {
                    appearance.Orient(quarter);
                    if(!appearance.ContainsWater(waterTest.transform.TransformPoint(new Vector3(-9,0,8)),4)||appearance.ContainsWater(waterTest.transform.TransformPoint(new Vector3(9,0,-8)),4))footprints=false;
                }
                check(footprints,"Water placement exclusion follows rotated oasis and lake footprints");
            }
            finally{UnityEngine.Object.DestroyImmediate(waterTest);}
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
