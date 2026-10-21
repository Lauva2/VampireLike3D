using UnityEditor;
using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        static Material EffectMaterial()
        {
            const string path="Assets/Art/Materials/Generated/CartoonEffect.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("VampireLike/CartoonEffect"));AssetDatabase.CreateAsset(material,path);}material.enableInstancing=true;EditorUtility.SetDirty(material);return material;
        }
        static Material IndicatorMaterial(bool player)
        {
            string path="Assets/Art/Materials/Generated/"+(player?"PlayerBeacon":"DangerIndicator")+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(EffectMaterial());AssetDatabase.CreateAsset(material,path);}
            material.SetFloat("_ZTest",(float)UnityEngine.Rendering.CompareFunction.Always);
            material.SetFloat("_Readability",0);
            material.renderQueue=player?3150:3120;material.enableInstancing=true;EditorUtility.SetDirty(material);return material;
        }
        static void ProjectileTrail(GameObject root,AbilityKind kind)
        {
            const string path="Assets/Art/Materials/Generated/ProjectileTrail.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("VampireLike/ProjectileTrail"));AssetDatabase.CreateAsset(material,path);}
            var trail=root.AddComponent<TrailRenderer>();trail.sharedMaterial=material;trail.time=kind==AbilityKind.Gun?.08f:.11f;trail.minVertexDistance=.20f;
            trail.startWidth=kind==AbilityKind.Gun?.055f:kind==AbilityKind.Sword?.09f:.13f;trail.endWidth=0;trail.numCapVertices=2;trail.numCornerVertices=1;
            Color color=kind==AbilityKind.Sword?Hex("71D9DC"):kind==AbilityKind.Fireball||kind==AbilityKind.Cannon?Hex("FFB762"):kind==AbilityKind.Spirit?Hex("92E4CB"):Gold;
            color.a=.42f;trail.startColor=color;color.a=0;trail.endColor=color;trail.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;trail.receiveShadows=false;
        }
    }
}
