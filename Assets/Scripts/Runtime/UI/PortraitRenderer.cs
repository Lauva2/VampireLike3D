using UnityEngine;

namespace VampireLike
{
    public static class PortraitRenderer
    {
        public static Texture2D Create(GameObject prefab)
        {
            var stage = new GameObject("Portrait studio"); stage.transform.position = new Vector3(10000,10000,10000);
            var lightRoot=new GameObject("Studio light",typeof(Light));lightRoot.transform.rotation=Quaternion.Euler(35,150,0);var light=lightRoot.GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.cullingMask=1<<12;light.color=new Color(1,.95f,.86f);
            var actor = Object.Instantiate(prefab,stage.transform,false);
            foreach(var behaviour in actor.GetComponentsInChildren<MonoBehaviour>()) behaviour.enabled=false;
            foreach(var collider in actor.GetComponentsInChildren<Collider>()) collider.enabled=false;
            foreach(var t in actor.GetComponentsInChildren<Transform>(true))t.gameObject.layer=12;
            var cameraRoot = new GameObject("Portrait camera",typeof(Camera)); var camera=cameraRoot.GetComponent<Camera>();
            camera.enabled=false;camera.cullingMask=1<<12;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
            camera.orthographic=true;camera.orthographicSize=.87f;camera.nearClipPlane=.1f;camera.farClipPlane=20;
            camera.transform.position=stage.transform.position+new Vector3(.15f,2.15f,4.5f);camera.transform.LookAt(stage.transform.position+new Vector3(0,1.85f,0));
            var rt=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);rt.Create();var previous=RenderTexture.active;
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();
            RenderTexture.active=previous;camera.targetTexture=null;rt.Release();Object.Destroy(rt);Object.Destroy(stage);Object.Destroy(cameraRoot);Object.Destroy(lightRoot);
            return image;
        }
        public static Texture2D Landscape(GameObject terrain,GameObject decoration,GameObject architecture,GameObject pathPrefab,BiomeId biome)
        {
            var stage=new GameObject("Landscape studio");stage.transform.position=new Vector3(20000,10000,20000);
            for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){var tile=Object.Instantiate(terrain,stage.transform,false);tile.transform.localPosition=new Vector3(x*32,0,z*32);}
            Physics.SyncTransforms();
            if(pathPrefab!=null)for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++){var paths=Object.Instantiate(pathPrefab,stage.transform,false);paths.transform.localPosition=new Vector3(x*32,0,z*32);paths.GetComponent<PathNetwork>().Configure(71,new Vector2Int(x,z),15,biome,new[]{biome,biome,biome,biome},biome==BiomeId.Forest||biome==BiomeId.Volcano,true,true,true);}
            for(int i=0;i<8;i++){var tree=Object.Instantiate(decoration,stage.transform,false);tree.transform.localPosition=new Vector3(Mathf.Sin(i*2.4f)*10,0,Mathf.Cos(i*2.4f)*9);tree.GetComponent<DecorationVariant>()?.Select(i%4==0?1:0);}
            if(architecture!=null){var landmark=Object.Instantiate(architecture,stage.transform,false);landmark.transform.localPosition=new Vector3(5,0,-3);}
            foreach(var behaviour in stage.GetComponentsInChildren<MonoBehaviour>())behaviour.enabled=false;
            foreach(var t in stage.GetComponentsInChildren<Transform>(true))t.gameObject.layer=12;
            var lightRoot=new GameObject("Landscape light",typeof(Light));lightRoot.transform.rotation=Quaternion.Euler(48,-35,0);var light=lightRoot.GetComponent<Light>();light.type=LightType.Directional;light.cullingMask=1<<12;light.intensity=1.1f;
            var cameraRoot=new GameObject("Landscape camera",typeof(Camera));var camera=cameraRoot.GetComponent<Camera>();camera.enabled=false;camera.cullingMask=1<<12;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.75f,.83f,.72f);camera.orthographic=true;camera.orthographicSize=11.5f;camera.farClipPlane=100;camera.transform.position=stage.transform.position+new Vector3(11,19,-18);camera.transform.LookAt(stage.transform.position);
            var rt=new RenderTexture(384,160,24,RenderTextureFormat.ARGB32);rt.Create();var previous=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(384,160,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,384,160),0,0);image.Apply();
            RenderTexture.active=previous;camera.targetTexture=null;rt.Release();Object.Destroy(rt);Object.Destroy(stage);Object.Destroy(cameraRoot);Object.Destroy(lightRoot);return image;
        }
    }
}
