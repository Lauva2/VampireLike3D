// External validation assembly. Never included in Assets or the distributable.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using VampireLike;

namespace ExternalValidation
{
    public static class FormalFlowBootstrap
    {
        public static void Begin()
        {
            var root=new GameObject("External formal flow validation");
            UnityEngine.Object.DontDestroyOnLoad(root);
            root.AddComponent<FormalFlowHarness>();
        }
    }
    public sealed class FormalFlowHarness : MonoBehaviour
    {
        GameManager game;
        string output;
        float begin,nextDecision,nextDodge,interactAt,nextStatus;
        int route,runs,deaths,shrineChoices;
        [Serializable] sealed class ShrineAudit { public string run,id; public int before,after,crystalDelta,eventDelta; public bool passed; }
        bool interacted,restart,desertVisited,settled,combatCaptured,bossCaptured;
        readonly List<string> errors=new List<string>();
        readonly Vector2[] destinations={new Vector2(32,0),Vector2.zero,new Vector2(0,32),Vector2.zero,new Vector2(-32,0),Vector2.zero,new Vector2(0,64)};
        static readonly BindingFlags InputFields=BindingFlags.Instance|BindingFlags.NonPublic;
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--evidence");
            if(index<0||index+1>=args.Length){Application.Quit(2);yield break;}
            output=args[index+1];Directory.CreateDirectory(output);restart=Array.IndexOf(args,"--formal-restart")>=0;
            begin=Time.realtimeSinceStartup;Application.logMessageReceived+=OnLog;
            Debug.Log("EXTERNAL_FORMAL_FLOW_START; no desktop input; production assemblies unchanged");
            while(GameManager.Instance==null||!GameManager.Instance.CanBeginRun){if(Time.realtimeSinceStartup-begin>90){errors.Add("Menu startup timeout");Finish(false);yield break;}yield return null;}
            game=GameManager.Instance;
            if(Debug.isDebugBuild||game.DiagnosticMode){errors.Add("Expected non-development Player with diagnostic mode disabled");Finish(false);yield break;}
            game.Inputs.HardwareEnabled=false;
            yield return VerifySelectionRenderers();
            if(errors.Count>0){Finish(false);yield break;}
            if(restart&&!game.Save.Profile.bossWins[0]){errors.Add("Restart lost the first boss unlock");Finish(false);yield break;}
            while(Time.realtimeSinceStartup-begin<1800&&errors.Count==0)
            {
                if(Time.realtimeSinceStartup>nextStatus){nextStatus=Time.realtimeSinceStartup+10;File.WriteAllText(Path.Combine(output,"Progress.json"),"{\"wallSeconds\":"+(Time.realtimeSinceStartup-begin).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\"flow\":\""+game.Flow+"\",\"map\":\""+(game.Run==null?"none":game.Run.biome.ToString())+"\",\"firstBossWin\":"+game.Save.Profile.bossWins[0].ToString().ToLowerInvariant()+",\"tasks\":"+game.Save.Profile.completedTasks.Count+",\"errors\":"+errors.Count+"}");}
                if(game.CanBeginRun)
                {
                    if(settled&&(!restart&&game.Save.Profile.bossWins[0]||restart&&desertVisited)){Capture("menu-after-settlement");Finish(true);yield break;}
                    Capture(restart?"restored-menu":"fresh-menu");
                    game.BeginRun("adventurer",restart?BiomeId.Desert:BiomeId.Forest);runs++;route=0;interacted=false;
                    float deadline=Time.realtimeSinceStartup+90;
                    while(!game.Ready&&game.Flow!=GameFlow.Error&&Time.realtimeSinceStartup<deadline)yield return null;
                    if(!game.Ready){errors.Add("Run startup failed");break;}
                }
                if(game.Flow==GameFlow.Settling)
                {
                    if(!(game.UI.GetPanel(UIPanelId.Results)?.gameObject.activeSelf == true)){yield return null;continue;}
                    if(game.Player!=null&&game.Player.Health<=0)deaths++;
                    Capture("settlement-"+runs);settled=true;
                    game.UI.GetPanel(UIPanelId.Results)?.gameObject.transform.Find("ResultsContinue").GetComponent<Button>().onClick.Invoke();yield return null;continue;
                }
                if(!game.Ready){yield return null;continue;}
                if(restart&&game.Run.seconds>3){desertVisited=true;Capture("restored-desert-run");game.FinishRun(true);yield return null;continue;}
                if(game.UI.GetPanel(UIPanelId.Victory)?.gameObject != null && (game.UI.GetPanel(UIPanelId.Victory)?.gameObject.activeSelf == true)){Capture("boss-victory");game.UI.GetPanel(UIPanelId.Victory)?.gameObject.transform.Find("VictoryRetreat").GetComponent<Button>().onClick.Invoke();yield return null;continue;}
                if(game.UI.GetPanel(UIPanelId.Upgrade)?.gameObject != null && (game.UI.GetPanel(UIPanelId.Upgrade)?.gameObject.activeSelf == true))Choose();
                if(!combatCaptured&&game.Run.seconds>30){combatCaptured=true;Capture("natural-combat");}
                if(!bossCaptured&&game.Combat.Boss!=null){bossCaptured=true;Capture("voluntary-boss");}
                if(Time.unscaledTime>nextDecision&&!game.IsPaused){nextDecision=Time.unscaledTime+.1f;Purchase();Move();}
                yield return null;
            }
            Finish(false);
        }
        IEnumerator VerifySelectionRenderers()
        {
            Button play=null,close=null;
            foreach(var button in game.UI.GetPanel(UIPanelId.Menu)?.gameObject.GetComponentsInChildren<Button>(true))if(button.name=="PlayButton")play=button;
            game.UI.ShowSelection(); yield return game.UI.WaitForPanels();
            foreach(var button in game.UI.GetPanel(UIPanelId.Selection)?.gameObject.GetComponentsInChildren<Button>(true))if(button.name=="SelectionClose")close=button;
            game.UI.CloseModal(); yield return game.UI.WaitForPanels();
            if(play==null||close==null){errors.Add("Production selection buttons missing");yield break;}
            for(int cycle=0;cycle<8&&errors.Count==0;cycle++)
            {
                play.onClick.Invoke();
                yield return game.UI.WaitForPanels();
                if(game.UI.ActiveModal!="selection"){errors.Add("Play did not open production selection");yield break;}
                foreach(var widget in game.UI.GetView<SelectionView>(UIPanelId.Selection)?.characterList.GetComponentsInChildren<UIWidget>())if(widget.button.interactable){widget.button.onClick.Invoke();break;}
                foreach(var scroll in game.UI.GetPanel(UIPanelId.Selection)?.gameObject.GetComponentsInChildren<ScrollRect>())scroll.verticalNormalizedPosition=cycle%2;
                Canvas.ForceUpdateCanvases();
                foreach(var mask in game.UI.GetPanel(UIPanelId.Selection)?.gameObject.GetComponentsInChildren<RectMask2D>())mask.PerformClipping();
                foreach(var graphic in game.UI.Root.GetComponentsInChildren<Graphic>(true))
                    if(graphic.GetComponent<CanvasRenderer>()==null||graphic.canvasRenderer!=graphic.GetComponent<CanvasRenderer>())errors.Add("Invalid production CanvasRenderer: "+graphic.name);
                yield return null;
                if(cycle==0)Capture("selection-renderers");
                close.onClick.Invoke();Canvas.ForceUpdateCanvases();yield return null;
                if(game.UI.ActiveModal!="menu"||!(game.UI.GetPanel(UIPanelId.Menu)?.gameObject.activeSelf == true))errors.Add("Selection close did not restore production menu");
                File.AppendAllText(Path.Combine(output,"SelectionRendererCycles.jsonl"),"{\"cycle\":"+(cycle+1)+",\"shrineChoices\":"+shrineChoices+",\"errorCount\":"+errors.Count+",\"closedModal\":\""+game.UI.ActiveModal+"\"}\n");
            }
        }
        float Score(UpgradeDefinition u)
        {
            if(u.effect==UpgradeEffect.NewAbility)return u.target==AbilityKind.Fireball?100:u.target==AbilityKind.Sword?95:55;
            if(u.id=="frost"||u.id=="lightning")return 92;
            if(u.id=="magnet"&&game.Run.Level(u.id)<3)return 75;
            if(u.id=="regen"&&game.Run.Level(u.id)<3)return 85;
            if(u.id=="vitality"&&game.Player.Health/game.Player.MaxHealth<.65f)return 90;
            if(u.effect==UpgradeEffect.Count)return 65-game.Run.Level(u.id)*5;
            if(u.id=="damage"||u.id=="haste")return 70-game.Run.Level(u.id)*4;
            return u.unique?62:30-game.Run.Level(u.id)*2;
        }
        void Choose()
        {
            if (game.UI.IsLoading || game.UI.GetView<UpgradeView>(UIPanelId.Upgrade)?.upgradeList == null) return;
            if (game.UI.ActiveModal == "blessing")
            {
                var fade = game.UI.GetPanel(UIPanelId.Upgrade)?.gameObject.GetComponent<CanvasGroup>(); if (fade != null && fade.alpha < .99f) return;
                foreach (Transform child in game.UI.GetView<UpgradeView>(UIPanelId.Upgrade)?.upgradeList)
                {
                    var card = child.GetComponent<UIWidget>(); if (card == null || !child.gameObject.activeSelf) continue;
                    var definition = Array.Find(game.Catalog.blessings, b => b.name == card.title.text);
                    if (definition == null) { errors.Add("Unrecognized shrine card"); return; }
                    Capture("shrine-choice-" + (shrineChoices + 1));
                    int crystals = game.Save.Profile.crystals, events = game.Save.Profile.events;
                    var audit = new ShrineAudit { run = game.Run.id, id = definition.id, before = game.Run.BlessingLevel(definition.id) };
                    card.button.onClick.Invoke(); audit.after = game.Run.BlessingLevel(definition.id);
                    audit.crystalDelta = game.Save.Profile.crystals - crystals; audit.eventDelta = game.Save.Profile.events - events;
                    audit.passed = audit.after == audit.before + 1 && audit.crystalDelta == 4 && audit.eventDelta == 1;
                    File.AppendAllText(Path.Combine(output, "ShrineChoices.jsonl"), JsonUtility.ToJson(audit) + "\n");
                    if (!audit.passed) errors.Add("Production shrine selection failed to grant exactly one blessing and base reward");
                    shrineChoices++; return;
                }
                return;
            }
            UIWidget chosen=null;float best=-1000;
            foreach(Transform t in game.UI.GetView<UpgradeView>(UIPanelId.Upgrade)?.upgradeList){var w=t.GetComponent<UIWidget>();if(w==null||!w.gameObject.activeSelf)continue;var u=Array.Find(game.Catalog.upgrades,x=>x.name==w.title.text);if(u!=null&&Score(u)>best){best=Score(u);chosen=w;}}
            if(chosen!=null)chosen.button.onClick.Invoke();else game.Progression.SkipLevel();
        }
        void Purchase()
        {
            UpgradeDefinition chosen=null;float best=-1000;
            foreach(var u in game.Catalog.upgrades){if(game.Progression.Eligible(u)&&game.Progression.Cost(u)<=game.Run.skillPoints&&Score(u)>best){chosen=u;best=Score(u);}}
            if(chosen!=null)game.Progression.Apply(chosen.id,true);
        }
        void Move()
        {
            Vector2 global=game.World.GlobalPosition,goal;
            if(route<destinations.Length)
            {
                goal=destinations[route];
                if(Vector2.Distance(global,goal)<3.7f)
                {
                    if(route%2==1){route++;return;}
                    if(!interacted&&(route!=6||game.Run.seconds>=180)){Interact();interacted=true;interactAt=Time.time;}
                    if(route==4&&Time.time-interactAt<18)goal+=new Vector2(Mathf.Sin(Time.time)*5,Mathf.Cos(Time.time)*5);
                    else if(route==6&&game.Run.seconds<180)goal+=new Vector2(Mathf.Sin(Time.time*.4f)*7,Mathf.Cos(Time.time*.4f)*7);
                    else {route++;interacted=false;}
                }
            }
            else if(game.Combat.BossChallengeActive)
            {
                Vector3 center=game.Combat.BossCenter,localGoal=center+new Vector3(Mathf.Sin(Time.time*.6f)*8,0,Mathf.Cos(Time.time*.6f)*8);
                goal=global+new Vector2(localGoal.x-game.Player.transform.position.x,localGoal.z-game.Player.transform.position.z);
            }
            else
            {
                WorldSite boss=null;foreach(var site in game.World.Sites)if(site.type==3&&!site.complete&&(boss==null||Vector3.Distance(site.position,game.Player.transform.position)<Vector3.Distance(boss.position,game.Player.transform.position)))boss=site;
                if(boss!=null&&game.Run.seconds>=180){goal=global+new Vector2(boss.position.x-game.Player.transform.position.x,boss.position.z-game.Player.transform.position.z);if(Vector2.Distance(global,goal)<4)Interact();}
                else goal=new Vector2(Mathf.Sin(Time.time*.2f)*9,64+Mathf.Cos(Time.time*.2f)*9);
            }
            if(!game.Combat.BossChallengeActive&&game.Run.seconds%60<20)
            {
                ExperienceOrb nearest=null;float best=8;
                foreach(var orb in game.GetRunObjects<ExperienceOrb>()){float d=Vector3.Distance(orb.transform.position,game.Player.transform.position);if(d<best){best=d;nearest=orb;}}
                if(nearest!=null)goal=global+new Vector2(nearest.transform.position.x-game.Player.transform.position.x,nearest.transform.position.z-game.Player.transform.position.z);
            }
            Vector2 direction=Vector2.ClampMagnitude(goal-global,1);Vector3 move=new Vector3(direction.x,0,direction.y);
            if(Physics.SphereCast(game.Player.transform.position+Vector3.up*.8f,.45f,move,out var hit,2,(1<<0)|(1<<10))){Vector3 side=Vector3.Cross(hit.normal,Vector3.up);if(Vector3.Dot(side,move)<0)side=-side;direction=new Vector2(side.x,side.z);}
            SetInput("simulated",true);SetInput("simulatedMove",direction);
            if(Time.time>nextDodge){nextDodge=Time.time+2.4f;SetInput("dodgeFrame",Time.frameCount+1);}
        }
        void Interact(){SetInput("interactFrame",Time.frameCount+1);}
        void SetInput(string name,object value){typeof(InputService).GetField(name,InputFields).SetValue(game.Inputs,value);}
        void Capture(string name)
        {
            Camera camera=Camera.main;if(camera==null)return;
            var canvas=game.UI.Root.GetComponent<Canvas>();var previousMode=canvas.renderMode;var previousCamera=canvas.worldCamera;float previousDistance=canvas.planeDistance;
            var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
            var target=new RenderTexture(1920,1080,24){antiAliasing=4};target.Create();Texture2D image=null;
            try{canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=target;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;image=new Texture2D(1920,1080,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());}
            finally{camera.targetTexture=previousTarget;RenderTexture.active=previousActive;canvas.renderMode=previousMode;canvas.worldCamera=previousCamera;canvas.planeDistance=previousDistance;if(image!=null)Destroy(image);target.Release();Destroy(target);}
        }
        void Finish(bool passed)
        {
            if (passed && !restart && shrineChoices == 0) { errors.Add("Natural production flow did not claim a shrine blessing"); passed = false; }
            if(game!=null)game.Save.Flush();
            File.WriteAllText(Path.Combine(output,"Summary.json"),"{\"passed\":"+(passed?"true":"false")+",\"development\":"+Debug.isDebugBuild.ToString().ToLowerInvariant()+",\"diagnosticMode\":"+(game!=null&&game.DiagnosticMode).ToString().ToLowerInvariant()+",\"externalInputAndValidatedPurchasesOnly\":true,\"restartPhase\":"+restart.ToString().ToLowerInvariant()+",\"wallSeconds\":"+(Time.realtimeSinceStartup-begin).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\"runs\":"+runs+",\"deaths\":"+deaths+",\"firstBossWin\":"+(game!=null&&game.Save.Profile.bossWins[0]).ToString().ToLowerInvariant()+",\"desertVisited\":"+desertVisited.ToString().ToLowerInvariant()+",\"tasks\":"+(game==null?0:game.Save.Profile.completedTasks.Count)+",\"shrineChoices\":"+shrineChoices+",\"errorCount\":"+errors.Count+"}");
            File.WriteAllLines(Path.Combine(output,"Errors.txt"),errors);Application.Quit(passed?0:1);
        }
        void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message+"\n"+stack);}
        void OnDestroy(){Application.logMessageReceived-=OnLog;}
    }
}
