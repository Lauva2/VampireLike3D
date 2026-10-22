#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;

namespace VampireLike
{
    public sealed class SoakRunner : MonoBehaviour
    {
        [Serializable] sealed class UpgradeSample { public string id; public int level; }
        [Serializable] sealed class BuildSample
        {
            public float wallSeconds,gameSeconds,pickupRadius;
            public int seed,level,quality,frameRateLimit;
            public bool fixedImportedBuild;
            public List<AbilityKind> abilities;
            public List<UpgradeSample> upgrades=new List<UpgradeSample>();
        }
        GameManager game;
        string output;
        float begin, nextSample, nextDodge, nextDense;
        Camera camera;
        RenderTexture target;
        readonly List<float> frames=new List<float>(2400);
        readonly List<string> errors=new List<string>();
        readonly FrameTiming[] timing=new FrameTiming[1];
        int samples, maxChunks, maxPool, maxPrefabs;
        double seconds=7200;
        int frameRateLimit=60;
        bool measuring;
        bool manualRender;
        bool fixedImportedBuild;
        ProfilerRecorder drawCalls,triangles;
        int renderedFrames;
        float avoidUntil;Vector2 avoidDirection;
        Vector2 lastTravelPosition;float lastTravelProgress;int escapeAttempt;
        Vector2 TravelInput()
        {
            if((Time.realtimeSinceStartup-begin)%60>=42)return Vector2.zero;
            Vector2 position=game.World.GlobalPosition;
            if((position-lastTravelPosition).sqrMagnitude>.56f){lastTravelPosition=position;lastTravelProgress=Time.time;}
            if(Time.time<avoidUntil)return avoidDirection;
            if(Time.time-lastTravelProgress>2)
            {
                avoidDirection=new Vector2(escapeAttempt++/3%2==0?.85f:-.85f,-.3f);avoidUntil=Time.time+2.5f;lastTravelProgress=Time.time;return avoidDirection;
            }
            if(Physics.SphereCast(game.Player.transform.position+Vector3.up*.8f,.45f,Vector3.forward,out var hit,2.5f,(1<<0)|(1<<10)))
            {
                Vector3 side=Vector3.Cross(hit.normal,Vector3.up).normalized;if(side.sqrMagnitude<.1f)side=Vector3.right;
                avoidDirection=new Vector2(side.x,side.z)*.8f+Vector2.up*.25f;avoidUntil=Time.time+1.4f;return avoidDirection;
            }
            return Vector2.up*.8f;
        }
        IEnumerator Start()
        {
            game=GameManager.Instance;if(!game.DiagnosticMode){Destroy(this);yield break;}
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--evidence");output=i>=0&&i+1<args.Length?args[i+1]:Path.Combine(Application.persistentDataPath,"Soak");Directory.CreateDirectory(output);
            i=Array.IndexOf(args,"--soak-seconds");if(i>=0&&i+1<args.Length)double.TryParse(args[i+1],out seconds);
            Application.logMessageReceived+=OnLog;
            i=Array.IndexOf(args,"--soak-frame-rate");
            if(i>=0&&(i+1>=args.Length||!int.TryParse(args[i+1],out frameRateLimit)||(frameRateLimit!=-1&&frameRateLimit!=60))){File.WriteAllText(Path.Combine(output,"Errors.txt"),"Invalid diagnostic frame rate limit; use -1 or 60");Application.Quit(1);yield break;}
            foreach(var task in game.Catalog.tasks)game.Save.Profile.completedTasks.Add(task.id);
            for(i=0;i<4;i++)game.Save.Profile.bossWins[i]=true;
            game.BeginRun("adventurer",BiomeId.Mixed);
            while(!game.Ready)yield return null;
            foreach(var u in game.Catalog.upgrades)if(u.effect==UpgradeEffect.NewAbility&&game.Progression.Eligible(u))game.Progression.GrantForDiagnostic(u.id);
            foreach(var u in game.Catalog.upgrades)if(game.Progression.Eligible(u))game.Progression.GrantForDiagnostic(u.id);
            i=Array.IndexOf(args,"--soak-upgrades");
            if(i>=0&&(i+1>=args.Length||!ImportUpgradeBuild(args[i+1]))){File.WriteAllLines(Path.Combine(output,"Errors.txt"),errors);Application.Quit(1);yield break;}
            game.UI.ShowHUD();
            if(!fixedImportedBuild)game.Run.seconds=1800;
            camera=Camera.main;manualRender=Application.isBatchMode;target=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32);target.antiAliasing=4;target.Create();
            QualitySettings.vSyncCount=0;Application.targetFrameRate=frameRateLimit;
            drawCalls=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count");triangles=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Triangles Count");Camera.onPostRender+=OnRendered;
            var canvas=game.UI.View.GetComponent<Canvas>();
            if(manualRender){camera.enabled=false;camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
            File.WriteAllText(Path.Combine(output,"Samples.csv"),"wallSeconds,gameSeconds,fps,p95FrameMs,maxFrameMs,unityAllocatedMB,unityReservedMB,managedMB,privateMB,enemies,poolObjects,chunks,prefabHandles,cpuFrameMs,gpuFrameMs,originX,originZ,biome,cameraRenders,drawCalls,triangles,experienceOrbs,unclaimedExperience,projectiles\n");
            begin=Time.realtimeSinceStartup;nextSample=begin+30;measuring=true;
            while(Time.realtimeSinceStartup-begin<seconds&&errors.Count==0)
            {
                if(!game.Ready){errors.Add("Stress run left Playing unexpectedly; see Player log for recovery cause");break;}
                game.Player.Heal(100000);
                if(game.UI.View.upgrade.activeSelf){var choices=game.Progression.Choices();if(!fixedImportedBuild&&choices.Count>0)game.Progression.Apply(choices[0].id,false);else game.Progression.SkipLevel();}
                if(game.UI.View.victory.activeSelf)game.UI.CloseModal();
                Vector2 travel=TravelInput();if(Time.time>nextDodge){nextDodge=Time.time+2.4f;game.Inputs.ForVerification(travel,true);}
                else game.Inputs.ForVerification(travel);
                if(Time.time>nextDense&&game.Combat.Enemies.Count<300)
                {
                    nextDense=Time.time+15;
                    for(int n=0;n<45;n++){float a=n*2.399f;game.Combat.Spawn(n%4,game.Player.transform.position+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*UnityEngine.Random.Range(10,22),n%11==0);}
                }
                frames.Add(Time.unscaledDeltaTime*1000);FrameTimingManager.CaptureFrameTimings();
                maxChunks=Mathf.Max(maxChunks,game.World.ActiveChunks);maxPool=Mathf.Max(maxPool,game.Pool.Count);maxPrefabs=Mathf.Max(maxPrefabs,game.Assets.LoadedPrefabCount);
                if(Time.realtimeSinceStartup>nextSample){Sample();nextSample+=30;}
                yield return null;
            }
            Sample();measuring=false;
            if(!manualRender){camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();camera.Render();}
            var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,"FinalFrame.png"),image.EncodeToPNG());RenderTexture.active=null;Destroy(image);
            File.WriteAllText(Path.Combine(output,"Summary.json"),"{\"finishedUtc\":\""+DateTime.UtcNow.ToString("O")+"\",\"wallSeconds\":"+(Time.realtimeSinceStartup-begin).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\"requestedSeconds\":"+seconds+",\"frameRateLimit\":"+frameRateLimit+",\"fixedImportedBuild\":"+(fixedImportedBuild?"true":"false")+",\"renderResolution\":\"1920x1080 4x MSAA\",\"renderEveryFrame\":true,\"controlledHealingAndUnlockedBuild\":true,\"samples\":"+samples+",\"maxChunks\":"+maxChunks+",\"maxPool\":"+maxPool+",\"maxPrefabHandles\":"+maxPrefabs+",\"errorCount\":"+errors.Count+"}");
            File.WriteAllLines(Path.Combine(output,"Errors.txt"),errors);
            game.FinishRun(true);game.Save.Flush();Application.Quit(errors.Count==0?0:1);
        }
        bool ImportUpgradeBuild(string path)
        {
            try
            {
                var build=JsonUtility.FromJson<BuildSample>(File.ReadAllText(path));
                if(build==null||build.level<1||float.IsNaN(build.gameSeconds)||float.IsInfinity(build.gameSeconds)||build.gameSeconds<0||build.quality<0||build.quality>=QualitySettings.names.Length)throw new InvalidDataException("Invalid build metadata");
                var abilities=new HashSet<AbilityKind>(build.abilities);
                for(int kind=0;kind<6;kind++)if(!abilities.Contains((AbilityKind)kind))throw new InvalidDataException("Stress snapshot must retain all six abilities");
                if(abilities.Count!=6)throw new InvalidDataException("Unknown ability in snapshot");
                var levels=new Dictionary<string,int>();
                foreach(var upgrade in build.upgrades)
                {
                    if(game.Catalog.Upgrade(upgrade.id)==null||upgrade.level<1||levels.ContainsKey(upgrade.id))throw new InvalidDataException("Invalid or duplicate upgrade in snapshot");
                    levels.Add(upgrade.id,upgrade.level);
                }
                game.Run.upgrades.Clear();foreach(var entry in levels)game.Run.upgrades.Add(entry.Key,entry.Value);
                game.Run.abilities.Clear();foreach(var ability in abilities)game.Run.abilities.Add(ability);
                game.Run.level=build.level;game.Run.seconds=build.gameSeconds;game.Run.experience=0;game.Run.nextExperience=int.MaxValue;
                game.Player.RefreshStats(true);game.Abilities.Refresh();
                if(!Mathf.Approximately(game.Player.PickupRadius,build.pickupRadius))throw new InvalidDataException("Imported pickup radius does not match refreshed player stats");
                QualitySettings.SetQualityLevel(build.quality,true);fixedImportedBuild=true;return true;
            }
            catch(Exception exception){errors.Add("Could not import fixed diagnostic upgrades: "+exception.Message);return false;}
        }
        void LateUpdate(){if(measuring&&manualRender&&camera!=null){Canvas.ForceUpdateCanvases();camera.Render();}}
        void Sample()
        {
            if(frames.Count==0)return;frames.Sort();float sum=0;foreach(float ms in frames)sum+=ms;
            uint count=FrameTimingManager.GetLatestTimings(1,timing);
            string cpu=count>0?timing[0].cpuFrameTime.ToString("F3",System.Globalization.CultureInfo.InvariantCulture):"NA";
            string gpu=count>0&&timing[0].gpuFrameTime>0?timing[0].gpuFrameTime.ToString("F3",System.Globalization.CultureInfo.InvariantCulture):"NA";
            long privateBytes=System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64;
            string row=string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0:F1},{1:F1},{2:F2},{3:F2},{4:F2},{5:F2},{6:F2},{7:F2},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20}\n",Time.realtimeSinceStartup-begin,game.Run.seconds,frames.Count*1000/sum,frames[Mathf.Min(frames.Count-1,(int)(frames.Count*.95f))],frames[frames.Count-1],Profiler.GetTotalAllocatedMemoryLong()/1048576.0,Profiler.GetTotalReservedMemoryLong()/1048576.0,GC.GetTotalMemory(false)/1048576.0,privateBytes>0?(privateBytes/1048576.0).ToString("F2",System.Globalization.CultureInfo.InvariantCulture):"NA",game.Combat.Enemies.Count,game.Pool.Count,game.World.ActiveChunks,game.Assets.LoadedPrefabCount,cpu,gpu,game.World.OriginChunk.x,game.World.OriginChunk.y,game.World.CurrentBiome,renderedFrames,drawCalls.Valid?drawCalls.LastValue:-1,triangles.Valid?triangles.LastValue:-1);
            var orbs=game.GetRunObjects<ExperienceOrb>();long experience=0;foreach(var orb in orbs)experience+=orb.Amount;
            row=row.TrimEnd('\n')+","+orbs.Length+","+experience+","+game.GetRunObjects<ProjectileActor>().Length+"\n";
            File.AppendAllText(Path.Combine(output,"Samples.csv"),row);samples++;frames.Clear();renderedFrames=0;
            var build=new BuildSample{wallSeconds=Time.realtimeSinceStartup-begin,gameSeconds=game.Run.seconds,seed=game.Run.seed,level=game.Run.level,quality=QualitySettings.GetQualityLevel(),frameRateLimit=frameRateLimit,fixedImportedBuild=fixedImportedBuild,pickupRadius=game.Player.PickupRadius,abilities=new List<AbilityKind>(game.Run.abilities)};
            foreach(var entry in game.Run.upgrades)build.upgrades.Add(new UpgradeSample{id=entry.Key,level=entry.Value});
            build.upgrades.Sort((a,b)=>string.CompareOrdinal(a.id,b.id));
            string snapshot=JsonUtility.ToJson(build);File.WriteAllText(Path.Combine(output,"CurrentBuild.json"),snapshot);File.AppendAllText(Path.Combine(output,"BuildSamples.jsonl"),snapshot+"\n");
        }
        void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){string detail=message+"\n"+stack;if(!errors.Contains(detail))errors.Add(detail);}}
        void OnRendered(Camera rendered){if(measuring&&rendered==camera)renderedFrames++;}
        void OnDestroy(){Application.logMessageReceived-=OnLog;Camera.onPostRender-=OnRendered;drawCalls.Dispose();triangles.Dispose();if(camera!=null){camera.targetTexture=null;camera.enabled=true;}if(target!=null){target.Release();Destroy(target);}}
    }
}

#endif
