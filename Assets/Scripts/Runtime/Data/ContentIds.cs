using System;
namespace VampireLike
{
    public static class ContentIds
    {
        // Existing serialized enum numbers never move; Mixed remains4.
        public static readonly BiomeId[] Regions = { BiomeId.Forest, BiomeId.Desert, BiomeId.Snow, BiomeId.Volcano, BiomeId.Swamp, BiomeId.Crystal };
        public static readonly BiomeId[] Maps = { BiomeId.Forest, BiomeId.Desert, BiomeId.Snow, BiomeId.Volcano, BiomeId.Mixed, BiomeId.Swamp, BiomeId.Crystal };
        public static readonly AbilityKind[] Weapons = { AbilityKind.Gun, AbilityKind.Fireball, AbilityKind.Sword, AbilityKind.Spirit, AbilityKind.Cannon, AbilityKind.Rune, AbilityKind.Crossbow, AbilityKind.Boomerang, AbilityKind.FrostStaff };
        public static int RegionIndex(BiomeId id) => Array.IndexOf(Regions, id);
        public static string RegionKey(BiomeId id)
        {
            switch (id) { case BiomeId.Forest:return "forest";case BiomeId.Desert:return "desert";case BiomeId.Snow:return "snow";case BiomeId.Volcano:return "volcano";case BiomeId.Swamp:return "swamp";case BiomeId.Crystal:return "crystal"; default:throw new ArgumentOutOfRangeException(nameof(id)); }
        }
    }
}
