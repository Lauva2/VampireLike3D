using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VampireLike
{
    public enum BiomeId { Forest = 0, Desert = 1, Snow = 2, Volcano = 3, Mixed = 4, Swamp = 5, Crystal = 6 }
    public enum AbilityKind { All = -1, Gun, Fireball, Sword, Spirit, Cannon, Rune, Crossbow, Boomerang, FrostStaff }
    public enum UpgradeEffect { Damage, AttackSpeed, MoveSpeed, Health, Pickup, Count, Range, Pierce, Bounce, Burn, Frost, Lightning, Orbit, SwordRain, SwordWave, Spread, Burst, Explosion, NewAbility, Chain, FireSword, IceShatter, ElectricAmmo, Duration, Crit, Armor, Regeneration, Resource, SummonSpeed, RunePulse, FireSpread }
    public enum TaskMetric { Kills, GunKills, FireKills, SwordKills, Freezes, EliteKills, BestSeconds, Bosses, Events, ElectricCombo, ShatterCombo, SwordFireCombo, CrossbowKills, BoomerangKills, FrostStaffKills }
    public enum UnlockKind { Upgrade, Ability, Character }

    [Serializable] public sealed class PrefabEntry { public string id; public AssetReferenceGameObject reference; }
    [Serializable] public sealed class AudioEntry { public string id; public AssetReferenceT<AudioClip> reference; }
    [Serializable] public sealed class CharacterDefinition
    {
        public string id, name, subtitle, description, prefabId, taskId;
        public AbilityKind startingAbility;
        public Color color = Color.white;
        public float damageBonus, healthBonus, speedBonus;
    }
    [Serializable] public sealed class AbilityDefinition
    {
        public string id, name, description, prefabId, projectileId, taskId;
        public AbilityKind kind;
        public Color color;
        public float damage = 10, cooldown = 1, range = 14, speed = 14;
    }
    [Serializable] public sealed class UpgradeDefinition
    {
        public string id, name, description, branch, taskId;
        public AbilityKind target = AbilityKind.All;
        public UpgradeEffect effect;
        public float value;
        public int cost = 3;
        public bool unique;
        public Color color = Color.white;
    }
    [Serializable] public sealed class BlessingDefinition
    {
        public string id, name, description, icon;
        public UpgradeEffect effect;
        public float value;
        public int rerolls;
        public Color color = Color.white;
    }
    [Serializable] public sealed class TaskDefinition
    {
        public string id, name, description, rewardId;
        public TaskMetric metric;
        public int target, crystals = 15;
        public UnlockKind rewardKind;
    }
    [Serializable] public sealed class EnemyDefinition
    {
        public int type;
        public string name, description, prefabId;
        public float health = 15, speed = 2.45f, damage = 7;
    }
    [Serializable] public sealed class BiomeDefinition
    {
        public BiomeId id;
        public string name, subtitle, bossName, terrainId, decorationId, bossId;
        public Color ground, secondary, accent;
        public int[] enemyTypes = { 0, 1, 2, 3 };
    }

    [CreateAssetMenu(menuName = "VampireLike/Game Catalog")]
    public sealed class GameCatalog : ScriptableObject
    {
        public PrefabEntry[] prefabs;
        public AudioEntry[] audio;
        public CharacterDefinition[] characters;
        public AbilityDefinition[] abilities;
        public UpgradeDefinition[] upgrades;
        public TaskDefinition[] tasks;
        public BiomeDefinition[] biomes;
        public BlessingDefinition[] blessings;
        public EnemyDefinition[] enemies;
        public EnemyDefinition Enemy(int type) => enemies == null ? null : Array.Find(enemies, e => e.type == type);
        public BlessingDefinition Blessing(string id) => blessings == null ? null : Array.Find(blessings, b => b.id == id);

        [NonSerialized] UpgradeDefinition[] indexedUpgrades;
        [NonSerialized] Dictionary<string, UpgradeDefinition> upgradeIndex;
        void OnEnable() { InvalidateUpgradeIndex(); }
        void OnValidate() { InvalidateUpgradeIndex(); }
        void InvalidateUpgradeIndex() { indexedUpgrades = null; upgradeIndex = null; }

        public AssetReferenceGameObject Prefab(string id)
        {
            foreach (var p in prefabs) if (p.id == id) return p.reference;
            throw new KeyNotFoundException("Unknown prefab: " + id);
        }
        public AbilityDefinition Ability(AbilityKind kind) => Array.Find(abilities, a => a.kind == kind);
        public CharacterDefinition Character(string id) => Array.Find(characters, a => a.id == id) ?? characters[0];
        public BiomeDefinition Biome(BiomeId id)
        {
            if (id == BiomeId.Mixed) id = BiomeId.Forest;
            foreach (var biome in biomes) if (biome.id == id) return biome;
            return null;
        }
        public UpgradeDefinition Upgrade(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (upgradeIndex == null || indexedUpgrades != upgrades)
            {
                upgradeIndex = new Dictionary<string, UpgradeDefinition>(StringComparer.Ordinal);
                if (upgrades != null)
                    foreach (var definition in upgrades)
                        if (definition != null && !string.IsNullOrEmpty(definition.id) && !upgradeIndex.ContainsKey(definition.id))
                            upgradeIndex.Add(definition.id, definition);
                indexedUpgrades = upgrades;
            }
            return upgradeIndex.TryGetValue(id, out var value) ? value : null;
        }
    }

    [Serializable] public sealed class PlayerProfile
    {
        public int version = 2, crystals, totalKills, gunKills, fireKills, swordKills, freezes, elites, events;
        public int damageLevel, healthLevel, speedLevel, pickupLevel;
        public int electricCombo, shatterCombo, swordFireCombo;
        public float bestSeconds, musicVolume = .35f, soundVolume = .65f;
        public int quality = 2;
        public bool[] bossWins = new bool[6];
        public int crossbowKills, boomerangKills, frostStaffKills;
        public List<string> completedTasks = new List<string>();
        public List<string> settledRuns = new List<string>();
        public int Metric(TaskMetric metric)
        {
            switch (metric)
            {
                case TaskMetric.CrossbowKills: return crossbowKills;
                case TaskMetric.BoomerangKills: return boomerangKills;
                case TaskMetric.FrostStaffKills: return frostStaffKills;
                case TaskMetric.Kills: return totalKills;
                case TaskMetric.GunKills: return gunKills;
                case TaskMetric.FireKills: return fireKills;
                case TaskMetric.SwordKills: return swordKills;
                case TaskMetric.Freezes: return freezes;
                case TaskMetric.EliteKills: return elites;
                case TaskMetric.BestSeconds: return (int)bestSeconds;
                case TaskMetric.Events: return events;
                case TaskMetric.ElectricCombo: return electricCombo;
                case TaskMetric.ShatterCombo: return shatterCombo;
                case TaskMetric.SwordFireCombo: return swordFireCombo;
                case TaskMetric.Bosses: int n = 0; foreach (bool b in bossWins) if (b) n++; return n;
                default: return 0;
            }
        }
    }

    public sealed class RunState
    {
        public readonly string id = Guid.NewGuid().ToString("N");
        public int seed, kills, skillPoints, earnedCrystals, level = 1, experience, nextExperience = 12, rerolls = 2;
        public float seconds;
        public BiomeId biome;
        public CharacterDefinition character;
        public bool settled;
        public readonly Dictionary<string, int> upgrades = new Dictionary<string, int>();
        public readonly Dictionary<string, int> blessings = new Dictionary<string, int>();
        public int BlessingLevel(string id) => blessings.TryGetValue(id, out int n) ? n : 0;
        public readonly HashSet<AbilityKind> abilities = new HashSet<AbilityKind>();
        public readonly HashSet<string> completedWorldEvents = new HashSet<string>();
        public int Level(string id) => upgrades.TryGetValue(id, out int n) ? n : 0;
        public float Effect(GameCatalog catalog, UpgradeEffect effect, AbilityKind target = AbilityKind.All)
        {
            float result = 0;
            foreach (var p in upgrades)
            {
                var u = catalog.Upgrade(p.Key);
                if (u != null && u.effect == effect && (u.target == AbilityKind.All || u.target == target)) result += u.value * p.Value;
            }
            if (catalog.blessings != null)
                foreach (var definition in catalog.blessings)
                    if (definition.effect == effect) result += definition.value * BlessingLevel(definition.id);
            return result;
        }
    }
}
