using System.Collections;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class VerificationRunner
    {
        IEnumerator EnemyGallery()
        {
            game.Pause("gallery",true);
            for(int type=0;type<4;type++)for(int elite=0;elite<2;elite++)
            {
                var enemy=game.Combat.Spawn(type,game.Player.transform.position+Vector3.forward*5,elite==1);
                if(enemy==null){Check(false,"Enemy gallery spawn "+type);continue;}
                yield return ModelCloseup(enemy,"enemy-"+type+(elite==1?"-elite":"-normal"));
                game.Combat.Enemies.Remove(enemy);game.Pool.Return(enemy.gameObject);
            }
            game.Pause("gallery",false);
        }
        IEnumerator ModelCloseup(EnemyActor actor,string name)
        {
            var rig=CameraRig.Active;var camera=Camera.main;Vector3 position=camera.transform.position;Quaternion rotation=camera.transform.rotation;float size=camera.orthographicSize;
            rig.enabled=false;game.Pause("model-evidence",true);game.UI.View.hud.SetActive(false);
            bool bar=actor.barRoot!=null&&actor.barRoot.gameObject.activeSelf;if(actor.barRoot!=null)actor.barRoot.gameObject.SetActive(false);
            Vector3 center=actor.transform.position+Vector3.up*1.8f;
            camera.transform.position=center+new Vector3(6,4,8);camera.transform.LookAt(center);camera.orthographicSize=3.9f;
            yield return Capture(name,1600,1200);
            if(actor.barRoot!=null)actor.barRoot.gameObject.SetActive(bar);
            camera.transform.SetPositionAndRotation(position,rotation);camera.orthographicSize=size;rig.enabled=true;game.UI.View.hud.SetActive(true);game.Pause("model-evidence",false);
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
            File.WriteAllText(Path.Combine(folder,"metadata.json"),"{\"source\":\"development Player actual runtime death animation\",\"wallClockTimestamps\":true,\"frames\":"+frame+",\"requestedIntervalSeconds\":0.03333,\"fixedSimulationFps\":null}");
        }
        IEnumerator DefeatEvidence()
        {
            ClearEnemies();game.Inputs.ForVerification(Vector2.zero);yield return Wait(.8f);
            game.Player.Hurt(100000);yield return RealtimeSequence("player-defeat",.85f);
            Check(game.Flow==GameFlow.Settling&&game.UI.View.results.activeSelf,"Player defeat animation reaches death settlement");
            yield return Capture("death-results");
        }
    }
}
