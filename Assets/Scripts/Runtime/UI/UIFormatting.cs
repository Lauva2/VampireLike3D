using UnityEngine;
namespace VampireLike
{
    public static class UIFormatting
    {
        public static string Clock(float seconds) => ((int)seconds / 60).ToString("00") + ":" + ((int)seconds % 60).ToString("00");
        public static string Icon(AbilityKind kind)
        {
            switch (kind) { case AbilityKind.Gun: return "弹"; case AbilityKind.Fireball: return "焰"; case AbilityKind.Sword: return "剑"; case AbilityKind.Spirit: return "灵"; case AbilityKind.Cannon: return "炮"; case AbilityKind.Rune: return "符"; case AbilityKind.Crossbow: return "弩"; case AbilityKind.Boomerang: return "旋"; case AbilityKind.FrostStaff: return "杖"; default: return "星"; }
        }
        public static string Icon(UpgradeDefinition upgrade)
        {
            if(upgrade.target!=AbilityKind.All)return Icon(upgrade.target);
            switch(upgrade.effect){case UpgradeEffect.Damage:return "攻";case UpgradeEffect.AttackSpeed:return "速";case UpgradeEffect.Health:case UpgradeEffect.Regeneration:return "生";case UpgradeEffect.MoveSpeed:return "足";case UpgradeEffect.Pickup:return "引";case UpgradeEffect.Armor:return "盾";case UpgradeEffect.Range:return "距";case UpgradeEffect.Count:return "叠";default:return "星";}
        }
    }
}
