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
        IEnumerator LoadingFailureProbe(string id)
        {
            int baseline=game.Assets.LoadedPrefabCount;
            Check(game.Assets.DiagnosticFailNextPrefab(id),"Arm controlled AA failure "+id);
            game.BeginRun("adventurer",BiomeId.Forest);
            float deadline=Time.realtimeSinceStartup+90;
            while(!game.CanBeginRun&&Time.realtimeSinceStartup<deadline)yield return null;
            yield return Wait(1);
            Check(game.CanBeginRun&&game.Player==null&&game.World==null,"Failed content returns safely to ready menu "+id);
            Check(game.Assets.LoadedPrefabCount==baseline,"Failed content releases all run prefab leases "+id);
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
            if(biome<4)
            {
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
                        game.UI.View.victory.SetActive(false);yield return RealtimeSequence("boss-death-"+biome,.65f);game.UI.View.victory.SetActive(true);
                        yield return Capture("victory-"+biome);Check(game.Save.Profile.bossWins[biome],"Boss first victory persists "+biome);
                        Check(game.Save.MapUnlocked((BiomeId)(biome==3?4:biome+1)),"First victory unlocks next map "+biome);
                        game.UI.CloseModal();
                    }
                }
            }
            else
            {
                int captured=0;var pairs=new System.Collections.Generic.HashSet<string>();
                for(int x=-12;x<13&&captured<4;x++)for(int z=-8;z<9&&captured<4;z++)
                {
                    var key=new Vector2Int(x,z);int a=(int)game.World.BiomeAt(key),b=(int)game.World.BiomeAt(key+Vector2Int.right);if(a==b)continue;
                    string pair=Mathf.Min(a,b)+"-"+Mathf.Max(a,b);if(!pairs.Add(pair))continue;
                    Place(game.World.LocalPosition(key)+Vector3.right*15);yield return AutoBattle(2);yield return Capture("mixed-boundary-"+pair);captured++;
                }
                Check(captured==4,"Mixed map captures four distinct neighboring biome pairs");
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
                if(game.Player!=null)game.Player.Heal(10000);
                if(game.UI.View.upgrade.activeSelf){var choices=game.Progression.Choices();if(choices.Count>0)game.Progression.Apply(choices[0].id,false);else game.Progression.SkipLevel();}
                if(movement)game.Inputs.ForVerification(new Vector2(Mathf.Sin(frame*.035f),Mathf.Cos(frame*.035f))*.75f,frame%36==0);
                else game.Inputs.ForVerification(new Vector2(frame/40%2==0?1:-1,0)*.1f,frame%40==0);
                RenderCapture(Path.Combine(folder,frame.ToString("D4")+".jpg"),1280,720,true);
                yield return null;
            }
            game.Inputs.ForVerification(Vector2.zero);Time.captureFramerate=previous;
            File.WriteAllText(Path.Combine(folder,"metadata.json"),"{\"source\":\"development Player Camera.Render with live UI\",\"fixedSimulationFps\":12,\"frames\":"+(seconds*12)+",\"controlledPlayerHealing\":true,\"bossImmortalForAnimationCapture\":"+(name.StartsWith("boss")?"true":"false")+"}");
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
