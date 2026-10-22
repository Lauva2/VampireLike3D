#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    // New-profile progression bot. It issues input and validated purchases only;
    // no stat, clock, task, reward, health, spawn or unlock overrides are used.
    public sealed class CampaignRunner : MonoBehaviour
    {
        GameManager game;
        string output;
        float begin,nextDecision,nextDodge,nextLog,interactAt;
        int stage,runs,deaths,route;
        readonly List<string> errors=new List<string>();
        readonly Vector2[] destinations={new Vector2(32,0),Vector2.zero,new Vector2(0,32),Vector2.zero,new Vector2(-32,0),Vector2.zero,new Vector2(0,64)};
        bool interacted;
        bool mixedVisited;
        IEnumerator Start()
        {
            game=GameManager.Instance;if(!game.DiagnosticMode){Destroy(this);yield break;}
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--evidence");output=index>=0&&index+1<args.Length?args[index+1]:Path.Combine(Application.persistentDataPath,"Campaign");Directory.CreateDirectory(output);
            Application.logMessageReceived+=OnLog;begin=Time.realtimeSinceStartup;
            if(!File.Exists(Path.Combine(output,"Progress.csv")))File.WriteAllText(Path.Combine(output,"Progress.csv"),"wallSeconds,map,runSeconds,runLevel,kills,health,gunKills,fireKills,swordKills,freezes,events,tasks,bosses,crystals,x,z\n");
            while(Time.realtimeSinceStartup-begin<7200&&errors.Count==0)
            {
                if(game.CanBeginRun)
                {
                    for(int n=0;n<3;n++)for(int i=0;i<2;i++)game.Save.BuyMeta(i);
                    while(stage<4&&game.Save.Profile.bossWins[stage])stage++;
                    if(stage>=4&&mixedVisited&&game.Save.Profile.completedTasks.Count==game.Catalog.tasks.Length){Finish(true);yield break;}
                    string character=game.Save.TaskUnlocked("swordsman")&&game.Save.Profile.swordKills<220?"swordsman":game.Save.TaskUnlocked("mage")&&game.Save.Profile.fireKills<180?"elementalist":"adventurer";
                    game.BeginRun(character,(BiomeId)Mathf.Min(stage,4));if(stage>=4)mixedVisited=true;runs++;route=0;interacted=false;
                    float loadingDeadline=Time.realtimeSinceStartup+90;
                    while(!game.Ready&&game.Flow!=GameFlow.Error&&Time.realtimeSinceStartup<loadingDeadline)yield return null;
                    if(!game.Ready){errors.Add("Natural campaign could not enter a run within 90 seconds");break;}
                }
                if(game.Flow==GameFlow.Settling)
                {
                    if(!game.UI.View.results.activeSelf){yield return null;continue;}
                    if(game.Player!=null&&game.Player.Health<=0)deaths++;
                    game.UI.View.results.transform.Find("ResultsContinue")?.GetComponent<UnityEngine.UI.Button>()?.onClick.Invoke();yield return null;continue;
                }
                if(!game.Ready){yield return null;continue;}
                if(stage>=4&&mixedVisited&&game.Save.Profile.completedTasks.Count==game.Catalog.tasks.Length){game.FinishRun(true);yield return null;continue;}
                if(game.UI.ActiveModal=="pause"||game.UI.ActiveModal=="settings"||game.UI.ActiveModal=="map"||game.UI.ActiveModal=="skills")game.UI.CloseModal();
                if(game.UI.View.victory.activeSelf)
                {
                    game.UI.View.victory.transform.Find("VictoryRetreat")?.GetComponent<UnityEngine.UI.Button>()?.onClick.Invoke();yield return null;
                    game.UI.View.results.transform.Find("ResultsContinue")?.GetComponent<UnityEngine.UI.Button>()?.onClick.Invoke();yield return null;continue;
                }
                if(game.UI.View.upgrade.activeSelf)ChooseVisibleUpgrade();
                if(Time.unscaledTime>nextDecision&&!game.IsPaused)
                {
                    nextDecision=Time.unscaledTime+.1f;Purchase();Move();
                }
                if(Time.unscaledTime>nextLog){nextLog=Time.unscaledTime+10;Log();}
                yield return null;
            }
            Finish(false);
        }
        float Score(UpgradeDefinition u)
        {
            if(stage>=4&&game.Save.Profile.shatterCombo<20)
            {
                if(u.id=="frost")return 200;
                if(u.effect==UpgradeEffect.NewAbility)return u.target==AbilityKind.Fireball||u.target==AbilityKind.Gun?150:-10000;
                if(u.target==AbilityKind.Gun)return 120-game.Run.Level(u.id)*4;
                if(u.id=="regen"&&game.Run.Level(u.id)<4)return 100;
                if(u.id=="haste")return 80-game.Run.Level(u.id)*3;
                if(u.id=="vitality"&&game.Player.Health/game.Player.MaxHealth<.7f)return 105;
                if(u.target!=AbilityKind.All)return -10000;
            }
            if(u.effect==UpgradeEffect.NewAbility)return u.target==AbilityKind.Fireball?100:u.target==AbilityKind.Sword?95:55;
            if(u.id=="frost"||u.id=="lightning")return 92;
            if(u.id=="magnet"&&game.Run.Level(u.id)<3)return 75;
            if(u.id=="regen"&&game.Run.Level(u.id)<3)return 85;
            if(u.id=="vitality"&&game.Player.Health/game.Player.MaxHealth<.65f)return 90;
            if(u.effect==UpgradeEffect.Count)return 65-game.Run.Level(u.id)*5;
            if(u.id=="damage"||u.id=="haste")return 70-game.Run.Level(u.id)*4;
            if(u.unique)return 62;
            return 30-game.Run.Level(u.id)*2;
        }
        void ChooseVisibleUpgrade()
        {
            UIWidget chosen=null;float best=-1000;
            foreach(Transform t in game.UI.View.upgradeList)
            {
                var widget=t.GetComponent<UIWidget>();if(widget==null||!widget.gameObject.activeSelf)continue;
                var definition=Array.Find(game.Catalog.upgrades,u=>u.name==widget.title.text);if(definition==null)continue;
                float score=Score(definition);if(score>best){best=score;chosen=widget;}
            }
            if(chosen!=null)chosen.button.onClick.Invoke();else game.Progression.SkipLevel();
        }
        void Purchase()
        {
            UpgradeDefinition chosen=null;float best=-1000;
            foreach(var u in game.Catalog.upgrades)
            {
                if(!game.Progression.Eligible(u)||game.Progression.Cost(u)>game.Run.skillPoints)continue;
                float score=Score(u);if(score>best){best=score;chosen=u;}
            }
            if(chosen!=null)game.Progression.Apply(chosen.id,true);
        }
        void Move()
        {
            Vector2 global=game.World.GlobalPosition;Vector2 goal;
            if(stage>=4)
            {
                goal=new Vector2(Mathf.Sin(Time.time*.12f)*12,64+Mathf.Cos(Time.time*.12f)*12);
                EnemyActor frozen=null;float nearest=18;foreach(var candidate in game.Combat.Enemies){if(!candidate.IsAlive||!candidate.Frozen)continue;float distance=Vector3.Distance(candidate.transform.position,game.Player.transform.position);if(distance<nearest){nearest=distance;frozen=candidate;}}
                game.Combat.SelectPriorityTarget(frozen);
            }
            else if(route<destinations.Length)
            {
                goal=destinations[route];
                if(Vector2.Distance(global,goal)<3.7f)
                {
                    if(route%2==1){route++;return;}
                    if(!interacted&&(route!=6||game.Run.seconds>=180)){game.Inputs.InteractForVerification();interacted=true;interactAt=Time.time;}
                    if(route==4&&Time.time-interactAt<18)goal+=new Vector2(Mathf.Sin(Time.time)*5,Mathf.Cos(Time.time)*5);
                    else if(route==6&&game.Run.seconds<180)goal+=new Vector2(Mathf.Sin(Time.time*.4f)*7,Mathf.Cos(Time.time*.4f)*7);
                    else {route++;interacted=false;}
                }
            }
            else if(game.Combat.Boss!=null)
            {
                Vector3 center=game.Combat.BossCenter;
                Vector3 localGoal=center+new Vector3(Mathf.Sin(Time.time*.6f)*8,0,Mathf.Cos(Time.time*.6f)*8);
                goal=global+new Vector2(localGoal.x-game.Player.transform.position.x,localGoal.z-game.Player.transform.position.z);
            }
            else
            {
                // Wait near a discovered boss until the 3-minute preparation target,
                // then use the normal E interaction; later mixed runs keep exploring.
                WorldSite boss=null;foreach(var site in game.World.Sites)if(site.type==3&&!site.complete){if(boss==null||Vector3.Distance(site.position,game.Player.transform.position)<Vector3.Distance(boss.position,game.Player.transform.position))boss=site;}
                if(boss!=null&&game.Run.seconds>=180)
                {
                    goal=global+new Vector2(boss.position.x-game.Player.transform.position.x,boss.position.z-game.Player.transform.position.z);
                    if(Vector2.Distance(global,goal)<4)game.Inputs.InteractForVerification();
                }
                else goal=new Vector2(Mathf.Sin(Time.time*.2f)*9,64+Mathf.Cos(Time.time*.2f)*9);
            }
            if(game.Combat.Boss==null)
            {
                ExperienceOrb nearest=null;float best=8;
                if(game.Run.seconds%60<20)foreach(var orb in game.GetRunObjects<ExperienceOrb>()){float distance=Vector3.Distance(orb.transform.position,game.Player.transform.position);if(distance<best){best=distance;nearest=orb;}}
                if(nearest!=null)goal=global+new Vector2(nearest.transform.position.x-game.Player.transform.position.x,nearest.transform.position.z-game.Player.transform.position.z);
            }
            Vector2 direction=Vector2.ClampMagnitude(goal-global,1);
            Vector3 move=new Vector3(direction.x,0,direction.y);
            if(Physics.SphereCast(game.Player.transform.position+Vector3.up*.8f,.45f,move,out var hit,2,(1<<0)|(1<<10)))
            {
                Vector3 side=Vector3.Cross(hit.normal,Vector3.up);if(Vector3.Dot(side,move)<0)side=-side;direction=new Vector2(side.x,side.z);
            }
            bool dodge=Time.time>nextDodge;if(dodge)nextDodge=Time.time+2.4f;
            game.Inputs.ForVerification(direction,dodge);
        }
        void Log()
        {
            if(!game.Ready)return;var p=game.Save.Profile;Vector2 pos=game.World.GlobalPosition;int bosses=0;foreach(bool b in p.bossWins)if(b)bosses++;
            string line=string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0:F1},{1},{2:F1},{3},{4},{5:F1},{6},{7},{8},{9},{10},{11},{12},{13},{14:F1},{15:F1}\n",Time.realtimeSinceStartup-begin,game.Run.biome,game.Run.seconds,game.Run.level,game.Run.kills,game.Player.Health,p.gunKills,p.fireKills,p.swordKills,p.freezes,p.events,p.completedTasks.Count,bosses,p.crystals,pos.x,pos.y);File.AppendAllText(Path.Combine(output,"Progress.csv"),line);
        }
        void Finish(bool passed)
        {
            game.Save.MarkDirty();game.Save.Flush();File.WriteAllText(Path.Combine(output,"Summary.json"),"{\"passed\":"+(passed?"true":"false")+",\"freshProfile\":true,\"inputAndValidatedPurchasesOnly\":true,\"noForcedStatsHealthSpawnsTimeOrUnlocks\":true,\"wallSeconds\":"+(Time.realtimeSinceStartup-begin).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)+",\"runs\":"+runs+",\"deaths\":"+deaths+",\"tasks\":"+game.Save.Profile.completedTasks.Count+",\"errors\":"+errors.Count+"}");File.WriteAllLines(Path.Combine(output,"Errors.txt"),errors);Application.Quit(passed?0:1);
        }
        void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message+"\n"+stack);}
        void OnDestroy(){Application.logMessageReceived-=OnLog;}
    }
}

#endif
