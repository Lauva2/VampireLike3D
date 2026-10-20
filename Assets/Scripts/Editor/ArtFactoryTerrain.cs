using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        static void TerrainFeatures(Transform t,int biome,System.Random random,int variant)
        {
            Color road=biome==0?Hex("C3B58A"):biome==1?Hex("E8CE96"):biome==2?Hex("DBE7E3"):Hex("8A7780");
            Ribbon(t,"EastWestTrail",0,variant==2?1.8f:2.6f,.024f,road,false,.28f+variant*.5f);
            Ribbon(t,"NorthSouthTrail",0,variant==1?1.8f:2.6f,.026f,road,true,.35f+variant*.5f);
            Part(t,"Crossing",PrimitiveType.Cylinder,new Vector3(0,.027f,0),new Vector3(3.6f,.003f,3.6f),road);
            for(int i=0;i<28;i++)
            {
                float x=(float)random.NextDouble()*31-15.5f;float z=(float)random.NextDouble()*2-1;
                Part(t,"TrailPebble",PrimitiveType.Cylinder,new Vector3(x,.032f,z),new Vector3(.10f,.007f,.08f),Color.Lerp(road,Brown,.14f));
                Part(t,"TrailPebble",PrimitiveType.Cylinder,new Vector3(z,.032f,x),new Vector3(.10f,.007f,.08f),Color.Lerp(road,Brown,.14f));
            }
            if((biome==0||biome==3)&&variant==0)
            {
                Color shore=biome==0?Hex("6D8977"):Hex("3D384A"), water=biome==0?Hex("4BADA6"):Hex("ED6037"),light=biome==0?Hex("92D5C1"):Hex("FFD06A");
                Ribbon(t,"RiverBank",8,3.3f,.04f,shore,false,1.1f);
                Ribbon(t,"River",8,2.5f,.065f,water,false,1.1f);
                for(int i=0;i<32;i++)
                {
                    float x=i-15.5f;float z=8+Mathf.Sin(x*Mathf.PI/16)*1.1f;
                    Cube(t,"Current",new Vector3(x,.067f,z+Mathf.Sin(i*2.4f)*.7f),new Vector3(.65f,.008f,.06f),light);
                }
                for(int i=0;i<10;i++) Cube(t,"BridgePlank",new Vector3(0,.17f,6.3f+i*.38f),new Vector3(3.4f,.22f,.34f),biome==0?Hex("A7794D"):Hex("A39B98"));
                for(int s=-1;s<=1;s+=2)
                {
                    for(int i=0;i<3;i++) Cube(t,"BridgePost",new Vector3(s*1.6f,.55f,6.4f+i*1.65f),new Vector3(.16f,.9f,.16f),biome==0?Brown:Hex("615663"));
                    Tube(t,"BridgeRail",new Vector3(s*1.6f,.94f,6.3f),new Vector3(s*1.6f,.94f,9.9f),.065f,biome==0?Brown:Hex("615663"));
                }
                if(biome==0)
                {
                    for(int i=0;i<10;i++) {float a=i*2.4f;Vector3 p=new Vector3(-9+Mathf.Sin(a)*3,.05f,7+Mathf.Cos(a)*2);Part(t,"LilyPad",PrimitiveType.Cylinder,p,new Vector3(.4f,.006f,.35f),Teal);Sphere(t,"WaterLily",p+Vector3.up*.06f,new Vector3(.15f,.08f,.15f),Cream);}
                }
                else for(int i=0;i<10;i++)
                {
                    Vector3 p=new Vector3(5+i*.8f,.02f,-8+Mathf.Sin(i)*2);
                    Cube(t,"MagmaCrack",p,new Vector3(.09f,.02f,2.6f),Orange,new Vector3(0,25+i*12,0));
                }
            }
            if(biome==0&&variant>0)
            {
                for(int i=0;i<(variant==1?2:5);i++)
                {
                    Vector3 p=new Vector3((i%2==0?-1:1)*(7+i*.8f),0,-7+i*3.2f);
                    Tube(t,"FallenLog",p+new Vector3(-1.8f,.3f,0),p+new Vector3(1.7f,.3f,.6f),.30f,Brown);
                    for(int j=0;j<4;j++)Sphere(t,"LogMoss",p+new Vector3((j-1.5f)*.7f,.52f,.2f),new Vector3(.65f,.19f,.48f),Hex("4F8C68"));
                }
                for(int i=0;i<18;i++){Vector3 p=new Vector3((float)random.NextDouble()*26-13,.03f,(float)random.NextDouble()*26-13);if(Mathf.Abs(p.x)<3||Mathf.Abs(p.z)<3)continue;Part(t,"WildflowerPatch",PrimitiveType.Cylinder,p,new Vector3(1.3f,.005f,.9f),Hex("8AB866"));for(int j=0;j<5;j++){Vector3 q=p+new Vector3(Mathf.Sin(j*2.4f)*.4f,.15f,Mathf.Cos(j*2.4f)*.4f);Sphere(t,"Wildflowers",q,new Vector3(.11f,.08f,.11f),i%2==0?Gold:Cream);}}
            }
            if(biome==1)
            {
                for(int i=0;i<6;i++)
                {
                    Vector3 p=new Vector3((i%2==0?-1:1)*(8+i%3*2),-.13f,(i/2-1)*10);
                    if(Vector3.Distance(p,new Vector3(-9,0,8))<6)continue;
                    Dune(t,p,new Vector3(8,1.1f+i*.12f,5),i%2==0?Hex("E8C58A"):Hex("CFA467"));
                    for(int j=0;j<3;j++) Tube(t,"SandRipple",p+new Vector3(-2,.43f+j*.005f,j*.36f),p+new Vector3(2,.43f+j*.005f,j*.36f),.018f,Hex("DDB67D"));
                }
                if(variant==0){Part(t,"OasisShore",PrimitiveType.Cylinder,new Vector3(-9,.05f,8),new Vector3(7,.03f,5),Hex("ADBB83"));
                Part(t,"Oasis",PrimitiveType.Cylinder,new Vector3(-9,.09f,8),new Vector3(5.5f,.02f,3.6f),Hex("51AEA5"));
                Part(t,"OasisLight",PrimitiveType.Cylinder,new Vector3(-9,.12f,8),new Vector3(4.7f,.008f,2.9f),Hex("7ACAC0"));}
                else for(int i=0;i<5;i++){Vector3 p=new Vector3(5+i*1.5f,.2f,7+Mathf.Sin(i)*2);Cube(t,"BuriedMasonry",p,new Vector3(1.2f,.4f+i*.17f,.9f),Hex("B99364"),new Vector3(0,i*22,0));}
            }
            if(biome==2&&variant==0)
            {
                Part(t,"LakeShore",PrimitiveType.Cylinder,new Vector3(-8,.025f,7),new Vector3(10,.025f,6.5f),Hex("8BB9C9"));
                Part(t,"FrozenLake",PrimitiveType.Cylinder,new Vector3(-8,.065f,7),new Vector3(8.8f,.018f,5.5f),Hex("65B8CC"));
                for(int i=0;i<5;i++) Tube(t,"IceCrack",new Vector3(-11+i*1.3f,.10f,6),new Vector3(-10+i*1.1f,.10f,8.2f),.025f,Hex("C8F0F0"));
                for(int i=0;i<6;i++) Spike(t,"IceRidge",new Vector3(5+i*.9f,0,10),new Vector3(5.1f+i*.9f,1.1f+Mathf.Sin(i)*.4f,10),.55f,Hex("85C4D7"));
            }
            if(biome==2&&variant>0)for(int i=0;i<7;i++){Vector3 p=new Vector3(-11+i*.4f,0,-10+i*2.5f);Sphere(t,"SnowBank",p,new Vector3(2.2f,.65f,2.6f),Hex("D9E9E6"));Spike(t,"BlueIce",p+Vector3.right*.5f,p+new Vector3(.7f,.8f,0),.4f,Hex("7EBAD1"));}
            if(biome==3&&variant>0)for(int i=0;i<12;i++){Vector3 p=new Vector3((i%2==0?-1:1)*(7+i%3),0,(i/2-2.5f)*4);Cube(t,"ObsidianSlab",p+Vector3.up*.12f,new Vector3(2.2f,.24f,1.4f),Hex("444354"),new Vector3(0,i*24,0));Cube(t,"EmberSeam",p+Vector3.up*.25f,new Vector3(1.5f,.008f,.06f),Orange,new Vector3(0,i*24,0));}
        }
        public static GameObject Landmark(int biome)
        {
            string id=new[]{"forest","desert","snow","volcano"}[biome];var root=Root("Landmark-"+id);root.AddComponent<OcclusionFader>();var t=Root("Architecture",root.transform).transform;
            Color stone=biome==0?Hex("99A48B"):biome==1?Hex("CFA679"):biome==2?Hex("99C0D0"):Hex("646079");
            Part(t,"Plinth",PrimitiveType.Cylinder,new Vector3(0,.1f,0),new Vector3(6,.1f,5),stone);
            if(biome==0)
            {
                Cube(t,"Cottage",new Vector3(0,1.25f,0),new Vector3(3.1f,2.5f,2.4f),Cream);
                Gable(t,Cream);
                Cube(t,"RoofL",new Vector3(-.9f,2.9f,0),new Vector3(2.2f,.25f,3.1f),Hex("427B78"),new Vector3(0,0,30));
                Cube(t,"RoofR",new Vector3(.9f,2.9f,0),new Vector3(2.2f,.25f,3.1f),Hex("2D6665"),new Vector3(0,0,-30));
                Cube(t,"Door",new Vector3(0,.85f,1.22f),new Vector3(.85f,1.7f,.08f),Brown);
                for(int s=-1;s<=1;s+=2){Cube(t,"Window",new Vector3(s*.95f,1.45f,1.23f),new Vector3(.53f,.65f,.09f),Teal);Cube(t,"Beam",new Vector3(s*1.43f,1.2f,1.24f),new Vector3(.15f,2.4f,.12f),Brown);}
                Cube(t,"Chimney",new Vector3(.8f,3.3f,-.5f),new Vector3(.55f,1.2f,.6f),stone);
            }
            else if(biome==1)
            {
                for(int s=-1;s<=1;s+=2){Cube(t,"Pillar",new Vector3(s*1.75f,1.5f,0),new Vector3(.85f,3,1),stone);Cube(t,"Capital",new Vector3(s*1.75f,2.9f,0),new Vector3(1.1f,.3f,1.2f),Cream);}
                Cube(t,"Lintel",new Vector3(0,3.25f,0),new Vector3(4.6f,.55f,1.2f),stone);Sphere(t,"SunSeal",new Vector3(0,3.3f,.66f),new Vector3(.8f,.8f,.1f),Gold);
                for(int i=0;i<4;i++)Cube(t,"Inscription",new Vector3(-.5f+i*.33f,2.97f,.63f),new Vector3(.12f,.13f,.02f),Teal);
            }
            else if(biome==2)
            {
                for(int s=-1;s<=1;s+=2){Cube(t,"Pillar",new Vector3(s*1.7f,1.25f,0),new Vector3(.7f,2.5f,.8f),stone);Spike(t,"PillarIce",new Vector3(s*1.7f,2.3f,0),new Vector3(s*1.7f,3.6f,0),.5f,Hex("8DD1E5"));}
                Cube(t,"Gate",new Vector3(0,2.7f,0),new Vector3(4.4f,.35f,1),Cream);Sphere(t,"SnowCap",new Vector3(0,2.93f,0),new Vector3(4.7f,.4f,1.3f),Hex("F2F5EC"));
                var ring=Ring(t,"SnowSeal",.58f,.1f,Gold);ring.transform.localPosition=Vector3.up*1.8f;ring.transform.localEulerAngles=new Vector3(90,0,0);
            }
            else
            {
                for(int s=-1;s<=1;s+=2){Cube(t,"ForgePillar",new Vector3(s*1.7f,1.7f,0),new Vector3(.9f,3.4f,1.1f),stone);Sphere(t,"Furnace",new Vector3(s*1.7f,2.3f,.57f),new Vector3(.4f,.7f,.1f),Orange);}
                Cube(t,"Anvil",new Vector3(0,.85f,0),new Vector3(2.1f,.5f,1),Hex("3E485B"));Cube(t,"AnvilBase",new Vector3(0,.4f,0),new Vector3(1,.8f,.8f),stone);
                Tube(t,"ForgeChain",new Vector3(-1.7f,3.35f,0),new Vector3(1.7f,3.35f,0),.1f,Gold);
            }
            Bake(t,"landmark-"+id);Blob(root.transform,2.7f);root.layer=10;
            if(biome==0){var obstacle=root.AddComponent<BoxCollider>();obstacle.center=new Vector3(0,1.5f,0);obstacle.size=new Vector3(3.2f,3,2.5f);}
            else
            {
                for(int s=-1;s<=1;s+=2){var pillar=Root("PillarCollider",root.transform);pillar.layer=10;var obstacle=pillar.AddComponent<BoxCollider>();obstacle.center=new Vector3(s*1.72f,1.5f,0);obstacle.size=new Vector3(.95f,3,1.2f);}
                if(biome==3){var anvil=Root("AnvilCollider",root.transform);anvil.layer=10;var obstacle=anvil.AddComponent<BoxCollider>();obstacle.center=Vector3.up*.6f;obstacle.size=new Vector3(2.1f,1.2f,1);}
            }
            return SavePrefab(root,"World/Landmarks","architecture-"+id);
        }
        static void Ribbon(Transform parent,string name,float center,float width,float height,Color color,bool vertical,float wiggle)
        {
            var vertices=new System.Collections.Generic.List<Vector3>();var indices=new System.Collections.Generic.List<int>();
            const int segments=64;
            for(int i=0;i<=segments;i++)
            {
                float x=-16+i*32f/segments;float wave=Mathf.Sin(x*Mathf.PI/16);float z=center+wave*wave*wave*wiggle;
                // All trails share position, tangent and width at every chunk edge.
                float edgeWidth=name.EndsWith("Trail")?Mathf.Lerp(width,2.6f,Mathf.SmoothStep(0,1,Mathf.Abs(x)/16)):width;
                Vector3 a=new Vector3(x,height,z-edgeWidth*.5f),b=new Vector3(x,height,z+edgeWidth*.5f);
                if(vertical){a=new Vector3(a.z,a.y,a.x);b=new Vector3(b.z,b.y,b.x);}
                vertices.Add(a);vertices.Add(b);
                if(i<segments){int j=i*2;if(!vertical){indices.Add(j);indices.Add(j+1);indices.Add(j+2);indices.Add(j+1);indices.Add(j+3);indices.Add(j+2);}else{indices.Add(j);indices.Add(j+2);indices.Add(j+1);indices.Add(j+1);indices.Add(j+2);indices.Add(j+3);}}
            }
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();var go=Root(name,parent);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=Mat(color);
        }
        static void Gable(Transform parent,Color color)
        {
            var mesh=new Mesh{name="Gable"};mesh.vertices=new[]{new Vector3(-1.5f,2.45f,1.19f),new Vector3(1.5f,2.45f,1.19f),new Vector3(0,3.4f,1.19f),new Vector3(-1.5f,2.45f,-1.19f),new Vector3(0,3.4f,-1.19f),new Vector3(1.5f,2.45f,-1.19f)};mesh.triangles=new[]{0,1,2,3,4,5};mesh.RecalculateNormals();var go=Root("Gable",parent);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=Mat(color);
        }
        static void Dune(Transform parent,Vector3 position,Vector3 size,Color color)
        {
            const string path="Assets/Art/Models/Generated/sand-dune.asset";var mesh=UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null)
            {
                var vertices=new System.Collections.Generic.List<Vector3>();var indices=new System.Collections.Generic.List<int>();const int columns=20,rows=14;
                for(int z=0;z<=rows;z++)for(int x=0;x<=columns;x++)
                {
                    float u=x/(float)columns,v=z/(float)rows;float ridge=.42f+Mathf.Sin(u*Mathf.PI)*.1f;float cross=v<ridge?v/ridge:1-(v-ridge)/(1-ridge);float height=Mathf.Pow(Mathf.Sin(u*Mathf.PI),1.35f)*Mathf.Pow(Mathf.Max(0,cross),v<ridge?1.8f:.65f);
                    vertices.Add(new Vector3(u-.5f,height,v-.5f));if(z==rows||x==columns)continue;int a=z*(columns+1)+x;indices.Add(a);indices.Add(a+columns+1);indices.Add(a+1);indices.Add(a+1);indices.Add(a+columns+1);indices.Add(a+columns+2);
                }
                mesh=new Mesh{name="Wind sculpted dune"};mesh.SetVertices(vertices);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();UnityEditor.AssetDatabase.CreateAsset(mesh,path);
            }
            var go=Root("SandDune",parent);go.transform.localPosition=position;go.transform.localScale=size;go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=Mat(color);go.AddComponent<MeshCollider>().sharedMesh=mesh;
        }
    }
}
