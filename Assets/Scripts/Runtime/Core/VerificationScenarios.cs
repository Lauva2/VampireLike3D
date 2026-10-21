#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        IEnumerator TerrainProbe()
        {
            // Controlled geometry diagnostic; it is not natural progression evidence.
            yield return LoadingFailureProbe("weapon/gun");
            yield return LoadingFailureProbe("terrain/forest");
            game.Save.Profile.bossWins[0]=true;
            game.BeginRun("adventurer",BiomeId.Desert);
            float deadline=Time.realtimeSinceStartup+90;
            while(!game.Ready&&Time.realtimeSinceStartup<deadline)yield return null;
            Check(game.Ready,"Terrain probe enters desert");if(!game.Ready)yield break;
            foreach(var position in new[]{Vector3.zero,new Vector3(47,0,47),new Vector3(-79,0,111),new Vector3(640,0,32)})
            {
                Place(position);yield return AutoBattle(2);yield return Capture("probe-"+game.World.CurrentChunk.x+"-"+game.World.CurrentChunk.y);
            }
            CameraRig.Active.DiagnosticZoom(23);Screen.SetResolution(2560,1080,false);yield return AutoBattle(2);
            yield return Capture("probe-max-zoom-ultrawide",2560,1080);
            var pendingScope=new AssetScope("cancelled-instance-probe");var parent=new GameObject("CancelledInstanceParent");bool callback=false;
            var routine=game.StartCoroutine(game.Assets.Instantiate("world/chest",pendingScope,parent.transform,_=>callback=true));
            game.Assets.Release(pendingScope);game.StopCoroutine(routine);yield return Wait(2);
            Check(!callback&&parent.transform.childCount==0,"Cancelled AA instantiate releases late result after coroutine stops");Destroy(parent);
        }
        IEnumerator PrepareOcclusionExit(System.Action<Renderer> done)
        {
            game.Inputs.ForVerification(Vector2.zero);game.Pause("occlusion-exit-probe",true);
            var block=new MaterialPropertyBlock();int attempts=0;
            foreach(var fader in game.GetRunObjects<OcclusionFader>())
            {
                if(fader==null||!fader.gameObject.activeInHierarchy)continue;
                var renderer=fader.GetComponentInChildren<Renderer>();
                if(renderer==null||renderer.bounds.size.y<1.5f)continue;
                Place(fader.transform.position+Vector3.back*.2f);yield return Wait(.32f);
                foreach(var part in fader.GetComponentsInChildren<Renderer>())
                {
                    part.GetPropertyBlock(block);float visibility=block.GetFloat("_Visibility");
                    if(visibility>0&&visibility<.99f){done(part);Check(true,"Activate real scenery occlusion before settlement");yield break;}
                }
                if(++attempts>=20)break;
            }
            Check(false,"Activate real scenery occlusion before settlement");
        }
        IEnumerator LoadingFailureProbe(string id)
        {
            float stageDeadline=Time.realtimeSinceStartup+25;
            while((MenuStage.Active==null||!MenuStage.Active.Ready)&&Time.realtimeSinceStartup<stageDeadline)yield return null;
            int baseline=game.Assets.LoadedPrefabCount;
            Check(game.Assets.DiagnosticFailNextPrefab(id),"Arm controlled AA failure "+id);
            game.BeginRun("adventurer",BiomeId.Forest);
            float deadline=Time.realtimeSinceStartup+90;
            while(!game.CanBeginRun&&Time.realtimeSinceStartup<deadline)yield return null;
            while((MenuStage.Active==null||!MenuStage.Active.Ready)&&Time.realtimeSinceStartup<deadline)yield return null;
            yield return Wait(.2f);
            Check(game.CanBeginRun&&game.Player==null&&game.World==null,"Failed content returns safely to ready menu "+id);
            Check(game.Assets.LoadedPrefabCount==baseline,"Failed content releases all run prefab leases "+id);
            yield return Capture("load-failure-"+id.Replace('/','-'));
        }
        void Place(Vector3 position)
        {
            var controller=game.Player.GetComponent<CharacterController>();controller.enabled=false;game.Player.transform.position=position+Vector3.up*.15f;controller.enabled=true;
        }
        void ClearEnemies()
        {
            foreach(var enemy in game.Combat.Enemies.ToArray())if(enemy!=game.Combat.Boss)game.Pool.Return(enemy.gameObject);
            game.Combat.Enemies.RemoveAll(e=>e!=game.Combat.Boss);
        }
        IEnumerator Scenarios(int biome)
        {
            if(biome==0)
            {
                ClearEnemies();Place(Vector3.zero);yield return AdvanceGameplay(.8f);game.Player.Heal(10000);
                float health=game.Player.Health;Vector3 startPosition=game.Player.transform.position;
                game.Inputs.ForVerification(Vector2.up,true);yield return null;yield return null;
                Check(game.Player.Invulnerable,"Shift dodge opens invulnerability window");
                game.Player.Hurt(9);Check(Mathf.Approximately(game.Player.Health,health),"Damage ignored inside dodge window");
                yield return AdvanceGameplay(.75f);game.Inputs.ForVerification(Vector2.zero);
                Check(!game.Player.Invulnerable,"Dodge invulnerability expires");
                game.Player.Hurt(9);Check(game.Player.Health<health,"Damage applies outside dodge window");
                Check(Vector3.Distance(startPosition,game.Player.transform.position)>3,"Dodge moves actual CharacterController");
                var drop=game.Player.transform.position+Vector3.right*12+Vector3.up*.4f;var merged=game.Combat.DropExperience(drop,2);
                for(int i=0;i<99;i++)game.Combat.DropExperience(drop,2);
                Check(merged!=null&&merged.Amount==200,"Nearby experience drops merge without losing their full value");
                ClearEnemies();var marked=game.Combat.Spawn(0,game.Player.transform.position+Vector3.forward*5);
                if(marked!=null)
                {
                    marked.DiagnosticImmortal=true;Physics.SyncTransforms();Vector3 aim=marked.transform.position+Vector3.up*.7f;
                    game.Combat.SelectPriorityFromRay(new Ray(aim+Vector3.up*10,Vector3.down));Check(game.Combat.Marked==marked,"World ray selects the priority target without desktop input");
                    game.Combat.SelectPriorityFromRay(new Ray(game.Player.transform.position+Vector3.up*10+Vector3.right*10,Vector3.up));Check(game.Combat.Marked==null,"Empty-world priority marking cancels the target");
                    game.Combat.SelectPriorityTarget(marked);marked.DiagnosticImmortal=false;marked.Hit(new DamagePacket{damage=100000,source=AbilityKind.Gun});Check(game.Combat.Marked==null,"Defeated target clears the priority marker");
                }
                yield return Sequence("character-move-dodge",6,true);
                ClearEnemies();Place(Vector3.zero);yield return Wait(.3f);
                foreach(var site in game.World.Sites)if(site.type==0){game.World.CompleteEvent(site.id);break;}
                Place(new Vector3(640,0,0));yield return AutoBattle(3);
                Check(game.World.OriginChunk.x!=0,"Floating origin activates beyond 500 units");
                Check(game.World.ActiveChunks<=49,"Resident world chunks remain bounded after long travel");
                yield return Capture("world-floating-origin");
                Place(-new Vector3(game.World.OriginChunk.x*32,0,game.World.OriginChunk.y*32));yield return AutoBattle(3);
                bool completed=false;foreach(var site in game.World.Sites)if(site.type==0&&site.complete)completed=true;
                Check(completed,"Completed event survives unload and revisit");
                game.UI.TogglePause();yield return Capture("05-pause");game.UI.CloseModal();
                foreach(var resolution in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(1920,1200),new Vector2Int(2560,1080)})
                {
                    Screen.SetResolution(resolution.x,resolution.y,false);yield return Wait(.4f);
                    game.UI.ToggleSkills();yield return Capture("resolution-skills-"+resolution.x+"x"+resolution.y,resolution.x,resolution.y);game.UI.CloseModal();
                    yield return Capture("resolution-hud-"+resolution.x+"x"+resolution.y,resolution.x,resolution.y);
                }
                Screen.SetResolution(1600,900,false);yield return Wait(.4f);
            }
            // Representative geography is captured from an actual neighboring chunk, not a showcase scene.
            Vector3 terrainCenter=game.World.LocalPosition(game.World.CurrentChunk);
            Place(terrainCenter+new Vector3(-3,0,5));yield return AutoBattle(1);yield return Capture("terrain-detail-"+biome);
            if(biome<4)yield return TerrainVariants(biome);
            if(biome<4)
            {
                yield return SanctuaryEvidence(biome);
                ClearEnemies();Vector3 center=game.Player.transform.position+Vector3.forward*8;
                string bossId=game.Catalog.biomes[biome].bossId;
                game.Combat.ActivateBoss("verification-boss-"+biome,center,(BiomeId)biome,bossId);
                var boss=game.Combat.Boss;Check(boss!=null,"Voluntary boss activation "+biome);
                if(boss!=null)
                {
                    boss.DiagnosticImmortal=true;
                    if(biome==0)
                    {
                        boss.Hit(new DamagePacket{damage=25,source=AbilityKind.Gun});Place(center+Vector3.back*35);yield return AutoBattle(2.5f);
                        Check(game.Combat.Boss==null,"Boss disengages after exit buffer");
                        Place(center+Vector3.back*8);game.Combat.ActivateBoss("verification-boss-"+biome,center,(BiomeId)biome,bossId);
                        boss=game.Combat.Boss;Check(boss!=null&&Mathf.Approximately(boss.Health,boss.MaxHealth),"Reactivated boss restores full health");
                        if(boss!=null)boss.DiagnosticImmortal=true;
                    }
                    if(boss!=null)
                    {
                        yield return ModelCloseup(boss,"boss-closeup-"+biome);
                        boss.DiagnosticHealth(.65f);yield return AutoBattle(.6f);yield return Capture("boss-"+biome);
                        yield return Sequence("boss-attacks-"+biome,12,false);
                        boss.DiagnosticImmortal=false;boss.Hit(new DamagePacket{damage=10000000,source=AbilityKind.Sword});
                        game.UI.View.victory.SetActive(false);yield return RealtimeSequence("boss-death-"+biome,1.35f);game.UI.View.victory.SetActive(true);
                        yield return Capture("victory-"+biome);Check(game.Save.Profile.bossWins[biome],"Boss first victory persists "+biome);
                        Check(game.Save.MapUnlocked((BiomeId)(biome==3?4:biome+1)),"First victory unlocks next map "+biome);
                        game.UI.CloseModal();
                    }
                }
            }
            else
            {
                Vector2 restore=game.World.GlobalPosition;
                for(int step=0;step<36;step++)
                {
                    if(!game.Ready){Check(false,"Run remains active through rapid travel step "+step);yield break;}
                    Place(game.World.LocalPosition(new Vector2Int(step%2==0?step+4:-step-4,step%5)));
                    game.Player.Heal(10000);yield return null;
                }
                yield return AutoBattle(2);
                Check(game.Ready&&string.IsNullOrEmpty(game.World?.LoadingError),"Rapid mixed-biome travel retains regions with pending AA loads");
                if(!game.Ready)yield break;
                Place(new Vector3(restore.x-game.World.OriginChunk.x*32,0,restore.y-game.World.OriginChunk.y*32));yield return AutoBattle(2);
                int captured=0;var pairs=new System.Collections.Generic.HashSet<string>();
                for(int x=-12;x<13&&captured<4;x++)for(int z=-8;z<9&&captured<4;z++)
                {
                    var key=new Vector2Int(x,z);int a=(int)game.World.BiomeAt(key),b=(int)game.World.BiomeAt(key+Vector2Int.right);if(a==b)continue;
                    string pair=Mathf.Min(a,b)+"-"+Mathf.Max(a,b);if(!pairs.Add(pair))continue;
                    Place(game.World.LocalPosition(key)+Vector3.right*15);yield return AutoBattle(2);yield return Capture("mixed-boundary-"+pair);captured++;
                }
                Check(captured==4,"Mixed map captures four distinct neighboring biome pairs");
                bool shore=false;
                for(int x=-12;x<=12&&!shore;x++)for(int z=-8;z<=8&&!shore;z++)
                {
                    var key=new Vector2Int(x,z);if(!game.World.HasWatercourse(key)||game.World.BiomeAt(key)!=BiomeId.Volcano||game.World.BiomeAt(key+Vector2Int.right)==BiomeId.Volcano)continue;
                    Place(game.World.LocalPosition(key)+new Vector3(9,0,5));yield return AutoBattle(2);yield return Capture("mixed-lava-shore");CameraRig.Active.DiagnosticZoom(9.5f);yield return Wait(.2f);yield return Capture("mixed-lava-shore-close");CameraRig.Active.DiagnosticZoom(13.5f);shore=true;
                }
                Check(shore,"Capture actual lava watercourse terminus at a mixed-biome boundary");
            }
            ClearEnemies();game.Run.seconds=1500;game.Player.Heal(10000);
            for(int i=0;i<200;i++)
            {
                float a=i*2.399f;float r=7+i%18;game.Combat.Spawn(i%4,game.Player.transform.position+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*r,i%13==0);
            }
            yield return AutoBattle(.4f);yield return Capture("late-density-"+biome);
            if(biome==4)yield return Sequence("late-density-combinations",8,true);
        }
        IEnumerator Sequence(string name,int seconds,bool movement)
        {
            string folder=Path.Combine(output,"Sequences",name);Directory.CreateDirectory(folder);
            int previous=Time.captureFramerate;Time.captureFramerate=12;
            for(int frame=0;frame<seconds*12;frame++)
            {
                if(!game.Ready){Check(false,"Run remains active during sequence "+name+" frame "+frame);break;}
                if(game.Player!=null)game.Player.Heal(10000);
                if(game.UI.View.upgrade.activeSelf){var choices=game.Progression.Choices();if(choices.Count>0)game.Progression.Apply(choices[0].id,false);else game.Progression.SkipLevel();}
                if(name.StartsWith("role-closeup"))
                {
                    game.Inputs.ForVerification(Vector2.zero);Vector3 center=game.Player.transform.position+Vector3.up*1.4f;
                    Camera.main.transform.position=center+new Vector3(8,5,10);Camera.main.transform.LookAt(center);
                }
                else if(movement)game.Inputs.ForVerification(new Vector2(Mathf.Sin(frame*.035f),Mathf.Cos(frame*.035f))*.75f,frame%36==0);
                else game.Inputs.ForVerification(new Vector2(frame/40%2==0?1:-1,0)*.1f,frame%40==0);
                RenderCapture(Path.Combine(folder,frame.ToString("D4")+".jpg"),1280,720,true);
                yield return null;
            }
            game.Inputs.ForVerification(Vector2.zero);Time.captureFramerate=previous;
            File.WriteAllText(Path.Combine(folder,"metadata.json"),"{\"source\":\"development Player Camera.Render with live UI\",\"fixedSimulationFps\":12,\"frames\":"+(seconds*12)+",\"controlledPlayerHealing\":true,\"enemyImmortalForRoleCapture\":"+(name.StartsWith("role")?"true":"false")+",\"enemyStationaryForRoleCapture\":"+(name.StartsWith("role")?"true":"false")+",\"bossImmortalForAnimationCapture\":"+(name.StartsWith("boss")?"true":"false")+"}");
        }
        IEnumerator AdvanceGameplay(float seconds)
        {
            float end=Time.time+seconds,deadline=Time.realtimeSinceStartup+15;
            while(Time.time<end&&Time.realtimeSinceStartup<deadline)
            {
                if(game.UI.View.upgrade.activeSelf){var options=game.Progression.Choices();if(options.Count>0)game.Progression.Apply(options[0].id,false);else game.Progression.SkipLevel();}
                yield return null;
            }
            Check(Time.time>=end,"Gameplay clock advanced for timed action assertion");
        }
    }
}

#endif
