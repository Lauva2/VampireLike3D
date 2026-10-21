using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        static GameObject BossLandmark()
        {
            var root=Root("boss-site");root.AddComponent<OcclusionFader>();
            var appearance=root.AddComponent<BossSiteAppearance>();appearance.themes=new GameObject[4];
            for(int biome=0;biome<4;biome++)
            {
                var theme=Root("Sanctuary"+biome,root.transform);appearance.themes[biome]=theme;
                var t=Root("Geometry",theme.transform).transform;
                Color stone=biome==0?Hex("91A58A"):biome==1?Hex("D3A16A"):biome==2?Hex("8FC3D6"):Hex("4A4B60");
                Part(t,"SanctuaryFloor",PrimitiveType.Cylinder,new Vector3(0,.025f,0),new Vector3(6.4f,.025f,6.4f),Color.Lerp(stone,GroundColors[biome],.5f));
                var seal=Ring(t,"InvocationSeal",2.45f,.045f,biome==2?Cream:Gold);seal.transform.localPosition=Vector3.up*.065f;
                for(int i=0;i<6;i++)
                {
                    float a=(i+.5f)*Mathf.PI/3;Vector3 p=new Vector3(Mathf.Sin(a)*3.6f,0,Mathf.Cos(a)*3.6f);float h=1.7f+i%3*.38f;
                    if(biome==0)
                    {
                        Tube(t,"AncientRoot",p+new Vector3(-.3f,.08f,0),p+Vector3.up*h,.22f,Brown);
                        Spike(t,"TwistedBranch",p+Vector3.up*(h*.65f),p+new Vector3(Mathf.Cos(a)*.6f,h+.45f,Mathf.Sin(a)*.6f),.15f,Brown);
                        for(int j=0;j<3;j++)Sphere(t,"SacredLeaves",p+new Vector3(Mathf.Sin(j*2.4f)*.35f,h+.25f,Mathf.Cos(j*2.4f)*.35f),new Vector3(.75f,.38f,.6f),j%2==0?Hex("7CAF6B"):Hex("B0CA79"));
                    }
                    else if(biome==1)
                    {
                        Cube(t,"SunTempleColumn",p+Vector3.up*h*.5f,new Vector3(.67f,h,.67f),stone,new Vector3(0,a*Mathf.Rad2Deg,0));
                        Cube(t,"SandstoneCapital",p+Vector3.up*h,new Vector3(.95f,.25f,.95f),Cream);
                        for(int j=0;j<3;j++)Cube(t,"TurquoiseInscription",p+new Vector3(0,.65f+j*.22f,.35f),new Vector3(.28f,.06f,.035f),Teal);
                    }
                    else if(biome==2)
                    {
                        Rock(t,"GlacialPedestal",p+Vector3.up*.25f,new Vector3(1.1f,.5f,1.1f),Cream,i);
                        Spike(t,"IceObelisk",p+Vector3.up*.3f,p+new Vector3(Mathf.Sin(a)*.18f,h+.35f,0),.45f,i%2==0?Hex("8ACDE2"):Hex("C0E9ED"));
                        var rune=Ring(t,"IceRune",.20f,.035f,Gold);rune.transform.localPosition=p+new Vector3(0,1.0f,.23f);rune.transform.localEulerAngles=new Vector3(90,0,0);
                    }
                    else
                    {
                        Part(t,"BasaltTotem",PrimitiveType.Cylinder,p+Vector3.up*h*.5f,new Vector3(.85f,h*.5f,.85f),stone);
                        for(int j=0;j<3;j++){var ring=Ring(t,"MoltenSeam",.43f,.035f,j==2?Gold:Orange);ring.transform.localPosition=p+Vector3.up*(.35f+j*.55f);}
                        Rock(t,"CrackedCrown",p+Vector3.up*h,new Vector3(1.05f,.55f,1.0f),Hex("6C6070"),i);
                    }
                    var obstacle=Root("SanctuaryCollider",theme.transform);obstacle.layer=10;obstacle.transform.localPosition=p;
                    var collider=obstacle.AddComponent<CapsuleCollider>();collider.radius=.34f;collider.height=h;collider.center=Vector3.up*h*.5f;
                    Sphere(t,"InvocationGem",p*.68f+Vector3.up*.09f,new Vector3(.16f,.12f,.16f),biome==2?Hex("79C9DF"):Gold);
                }
                Bake(t,"sanctuary-"+biome);theme.SetActive(biome==0);
            }
            return SavePrefab(root,"World/Landmarks","boss-site");
        }
    }
}
