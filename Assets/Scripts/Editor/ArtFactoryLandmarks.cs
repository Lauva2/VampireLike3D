using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        static GameObject BossLandmark()
        {
            var root=Root("boss-site");root.AddComponent<OcclusionFader>();
            var appearance=root.AddComponent<BossSiteAppearance>();appearance.themes=new GameObject[4];appearance.invocationSeals=new GameObject[4];
            for(int biome=0;biome<4;biome++)
            {
                var theme=Root("Sanctuary"+biome,root.transform);appearance.themes[biome]=theme;
                var t=Root("Geometry",theme.transform).transform;
                Color stone=biome==0?Hex("91A58A"):biome==1?Hex("D3A16A"):biome==2?Hex("8FC3D6"):Hex("4A4B60");
                var spent=Ring(t,"AncientSealGroove",2.45f,.035f,Color.Lerp(stone,GroundColors[biome],.25f));spent.transform.localPosition=Vector3.up*.08f;
                var invocation=Root("Invocation",theme.transform);appearance.invocationSeals[biome]=invocation;
                var seal=Ring(invocation.transform,"InvocationSeal",2.45f,.045f,biome==2?Cream:Gold);seal.transform.localPosition=Vector3.up*.095f;
                // Cluster the high silhouette behind the clearing. All four road exits stay open.
                var anchors=new[]{new Vector3(-5.4f,0,2.9f),new Vector3(-3.7f,0,4.4f),new Vector3(-.9f,0,5.1f),new Vector3(2.5f,0,4.6f),new Vector3(-5.5f,0,-1.3f),new Vector3(3.3f,0,3.8f)};
                for(int i=0;i<6;i++)
                {
                    Vector3 p=anchors[i];float a=Mathf.Atan2(p.x,p.z);float h=new[]{2.8f,2.1f,1.65f,1.2f,.52f,.36f}[i];
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
                    if(i<4){var obstacle=Root("SanctuaryCollider",theme.transform);obstacle.layer=10;obstacle.transform.localPosition=p;
                    var collider=obstacle.AddComponent<CapsuleCollider>();collider.radius=.5f;collider.height=h;collider.center=Vector3.up*h*.5f;}
                    Sphere(invocation.transform,"InvocationGem",p.normalized*2.45f+Vector3.up*.12f,new Vector3(.16f,.12f,.16f),biome==2?Hex("79C9DF"):Gold);
                }
                SanctuaryBanks(t,biome,stone);
                // Low approach stones describe a side entrance without fencing in a Boss fight.
                for(int i=0;i<3;i++)GroundRock(t,"ApproachStone",new Vector3(4.5f-i*.8f,.09f,-2.1f+i*.48f),new Vector3(1.45f,.16f,1.0f),Color.Lerp(stone,Cream,.3f),i+85);
                Bake(invocation.transform,"sanctuary-invocation-"+biome);
                Bake(t,"sanctuary-"+biome);theme.SetActive(biome==0);
            }
            return SavePrefab(root,"World/Landmarks","boss-site");
        }
        static void SanctuaryBanks(Transform t,int biome,Color stone)
        {
            var bank=new[]{new Vector3(-6.6f,0,-1.8f),new Vector3(-6.4f,0,.9f),new Vector3(-5.5f,0,3.6f),new Vector3(-3.2f,0,5.1f),new Vector3(-.7f,0,5.6f),new Vector3(2.1f,0,4.9f)};
            for(int i=0;i<bank.Length;i++)
            {
                var p=bank[i];float h=new[]{.30f,.65f,1.15f,1.35f,.85f,.4f}[i];
                var rock=GroundRock(t,"SanctuaryBank",p+Vector3.up*h*.5f,new Vector3(3.8f,h,2.8f),stone,i+91);
                rock.layer=10;rock.AddComponent<MeshCollider>().sharedMesh=rock.GetComponent<MeshFilter>().sharedMesh;
                if(biome==0){GroundRock(t,"MossedRootBank",p+Vector3.up*h*.9f,new Vector3(3.1f,.32f,2.35f),Hex("7CAF6B"),i+91);Tube(t,"ExposedRoot",p+new Vector3(-1,.18f,-1),p+new Vector3(1.4f,.1f,-.3f),.13f,Brown);if(i<4)Sphere(t,"BankUnderstory",p+new Vector3(-.9f,h+.2f,.2f),new Vector3(1.8f,.65f,1.6f),Hex("67945C"));}
                else if(biome==1){GroundRock(t,"WindwornSandApron",p+new Vector3(.8f,.05f,-.8f),new Vector3(4.2f,.18f,3.1f),Hex("DEBD88"),i+94);Cube(t,"FallenTempleLintel",p+new Vector3(.5f,.3f,.25f),new Vector3(2.6f,.5f,.7f),stone,new Vector3(0,17+i*23,5));}
                else if(biome==2)GroundRock(t,"SnowdriftCrown",p+Vector3.up*h*.85f,new Vector3(3.7f,.4f,2.8f),Hex("E5EFE8"),i+91);
                else{GroundRock(t,"CooledLavaApron",p+new Vector3(.5f,.07f,-.9f),new Vector3(4.4f,.18f,3),Hex("5B5261"),i+94);Tube(t,"BankEmberSeam",p+new Vector3(-1,.16f,-1),p+new Vector3(.8f,.12f,-1.3f),.035f,Hex("BD8060"));}
            }
        }
    }
}
