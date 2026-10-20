using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        public static readonly Color[] GroundColors = { Hex("78A75A"), Hex("D9B371"), Hex("B9D6DD"), Hex("655A66") };
        public static readonly Color[] AccentColors = { Hex("438C76"), Hex("D0934E"), Hex("62ABC7"), Hex("E17D4E") };
        public static GameObject Terrain(int biome,int variant=0)
        {
            string id = new[] { "forest", "desert", "snow", "volcano" }[biome]+(variant==0?"":variant==1?"-wild":"-ridge");
            var root = Root("Terrain-" + id); var t = Root("Surface", root.transform).transform;
            Cube(t, "Ground", new Vector3(0, -.2f, 0), new Vector3(32, .4f, 32), GroundColors[biome]);
            var random = new System.Random(921 + biome+variant*171);
            for (int i = 0; i < 24; i++)
            {
                Vector3 p = new Vector3((float)random.NextDouble() * 31 - 15.5f, .015f, (float)random.NextDouble() * 31 - 15.5f);
                float scale = .3f + (float)random.NextDouble() * 1.6f;
                Color c = Color.Lerp(GroundColors[biome], biome == 2 ? Color.white : Cream, .12f + (float)random.NextDouble() * .14f);
                Part(t, "GroundPatch", PrimitiveType.Cylinder, p, new Vector3(scale, .003f, scale * .7f), c);
            }
            TerrainFeatures(t, biome, random,variant);
            if (biome == 0 || biome == 2)
            {
                for (int i = 0; i < 95; i++)
                {
                    Vector3 p = new Vector3((float)random.NextDouble() * 31 - 15.5f, .01f, (float)random.NextDouble() * 31 - 15.5f);
                    Color c = biome == 0 ? Color.Lerp(Teal, GroundColors[biome], .5f) : Hex("BDD6DF");
                    for (int j = 0; j < 3; j++) Spike(t, "Tuft", p + Vector3.right * j * .05f, p + new Vector3((j - 1) * .1f, .16f + j * .04f, .03f), .025f, c);
                    if (i % 6 == 0) Sphere(t, "Flower", p + Vector3.up * .16f, new Vector3(.13f, .07f, .13f), i % 2 == 0 ? Cream : Hex("F3C281"));
                }
            }
            Bake(t, "terrain-" + id);
            var collider = root.AddComponent<BoxCollider>(); collider.center = Vector3.down * .2f; collider.size = new Vector3(32, .4f, 32);
            if(variant==0&&(biome==0||biome==3)){var bridge=Root("BridgeCollider",root.transform);var bridgeCollider=bridge.AddComponent<BoxCollider>();bridgeCollider.center=new Vector3(0,.14f,8);bridgeCollider.size=new Vector3(3.3f,.28f,3.9f);}
            return SavePrefab(root, "World/Terrain", id);
        }
        public static GameObject Decoration(int biome)
        {
            string id = new[] { "forest", "desert", "snow", "volcano" }[biome];
            var root = Root("Decoration-" + id); root.AddComponent<OcclusionFader>();var variants = root.AddComponent<DecorationVariant>(); variants.variants = new Transform[3];
            for (int variant = 0; variant < 3; variant++)
            {
                var t = Root("Variant" + variant, root.transform).transform; variants.variants[variant] = t;
                var geometry = Root("Geometry", t).transform;
                if (variant == 0)
                {
                    Capsule(geometry, "Trunk", new Vector3(0, 1.05f, 0), new Vector3(.55f, 1.1f, .55f), Brown);
                    if (biome == 0)
                    {
                        Sphere(geometry, "Canopy", new Vector3(0, 2.7f, 0), new Vector3(2.5f, 2.25f, 2.25f), Hex("73AD79"));
                        Sphere(geometry, "Crown", new Vector3(.55f, 3.2f, -.1f), new Vector3(1.8f, 1.8f, 1.8f), Hex("A1CD7D"));
                        Sphere(geometry, "SideCrown", new Vector3(-.7f, 2.6f, .25f), new Vector3(1.7f, 1.65f, 1.7f), Hex("6C9E70"));
                        for (int i = 0; i < 4; i++) Sphere(geometry, "Fruit", new Vector3(Mathf.Sin(i * 2) * .8f, 2.5f + Mathf.Cos(i) * .3f, .9f), Vector3.one * .17f, Orange);
                    }
                    else if (biome == 1)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            float a = i * Mathf.PI * 2 / 7;
                            Spike(geometry, "PalmLeaf", new Vector3(0, 2.25f, 0), new Vector3(Mathf.Sin(a) * 1.8f, 2.3f + Mathf.Cos(i) * .3f, Mathf.Cos(a) * 1.8f), .3f, i % 2 == 0 ? Hex("74B39B") : Teal);
                        }
                        Sphere(geometry, "Coconuts", new Vector3(.1f, 2.1f, .2f), Vector3.one * .45f, Gold);
                    }
                    else if (biome == 2)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Spike(geometry, "Pine", new Vector3(0, 1 + i * .6f, 0), new Vector3(0, 2.7f + i * .55f, 0), 1.2f - i * .23f, Hex("82B5B7"));
                            Spike(geometry, "Snow", new Vector3(0, 1.3f + i * .6f, 0), new Vector3(0, 2.85f + i * .55f, 0), 1.08f - i * .23f, Hex("F3F4E9"));
                        }
                    }
                    else
                    {
                        Object.DestroyImmediate(geometry.Find("Trunk").gameObject);
                        for (int i = 0; i < 5; i++)
                        {
                            float h=1.3f+i*.43f; Vector3 p=new Vector3(Mathf.Sin(i*2.4f)*.6f,h*.5f,Mathf.Cos(i*2.4f)*.5f);
                            Part(geometry,"BasaltColumn",PrimitiveType.Cylinder,p,new Vector3(.68f,h*.5f,.68f),i%2==0?Hex("3F485A"):Hex("53566A"));
                            Part(geometry,"CoolingRim",PrimitiveType.Cylinder,p+Vector3.up*h*.48f,new Vector3(.69f,.035f,.69f),Hex("A36D5E"));
                        }
                    }
                    var obstacle = Root("TrunkCollider", t); obstacle.layer = 10;
                    var c = obstacle.AddComponent<CapsuleCollider>(); c.radius = .3f; c.height = 2; c.center = Vector3.up;
                }
                else if (variant == 1)
                {
                    Color stone = biome == 0 ? Hex("91A69B") : biome == 1 ? Hex("C39D78") : biome == 2 ? Hex("A9CED9") : Hex("84848B");
                    for (int i = 0; i < 4; i++) Sphere(geometry, "Stone", new Vector3(Mathf.Sin(i * 2.4f) * .7f, .3f + i * .08f, Mathf.Cos(i * 2.4f) * .5f), new Vector3(.9f, .6f + i * .12f, .8f), Color.Lerp(stone, Cream, i * .08f));
                    if (biome == 2 || biome == 3) for (int i = 0; i < 3; i++) Spike(geometry, "Crystal", new Vector3((i - 1) * .3f, .35f, .2f), new Vector3((i - 1) * .4f, 1.3f + i * .2f, .1f), .18f, biome == 2 ? Hex("8BD5E0") : Orange);
                    var rockObstacle=Root("RockCollider",t);rockObstacle.layer=10;var rockCollider=rockObstacle.AddComponent<CapsuleCollider>();rockCollider.center=new Vector3(0,.55f,0);rockCollider.radius=.75f;rockCollider.height=1.1f;
                }
                else
                {
                    for (int i = 0; i < 7; i++)
                    {
                        Vector3 p = new Vector3(Mathf.Sin(i * 2.4f) * (.25f+i*.09f), 0, Mathf.Cos(i * 2.4f) * (.2f+i*.07f));
                        if (biome == 1)
                        {
                            Capsule(geometry, "Cactus", p + Vector3.up * .5f, new Vector3(.3f, .5f + i * .035f, .3f), Hex("80AD8B"));
                            Sphere(geometry, "Flower", p + Vector3.up * (1.05f + i * .07f), new Vector3(.17f, .12f, .17f), Hex("F0A6A2"));
                        }
                        else
                        {
                            if(biome==3) { Spike(geometry,"EmberCrystal",p,p+new Vector3(.08f,.5f+i*.06f,0),.15f,i%2==0?Hex("FFB33E"):Hex("DD5639")); }
                            else {
                                Capsule(geometry, "Stem", p + Vector3.up * .16f, new Vector3(.07f, .15f, .07f), biome == 2 ? Teal : Hex("A7A478"));
                                Sphere(geometry, "Mushroom", p + Vector3.up * .35f, new Vector3(.39f, .19f, .35f), biome == 0 ? i % 2 == 0 ? Hex("E8A47E") : Cream : Hex("ADCAE0"));
                                Sphere(geometry, "Spot", p + new Vector3(.06f, .44f, .02f), Vector3.one * .07f, Cream);
                            }
                        }
                    }
                }
                Bake(geometry, "deco-" + id + "-" + variant);
                Blob(t,variant==0?1.2f:variant==1?1:.7f);
            }
            return SavePrefab(root, "World/Decoration", id);
        }
        public static GameObject Site(int type)
        {
            string id = new[] { "chest", "camp", "shrine", "boss-site" }[type]; var root = Root(id); var t = Root("Model", root.transform).transform;
            if (type == 0)
            {
                Cube(t, "Chest", new Vector3(0, .4f, 0), new Vector3(1.3f, .8f, .85f), Brown);
                Sphere(t, "Lid", new Vector3(0, .81f, 0), new Vector3(1.35f, .45f, .88f), Hex("B17D4C"));
                for (int s = -1; s <= 1; s += 2) Cube(t, "Band", new Vector3(s * .43f, .45f, .44f), new Vector3(.12f, .8f, .045f), Gold);
                Cube(t, "Lock", new Vector3(0, .65f, .47f), new Vector3(.26f, .29f, .07f), Gold);
            }
            else if (type == 1)
            {
                for (int i = 0; i < 7; i++) Sphere(t, "Stone", new Vector3(Mathf.Sin(i) * .7f, .14f, Mathf.Cos(i) * .7f), new Vector3(.45f, .28f, .4f), Hex("9DA89B"));
                for (int i = 0; i < 4; i++) Spike(t, "Fire", new Vector3(Mathf.Sin(i * 2) * .2f, .2f, Mathf.Cos(i * 2) * .2f), new Vector3(0, 1.3f + i * .1f, 0), .26f, i % 2 == 0 ? Orange : Gold);
                Tube(t, "FlagPole", new Vector3(1, 0, 0), new Vector3(1, 2.6f, 0), .055f, Brown);
                Cube(t, "Flag", new Vector3(1.4f, 2.2f, 0), new Vector3(.85f, .55f, .055f), Teal);
            }
            else if (type == 2)
            {
                Part(t, "Base", PrimitiveType.Cylinder, new Vector3(0, .18f, 0), new Vector3(2, .18f, 2), Hex("C5BB9D"));
                var ring = Ring(t, "Ring", .72f, .08f, Gold); ring.transform.localPosition = Vector3.up * .9f;
                Sphere(t, "Crystal", new Vector3(0, 1.2f, 0), new Vector3(.6f, 1, .6f), Hex("9BDFD0"));
                for (int i = 0; i < 4; i++) Sphere(t, "Mote", new Vector3(Mathf.Sin(i * 1.57f) * .9f, 1.5f, Mathf.Cos(i * 1.57f) * .9f), Vector3.one * .16f, Cream);
            }
            else
            {
                Part(t, "Platform", PrimitiveType.Cylinder, new Vector3(0, .10f, 0), new Vector3(5, .1f, 5), Hex("B1BBA8"));
                var seal = Ring(t, "Seal", 1.9f, .075f, Gold); seal.transform.localPosition = Vector3.up * .25f;
                for (int i = 0; i < 5; i++)
                {
                    Vector3 p = new Vector3(Mathf.Sin(i * Mathf.PI * 2 / 5) * 2.4f, 0, Mathf.Cos(i * Mathf.PI * 2 / 5) * 2.4f);
                    Sphere(t, "StandingStone", p + Vector3.up * 1, new Vector3(.6f, 2, .6f), Hex("8A9F96"));
                    Sphere(t, "RuneLight", p + Vector3.up * 1.8f, Vector3.one * .27f, Hex("B5E9CB"));
                }
            }
            Bake(t, "site-" + id);Blob(root.transform,type==3?2.5f:1.2f); return SavePrefab(root, "World/Landmarks", id);
        }
        public static GameObject Effect(string id)
        {
            var root = Root(id); var t = Root("Model", root.transform).transform;
            Color c = id == "frost" ? Hex("A6E4EE") : id == "rune" || id == "dodge" ? Hex("8EDBC6") : id == "lightning" ? Hex("F9DF83") : id == "mark" ? Hex("FFD06A") : Orange;
            if (id == "warning"||id=="warning-line")
            {
                if(id=="warning")
                {
                    Ring(t, "Outer", .5f, .022f, Hex("7D2048")); Ring(t, "Inner", .46f, .014f, Hex("FF5472"));
                    for(int i=0;i<8;i++) { float a=i*Mathf.PI/4; Vector3 p=new Vector3(Mathf.Sin(a)*.50f,.025f,Mathf.Cos(a)*.50f); Spike(t,"DangerChevron",p,new Vector3(Mathf.Sin(a)*.40f,.025f,Mathf.Cos(a)*.40f),.033f,Hex("E53562")); }
                }
                else for(int s=-1;s<=1;s+=2){Cube(t,"LaneEdge",new Vector3(s*.49f,.01f,0),new Vector3(.025f,.012f,1),Hex("A52A54"));Cube(t,"LaneEnd",new Vector3(0,.01f,s*.49f),new Vector3(1,.012f,.025f),Hex("A52A54"));}
                Bake(t, "fx-"+id+"-rim");
                var hazard = root.AddComponent<AttackHazard>(); hazard.fill = Root("Fill", root.transform).transform;
                // Sparse progress strokes preserve the ground and silhouettes under overlapping attacks.
                for(int i=-2;i<=2;i++) Cube(hazard.fill,"Hatch",new Vector3(i*.16f,.02f,0),new Vector3(.014f,.005f,Mathf.Sqrt(.18f-i*i*.0256f)*2),Hex("D6426B"),new Vector3(0,25,0));
                Tube(hazard.fill,"Exclamation",new Vector3(0,.03f,-.10f),new Vector3(0,.03f,.13f),.028f,Cream);Sphere(hazard.fill,"Point",new Vector3(0,.04f,-.21f),Vector3.one*.065f,Cream);
                Bake(hazard.fill, "fx-"+id+"-fill");
            }
            else
            {
                EffectGeometry(t,id,c);
                Bake(t, "fx-" + id); if (id != "mark"&&id!="player") root.AddComponent<TransientEffect>();
            }
            foreach(var renderer in root.GetComponentsInChildren<Renderer>()){renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;}
            return SavePrefab(root, "VFX", id);
        }
        public static GameObject Experience()
        {
            var root = Root("experience"); Spike(root.transform, "Gem", Vector3.zero, Vector3.up * .45f, .19f, Hex("8EE4DD"));
            Spike(root.transform, "Bottom", Vector3.zero, Vector3.down * .22f, .19f, Teal);
            Bake(root.transform, "experience-orb"); root.AddComponent<ExperienceOrb>(); return SavePrefab(root, "Pickups", "experience");
        }
    }
}
