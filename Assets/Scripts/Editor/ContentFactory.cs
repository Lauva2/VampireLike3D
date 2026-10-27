using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace VampireLike.Editor
{
    public static class ContentFactory
    {
        public static GameCatalog Create(List<PrefabEntry> prefabs, List<AudioEntry> audio)
        {
            const string path = "Assets/Config/Catalogs/GameCatalog.asset"; Directory.CreateDirectory("Assets/Config/Catalogs");
            var c = AssetDatabase.LoadAssetAtPath<GameCatalog>(path);
            if (c == null) { c = ScriptableObject.CreateInstance<GameCatalog>(); AssetDatabase.CreateAsset(c, path); }
            c.prefabs = prefabs.ToArray(); c.audio = audio.ToArray();
            c.blessings = new[]
            {
                new BlessingDefinition { id="vitality", name="生命回响", description="最大生命 +18，并立即恢复新增的生命上限。", icon="生", effect=UpgradeEffect.Health, value=18, rerolls=0, color=ArtFactory.Hex("D88171") },
                new BlessingDefinition { id="might", name="锋芒祝福", description="所有能力伤害 +8%。与现有武器强化共同叠加。", icon="剑", effect=UpgradeEffect.Damage, value=0.08f, rerolls=0, color=ArtFactory.Hex("D3A458") },
                new BlessingDefinition { id="momentum", name="疾风眷顾", description="移动速度 +5%，让探索和躲避更加从容。", icon="足", effect=UpgradeEffect.MoveSpeed, value=0.05f, rerolls=0, color=ArtFactory.Hex("62A990") },
                new BlessingDefinition { id="attraction", name="星尘引路", description="经验拾取半径 +0.6米，更容易收集战场收获。", icon="引", effect=UpgradeEffect.Pickup, value=0.6f, rerolls=0, color=ArtFactory.Hex("779EC1") },
                new BlessingDefinition { id="insight", name="战术启迪", description="获得1次本局升级重抽，可稍后使用。", icon="途", effect=UpgradeEffect.Damage, value=0, rerolls=1, color=ArtFactory.Hex("AA87BC") },
                new BlessingDefinition { id="renewal", name="生息祝福", description="每秒恢复0.3生命。可在后续祭坛继续叠加。", icon="林", effect=UpgradeEffect.Regeneration, value=0.3f, rerolls=0, color=ArtFactory.Hex("79AA76") },
            };
            c.characters = new[]
            {
                new CharacterDefinition { id="adventurer", name="远行者", subtitle="弹道起步 · 灵活均衡", description="从手枪启程，移动速度提高5%。", prefabId="character/adventurer", startingAbility=AbilityKind.Gun, color=ArtFactory.Teal, speedBonus=.05f },
                new CharacterDefinition { id="elementalist", name="元素学徒", subtitle="魔法起步 · 元素共鸣", description="初始火球，所有伤害提高12%。", prefabId="character/elementalist", startingAbility=AbilityKind.Fireball, color=ArtFactory.Hex("8A78B5"), taskId="mage", damageBonus=.12f },
                new CharacterDefinition { id="swordsman", name="御剑者", subtitle="飞剑起步 · 生生不息", description="初始飞剑，最大生命增加20点。", prefabId="character/swordsman", startingAbility=AbilityKind.Sword, color=ArtFactory.Hex("6CAC8C"), taskId="swordsman", healthBonus=20 }
            };
            c.abilities = new[]
            {
                A("gun","自动手枪","自动瞄准点射，连射与散射可以叠加。",AbilityKind.Gun,9,.65f,17,25,ArtFactory.Gold),
                A("fireball","火球","短暂蓄亮后抛射火球，落点爆炸清扫敌群。",AbilityKind.Fireball,16,1.8f,17,13,ArtFactory.Orange),
                A("sword","飞剑","飞剑自周身沿曲线御敌，贯穿后无伤回返。",AbilityKind.Sword,14,1.25f,17,19,ArtFactory.Teal),
                A("spirit","灵体","召唤灵体跟随、蓄势扑击敌人并回位。",AbilityKind.Spirit,13,1.3f,16,17,ArtFactory.Hex("81C9BC"),"spirit"),
                A("cannon","爆破炮","装填后高抛炮弹，落地爆炸并显示落点。",AbilityKind.Cannon,36,3.8f,20,13,ArtFactory.Hex("CF9B59"),"cannon"),
                A("rune","符阵","展开脚下符阵，叠阵强化追加分层脉冲。",AbilityKind.Rune,24,4.5f,5,10,ArtFactory.Hex("B5A172"),"rune")
            };
            var u = new List<UpgradeDefinition>();
            u.Add(U("damage","力量汇流","所有自动能力伤害提高12%。",UpgradeEffect.Damage,.12f));
            u.Add(U("haste","疾风节拍","所有自动能力攻击速度提高10%。",UpgradeEffect.AttackSpeed,.1f));
            u.Add(U("vitality","生机","最大生命 +20，并回复20点生命。",UpgradeEffect.Health,20));
            u.Add(U("swiftness","轻步","移动速度提高7%。",UpgradeEffect.MoveSpeed,.07f));
            u.Add(U("magnet","拾光","经验拾取范围增加0.7。",UpgradeEffect.Pickup,.7f));
            u.Add(U("crit","灵光一击","暴击率增加8%，暴击造成额外伤害。",UpgradeEffect.Crit,.08f));
            u.Add(U("armor","护身","提高护甲，降低受到的伤害。",UpgradeEffect.Armor,.15f));
            u.Add(U("regen","回春","每秒恢复0.5点生命。",UpgradeEffect.Regeneration,.5f));
            u.Add(U("range","远见","能力索敌与范围增加1.2。",UpgradeEffect.Range,1.2f));
            u.Add(U("projectiles","万象增殖","所有投射能力每次增加1枚投射物。",UpgradeEffect.Count,1,cost:5));
            foreach (var ability in c.abilities)
                u.Add(U("ability."+ability.id,ability.name,ability.description,UpgradeEffect.NewAbility,1,ability.kind,Branch(ability.kind),ability.taskId,true,4));
            u.Add(U("burn","灼烧","火球附加灼烧，并在爆炸处留下短暂火焰区域。",UpgradeEffect.Burn,.22f,AbilityKind.Fireball,"元素"));
            u.Add(U("spreadfire","烈焰扩散","灼烧目标死亡时向附近敌人传播火焰。",UpgradeEffect.FireSpread,1,AbilityKind.Fireball,"元素","spreadfire",true,5));
            u.Add(U("frost","冰霜新星","周期性释放冰环，并让火球减速、冻结敌人。",UpgradeEffect.Frost,.2f,AbilityKind.Fireball,"元素","frost",true,5));
            u.Add(U("lightning","落雷","周期性雷击附近目标，可与冰火同时存在。",UpgradeEffect.Lightning,1,AbilityKind.Fireball,"元素","lightning",true,5));
            u.Add(U("chain","连锁雷击","火球命中引发电弧，跳向附近敌人。",UpgradeEffect.Chain,2,AbilityKind.Fireball,"元素","chain",false,5));
            u.Add(U("burst","连射","手枪每轮额外连续射击1次，每次保留散射弹道。",UpgradeEffect.Burst,1,AbilityKind.Gun,"弹道"));
            u.Add(U("pierce","穿透弹","子弹额外贯穿1个目标。",UpgradeEffect.Pierce,1,AbilityKind.Gun,"弹道","pierce"));
            u.Add(U("spread","散射","手枪增加两条侧向弹道。",UpgradeEffect.Spread,2,AbilityKind.Gun,"弹道","spread"));
            u.Add(U("bounce","弹射","子弹命中后转向新的敌人。",UpgradeEffect.Bounce,1,AbilityKind.Gun,"弹道","bounce"));
            u.Add(U("explosion","爆破弹","子弹命中产生爆炸，后续强化扩大爆炸范围。",UpgradeEffect.Explosion,.85f,AbilityKind.Gun,"爆破","explosion",false,5));
            u.Add(U("swordcount","飞剑增殖","每轮额外派出1柄追击飞剑。",UpgradeEffect.Count,1,AbilityKind.Sword,"剑修"));
            u.Add(U("swordpierce","贯穿剑","飞剑额外穿过1个目标。",UpgradeEffect.Pierce,1,AbilityKind.Sword,"剑修","swordpierce"));
            u.Add(U("orbit","绕身剑阵","飞剑剑尖朝前绕身旋转，保留原有追击飞剑。",UpgradeEffect.Orbit,1,AbilityKind.Sword,"剑修","orbit",true,6));
            u.Add(U("rain","万剑齐发","在目标位置展开剑域，持续落下剑雨，保留追击与剑阵。",UpgradeEffect.SwordRain,1,AbilityKind.Sword,"剑修","rain",true,6));
            u.Add(U("wave","剑气","飞剑命中后对附近敌人释放范围剑气。",UpgradeEffect.SwordWave,1,AbilityKind.Sword,"剑修","wave",true,5));
            u.Add(U("spiritcount","灵体增殖","额外召唤1个灵体，共同扑击目标。",UpgradeEffect.Count,1,AbilityKind.Spirit,"召唤"));
            u.Add(U("spiritdamage","灵契","灵体伤害提高25%。",UpgradeEffect.Damage,.25f,AbilityKind.Spirit,"召唤"));
            u.Add(U("spiritspeed","心意相通","灵体出击频率提高20%，扑击随之加快。",UpgradeEffect.SummonSpeed,.2f,AbilityKind.Spirit,"召唤"));
            u.Add(U("cannoncount","多重炮击","爆破炮每次额外发射1枚炮弹。",UpgradeEffect.Count,1,AbilityKind.Cannon,"爆破",cost:5));
            u.Add(U("cannonradius","烈性装药","爆破炮爆炸范围增加0.8。",UpgradeEffect.Explosion,.8f,AbilityKind.Cannon,"爆破"));
            u.Add(U("runerange","阵域","符阵范围增加1.1。",UpgradeEffect.Range,1.1f,AbilityKind.Rune,"符阵"));
            u.Add(U("runepulse","叠阵","符阵总伤害提高35%，并追加一层脉冲。",UpgradeEffect.RunePulse,.35f,AbilityKind.Rune,"符阵"));
            u.Add(U("runecooldown","符意流转","符阵触发速度提高18%。",UpgradeEffect.AttackSpeed,.18f,AbilityKind.Rune,"符阵"));
            u.Add(U("electricammo","雷电弹药","子弹命中后引发连锁电弧。",UpgradeEffect.ElectricAmmo,1,AbilityKind.Gun,"联动","electricammo",true,7));
            u.Add(U("shatter","碎冰冲击","子弹对冻结目标造成60%额外伤害。",UpgradeEffect.IceShatter,1,AbilityKind.Gun,"联动","shatter",true,6));
            u.Add(U("firesword","烈焰飞剑","飞剑附加灼烧，并可参与火焰传播。",UpgradeEffect.FireSword,.3f,AbilityKind.Sword,"联动","firesword",true,6));
            c.upgrades = u.ToArray();
            c.tasks = new[]
            {
                T("spirit","初识万象","累计击败60个敌人，解锁灵体。",TaskMetric.Kills,60,"ability.spirit",UnlockKind.Ability),
                T("cannon","火力扩张","累计击败120个敌人，解锁爆破炮。",TaskMetric.Kills,120,"ability.cannon",UnlockKind.Ability),
                T("rune","古老回响","完成2次探索事件，解锁符阵。",TaskMetric.Events,2,"ability.rune",UnlockKind.Ability),
                T("mage","元素新生","累计用火焰击败80个敌人，解锁元素学徒。",TaskMetric.FireKills,80,"elementalist",UnlockKind.Character),
                T("swordsman","剑心初成","累计用飞剑击败80个敌人，解锁御剑者。",TaskMetric.SwordKills,80,"swordsman",UnlockKind.Character),
                T("spreadfire","薪火相传","累计用火焰击败40个敌人，解锁烈焰扩散。",TaskMetric.FireKills,40,"spreadfire"),
                T("frost","寒意初临","单局生存达到90秒，解锁冰霜新星。",TaskMetric.BestSeconds,90,"frost"),
                T("lightning","雷声渐起","累计击败2个精英，解锁落雷。",TaskMetric.EliteKills,2,"lightning"),
                T("chain","电光交织","累计用火焰击败180个敌人，解锁连锁雷击。",TaskMetric.FireKills,180,"chain"),
                T("pierce","弹无虚发","累计用枪械击败30个敌人，解锁穿透弹。",TaskMetric.GunKills,30,"pierce"),
                T("spread","弹幕初现","累计用枪械击败100个敌人，解锁散射。",TaskMetric.GunKills,100,"spread"),
                T("bounce","巧妙弹道","累计用枪械击败250个敌人，解锁弹射。",TaskMetric.GunKills,250,"bounce"),
                T("explosion","探索者","完成3次探索事件，解锁爆破弹。",TaskMetric.Events,3,"explosion"),
                T("swordpierce","一剑贯之","累计用飞剑击败30个敌人，解锁贯穿剑。",TaskMetric.SwordKills,30,"swordpierce"),
                T("orbit","御剑护身","单局生存达到180秒，解锁绕身剑阵。",TaskMetric.BestSeconds,180,"orbit"),
                T("rain","剑雨将至","累计击败5个精英，解锁万剑齐发。",TaskMetric.EliteKills,5,"rain"),
                T("wave","剑气纵横","累计用飞剑击败220个敌人，解锁剑气。",TaskMetric.SwordKills,220,"wave"),
                T("electricammo","雷火交响","拥有落雷时累计用枪械击败30个敌人。",TaskMetric.ElectricCombo,30,"electricammo"),
                T("shatter","冰封破晓","累计用枪械击败20个冻结敌人。",TaskMetric.ShatterCombo,20,"shatter"),
                T("firesword","剑与火","同时拥有火球时累计用飞剑击败40个敌人。",TaskMetric.SwordFireCombo,40,"firesword")
            };
            string[] names = { "青翠林地", "流金沙海", "晴雪原野", "暖焰火山" }, ids = { "forest", "desert", "snow", "volcano" };
            string[] subtitles = { "林间空地与古老石阵，怪群从四周聚集。", "沙丘、绿洲与遗迹，在开阔地带应对追逐。", "冰脊与晴朗雪地，在地形间寻找安全路线。", "温暖岩地与喷发区域，留意地面的危险预警。" };
            string[] bosses = { "古树守卫", "巨型沙虫", "霜原巨兽", "熔岩巨像" };
            c.biomes = new BiomeDefinition[4];
            for (int i = 0; i < 4; i++) c.biomes[i] = new BiomeDefinition { id=(BiomeId)i, name=names[i], subtitle=subtitles[i], bossName=bosses[i], terrainId="terrain/"+ids[i], decorationId="decoration/"+ids[i], bossId="boss/"+ids[i], ground=ArtFactory.GroundColors[i], secondary=ArtFactory.Cream, accent=ArtFactory.AccentColors[i] };
            ExpansionContent.Apply(c);
            EditorUtility.SetDirty(c); return c;
        }
        static AbilityDefinition A(string id,string name,string description,AbilityKind kind,float damage,float cooldown,float range,float speed,Color color,string task="") => new AbilityDefinition { id=id,name=name,description=description,kind=kind,damage=damage,cooldown=cooldown,range=range,speed=speed,color=color,taskId=task,prefabId="weapon/"+id,projectileId="projectile/"+id };
        static UpgradeDefinition U(string id,string name,string description,UpgradeEffect effect,float value,AbilityKind target=AbilityKind.All,string branch="通用",string task="",bool unique=false,int cost=3) => new UpgradeDefinition { id=id,name=name,description=description,effect=effect,value=value,target=target,branch=branch,taskId=task,unique=unique,cost=cost,color=target==AbilityKind.All?ArtFactory.Teal:target==AbilityKind.Fireball?ArtFactory.Orange:target==AbilityKind.Sword?ArtFactory.Teal:ArtFactory.Gold };
        static TaskDefinition T(string id,string name,string description,TaskMetric metric,int target,string reward,UnlockKind kind=UnlockKind.Upgrade) => new TaskDefinition { id=id,name=name,description=description,metric=metric,target=target,rewardId=reward,rewardKind=kind,crystals=15 };
        static string Branch(AbilityKind kind) { switch(kind) { case AbilityKind.Gun:return "弹道";case AbilityKind.Fireball:return "元素";case AbilityKind.Sword:return "剑修";case AbilityKind.Spirit:return "召唤";case AbilityKind.Cannon:return "爆破";default:return "符阵"; } }
    }
}
