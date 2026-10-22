using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        public static readonly Color Teal = Hex("268E8B"), Cream = Hex("FFF0CF"), Orange = Hex("F39D42"), Ink = Hex("293C45"), Brown = Hex("784B32"), Skin = Hex("FFC796"), Gold = Hex("E3B859");
        static Material master;
        static Mesh smallSphere,largeSphere,roundedCapsule;
        static readonly Dictionary<Color, Material> colors = new Dictionary<Color, Material>();
        public static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var c); return c; }
        public static void Begin()
        {
            Directory.CreateDirectory("Assets/Art/Models/Generated"); Directory.CreateDirectory("Assets/Art/Materials/Generated");
            master = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Cartoon.mat");
            if (master == null) { master = new Material(Shader.Find("VampireLike/CartoonVertex")); AssetDatabase.CreateAsset(master, "Assets/Art/Materials/Cartoon.mat"); }
            master.enableInstancing = true; colors.Clear();
            duneBuilt=false;
        }
        public static GameObject Root(string name, Transform parent = null)
        { var go = new GameObject(name); if (parent != null) go.transform.SetParent(parent, false); return go; }
        static Material Mat(Color color)
        {
            if (colors.TryGetValue(color, out var result)) return result;
            result = new Material(master) { color = color }; colors[color] = result; return result;
        }
        public static GameObject Part(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Vector3 rotation = default)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = scale; go.transform.localEulerAngles = rotation;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            if(type==PrimitiveType.Sphere)
            {
                bool large=Mathf.Max(scale.x,Mathf.Max(scale.y,scale.z))>.65f;
                if(large){if(largeSphere==null)largeSphere=RoundedMesh(20,12,false);go.GetComponent<MeshFilter>().sharedMesh=largeSphere;}
                else{if(smallSphere==null)smallSphere=RoundedMesh(12,8,false);go.GetComponent<MeshFilter>().sharedMesh=smallSphere;}
            }
            else if(type==PrimitiveType.Capsule){if(roundedCapsule==null)roundedCapsule=RoundedMesh(12,8,true);go.GetComponent<MeshFilter>().sharedMesh=roundedCapsule;}
            go.GetComponent<Renderer>().sharedMaterial = Mat(color); return go;
        }
        // Smooth analytic normals preserve the silhouette while small details avoid the built-in mesh's excess rings.
        static Mesh RoundedMesh(int sides,int latitude,bool capsule)
        {
            var v=new List<Vector3>();var n=new List<Vector3>();var indices=new List<int>();
            int rings=latitude+(capsule?1:0);
            for(int row=0;row<=rings;row++)
            {
                int arcRow=capsule&&row>latitude/2?row-1:row;
                float a=-Mathf.PI*.5f+Mathf.PI*arcRow/latitude;
                float y=Mathf.Sin(a)*.5f+(capsule?(row<=latitude/2?-.5f:.5f):0);
                for(int side=0;side<=sides;side++)
                {
                    float angle=side*Mathf.PI*2/sides;var normal=new Vector3(Mathf.Sin(angle)*Mathf.Cos(a),Mathf.Sin(a),Mathf.Cos(angle)*Mathf.Cos(a));
                    v.Add(new Vector3(normal.x*.5f,y,normal.z*.5f));n.Add(normal);
                    if(row==rings||side==sides)continue;
                    int p=row*(sides+1)+side;
                    if(row>0){indices.Add(p);indices.Add(p+1);indices.Add(p+sides+1);}
                    if(row<rings-1){indices.Add(p+1);indices.Add(p+sides+2);indices.Add(p+sides+1);}
                }
            }
            var mesh=new Mesh{name=capsule?"Smooth compact capsule":"Smooth compact sphere"};mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();return mesh;
        }
        static GameObject Sphere(Transform t, string n, Vector3 p, Vector3 s, Color c) => Part(t, n, PrimitiveType.Sphere, p, s, c);
        static GameObject Cube(Transform t, string n, Vector3 p, Vector3 s, Color c, Vector3 r = default) => Part(t, n, PrimitiveType.Cube, p, s, c, r);
        public static GameObject Capsule(Transform t, string n, Vector3 p, Vector3 s, Color c, Vector3 r = default) => Part(t, n, PrimitiveType.Capsule, p, s, c, r);
        public static void Tube(Transform parent, string name, Vector3 a, Vector3 b, float radius, Color c)
        {
            var go = Capsule(parent, name, (a + b) * .5f, new Vector3(radius * 2, Vector3.Distance(a, b) * .5f, radius * 2), c);
            go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, b - a);
        }
        public static GameObject Spike(Transform parent, string name, Vector3 a, Vector3 b, float radius, Color color)
        {
            const int sides = 10; const int rings = 5; var vertices = new List<Vector3>(); var triangles = new List<int>();
            var direction = b - a; var rotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            for (int ring = 0; ring <= rings; ring++)
            {
                float t = ring / (float)rings; float r = radius * Mathf.Pow(1 - t, .7f);
                for (int side = 0; side < sides; side++)
                {
                    float angle = side * Mathf.PI * 2 / sides;
                    var point = new Vector3(Mathf.Cos(angle) * r, t * direction.magnitude, Mathf.Sin(angle) * r * .65f);
                    point.x += Mathf.Sin(t * Mathf.PI) * radius * .65f;
                    vertices.Add(a + rotation * point);
                    if (ring == rings) continue;
                    int i = ring * sides + side, j = ring * sides + (side + 1) % sides;
                    triangles.Add(i); triangles.Add(i + sides); triangles.Add(j); triangles.Add(j); triangles.Add(i + sides); triangles.Add(j + sides);
                }
            }
            var mesh = new Mesh { name = name }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals();
            var go = Root(name, parent); go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterial = Mat(color); return go;
        }
        public static GameObject Ring(Transform parent, string name, float radius, float tube, Color c, float arc = 360)
        {
            int segments = Mathf.Max(8, (int)(arc / 8)); const int cross = 8;
            var vertices = new List<Vector3>(); var indices = new List<int>();
            for (int i = 0; i <= segments; i++)
            {
                float a = i / (float)segments * arc * Mathf.Deg2Rad;
                for (int j = 0; j < cross; j++)
                {
                    float b = j * Mathf.PI * 2 / cross;
                    vertices.Add(new Vector3(Mathf.Cos(a) * (radius + Mathf.Cos(b) * tube), Mathf.Sin(b) * tube, Mathf.Sin(a) * (radius + Mathf.Cos(b) * tube)));
                    if (i == segments) continue;
                    int v = i * cross + j, next = i * cross + (j + 1) % cross;
                    indices.Add(v); indices.Add(next); indices.Add(v + cross); indices.Add(next); indices.Add(next + cross); indices.Add(v + cross);
                }
            }
            var mesh = new Mesh { name = name }; mesh.SetVertices(vertices); mesh.SetTriangles(indices, 0); mesh.RecalculateNormals();
            var go = Root(name, parent); go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterial = Mat(c); return go;
        }
        public static void Bake(Transform group, string id)
        {
            var vertices = new List<Vector3>(); var normals = new List<Vector3>(); var vertexColors = new List<Color>(); var triangles = new List<int>();var effectParts=new List<Vector4>();
            var filters = group.GetComponentsInChildren<MeshFilter>(true);
            foreach (var filter in filters)
            {
                var mesh = filter.sharedMesh; if (mesh == null) continue;
                var renderer = filter.GetComponent<MeshRenderer>(); if (renderer == null) continue;
                var matrix = group.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                var normalMatrix = matrix.inverse.transpose;
                Color color = renderer.sharedMaterial != null ? renderer.sharedMaterial.color : Color.white;
                int offset = vertices.Count; var vs = mesh.vertices; var ns = mesh.normals;
                string surface=filter.name.ToLowerInvariant();float gloss=surface.Contains("eye")||surface.Contains("iris")||surface.Contains("gem")||surface.Contains("orb")||surface.Contains("lens")?.75f:surface.Contains("blade")||surface.Contains("ice")||surface.Contains("metal")||surface.Contains("barrel")||surface.Contains("buckle")?.42f:.055f;
                Color packed=color.linear;packed.a=gloss;
                Vector3 center=matrix.MultiplyPoint3x4(mesh.bounds.center);
                float phase=surface.Contains("bolt")||surface.Contains("electric")?3:surface.Contains("core")?0:surface.Contains("wave")||surface.Contains("circle")||surface.Contains("halo")||surface.Contains("seal")?1:2;
                for (int i = 0; i < vs.Length; i++)
                {
                    Vector3 point=matrix.MultiplyPoint3x4(vs[i]),normal=normalMatrix.MultiplyVector(ns[i]).normalized;
                    if(!Finite(point)||!Finite(normal))throw new System.InvalidOperationException("Non-finite geometry: "+id+" / "+filter.name+" vertex "+i);
                    vertices.Add(point);normals.Add(normal);vertexColors.Add(packed);
                    effectParts.Add(new Vector4(phase,center.x,center.y,center.z));
                }
                foreach (int index in mesh.triangles) triangles.Add(offset + index);
            }
            if (vertices.Count == 0) return;
            var combined = new Mesh { name = id, indexFormat = vertices.Count > 65530 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16 };
            combined.SetVertices(vertices); combined.SetNormals(normals); combined.SetColors(vertexColors);combined.SetUVs(1,effectParts); combined.SetTriangles(triangles, 0); combined.RecalculateBounds();
            string path = "Assets/Art/Models/Generated/" + id + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) { EditorUtility.CopySerialized(combined, existing); Object.DestroyImmediate(combined); combined = existing; }
            else AssetDatabase.CreateAsset(combined, path);
            EditorUtility.SetDirty(combined);
            int collisionIndex=0;
            foreach(var collider in group.GetComponentsInChildren<MeshCollider>(true))
            {
                if(collider.sharedMesh==null||AssetDatabase.Contains(collider.sharedMesh))continue;
                string collisionPath="Assets/Art/Models/Generated/"+id+"-collision-"+(collisionIndex++)+".asset";
                var collisionMesh=AssetDatabase.LoadAssetAtPath<Mesh>(collisionPath);
                if(collisionMesh!=null)EditorUtility.CopySerialized(collider.sharedMesh,collisionMesh);
                else{collisionMesh=Object.Instantiate(collider.sharedMesh);AssetDatabase.CreateAsset(collisionMesh,collisionPath);}
                EditorUtility.SetDirty(collisionMesh);collider.sharedMesh=collisionMesh;
            }
            foreach (var filter in filters) { Object.DestroyImmediate(filter.GetComponent<MeshRenderer>()); Object.DestroyImmediate(filter); }
            var targetFilter = group.gameObject.GetComponent<MeshFilter>(); if (targetFilter == null) targetFilter = group.gameObject.AddComponent<MeshFilter>();
            var targetRenderer = group.gameObject.GetComponent<MeshRenderer>(); if (targetRenderer == null) targetRenderer = group.gameObject.AddComponent<MeshRenderer>();
            targetFilter.sharedMesh = combined; targetRenderer.sharedMaterial = master;
        }
        public static GameObject SavePrefab(GameObject root, string folder, string id)
        {
            string dir = "Assets/Art/Prefabs/" + folder; Directory.CreateDirectory(dir);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, dir + "/" + id + ".prefab"); Object.DestroyImmediate(root); return prefab;
        }
        static bool Finite(Vector3 value)=>!float.IsNaN(value.x)&&!float.IsNaN(value.y)&&!float.IsNaN(value.z)&&!float.IsInfinity(value.x)&&!float.IsInfinity(value.y)&&!float.IsInfinity(value.z);
        public static GameObject Character(int variant)
        {
            string id = new[] { "adventurer", "elementalist", "swordsman" }[variant];
            Color coat = variant == 0 ? Teal : variant == 1 ? Hex("7267B1") : Hex("4FAD83");
            Color hair = variant == 1 ? Hex("E8D4BA") : variant == 2 ? Hex("263E50") : Hex("75432B");
            var root = Root(id); root.layer = 9;
            var controller = root.AddComponent<CharacterController>(); controller.radius = .4f; controller.height = 2.1f; controller.center = Vector3.up * 1.05f; controller.stepOffset = .35f;
            root.AddComponent<PlayerController>(); root.AddComponent<AbilityController>();
            var anim = root.AddComponent<CharacterAnimator>(); anim.visual = Root("Visual", root.transform).transform;
            var body = Root("Body", anim.visual).transform;
            Sphere(body, "Coat", new Vector3(0, .98f, 0), new Vector3(.75f, .76f, .5f), coat);
            Cube(body, "Shirt", new Vector3(0, 1.02f, .242f), new Vector3(.32f, .46f, .055f), Cream);
            Cube(body, "Belt", new Vector3(0, .73f, .01f), new Vector3(.71f, .10f, .47f), Brown);
            Cube(body, "Buckle", new Vector3(0, .74f, .263f), new Vector3(.16f, .14f, .05f), Gold);
            Cube(body, "BuckleInset", new Vector3(0, .74f, .294f), new Vector3(.085f, .075f, .012f), Brown);
            Tube(body, "Strap", new Vector3(-.28f, 1.3f, .26f), new Vector3(.29f, .76f, .265f), .04f, Brown);
            for (int i = 0; i < 3; i++) Sphere(body, "Button", new Vector3(.06f, .93f + i * .12f, .28f), Vector3.one * .045f, Gold);
            Cube(body, "CoatLapelL", new Vector3(-.22f, 1.09f, .24f), new Vector3(.09f, .46f, .065f), coat * 1.18f, new Vector3(0, 0, -19));
            Cube(body, "CoatLapelR", new Vector3(.22f, 1.09f, .24f), new Vector3(.09f, .46f, .065f), coat * 1.18f, new Vector3(0, 0, 19));
            if(variant==0){Sphere(body, "Pouch", new Vector3(.4f, .74f, 0), new Vector3(.2f, .24f, .27f), Brown);Sphere(body, "Backpack", new Vector3(0, 1, -.25f), new Vector3(.48f, .51f, .25f), Hex("B27842"));}
            if(variant==1){Cube(body,"SpellBook",new Vector3(.4f,.80f,0),new Vector3(.23f,.36f,.27f),Hex("594C88"));Cube(body,"BookPages",new Vector3(.4f,.81f,.14f),new Vector3(.17f,.28f,.02f),Cream);}
            if(variant==1)
            {
                Sphere(body,"MageRobe",new Vector3(0,.58f,0),new Vector3(.92f,.8f,.65f),coat);
                Cube(body,"RobeTrim",new Vector3(0,.68f,.33f),new Vector3(.10f,.60f,.04f),Gold);
                for(int i=0;i<3;i++)Sphere(body,"RobeStar",new Vector3((i-1)*.22f,.49f+Mathf.Abs(i-1)*.12f,.29f),Vector3.one*.07f,Cream);
            }
            if(variant==2)
            {
                Sphere(body,"InnerRobe",new Vector3(0,.68f,0),new Vector3(.78f,.85f,.58f),Cream);
                for(int side=-1;side<=1;side+=2)Cube(body,"SplitCoatTail",new Vector3(side*.24f,.68f,.18f),new Vector3(.25f,.84f,.22f),coat,new Vector3(0,0,side*-9));
                Tube(body,"SwordScabbard",new Vector3(-.43f,.42f,-.35f),new Vector3(.50f,1.9f,-.35f),.10f,Hex("284F59"));
                Tube(body,"SwordHilt",new Vector3(.48f,1.86f,-.35f),new Vector3(.68f,2.18f,-.35f),.055f,Brown);
                Tube(body,"ScabbardGuard",new Vector3(.23f,1.99f,-.35f),new Vector3(.72f,1.70f,-.35f),.055f,Gold);
                Sphere(body,"JadePendant",new Vector3(.12f,.78f,.37f),new Vector3(.14f,.20f,.07f),Teal);
            }
            var scarfRing = Ring(body, "Scarf", .23f, .07f, variant==2?Cream:variant==1?Hex("C5ACD9"):Orange); scarfRing.transform.localPosition = new Vector3(0, 1.36f, 0);
            Bake(body, id + "-body");
            anim.head = Root("Head", anim.visual).transform; anim.head.localPosition = Vector3.up * 1.88f;
            Sphere(anim.head, "Face", Vector3.zero, new Vector3(1.15f, 1.06f, 1.01f), Skin);
            Sphere(anim.head, "HairCap", new Vector3(0, .28f, -.04f), new Vector3(1.17f, .68f, 1.02f), hair);
            for (int i = 0; i < 7; i++)
            {
                float x = (i - 3) * .15f;
                Spike(anim.head, "HairLock", new Vector3(x, .33f, .30f), new Vector3(x - .13f, .02f + Mathf.Abs(i - 3) * .045f, .49f), .16f, Color.Lerp(hair, Orange, i % 2 == 0 ? .13f : .02f));
            }
            Spike(anim.head, "Quiff", new Vector3(.06f, .43f, -.05f), new Vector3(.34f, .75f, .12f), .22f, hair * 1.2f);
            if(variant==0)
            {
                for(int sign=-1;sign<=1;sign+=2)
                {
                    var goggles=Ring(anim.head,"Goggles",.115f,.028f,Gold);goggles.transform.localPosition=new Vector3(sign*.19f,.36f,.39f);goggles.transform.localEulerAngles=new Vector3(90,0,0);
                    Sphere(anim.head,"GoggleLens",new Vector3(sign*.19f,.36f,.39f),new Vector3(.19f,.18f,.035f),Teal);
                }
                Tube(anim.head,"GoggleBridge",new Vector3(-.09f,.37f,.4f),new Vector3(.09f,.37f,.4f),.024f,Gold);
            }
            var eyes=Root("Eyes",anim.head).transform;anim.eyes=eyes;
            for (int sign = -1; sign <= 1; sign += 2)
            {
                Sphere(anim.head, "Ear", new Vector3(sign * .55f, -.04f, 0), new Vector3(.19f, .25f, .17f), Skin);
                Sphere(eyes, "EyeWhite", new Vector3(sign * .225f, .015f, .448f), new Vector3(.29f, .34f, .095f), Color.white);
                Sphere(eyes, "Iris", new Vector3(sign * .225f, .005f, .502f), new Vector3(.19f, .25f, .035f), variant == 1 ? Teal : Brown);
                Sphere(eyes, "Pupil", new Vector3(sign * .225f, .012f, .52f), new Vector3(.095f, .16f, .025f), Ink);
                Sphere(eyes, "EyeSpark", new Vector3(sign * .225f - .033f, .065f, .535f), Vector3.one * .058f, Color.white);
                Tube(anim.head, "Brow", new Vector3(sign * .12f, .23f, .44f), new Vector3(sign * .34f, .25f, .42f), .025f, hair);
                Sphere(anim.head, "Cheek", new Vector3(sign * .36f, -.16f, .43f), new Vector3(.15f, .07f, .025f), Hex("F49C87"));
            }
            Sphere(anim.head, "Nose", new Vector3(0, -.12f, .515f), new Vector3(.11f, .10f, .105f), Skin * .98f);
            if (variant == 1)
            {
                var brim = Ring(anim.head, "HatBrim", .57f, .095f, coat); brim.transform.localPosition = Vector3.up * .45f;
                Spike(anim.head, "Hat", new Vector3(0, .5f, 0), new Vector3(.2f, 1.2f, -.08f), .48f, coat);
                Sphere(anim.head, "Star", new Vector3(.08f, .75f, .31f), Vector3.one * .14f, Gold);
            }
            if (variant == 2) Spike(anim.head, "Ponytail", new Vector3(0, .1f, -.45f), new Vector3(.1f, -.65f, -.57f), .22f, hair);
            if(variant==2)
            {
                var ornament=Ring(anim.head,"JadeHairCrown",.19f,.035f,Gold);ornament.transform.localPosition=new Vector3(0,.48f,-.05f);
                Tube(anim.head,"HairPin",new Vector3(-.36f,.52f,0),new Vector3(.36f,.52f,0),.035f,Gold);
            }
            eyes.SetParent(null,true);Bake(anim.head, id + "-head");eyes.SetParent(anim.head,true);Bake(eyes,id+"-eyes");
            anim.eyelids=new Transform[2];
            for(int i=0;i<2;i++)
            {
                var lid=Root("Eyelid"+i,anim.head).transform;lid.localPosition=new Vector3((i==0?-1:1)*.225f,.015f,.548f);
                Tube(lid,"ClosedEyeLeft",new Vector3(-.115f,.005f,-.025f),new Vector3(0,-.035f,0),.019f,Brown);
                Tube(lid,"ClosedEyeRight",new Vector3(0,-.035f,0),new Vector3(.115f,.005f,-.025f),.019f,Brown);
                Bake(lid,id+"-eyelid-"+i);lid.gameObject.SetActive(false);anim.eyelids[i]=lid;
            }
            anim.mouth=Root("Mouth",anim.head).transform;anim.mouth.localPosition=new Vector3(0,-.27f,.478f);
            Sphere(anim.mouth,"Smile",Vector3.zero,new Vector3(.16f,.055f,.025f),Hex("A45C45"));Bake(anim.mouth,id+"-mouth");
            for (int sign = -1; sign <= 1; sign += 2)
            {
                var arm = Root(sign < 0 ? "ArmL" : "ArmR", anim.visual).transform; arm.localPosition = new Vector3(sign * .43f, 1.18f, 0);
                Capsule(arm, "Sleeve", new Vector3(0, -.06f, 0), new Vector3(.30f, .19f, .31f), coat);
                Capsule(arm, "Arm", new Vector3(sign * .02f, -.29f, .015f), new Vector3(.20f, .14f, .21f), Skin);
                Sphere(arm, "Glove", new Vector3(sign * .02f, -.46f, .03f), new Vector3(.24f, .22f, .25f), Brown);
                Cube(arm, "Cuff", new Vector3(0, -.37f, 0), new Vector3(.25f, .06f, .25f), Gold);
                if(variant==1&&sign<0)
                {
                    Tube(arm,"Wand",new Vector3(-.05f,-1.12f,.13f),new Vector3(-.05f,.57f,.13f),.04f,Brown);
                    Sphere(arm,"WandOrb",new Vector3(-.05f,.67f,.13f),Vector3.one*.23f,Teal);
                    var wandRing=Ring(arm,"WandRing",.20f,.035f,Gold);wandRing.transform.localPosition=new Vector3(-.05f,.67f,.13f);wandRing.transform.localEulerAngles=new Vector3(60,0,0);
                }
                if(variant==0&&sign>0)
                {
                    Cube(arm,"PistolMetalSlide",new Vector3(0,-.73f,.11f),new Vector3(.19f,.43f,.22f),Hex("607D84"));
                    Tube(arm,"PistolBarrel",new Vector3(0,-.82f,.11f),new Vector3(0,-.99f,.11f),.065f,Ink);
                    Tube(arm,"PistolGrip",new Vector3(0,-.50f,.03f),new Vector3(0,-.61f,-.11f),.06f,Brown);
                    var muzzle=Ring(arm,"MuzzleRim",.067f,.016f,Gold);muzzle.transform.localPosition=new Vector3(0,-1.0f,.11f);
                    Cube(arm,"PistolSight",new Vector3(0,-.91f,.24f),new Vector3(.045f,.06f,.045f),Gold);
                }
                if(variant==2&&sign>0)
                {
                    for(int finger=0;finger<2;finger++)Capsule(arm,"SwordMudraFinger",new Vector3((finger-.5f)*.072f,-.62f,.06f),new Vector3(.065f,.11f,.07f),Skin);
                    var seal=Ring(arm,"JadeWristSeal",.12f,.022f,Gold);seal.transform.localPosition=new Vector3(0,-.34f,0);
                }
                Bake(arm, id + "-arm" + sign); if (sign < 0) anim.leftArm = arm; else anim.rightArm = arm;
                var leg = Root(sign < 0 ? "LegL" : "LegR", anim.visual).transform; leg.localPosition = new Vector3(sign * .18f, .69f, 0);
                Capsule(leg, "Trousers", new Vector3(0, -.18f, 0), new Vector3(.25f, .21f, .28f), variant==2?Cream:Ink);
                Sphere(leg, "Boot", new Vector3(0, -.48f, .07f), new Vector3(.32f, .37f, .46f), Brown);
                Sphere(leg, "Sole", new Vector3(0, -.61f, .07f), new Vector3(.30f, .075f, .41f), Ink);
                for (int i = 0; i < 2; i++) Cube(leg, "BootBuckle", new Vector3(0, -.39f - i * .1f, .25f), new Vector3(.19f, .045f, .025f), Cream);
                Bake(leg, id + "-leg" + sign); if (sign < 0) anim.leftLeg = leg; else anim.rightLeg = leg;
            }
            anim.scarf = Root("ScarfTail", anim.visual).transform; anim.scarf.localPosition = new Vector3(-.12f, 1.34f, -.15f);
            Spike(anim.scarf, "Tail", Vector3.zero, new Vector3(-.55f, -.12f, -.65f), .14f, variant==2?Cream:variant==1?Hex("C5ACD9"):Orange); Bake(anim.scarf, id + "-scarf");
            Blob(root.transform,.72f);
            return SavePrefab(root, "Characters", id);
        }
    }
}
