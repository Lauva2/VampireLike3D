using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        static void EffectGeometry(Transform t,string id,Color color)
        {
            if(id=="arc")
            {
                Vector3 previous=Vector3.zero;
                for(int i=1;i<=8;i++){Vector3 p=new Vector3(i==8?0:Mathf.Sin(i*2.4f)*.13f,.04f,i/8f);Tube(t,"BoltBorder",previous,p,.035f,Hex("9275C8"));Tube(t,"BoltCore",previous+Vector3.up*.012f,p+Vector3.up*.012f,.016f,Hex("FFF0AC"));previous=p;}return;
            }
            if(id=="slash")
            {
                for(int i=0;i<14;i++){float a=-1.2f+i*2.4f/14,b=-1.2f+(i+1)*2.4f/14;Tube(t,"SwordArc",new Vector3(Mathf.Sin(a)*.75f,.1f,Mathf.Cos(a)*.75f),new Vector3(Mathf.Sin(b)*.75f,.1f,Mathf.Cos(b)*.75f),.02f+Mathf.Sin(i/14f*Mathf.PI)*.035f,Hex("99ECE8"));}return;
            }
            if(id=="leafburst"||id=="sand"||id=="iceburst"||id=="embers")
            {
                for(int i=0;i<18;i++)
                {
                    float a=i*2.399f;Vector3 p=new Vector3(Mathf.Sin(a)*(.25f+i%3*.18f),.2f+i%4*.17f,Mathf.Cos(a)*(.25f+i%3*.18f));
                    if(id=="leafburst")Spike(t,"FallingLeaf",p,p+new Vector3(.12f,.18f,.12f),.10f,i%2==0?Hex("86BD6A"):Gold);
                    else if(id=="iceburst")Rock(t,"IceFragment",p,new Vector3(.17f,.38f,.18f),i%2==0?Hex("8FDCE7"):Hex("D3F3EF"),i);
                    else if(id=="sand")Sphere(t,"SandPuff",p,new Vector3(.38f,.20f,.32f),i%2==0?Hex("E6BD7C"):Hex("CFA26A"));
                    else Rock(t,"CoolingEmber",p,Vector3.one*(.14f+i%3*.05f),i%3==0?Orange:Hex("4F4C5C"),i);
                }
                Ring(t,"DissolveWave",.5f,.025f,id=="iceburst"?Hex("ABDCEB"):id=="leafburst"?Teal:Gold);return;
            }
            if(id=="lightning")
            {
                var points=new[]{new Vector3(.1f,2.4f,0),new Vector3(-.19f,1.6f,0),new Vector3(.15f,1.65f,0),new Vector3(-.12f,.7f,0),new Vector3(.12f,.76f,0),Vector3.zero};
                for(int i=0;i<points.Length-1;i++){Tube(t,"BoltEdge",points[i],points[i+1],.052f,Hex("B17AE3"));Tube(t,"Bolt",points[i]+Vector3.back*.025f,points[i+1]+Vector3.back*.025f,.028f,Hex("FFF0AD"));}
                for(int i=0;i<5;i++){float a=i*1.26f;Spike(t,"ElectricSpark",Vector3.zero,new Vector3(Mathf.Sin(a)*.6f,.1f,Mathf.Cos(a)*.6f),.04f,Gold);}return;
            }
            if(id=="impact")
            {
                for(int i=0;i<7;i++){float a=i*.897f;Spike(t,"HitStar",new Vector3(Mathf.Sin(a)*.1f,0,Mathf.Cos(a)*.1f),new Vector3(Mathf.Sin(a)*.55f,.18f,Mathf.Cos(a)*.55f),.055f,i%2==0?Gold:Cream);}return;
            }
            if(id=="flame"||id=="explosion")
            {
                if(id=="explosion")Ring(t,"Shockwave",.5f,.045f,Gold);
                for(int i=0;i<8;i++){float a=i*.785f;Vector3 p=new Vector3(Mathf.Sin(a)*.32f,.05f,Mathf.Cos(a)*.32f);Spike(t,"FlameTongue",p,p+new Vector3(Mathf.Sin(a)*.22f,.35f+i%3*.13f,Mathf.Cos(a)*.22f),.12f,i%2==0?Orange:Gold);}
                Sphere(t,"HotCore",new Vector3(0,.17f,0),Vector3.one*.35f,Hex("FFE6A1"));return;
            }
            if(id=="frost")
            {
                Ring(t,"FrostWave",.5f,.025f,Hex("55A9D1"));
                for(int i=0;i<8;i++){float a=i*.785f;Vector3 p=new Vector3(Mathf.Sin(a)*.4f,.02f,Mathf.Cos(a)*.4f);Spike(t,"IceShard",p,p+new Vector3(Mathf.Sin(a)*.15f,.26f,Mathf.Cos(a)*.15f),.065f,i%2==0?Hex("79D8F0"):Cream);}return;
            }
            if(id=="roots")
            {
                for(int i=0;i<5;i++){float a=i*1.26f;Vector3 p=new Vector3(Mathf.Sin(a)*.4f,0,Mathf.Cos(a)*.4f);Spike(t,"RootStrike",p,new Vector3(Mathf.Sin(a)*.15f,.65f+i*.07f,Mathf.Cos(a)*.15f),.12f,i%2==0?Brown:Teal);}return;
            }
            if(id=="rune")
            {
                Ring(t,"RuneCircle",.5f,.018f,Gold);
                for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;Cube(t,"SealCorner",new Vector3(Mathf.Sin(a)*.35f,.02f,Mathf.Cos(a)*.35f),new Vector3(.25f,.015f,.035f),Gold,new Vector3(0,-i*90+45,0));}
                return;
            }
            Ring(t,"Halo",.5f,id=="player"?.035f:.025f,id=="player"?Hex("3AEEE1"):color);
            if(id=="player")for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;Spike(t,"PlayerPointer",new Vector3(Mathf.Sin(a)*.68f,0,Mathf.Cos(a)*.68f),new Vector3(Mathf.Sin(a)*.53f,.01f,Mathf.Cos(a)*.53f),.08f,Cream);}
            if(id=="dodge")for(int i=0;i<4;i++)Cube(t,"SpeedStreak",new Vector3((i-1.5f)*.2f,.12f,-.25f-i*.1f),new Vector3(.04f,.045f,.65f),Cream);
            if(id=="mark")for(int i=0;i<4;i++){float a=i*1.57f;Spike(t,"TargetPointer",new Vector3(Mathf.Sin(a)*.7f,0,Mathf.Cos(a)*.7f),new Vector3(Mathf.Sin(a)*.53f,0,Mathf.Cos(a)*.53f),.08f,Gold);}
        }
    }
}
