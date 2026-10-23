using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VampireLike.Editor
{
    public static class ExpansionContent
    {
        static UpgradeDefinition U(string id, string name, string description, AbilityKind target, UpgradeEffect effect, float value, string branch, string task = "", bool unique = false)
            => new UpgradeDefinition { id=id, name=name, description=description, target=target, effect=effect, value=value, branch=branch, taskId=task, unique=unique, cost=4, color=target==AbilityKind.Crossbow?ArtFactory.Gold:target==AbilityKind.Boomerang?ArtFactory.Teal:ArtFactory.Hex("83BEE1") };
        static AbilityDefinition A(string id, string name, string description, AbilityKind kind, float damage, float cooldown, float range, float speed, Color color, string task="")
            => new AbilityDefinition { id=id,name=name,description=description,kind=kind,damage=damage,cooldown=cooldown,range=range,speed=speed,color=color,taskId=task,prefabId="weapon/"+id,projectileId="projectile/"+id };
        static TaskDefinition T(string id,string name,string description,TaskMetric metric,int target,string reward,bool ability=false)
            => new TaskDefinition { id=id,name=name,description=description,metric=metric,target=target,rewardId=reward,rewardKind=ability?UnlockKind.Ability:UnlockKind.Upgrade,crystals=20 };
        public static void Apply(GameCatalog catalog)
        {
            var abilities=new List<AbilityDefinition>(catalog.abilities);
            abilities.Add(A("crossbow","连弩","短暂拉弦后释放高速弩矢，沿平行轨道贯穿敌人。",AbilityKind.Crossbow,12,.85f,20,30,ArtFactory.Gold));
            abilities.Add(A("boomerang","回旋刃","沿弧线掷出回旋刃，回程随玩家走位弯曲，往返各命中一次。",AbilityKind.Boomerang,21,2.5f,15,17,ArtFactory.Teal,"boomerang"));
            abilities.Add(A("froststaff","寒霜法杖","向前喷出四段扇形寒气，连续接触积累冰冻。",AbilityKind.FrostStaff,23,2.8f,8,15,ArtFactory.Hex("83BEE1"),"froststaff"));
            catalog.abilities=abilities.ToArray();
            var upgrades=new List<UpgradeDefinition>(catalog.upgrades);
            upgrades.Add(U("ability.crossbow","连弩","短暂拉弦后释放高速弩矢，沿平行轨道贯穿敌人。",AbilityKind.Crossbow,UpgradeEffect.NewAbility,1,"弹道",unique:true));
            upgrades.Add(U("ability.boomerang","回旋刃","沿弧线掷出回旋刃，回程随玩家走位弯曲，往返各命中一次。",AbilityKind.Boomerang,UpgradeEffect.NewAbility,1,"剑修","boomerang",true));
            upgrades.Add(U("ability.froststaff","寒霜法杖","向前喷出四段扇形寒气，连续接触积累冰冻。",AbilityKind.FrostStaff,UpgradeEffect.NewAbility,1,"元素","froststaff",true));
            upgrades.Add(U("crossbow.volley","并列弩机","连弩每轮额外发射1枚弩矢。",AbilityKind.Crossbow,UpgradeEffect.Count,1,"弹道"));
            upgrades.Add(U("crossbow.reload","轻巧上弦","连弩攻击速度提高18%。",AbilityKind.Crossbow,UpgradeEffect.AttackSpeed,.18f,"弹道"));
            upgrades.Add(U("crossbow.pierce","重型箭镞","弩矢额外贯穿1个目标。",AbilityKind.Crossbow,UpgradeEffect.Pierce,1,"弹道"));
            upgrades.Add(U("crossbow.bounce","折返箭头","弩矢可转向弹射1次。",AbilityKind.Crossbow,UpgradeEffect.Bounce,1,"弹道"));
            upgrades.Add(U("boomerang.count","双月齐旋","每次额外投掷1枚回旋刃。",AbilityKind.Boomerang,UpgradeEffect.Count,1,"剑修"));
            upgrades.Add(U("boomerang.reach","远行刃轨","回旋刃向外飞行距离增加2米。",AbilityKind.Boomerang,UpgradeEffect.Range,2,"剑修"));
            upgrades.Add(U("boomerang.damage","磨砺刃锋","回旋刃伤害提高22%。",AbilityKind.Boomerang,UpgradeEffect.Damage,.22f,"剑修"));
            upgrades.Add(U("boomerang.haste","御风回旋","回旋刃投掷速度提高16%。",AbilityKind.Boomerang,UpgradeEffect.AttackSpeed,.16f,"剑修"));
            upgrades.Add(U("froststaff.count","双晶施法","寒气叠加1重施法伤害，并扩大扇形角度。",AbilityKind.FrostStaff,UpgradeEffect.Count,1,"元素"));
            upgrades.Add(U("froststaff.radius","寒域扩张","寒气射程增加0.7米，扇形角度扩大。",AbilityKind.FrostStaff,UpgradeEffect.Explosion,.7f,"元素"));
            upgrades.Add(U("froststaff.freeze","深寒结晶","每段寒气的冰冻积累增加15%。",AbilityKind.FrostStaff,UpgradeEffect.Frost,.15f,"元素"));
            upgrades.Add(U("froststaff.haste","霜语连咏","寒霜法杖攻击速度提高16%。",AbilityKind.FrostStaff,UpgradeEffect.AttackSpeed,.16f,"元素"));
            upgrades.Add(U("crossbow.frost","淬霜弩矢","弩矢附带减速与35%冻结概率。",AbilityKind.Crossbow,UpgradeEffect.Frost,.35f,"联动","crossbow.frost",true));
            upgrades.Add(U("boomerang.lightning","回旋电刃","回旋刃命中会向附近目标传导电弧。",AbilityKind.Boomerang,UpgradeEffect.Chain,1,"联动","boomerang.lightning",true));
            upgrades.Add(U("froststaff.shatter","碎霜领域","寒气对冻结目标额外造成60%伤害并迸出碎冰。",AbilityKind.FrostStaff,UpgradeEffect.IceShatter,1,"联动","froststaff.shatter",true));
            catalog.upgrades=upgrades.ToArray();
            var tasks=new List<TaskDefinition>(catalog.tasks);
            tasks.Add(T("boomerang","往返之道","累计完成4次探索事件，解锁回旋刃。",TaskMetric.Events,4,"ability.boomerang",true));
            tasks.Add(T("froststaff","霜语初醒","累计冻结40个敌人，解锁寒霜法杖。",TaskMetric.Freezes,40,"ability.froststaff",true));
            tasks.Add(T("crossbow.frost","霜镞试炼","累计用连弩击败80个敌人。",TaskMetric.CrossbowKills,80,"crossbow.frost"));
            tasks.Add(T("boomerang.lightning","电刃回响","累计用回旋刃击败80个敌人。",TaskMetric.BoomerangKills,80,"boomerang.lightning"));
            tasks.Add(T("froststaff.shatter","碎晶回声","累计用寒霜法杖击败80个敌人。",TaskMetric.FrostStaffKills,80,"froststaff.shatter"));
            catalog.tasks=tasks.ToArray();
            var biomes=new List<BiomeDefinition>(catalog.biomes);
            biomes.Add(new BiomeDefinition{id=BiomeId.Swamp,name="荧光菌沼",subtitle="菌伞、浅潭与栈道，避开孢囊和跃击。",bossName="菌冠巨蛙",terrainId="terrain/swamp",decorationId="decoration/swamp",bossId="boss/swamp",ground=ArtFactory.Hex("709A85"),secondary=ArtFactory.Hex("C4DDB2"),accent=ArtFactory.Hex("8F75B4"),enemyTypes=new[]{5,6,0,3}});
            biomes.Add(new BiomeDefinition{id=BiomeId.Crystal,name="赤晶峡谷",subtitle="赤晶簇、层岩与古门，注意棱镜交叉火力。",bossName="赤晶守卫",terrainId="terrain/crystal",decorationId="decoration/crystal",bossId="boss/crystal",ground=ArtFactory.Hex("BA918A"),secondary=ArtFactory.Hex("ECD7BC"),accent=ArtFactory.Hex("C65E80"),enemyTypes=new[]{7,8,2,1}});
            catalog.biomes=biomes.ToArray();
            catalog.enemies=new[]{
                new EnemyDefinition{type=0,name="莹叶史莱姆",description="持续靠近的林地生物。",prefabId="enemy/0",health=15,speed=2.45f,damage=7},
                new EnemyDefinition{type=1,name="迅足兔",description="快速追逐目标。",prefabId="enemy/1",health=8,speed=4.5f,damage=7},
                new EnemyDefinition{type=2,name="厚甲犀",description="预警后沿直线冲锋。",prefabId="enemy/2",health=30,speed=2.15f,damage=13},
                new EnemyDefinition{type=3,name="孢子法师",description="保持距离发射魔法弹。",prefabId="enemy/3",health=15,speed=2,damage=7},
                new EnemyDefinition{type=5,name="孢囊行者",description="在目标附近散布延迟爆发的孢囊。",prefabId="enemy/5",health=23,speed=1.75f,damage=9},
                new EnemyDefinition{type=6,name="跃沼蛙",description="蓄力后跃向锁定落点。",prefabId="enemy/6",health=19,speed=2.4f,damage=11},
                new EnemyDefinition{type=7,name="晶甲虫",description="正面晶甲减伤，蓄力冲锋后短暂暴露。",prefabId="enemy/7",health=37,speed=1.9f,damage=13},
                new EnemyDefinition{type=8,name="棱镜精灵",description="远距发射扇形晶弹。",prefabId="enemy/8",health=20,speed=2.1f,damage=9}
            };
            EditorUtility.SetDirty(catalog);
        }
    }
}
