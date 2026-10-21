using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class TransientEffect : MonoBehaviour
    {
        PoolService pool;
        float end, begin, size;
        bool unscaled;
        Vector3 baseScale;
        float Clock=>unscaled?Time.unscaledTime:Time.time;
        Renderer[] renderers;
        MaterialPropertyBlock block;
        void Awake(){renderers=GetComponentsInChildren<Renderer>();block=new MaterialPropertyBlock();}
        public void Initialize(PoolService owner, float duration, float scale,bool useUnscaled=false,bool hostile=false)
        {
            pool = owner;unscaled=useUnscaled; begin = Clock; end = begin + duration; size = scale;
            baseScale=new Vector3(scale,Mathf.Min(scale,2),scale);transform.localScale=baseScale;
            foreach(var renderer in renderers){renderer.GetPropertyBlock(block);block.SetFloat("_Visibility",1);block.SetFloat("_FxStyle",1);block.SetFloat("_FxProgress",0);block.SetFloat("_Readability",hostile?0:1);block.SetFloat("_Hostile",hostile?1:0);renderer.SetPropertyBlock(block);}
            foreach (var particle in GetComponentsInChildren<ParticleSystem>()) { particle.Clear(); particle.Play(); }
        }
        public void InitializeLink(PoolService owner,float duration,Vector3 start,Vector3 finish)
        {
            Initialize(owner,duration,1);transform.position=start;transform.rotation=Quaternion.LookRotation(finish-start);baseScale=new Vector3(.5f,.5f,Vector3.Distance(start,finish));transform.localScale=baseScale;
        }
        void Update()
        {
            if (pool == null) return;
            if (Clock >= end) { pool.Return(gameObject); return; }
            float t = (Clock - begin) / Mathf.Max(.01f, end - begin);
            transform.localScale=baseScale;
            foreach(var renderer in renderers){renderer.GetPropertyBlock(block);block.SetFloat("_FxProgress",t);block.SetFloat("_Visibility",1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,1,t)));renderer.SetPropertyBlock(block);}
        }
    }
}
