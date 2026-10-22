using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        static readonly Color Moss = Hex("72A68C"), Violet = Hex("9573AF"), Rose = Hex("C65E80"), Sandstone = Hex("C19B91");

        // Closed faceted crystals have a different silhouette and surface language from ice spikes.
        static void Crystal(Transform t, Vector3 at, float height, float width, Color color, float lean = 0)
        {
            var rig = Root("CrystalGem", t).transform; rig.localPosition = at; rig.localEulerAngles = new Vector3(lean, lean * 3, lean * .5f);
            const int sides = 6;
            var v = new Vector3[sides * 12]; var indices = new int[v.Length];
            int n = 0;
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2 / sides, b = (i + 1) * Mathf.PI * 2 / sides;
                Vector3 p = new Vector3(Mathf.Sin(a) * width, 0, Mathf.Cos(a) * width), q = new Vector3(Mathf.Sin(b) * width, 0, Mathf.Cos(b) * width);
                Vector3 pu = p + Vector3.up * height * .64f, qu = q + Vector3.up * height * .64f;
                foreach (var point in new[] { p, pu, q, q, pu, qu, pu, Vector3.up * height, qu, q, Vector3.zero, p }) { v[n] = point; indices[n] = n++; }
            }
            for (int i = 0; i < indices.Length; i += 3) { int swap = indices[i + 1]; indices[i + 1] = indices[i + 2]; indices[i + 2] = swap; }
            var mesh = new Mesh { name = "Six sided crystal" }; mesh.vertices = v; mesh.triangles = indices; mesh.RecalculateNormals();
            rig.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh; rig.gameObject.AddComponent<MeshRenderer>().sharedMaterial = Mat(color);
        }
        static void Mushroom(Transform t, Vector3 p, float height, Color cap)
        {
            Capsule(t, "Stem", p + Vector3.up * height * .43f, new Vector3(height * .24f, height * .43f, height * .24f), Cream);
            Sphere(t, "Gills", p + Vector3.up * height * .85f, new Vector3(height * .91f, height * .14f, height * .85f), Hex("CEDAAF"));
            Sphere(t, "MushroomCap", p + Vector3.up * height, new Vector3(height * 1.08f, height * .43f, height), cap);
            for (int i = 0; i < 6; i++)
            {
                float a = i * 2.399f;
                Sphere(t, "LuminousDot", p + new Vector3(Mathf.Sin(a) * height * .28f, height * 1.16f, Mathf.Cos(a) * height * .27f), new Vector3(.13f, .04f, .13f) * height, Hex("E8F2BD"));
            }
        }
        public static GameObject ExpansionTerrain(BiomeId biome, int variant)
        {
            bool swamp = biome == BiomeId.Swamp;
            string id = ContentIds.RegionKey(biome) + (variant == 0 ? "" : variant == 1 ? "-wild" : "-ridge");
            var root = Root("Terrain-" + id); var surface = Root("Surface", root.transform).transform;
            Color ground = swamp ? Hex("709A85") : Hex("BA918A");
            Cube(surface, "Ground", new Vector3(0, -.2f, 0), new Vector3(32, .4f, 32), ground);
            var land = Root("Landform", root.transform).transform;
            var appearance = root.AddComponent<TerrainAppearance>(); appearance.landform = land;
            var low = Root("LowDetails", land).transform;
            var random = new System.Random(7831 + variant * 97 + (int)biome);
            for (int i = 0; i < 65; i++)
            {
                float x = (float)random.NextDouble() * 30 - 15, z = (float)random.NextDouble() * 30 - 15;
                if (Mathf.Abs(x) < 3.2f || Mathf.Abs(z) < 3.2f) continue;
                Vector3 p = new Vector3(x, .012f, z);
                Part(low, "GroundFleck", PrimitiveType.Cylinder, p, new Vector3(.6f + i % 3 * .4f, .004f, .45f + i % 4 * .2f), Color.Lerp(ground, Cream, .14f));
                if (swamp && i % 3 == 0) Mushroom(low, p, .26f + i % 4 * .08f, i % 2 == 0 ? Violet : Hex("88BEB1"));
                if (!swamp && i % 3 == 0) Crystal(low, p, .18f + i % 4 * .12f, .1f, i % 2 == 0 ? Rose : Hex("E7C1B3"), i % 13);
            }
            if (swamp)
            {
                Vector3 pond = new Vector3(-8.5f, .018f, 8.5f);
                appearance.waterCenter = new Vector2(pond.x, pond.z); appearance.waterRadii = new Vector2(4.2f, 3.1f);
                Part(low, "PondRim", PrimitiveType.Cylinder, pond - Vector3.up * .01f, new Vector3(9, .02f, 6.8f), Hex("526F6B"));
                Part(low, "ShallowJadeWater", PrimitiveType.Cylinder, pond, new Vector3(8.4f, .012f, 6.2f), Hex("74BCAF"));
                for (int i = 0; i < 8; i++)
                {
                    Vector3 p = pond + new Vector3(Mathf.Sin(i * 2.4f) * 2.7f, .034f, Mathf.Cos(i * 2.4f) * 1.9f);
                    Part(low, "LilyPad", PrimitiveType.Cylinder, p, new Vector3(.55f, .01f, .5f), Hex("A8CD92"));
                    if (i % 3 == 0) Sphere(low, "WaterFlower", p + Vector3.up * .12f, new Vector3(.23f, .15f, .23f), Hex("E9BFCD"));
                }
                // Low broken boardwalks sit outside the four connected road exits.
                for (int i = 0; i < 7; i++) Cube(low, "Boardwalk", new Vector3(7 + i * .65f, .10f, -8), new Vector3(.55f, .13f, 2.1f), i % 2 == 0 ? Hex("AE9B74") : Hex("98896C"), new Vector3(0, i % 2 * 3, 0));
            }
            var bank = Root("ReliefBank", land).transform;
            Vector3 center = variant == 0 ? new Vector3(9, 0, 10) : variant == 1 ? new Vector3(-10, 0, -9) : new Vector3(9, 0, -10);
            for (int i = 0; i < 5; i++)
            {
                Vector3 p = center + new Vector3((i - 2) * 1.25f, .3f + i % 2 * .25f, Mathf.Sin(i * 1.3f));
                GroundRock(bank, "LayeredBank", p, new Vector3(2.6f, .7f + i % 3 * .4f, 2.3f), swamp ? Hex("8CAA89") : Sandstone, i + 31);
                if (swamp) Mushroom(bank, p + Vector3.up * .35f, .7f + i % 3 * .25f, Violet);
                else Crystal(bank, p + Vector3.up * .4f, .9f + i % 3 * .5f, .28f, i % 2 == 0 ? Rose : Hex("E4A59B"), (i - 2) * 8);
            }
            Bake(bank, "terrain-" + id + "-bank"); bank.gameObject.AddComponent<MeshCollider>().sharedMesh = bank.GetComponent<MeshFilter>().sharedMesh; bank.gameObject.AddComponent<OcclusionFader>();
            Bake(low, "terrain-" + id + "-details"); Bake(surface, "terrain-" + id);
            var collider = root.AddComponent<BoxCollider>(); collider.center = Vector3.down * .2f; collider.size = new Vector3(32, .4f, 32);
            return SavePrefab(root, "World/Terrain", id);
        }
        public static GameObject ExpansionDecoration(BiomeId biome)
        {
            bool swamp = biome == BiomeId.Swamp; string id = ContentIds.RegionKey(biome);
            var root = Root("Decoration-" + id); root.AddComponent<OcclusionFader>();
            var variants = root.AddComponent<DecorationVariant>(); variants.variants = new Transform[3];
            for (int j = 0; j < 3; j++)
            {
                var t = Root("Variant" + j, root.transform).transform; variants.variants[j] = t;
                var g = Root("Geometry", t).transform;
                if (swamp)
                {
                    if (j == 0) { Mushroom(g, Vector3.zero, 2.7f, Violet); Mushroom(g, new Vector3(.7f, 0, .5f), 1.35f, Hex("7CB9AE")); }
                    if (j == 1)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Tube(g, "RootArch", new Vector3((i - 2) * .36f, .05f, -.55f), new Vector3((i - 2) * .28f, 1.1f + i * .2f, .1f), .14f, Hex("687D65"));
                            Mushroom(g, new Vector3((i - 2) * .32f, .65f, .15f), .6f, Violet);
                        }
                    }
                    if (j == 2) for (int i = 0; i < 7; i++)
                    {
                        Vector3 p = new Vector3(Mathf.Sin(i * 2.4f) * .65f, 0, Mathf.Cos(i * 2.4f) * .5f);
                        Spike(g, "ReedLeaf", p, p + new Vector3(.16f, .65f + i % 3 * .2f, .1f), .09f, Moss);
                        Sphere(g, "SporeBulb", p + Vector3.up * (.6f + i % 3 * .2f), new Vector3(.15f, .18f, .15f), Hex("D4E7A1"));
                    }
                }
                else
                {
                    for (int i = 0; i < (j == 0 ? 5 : j == 1 ? 3 : 7); i++)
                    {
                        Vector3 p = new Vector3(Mathf.Sin(i * 2.4f) * .6f, 0, Mathf.Cos(i * 2.4f) * .5f);
                        if (j == 1) GroundRock(g, "ShaleLayer", p + Vector3.up * (.3f + i * .23f), new Vector3(1.7f - i * .2f, .5f, 1.3f), Sandstone, i + 4);
                        else Crystal(g, p, j == 0 ? 1.5f + i * .35f : .4f + i % 3 * .17f, j == 0 ? .29f : .13f, i % 2 == 0 ? Rose : Hex("E3AFB6"), (i - 2) * 9);
                    }
                }
                if (j < 2)
                {
                    var obstacle = Root("SolidBase", t); obstacle.layer = 10;
                    var collider = obstacle.AddComponent<CapsuleCollider>(); collider.radius = j == 0 ? .55f : .65f; collider.height = j == 0 ? 2.3f : 1.5f; collider.center = Vector3.up * collider.height * .5f;
                }
                Bake(g, "deco-" + id + "-" + j); Blob(t, j == 0 ? 1.1f : .8f);
            }
            return SavePrefab(root, "World/Decoration", id);
        }
        public static GameObject ExpansionLandmark(BiomeId biome)
        {
            bool swamp = biome == BiomeId.Swamp; string id = ContentIds.RegionKey(biome);
            var root = Root("Architecture-" + id); root.AddComponent<OcclusionFader>(); var t = Root("Geometry", root.transform).transform;
            if (swamp)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Tube(t, "Stilt", new Vector3(side * 1.5f, 0, -.9f), new Vector3(side * 1.5f, 2, -.9f), .16f, Brown);
                    Tube(t, "Stilt", new Vector3(side * 1.5f, 0, .9f), new Vector3(side * 1.5f, 2, .9f), .16f, Brown);
                    Mushroom(t, new Vector3(side * 2, 0, -.5f), 1.5f, Violet);
                }
                Cube(t, "RaisedCabin", new Vector3(0, 1.6f, 0), new Vector3(3.2f, 1.6f, 2.3f), Hex("ABBA97"));
                Sphere(t, "CanopyRoof", new Vector3(0, 2.65f, 0), new Vector3(4.2f, 1.1f, 3.6f), Hex("756B97"));
                Cube(t, "Door", new Vector3(0, 1.4f, 1.18f), new Vector3(.85f, 1.18f, .07f), Hex("566B61"));
                for (int s = -1; s <= 1; s += 2) { Cube(t, "WindowFrame", new Vector3(s, 1.8f, 1.19f), new Vector3(.55f, .58f, .08f), Brown); Cube(t, "WindowGlow", new Vector3(s, 1.8f, 1.24f), new Vector3(.41f, .43f, .03f), Hex("E7E5AA")); }
                for (int i = 0; i < 4; i++) Cube(t, "Stair", new Vector3(0, .1f + i * .18f, 2.1f - i * .25f), new Vector3(1.1f, .2f, .45f), Hex("A9A085"));
            }
            else
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    for (int i = 0; i < 4; i++) Cube(t, "CarvedPier", new Vector3(side * 1.8f, .3f + i * .65f, 0), new Vector3(1.05f - i * .07f, .6f, 1.1f), i % 2 == 0 ? Sandstone : Hex("DFC3AD"));
                    Crystal(t, new Vector3(side * 1.8f, 2.6f, 0), 1.15f, .32f, Rose, side * 12);
                    for (int i = 0; i < 3; i++) Cube(t, "GemInlay", new Vector3(side * 1.8f, .8f + i * .48f, .57f), new Vector3(.27f, .18f, .04f), Rose, new Vector3(0, 0, 45));
                }
                Cube(t, "ArchLintel", new Vector3(0, 2.4f, 0), new Vector3(3.8f, .48f, 1), Hex("DEC4AE"));
                Crystal(t, new Vector3(0, 2.65f, 0), .8f, .32f, Hex("E4AAB8"));
                for (int i = 0; i < 5; i++) Cube(t, "AncientPaving", new Vector3(0, .06f, (i - 2) * .65f), new Vector3(2.4f, .1f, .57f), i % 2 == 0 ? Hex("DDC5B0") : Sandstone);
            }
            // Side columns collide; the visible approach and Crystal arch remain walkable.
            foreach (int side in new[] { -1, 1 })
            {
                var o = Root("BuildingCollider", root.transform); o.layer = 10;
                o.transform.localPosition = new Vector3(side * (swamp ? .9f : 1.8f), swamp ? 1.3f : 1.35f, swamp ? -.15f : 0);
                var c = o.AddComponent<BoxCollider>(); c.size = swamp ? new Vector3(1.4f, 2.2f, 2.3f) : new Vector3(1, 2.7f, 1.1f);
            }
            Bake(t, "architecture-" + id); Blob(root.transform, 2.1f); return SavePrefab(root, "World/Architecture", id);
        }
        // Extend the existing authored prefab in place, keeping all original theme GUIDs and geometry.
        public static GameObject ExpansionSanctuary(GameObject original)
        {
            var root = Object.Instantiate(original); root.name = "boss-site";
            var appearance = root.GetComponent<BossSiteAppearance>();
            System.Array.Resize(ref appearance.themes, 6); System.Array.Resize(ref appearance.invocationSeals, 6);
            for (int region = 4; region < 6; region++)
            {
                bool swamp = region == 4; var old = root.transform.Find("Sanctuary" + region); if (old != null) Object.DestroyImmediate(old.gameObject);
                var theme = Root("Sanctuary" + region, root.transform); appearance.themes[region] = theme;
                var t = Root("Geometry", theme.transform).transform;
                var seal = Root("Invocation", theme.transform); appearance.invocationSeals[region] = seal;
                Ring(t, "SpentGroove", 2.45f, .035f, swamp ? Hex("A4BD92") : Sandstone);
                Ring(seal.transform, "InvocationSeal", 2.45f, .045f, swamp ? Hex("D6E5A2") : Hex("F0C2C0"));
                var anchors = new[] { new Vector3(-5, 0, 3), new Vector3(-3.2f, 0, 4.8f), new Vector3(0, 0, 5), new Vector3(3, 0, 4.6f) };
                for (int i = 0; i < anchors.Length; i++)
                {
                    Vector3 p = anchors[i]; float h = 2.8f - i * .35f;
                    if (swamp)
                    {
                        Mushroom(t, p, h, i % 2 == 0 ? Violet : Hex("82B5A6"));
                        for (int j = 0; j < 3; j++) Tube(t, "TwinedRoot", p + Vector3.right * (j - 1) * .3f, p + new Vector3((j - 1) * .1f, 1.5f, -.2f), .12f, Moss);
                    }
                    else
                    {
                        GroundRock(t, "RedShaleBase", p + Vector3.up * .3f, new Vector3(1.7f, .6f, 1.5f), Sandstone, i + 14);
                        Crystal(t, p + Vector3.up * .5f, h, .45f, i % 2 == 0 ? Rose : Hex("E6B6B9"), i * 7 - 12);
                    }
                    var obstacle = Root("SanctuaryCollider", theme.transform); obstacle.layer = 10; obstacle.transform.localPosition = p;
                    var c = obstacle.AddComponent<CapsuleCollider>(); c.radius = .45f; c.height = h; c.center = Vector3.up * h * .5f;
                }
                Bake(t, "sanctuary-" + region); Bake(seal.transform, "sanctuary-invocation-" + region); theme.SetActive(false);
            }
            return SavePrefab(root, "World/Landmarks", "boss-site");
        }
    }
}
