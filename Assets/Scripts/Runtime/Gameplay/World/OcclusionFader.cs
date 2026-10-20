using UnityEngine;

namespace VampireLike
{
    public sealed class OcclusionFader : MonoBehaviour
    {
        Renderer[] renderers;
        MaterialPropertyBlock block;
        float visibility=1,next;
        void Awake(){renderers=GetComponentsInChildren<Renderer>(true);block=new MaterialPropertyBlock();}
        void OnEnable(){visibility=1;next=0;}
        void LateUpdate()
        {
            if(Time.unscaledTime<next)return;next=Time.unscaledTime+.05f;
            var game=GameManager.Instance;var cam=Camera.main;
            bool fade=false;
            if(game!=null&&game.Ready&&cam!=null&&(transform.position-game.Player.transform.position).sqrMagnitude<160)
            {
                Vector3 player=cam.WorldToViewportPoint(game.Player.transform.position+Vector3.up*1.1f);
                foreach(var renderer in renderers)
                {
                    if(!renderer.gameObject.activeInHierarchy||renderer.name=="ContactShadow")continue;
                    var bounds=renderer.bounds;Vector3 c=cam.WorldToViewportPoint(bounds.center);
                    if(c.z>player.z+1.5f)continue;
                    Vector2 min=Vector2.one*100,max=Vector2.one*-100;
                    for(int corner=0;corner<8;corner++)
                    {
                        Vector3 v=bounds.center+Vector3.Scale(bounds.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1));
                        Vector3 screen=cam.WorldToViewportPoint(v);min=Vector2.Min(min,screen);max=Vector2.Max(max,screen);
                    }
                    if(player.x>=min.x-.015f&&player.x<=max.x+.015f&&player.y>=min.y-.025f&&player.y<=max.y+.025f){fade=true;break;}
                }
            }
            visibility=Mathf.MoveTowards(visibility,fade?.26f:1,.22f);
            foreach(var renderer in renderers){renderer.GetPropertyBlock(block);block.SetFloat("_Visibility",visibility);renderer.SetPropertyBlock(block);}
        }
    }
}
