using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class TransientEffect : MonoBehaviour
    {
        PoolService pool;
        float end, begin, size;
        Renderer[] renderers;
        MaterialPropertyBlock block;
        void Awake(){renderers=GetComponentsInChildren<Renderer>();block=new MaterialPropertyBlock();}
        public void Initialize(PoolService owner, float duration, float scale)
        {
            pool = owner; begin = Time.time; end = begin + duration; size = scale;
            transform.localScale = new Vector3(scale,Mathf.Min(scale,2),scale);
            foreach(var renderer in renderers){renderer.GetPropertyBlock(block);block.SetFloat("_Visibility",1);renderer.SetPropertyBlock(block);}
            foreach (var particle in GetComponentsInChildren<ParticleSystem>()) { particle.Clear(); particle.Play(); }
        }
        void Update()
        {
            if (pool == null) return;
            if (Time.time >= end) { pool.Return(gameObject); return; }
            float t = (Time.time - begin) / Mathf.Max(.01f, end - begin);
            transform.localScale = new Vector3(size,Mathf.Min(size,2),size) * (.5f + t * .8f);
            foreach(var renderer in renderers){renderer.GetPropertyBlock(block);block.SetFloat("_Visibility",1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,1,t)));renderer.SetPropertyBlock(block);}
            transform.Rotate(0, Time.deltaTime * 80, 0);
        }
    }
}
