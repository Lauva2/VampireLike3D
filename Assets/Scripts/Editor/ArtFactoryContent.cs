using UnityEngine;

namespace VampireLike.Editor
{
    public static partial class ArtFactory
    {
        static void Eyes(Transform parent, Vector3 center, float size, Color iris)
        {
            for (int sign = -1; sign <= 1; sign += 2)
            {
                Vector3 eye = center + Vector3.right * sign * size * .26f;
                Sphere(parent, "Eye", eye, new Vector3(.22f, .29f, .08f) * size, Cream);
                Sphere(parent, "Iris", eye + Vector3.forward * size * .045f, new Vector3(.12f, .18f, .035f) * size, iris);
                Sphere(parent, "Shine", eye + new Vector3(-.025f, .06f, .068f) * size, Vector3.one * size * .045f, Color.white);
            }
        }
        public static GameObject Enemy(int type, bool boss = false)
        {
            string id = boss ? new[] { "forest", "desert", "snow", "volcano" }[type] : type.ToString();
            var root = Root(boss ? "Boss-" + id : "Enemy-" + id); root.layer = 8;
            var actor = root.AddComponent<EnemyActor>();
            var collider = root.AddComponent<SphereCollider>(); collider.isTrigger = true; collider.radius = boss ? 1.7f : .65f; collider.center = Vector3.up * (boss ? 1.7f : .7f);
            actor.visual = Root("Visual", root.transform).transform;
            var t = actor.visual;
            if (!boss)
            {
                if (type == 0)
                {
                    Sphere(t, "Jelly", new Vector3(0, .48f, 0), new Vector3(1.25f, .97f, 1.1f), Hex("9CCF76"));
                    Sphere(t, "JellyGlow", new Vector3(-.26f, .75f, -.22f), new Vector3(.34f, .18f, .4f), Hex("C5E897"));
                    Eyes(t, new Vector3(0, .55f, .48f), 1, Ink);
                    Spike(t, "Leaf", new Vector3(0, .92f, 0), new Vector3(.42f, 1.32f, .07f), .2f, Teal);
                    Sphere(t, "Smile", new Vector3(0, .29f, .49f), new Vector3(.14f, .06f, .04f), Brown);
                }
                if (type == 1)
                {
                    Sphere(t, "Body", new Vector3(0, .55f, 0), new Vector3(.8f, .83f, .9f), Hex("F4B36F"));
                    Sphere(t, "Muzzle", new Vector3(0, .42f, .4f), new Vector3(.52f, .35f, .35f), Cream);
                    for (int s = -1; s <= 1; s += 2) { Spike(t, "Ear", new Vector3(s * .23f, .85f, 0), new Vector3(s * .34f, 1.49f, -.05f), .2f, Orange); Sphere(t, "Foot", new Vector3(s * .25f, .15f, .1f), new Vector3(.3f, .25f, .48f), Brown); }
                    Eyes(t, new Vector3(0, .66f, .4f), .85f, Brown);
                }
                if (type == 2)
                {
                    Sphere(t, "Shell", new Vector3(0, .65f, -.1f), new Vector3(1.2f, .93f, 1.4f), Hex("6FA6C4"));
                    Sphere(t, "Face", new Vector3(0, .48f, .49f), new Vector3(.83f, .63f, .5f), Hex("C1DCEC"));
                    Eyes(t, new Vector3(0, .57f, .71f), .85f, Ink);
                    Spike(t, "Horn", new Vector3(0, .74f, .61f), new Vector3(0, 1.3f, .96f), .2f, Gold);
                    for (int s = -1; s <= 1; s += 2) for (int i = 0; i < 3; i++) Capsule(t, "Leg", new Vector3(s * .52f, .19f, (i - 1) * .4f), new Vector3(.26f, .14f, .38f), Ink, new Vector3(0, 0, s * -25));
                }
                if (type == 3)
                {
                    Capsule(t, "Stem", new Vector3(0, .43f, 0), new Vector3(.55f, .34f, .5f), Cream);
                    Sphere(t, "Cap", new Vector3(0, .98f, 0), new Vector3(1.4f, .65f, 1.18f), Hex("A28AC8"));
                    for (int i = 0; i < 5; i++) Sphere(t, "CapDot", new Vector3(Mathf.Sin(i * 2.4f) * .4f, 1.24f, Mathf.Cos(i * 2.4f) * .3f), new Vector3(.15f, .06f, .16f), Cream);
                    Eyes(t, new Vector3(0, .51f, .25f), .72f, Hex("6D5087"));
                    Tube(t, "Staff", new Vector3(.54f, .1f, 0), new Vector3(.6f, 1.35f, 0), .045f, Brown);
                    Sphere(t, "Orb", new Vector3(.6f, 1.42f, 0), Vector3.one * .24f, Hex("EAB6E9"));
                }
            }
            else if (type == 0)
            {
                Capsule(t, "Trunk", new Vector3(0, 1.65f, 0), new Vector3(2.15f, 1.4f, 1.65f), Brown);
                Sphere(t, "Face", new Vector3(0, 2.25f, .7f), new Vector3(1.55f, 1.4f, .65f), Hex("B88B54"));
                Eyes(t, new Vector3(0, 2.45f, 1.02f), 2.1f, Teal);
                for (int s = -1; s <= 1; s += 2)
                {
                    Tube(t, "Arm", new Vector3(s * .85f, 2.6f, 0), new Vector3(s * 1.9f, 1.1f, .2f), .42f, Brown);
                    for (int j = 0; j < 3; j++) Spike(t, "Root", new Vector3(s * .7f, .35f, 0), new Vector3(s * (1.2f + j * .28f), .08f, .45f - j * .5f), .3f, Brown);
                    Sphere(t, "ShoulderLeaves", new Vector3(s * 1.1f, 3, -.1f), Vector3.one * 1.7f, Hex("71B677"));
                }
                for (int j = 0; j < 5; j++) Sphere(t, "Crown", new Vector3(Mathf.Sin(j * 2.4f) * .8f, 3.6f + Mathf.Cos(j * 2) * .2f, Mathf.Cos(j * 2.4f) * .65f), Vector3.one * (1.7f + j * .05f), j % 2 == 0 ? Hex("8DCA6D") : Hex("5FA772"));
                var rune = Ring(t, "HeartRune", .25f, .06f, Hex("A0E6D8")); rune.transform.localPosition = new Vector3(0, 1.4f, .85f); rune.transform.localEulerAngles = new Vector3(90, 0, 0);
            }
            else if (type == 1)
            {
                SandWorm(t,actor,root.transform);
            }
            else if (type == 2)
            {
                Sphere(t, "FurBody", new Vector3(0, 1.6f, 0), new Vector3(2.5f, 2.6f, 1.8f), Hex("DDEEF1"));
                Sphere(t, "Head", new Vector3(0, 2.9f, .35f), new Vector3(1.9f, 1.65f, 1.7f), Hex("E3EFF2"));
                Eyes(t, new Vector3(0, 3, 1.15f), 2.1f, Hex("3E90AE"));
                Sphere(t, "Muzzle", new Vector3(0, 2.65f, 1.08f), new Vector3(.82f, .46f, .35f), Hex("A4CFDB"));
                for (int s = -1; s <= 1; s += 2)
                {
                    Capsule(t, "Arm", new Vector3(s * 1.25f, 1.6f, .05f), new Vector3(.9f, .85f, .92f), Hex("DDEEF1"), new Vector3(0, 0, s * 15));
                    Sphere(t, "Foot", new Vector3(s * .65f, .33f, .28f), new Vector3(.95f, .7f, 1.1f), Hex("79B6C9"));
                    Spike(t, "IceHorn", new Vector3(s * .68f, 3.45f, .1f), new Vector3(s * 1.0f, 4.3f, -.1f), .25f, Hex("7BCEE3"));
                }
            }
            else
            {
                Color rock = Hex("736D77"), lava = Hex("F6A34E");
                Sphere(t, "Core", new Vector3(0, 1.9f, 0), new Vector3(2, 2.3f, 1.55f), lava);
                for (int j = 0; j < 4; j++) Sphere(t, "RockPlate", new Vector3((j % 2 == 0 ? -.53f : .53f), 1.4f + j / 2 * .83f, .36f), new Vector3(1.05f, 1, 1.1f), rock);
                Sphere(t, "Head", new Vector3(0, 3.12f, .18f), new Vector3(1.7f, 1.3f, 1.4f), rock);
                Eyes(t, new Vector3(0, 3.22f, .83f), 1.7f, lava);
                for (int s = -1; s <= 1; s += 2)
                {
                    Sphere(t, "Arm", new Vector3(s * 1.25f, 1.85f, 0), new Vector3(1.15f, 1.8f, 1.15f), rock);
                    Sphere(t, "Fist", new Vector3(s * 1.4f, .85f, .25f), new Vector3(1.12f, .85f, 1.13f), lava);
                    Sphere(t, "Boot", new Vector3(s * .66f, .36f, .2f), new Vector3(1.05f, .72f, 1.2f), rock);
                    Spike(t, "Crown", new Vector3(s * .52f, 3.6f, -.1f), new Vector3(s * .7f, 4.3f, -.2f), .28f, lava);
                }
            }
            if(boss)
            {
                BossDetails(t,type);
                var arms=new System.Collections.Generic.List<Transform>();foreach(Transform child in t)if(child.name.StartsWith("Arm")||child.name=="Fist")arms.Add(child);
                for(int side=-1;side<=1;side+=2)
                {
                    var pivot=Root(side<0?"ArmRigL":"ArmRigR",root.transform).transform;pivot.localPosition=new Vector3(side*1.05f,2.55f,0);
                    foreach(var part in arms){var renderer=part.GetComponent<Renderer>();float x=renderer!=null?t.InverseTransformPoint(renderer.bounds.center).x:part.localPosition.x;if(Mathf.Sign(x)==side)part.SetParent(pivot,true);}
                    if(pivot.childCount==0){Object.DestroyImmediate(pivot.gameObject);continue;}
                    Bake(pivot,"boss-"+id+"-arm"+side);if(side<0)actor.leftArm=pivot;else actor.rightArm=pivot;
                }
            }
            Bake(t, (boss ? "boss-" : "enemy-") + id);
            if(actor.leftArm!=null)actor.leftArm.SetParent(t,true);if(actor.rightArm!=null)actor.rightArm.SetParent(t,true);
            if(actor.bodySegments!=null)foreach(var segment in actor.bodySegments)segment.SetParent(t,true);
            if(!boss)
            {
                actor.eliteHalo=Root("EliteCrown",root.transform).transform;actor.eliteHalo.localPosition=Vector3.up*1.65f;
                Ring(actor.eliteHalo,"Crown",.48f,.045f,Gold);
                for(int i=0;i<5;i++)Spike(actor.eliteHalo,"CrownTip",new Vector3(Mathf.Sin(i*1.26f)*.45f,0,Mathf.Cos(i*1.26f)*.45f),new Vector3(Mathf.Sin(i*1.26f)*.5f,.27f,Mathf.Cos(i*1.26f)*.5f),.08f,Gold);
                if(type==0)for(int s=-1;s<=1;s+=2)Spike(actor.eliteHalo,"LeafAntler",new Vector3(s*.35f,-.85f,0),new Vector3(s*.9f,-.22f,-.12f),.20f,Hex("EBD778"));
                if(type==1)for(int s=-1;s<=1;s+=2){Rock(actor.eliteHalo,"RunnerPauldron",new Vector3(s*.45f,-.95f,.1f),new Vector3(.38f,.43f,.55f),Gold,s+2);Tube(actor.eliteHalo,"RunnerStripe",new Vector3(s*.22f,-.95f,.43f),new Vector3(s*.30f,-1.15f,.44f),.045f,Ink);}
                if(type==2)for(int i=0;i<5;i++)Spike(actor.eliteHalo,"ArmoredSpine",new Vector3(Mathf.Sin(i*2.4f)*.46f,-.69f,Mathf.Cos(i*2.4f)*.5f),new Vector3(Mathf.Sin(i*2.4f)*.75f,-.15f,Mathf.Cos(i*2.4f)*.65f),.14f,Gold);
                if(type==3)for(int s=-1;s<=1;s+=2){Tube(actor.eliteHalo,"LanternHook",new Vector3(s*.35f,-.42f,0),new Vector3(s*.85f,-.36f,0),.035f,Gold);Sphere(actor.eliteHalo,"Lantern",new Vector3(s*.85f,-.59f,0),new Vector3(.25f,.36f,.25f),Hex("D9B8EA"));}
                Bake(actor.eliteHalo,"enemy-"+id+"-elite");actor.eliteHalo.gameObject.SetActive(false);
            }
            Blob(root.transform,boss?2.2f:.68f);
            actor.barRoot = Root("HealthBar", root.transform).transform; actor.barRoot.localPosition = Vector3.up * (boss ? 4.8f : 1.7f);
            float width = boss ? 2.4f : .95f;
            Cube(actor.barRoot, "Back", Vector3.zero, new Vector3(width, .12f, .035f), Ink);
            actor.barFill = Root("Fill", actor.barRoot).transform;
            Cube(actor.barFill, "Green", new Vector3(0, 0, -.025f), new Vector3(width * .94f, .075f, .025f), Hex("84D3A0"));
            Bake(actor.barFill, (boss ? "boss-" : "enemy-") + id + "-barfill");
            var back = actor.barRoot.Find("Back"); Bake(back, (boss ? "boss-" : "enemy-") + id + "-barback");
            return SavePrefab(root, boss ? "Bosses" : "Enemies", id);
        }
        public static GameObject Weapon(AbilityKind kind, bool projectile = false)
        {
            string id = kind.ToString().ToLowerInvariant(); var root = Root(id);
            var model = Root("Model", root.transform).transform;
            if (kind == AbilityKind.Sword)
            {
                Spike(model, "Blade", new Vector3(0, .18f, 0), new Vector3(0, 1.3f, 0), .15f, Hex("B9E9EA"));
                Tube(model, "Hilt", Vector3.down * .28f, Vector3.up * .18f, .055f, Brown);
                Cube(model, "Guard", new Vector3(0, .18f, 0), new Vector3(.48f, .09f, .12f), Gold);
                Sphere(model, "Gem", new Vector3(0, .19f, .08f), Vector3.one * .12f, Orange);
            }
            else if (kind == AbilityKind.Gun || kind == AbilityKind.Cannon)
            {
                Color metal = kind == AbilityKind.Gun ? Teal : Hex("677C98");
                Capsule(model, "Barrel", new Vector3(0, .10f, .25f), new Vector3(.22f, .3f, .22f), Gold, new Vector3(90, 0, 0));
                Cube(model, "Body", Vector3.zero, new Vector3(.3f, .28f, .48f), metal);
                Capsule(model, "Grip", new Vector3(0, -.23f, -.10f), new Vector3(.15f, .19f, .18f), Brown, new Vector3(-18, 0, 0));
                Sphere(model, "Muzzle", new Vector3(0, .1f, .57f), new Vector3(.2f, .2f, .06f), Ink);
                Cube(model, "Sight", new Vector3(0, .27f, .22f), new Vector3(.07f, .06f, .18f), Orange);
                if (kind == AbilityKind.Cannon) model.localScale = Vector3.one * 1.5f;
            }
            else if (kind == AbilityKind.Fireball)
            {
                Sphere(model, "Ember", Vector3.zero, Vector3.one * .5f, Orange);
                for (int i = 0; i < 4; i++) Spike(model, "Flame", new Vector3(Mathf.Sin(i * 1.7f) * .13f, 0, Mathf.Cos(i * 1.7f) * .13f), new Vector3(0, .65f + i * .08f, 0), .16f, i % 2 == 0 ? Gold : Hex("F27546"));
            }
            else if (kind == AbilityKind.Spirit)
            {
                Sphere(model, "Spirit", Vector3.zero, new Vector3(.6f, .7f, .55f), Hex("9FE5CF"));
                Eyes(model, new Vector3(0, .04f, .25f), .55f, Teal);
                for (int s = -1; s <= 1; s += 2) Spike(model, "Wing", new Vector3(s * .18f, .12f, 0), new Vector3(s * .66f, .4f, -.08f), .18f, Cream);
            }
            else
            {
                var ring = Ring(model, "Seal", .36f, .065f, Gold); ring.transform.localEulerAngles = new Vector3(75, 0, 0);
                Cube(model, "Paper", Vector3.zero, new Vector3(.33f, .56f, .05f), Cream);
                for (int i = 0; i < 3; i++) Cube(model, "Rune", new Vector3(0, .15f - i * .14f, .035f), new Vector3(.19f, .035f, .02f), Orange, new Vector3(0, 0, i % 2 == 0 ? 15 : -15));
            }
            if (projectile)
            {
                if (kind == AbilityKind.Sword) model.localEulerAngles = new Vector3(90, 0, 0);
                if (kind == AbilityKind.Gun || kind == AbilityKind.Cannon)
                {
                    Object.DestroyImmediate(model.gameObject); model = Root("Model", root.transform).transform;
                    Sphere(model, "Shot", Vector3.zero, kind == AbilityKind.Gun ? new Vector3(.10f, .10f, .65f) : Vector3.one * .42f, kind == AbilityKind.Gun ? Gold : Orange);
                }
                root.AddComponent<ProjectileActor>();
            }
            Bake(model, (projectile ? "shot-" : "weapon-") + id);
            return SavePrefab(root, projectile ? "Projectiles" : "Weapons", id);
        }
    }
}
