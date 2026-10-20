using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VampireLike
{
    public enum BiomeId { Forest, Desert, Snow, Volcano, Mixed }
    public enum AbilityKind { All = -1, Gun, Fireball, Sword, Spirit, Cannon, Rune }
    public enum UpgradeEffect { Damage, AttackSpeed, MoveSpeed, Health, Pickup, Count, Range, Pierce, Bounce, Burn, Frost, Lightning, Orbit, SwordRain, SwordWave, Spread, Burst, Explosion, NewAbility, Chain, FireSword, IceShatter, ElectricAmmo, Duration, Crit, Armor, Regeneration, Resource, SummonSpeed, RunePulse, FireSpread }
    public enum TaskMetric { Kills, GunKills, FireKills, SwordKills, Freezes, EliteKills, BestSeconds, Bosses, Events, ElectricCombo, ShatterCombo, SwordFireCombo }
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
    [Serializable] public sealed class TaskDefinition
    {
        public string id, name, description, rewardId;
        public TaskMetric metric;
        public int target, crystals = 15;
        public UnlockKind rewardKind;
    }
    [Serializable] public sealed class BiomeDefinition
    {
        public BiomeId id;
        public string name, subtitle, bossName, terrainId, decorationId, bossId;
        public Color ground, secondary, accent;
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

        public AssetReferenceGameObject Prefab(string id)
        {
            foreach (var p in prefabs) if (p.id == id) return p.reference;
            throw new KeyNotFoundException("Unknown prefab: " + id);
        }
        public AbilityDefinition Ability(AbilityKind kind) => Array.Find(abilities, a => a.kind == kind);
        public CharacterDefinition Character(string id) => Array.Find(characters, a => a.id == id) ?? characters[0];
        public BiomeDefinition Biome(BiomeId id) => biomes[(int)(id == BiomeId.Mixed ? BiomeId.Forest : id)];
        public UpgradeDefinition Upgrade(string id) => Array.Find(upgrades, a => a.id == id);
    }

    [Serializable] public sealed class PlayerProfile
    {
        public int version = 1, crystals, totalKills, gunKills, fireKills, swordKills, freezes, elites, events;
        public int damageLevel, healthLevel, speedLevel, pickupLevel;
        public int electricCombo, shatterCombo, swordFireCombo;
        public float bestSeconds, musicVolume = .35f, soundVolume = .65f;
        public int quality = 2;
        public bool[] bossWins = new bool[4];
        public List<string> completedTasks = new List<string>();
        public List<string> settledRuns = new List<string>();
        public int Metric(TaskMetric metric)
        {
            switch (metric)
            {
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
            return result;
        }
    }
}
