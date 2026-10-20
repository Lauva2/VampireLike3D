using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public struct DamagePacket
    {
        public float damage, burn, frost;
        public int lightning;
        public AbilityKind source;
        public bool critical, shatter, burning;
    }

    public sealed class CombatWorld : MonoBehaviour
    {
        public readonly List<EnemyActor> Enemies = new List<EnemyActor>();
        public EnemyActor Marked { get; private set; }
        public EnemyActor Boss { get; private set; }
        public string BossSiteId { get; private set; }
        public float RetreatProgress { get; private set; }
        public Vector3 BossCenter { get; private set; }
        GameManager game;
        float nextSpawn, nextGrid;
        readonly Dictionary<Vector2Int, List<EnemyActor>> grid = new Dictionary<Vector2Int, List<EnemyActor>>();
        readonly Stack<List<EnemyActor>> gridLists = new Stack<List<EnemyActor>>();
        readonly List<EnemyActor> hits = new List<EnemyActor>(64);
        readonly List<EnemyActor> spreadHits = new List<EnemyActor>(24);
        GameObject markVisual;

        public void Initialize(GameManager manager) { game = manager; }
        void Update()
        {
            if (!game.Ready || game.IsPaused) return;
            if (Time.time >= nextGrid) { RebuildGrid(); nextGrid = Time.time + .08f; }
            if (Time.time >= nextSpawn)
            {
                nextSpawn = Time.time + Mathf.Max(.12f, .9f - game.Run.seconds / 2000);
                int density = Mathf.Min(420, 22 + (int)(game.Run.seconds * .2f));
                if (Enemies.Count < density)
                {
                    int count = 1 + (int)(game.Run.seconds / 240);
                    for (int i = 0; i < count; i++)
                    {
                        float a = Random.value * Mathf.PI * 2;
                        Vector3 pos = game.Player.transform.position + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * Random.Range(20, 27);
                        Spawn(Random.Range(0, game.Run.seconds < 50 ? 2 : 4), pos, Random.value < .025f + game.Run.seconds / 20000);
                    }
                }
            }
            if (game.Inputs.Mark)
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                EnemyActor selected = null;
                if (Physics.Raycast(ray, out var hit, 160, 1 << 8)) selected = hit.collider.GetComponentInParent<EnemyActor>();
                SetMark(selected != null && selected.IsAlive ? selected : null);
            }
            if (Marked != null && !Marked.IsAlive) SetMark(null);
            if (Marked != null)
            {
                if (markVisual == null) markVisual = game.Pool.Rent("vfx/mark", Marked.transform.position, Quaternion.identity);
                if (markVisual != null) markVisual.transform.position = Marked.transform.position + Vector3.up * .07f;
            }
            if (Boss != null)
            {
                float d = Vector3.Distance(game.Player.transform.position, BossCenter);
                RetreatProgress = d > 25 ? RetreatProgress + Time.deltaTime : 0;
                if (RetreatProgress > 2)
                {
                    var old = Boss; Boss = null; Remove(old); game.Pool.Return(old.gameObject);
                    ClearBossHazards(); RetreatProgress = 0;
                    game.UI.Toast("已脱离挑战 · Boss 已重置，准备好后可再次开启。", 5);
                }
            }
        }
        public EnemyActor Spawn(int type, Vector3 pos, bool elite = false)
        {
            if (Physics.CheckSphere(pos + Vector3.up, .6f, 1 << 10)) return null;
            GameObject go = game.Pool.Rent("enemy/" + type, pos, Quaternion.identity);
            if (go == null) return null;
            var enemy = go.GetComponent<EnemyActor>();
            enemy.Initialize(game, type, elite, game.World.CurrentBiome);
            Enemies.Add(enemy); return enemy;
        }
        public void ActivateBoss(string siteId, Vector3 position, BiomeId biome, string prefabId)
        {
            if (Boss != null) { game.UI.Toast("当前已有 Boss 挑战，先完成或离开战场。 "); return; }
            var go = game.Pool.Rent(prefabId, position, Quaternion.identity);
            if (go == null) { game.UI.Toast("Boss 资源尚在准备，请稍后重试。"); return; }
            Boss = go.GetComponent<EnemyActor>(); Boss.Initialize(game, 4, true, biome);
            BossCenter = position; BossSiteId = siteId; RetreatProgress = 0;
            Enemies.Add(Boss); SetMark(Boss);
            game.UI.Toast("挑战开启 · " + game.Catalog.Biome(biome).bossName + "\n离开区域可脱战，Boss 将恢复满血。", 3);
            game.Audio.Play(SoundCue.Boss);
        }
        void SetMark(EnemyActor target)
        {
            Marked = target;
            if (markVisual != null) { game.Pool.Return(markVisual); markVisual = null; }
        }
        void RebuildGrid()
        {
            foreach (var list in grid.Values) { list.Clear(); gridLists.Push(list); }
            grid.Clear();
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                var enemy = Enemies[i];
                if (enemy == null || !enemy.IsAlive) { Enemies.RemoveAt(i); continue; }
                Vector2Int key = Cell(enemy.transform.position);
                if (!grid.TryGetValue(key, out var list)) grid[key] = list = gridLists.Count > 0 ? gridLists.Pop() : new List<EnemyActor>(12);
                list.Add(enemy);
                if (enemy != Boss && (enemy.transform.position - game.Player.transform.position).sqrMagnitude > 2500)
                { Enemies.RemoveAt(i); game.Pool.Return(enemy.gameObject); }
            }
        }
        static Vector2Int Cell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x / 5), Mathf.FloorToInt(p.z / 5));
        public void Query(Vector3 p, float radius, List<EnemyActor> result)
        {
            result.Clear(); Vector2Int low = Cell(p - Vector3.one * radius), high = Cell(p + Vector3.one * radius);
            float square = radius * radius;
            for (int x = low.x; x <= high.x; x++) for (int z = low.y; z <= high.y; z++)
                if (grid.TryGetValue(new Vector2Int(x, z), out var list)) foreach (var enemy in list)
                    if (enemy != null && enemy.IsAlive && (enemy.transform.position - p).sqrMagnitude <= square) result.Add(enemy);
        }
        public EnemyActor Target(Vector3 from, float range, HashSet<int> exclude = null)
        {
            if (Marked != null && Marked.IsAlive && (Marked.transform.position - from).sqrMagnitude < range * range && (exclude == null || !exclude.Contains(Marked.GetInstanceID())) && Visible(from, Marked)) return Marked;
            Query(from, range, hits); EnemyActor best = null; float distance = range * range;
            foreach (var enemy in hits)
            {
                if (exclude != null && exclude.Contains(enemy.GetInstanceID())) continue;
                float d = (enemy.transform.position - from).sqrMagnitude;
                if (d >= distance || !Visible(from, enemy)) continue;
                distance = d; best = enemy;
            }
            return best;
        }
        public bool Visible(Vector3 from, EnemyActor enemy) => !Physics.Linecast(from + Vector3.up, enemy.transform.position + Vector3.up, 1 << 10);
        public void Kill(EnemyActor enemy, DamagePacket packet)
        {
            Remove(enemy); game.Run.kills++;
            game.World.NotifyKill(enemy.transform.position);
            var p = game.Save.Profile; p.totalKills++;
            if (packet.source == AbilityKind.Gun) p.gunKills++;
            if (packet.source == AbilityKind.Fireball || packet.burn > 0 || packet.burning) p.fireKills++;
            if (packet.source == AbilityKind.Sword) p.swordKills++;
            if (packet.source == AbilityKind.Gun && game.Run.Effect(game.Catalog, UpgradeEffect.Lightning, AbilityKind.Fireball) > 0) p.electricCombo++;
            if (packet.source == AbilityKind.Gun && enemy.Frozen) p.shatterCombo++;
            if (packet.source == AbilityKind.Sword && game.Run.abilities.Contains(AbilityKind.Fireball)) p.swordFireCombo++;
            if ((packet.burning || packet.burn > 0) && game.Run.Effect(game.Catalog, UpgradeEffect.FireSpread, AbilityKind.Fireball) > 0)
            {
                Query(enemy.transform.position, 4, spreadHits);
                var spread = packet; spread.damage = 0; spread.burn = Mathf.Max(2, packet.damage * .25f); spread.burning = true;
                foreach (var other in spreadHits) if (other != enemy && other.IsAlive) other.Hit(spread);
            }
            if (enemy.Elite) p.elites++;
            if (enemy == Boss)
            {
                var biome = enemy.Biome;
                p.bossWins[(int)biome] = true;
                game.Progression.Reward(15, 60); game.Progression.CheckTasks();
                game.World.CompleteEvent(BossSiteId);
                game.Save.MarkDirty(); game.Save.Flush(); Boss = null; RetreatProgress = 0; ClearBossHazards();
                game.UI.ShowBossVictory(biome); game.Audio.Play(SoundCue.Unlock);
            }
            else if (enemy.Elite) game.Progression.Reward(3, 3);
            if (game.Run.kills % 7 == 0) game.Progression.Reward(0, 1);
            game.Save.MarkDirty();
            var orb = game.Pool.Rent("pickup/experience", enemy.transform.position + Vector3.up * .4f, Quaternion.identity);
            if (orb != null) orb.GetComponent<ExperienceOrb>().Initialize(game, enemy.Type == 4 ? 80 : enemy.Elite ? 12 : 2);
            Effect("vfx/impact", enemy.transform.position + Vector3.up * .5f, .35f, enemy.Type == 4 ? 5 : 1.1f);
            enemy.PlayDeath();
        }
        void Remove(EnemyActor enemy) { Enemies.Remove(enemy); if (Marked == enemy) SetMark(null); }
        public void Effect(string id, Vector3 point, float duration, float size)
        {
            var go = game.Pool.Rent(id, point, Quaternion.identity);
            if (go != null) go.GetComponent<TransientEffect>().Initialize(game.Pool, duration, size);
        }
        public void Hazard(Vector3 p, float radius, float delay, float active, float damage, bool boss)
        {
            var go = game.Pool.Rent("vfx/warning", p + Vector3.up * .04f, Quaternion.identity);
            if (go != null) go.GetComponent<AttackHazard>().Initialize(game, radius, delay, active, damage, boss);
        }
        public void HazardLine(Vector3 start,Vector3 end,float width,float delay,float active,float damage,bool boss)
        {
            if((end-start).sqrMagnitude<.01f)return;
            var go=game.Pool.Rent("vfx/warning-line",start,Quaternion.identity);
            if(go!=null)go.GetComponent<AttackHazard>().InitializeLine(game,start,end,width,delay,active,damage,boss);
        }
        public void EnemyShot(Vector3 origin, Vector3 direction, float damage)
        {
            var go = game.Pool.Rent("projectile/enemy", origin, Quaternion.LookRotation(direction));
            if (go != null) go.GetComponent<ProjectileActor>().InitializeEnemy(game, direction, damage);
        }
        void ClearBossHazards()
        {
            if (game.Pool == null) return;
            var hazards = game.GetRunObjects<AttackHazard>();
            foreach (var hazard in hazards) if (hazard.BossOwned) game.Pool.Return(hazard.gameObject);
        }
        public void ShiftOrigin(Vector3 shift)
        {
            BossCenter -= shift;
            foreach (var enemy in Enemies) if (enemy != null) enemy.transform.position -= shift;
            var movers = game.GetRunObjects<ProjectileActor>(); foreach (var p in movers) p.transform.position -= shift;
            var orbs = game.GetRunObjects<ExperienceOrb>(); foreach (var p in orbs) p.transform.position -= shift;
            var hazards = game.GetRunObjects<AttackHazard>(); foreach (var p in hazards) p.transform.position -= shift;
            var effects = game.GetRunObjects<TransientEffect>(); foreach (var p in effects) p.transform.position -= shift;
            RebuildGrid();
        }
        public void Dispose() { SetMark(null); Enemies.Clear(); grid.Clear(); Boss = null; }
    }
}
