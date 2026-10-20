using UnityEditor;
using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        public static void Blob(Transform parent,float radius)
        {
            const string path="Assets/Art/Models/Generated/contact-shadow.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null)
            {
                var vertices=new System.Collections.Generic.List<Vector3>{Vector3.zero};var colors=new System.Collections.Generic.List<Color>{Color.white};var indices=new System.Collections.Generic.List<int>();
                for(int ring=0;ring<3;ring++)for(int i=0;i<32;i++){float a=i*Mathf.PI*2/32;float r=new[]{.35f,.68f,1f}[ring];vertices.Add(new Vector3(Mathf.Sin(a)*r,0,Mathf.Cos(a)*r));colors.Add(new Color(1,1,1,new[]{.85f,.35f,0f}[ring]));}
                for(int i=0;i<32;i++){indices.Add(0);indices.Add(1+i);indices.Add(1+(i+1)%32);}
                for(int ring=0;ring<2;ring++)for(int i=0;i<32;i++){int a=1+ring*32+i,b=1+ring*32+(i+1)%32;indices.Add(a);indices.Add(a+32);indices.Add(b);indices.Add(b);indices.Add(a+32);indices.Add(b+32);}
                mesh=new Mesh{name="Contact shadow"};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);
            }
            const string materialPath="Assets/Art/Materials/ContactShadow.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null){material=new Material(Shader.Find("VampireLike/ContactShadow"));AssetDatabase.CreateAsset(material,materialPath);}
            var go=Root("ContactShadow",parent);go.transform.localPosition=Vector3.up*.035f;go.transform.localScale=new Vector3(radius,1,radius*.8f);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
        }
    }
}
