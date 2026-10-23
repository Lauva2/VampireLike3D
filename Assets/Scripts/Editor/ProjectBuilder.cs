using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace VampireLike.Editor
{
    public static class ProjectBuilder
    {
        static readonly List<PrefabEntry> prefabs = new List<PrefabEntry>();
        public static string Workspace
        {
            get { string p=Directory.GetCurrentDirectory().Replace('\\','/'); return p.EndsWith("/.build/Project") ? p.Substring(0,p.Length-"/.build/Project".Length) : p; }
        }
        [MenuItem("万象远征/生成与更新游戏内容")]
        public static void Generate()
        {
            Directory.CreateDirectory(Path.Combine(Workspace,"Artifacts"));
            prefabs.Clear(); AssetDatabase.Refresh(); ArtFactory.Begin();
            PlayerSettings.companyName="VampireLike";PlayerSettings.productName="万象远征";
            if(string.IsNullOrEmpty(PlayerSettings.bundleVersion))PlayerSettings.bundleVersion="1.0.1";
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.runInBackground=true; PlayerSettings.colorSpace=ColorSpace.Linear;
            PlayerSettings.enableFrameTimingStats=true;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.Mono2x);
            EditorSettings.serializationMode=SerializationMode.ForceText;
            ConfigureLayers();
            for(int i=0;i<3;i++) Add("character/"+new[]{"adventurer","elementalist","swordsman"}[i],ArtFactory.Character(i));
            Add("path/network",ArtFactory.Paths());
            for(int i=0;i<4;i++)
            {
                string id=new[]{"forest","desert","snow","volcano"}[i];
                Add("enemy/"+i,ArtFactory.Enemy(i));Add("boss/"+id,ArtFactory.Enemy(i,true));
                Add("terrain/"+id,ArtFactory.Terrain(i));Add("decoration/"+id,ArtFactory.Decoration(i));
                Add("terrain/"+id+"-wild",ArtFactory.Terrain(i,1));Add("terrain/"+id+"-ridge",ArtFactory.Terrain(i,2));
                Add("architecture/"+id,ArtFactory.Landmark(i));
            }
            foreach(AbilityKind kind in Enum.GetValues(typeof(AbilityKind)))
            {
                if(kind==AbilityKind.All || kind >= AbilityKind.Crossbow)continue; string id=kind.ToString().ToLowerInvariant();
                Add("weapon/"+id,ArtFactory.Weapon(kind));Add("projectile/"+id,ArtFactory.Weapon(kind,true));
            }
            var enemyShot=ArtFactory.Root("EnemyProjectile");
            ArtFactory.Part(enemyShot.transform,"DangerOrb",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.32f,ArtFactory.Hex("E58FA9"));
            ArtFactory.Bake(enemyShot.transform,"enemy-projectile");enemyShot.AddComponent<ProjectileActor>();Add("projectile/enemy",ArtFactory.SavePrefab(enemyShot,"Projectiles","enemy"));
            for(int i=0;i<4;i++) Add("world/"+new[]{"chest","camp","shrine","boss-site"}[i],ArtFactory.Site(i));
            foreach(string id in new[]{"impact","flame","explosion","frost","lightning","rune","roots","dodge","mark","player","warning","warning-line","arc","slash","leafburst","sand","iceburst","embers"})Add("vfx/"+id,ArtFactory.Effect(id));
            Add("pickup/experience",ArtFactory.Experience());
            Add("ui/root",UIFactory.Create());
            foreach (var panel in UIPrefabFactory.Panels) Add(UIView.Address(panel.Key), panel.Value);
            AddExpansionAssets();
            var audio=AudioFactory.Create();var catalog=ContentFactory.Create(prefabs,audio);
            ConfigureAddressables(catalog,audio);CreateScenes();AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Validate();
            File.WriteAllText(Path.Combine(Workspace,"Artifacts/Generation.json"),"{\"generated\":true,\"prefabs\":"+prefabs.Count+",\"upgrades\":"+catalog.upgrades.Length+",\"tasks\":"+catalog.tasks.Length+"}");
            Debug.Log("GAME_GENERATION_COMPLETE");
        }
        public static void ExpandVerifyAndBuild()
        {
            AssetDatabase.Refresh(); ArtFactory.Begin(); prefabs.Clear();
            var existing = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Config/Catalogs/GameCatalog.asset");
            prefabs.AddRange(existing.prefabs);
            var audio = AudioFactory.Create();
            AddExpansionAssets();
            var catalog = ContentFactory.Create(prefabs, audio);
            ConfigureAddressables(catalog, audio); AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            VerifyAndBuild();
        }
        public static void RebuildUIVerifyAndBuild()
        {
            AssetDatabase.Refresh(); ArtFactory.Begin(); prefabs.Clear();
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Config/Catalogs/GameCatalog.asset");
            prefabs.AddRange(catalog.prefabs);
            Add("ui/root", UIFactory.Create());
            foreach (var panel in UIPrefabFactory.Panels) Add(UIView.Address(panel.Key), panel.Value);
            catalog.prefabs = prefabs.ToArray(); EditorUtility.SetDirty(catalog);
            ConfigureAddressables(catalog, new List<AudioEntry>(catalog.audio));
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); VerifyAndBuild();
        }
        static void AddExpansionAssets()
        {
            foreach (var biome in new[] { BiomeId.Swamp, BiomeId.Crystal })
            {
                string id = ContentIds.RegionKey(biome);
                Add("terrain/" + id, ArtFactory.ExpansionTerrain(biome, 0));
                Add("terrain/" + id + "-wild", ArtFactory.ExpansionTerrain(biome, 1));
                Add("terrain/" + id + "-ridge", ArtFactory.ExpansionTerrain(biome, 2));
                Add("decoration/" + id, ArtFactory.ExpansionDecoration(biome));
                Add("architecture/" + id, ArtFactory.ExpansionLandmark(biome));
                Add("boss/" + id, ArtFactory.ExpansionEnemy(biome == BiomeId.Swamp ? 5 : 6, true));
            }
            for (int type = 5; type <= 8; type++) Add("enemy/" + type, ArtFactory.ExpansionEnemy(type));
            foreach (var kind in new[] { AbilityKind.Crossbow, AbilityKind.Boomerang, AbilityKind.FrostStaff })
            {
                string id = kind.ToString().ToLowerInvariant();
                Add("weapon/" + id, ArtFactory.ExpansionWeapon(kind, false));
                Add("projectile/" + id, ArtFactory.ExpansionWeapon(kind, true));
            }
            Add("projectile/prism", ArtFactory.PrismProjectile());
            Add("vfx/spore", ArtFactory.ExpansionEffect("spore")); Add("vfx/crystalburst", ArtFactory.ExpansionEffect("crystalburst"));
            var sanctuary = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Prefabs/World/Landmarks/boss-site.prefab");
            Add("world/boss-site", ArtFactory.ExpansionSanctuary(sanctuary));
        }
        static void Add(string id,GameObject prefab)
        {
            if(prefab==null)throw new InvalidOperationException("Failed prefab "+id);
            prefabs.RemoveAll(entry => entry.id == id);
            prefabs.Add(new PrefabEntry{id=id,reference=new AssetReferenceGameObject(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab)))});
        }
        static void ConfigureLayers()
        {
            var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=tags.FindProperty("layers");
            layers.GetArrayElementAtIndex(8).stringValue="Enemy";layers.GetArrayElementAtIndex(9).stringValue="Player";layers.GetArrayElementAtIndex(10).stringValue="Obstacle";tags.ApplyModifiedProperties();
            Physics.IgnoreLayerCollision(8,9,true);Physics.IgnoreLayerCollision(8,8,true);
        }
        static void ConfigureAddressables(GameCatalog catalog,List<AudioEntry> audio)
        {
            var settings=AddressableAssetSettingsDefaultObject.GetSettings(true);settings.BuildRemoteCatalog=false;
            foreach(var p in prefabs) Register(settings,p.reference.AssetGUID,p.id,p.id.StartsWith("terrain/")||p.id.StartsWith("decoration/")||p.id.StartsWith("architecture/")||p.id.StartsWith("boss/")?"World-"+p.id.Substring(p.id.IndexOf('/')+1):p.id.StartsWith("ui/")?"UI":"Combat");
            foreach(var a in audio)Register(settings,a.reference.AssetGUID,a.id,"Audio");
            Register(settings,AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(catalog)),"config/catalog","Core");
            Register(settings,AssetDatabase.AssetPathToGUID("Assets/Scenes/MainMenu.unity"),"scene/main-menu","Scenes");
            Register(settings,AssetDatabase.AssetPathToGUID("Assets/Scenes/Gameplay.unity"),"scene/gameplay","Scenes");
            EditorUtility.SetDirty(settings);
        }
        static void Register(AddressableAssetSettings settings,string guid,string address,string groupName)
        {
            if(string.IsNullOrEmpty(guid))return;
            var group=settings.FindGroup(groupName)??settings.CreateGroup(groupName,false,false,false,null,typeof(BundledAssetGroupSchema),typeof(ContentUpdateGroupSchema));
            var schema=group.GetSchema<BundledAssetGroupSchema>();schema.BuildPath.SetVariableByName(settings,AddressableAssetSettings.kLocalBuildPath);schema.LoadPath.SetVariableByName(settings,AddressableAssetSettings.kLocalLoadPath);
            var entry=settings.CreateOrMoveEntry(guid,group);entry.address=address;entry.SetLabel("game",true,true);
        }
        static void CreateScenes()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var boot=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var system=new GameObject("GameManager");system.AddComponent<GameManager>();
            var events=new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));events.transform.SetParent(system.transform);
            EditorSceneManager.SaveScene(boot,"Assets/Scenes/Boot.unity");
            foreach(bool menu in new[]{true,false})
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(CameraRig));camera.tag="MainCamera";
                var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=menu?8.6f:13.5f;cam.nearClipPlane=.1f;cam.farClipPlane=180;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=ArtFactory.Hex("DCE8CF");
                var rig=camera.GetComponent<CameraRig>();rig.menu=menu;
                camera.transform.position=menu?new Vector3(10,9,-14):new Vector3(0,22,-16);camera.transform.LookAt(menu?new Vector3(1,1.2f,0):Vector3.zero);
                var sun=new GameObject("Sun",typeof(Light));sun.transform.rotation=Quaternion.Euler(48,-35,0);var light=sun.GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.2f;light.color=ArtFactory.Hex("FFF1D9");light.shadows=LightShadows.Soft;light.shadowStrength=.55f;
                RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=ArtFactory.Hex("C9D9D4");RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=70;RenderSettings.fogEndDistance=140;RenderSettings.fogColor=ArtFactory.Hex("DFE9D4");
                if(menu)new GameObject("MenuStage").AddComponent<MenuStage>();
                EditorSceneManager.SaveScene(scene,"Assets/Scenes/"+(menu?"MainMenu":"Gameplay")+".unity");
            }
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Boot.unity",true)};
            var settings=AddressableAssetSettingsDefaultObject.Settings;
            Register(settings,AssetDatabase.AssetPathToGUID("Assets/Scenes/MainMenu.unity"),"scene/main-menu","Scenes");Register(settings,AssetDatabase.AssetPathToGUID("Assets/Scenes/Gameplay.unity"),"scene/gameplay","Scenes");
            EditorSceneManager.OpenScene("Assets/Scenes/Boot.unity");
        }
        [MenuItem("万象远征/检查内容约束")]
        public static void Validate()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Config/Catalogs/GameCatalog.asset");
            if(catalog==null)throw new InvalidOperationException("Missing catalog");
            var ids=new HashSet<string>();
            foreach(var entry in catalog.prefabs)
            {
                if(!ids.Add(entry.id))throw new InvalidOperationException("Duplicate id "+entry.id);
                string path=AssetDatabase.GUIDToAssetPath(entry.reference.AssetGUID);
                if(!path.StartsWith("Assets/Art/Prefabs/"))throw new InvalidOperationException("Invalid prefab directory "+path);
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(prefab==null)throw new InvalidOperationException("Missing prefab "+path);
                foreach(var t in prefab.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new InvalidOperationException("Missing script "+path);
                // Inspect the serialized dependency, not Graphic.canvasRenderer:
                // that accessor lazily adds a renderer and can hide broken templates.
                foreach(var graphic in prefab.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
                    if(graphic.GetComponent<CanvasRenderer>()==null)throw new InvalidOperationException("Missing serialized CanvasRenderer: "+path+" / "+graphic.name);
            }
            ids.Clear();foreach(var u in catalog.upgrades)if(!ids.Add(u.id))throw new InvalidOperationException("Duplicate upgrade "+u.id);
            Debug.Log("CONTENT_VALIDATION_PASSED "+catalog.prefabs.Length+" prefabs; "+catalog.upgrades.Length+" upgrades");
        }
        [MenuItem("万象远征/构建开发版")]
        public static void BuildDevelopment(){Build(true);}
        public static void VerifyAndBuild(){ProjectChecks.Run();BuildDevelopment();}
        public static void GenerateVerifyAndBuild(){Generate();VerifyAndBuild();}
        [MenuItem("万象远征/构建正式版")]
        public static void BuildRelease(){ProjectChecks.Run();Build(false);}
        [Serializable] sealed class SourceFileIdentity{public string path,sha256;}
        [Serializable] sealed class SourceIdentity{public string unity,version,createdUtc;public bool development;public List<SourceFileIdentity> files=new List<SourceFileIdentity>();}
        static void RecordSourceIdentity(string folder,bool development)
        {
            var identity=new SourceIdentity{unity=Application.unityVersion,version=PlayerSettings.bundleVersion,createdUtc=DateTime.UtcNow.ToString("O"),development=development};
            string project=Directory.GetCurrentDirectory();
            using(var sha=System.Security.Cryptography.SHA256.Create())
            foreach(string root in new[]{"Assets","Packages","ProjectSettings"})
            {
                var files=Directory.GetFiles(Path.Combine(project,root),"*",SearchOption.AllDirectories);Array.Sort(files,StringComparer.Ordinal);
                foreach(string path in files)
                {
                    string relative=path.Substring(project.Length+1).Replace('\\','/');
                    if(relative.StartsWith("Assets/AddressableAssetsData/Windows/")||relative.StartsWith("Assets/AddressableAssetsData/LinkerBuildProcessor/"))continue;
                    identity.files.Add(new SourceFileIdentity{path=relative,sha256=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant()});
                }
            }
            File.WriteAllText(Path.Combine(folder,"SourceIdentity.json"),JsonUtility.ToJson(identity,true));
        }
        static void Build(bool development)
        {
            Validate();AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult content);
            if(!string.IsNullOrEmpty(content.Error))throw new InvalidOperationException(content.Error);
            string folder=Path.Combine(Workspace,"Builds",development?"Development":"Windows");Directory.CreateDirectory(folder);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Boot.unity"},locationPathName=Path.Combine(folder,"WanXiangExpedition.exe"),target=BuildTarget.StandaloneWindows64,options=development?BuildOptions.Development:BuildOptions.None});
            File.WriteAllText(Path.Combine(Workspace,"Artifacts",development?"DevelopmentBuild.txt":"ReleaseBuild.txt"),report.summary.result+"\nBytes: "+report.summary.totalSize+"\nErrors: "+report.summary.totalErrors+"\nSeconds: "+report.summary.totalTime.TotalSeconds);
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Player build failed");
            RecordSourceIdentity(folder,development);
        }
    }
}
