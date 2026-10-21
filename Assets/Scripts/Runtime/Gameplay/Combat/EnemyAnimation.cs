using System.Collections;
using UnityEngine;

namespace VampireLike
{
    public sealed partial class EnemyActor
    {
        float windup;
        Renderer heartRenderer;
        public string AttackPhase
        {
            get{float t=Time.time-attackAnimation;return t<0||t>windup+.65f?"idle":t<windup?"windup":t<windup+.18f?"strike":"recover";}
        }
        void AnimateIntent(float phase)
        {
            float t=Time.time-attackAnimation;
            if(t<0||t>windup+.65f)return;
            float anticipation=Mathf.SmoothStep(0,1,t/Mathf.Max(.01f,windup));
            float recover=Mathf.SmoothStep(0,1,(t-windup)/.65f);
            float amount=t<windup?anticipation:1-recover;
            if(Type==4)
            {
                float slam=t<windup?-112*anticipation:Mathf.Lerp(38,0,recover);
                if(leftArm!=null)leftArm.localRotation=Quaternion.Euler(slam,0,-10-amount*12);
                if(rightArm!=null)rightArm.localRotation=Quaternion.Euler(slam,0,10+amount*12);
                if(Biome==BiomeId.Desert)
                {
                    visual.localPosition=Vector3.up*(t<windup?-.48f*anticipation:.16f*Mathf.Sin(recover*Mathf.PI));
                    visual.localRotation=Quaternion.Euler(t<windup?12*anticipation:-14*(1-recover),0,0);
                    visual.localScale=new Vector3(1+amount*.12f,1-amount*.18f,1+amount*.10f);
                }
                else if(Biome==BiomeId.Snow)
                {
                    visual.localScale=new Vector3(1+amount*.07f,1-amount*.12f,1+amount*.07f);
                    visual.localPosition=Vector3.up*(t<windup?Mathf.Sin(anticipation*Mathf.PI)*.55f:0);
                    visual.localRotation=Quaternion.Euler(t<windup?-14*anticipation:18*(1-recover),0,0);
                }
                else visual.localRotation=Quaternion.Euler(t<windup?-13*anticipation:16*(1-recover),0,Mathf.Sin(phase)*1.5f);
                if(heartRenderer!=null){heartRenderer.GetPropertyBlock(block);block.SetFloat("_Emission",.12f+amount*.85f);heartRenderer.SetPropertyBlock(block);}
            }
            else if(Type==2)visual.localScale=new Vector3(1+amount*.12f,1-amount*.18f,1+amount*.1f);
            else if(Type==3){visual.localScale=new Vector3(1+amount*.08f,1-amount*.1f,1+amount*.08f);visual.localRotation=Quaternion.Euler(-amount*14,0,Mathf.Sin(phase)*3);}
        }
        IEnumerator Death()
        {
            if(barRoot!=null)barRoot.gameObject.SetActive(false);if(eliteHalo!=null)eliteHalo.gameObject.SetActive(false);
            bool boss=Type==4;float duration=boss?1.05f:.32f;
            if(boss)game.Combat.Effect(Biome==BiomeId.Forest?"vfx/leafburst":Biome==BiomeId.Desert?"vfx/sand":Biome==BiomeId.Snow?"vfx/iceburst":"vfx/embers",transform.position,duration,2.2f,true);
            float start=Time.unscaledTime;
            while(Time.unscaledTime-start<duration)
            {
                float t=Mathf.Clamp01((Time.unscaledTime-start)/duration),ease=Mathf.SmoothStep(0,1,t);
                if(boss&&Biome==BiomeId.Desert){visual.localPosition=Vector3.down*ease*3.0f;visual.localRotation=Quaternion.Euler(20*ease,0,0);visual.localScale=new Vector3(1,1,1-t*.25f);}
                else if(boss&&Biome==BiomeId.Forest){visual.localRotation=Quaternion.Euler(0,0,-ease*78);visual.localPosition=new Vector3(ease*.6f,-ease*.22f,0);visual.localScale=Vector3.one*(1-t*.15f);}
                else if(boss&&Biome==BiomeId.Snow){visual.localScale=new Vector3(1+ease*.15f,1-ease*.60f,1+ease*.15f);visual.localRotation=Quaternion.Euler(ease*22,0,0);}
                else if(boss){visual.localPosition=Vector3.down*ease*.7f;visual.localScale=new Vector3(1+ease*.12f,1-ease*.55f,1+ease*.12f);if(leftArm!=null)leftArm.localRotation=Quaternion.Euler(ease*55,0,-ease*30);if(rightArm!=null)rightArm.localRotation=Quaternion.Euler(ease*55,0,ease*30);}
                else{visual.localScale=new Vector3(1+t*.3f,1-t*.85f,1+t*.1f);visual.localRotation=Quaternion.Euler(Type==1?t*90:0,0,Type==2?t*75:0);visual.localPosition=Vector3.up*Mathf.Sin(t*Mathf.PI)*.22f;}
                foreach(var renderer in renderers){renderer.GetPropertyBlock(block);block.SetFloat("_Visibility",1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(boss?.60f:.4f,1,t)));block.SetFloat("_Emission",0);block.SetFloat("_Flash",0);if(boss&&Biome==BiomeId.Volcano)block.SetColor("_Tint",Color.Lerp(Color.white,new Color(.25f,.27f,.32f),ease));renderer.SetPropertyBlock(block);}
                yield return null;
            }
            if(game.Pool!=null)game.Pool.Return(gameObject);
        }
    }
}
