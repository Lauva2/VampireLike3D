using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class AttackHazard : MonoBehaviour
    {
        public bool BossOwned { get; private set; }
        GameManager game;
        float activate, expires, radius, damage, nextHit, began;
        float halfLength,nextEffect;
        bool line;
        BiomeId biome;
        public Transform fill;
        Renderer[] renderers;MaterialPropertyBlock block;
        void Awake(){renderers=GetComponentsInChildren<Renderer>();block=new MaterialPropertyBlock();}
        public void Initialize(GameManager manager, float range, float delay, float active, float power, bool boss)
        {
            game = manager; radius = range; damage = power; began=Time.time;activate = Time.time + delay; expires = activate + active; nextHit = 0; BossOwned = boss;
            line=false;halfLength=0;nextEffect=activate;biome=boss&&game.Combat.Boss!=null?game.Combat.Boss.Biome:game.World.CurrentBiome;
            transform.localScale = new Vector3(radius * 2, 1, radius * 2);
            if (fill != null) fill.localScale = Vector3.one * .05f;
            foreach(var renderer in renderers){renderer.GetPropertyBlock(block);block.SetFloat("_Visibility",1);block.SetFloat("_Readability",0);block.SetFloat("_Hostile",0);block.SetFloat("_Hazard",1);renderer.SetPropertyBlock(block);}
        }
        public void InitializeLine(GameManager manager,Vector3 start,Vector3 end,float width,float delay,float active,float power,bool boss)
        {
            Initialize(manager,width,delay,active,power,boss);line=true;halfLength=Vector3.Distance(start,end)*.5f;transform.position=(start+end)*.5f+Vector3.up*.05f;transform.rotation=Quaternion.LookRotation(end-start);transform.localScale=new Vector3(width*2,1,halfLength*2);
        }
        public bool Contains(Vector3 position,float margin=0)
        {
            Vector3 delta=position-transform.position;delta.y=0;
            return line?Mathf.Abs(Vector3.Dot(delta,transform.forward))<halfLength+margin&&Mathf.Abs(Vector3.Dot(delta,transform.right))<radius+margin:delta.sqrMagnitude<(radius+margin)*(radius+margin);
        }
        void Update()
        {
            if (game == null || !game.Ready || game.IsPaused) return;
            if (Time.time >= expires) { game.Pool.Return(gameObject); return; }
            bool active = Time.time >= activate;
            float anticipation=Mathf.Clamp01((Time.time-began)/Mathf.Max(.01f,activate-began));
            float visibility=active?1:Mathf.Lerp(.28f,.95f,anticipation*anticipation*anticipation);
            foreach(var renderer in renderers)
            {
                renderer.GetPropertyBlock(block);
                block.SetFloat("_Visibility",visibility*(renderer.transform==fill?(active?.58f:.32f):1));
                block.SetFloat("_DangerActive",active?1:anticipation*anticipation);
                renderer.SetPropertyBlock(block);
            }
            if (fill != null) fill.localScale = new Vector3(1,1,1)*(active ? 1 : Mathf.Lerp(.12f,1,Mathf.Clamp01((Time.time-began)/Mathf.Max(.01f,activate-began))));
            if(active&&Time.time>nextEffect)
            {
                nextEffect=Time.time+.85f;
                string id=BossOwned?(biome==BiomeId.Forest?"vfx/roots":biome==BiomeId.Snow?"vfx/frost":biome==BiomeId.Volcano?"vfx/flame":"vfx/impact"):"vfx/impact";
                game.Combat.Effect(id,transform.position,.45f,Mathf.Min(radius*1.4f,3),hostile:true);
            }
            if (active && Time.time > nextHit && Contains(game.Player.transform.position,.25f))
            { game.Player.Hurt(damage); nextHit = Time.time + .8f; }
        }
    }
}
