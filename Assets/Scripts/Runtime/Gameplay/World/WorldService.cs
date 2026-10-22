using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class WorldSite
    {
        public string id;
        public int type, kills;
        public Vector2Int chunk;
        public Vector3 position;
        public BiomeId biome;
        public bool complete, started;
        public GameObject visual;
    }
    public sealed class WorldService : MonoBehaviour
    {
        public const int ChunkSize = 32;
        sealed class Region
        {
            public AssetScope scope;
            public PoolService pool;
            public int count;
            public bool loading;
        }
        sealed class Chunk
        {
            public GameObject root,ground,architecture;
            public Region region;
            public PathNetwork path;
            public bool constructing;
            public readonly List<GameObject> instances = new List<GameObject>();
            public WorldSite site;
        }
        GameManager game;
        WorldJournal journal;
        readonly Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
        readonly Dictionary<BiomeId, Region> regions = new Dictionary<BiomeId, Region>();
        readonly Queue<Vector2Int> queue = new Queue<Vector2Int>();
        readonly HashSet<Vector2Int> queued = new HashSet<Vector2Int>();
        readonly List<Vector2Int> remove = new List<Vector2Int>();
        readonly List<WorldSite> sites = new List<WorldSite>();
        public IReadOnlyList<WorldSite> Sites => sites;
        public string InteractionText { get; private set; }
        public BiomeId CurrentBiome { get; private set; }
        public Vector2Int CurrentChunk { get; private set; }
        public Vector2Int OriginChunk { get; private set; }
        public int ActiveChunks => chunks.Count;
        public bool ViewReady
        {
            get{for(int x=-2;x<=2;x++)for(int z=-2;z<=2;z++)if(!chunks.TryGetValue(CurrentChunk+new Vector2Int(x,z),out var chunk)||chunk.constructing)return false;return true;}
        }
        public string LoadingError{get;private set;}
        public Vector2 GlobalPosition => new Vector2(OriginChunk.x * ChunkSize + game.Player.transform.position.x, OriginChunk.y * ChunkSize + game.Player.transform.position.z);
        bool building, disposed;
        float nextRegionalHazard;

        public IEnumerator Initialize(GameManager manager)
        {
            game = manager; CurrentBiome = game.Run.biome == BiomeId.Mixed ? BiomeAt(Vector2Int.zero) : game.Run.biome;
            journal = new WorldJournal(game.Run.id);
            journal.Visit(Vector2Int.zero);
            for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z++){yield return BuildChunk(new Vector2Int(x,z));if(!string.IsNullOrEmpty(LoadingError))yield break;}
            Schedule(Vector2Int.zero);
        }
        public static int SeedHash(int seed, int x, int z)
        {
            unchecked { uint h = (uint)seed ^ (uint)x * 374761393u ^ (uint)z * 668265263u; h = (h ^ (h >> 13)) * 1274126177u; return (int)(h ^ (h >> 16)) & int.MaxValue; }
        }
        public BiomeId BiomeAt(Vector2Int key)
        {
            if (game.Run.biome != BiomeId.Mixed) return game.Run.biome;
            // Jittered regions form broad, irregular destinations rather than parallel stripes.
            int cellX=Mathf.FloorToInt(key.x/5f),cellZ=Mathf.FloorToInt(key.y/5f);float best=float.MaxValue;int selected=0;
            for(int x=cellX-1;x<=cellX+1;x++)for(int z=cellZ-1;z<=cellZ+1;z++)
            {
                int hash=SeedHash(game.Run.seed,x,z);
                Vector2 site=new Vector2((x+.2f+(hash%997)/1661f)*5,(z+.2f+((hash/997)%997)/1661f)*5);
                float distance=(site-new Vector2(key.x,key.y)).sqrMagnitude;
                if(distance<best){best=distance;selected=((x+z*2)%4+4)%4;}
            }
            return (BiomeId)selected;
        }
        public bool Visited(Vector2Int key) => journal != null && journal.Visited(key);
        public Vector3 LocalPosition(Vector2Int chunk) => new Vector3((chunk.x - OriginChunk.x) * ChunkSize, 0, (chunk.y - OriginChunk.y) * ChunkSize);
        IEnumerator BuildChunk(Vector2Int key)
        {
            if (disposed || chunks.ContainsKey(key)) yield break;
            BiomeId biome = BiomeAt(key);
            if (!regions.TryGetValue(biome, out Region region))
            {
                region = new Region { scope = new AssetScope("biome-" + biome), pool = new PoolService(transform), loading = true };
                regions[biome] = region;
                var data = game.Catalog.Biome(biome);
                yield return game.Assets.LoadPrefab("path/network",region.scope,p=>RegisterRegion(region,"path/network",p));
                yield return game.Assets.LoadPrefab(data.terrainId, region.scope, p => RegisterRegion(region,data.terrainId,p));
                foreach(string suffix in new[]{"-wild","-ridge"}){string terrain=data.terrainId+suffix;yield return game.Assets.LoadPrefab(terrain,region.scope,p=>RegisterRegion(region,terrain,p));}
                yield return game.Assets.LoadPrefab(data.decorationId, region.scope, p => RegisterRegion(region,data.decorationId, p));
                string architecture="architecture/"+new[]{"forest","desert","snow","volcano"}[(int)biome];
                yield return game.Assets.LoadPrefab(architecture, region.scope, p => RegisterRegion(region,architecture, p));
                yield return game.Assets.LoadPrefab(data.bossId, game.RunAssets, p => {if(p==null)LoadingError="缺少挑战资源";game.Pool.Register(data.bossId,p);});
                region.loading = false;
            }
            if (disposed||!string.IsNullOrEmpty(LoadingError)) yield break;
            var chunk = new Chunk { region = region, root = new GameObject("Chunk " + key), constructing = true };
            chunk.root.transform.SetParent(transform, false); chunk.root.transform.position = LocalPosition(key);
            region.count++; chunks.Add(key, chunk);
            var config = game.Catalog.Biome(biome);
            int terrainVariant=TerrainVariant(key,biome);
            string terrainId=config.terrainId+(terrainVariant==0?"":terrainVariant==1?"-wild":"-ridge");
            var ground = region.pool.Rent(terrainId, chunk.root.transform.position, Quaternion.identity);
            chunk.ground=ground;
            if(ground==null){LoadingError="地图区块创建失败: "+terrainId;yield break;}
            if (ground != null)
            {
                ground.transform.SetParent(chunk.root.transform, true); chunk.instances.Add(ground);
                bool fixedWaterway=terrainVariant==0&&(biome==BiomeId.Forest||biome==BiomeId.Volcano);
                ground.GetComponent<TerrainAppearance>()?.Orient(fixedWaterway?0:SeedHash(game.Run.seed^0x51a,key.x,key.y)%4);
                var block=new MaterialPropertyBlock();block.SetFloat("_Terrain",1);
                var directions=new[]{Vector2Int.right,Vector2Int.left,Vector2Int.up,Vector2Int.down};var names=new[]{"_EdgeEast","_EdgeWest","_EdgeNorth","_EdgeSouth"};
                Vector4 blend=Vector4.zero;
                for(int edge=0;edge<4;edge++) {var adjacent=BiomeAt(key+directions[edge]);blend[edge]=adjacent!=biome?1:0;Color c=Color.Lerp(config.ground,game.Catalog.Biome(adjacent).ground,.5f).linear;block.SetVector(names[edge],new Vector4(c.r,c.g,c.b,1));}
                block.SetVector("_BlendEdges",blend);
                block.SetVector("_WorldOrigin",new Vector4(key.x*ChunkSize,key.y*ChunkSize,0,0));
                block.SetFloat("_OrganicTerrain",game.Run.biome==BiomeId.Mixed?1:0);
                block.SetVector("_RegionCenter",(Vector4)config.ground.linear);
                for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++)
                    if(dx!=0||dz!=0)block.SetVector("_Region"+(dx+1)+(dz+1),(Vector4)game.Catalog.Biome(BiomeAt(key+new Vector2Int(dx,dz))).ground.linear);
                foreach(var renderer in ground.GetComponentsInChildren<Renderer>())renderer.SetPropertyBlock(block);
            }
            var random = new System.Random(SeedHash(game.Run.seed, key.x, key.y));
            Physics.SyncTransforms();
            if(game.Run.biome==BiomeId.Mixed)
            {
                var directions=new[]{Vector2Int.right,Vector2Int.left,Vector2Int.up,Vector2Int.down};
                for(int edge=0;edge<4;edge++)
                {
                    var adjacent=BiomeAt(key+directions[edge]);if(adjacent==biome)continue;
                    string decorationId=game.Catalog.Biome(adjacent).decorationId;
                    if(!region.scope.keys.Contains(decorationId))yield return game.Assets.LoadPrefab(decorationId,region.scope,p=>RegisterRegion(region,decorationId,p));
                    for(int i=0;i<6;i++)
                    {
                        float along=-13+(float)random.NextDouble()*26;float inward=.8f+(float)random.NextDouble()*7.5f;
                        Vector3 offset=edge<2?new Vector3((edge==0?1:-1)*(16-inward),0,along):new Vector3(along,0,(edge==2?1:-1)*(16-inward));
                        if(RiverAt(key)&&PathNetwork.NearChannel(offset,key.x,1.5f))continue;
                        var transition=region.pool.Rent(decorationId,GroundPosition(chunk.root.transform.position+offset),Quaternion.Euler(0,i*137+edge*43,0));
                        if(transition==null)continue;
                        transition.transform.SetParent(chunk.root.transform,true);transition.transform.localScale=Vector3.one*(.55f+i%3*.12f);
                        transition.GetComponent<DecorationVariant>()?.Select(i%3==0?0:1);chunk.instances.Add(transition);
                    }
                }
            }
            int landmarkType = SiteType(key, random);
            var pathObject=region.pool.Rent("path/network",chunk.root.transform.position,Quaternion.identity);
            if(pathObject==null){LoadingError="道路区块创建失败";yield break;}
            pathObject.transform.SetParent(chunk.root.transform,true);chunk.instances.Add(pathObject);
            var path=pathObject.GetComponent<PathNetwork>();chunk.path=path;var edgeDirections=new[]{Vector2Int.right,Vector2Int.left,Vector2Int.up,Vector2Int.down};var adjacentBiomes=new BiomeId[4];int roadMask=0;
            bool river=RiverAt(key);
            for(int edge=0;edge<4;edge++){var next=key+edgeDirections[edge];adjacentBiomes[edge]=BiomeAt(next);if(PathNetwork.Connected(game.Run.seed,key,next))roadMask|=1<<edge;}
            ground.GetComponent<TerrainAppearance>()?.Configure(river&&(roadMask&4)!=0,PathNetwork.BridgeOffset(key));
            Physics.SyncTransforms();
            path.Configure(game.Run.seed,key,roadMask,biome,adjacentBiomes,river,RiverAt(key+Vector2Int.left)&&BiomeAt(key+Vector2Int.left)==biome,RiverAt(key+Vector2Int.right)&&BiomeAt(key+Vector2Int.right)==biome,landmarkType>=0||key==Vector2Int.zero,landmarkType==3);
            bool architectureHere=key==Vector2Int.zero||SeedHash(game.Run.seed,key.x,key.y)%7==0;
            Vector3 architectureOffset=Vector3.zero;bool architecturePlaced=false;
            if(architectureHere)for(int attempt=0;attempt<24;attempt++)
            {
                var candidate=new Vector3((random.Next(2)==0?-1:1)*(8+(float)random.NextDouble()*4),0,(random.Next(2)==0?-1:1)*(8+(float)random.NextDouble()*4));
                if(path.NearPath(candidate,4)||path.ContainsWater(candidate,4)||ground.GetComponent<TerrainAppearance>().ContainsWater(chunk.root.transform.position+candidate,4)||!LevelFoundation(chunk.root.transform.position+candidate))continue;
                architectureOffset=candidate;architecturePlaced=true;break;
            }
            architectureHere&=architecturePlaced;
            if(architectureHere)
            {
                string architecture="architecture/"+new[]{"forest","desert","snow","volcano"}[(int)biome];
                Vector3 approach=path.NearestPoint(architectureOffset)-architectureOffset;approach.y=0;
                Quaternion facing=approach.sqrMagnitude>.01f?Quaternion.LookRotation(approach):Quaternion.identity;
                var buildingObject=region.pool.Rent(architecture,GroundPosition(chunk.root.transform.position+architectureOffset),facing);
                if(buildingObject!=null){chunk.architecture=buildingObject;buildingObject.transform.SetParent(chunk.root.transform,true);chunk.instances.Add(buildingObject);path.ConnectArchitecture(architectureOffset+facing*Vector3.forward*2.6f,biome);}
            }
            int decorationCount=biome==BiomeId.Forest?(terrainVariant==2?60:terrainVariant==1?24:30):20;
            Vector2 cluster=new Vector2((float)random.NextDouble()*16-8,(float)random.NextDouble()*16-8);
            for (int i = 0; i < decorationCount; i++)
            {
                Vector3 offset = new Vector3((float)random.NextDouble() * 30 - 15, 0, (float)random.NextDouble() * 30 - 15);
                if(i%3!=0){float a=(float)random.NextDouble()*Mathf.PI*2;float radius=2+(float)random.NextDouble()*5;offset=new Vector3(Mathf.Clamp(cluster.x+Mathf.Sin(a)*radius,-14,14),0,Mathf.Clamp(cluster.y+Mathf.Cos(a)*radius,-14,14));}
                if (key == Vector2Int.zero && offset.magnitude < 8 || landmarkType >= 0 && offset.magnitude < 7) continue;
                if (path.NearPath(offset,2.7f)) continue;
                if(architectureHere&&(offset-architectureOffset).sqrMagnitude<25)continue;
                if(path.ContainsWater(offset,1.5f))continue;
                if(ground.GetComponent<TerrainAppearance>().ContainsWater(chunk.root.transform.position+offset,1.5f))continue;
                var decoration = region.pool.Rent(config.decorationId, GroundPosition(chunk.root.transform.position + offset), Quaternion.Euler(0, random.Next(360), 0));
                if (decoration == null) continue;
                decoration.transform.SetParent(chunk.root.transform, true);
                decoration.transform.localScale = Vector3.one * (.65f + (float)random.NextDouble() * (terrainVariant==2?1.15f:.65f));
                int variation=random.Next();if(biome==BiomeId.Forest){int chance=random.Next(100);variation=chance<(terrainVariant==2?75:terrainVariant==1?40:55)?0:chance<90?1:2;}
                decoration.GetComponent<DecorationVariant>()?.Select(variation);
                chunk.instances.Add(decoration);
            }
            var record = journal.Load(key);
            if (landmarkType >= 0)
            {
                var site = new WorldSite { id = key.x + ":" + key.y + ":" + landmarkType, type = landmarkType, biome = biome, chunk = key, position = chunk.root.transform.position };
                site.complete = record.completed.Contains(site.id);
                site.visual = game.Pool.Rent(landmarkType == 3 ? "world/boss-site" : landmarkType == 0 ? "world/chest" : landmarkType == 1 ? "world/camp" : "world/shrine", site.position, Quaternion.identity);
                if(site.visual!=null)site.visual.GetComponent<BossSiteAppearance>()?.Configure(biome);
                if (site.visual != null) site.visual.transform.SetParent(chunk.root.transform, true);
                sites.Add(site); chunk.site = site;
                UpdateSiteAppearance(site);
            }
            chunk.constructing=false;
        }
        static int SiteType(Vector2Int key, System.Random random)
        {
            if (key == new Vector2Int(1, 0)) return 0;
            if (key == new Vector2Int(0, 1)) return 2;
            if (key == new Vector2Int(-1, 0)) return 1;
            if (key == new Vector2Int(0, 2)) return 3;
            if (Mathf.Abs(key.x) % 3 == 0 && Mathf.Abs(key.y) % 3 == 2) return 3;
            return random.NextDouble() < .28 ? random.Next(3) : -1;
        }
        void RegisterRegion(Region region,string id,GameObject prefab){if(prefab==null)LoadingError="缺少地图资源: "+id;else region.pool.Register(id,prefab);}
        int TerrainVariant(Vector2Int key,BiomeId biome)
        {
            if(biome==BiomeId.Forest||biome==BiomeId.Volcano)return RiverAt(key)?0:1+SeedHash(game.Run.seed,key.x,key.y)%2;
            int shape=SeedHash(game.Run.seed,key.x,key.y)%7;
            return shape==0?0:shape<4?1:2;
        }
        bool RiverAt(Vector2Int key)
        {
            var biome=BiomeAt(key);if(biome!=BiomeId.Forest&&biome!=BiomeId.Volcano)return false;
            int band=Mathf.FloorToInt(key.y/7f),row=key.y-band*7;
            return row==1+SeedHash(game.Run.seed^0x39ad,0,band)%5;
        }
        public int RoadConnections(Vector2Int key)
        {
            int mask=0;var directions=new[]{Vector2Int.right,Vector2Int.left,Vector2Int.up,Vector2Int.down};
            for(int edge=0;edge<4;edge++){var next=key+directions[edge];if(PathNetwork.Connected(game.Run.seed,key,next))mask|=1<<edge;}return mask;
        }
        public bool HasWatercourse(Vector2Int key)=>RiverAt(key);
        public bool TryDiagnosticArchitecture(Vector2Int key,out Vector3 position,out bool foundationClear)
        {
            position=Vector3.zero;foundationClear=false;
            if(!game.DiagnosticMode||!chunks.TryGetValue(key,out var chunk)||chunk.constructing||chunk.architecture==null)return false;
            position=chunk.architecture.transform.position;
            foundationClear=!chunk.path.ContainsWater(position-chunk.root.transform.position,4)&&!chunk.ground.GetComponent<TerrainAppearance>().ContainsWater(position,4)&&LevelFoundation(position);
            return true;
        }
        static bool LevelFoundation(Vector3 center)
        {
            float min=float.MaxValue,max=float.MinValue;
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
            {
                if(!Physics.Raycast(center+new Vector3(x*2.2f,8,z*2.2f),Vector3.down,out var hit,20,1<<0)||hit.normal.y<.94f)return false;
                min=Mathf.Min(min,hit.point.y);max=Mathf.Max(max,hit.point.y);
            }
            return max-min<.32f;
        }
        static Vector3 GroundPosition(Vector3 p){if(Physics.Raycast(p+Vector3.up*8,Vector3.down,out var hit,20,1<<0))p.y=hit.point.y;return p;}
        [System.Serializable] sealed class ChunkEvidence {public Vector2Int key;public string biome,terrain;public Vector3 position,min,max;public bool active;public int vertices;public Vector4 blend;}
        [System.Serializable] sealed class WorldEvidence {public Vector2 global;public Vector2Int current,origin;public Vector3 camera;public float zoom;public List<ChunkEvidence> chunks=new List<ChunkEvidence>();}
        public string DiagnosticSnapshot()
        {
            if(!game.DiagnosticMode)return "{}";
            var data=new WorldEvidence{global=GlobalPosition,current=CurrentChunk,origin=OriginChunk,camera=Camera.main.transform.position,zoom=Camera.main.orthographicSize};
            foreach(var pair in chunks)
            {
                var chunk=pair.Value;var evidence=new ChunkEvidence{key=pair.Key,biome=BiomeAt(pair.Key).ToString(),position=chunk.root.transform.position};
                var instance=chunk.ground;
                if(instance!=null)
                {
                    var renderer=instance.GetComponentInChildren<MeshRenderer>();evidence.terrain=instance.name;evidence.active=instance.activeInHierarchy&&renderer!=null&&renderer.enabled;
                    if(renderer!=null){evidence.min=renderer.bounds.min;evidence.max=renderer.bounds.max;var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);evidence.blend=block.GetVector("_BlendEdges");evidence.vertices=renderer.GetComponent<MeshFilter>().sharedMesh.vertexCount;}
                }
                data.chunks.Add(evidence);
            }
            return JsonUtility.ToJson(data,true);
        }
        void Schedule(Vector2Int center)
        {
            for (int ring = 0; ring <= 2; ring++) for (int x = -ring; x <= ring; x++) for (int z = -ring; z <= ring; z++)
            {
                if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) != ring) continue;
                var key = center + new Vector2Int(x, z);
                if (!chunks.ContainsKey(key) && queued.Add(key)) queue.Enqueue(key);
            }
        }
        void Update()
        {
            if (game == null || !game.Ready || game.IsPaused || disposed) return;
            if(!string.IsNullOrEmpty(LoadingError)){game.AbortToMenu(LoadingError);return;}
            Vector2 global = GlobalPosition;
            var key = new Vector2Int(Mathf.FloorToInt((global.x + ChunkSize * .5f) / ChunkSize), Mathf.FloorToInt((global.y + ChunkSize * .5f) / ChunkSize));
            if (key != CurrentChunk) { CurrentChunk = key; journal.Visit(key);queue.Clear();queued.Clear();Schedule(key); RemoveFar(key); }
            var biome = BiomeAt(key);
            if (biome != CurrentBiome) { CurrentBiome = biome; game.Audio.SetBiome(biome); game.UI.Toast("踏入「" + game.Catalog.Biome(biome).name + "」", 3); }
            if (!building && queue.Count > 0) StartCoroutine(BuildNext());
            if (game.Player.transform.position.sqrMagnitude > 500 * 500) ShiftOrigin();
            UpdateInteraction();
            if(CurrentBiome==BiomeId.Volcano&&TerrainVariant(CurrentChunk,CurrentBiome)==0&&Time.time>nextRegionalHazard)
            {
                Vector3 local=game.Player.transform.position-LocalPosition(CurrentChunk);
                if(chunks.TryGetValue(CurrentChunk,out var current)&&current.path!=null&&current.path.ContainsWater(local))
                {
                    bool onBridge=Mathf.Abs(local.x)<2&&current.ground.GetComponent<TerrainAppearance>()?.HasBridge==true;
                    if(!onBridge){nextRegionalHazard=Time.time+1.3f;game.Combat.Hazard(game.Player.transform.position,1,.65f,.25f,12,false);}
                }
            }
        }
        IEnumerator BuildNext()
        {
            building = true; var key = queue.Dequeue(); queued.Remove(key);
            if (Mathf.Max(Mathf.Abs(key.x - CurrentChunk.x), Mathf.Abs(key.y - CurrentChunk.y)) <= 2) yield return BuildChunk(key);
            if(!disposed)RemoveFar(CurrentChunk);
            building = false;
        }
        void RemoveFar(Vector2Int center)
        {
            remove.Clear();
            foreach (var pair in chunks)
            {
                if(pair.Value.constructing)continue;
                if (Mathf.Max(Mathf.Abs(pair.Key.x - center.x), Mathf.Abs(pair.Key.y - center.y)) <= 3) continue;
                if (pair.Value.site != null && game.Combat.Boss != null && pair.Value.site.id == game.Combat.BossSiteId) continue;
                remove.Add(pair.Key);
            }
            foreach (var key in remove)
            {
                var chunk = chunks[key];
                foreach (var instance in chunk.instances) chunk.region.pool.Return(instance);
                if (chunk.site != null) { sites.Remove(chunk.site); game.Pool.Return(chunk.site.visual); }
                Destroy(chunk.root); journal.Unload(key); chunks.Remove(key); chunk.region.count--;
            }
            var unused = new List<BiomeId>();
            // A rapid boundary crossing may happen while the first AA load is yielding.
            // Keep that region's pool and scope alive until BuildChunk owns its first chunk.
            foreach (var pair in regions) if (pair.Value.count == 0 && !pair.Value.loading) unused.Add(pair.Key);
            foreach (var biome in unused)
            {
                var region = regions[biome]; region.pool.Clear(); regions.Remove(biome);
                game.StartCoroutine(ReleaseLater(game.Assets, region.scope));
            }
        }
        static IEnumerator ReleaseLater(AssetService assets, AssetScope scope) { yield return null; yield return null; assets.Release(scope); }
        void UpdateInteraction()
        {
            WorldSite nearest = null; InteractionText = null; float best = 4.8f;
            foreach (var site in sites)
            {
                float distance = Vector3.Distance(game.Player.transform.position, site.position);
                if (site.started && !site.complete && site.kills >= 6)
                { game.Progression.Reward(7, 12); CompleteEvent(site.id); game.UI.Toast("营地已肃清 · 定向点 +7，晶石 +12"); }
                if (site.complete || distance >= best) continue;
                nearest = site; best = distance;
            }
            if (nearest == null) return;
            if (nearest.type == 3) InteractionText = game.Combat.Boss == null ? "E  开启挑战 · " + game.Catalog.Biome(nearest.biome).bossName : "Boss 挑战中 · 离开区域可脱战";
            else if (nearest.type == 0) InteractionText = "E  打开补给箱 · 定向点与晶石";
            else if (nearest.type == 1) InteractionText = nearest.started ? "营地肃清 " + nearest.kills + "/6" : "E  挑战营地 · 击败6个守卫";
            else InteractionText = "E  激活祭坛 · 恢复生命并获得成长";
            if (game.Inputs.Interact) Interact(nearest);
        }
        void Interact(WorldSite site)
        {
            if (site.complete) return;
            if (site.type == 3) { game.Combat.ActivateBoss(site.id, site.position, site.biome, game.Catalog.Biome(site.biome).bossId); return; }
            if (site.type == 1)
            {
                if (site.started) return; site.started = true;
                for (int i = 0; i < 6; i++) game.Combat.Spawn(i % 4, site.position + Quaternion.Euler(0, i * 60, 0) * Vector3.forward * 7, i % 3 == 0);
                game.UI.Toast("营地挑战 · 在附近击败6个守卫。"); return;
            }
            if (site.type == 2) { game.Player.Heal(game.Player.MaxHealth * .45f); game.Progression.Reward(3, 4); game.Progression.Experience(18); }
            else { game.Progression.Reward(5, 8); game.Progression.Experience(12); }
            CompleteEvent(site.id);
            game.Audio.Play(SoundCue.Unlock); game.Combat.Effect("vfx/rune", site.position, .8f, 4);
            game.UI.Toast(site.type == 2 ? "祭坛祝福 · 生命恢复，定向点 +3" : "发现补给 · 定向点 +5，晶石 +8");
        }
        public void NotifyKill(Vector3 position)
        {
            foreach (var site in sites) if (site.type == 1 && site.started && !site.complete && Vector3.Distance(position, site.position) < 20) site.kills++;
        }
        public void CompleteEvent(string id)
        {
            foreach (var site in sites)
            {
                if (site.id != id || site.complete) continue;
                site.complete = true; journal.Complete(site.chunk, id);
                UpdateSiteAppearance(site);
                game.Save.Profile.events++; game.Save.MarkDirty(); game.Save.Flush(); return;
            }
        }
        static void UpdateSiteAppearance(WorldSite site)
        {
            if(site.visual==null)return;
            if(site.type==3)site.visual.GetComponent<BossSiteAppearance>()?.SetCompleted(site.complete);
            else if(site.complete)site.visual.transform.localScale=Vector3.one*.65f;
        }
        void ShiftOrigin()
        {
            var delta = new Vector2Int(Mathf.RoundToInt(game.Player.transform.position.x / ChunkSize), Mathf.RoundToInt(game.Player.transform.position.z / ChunkSize));
            Vector3 shift = new Vector3(delta.x * ChunkSize, 0, delta.y * ChunkSize); OriginChunk += delta;
            game.Player.ShiftOrigin(shift); CameraRig.Active?.ShiftOrigin(shift); game.Combat.ShiftOrigin(shift);
            foreach (var chunk in chunks.Values) chunk.root.transform.position -= shift;
            foreach (var site in sites) site.position -= shift;
        }
        public void Dispose()
        {
            disposed = true; StopAllCoroutines();
            foreach (var region in regions.Values) { region.pool.Clear(); game.StartCoroutine(ReleaseLater(game.Assets, region.scope)); }
            regions.Clear(); chunks.Clear(); sites.Clear(); queued.Clear(); queue.Clear(); journal?.Dispose();
        }
    }
}
