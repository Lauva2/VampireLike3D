using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    // The AA prefab supplies the renderer/material; each pooled chunk owns its generated mesh.
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public sealed class PathNetwork : MonoBehaviour
    {
        readonly List<Vector3> vertices=new List<Vector3>(640),centers=new List<Vector3>(128);
        readonly List<Color> colors=new List<Color>(640);
        readonly List<int> triangles=new List<int>(1600);
        Mesh mesh;
        bool hasWater;float waterStart,waterEnd,waterWorldX,bridgeOffset;
        public static float BridgeOffset(Vector2Int key)=>Mathf.Sin(key.x*32*Mathf.PI/64)*.8f;
        static float ChannelZ(float x,float worldX)=>8+Mathf.Sin(x*Mathf.PI/16)*1.1f+Mathf.Sin((worldX+x)*Mathf.PI/64)*.8f;
        static float ChannelWidth(float x,float worldX)=>2.65f*(.95f+Mathf.Sin((worldX+x)*Mathf.PI/43)*.13f);
        float WaterZ(float x)=>ChannelZ(x,waterWorldX);
        float WaterWidth(float x)=>ChannelWidth(x,waterWorldX);
        public static bool NearChannel(Vector3 local,int chunkX,float margin)
        {
            float z=ChannelZ(local.x,chunkX*32);return Mathf.Abs(local.z-z)<ChannelWidth(local.x,chunkX*32)*.5f+margin;
        }
        public static Color RoadColor(BiomeId biome)
        {
            string code=biome==BiomeId.Forest?"C3B58A":biome==BiomeId.Desert?"E8CE96":biome==BiomeId.Snow?"DBE7E3":"8A7780";
            ColorUtility.TryParseHtmlString("#"+code,out var result);return result;
        }
        void Awake(){mesh=new Mesh{name="Owned generated path network"};mesh.MarkDynamic();GetComponent<MeshFilter>().sharedMesh=mesh;}
        public static Vector2Int Parent(int seed,Vector2Int key)
        {
            if(key==Vector2Int.zero)return key;
            bool horizontal=key.y==0||key.x!=0&&WorldService.SeedHash(seed,key.x,key.y)%2==0;
            return key+(horizontal?new Vector2Int(key.x>0?-1:1,0):new Vector2Int(0,key.y>0?-1:1));
        }
        public static bool Connected(int seed,Vector2Int a,Vector2Int b)
        {
            if(Parent(seed,a)==b||Parent(seed,b)==a)return true;
            Vector2Int first=a.x<b.x||a.x==b.x&&a.y<b.y?a:b;
            return WorldService.SeedHash(seed^0x5a19,first.x*2+(a.x==b.x?1:0),first.y)%13==0;
        }
        public static Vector3 EdgeAnchor(int seed,Vector2Int key,Vector2Int direction)
        {
            Vector2Int next=key+direction,first=direction.x<0||direction.y<0?next:key;
            float offset=(WorldService.SeedHash(seed^0x27d1,first.x*2+(direction.x==0?1:0),first.y)%1001/1000f-.5f)*8;
            return new Vector3(direction.x*16+(direction.x==0?offset:0),0,direction.y*16+(direction.y==0?offset:0));
        }
        public void Configure(int seed,Vector2Int key,int mask,BiomeId biome,BiomeId[] adjacent,bool river,bool waterWest,bool waterEast,bool landmark,bool sanctuary=false)
        {
            vertices.Clear();colors.Clear();triangles.Clear();centers.Clear();
            hasWater=river;
            waterWorldX=key.x*32;bridgeOffset=BridgeOffset(key);
            if(river)Water(biome,waterWest,waterEast);
            var random=new System.Random(WorldService.SeedHash(seed^0x17fa,key.x,key.y));
            Vector3 node=sanctuary?new Vector3(4.8f,0,-4.8f):landmark?Vector3.zero:new Vector3((float)random.NextDouble()*10-5,0,(float)random.NextDouble()*10-5);
            Color road=RoadColor(biome);Disc(node,1.45f,.068f,road,true);
            var directions=new[]{Vector3.right,Vector3.left,Vector3.forward,Vector3.back};
            for(int edge=0;edge<4;edge++)
            {
                if((mask&(1<<edge))==0)continue;
                var direction=new Vector2Int(Mathf.RoundToInt(directions[edge].x),Mathf.RoundToInt(directions[edge].z));
                Vector3 end=EdgeAnchor(seed,key,direction);
                Color ending=Color.Lerp(road,RoadColor(adjacent[edge]),.5f);
                if(sanctuary&&edge==1)
                {
                    Vector3 approach=new Vector3(-5,0,-4.8f);
                    Curve(node,node+Vector3.left*3,approach+Vector3.right*3,approach,road,road);
                    Curve(approach,approach+Vector3.left*4,end-Vector3.left*5,end,road,ending);continue;
                }
                if(sanctuary&&edge==2)
                {
                    Vector3 shoulder=new Vector3(6,0,3.6f);
                    Curve(node,node+Vector3.forward*4,shoulder-Vector3.forward*3,shoulder,road,road);
                    if(river)
                    {
                        Vector3 bridge=new Vector3(0,0,6+bridgeOffset);
                        Curve(shoulder,shoulder+Vector3.forward*2,bridge-Vector3.forward*1.5f,bridge,road,road);
                        Curve(bridge,bridge+Vector3.forward*3,end-Vector3.forward*5,end,road,ending);
                    }
                    else Curve(shoulder,shoulder+Vector3.forward*4,end-Vector3.forward*5,end,road,ending);
                    continue;
                }
                if(river&&edge==2)
                {
                    Curve(node,node+Vector3.forward*3,new Vector3(0,0,4+bridgeOffset),new Vector3(0,0,6+bridgeOffset),road,road);
                    Curve(new Vector3(0,0,6+bridgeOffset),new Vector3(0,0,9+bridgeOffset),end-Vector3.forward*5,end,road,ending);
                }
                else
                {
                    Vector3 bend=Vector3.Cross(Vector3.up,directions[edge])*((float)random.NextDouble()*5-2.5f);
                    Curve(node,Vector3.Lerp(node,end,.35f)+bend,end-directions[edge]*5,end,road,ending);
                }
            }
            Publish();
        }
        public void ConnectArchitecture(Vector3 entrance,BiomeId biome)
        {
            if(centers.Count==0)return;
            Vector3 nearest=NearestPoint(entrance);
            Color color=RoadColor(biome);Curve(nearest,Vector3.Lerp(nearest,entrance,.3f),Vector3.Lerp(nearest,entrance,.7f),entrance,color,color);
            Disc(nearest,1.38f,.068f,color,true);Disc(entrance,1.32f,.068f,color,true);
            Publish();
        }
        public Vector3 NearestPoint(Vector3 position)
        {
            if(centers.Count==0)return Vector3.zero;
            Vector3 nearest=centers[0];float best=float.MaxValue;
            foreach(var center in centers){float distance=(position-center).sqrMagnitude;if(distance<best){best=distance;nearest=center;}}
            return nearest;
        }
        void Publish()
        {
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        }
        public bool NearPath(Vector3 local,float margin)
        {
            foreach(var center in centers){Vector3 d=local-center;d.y=0;if(d.sqrMagnitude<margin*margin)return true;}return false;
        }
        public bool ContainsWater(Vector3 local,float margin=0)
        {
            if(!hasWater)return false;float x=Mathf.Clamp(local.x,waterStart,waterEnd),z=WaterZ(x),radius=WaterWidth(x)*.5f+margin;
            return new Vector2(local.x-x,local.z-z).sqrMagnitude<radius*radius;
        }
        float GroundHeight(Vector3 p)
        {
            Vector3 world=transform.TransformPoint(p);
            return Physics.Raycast(world+Vector3.up*8,Vector3.down,out var hit,20,1<<0)?hit.point.y-transform.position.y:0;
        }
        void Vertex(Vector3 p,Color color){vertices.Add(p);Color packed=color.linear;packed.a=.04f;colors.Add(packed);}
        void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color first,Color last)
        {
            int index=vertices.Count;Vertex(a,first);Vertex(b,first);Vertex(c,last);Vertex(d,last);
            bool up=Vector3.Cross(c-a,b-a).y>=0;
            triangles.Add(index);triangles.Add(index+(up?2:1));triangles.Add(index+(up?1:2));triangles.Add(index+1);triangles.Add(index+(up?2:3));triangles.Add(index+(up?3:2));
        }
        void Curve(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color,Color ending)
        {
            const int steps=64,across=6;var previous=new Vector3[across+1];
            for(int i=0;i<=steps;i++)
            {
                float t=i/(float)steps,s=1-t;Vector3 p=s*s*s*a+3*s*s*t*b+3*s*t*t*c+t*t*t*d;
                Vector3 tangent=(3*s*s*(b-a)+6*s*t*(c-b)+3*t*t*(d-c)).normalized;
                Vector3 side=Vector3.Cross(Vector3.up,tangent).normalized*1.3f;
                centers.Add(p);Vector3 last=default,old=default;
                for(int j=0;j<=across;j++)
                {
                    Vector3 current=p+side*(j*2f/across-1);current.y=GroundHeight(current)+.065f;
                    if(i>0&&j>0)Quad(old,previous[j],last,current,Color.Lerp(color,ending,Mathf.SmoothStep(0,1,(i-1)/(float)steps)),Color.Lerp(color,ending,Mathf.SmoothStep(0,1,t)));
                    old=previous[j];previous[j]=current;last=current;
                }
            }
        }
        void Disc(Vector3 center,float radius,float height,Color color,bool followGround)
        {
            const int count=32;int start=vertices.Count;center.y=(followGround?GroundHeight(center):0)+height;Vertex(center,color);
            for(int i=0;i<=count;i++){float a=i*Mathf.PI*2/count;Vector3 p=center+new Vector3(Mathf.Sin(a),0,Mathf.Cos(a))*radius;if(followGround)p.y=GroundHeight(p)+height;Vertex(p,color);if(i>0){triangles.Add(start);triangles.Add(start+i);triangles.Add(start+i+1);}}
        }
        void Water(BiomeId biome,bool west,bool east)
        {
            bool forest=biome==BiomeId.Forest;
            ColorUtility.TryParseHtmlString(forest?"#6D8977":"#3D384A",out var bank);ColorUtility.TryParseHtmlString(forest?"#4BADA6":"#ED6037",out var water);
            float start=west?-16:-11,end=east?16:11;
            waterStart=start;waterEnd=end;
            Color shore=forest?new Color(.52f,.61f,.39f):new Color(.43f,.40f,.47f);
            for(int layer=0;layer<3;layer++)
            {
                float extra=layer==0?1.9f:layer==1?.65f:0,height=.025f+layer*.025f;Color color=layer==0?shore:layer==1?bank:water;
                int steps=Mathf.CeilToInt((end-start)*2);
                for(int i=0;i<steps;i++)
                {
                    float x=Mathf.Lerp(start,end,i/(float)steps),nx=Mathf.Lerp(start,end,(i+1)/(float)steps);
                    float z=WaterZ(x),nz=WaterZ(nx),width=WaterWidth(x)+extra,nextWidth=WaterWidth(nx)+extra;
                    Quad(new Vector3(x,height,z-width*.5f),new Vector3(x,height,z+width*.5f),new Vector3(nx,height,nz-nextWidth*.5f),new Vector3(nx,height,nz+nextWidth*.5f),color,color);
                }
                if(!west)Disc(new Vector3(start,0,WaterZ(start)),(WaterWidth(start)+extra)*.5f,height+.0002f,color,false);
                if(!east)Disc(new Vector3(end,0,WaterZ(end)),(WaterWidth(end)+extra)*.5f,height+.0002f,color,false);
            }
            for(float x=start+1.5f;x<end-1;x+=3.7f)
            {
                if(Mathf.Abs(x)<2)continue;
                for(int side=-1;side<=1;side+=2)
                {
                    float radius=.28f+.18f*(.5f+.5f*Mathf.Sin((waterWorldX+x)*1.7f+side));
                    Boulder(new Vector3(x,.04f,WaterZ(x)+side*(WaterWidth(x)*.5f+.38f)),radius,forest?bank:shore);
                }
            }
        }
        void Boulder(Vector3 center,float radius,Color color)
        {
            int start=vertices.Count;Vertex(center+Vector3.up*radius*.65f,color);
            const int count=7;
            for(int i=0;i<=count;i++){float a=i*Mathf.PI*2/count;Vertex(center+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius*.8f),Color.Lerp(color,Color.white,.08f));if(i>0){triangles.Add(start);triangles.Add(start+i);triangles.Add(start+i+1);}}
        }
        void OnDestroy(){if(mesh!=null){if(Application.isPlaying)Destroy(mesh);else DestroyImmediate(mesh);}}
    }
}
