using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        public static GameObject ExpansionEnemy(int type, bool boss = false)
        {
            string id = boss ? (type == 5 ? "swamp" : "crystal") : type.ToString();
            var root = Root((boss ? "Boss-" : "Enemy-") + id); root.layer = 8;
            var actor = root.AddComponent<EnemyActor>(); var collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true; collider.radius = boss ? 1.7f : .65f; collider.center = Vector3.up * (boss ? 1.6f : .7f);
            actor.visual = Root("Visual", root.transform).transform; var t = actor.visual;
            if (boss && type == 6)
            {
                Sphere(t, "HeartCore", new Vector3(0, 1.9f, 0), new Vector3(1.5f, 1.7f, 1.3f), Hex("F0CDB5"));
                for (int side = -1; side <= 1; side += 2)
                {
                    for (int j = 0; j < 3; j++) GroundRock(t, "LayeredArmor", new Vector3(side * .58f, 1.2f + j * .5f, .22f), new Vector3(.9f, .64f, 1.25f), j % 2 == 0 ? Sandstone : Hex("DBB1A5"), j + side + 7);
                    Crystal(t, new Vector3(side * 1.2f, 2.1f, 0), 1.7f, .46f, Rose, side * 23);
                    Rock(t, "Arm", new Vector3(side * 1.4f, 1.8f, 0), new Vector3(.9f, 1.4f, .9f), Hex("B78586"), side + 6);
                    Crystal(t, new Vector3(side * 1.4f, .9f, .2f), .9f, .4f, Hex("D593A3"), side * -9);
                    GroundRock(t, "Foot", new Vector3(side * .6f, .3f, .1f), new Vector3(1, .6f, 1.2f), Sandstone, side + 11);
                }
                Rock(t, "Helmet", new Vector3(0, 3.15f, .05f), new Vector3(1.8f, 1.25f, 1.4f), Hex("D7A9A6"), 0);
                for (int i = 0; i < 3; i++) Crystal(t, new Vector3((i - 1) * .4f, 3.6f, -.1f), .9f + (i == 1 ? .4f : 0), .22f, Rose, (i - 1) * 15);
                Crystal(t, new Vector3(0, 1.9f, .72f), .8f, .22f, Hex("F1D8C1"), 0);
                Eyes(t, new Vector3(0, 3.2f, .75f), 1.75f, Hex("6D435F"));
            }
            else if (type == 6 || boss)
            {
                float s = boss ? 2.65f : 1;
                Color skin = boss ? Hex("699B84") : Hex("9BBC79");
                Sphere(t, "FrogBody", new Vector3(0, .58f, -.08f) * s, new Vector3(1.1f, .85f, 1.1f) * s, skin);
                Sphere(t, "CreamBelly", new Vector3(0, .46f, .35f) * s, new Vector3(.8f, .48f, .38f) * s, Hex("D3DCA2"));
                for (int side = -1; side <= 1; side += 2)
                {
                    Sphere(t, "EyeMound", new Vector3(side * .32f, .94f, .28f) * s, Vector3.one * .44f * s, skin);
                    Sphere(t, "HindThigh", new Vector3(side * .55f, .28f, -.25f) * s, new Vector3(.52f, .4f, .7f) * s, skin);
                    Sphere(t, "WebbedFoot", new Vector3(side * .55f, .12f, .3f) * s, new Vector3(.55f, .16f, .58f) * s, Hex("638D72"));
                    for (int toe = 0; toe < 3; toe++) Sphere(t, "Toe", new Vector3(side * .55f + (toe - 1) * .14f, .11f, .6f) * s, new Vector3(.14f, .12f, .2f) * s, Hex("B3C98E"));
                }
                Eyes(t, new Vector3(0, .96f, .5f) * s, 1.2f * s, Hex("50646A"));
                Tube(t, "Smile", new Vector3(-.28f, .58f, .57f) * s, new Vector3(.28f, .58f, .57f) * s, .024f * s, Hex("526D5D"));
                if (boss)
                {
                    for (int i = 0; i < 5; i++) Mushroom(t, new Vector3(Mathf.Sin(i * 2.4f) * 1, 1.3f, -.5f + Mathf.Cos(i * 2.4f) * .5f), .8f + i % 3 * .2f, i % 2 == 0 ? Violet : Hex("8DB9A7"));
                    var crown = Ring(t, "RoyalCrown", .65f, .09f, Gold); crown.transform.localPosition = new Vector3(0, 2.9f, .55f);
                    for (int i = 0; i < 5; i++) Crystal(t, new Vector3(Mathf.Sin(i * 1.26f) * .6f, 2.9f, .55f + Mathf.Cos(i * 1.26f) * .6f), .4f, .1f, Gold);
                }
            }
            else if (type == 5)
            {
                Capsule(t, "StalkBody", new Vector3(0, .56f, 0), new Vector3(.65f, .42f, .65f), Hex("B7CEA1"));
                Sphere(t, "SporeSack", new Vector3(0, .98f, -.2f), new Vector3(1.1f, 1.25f, .95f), Violet);
                Mushroom(t, new Vector3(0, .5f, -.1f), .82f, Hex("8C70A7"));
                for (int i = 0; i < 5; i++) Sphere(t, "SporePore", new Vector3(Mathf.Sin(i * 2.4f) * .42f, 1.2f + i % 2 * .22f, -.22f + Mathf.Cos(i * 2.4f) * .35f), Vector3.one * .18f, Hex("D9E8A9"));
                for (int side = -1; side <= 1; side += 2) Sphere(t, "RootFoot", new Vector3(side * .28f, .14f, .1f), new Vector3(.35f, .22f, .5f), Moss);
                Eyes(t, new Vector3(0, .58f, .36f), .8f, Hex("635573"));
            }
            else if (type == 7)
            {
                Sphere(t, "BeetleBody", new Vector3(0, .5f, -.1f), new Vector3(1.1f, .7f, 1.3f), Hex("946F85"));
                for (int side = -1; side <= 1; side += 2)
                {
                    Rock(t, "FrontArmor", new Vector3(side * .3f, .7f, .28f), new Vector3(.7f, .7f, .9f), Hex("D6A09E"), side + 5);
                    for (int i = 0; i < 3; i++) Tube(t, "JointedLeg", new Vector3(side * .42f, .35f, (i - 1) * .35f), new Vector3(side * .84f, .08f, (i - 1) * .45f), .07f, Ink);
                }
                for (int i = 0; i < 3; i++) Crystal(t, new Vector3(0, .75f, (i - 1) * .3f), .58f + i * .12f, .16f, Rose, i * 9);
                Eyes(t, new Vector3(0, .51f, .62f), .83f, Hex("65435F"));
            }
            else
            {
                Rock(t, "FloatingMask", new Vector3(0, .95f, 0), new Vector3(.83f, 1.1f, .65f), Hex("E1B4BB"), 0);
                Crystal(t, new Vector3(0, .55f, 0), 1.35f, .27f, Rose);
                Eyes(t, new Vector3(0, 1.03f, .36f), .75f, Hex("5B536F"));
                for (int side = -1; side <= 1; side += 2)
                {
                    Crystal(t, new Vector3(side * .55f, .67f, -.06f), .73f, .17f, Hex("D89AAB"), side * -24);
                    var orbit = Ring(t, "PrismWing", .36f, .035f, Gold, 230); orbit.transform.localPosition = new Vector3(side * .5f, 1, 0); orbit.transform.localEulerAngles = new Vector3(65, side * 25, 0);
                }
                Crystal(t, new Vector3(0, .08f, 0), .35f, .12f, Hex("E8CBC1"));
            }
            actor.eyeRig = t.Find("FaceEyes");
            if (actor.eyeRig != null) { actor.eyeRig.SetParent(root.transform, true); Bake(actor.eyeRig, (boss ? "boss-" : "enemy-") + id + "-eyes"); }
            Bake(t, (boss ? "boss-" : "enemy-") + id); if (actor.eyeRig != null) actor.eyeRig.SetParent(t, true);
            if (!boss)
            {
                actor.eliteHalo = Root("EliteCrown", root.transform).transform; actor.eliteHalo.localPosition = Vector3.up * (type == 5 ? 1.9f : 1.65f);
                Ring(actor.eliteHalo, "Crown", .46f, .045f, Gold);
                for (int i = 0; i < 5; i++) Crystal(actor.eliteHalo, new Vector3(Mathf.Sin(i * 1.26f) * .42f, 0, Mathf.Cos(i * 1.26f) * .42f), .28f, .065f, Gold);
                for (int side = -1; side <= 1; side += 2)
                {
                    if (type == 5) Mushroom(actor.eliteHalo, new Vector3(side * .55f, -.9f, -.2f), .55f, Hex("C5CD8D"));
                    if (type == 6) Rock(actor.eliteHalo, "FrogPauldron", new Vector3(side * .55f, -1.1f, -.18f), new Vector3(.45f, .3f, .6f), Gold, side + 7);
                    if (type == 7 || type == 8) Crystal(actor.eliteHalo, new Vector3(side * .55f, -.85f, -.2f), .8f, .15f, Gold, side * 28);
                }
                Bake(actor.eliteHalo, "enemy-" + id + "-elite"); actor.eliteHalo.gameObject.SetActive(false);
            }
            Blob(root.transform, boss ? 2.1f : .7f);
            actor.barRoot = Root("HealthBar", root.transform).transform; actor.barRoot.localPosition = Vector3.up * (boss ? 4.9f : type == 5 ? 2 : 1.9f);
            float width = boss ? 2.4f : .95f; var back = Cube(actor.barRoot, "Back", Vector3.zero, new Vector3(width, .12f, .035f), Ink);
            actor.barFill = Root("Fill", actor.barRoot).transform; Cube(actor.barFill, "Green", new Vector3(0, 0, -.025f), new Vector3(width * .94f, .075f, .025f), Hex("84D3A0"));
            Bake(actor.barFill, (boss ? "boss-" : "enemy-") + id + "-barfill"); Bake(back.transform, (boss ? "boss-" : "enemy-") + id + "-barback");
            foreach (var renderer in actor.barRoot.GetComponentsInChildren<MeshRenderer>()) { renderer.sharedMaterial = EffectMaterial(); renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false; }
            return SavePrefab(root, boss ? "Bosses" : "Enemies", id);
        }
        public static GameObject ExpansionWeapon(AbilityKind kind, bool projectile)
        {
            string id = kind.ToString().ToLowerInvariant(); var root = Root(id); var t = Root("Model", root.transform).transform;
            if (kind == AbilityKind.Crossbow && !projectile)
            {
                Cube(t, "WoodStock", Vector3.zero, new Vector3(.16f, .18f, .85f), Brown);
                Cube(t, "BoltMagazine", new Vector3(0, .17f, -.15f), new Vector3(.26f, .25f, .35f), Teal);
                for (int side = -1; side <= 1; side += 2)
                {
                    Tube(t, "BowLimb", new Vector3(0, .03f, .22f), new Vector3(side * .58f, .03f, .05f), .055f, Gold);
                    Tube(t, "BowString", new Vector3(side * .58f, .03f, .05f), new Vector3(0, .03f, -.35f), .012f, Cream);
                }
                Spike(t, "LoadedBolt", new Vector3(0, .13f, .1f), new Vector3(0, .13f, .68f), .045f, Hex("E8DFB4"));
            }
            else if (kind == AbilityKind.Boomerang)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Tube(t, "CurvedWood", new Vector3(0, 0, -.2f), new Vector3(side * .45f, 0, .15f), .11f, Teal);
                    Tube(t, "BladeEdge", new Vector3(side * .12f, .015f, -.2f), new Vector3(side * .55f, .015f, .16f), .036f, Hex("D9EBC8"));
                    Cube(t, "GoldBinding", new Vector3(side * .3f, .02f, .04f), new Vector3(.14f, .12f, .17f), Gold, new Vector3(0, side * 40, 0));
                }
                Sphere(t, "JadeGem", new Vector3(0, .05f, -.18f), new Vector3(.16f, .09f, .16f), Hex("B8E5CB"));
            }
            else if (kind == AbilityKind.FrostStaff && !projectile)
            {
                Tube(t, "SilverShaft", Vector3.down * .6f, Vector3.up * .45f, .048f, Hex("637FA4"));
                var crown = Ring(t, "StaffCrescent", .23f, .04f, Hex("D4E8E6"), 270); crown.transform.localPosition = Vector3.up * .53f; crown.transform.localEulerAngles = new Vector3(90, 0, 45);
                Crystal(t, new Vector3(0, .4f, 0), .5f, .13f, Hex("84CEE4"));
                for (int i = 0; i < 3; i++) { var binding = Ring(t, "Binding", .06f, .015f, Gold); binding.transform.localPosition = Vector3.up * (-.25f + i * .13f); }
            }
            else if (kind == AbilityKind.Crossbow)
            {
                Tube(t, "BoltShaft", Vector3.back * .5f, Vector3.forward * .3f, .035f, Gold);
                Spike(t, "BoltTip", Vector3.forward * .26f, Vector3.forward * .65f, .085f, Hex("DCE8C4"));
                for (int side = -1; side <= 1; side += 2) Cube(t, "BoltFeather", new Vector3(side * .07f, 0, -.4f), new Vector3(.16f, .025f, .21f), Teal, new Vector3(0, side * 24, 0));
            }
            else
            {
                Crystal(t, new Vector3(0, -.15f, 0), .55f, .17f, Hex("91D9EC")); t.localEulerAngles = new Vector3(90, 0, 0);
                for (int i = 0; i < 4; i++) Crystal(t, new Vector3(Mathf.Sin(i * 1.57f) * .2f, -.2f, Mathf.Cos(i * 1.57f) * .2f), .25f, .055f, Cream, i * 13);
            }
            Bake(t, (projectile ? "shot-" : "weapon-") + id);
            if (projectile) { root.AddComponent<ProjectileActor>(); ProjectileTrail(root, kind); foreach (var r in root.GetComponentsInChildren<MeshRenderer>()) { r.sharedMaterial = EffectMaterial(); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false; } }
            return SavePrefab(root, projectile ? "Projectiles" : "Weapons", id);
        }
        public static GameObject PrismProjectile()
        {
            var root = Root("PrismProjectile"); Crystal(root.transform, Vector3.down * .15f, .45f, .14f, Rose); Bake(root.transform, "prism-projectile"); root.AddComponent<ProjectileActor>();
            return SavePrefab(root, "Projectiles", "prism");
        }
        public static GameObject ExpansionEffect(string id)
        {
            var root = Root(id); var t = Root("Model", root.transform).transform;
            for (int i = 0; i < 12; i++)
            {
                Vector3 p = new Vector3(Mathf.Sin(i * 2.4f) * (.25f + i % 3 * .17f), .1f + i % 4 * .15f, Mathf.Cos(i * 2.4f) * (.25f + i % 3 * .17f));
                if (id == "spore") Sphere(t, "SporeMote", p, Vector3.one * (.09f + i % 3 * .025f), i % 2 == 0 ? Hex("D7E8A7") : Violet);
                else Crystal(t, p, .2f + i % 3 * .07f, .07f, i % 2 == 0 ? Rose : Hex("F0C9C0"), i * 13);
            }
            Ring(t, "DissolveWave", .5f, .02f, id == "spore" ? Moss : Rose); Bake(t, "fx-" + id);
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>()) { r.sharedMaterial = EffectMaterial(); r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false; }
            root.AddComponent<TransientEffect>(); return SavePrefab(root, "VFX", id);
        }
    }
}
