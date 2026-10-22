using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    // Resolution independent pictograms, authored inside the AA UI prefab.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AbilityIcon : MaskableGraphic
    {
        [SerializeField] string symbol = "星";
        VertexHelper mesh;
        Vector2 origin;
        float scale;
        public void Set(string value) { if (symbol == value) return; symbol = value; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); mesh = vh; var r = rectTransform.rect; origin = r.center; scale = Mathf.Min(r.width, r.height) / 2;
            switch (symbol)
            {
                case "弹": case "攻":
                    Poly(new Vector2(-.72f,.15f),new Vector2(-.6f,.48f),new Vector2(.6f,.48f),new Vector2(.76f,.25f),new Vector2(.56f,.06f),new Vector2(.02f,.06f),new Vector2(-.12f,-.58f),new Vector2(-.47f,-.58f),new Vector2(-.32f,.1f));
                    Line(new Vector2(.24f,.05f),new Vector2(.26f,-.18f),.07f); Line(new Vector2(.26f,-.18f),new Vector2(-.02f,-.18f),.07f); break;
                case "焰":
                    Poly(new Vector2(-.48f,-.4f),new Vector2(-.55f,.03f),new Vector2(-.18f,.43f),new Vector2(-.12f,.78f),new Vector2(.25f,.39f),new Vector2(.16f,.05f),new Vector2(.48f,.28f),new Vector2(.56f,-.2f),new Vector2(.35f,-.53f),new Vector2(0,-.7f)); break;
                case "剑":
                    Poly(new Vector2(.55f,.75f),new Vector2(.43f,.1f),new Vector2(-.22f,-.45f),new Vector2(-.42f,-.25f),new Vector2(.1f,.43f));
                    Line(new Vector2(-.56f,-.06f),new Vector2(.07f,-.69f),.13f); Line(new Vector2(-.38f,-.43f),new Vector2(-.64f,-.69f),.17f); break;
                case "灵":
                    Disc(Vector2.zero,.43f,24); Poly(new Vector2(-.3f,0),new Vector2(-.85f,.3f),new Vector2(-.63f,-.22f),new Vector2(-.28f,-.33f));
                    Poly(new Vector2(.3f,0),new Vector2(.85f,.3f),new Vector2(.63f,-.22f),new Vector2(.28f,-.33f));
                    Star(new Vector2(0,.67f),.16f,4); break;
                case "炮":
                    Disc(new Vector2(-.08f,-.16f),.5f,24); Line(new Vector2(.17f,.28f),new Vector2(.4f,.55f),.16f); Line(new Vector2(.4f,.55f),new Vector2(.63f,.51f),.08f); Star(new Vector2(.69f,.68f),.21f,4); break;
                case "符":
                    Line(new Vector2(-.42f,-.72f),new Vector2(-.42f,.65f),.1f); Line(new Vector2(-.42f,.65f),new Vector2(.42f,.65f),.1f); Line(new Vector2(.42f,.65f),new Vector2(.42f,-.72f),.1f); Line(new Vector2(.42f,-.72f),new Vector2(-.42f,-.72f),.1f);
                    Line(new Vector2(-.23f,.25f),new Vector2(.23f,.25f),.1f); Line(new Vector2(0,.43f),new Vector2(0,-.47f),.1f); Line(new Vector2(-.19f,-.12f),new Vector2(.2f,-.36f),.1f); break;
                case "弩":
                    Line(new Vector2(0,-.7f),new Vector2(0,.65f),.13f);Line(new Vector2(-.7f,.3f),new Vector2(-.4f,.62f),.12f);Line(new Vector2(-.4f,.62f),new Vector2(.4f,.62f),.12f);Line(new Vector2(.4f,.62f),new Vector2(.7f,.3f),.12f);Line(new Vector2(-.7f,.3f),new Vector2(.7f,.3f),.06f);Poly(new Vector2(0,.85f),new Vector2(-.18f,.5f),new Vector2(.18f,.5f));break;
                case "旋":
                    Poly(new Vector2(-.8f,-.45f),new Vector2(-.32f,.56f),new Vector2(.02f,.7f),new Vector2(.3f,.48f),new Vector2(.78f,-.4f),new Vector2(.43f,-.21f),new Vector2(.04f,.14f),new Vector2(-.42f,-.22f));break;
                case "杖":
                    Line(new Vector2(-.4f,-.75f),new Vector2(.2f,.3f),.15f);Poly(new Vector2(.22f,.84f),new Vector2(-.12f,.46f),new Vector2(.2f,.13f),new Vector2(.58f,.5f));break;
                case "菌":
                    Line(new Vector2(0,-.7f),new Vector2(0,.15f),.25f);Poly(new Vector2(-.8f,.05f),new Vector2(-.55f,.55f),new Vector2(0,.78f),new Vector2(.55f,.55f),new Vector2(.8f,.05f));break;
                case "晶":
                    Poly(new Vector2(0,.85f),new Vector2(-.48f,.08f),new Vector2(0,-.85f),new Vector2(.48f,.08f));break;
                case "林":
                    Line(new Vector2(0,-.75f),new Vector2(0,.5f),.18f); Disc(new Vector2(-.28f,.14f),.36f,16); Disc(new Vector2(.28f,.14f),.36f,16); Disc(new Vector2(0,.46f),.4f,16); break;
                case "沙":
                    Disc(new Vector2(.42f,.48f),.22f,20); Poly(new Vector2(-.8f,-.62f),new Vector2(-.23f,.3f),new Vector2(.35f,-.62f)); Poly(new Vector2(-.03f,-.62f),new Vector2(.45f,.12f),new Vector2(.86f,-.62f)); break;
                case "雪":
                    for (int i=0;i<6;i++) { Vector2 d=Dir(i*60); Line(-d*.12f,d*.73f,.11f); Vector2 side=Dir(i*60+50); Line(d*.45f,d*.45f-side*.23f,.08f); } break;
                case "象": for(int i=0;i<4;i++) Star(Dir(i*90)*.43f,.3f,4); break;
                case "生":
                    Poly(new Vector2(0,-.68f),new Vector2(-.65f,.08f),new Vector2(-.65f,.4f),new Vector2(-.34f,.6f),new Vector2(0,.34f),new Vector2(.34f,.6f),new Vector2(.65f,.4f),new Vector2(.65f,.08f)); break;
                case "速": Poly(new Vector2(.23f,.78f),new Vector2(-.57f,-.04f),new Vector2(-.03f,-.04f),new Vector2(-.25f,-.78f),new Vector2(.57f,.14f),new Vector2(.03f,.14f)); break;
                case "引": Ring(.57f,.11f); Disc(Vector2.zero,.2f,16); break;
                case "足":Poly(new Vector2(-.5f,-.6f),new Vector2(-.6f,-.3f),new Vector2(-.3f,.6f),new Vector2(.05f,.6f),new Vector2(.08f,-.18f),new Vector2(.55f,-.35f),new Vector2(.67f,-.6f));break;
                case "盾":Poly(new Vector2(-.6f,.5f),new Vector2(0,.72f),new Vector2(.6f,.5f),new Vector2(.51f,-.2f),new Vector2(0,-.72f),new Vector2(-.51f,-.2f));break;
                case "距":Ring(.5f,.1f);for(int i=0;i<4;i++)Line(Dir(i*90)*.33f,Dir(i*90)*.8f,.1f);break;
                case "叠":for(int i=0;i<3;i++)Poly(new Vector2(-.65f+i*.48f,-.63f),new Vector2(-.65f+i*.48f,.18f),new Vector2(-.48f+i*.48f,.65f),new Vector2(-.31f+i*.48f,.18f),new Vector2(-.31f+i*.48f,-.63f));break;
                case "锁":Poly(new Vector2(-.6f,-.7f),new Vector2(-.6f,.18f),new Vector2(.6f,.18f),new Vector2(.6f,-.7f));for(int i=0;i<15;i++)Line(Dir(i*180f/15)*.36f+Vector2.up*.23f,Dir((i+1)*180f/15)*.36f+Vector2.up*.23f,.14f);break;
                case "成": Line(new Vector2(-.57f,-.02f),new Vector2(-.18f,-.43f),.19f); Line(new Vector2(-.18f,-.43f),new Vector2(.61f,.47f),.19f); break;
                case "途": Line(new Vector2(-.43f,-.67f),new Vector2(.43f,.67f),.12f); Star(new Vector2(.43f,.67f),.23f,4); Disc(new Vector2(-.43f,-.67f),.18f,16); break;
                default: Star(Vector2.zero,.78f,5); break;
            }
        }
        static Vector2 Dir(float degrees) { float a=degrees*Mathf.Deg2Rad; return new Vector2(Mathf.Cos(a),Mathf.Sin(a)); }
        void Vertex(Vector2 p) { mesh.AddVert(origin+p*scale,color,Vector2.zero); }
        void Poly(params Vector2[] points)
        {
            int first=mesh.currentVertCount; Vector2 center=Vector2.zero; foreach(var p in points) center+=p; center/=points.Length; Vertex(center);
            foreach(var p in points) Vertex(p);
            for(int i=0;i<points.Length;i++) mesh.AddTriangle(first,first+i+1,first+(i+1)%points.Length+1);
        }
        void Line(Vector2 a,Vector2 b,float thickness) { Vector2 n=new Vector2(-(b-a).y,(b-a).x).normalized*thickness*.5f; Poly(a-n,b-n,b+n,a+n); }
        void Disc(Vector2 c,float radius,int n) { var p=new Vector2[n];for(int i=0;i<n;i++)p[i]=c+Dir(i*360f/n)*radius;Poly(p); }
        void Ring(float r,float width) { for(int i=0;i<28;i++) Line(Dir(i*360f/28)*r,Dir((i+1)*360f/28)*r,width); }
        void Star(Vector2 c,float r,int tips) { var p=new Vector2[tips*2]; for(int i=0;i<p.Length;i++)p[i]=c+Dir(90+i*180f/tips)*r*(i%2==0?1:.43f); Poly(p); }
    }
}
