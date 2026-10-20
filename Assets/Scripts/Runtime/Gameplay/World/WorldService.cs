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
        }
        sealed class Chunk
        {
            public GameObject root,ground;
            public Region region;
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
            float wiggle = Mathf.PerlinNoise((key.x + game.Run.seed % 997) * .13f, (key.y + game.Run.seed % 563) * .13f) * 2;
            int band = Mathf.FloorToInt((key.x + key.y * .4f + wiggle) / 3f);
            return (BiomeId)((band % 4 + 4) % 4);
        }
        public bool Visited(Vector2Int key) => journal != null && journal.Visited(key);
        public Vector3 LocalPosition(Vector2Int chunk) => new Vector3((chunk.x - OriginChunk.x) * ChunkSize, 0, (chunk.y - OriginChunk.y) * ChunkSize);
        IEnumerator BuildChunk(Vector2Int key)
        {
            if (disposed || chunks.ContainsKey(key)) yield break;
            BiomeId biome = BiomeAt(key);
            if (!regions.TryGetValue(biome, out Region region))
            {
                region = new Region { scope = new AssetScope("biome-" + biome), pool = new PoolService(transform) };
                regions[biome] = region;
                var data = game.Catalog.Biome(biome);
                yield return game.Assets.LoadPrefab(data.terrainId, region.scope, p => RegisterRegion(region,data.terrainId,p));
                foreach(string suffix in new[]{"-wild","-ridge"}){string terrain=data.terrainId+suffix;yield return game.Assets.LoadPrefab(terrain,region.scope,p=>RegisterRegion(region,terrain,p));}
                yield return game.Assets.LoadPrefab(data.decorationId, region.scope, p => RegisterRegion(region,data.decorationId, p));
                string architecture="architecture/"+new[]{"forest","desert","snow","volcano"}[(int)biome];
                yield return game.Assets.LoadPrefab(architecture, region.scope, p => RegisterRegion(region,architecture, p));
                yield return game.Assets.LoadPrefab(data.bossId, game.RunAssets, p => {if(p==null)LoadingError="缺少挑战资源";game.Pool.Register(data.bossId,p);});
            }
            if (disposed||!string.IsNullOrEmpty(LoadingError)) yield break;
            var chunk = new Chunk { region = region, root = new GameObject("Chunk " + key) };
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
                var block=new MaterialPropertyBlock();block.SetFloat("_Terrain",1);
                var directions=new[]{Vector2Int.right,Vector2Int.left,Vector2Int.up,Vector2Int.down};var names=new[]{"_EdgeEast","_EdgeWest","_EdgeNorth","_EdgeSouth"};
                Vector4 blend=Vector4.zero;
                for(int edge=0;edge<4;edge++) {var adjacent=BiomeAt(key+directions[edge]);blend[edge]=adjacent!=biome?1:0;Color c=Color.Lerp(config.ground,game.Catalog.Biome(adjacent).ground,.5f).linear;block.SetVector(names[edge],new Vector4(c.r,c.g,c.b,1));}
                block.SetVector("_BlendEdges",blend);
                block.SetVector("_WorldOrigin",new Vector4(key.x*ChunkSize,key.y*ChunkSize,0,0));
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
                        float along=-11+i*4.4f;float inward=1.4f+(i%3)*1.3f;
                        Vector3 offset=edge<2?new Vector3((edge==0?1:-1)*(16-inward),0,along):new Vector3(along,0,(edge==2?1:-1)*(16-inward));
                        var transition=region.pool.Rent(decorationId,GroundPosition(chunk.root.transform.position+offset),Quaternion.Euler(0,i*137+edge*43,0));
                        if(transition==null)continue;
                        transition.transform.SetParent(chunk.root.transform,true);transition.transform.localScale=Vector3.one*(.55f+i%3*.12f);
                        transition.GetComponent<DecorationVariant>()?.Select(i%3==0?0:1);chunk.instances.Add(transition);
                    }
                }
            }
            int landmarkType = SiteType(key, random);
            bool architectureHere=key==Vector2Int.zero||SeedHash(game.Run.seed,key.x,key.y)%7==0;
            Vector3 architectureOffset=new Vector3(random.Next(2)==0?-9:9,0,random.Next(2)==0?-8:8);
            if(architectureHere)
            {
                string architecture="architecture/"+new[]{"forest","desert","snow","volcano"}[(int)biome];
                var buildingObject=region.pool.Rent(architecture,GroundPosition(chunk.root.transform.position+architectureOffset),Quaternion.Euler(0,0,0));
                if(buildingObject!=null){buildingObject.transform.SetParent(chunk.root.transform,true);chunk.instances.Add(buildingObject);}
            }
            int decorationCount=biome==BiomeId.Forest?(terrainVariant==2?42:terrainVariant==1?16:26):20;
            Vector2 cluster=new Vector2((float)random.NextDouble()*16-8,(float)random.NextDouble()*16-8);
            for (int i = 0; i < decorationCount; i++)
            {
                Vector3 offset = new Vector3((float)random.NextDouble() * 30 - 15, 0, (float)random.NextDouble() * 30 - 15);
                if(i%3!=0){float a=(float)random.NextDouble()*Mathf.PI*2;float radius=2+(float)random.NextDouble()*5;offset=new Vector3(Mathf.Clamp(cluster.x+Mathf.Sin(a)*radius,-14,14),0,Mathf.Clamp(cluster.y+Mathf.Cos(a)*radius,-14,14));}
                if (key == Vector2Int.zero && offset.magnitude < 8 || landmarkType >= 0 && offset.magnitude < 7) continue;
                if (Mathf.Abs(offset.x) < 2 || Mathf.Abs(offset.z) < 2) continue;
                if(architectureHere&&(offset-architectureOffset).sqrMagnitude<25)continue;
                if(terrainVariant==0&&(biome==BiomeId.Forest||biome==BiomeId.Volcano)&&Mathf.Abs(offset.z-(8+Mathf.Sin(offset.x*Mathf.PI/16)*1.1f))<2)continue;
                var decoration = region.pool.Rent(config.decorationId, GroundPosition(chunk.root.transform.position + offset), Quaternion.Euler(0, random.Next(360), 0));
                if (decoration == null) continue;
                decoration.transform.SetParent(chunk.root.transform, true);
                decoration.transform.localScale = Vector3.one * (.65f + (float)random.NextDouble() * (terrainVariant==2?1.15f:.65f));
                decoration.GetComponent<DecorationVariant>()?.Select(random.Next());
                chunk.instances.Add(decoration);
            }
            var record = journal.Load(key);
            if (landmarkType >= 0)
            {
                var site = new WorldSite { id = key.x + ":" + key.y + ":" + landmarkType, type = landmarkType, biome = biome, chunk = key, position = chunk.root.transform.position };
                site.complete = record.completed.Contains(site.id);
                site.visual = game.Pool.Rent(landmarkType == 3 ? "world/boss-site" : landmarkType == 0 ? "world/chest" : landmarkType == 1 ? "world/camp" : "world/shrine", site.position, Quaternion.identity);
                if (site.visual != null) site.visual.transform.SetParent(chunk.root.transform, true);
                sites.Add(site); chunk.site = site;
                if (site.complete && site.visual != null) site.visual.transform.localScale = Vector3.one * .65f;
            }
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
            if(biome==BiomeId.Forest||biome==BiomeId.Volcano)return (key.y%3+3)%3==1?0:1+SeedHash(game.Run.seed,key.x,key.y)%2;
            return SeedHash(game.Run.seed,key.x,key.y)%3;
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
            if (key != CurrentChunk) { CurrentChunk = key; journal.Visit(key);Schedule(key); RemoveFar(key); }
            var biome = BiomeAt(key);
            if (biome != CurrentBiome) { CurrentBiome = biome; game.Audio.SetBiome(biome); game.UI.Toast("踏入「" + game.Catalog.Biome(biome).name + "」", 3); }
            if (!building && queue.Count > 0) StartCoroutine(BuildNext());
            if (game.Player.transform.position.sqrMagnitude > 500 * 500) ShiftOrigin();
            UpdateInteraction();
            if(CurrentBiome==BiomeId.Volcano&&TerrainVariant(CurrentChunk,CurrentBiome)==0&&Time.time>nextRegionalHazard)
            {
                Vector3 local=game.Player.transform.position-LocalPosition(CurrentChunk);
                if(Mathf.Abs(local.x)>2&&Mathf.Abs(local.z-(8+Mathf.Sin(local.x*Mathf.PI/16)*1.1f))<1.1f)
                {nextRegionalHazard=Time.time+1.3f;game.Combat.Hazard(game.Player.transform.position,1,.65f,.25f,12,false);}
            }
        }
        IEnumerator BuildNext()
        {
            building = true; var key = queue.Dequeue(); queued.Remove(key);
            if (Mathf.Max(Mathf.Abs(key.x - CurrentChunk.x), Mathf.Abs(key.y - CurrentChunk.y)) <= 2) yield return BuildChunk(key);
            building = false;
        }
        void RemoveFar(Vector2Int center)
        {
            remove.Clear();
            foreach (var pair in chunks)
            {
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
            foreach (var pair in regions) if (pair.Value.count == 0) unused.Add(pair.Key);
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
                if (site.visual != null) site.visual.transform.localScale = Vector3.one * .65f;
                game.Save.Profile.events++; game.Save.MarkDirty(); game.Save.Flush(); return;
            }
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
