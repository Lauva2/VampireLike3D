using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        // Hand-shaped low-poly plates give stone a different surface language from skin.
        static GameObject Rock(Transform parent,string name,Vector3 position,Vector3 size,Color color,int seed)
        {
            float g=(1+Mathf.Sqrt(5))*.5f;
            var points=new[]{new Vector3(-1,g,0),new Vector3(1,g,0),new Vector3(-1,-g,0),new Vector3(1,-g,0),new Vector3(0,-1,g),new Vector3(0,1,g),new Vector3(0,-1,-g),new Vector3(0,1,-g),new Vector3(g,0,-1),new Vector3(g,0,1),new Vector3(-g,0,-1),new Vector3(-g,0,1)};
            int[] faces={0,11,5,0,5,1,0,1,7,0,7,10,0,10,11,1,5,9,5,11,4,11,10,2,10,7,6,7,1,8,3,9,4,3,4,2,3,2,6,3,6,8,3,8,9,4,9,5,2,4,11,6,2,10,8,6,7,9,8,1};
            var vertices=new Vector3[faces.Length];var triangles=new int[faces.Length];
            for(int i=0;i<vertices.Length;i++){var v=points[faces[i]].normalized;float wobble=.94f+.06f*Mathf.Sin(faces[i]*3.1f+seed);vertices[i]=Vector3.Scale(v*wobble,size*.5f);triangles[i]=i;}
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();
            var go=Root(name,parent);go.transform.localPosition=position;go.transform.localEulerAngles=new Vector3(seed*7,seed*19,seed*3);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=Mat(color);
            return go;
        }
        static GameObject GroundRock(Transform parent,string name,Vector3 position,Vector3 size,Color color,int seed)
        {
            var rock=Rock(parent,name,position,size,color,seed);
            rock.transform.localEulerAngles=new Vector3(0,seed*19,0);
            return rock;
        }
        static void SandWorm(Transform t,EnemyActor actor,Transform root)
        {
            actor.bodySegments=new Transform[7];
            for(int j=0;j<7;j++)
            {
                var rig=Root("SegmentRig"+j,root).transform;rig.localPosition=new Vector3(Mathf.Sin(j*.7f)*.55f,.65f-j*.045f,-.55f-j*.56f);
                float width=1.9f-j*.19f;
                Sphere(rig,"Shell",Vector3.zero,new Vector3(width,1.13f-j*.09f,.95f),j%2==0?Hex("D39A51"):Hex("E9BA73"));
                var ring=Ring(rig,"ChitinRidge",width*.40f,.055f,Hex("865E40"));ring.transform.localEulerAngles=new Vector3(90,0,0);ring.transform.localScale=new Vector3(1,.68f,1);
                Spike(rig,"BackFin",new Vector3(0,.38f,0),new Vector3(0,.85f-j*.055f,-.16f),.22f-j*.018f,Hex("B56C3F"));
                Bake(rig,"boss-desert-segment-"+j);actor.bodySegments[j]=rig;
            }
            Sphere(t,"HeadShell",new Vector3(0,1.3f,.30f),new Vector3(2.2f,1.8f,1.8f),Hex("DCA657"));
            Rock(t,"ForeheadPlate",new Vector3(0,1.93f,.62f),new Vector3(1.8f,.48f,1.1f),Hex("F0C67E"),3);
            Eyes(t,new Vector3(0,1.58f,1.09f),2.15f,Hex("4A493B"));
            Sphere(t,"OpenMouth",new Vector3(0,.94f,1.08f),new Vector3(1.30f,.62f,.24f),Hex("67463A"));
            for(int s=-1;s<=1;s+=2)
            {
                Spike(t,"Mandible",new Vector3(s*.78f,.94f,.85f),new Vector3(s*.52f,.70f,1.60f),.24f,Hex("FFF0CA"));
                for(int j=0;j<3;j++)Spike(t,"CheekPlate",new Vector3(s*.86f,1.0f+j*.24f,.35f),new Vector3(s*1.2f,1.1f+j*.24f,.53f),.18f,Hex("B97D48"));
            }
            for(int i=0;i<5;i++)Spike(t,"Tooth",new Vector3((i-2)*.18f,1.15f,1.23f),new Vector3((i-2)*.18f,.99f,1.27f),.07f,Cream);
        }
        static void BossDetails(Transform t,int type)
        {
            if(type==0)
            {
                for(int i=0;i<11;i++){float a=i*.57f;Vector3 p=new Vector3(Mathf.Sin(a)*.90f,1.1f+(i%3)*.34f,Mathf.Cos(a)*.72f);Tube(t,"BarkRidge",p-Vector3.up*.45f,p+new Vector3(.06f,.48f,.02f),.07f,Hex("A27345"));}
                for(int s=-1;s<=1;s+=2)
                {
                    Tube(t,"WoodBrow",new Vector3(s*.18f,2.82f,1.00f),new Vector3(s*.76f,2.95f,.93f),.10f,Hex("6A472F"));
                    for(int j=0;j<3;j++)Spike(t,"ArmFinger",new Vector3(s*1.87f,1.1f,.19f+j*.16f),new Vector3(s*(2.05f+j*.12f),.55f,.4f+j*.18f),.16f,Brown);
                    for(int j=0;j<5;j++)Spike(t,"LeafBeard",new Vector3(s*(.12f+j*.11f),1.95f,.94f),new Vector3(s*(.12f+j*.13f),1.35f+j*.06f,1.05f),.13f,j%2==0?Hex("6B9E56"):Hex("8CBD67"));
                }
                for(int i=0;i<8;i++)Sphere(t,"Moss",new Vector3(Mathf.Sin(i*2.4f)*.82f,.6f+i%3*.16f,Mathf.Cos(i*2.4f)*.65f),new Vector3(.48f,.22f,.45f),Hex("6F9855"));
                for(int i=0;i<3;i++)Spike(t,"CrownBranch",new Vector3((i-1)*.65f,3.8f,0),new Vector3((i-1)*1.05f,4.7f+i%2*.35f,-.15f),.17f,Brown);
            }
            else if(type==2)
            {
                for(int s=-1;s<=1;s+=2)
                {
                    for(int j=0;j<6;j++)Spike(t,"FurTuft",new Vector3(s*.8f,2.4f-j*.2f,.4f),new Vector3(s*(1.20f-j*.03f),2.07f-j*.2f,.62f),.24f,Hex("D5EAF0"));
                    Rock(t,"ArmIcePlate",new Vector3(s*1.32f,1.9f,.24f),new Vector3(.8f,.8f,.8f),Hex("7BBAD4"),s+3);
                    for(int j=0;j<3;j++)Spike(t,"ArmClaw",new Vector3(s*1.23f+(j-1)*.20f,.85f,.34f),new Vector3(s*1.23f+(j-1)*.20f,.61f,.72f),.09f,Hex("477F9E"));
                    for(int j=0;j<3;j++)Spike(t,"FrostMantle",new Vector3(s*.50f,2.5f,-.32f-j*.15f),new Vector3(s*(1.25f+j*.20f),3.0f+j*.22f,-.5f-j*.15f),.32f,Hex("8DCBDF"));
                }
                Sphere(t,"Nose",new Vector3(0,2.75f,1.28f),new Vector3(.35f,.22f,.14f),Hex("33566E"));
                for(int i=0;i<7;i++)Spike(t,"ChestFur",new Vector3((i-3)*.16f,2.1f,.83f),new Vector3((i-3)*.12f,1.35f+Mathf.Abs(i-3)*.09f,.94f),.16f,Hex("ECF4F1"));
            }
            else if(type==3)
            {
                for(int s=-1;s<=1;s+=2)
                {
                    Rock(t,"ShoulderArmor",new Vector3(s*1.15f,2.65f,-.03f),new Vector3(1.5f,1.3f,1.5f),Hex("4F5061"),s+4);
                    Rock(t,"ArmGauntlet",new Vector3(s*1.4f,1.06f,.27f),new Vector3(1.35f,1.05f,1.3f),Hex("555563"),s+7);
                    for(int j=0;j<3;j++)Rock(t,"ArmKnuckle",new Vector3(s*1.4f+(j-1)*.31f,.94f,.83f),new Vector3(.34f,.41f,.4f),Hex("383F51"),j+8);
                    for(int j=0;j<3;j++)Tube(t,"ArmLavaSeam",new Vector3(s*1.12f,1.55f+j*.16f,.58f),new Vector3(s*1.60f,1.42f+j*.13f,.62f),.035f,Hex("FFC86C"));
                    Rock(t,"CheekBasalt",new Vector3(s*.65f,3.03f,.48f),new Vector3(.55f,.85f,.62f),Hex("4E4F60"),s+6);
                    Rock(t,"BootPlate",new Vector3(s*.66f,.33f,.36f),new Vector3(1.2f,.65f,1.3f),Hex("454759"),s+10);
                    for(int j=0;j<2;j++)Spike(t,"BackVent",new Vector3(s*.68f,2.5f,-.59f),new Vector3(s*(.9f+j*.2f),3.6f+j*.25f,-.7f),.28f,Hex("555365"));
                }
                for(int i=0;i<6;i++)Rock(t,"ChestArmor",new Vector3((i%2==0?-.47f:.47f),1.3f+i/2*.48f,.60f),new Vector3(.90f,.65f,.62f),i%2==0?Hex("575566"):Hex("68616C"),i+1);
                var heart=Ring(t,"MoltenHeart",.28f,.055f,Hex("FFD47E"));heart.transform.localPosition=new Vector3(0,1.93f,.92f);heart.transform.localEulerAngles=new Vector3(90,0,0);
            }
        }
    }
}
