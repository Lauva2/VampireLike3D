using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace VampireLike
{
    public enum GameFlow { Boot, Menu, Preparing, Playing, Settling, Error }

    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameFlow Flow { get; private set; }
        public AssetService Assets { get; private set; }
        public SaveService Save { get; private set; }
        public InputService Inputs { get; private set; }
        public UIManager UI { get; private set; }
        public GameCatalog Catalog => Assets?.Catalog;
        public AssetScope RunAssets => runScope;
        public RunState Run { get; private set; }
        public PlayerController Player { get; private set; }
        public WorldService World { get; private set; }
        public CombatWorld Combat { get; private set; }
        public AbilityController Abilities { get; private set; }
        public ProgressionService Progression { get; private set; }
        public PoolService Pool { get; private set; }
        public AudioService Audio { get; private set; }
        public bool IsPaused => pauses.Count != 0;
        public bool Ready => Flow == GameFlow.Playing && Player != null;
        public bool CanBeginRun => Flow == GameFlow.Menu && !transition;
        public bool DiagnosticMode { get; private set; }
        public string Error { get; private set; }
        public T[] GetRunObjects<T>() where T : Component => runRoot != null ? runRoot.GetComponentsInChildren<T>() : Array.Empty<T>();
        readonly HashSet<string> pauses = new HashSet<string>();
        AssetScope appScope, runScope;
        AsyncOperationHandle<SceneInstance> activeScene;
        GameObject runRoot;
        bool transition;
        float nextDiagnosticStatus;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            Inputs = new InputService(); Assets = new AssetService();
            DiagnosticMode = Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(), "--verify") >= 0;
            var args=Environment.GetCommandLineArgs();int evidence=Array.IndexOf(args,"--evidence");
            Inputs.HardwareEnabled=!DiagnosticMode||Array.IndexOf(args,"--manual")>=0;
            string path = DiagnosticMode ? System.IO.Path.Combine(evidence>=0&&evidence+1<args.Length?args[evidence+1]:Application.persistentDataPath, "verification-profile.json") :
                Array.IndexOf(args,"--portable")>=0?System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath),"Saves","profile-v1.json"):null;
            Save = new SaveService(path);
            UI = gameObject.AddComponent<UIManager>();
            Audio = gameObject.AddComponent<AudioService>();
        }
        IEnumerator Start()
        {
            yield return Assets.Initialize();
            if (Catalog == null) { Fail(Assets.LastError); yield break; }
            appScope = new AssetScope("application-ui");
            yield return UI.Initialize(this, appScope);
            if (!UI.Ready) { Fail("界面加载失败，请重新启动或检查内容文件。"); yield break; }
            yield return Audio.Initialize(this);
            UI.RestoreSettings();
            yield return ShowMenu();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (DiagnosticMode)
            {
                if(Array.IndexOf(Environment.GetCommandLineArgs(),"--manual")>=0)yield break;
                if(Array.IndexOf(Environment.GetCommandLineArgs(),"--campaign")>=0)gameObject.AddComponent<CampaignRunner>();
                else if(Array.IndexOf(Environment.GetCommandLineArgs(),"--soak")>=0)gameObject.AddComponent<SoakRunner>();
                else gameObject.AddComponent<VerificationRunner>();
            }
#endif
        }
        void Update()
        {
            Save?.Tick();
            if(DiagnosticMode&&Time.unscaledTime>nextDiagnosticStatus){nextDiagnosticStatus=Time.unscaledTime+5;WriteDiagnosticStatus();}
            if (!Ready) return;
            if (Inputs.HardwareEnabled&&Input.GetKeyDown(KeyCode.Escape)) UI.TogglePause();
            if (Inputs.HardwareEnabled&&Input.GetKeyDown(KeyCode.Tab)) UI.ToggleSkills();
            if (Inputs.HardwareEnabled&&Input.GetKeyDown(KeyCode.M)) UI.ToggleMap();
            Inputs.Blocked = IsPaused || UI.BlocksGameplay;
            if (IsPaused) return;
            Run.seconds += Time.deltaTime;
            Save.Profile.bestSeconds = Mathf.Max(Save.Profile.bestSeconds, Run.seconds);
            Progression.Tick();
            if (Player.Health <= 0) FinishRun(false);
        }
        public void Pause(string reason, bool value)
        {
            if (value) pauses.Add(reason); else pauses.Remove(reason);
            Time.timeScale = pauses.Count > 0 ? 0 : 1;
            Inputs.Blocked = pauses.Count > 0;
        }
        public void BeginRun(string characterId, BiomeId biome)
        {
            if (transition || Flow != GameFlow.Menu) return;
            var character = Catalog.Character(characterId);
            if (!Save.TaskUnlocked(character.taskId) || !Save.MapUnlocked(biome)) { UI.Toast("完成对应任务后解锁。"); return; }
            StartCoroutine(PrepareRun(character, biome));
        }
        IEnumerator PrepareRun(CharacterDefinition character, BiomeId biome)
        {
            transition = true; Flow = GameFlow.Preparing; ClearPause(); UI.ShowLoading("正在展开新的世界…");
            yield return Assets.UnloadScene(activeScene);
            activeScene = default;
            yield return Assets.LoadScene("scene/gameplay", h => activeScene = h);
            if (!activeScene.IsValid()) { yield return Recover("战场加载失败"); yield break; }
            Run = new RunState { seed = UnityEngine.Random.Range(1000, int.MaxValue), biome = biome, character = character };
            Run.abilities.Add(character.startingAbility);
            runScope = new AssetScope("run-" + Run.id);
            runRoot = new GameObject("RunSession");
            Pool = new PoolService(runRoot.transform);
            int loaded = 0;
            foreach (var entry in Catalog.prefabs)
            {
                if (entry.id.StartsWith("terrain/") || entry.id.StartsWith("path/") || entry.id.StartsWith("decoration/") || entry.id.StartsWith("architecture/") || entry.id.StartsWith("boss/") || entry.id.StartsWith("ui/")) continue;
                if (entry.id.StartsWith("character/") && entry.id != character.prefabId) continue;
                bool missing=false;
                yield return Assets.LoadPrefab(entry.id, runScope, p => {missing=p==null;Pool.Register(entry.id, p);});
                if(missing){yield return Recover("内容加载失败: "+entry.id);yield break;}
                UI.LoadingProgress(++loaded / (float)Catalog.prefabs.Length);
            }
            var playerObject = Pool.Rent(character.prefabId, Vector3.zero, Quaternion.Euler(0, 180, 0));
            if (playerObject == null) { yield return Recover("角色资源加载失败"); yield break; }
            Player = playerObject.GetComponent<PlayerController>(); Player.Initialize(this);
            Combat = runRoot.AddComponent<CombatWorld>(); Combat.Initialize(this);
            Abilities = playerObject.GetComponent<AbilityController>(); Abilities.Initialize(this);
            World = runRoot.AddComponent<WorldService>();
            yield return World.Initialize(this);
            if(!string.IsNullOrEmpty(World.LoadingError)){yield return Recover(World.LoadingError);yield break;}
            Progression = new ProgressionService(this);
            CameraRig.Active?.SetTarget(Player.transform);
            Flow = GameFlow.Playing; transition = false; UI.ShowHUD();
            UI.Toast("探索地标，积累力量。Boss 由你主动挑战。", 5);
            Audio.SetBiome(World.CurrentBiome);
        }
        IEnumerator Recover(string message)
        {
            Debug.LogWarning("Run recovery: " + message);
            CleanupRun();
            yield return null;
            Assets.Release(runScope); runScope = null;
            if (activeScene.IsValid()) yield return Assets.UnloadScene(activeScene);
            transition = false; yield return ShowMenu(); UI.Toast(message + "，已返回主菜单。", 6);
        }
        public void AbortToMenu(string message){if(transition)return;transition=true;StartCoroutine(Recover(message));}
        public void FinishRun(bool extraction)
        {
            if (!Ready || transition || Run.settled) return;
            transition = true; Flow = GameFlow.Settling; Run.settled = true;
            Pause("settlement", true);
            Save.Settle(Run.id, Run.seconds);
            if(extraction){UI.ShowResults(true);transition=false;}
            else StartCoroutine(DefeatThenResults());
        }
        IEnumerator DefeatThenResults()
        {
            UI.View.hud.SetActive(false);yield return Player.PlayDefeat();UI.ShowResults(false);transition=false;
        }
        public void ReturnToMenu()
        {
            if (transition) return;
            StartCoroutine(ReturnRoutine());
        }
        IEnumerator ReturnRoutine()
        {
            transition = true; UI.ShowLoading("收好行囊，返回营地…"); ClearPause();
            CleanupRun();
            yield return null;
            if (runScope != null) { Assets.Release(runScope); runScope = null; }
            yield return Assets.UnloadScene(activeScene); activeScene = default;
            yield return ShowMenu(); transition = false;
        }
        IEnumerator ShowMenu()
        {
            Flow = GameFlow.Menu;
            yield return Assets.LoadScene("scene/main-menu", h => activeScene = h);
            if (!activeScene.IsValid()) { Fail("主菜单场景加载失败。"); yield break; }
            UI.ShowMenu();
            Debug.Log("MAIN_MENU_READY version="+Application.version+" development="+Debug.isDebugBuild+" save="+Save.Path);
            if (!string.IsNullOrEmpty(Save.RecoveryMessage)) UI.Toast(Save.RecoveryMessage, 6);
        }
        void CleanupRun()
        {
            World?.Dispose(); Combat?.Dispose(); Abilities?.Dispose();
            Pool?.Clear();
            if (runRoot != null) Destroy(runRoot);
            Player = null; World = null; Combat = null; Abilities = null; Progression = null; Pool = null;
            Save.Flush();
        }
        void ClearPause() { pauses.Clear(); Time.timeScale = 1; Inputs.Blocked = false; }
        [Serializable] sealed class DiagnosticState
        {
            public string flow,modal,pauses;public bool transition,ready;public float seconds,health,timeScale,x,z;public int level,chunks,enemies,prefabs,pool;public string[] choices;
        }
        void WriteDiagnosticStatus()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--evidence");if(index<0||index+1>=args.Length)return;
            var choices=new List<string>();if(UI!=null&&UI.Ready)foreach(Transform t in UI.View.upgradeList){var widget=t.GetComponent<UIWidget>();if(widget!=null&&widget.gameObject.activeInHierarchy)choices.Add(widget.title.text+":"+widget.button.interactable);}
            var state=new DiagnosticState{flow=Flow.ToString(),modal=UI?.ActiveModal,pauses=string.Join(",",pauses),transition=transition,ready=Ready,seconds=Run?.seconds??0,health=Player!=null?Player.Health:0,timeScale=Time.timeScale,level=Run?.level??0,chunks=World!=null?World.ActiveChunks:0,enemies=Combat!=null?Combat.Enemies.Count:0,prefabs=Assets?.LoadedPrefabCount??0,pool=Pool?.Count??0,choices=choices.ToArray()};
            if(World!=null){state.x=World.GlobalPosition.x;state.z=World.GlobalPosition.y;}
            try{System.IO.Directory.CreateDirectory(args[index+1]);System.IO.File.WriteAllText(System.IO.Path.Combine(args[index+1],"RuntimeStatus.json"),JsonUtility.ToJson(state,true));}catch(System.IO.IOException ex){Debug.LogWarning(ex.Message);}
        }
        void Fail(string message) { Error = message; Flow = GameFlow.Error; Debug.LogError(message); }
        void OnGUI()
        {
            if (Flow != GameFlow.Error && Flow != GameFlow.Boot) return;
            GUI.color = Color.white;
            GUI.Label(new Rect(32, 32, Screen.width - 64, 100), Flow == GameFlow.Error ? Error : "Loading…");
        }
        void OnApplicationPause(bool paused) { if (paused) { Save?.MarkDirty(); Save?.Flush(); } }
        void OnApplicationQuit() { Save?.MarkDirty(); Save?.Flush(); }
        void OnDestroy()
        {
            if (Instance != this) return;
            ClearPause(); CleanupRun();
            Assets?.Release(runScope); Assets?.Release(appScope); Assets?.Dispose(); Instance = null;
        }
    }
}
