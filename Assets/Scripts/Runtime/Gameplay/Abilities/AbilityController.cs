using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class AbilityController : MonoBehaviour
    {
        GameManager game;
        readonly Dictionary<AbilityKind, float> cooldowns = new Dictionary<AbilityKind, float>();
        readonly Dictionary<AbilityKind, GameObject> visuals = new Dictionary<AbilityKind, GameObject>();
        readonly List<GameObject> orbitSwords = new List<GameObject>();
        readonly List<EnemyActor> targets = new List<EnemyActor>(60);
        readonly List<EnemyActor> rainTargets = new List<EnemyActor>(32);
        float nextOrbit, nextRain, nextFrost, nextLightning;
        public void Initialize(GameManager manager) { game = manager; Refresh(); }
        public void Refresh()
        {
            foreach (var kind in game.Run.abilities)
            {
                if (!cooldowns.ContainsKey(kind)) cooldowns[kind] = Time.time + .2f;
                if (visuals.ContainsKey(kind)) continue;
                var definition = game.Catalog.Ability(kind);
                var go = game.Pool.Rent(definition.prefabId, transform.position, Quaternion.identity);
                if (go != null) { go.transform.SetParent(transform, false); visuals[kind] = go; }
            }
            if (game.Run.Effect(game.Catalog, UpgradeEffect.Orbit, AbilityKind.Sword) > 0)
            {
                int visible = Mathf.Min(10, 3 + Mathf.FloorToInt(game.Run.Effect(game.Catalog, UpgradeEffect.Count, AbilityKind.Sword)));
                while (orbitSwords.Count < visible)
                {
                    var sword = game.Pool.Rent("weapon/sword", transform.position, Quaternion.identity);
                    if (sword == null) break;
                    sword.transform.SetParent(transform, false); orbitSwords.Add(sword);
                }
            }
        }
        float Effect(UpgradeEffect effect, AbilityKind kind) => game.Run.Effect(game.Catalog, effect, kind);
        public DamagePacket Packet(AbilityKind kind, float multiplier = 1)
        {
            var ability = game.Catalog.Ability(kind);
            float damage = ability.damage * (1 + game.Save.Profile.damageLevel * .06f + game.Run.character.damageBonus + Effect(UpgradeEffect.Damage, kind));
            float critChance = Effect(UpgradeEffect.Crit, kind);
            bool crit = Random.value < critChance;
            if (crit) damage *= 1.7f + Mathf.Max(0, critChance - 1);
            return new DamagePacket
            {
                damage = damage * multiplier, source = kind, critical = crit,
                burn = Effect(UpgradeEffect.Burn, kind) * damage + (kind == AbilityKind.Sword ? Effect(UpgradeEffect.FireSword, kind) * damage : 0),
                frost = Effect(UpgradeEffect.Frost, kind) + (kind == AbilityKind.FrostStaff ? .35f : 0),
                lightning = Mathf.FloorToInt(Effect(UpgradeEffect.Chain, kind)) + (kind == AbilityKind.Gun && Effect(UpgradeEffect.ElectricAmmo, kind) > 0 ? 2 : 0),
                shatter = Effect(UpgradeEffect.IceShatter, kind) > 0
            };
        }
        void Update()
        {
            if (game == null || !game.Ready || game.IsPaused) return;
            int visualIndex = 0;
            foreach (var kind in game.Run.abilities)
            {
                var definition = game.Catalog.Ability(kind);
                if (visuals.TryGetValue(kind, out var visual))
                {
                    float a = Time.time * .7f + visualIndex * 1.1f;
                    visual.transform.position = transform.position + new Vector3(Mathf.Sin(a) * 1.2f, 1.25f + Mathf.Sin(a * 2) * .1f, Mathf.Cos(a) * 1.2f);
                    visual.transform.rotation = Quaternion.Euler(0, Time.time * 30 + visualIndex * 60, kind == AbilityKind.Sword ? -20 : 0);
                }
                visualIndex++;
                if (Time.time < cooldowns[kind]) continue;
                float range = definition.range + Effect(UpgradeEffect.Range, kind);
                var target = game.Combat.Target(transform.position, range);
                if (target == null && kind != AbilityKind.Rune) { cooldowns[kind] = Time.time + .12f; continue; }
                float speed = 1 + Effect(UpgradeEffect.AttackSpeed, kind) + (kind == AbilityKind.Spirit ? Effect(UpgradeEffect.SummonSpeed, kind) : 0);
                cooldowns[kind] = Time.time + definition.cooldown / Mathf.Max(.1f, speed);
                if (kind == AbilityKind.Rune) Pulse(kind, 4.2f + Effect(UpgradeEffect.Range, kind), "vfx/rune");
                else Shoot(kind, target, 1);
            }
            AnimateOrbit();
            if (game.Run.abilities.Contains(AbilityKind.Sword) && Effect(UpgradeEffect.SwordRain, AbilityKind.Sword) > 0 && Time.time >= nextRain)
            {
                nextRain = Time.time + 5 / (1 + Effect(UpgradeEffect.AttackSpeed, AbilityKind.Sword));
                game.Combat.Query(transform.position, 18, rainTargets);
                int count = Mathf.Min(rainTargets.Count, 12);
                for (int i = 0; i < count; i++) if (rainTargets[i].IsAlive) Shoot(AbilityKind.Sword, rainTargets[i], .65f);
                game.Audio.Play(SoundCue.Sword);
            }
            if (game.Run.abilities.Contains(AbilityKind.Fireball) && Effect(UpgradeEffect.Frost, AbilityKind.Fireball) > 0 && Time.time > nextFrost)
            { nextFrost = Time.time + 5; Pulse(AbilityKind.Fireball, 5.4f, "vfx/frost", true); }
            if (game.Run.abilities.Contains(AbilityKind.Fireball) && Effect(UpgradeEffect.Lightning, AbilityKind.Fireball) > 0 && Time.time > nextLightning)
            {
                nextLightning = Time.time + 3.5f;
                var target = game.Combat.Target(transform.position, 18);
                if (target != null) { target.Hit(Packet(AbilityKind.Fireball, 1.5f)); game.Combat.Effect("vfx/lightning", target.transform.position, .3f, 2); game.Audio.Play(SoundCue.Magic); }
            }
        }
        void Shoot(AbilityKind kind, EnemyActor target, float strength)
        {
            if (target == null || !target.IsAlive) return;
            var definition = game.Catalog.Ability(kind);
            int logicalCount = 1 + Mathf.FloorToInt(Effect(UpgradeEffect.Count, kind) + Effect(UpgradeEffect.Spread, kind) + Effect(UpgradeEffect.Burst, kind));
            int drawnCount = Mathf.Min(16, logicalCount);
            Vector3 origin = transform.position + Vector3.up * 1.05f;
            Vector3 direction = (target.transform.position + Vector3.up * .8f - origin).normalized;
            game.Player.FaceAttack(direction);
            GetComponent<CharacterAnimator>()?.Attack(kind,direction);
            int penetration = Mathf.FloorToInt(Effect(UpgradeEffect.Pierce, kind)) + (kind == AbilityKind.Sword || kind == AbilityKind.Crossbow ? 1 : 0);
            int bounce = Mathf.FloorToInt(Effect(UpgradeEffect.Bounce, kind));
            float splash = (kind == AbilityKind.Fireball ? 1.8f : kind == AbilityKind.Cannon ? 3.3f : kind == AbilityKind.FrostStaff ? 2.4f : 0) + Effect(UpgradeEffect.Explosion, kind);
            for (int i = 0; i < drawnCount; i++)
            {
                Vector3 shotDirection = Quaternion.Euler(0, (i - (drawnCount - 1) * .5f) * 7, 0) * direction;
                var go = game.Pool.Rent(definition.projectileId, origin, Quaternion.LookRotation(shotDirection));
                if (go == null) continue;
                var packet = Packet(kind, strength * logicalCount / drawnCount);
                if (kind == AbilityKind.Boomerang) go.GetComponent<ProjectileActor>().InitializeReturn(game, shotDirection, definition.speed, packet, definition.range + Effect(UpgradeEffect.Range, kind));
                else go.GetComponent<ProjectileActor>().Initialize(game, shotDirection, definition.speed, packet, penetration, bounce, splash);
            }
            if (kind == AbilityKind.Sword && Effect(UpgradeEffect.SwordWave, kind) > 0)
            {
                game.Combat.Query(target.transform.position, 3, targets);
                var packet = Packet(kind, .35f);
                foreach (var enemy in targets) if (enemy.IsAlive) enemy.Hit(packet);
                game.Combat.Effect("vfx/slash", target.transform.position, .35f, 2.5f,rotation:Quaternion.LookRotation(direction));
            }
            game.Audio.Play(kind == AbilityKind.Gun ? SoundCue.Gun : kind == AbilityKind.Sword ? SoundCue.Sword : SoundCue.Magic);
        }
        void Pulse(AbilityKind kind, float radius, string effectId, bool freeze = false)
        {
            game.Combat.Query(transform.position, radius, targets);
            var packet = Packet(kind, freeze ? .5f : 1 + Effect(UpgradeEffect.RunePulse, kind));
            if (freeze) packet.frost = .85f;
            foreach (var enemy in targets) if (enemy.IsAlive) enemy.Hit(packet);
            game.Combat.Effect(effectId, transform.position + Vector3.up * .1f, .7f, radius);
        }
        void AnimateOrbit()
        {
            int n = orbitSwords.Count;
            if (n == 0) return;
            bool hit = Time.time >= nextOrbit;
            if (hit) nextOrbit = Time.time + .3f;
            int logicalCount = 3 + Mathf.FloorToInt(Effect(UpgradeEffect.Count, AbilityKind.Sword));
            for (int i = 0; i < n; i++)
            {
                float a = Time.time * 3 + i * Mathf.PI * 2 / n;
                var sword = orbitSwords[i];
                sword.transform.position = transform.position + new Vector3(Mathf.Sin(a) * 2.6f, .85f, Mathf.Cos(a) * 2.6f);
                sword.transform.rotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 75);
                if (!hit) continue;
                game.Combat.Query(sword.transform.position, 1.3f, targets);
                var packet = Packet(AbilityKind.Sword, .35f * logicalCount / n);
                foreach (var enemy in targets) if (enemy.IsAlive) enemy.Hit(packet);
            }
        }
        public void Dispose()
        {
            if (game?.Pool != null)
            {
                foreach (var visual in visuals.Values) if (visual != null) game.Pool.Return(visual);
                foreach (var sword in orbitSwords) if (sword != null) game.Pool.Return(sword);
            }
            visuals.Clear(); orbitSwords.Clear(); cooldowns.Clear();
        }
    }
}
