#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        IEnumerator TerrainVariants(int biome)
        {
            Vector2 previous=game.World.GlobalPosition;
            for(int variant=0;variant<3;variant++)
            {
                Vector2Int chosen=Vector2Int.zero;bool found=false;
                for(int x=2;x<=32&&!found;x++)for(int z=-7;z<=7&&!found;z++)
                {
                    var key=new Vector2Int(x,z);
                    if(game.World.TerrainVariant(key,(BiomeId)biome)==variant){chosen=key;found=true;}
                }
                Check(found,"Locate actual terrain variant "+biome+" / "+variant);
                Place(game.World.LocalPosition(chosen)+new Vector3(-3,0,4));yield return AutoBattle(2);
                yield return Capture("geography-"+biome+"-"+variant);
                CameraRig.Active.DiagnosticZoom(19);yield return Wait(.2f);yield return Capture("geography-wide-"+biome+"-"+variant);CameraRig.Active.DiagnosticZoom(13.5f);
            }
            Place(new Vector3(previous.x-game.World.OriginChunk.x*32,0,previous.y-game.World.OriginChunk.y*32));yield return AutoBattle(2);
        }
        IEnumerator EnemyGallery()
        {
            game.Pause("gallery",true);
            foreach(var enemyDefinition in game.Catalog.enemies)for(int elite=0;elite<2;elite++)
            {
                int type=enemyDefinition.type;EnemyActor enemy=null;
                for(int attempt=0;attempt<24&&enemy==null;attempt++)
                {
                    float angle=attempt*2.399f;Vector3 point=game.Player.transform.position+new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle))*(5+attempt%3);
                    enemy=game.Combat.Spawn(type,point,elite==1);
                }
                if(enemy==null){Check(false,"Enemy gallery spawn "+type);continue;}
                yield return ModelCloseup(enemy,"enemy-"+type+(elite==1?"-elite":"-normal"));
                game.Combat.Enemies.Remove(enemy);game.Pool.Return(enemy.gameObject);
            }
            game.Pause("gallery",false);
        }
        IEnumerator SanctuaryEvidence(int biome)
        {
            Vector2 previous=game.World.GlobalPosition;
            Place(game.World.LocalPosition(new Vector2Int(0,2))+Vector3.back*4);yield return AutoBattle(2);yield return Capture("sanctuary-"+biome);
            CameraRig.Active.DiagnosticZoom(8.5f);yield return Wait(.2f);yield return Capture("sanctuary-close-"+biome);CameraRig.Active.DiagnosticZoom(13.5f);
            WorldSite site=null;foreach(var candidate in game.World.Sites)if(candidate.chunk==new Vector2Int(0,2)&&candidate.type==3){site=candidate;break;}
            Check(site!=null&&site.visual!=null,"Themed challenge site loads at controlled position "+biome);
            if(site!=null&&site.visual!=null)
            {
                Vector3 scale=site.visual.transform.localScale;int events=game.Save.Profile.events;
                game.World.CompleteEvent(site.id);game.World.CompleteEvent(site.id);
                Check(site.visual.transform.localScale==scale&&game.Save.Profile.events==events+1,"Challenge completion preserves scenery and grants progress once "+biome);
                var appearance=site.visual.GetComponent<BossSiteAppearance>();
                Check(appearance!=null&&appearance.invocationSeals.Length==ContentIds.Regions.Length&&!appearance.invocationSeals[ContentIds.RegionIndex((BiomeId)biome)].activeSelf,"Completed challenge extinguishes its invocation seal "+biome);
                yield return AutoBattle(.5f);
                Check(string.IsNullOrEmpty(game.World.InteractionText),"Completed challenge clears its interaction prompt "+biome);
                yield return Capture("sanctuary-completed-"+biome);
                Place(game.World.LocalPosition(new Vector2Int(14,2)));yield return AutoBattle(2);
                Place(game.World.LocalPosition(new Vector2Int(0,2))+Vector3.back*4);yield return AutoBattle(2);
                WorldSite revisited=null;foreach(var candidate in game.World.Sites)if(candidate.chunk==new Vector2Int(0,2)&&candidate.type==3){revisited=candidate;break;}
                Check(revisited!=null&&revisited.complete&&revisited.visual!=null&&revisited.visual.transform.localScale==scale&&!revisited.visual.GetComponent<BossSiteAppearance>().invocationSeals[ContentIds.RegionIndex((BiomeId)biome)].activeSelf,"Challenge completion survives chunk unload with full-size scenery "+biome);
            }
            Place(new Vector3(previous.x-game.World.OriginChunk.x*32,0,previous.y-game.World.OriginChunk.y*32));yield return AutoBattle(1);
        }
        IEnumerator WatersideArchitecture(int biome)
        {
            Vector2 previous=game.World.GlobalPosition;bool found=false;int attempts=0;
            for(int distance=0;distance<=16&&!found;distance++)for(int z=-7;z<=7&&!found;z++)
            {
                var key=new Vector2Int(distance,z);
                if(WorldService.SeedHash(game.Run.seed,key.x,key.y)%7!=0)continue;
                if((biome==0||biome==3)&&!game.World.HasWatercourse(key))continue;
                if(++attempts>10)break;
                Place(game.World.LocalPosition(key)+new Vector3(-3,0,-3));yield return AutoBattle(2);
                float deadline=Time.realtimeSinceStartup+20;while(!game.World.ViewReady&&Time.realtimeSinceStartup<deadline)yield return null;
                if(!game.World.TryDiagnosticArchitecture(key,out var building,out bool clear))continue;
                Check(clear,"Waterside architecture has level foundation clear of water "+biome);
                Place(building+Vector3.back*4);yield return AutoBattle(1);yield return Capture("waterside-architecture-"+biome);
                CameraRig.Active.DiagnosticZoom(19);yield return Wait(.2f);yield return Capture("waterside-architecture-wide-"+biome);CameraRig.Active.DiagnosticZoom(13.5f);
                File.WriteAllText(Path.Combine(output,"Captures","waterside-architecture-"+biome+".placement.json"),"{\"actualGeneratedChunk\":\""+key+"\",\"foundationClear\":"+clear.ToString().ToLowerInvariant()+",\"forcedBuildingPlacement\":false}");found=true;
            }
            Check(found,"Actual waterside architecture evidence found "+biome);
            Place(new Vector3(previous.x-game.World.OriginChunk.x*32,0,previous.y-game.World.OriginChunk.y*32));yield return AutoBattle(1);
        }
        IEnumerator RoleActionCloseup(string id)
        {
            var rig=CameraRig.Active;var camera=Camera.main;Vector3 position=camera.transform.position;Quaternion rotation=camera.transform.rotation;float size=camera.orthographicSize;
            rig.enabled=false;Vector3 center=game.Player.transform.position+Vector3.up*1.4f;
            camera.transform.position=center+new Vector3(8,5,10);camera.transform.LookAt(center);camera.orthographicSize=4.2f;
            yield return Sequence("role-closeup-"+id,4,false);
            camera.transform.SetPositionAndRotation(position,rotation);camera.orthographicSize=size;rig.enabled=true;
        }
        IEnumerator ModelCloseup(EnemyActor actor,string name)
        {
            var rig=CameraRig.Active;var camera=Camera.main;Vector3 position=camera.transform.position;Quaternion rotation=camera.transform.rotation;float size=camera.orthographicSize;int mask=camera.cullingMask;
            var parts=actor.GetComponentsInChildren<Transform>(true);var layers=new int[parts.Length];for(int i=0;i<parts.Length;i++){layers[i]=parts[i].gameObject.layer;parts[i].gameObject.layer=12;}
            camera.cullingMask=(1<<12)|(1<<5);
            rig.enabled=false;game.Pause("model-evidence",true);game.UI.GetPanel(UIPanelId.HUD)?.gameObject.SetActive(false);
            bool bar=actor.barRoot!=null&&actor.barRoot.gameObject.activeSelf;if(actor.barRoot!=null)actor.barRoot.gameObject.SetActive(false);
            Bounds bounds=new Bounds(actor.transform.position+Vector3.up,.1f*Vector3.one);bool first=true;
            foreach(var renderer in actor.GetComponentsInChildren<Renderer>())
            {
                if(renderer.name=="ContactShadow")continue;
                if(first){bounds=renderer.bounds;first=false;}else bounds.Encapsulate(renderer.bounds);
            }
            Vector3 center=bounds.center;
            camera.transform.position=center+new Vector3(6,4,8);camera.transform.LookAt(center);camera.orthographicSize=Mathf.Max(.9f,bounds.extents.magnitude*.95f);
            yield return Capture(name,1600,1200);
            File.WriteAllText(Path.Combine(output,"Captures",name+".model.json"),"{\"source\":\"actual runtime actor\",\"cameraIsolatesActorFromForeground\":true}");
            for(int i=0;i<parts.Length;i++)parts[i].gameObject.layer=layers[i];camera.cullingMask=mask;
            if(actor.barRoot!=null)actor.barRoot.gameObject.SetActive(bar);
            camera.transform.SetPositionAndRotation(position,rotation);camera.orthographicSize=size;rig.enabled=true;game.UI.GetPanel(UIPanelId.HUD)?.gameObject.SetActive(true);game.Pause("model-evidence",false);
        }
        IEnumerator FeedbackEvidence()
        {
            ClearEnemies();game.Inputs.ForVerification(Vector2.zero);game.UI.CloseModal();
            game.Progression.Experience(game.Run.nextExperience);game.UI.ShowUpgrade(game.Progression.Choices());
            yield return RealtimeSequence("feedback-upgrade-open",.5f);
            var choices=game.Progression.Choices();if(choices.Count>0)game.Progression.Apply(choices[0].id,false);else game.Progression.SkipLevel();
            yield return RealtimeSequence("feedback-upgrade-reward",.5f);
            var task=game.Catalog.tasks[0];game.Save.Profile.completedTasks.Remove(task.id);game.Progression.CheckTasks();
            yield return RealtimeSequence("feedback-task-unlock",.6f);
            game.UI.ToggleSkills();yield return RealtimeSequence("feedback-skills-open",.5f);game.UI.CloseModal();
            game.UI.TogglePause();yield return RealtimeSequence("feedback-pause-open",.5f);game.UI.CloseModal();
        }
        IEnumerator RealtimeSequence(string name,float seconds)
        {
            string folder=Path.Combine(output,"Sequences",name);Directory.CreateDirectory(folder);
            float start=Time.realtimeSinceStartup;int frame=0;
            using(var times=new StreamWriter(Path.Combine(folder,"timestamps.csv")))
            {
                times.WriteLine("frame,wallSeconds");
                while(Time.realtimeSinceStartup-start<seconds)
                {
                    times.WriteLine(frame+","+(Time.realtimeSinceStartup-start).ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
                    RenderCapture(Path.Combine(folder,frame.ToString("D4")+".jpg"),1280,720,true);frame++;
                    yield return new WaitForSecondsRealtime(1f/30);
                }
            }
            File.WriteAllText(Path.Combine(folder,"metadata.json"),"{\"source\":\"development Player actual runtime animation\",\"wallClockTimestamps\":true,\"frames\":"+frame+",\"requestedIntervalSeconds\":0.03333,\"fixedSimulationFps\":null}");
        }
        IEnumerator DefeatEvidence()
        {
            ClearEnemies();ClearExpansionEffects();game.Inputs.ForVerification(Vector2.zero);game.Abilities.enabled=false;
            yield return AdvanceGameplay(.8f);ClearEnemies();ClearExpansionEffects();
            Check(!game.IsPaused&&!game.Player.Invulnerable,"Death capture begins after pending upgrade and invulnerability windows expire");
            game.Player.Hurt(100000);yield return RealtimeSequence("player-defeat",.85f);
            Check(game.Flow==GameFlow.Settling&&(game.UI.GetPanel(UIPanelId.Results)?.gameObject.activeSelf == true),"Player defeat animation reaches death settlement");
            yield return Capture("death-results");
        }
    }
}

#endif
