using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class MapGraphic : MaskableGraphic
    {
        public int radius = 4;
        public Vector2Int offset;
        float next;
        void Update() { if (Time.unscaledTime > next) { next = Time.unscaledTime + .4f; SetVerticesDirty(); } }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); var game = GameManager.Instance;
            if (game == null || !game.Ready || game.World == null) return;
            var rect = rectTransform.rect;
            float size = Mathf.Min(rect.width, rect.height) / (radius * 2 + 1);
            var center = game.World.CurrentChunk + offset;
            for (int x = -radius; x <= radius; x++) for (int z = -radius; z <= radius; z++)
            {
                var key = center + new Vector2Int(x, z);
                bool visited = game.World.Visited(key);
                Color c = visited ? game.Catalog.Biome(game.World.BiomeAt(key)).ground : new Color(.12f, .21f, .24f);
                Vector2 p = new Vector2(x * size, z * size);
                Quad(vh, p, Vector2.one * (size - 2), c);
                if(visited)
                {
                    Color trail=Color.Lerp(c,new Color(1,.89f,.67f),.55f);
                    int roads=game.World.RoadConnections(key);var directions=new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down};
                    for(int edge=0;edge<4;edge++)if((roads&(1<<edge))!=0)Quad(vh,p+directions[edge]*size*.25f,edge<2?new Vector2(size*.5f,size*.075f):new Vector2(size*.075f,size*.5f),trail);
                    var biome=game.World.BiomeAt(key);
                    if(game.World.HasWatercourse(key))Quad(vh,p+Vector2.up*size*.25f,new Vector2(size-2,size*.07f),biome==BiomeId.Forest?new Color(.25f,.65f,.63f):new Color(.97f,.40f,.21f));
                    if(biome==BiomeId.Desert)Triangle(vh,p+new Vector2(-size*.25f,size*.15f),size*.16f,new Color(.93f,.77f,.47f));
                    if(biome==BiomeId.Snow)Diamond(vh,p+new Vector2(-size*.25f,size*.15f),size*.14f,new Color(.58f,.85f,.93f));
                }
                if (key == game.World.CurrentChunk) Quad(vh, p, Vector2.one * size * .27f, Color.white);
                foreach (var site in game.World.Sites)
                    if (site.chunk == key && !site.complete)
                    {
                        Vector2 mark=p+Vector2.one*size*.22f;Color marker=site.type==3?new Color(1,.35f,.4f):new Color(1,.85f,.35f);
                        if(site.type==3||site.type==2)Diamond(vh,mark,size*.14f,marker);else if(site.type==1)Triangle(vh,mark,size*.15f,marker);else Quad(vh,mark,Vector2.one*size*.18f,marker);
                    }
            }
        }
        static void Quad(VertexHelper vh, Vector2 p, Vector2 size, Color color)
        {
            int i = vh.currentVertCount; Vector2 h = size * .5f;
            vh.AddVert(p + new Vector2(-h.x, -h.y), color, Vector2.zero); vh.AddVert(p + new Vector2(-h.x, h.y), color, Vector2.up);
            vh.AddVert(p + new Vector2(h.x, h.y), color, Vector2.one); vh.AddVert(p + new Vector2(h.x, -h.y), color, Vector2.right);
            vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i, i + 2, i + 3);
        }
        static void Triangle(VertexHelper vh,Vector2 p,float r,Color c){int i=vh.currentVertCount;vh.AddVert(p+new Vector2(0,r),c,Vector2.zero);vh.AddVert(p+new Vector2(-r,-r),c,Vector2.zero);vh.AddVert(p+new Vector2(r,-r),c,Vector2.zero);vh.AddTriangle(i,i+1,i+2);}
        static void Diamond(VertexHelper vh,Vector2 p,float r,Color c){int i=vh.currentVertCount;vh.AddVert(p+Vector2.up*r,c,Vector2.zero);vh.AddVert(p+Vector2.left*r,c,Vector2.zero);vh.AddVert(p+Vector2.down*r,c,Vector2.zero);vh.AddVert(p+Vector2.right*r,c,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
    }
}
