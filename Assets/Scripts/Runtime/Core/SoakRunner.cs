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
        bool measuring;
        bool manualRender;
        ProfilerRecorder drawCalls,triangles;
        int renderedFrames;
        IEnumerator Start()
        {
            game=GameManager.Instance;if(!game.DiagnosticMode){Destroy(this);yield break;}
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--evidence");output=i>=0&&i+1<args.Length?args[i+1]:Path.Combine(Application.persistentDataPath,"Soak");Directory.CreateDirectory(output);
            i=Array.IndexOf(args,"--soak-seconds");if(i>=0&&i+1<args.Length)double.TryParse(args[i+1],out seconds);
            Application.logMessageReceived+=OnLog;
            foreach(var task in game.Catalog.tasks)game.Save.Profile.completedTasks.Add(task.id);
            for(i=0;i<4;i++)game.Save.Profile.bossWins[i]=true;
            game.BeginRun("adventurer",BiomeId.Mixed);
            while(!game.Ready)yield return null;
            foreach(var u in game.Catalog.upgrades)if(u.effect==UpgradeEffect.NewAbility&&game.Progression.Eligible(u))game.Progression.GrantForDiagnostic(u.id);
            foreach(var u in game.Catalog.upgrades)if(game.Progression.Eligible(u))game.Progression.GrantForDiagnostic(u.id);
            game.UI.ShowHUD();
            game.Run.seconds=1800;
            camera=Camera.main;manualRender=Application.isBatchMode;target=new RenderTexture(1920,1080,24,RenderTextureFormat.ARGB32);target.antiAliasing=4;target.Create();
            QualitySettings.vSyncCount=0;Application.targetFrameRate=60;
            drawCalls=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Draw Calls Count");triangles=ProfilerRecorder.StartNew(ProfilerCategory.Render,"Triangles Count");Camera.onPostRender+=OnRendered;
            var canvas=game.UI.View.GetComponent<Canvas>();
            if(manualRender){camera.enabled=false;camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
            File.WriteAllText(Path.Combine(output,"Samples.csv"),"wallSeconds,gameSeconds,fps,p95FrameMs,maxFrameMs,unityAllocatedMB,unityReservedMB,managedMB,privateMB,enemies,poolObjects,chunks,prefabHandles,cpuFrameMs,gpuFrameMs,originX,originZ,biome,cameraRenders,drawCalls,triangles\n");
            begin=Time.realtimeSinceStartup;nextSample=begin+30;measuring=true;
            while(Time.realtimeSinceStartup-begin<seconds&&errors.Count==0)
            {
                game.Player.Heal(100000);
                if(game.UI.View.upgrade.activeSelf){var choices=game.Progression.Choices();if(choices.Count>0)game.Progression.Apply(choices[0].id,false);else game.Progression.SkipLevel();}
                if(game.UI.View.victory.activeSelf)game.UI.CloseModal();
                if(Time.time>nextDodge){nextDodge=Time.time+2.4f;game.Inputs.ForVerification(Vector2.up*.8f,true);}
                else game.Inputs.ForVerification((Time.realtimeSinceStartup-begin)%60<42?Vector2.up*.8f:Vector2.zero);
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
            File.WriteAllText(Path.Combine(output,"Summary.json"),"{\"finishedUtc\":\""+DateTime.UtcNow.ToString("O")+"\",\"wallSeconds\":"+(Time.realtimeSinceStartup-begin).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\"requestedSeconds\":"+seconds+",\"renderResolution\":\"1920x1080 4x MSAA\",\"renderEveryFrame\":true,\"controlledHealingAndUnlockedBuild\":true,\"samples\":"+samples+",\"maxChunks\":"+maxChunks+",\"maxPool\":"+maxPool+",\"maxPrefabHandles\":"+maxPrefabs+",\"errorCount\":"+errors.Count+"}");
            File.WriteAllLines(Path.Combine(output,"Errors.txt"),errors);
            game.FinishRun(true);game.Save.Flush();Application.Quit(errors.Count==0?0:1);
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
            File.AppendAllText(Path.Combine(output,"Samples.csv"),row);samples++;frames.Clear();renderedFrames=0;
        }
        void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){string detail=message+"\n"+stack;if(!errors.Contains(detail))errors.Add(detail);}}
        void OnRendered(Camera rendered){if(measuring&&rendered==camera)renderedFrames++;}
        void OnDestroy(){Application.logMessageReceived-=OnLog;Camera.onPostRender-=OnRendered;drawCalls.Dispose();triangles.Dispose();if(camera!=null){camera.targetTexture=null;camera.enabled=true;}if(target!=null){target.Release();Destroy(target);}}
    }
}
