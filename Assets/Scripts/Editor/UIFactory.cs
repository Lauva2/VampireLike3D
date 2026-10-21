using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike.Editor
{
    public static class UIFactory
    {
        static Font font, boldFont;
        static Sprite rounded, circle;
        static readonly Color Ink = ArtFactory.Hex("233E46"), Muted = ArtFactory.Hex("435B56"), Paper = ArtFactory.Hex("FBF7E9"), Mint = ArtFactory.Hex("237C70"), Sand = ArtFactory.Hex("EEE5CF");
        public static GameObject Create()
        {
            font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/UI/Fonts/NotoSansCJKsc-Regular.otf");
            boldFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/UI/Fonts/NotoSansCJKsc-Bold.otf");
            if (font == null) throw new System.InvalidOperationException("Chinese font missing");
            rounded = Sprite("Rounded", 12); circle = Sprite("Circle", 32);
            var root = new GameObject("UIRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(UIRefs));
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 20;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = .5f;
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var r = root.GetComponent<UIRefs>();
            r.menu = Panel(root.transform, "Menu", new Color(0, 0, 0, 0));
            var menuCard = Box(r.menu.transform, "MenuCard", new Vector2(-500, 0), new Vector2(480, 760), new Color(.985f, .97f, .91f, .96f));
            Label(menuCard, "MenuEyebrow", "THE INFINITE EXPEDITION", new Vector2(0, 300), new Vector2(390, 30), 17, Mint);
            Label(menuCard, "MenuTitle", "万象\n远征", new Vector2(0, 164), new Vector2(390, 240), 83, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(menuCard, "MenuTagline", "把万般力量，织成你的旅途。", new Vector2(0, 15), new Vector2(390, 42), 23, Ink);
            Label(menuCard, "MenuDescription", "魔法、枪械与飞剑自由交织。\n穿越无限群系，发现属于你的组合。", new Vector2(0, -50), new Vector2(390, 66), 18, Muted);
            Button(menuCard, "PlayButton", "启程  →", new Vector2(0, -145), new Vector2(390, 66), Mint, Color.white, 26);
            Button(menuCard, "ProgressButton", "旅途手册 · 永久成长", new Vector2(0, -225), new Vector2(390, 54), Sand, Ink);
            Button(menuCard, "SettingsButton", "设置", new Vector2(-101, -295), new Vector2(187, 48), Sand, Ink);
            Button(menuCard, "QuitButton", "退出", new Vector2(101, -295), new Vector2(187, 48), Sand, Ink);
            Label(r.menu.transform, "MenuCrystalText", "旅途晶石  0", new Vector2(580, 375), new Vector2(300, 44), 21, Ink, TextAnchor.MiddleRight);
            Label(r.menu.transform, "MenuFooter", "自动战斗  /  自由构筑  /  无限探索", new Vector2(430, -385), new Vector2(600, 35), 19, Ink, TextAnchor.MiddleRight);

            r.selection = Screen(root.transform, "Selection", "准备远征", "选择同行者与起始群系。所有角色都能自由学习全部体系。", "SelectionClose");
            Label(r.selection.transform, "SelectionCrystalText", "", new Vector2(440, 282), new Vector2(320, 38), 20, Mint, TextAnchor.MiddleRight);
            Label(r.selection.transform, "CharactersHeading", "同行者", new Vector2(-460, 230), new Vector2(300, 36), 24, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(r.selection.transform, "BiomesHeading", "世界入口", new Vector2(180, 230), new Vector2(650, 36), 24, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            r.characterList = Scroll(r.selection.transform, "Characters", new Vector2(-460, -35), new Vector2(350, 470), 1, new Vector2(332, 145));
            r.biomeList = Scroll(r.selection.transform, "Biomes", new Vector2(180, -35), new Vector2(850, 470), 3, new Vector2(265, 215));
            Label(r.selection.transform, "SelectionDescription", "", new Vector2(-180, -310), new Vector2(900, 48), 18, Muted);
            Button(r.selection.transform, "StartButton", "踏入世界  →", new Vector2(480, -315), new Vector2(280, 60), Mint, Color.white, 23);

            r.hud = Panel(root.transform, "HUD", Color.clear);
            var health = Box(r.hud.transform, "HealthCard", new Vector2(-570, 365), new Vector2(340, 105), Paper);
            Label(health, "LevelText", "Lv. 1", new Vector2(-106, 23), new Vector2(90, 34), 23, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(health, "HealthCaption", "生命", new Vector2(27, 24), new Vector2(150, 28), 16, Muted);
            r.healthFill = Bar(health, "HealthBar", new Vector2(0, -12), new Vector2(292, 17), Mint);
            Label(health, "HealthText", "100 / 100", new Vector2(0, -37), new Vector2(292, 23), 16, Muted);
            r.experienceFill = Bar(r.hud.transform, "ExperienceBar", new Vector2(0, 443), new Vector2(1600, 9), ArtFactory.Hex("E6B85E"));
            var timer = Box(r.hud.transform, "TimerCard", new Vector2(0, 397), new Vector2(348, 106), Paper);
            Label(timer, "TimerText", "00:00", new Vector2(0,20), new Vector2(150, 48), 31, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            Label(timer, "BiomeText", "林地 · 无限探索", new Vector2(0, -26), new Vector2(320, 30), 18, Ink, TextAnchor.MiddleCenter);
            Box(r.hud.transform,"CurrencyPlaque",new Vector2(490,391),new Vector2(230,38),Paper);
            Label(r.hud.transform, "RunCurrencyText", "", new Vector2(400, 391), new Vector2(400, 34), 19, Ink, TextAnchor.MiddleRight);
            Box(r.hud.transform,"KillsPlaque",new Vector2(-650,282),new Vector2(170,34),Paper);
            Label(r.hud.transform, "KillsText", "", new Vector2(-570, 282), new Vector2(330, 25), 17, Ink);
            var miniBack = Box(r.hud.transform, "MiniMapFrame", new Vector2(665, 287), new Vector2(174, 174), Paper);
            r.miniMap = Map(miniBack, "MiniMap", Vector2.zero, new Vector2(150, 150), 2);
            Box(r.hud.transform,"MarkPlaque",new Vector2(580,160),new Vector2(320,36),Paper);
            Label(r.hud.transform, "MarkText", "右键敌人 · 标记集火", new Vector2(510, 160), new Vector2(450, 32), 16, Ink, TextAnchor.MiddleRight);
            var bottom = Box(r.hud.transform, "AbilityBar", new Vector2(-5, -380), new Vector2(720, 66), Paper);
            var abilityScroll = Rect(bottom, "Abilities", Vector2.zero, new Vector2(700,60));
            var abilityScroller=abilityScroll.gameObject.AddComponent<ScrollRect>(); abilityScroller.vertical=false; abilityScroller.horizontal=true;
            var abilityViewport=Rect(abilityScroll,"Viewport",Vector2.zero,new Vector2(700,60));abilityViewport.gameObject.AddComponent<RectMask2D>();
            r.abilityList=Rect(abilityViewport,"AbilityList",Vector2.zero,new Vector2(700,60));
            var abilityRT=(RectTransform)r.abilityList;abilityRT.anchorMin=abilityRT.anchorMax=new Vector2(0,.5f);abilityRT.pivot=new Vector2(0,.5f);abilityRT.anchoredPosition=Vector2.zero;
            var abilityLayout=r.abilityList.gameObject.AddComponent<HorizontalLayoutGroup>();abilityLayout.childControlWidth=false;abilityLayout.childControlHeight=false;abilityLayout.childForceExpandWidth=false;abilityLayout.spacing=6;
            r.abilityList.gameObject.AddComponent<ContentSizeFitter>().horizontalFit=ContentSizeFitter.FitMode.PreferredSize;
            abilityScroller.content=abilityRT;abilityScroller.viewport=abilityViewport;
            Label(r.hud.transform, "InteractionText", "", new Vector2(0, -282), new Vector2(900, 60), 22, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            Box(r.hud.transform,"ControlsPlaque",new Vector2(-585,-355),new Vector2(330,64),Paper);
            Label(r.hud.transform, "ControlsText", "WASD 移动   Shift 无敌闪避\n右键标记   E 交互", new Vector2(-575, -355), new Vector2(330, 70), 16, Ink);
            r.dodgeFill = Bar(r.hud.transform, "DodgeBar", new Vector2(-575, -407), new Vector2(310, 8), ArtFactory.Hex("DCAC55"));
            Button(r.hud.transform, "HUDSkills", "Tab  构筑", new Vector2(525, -380), new Vector2(130, 46), Paper, Ink, 17);
            Button(r.hud.transform, "HUDMap", "M  地图", new Vector2(665, -380), new Vector2(130, 46), Paper, Ink, 17);
            Button(r.hud.transform, "HUDPause", "Esc  暂停", new Vector2(665, -320), new Vector2(130, 42), Paper, Ink, 16);
            r.bossBar = Box(r.hud.transform, "BossBar", new Vector2(0, 314), new Vector2(620, 66), Paper).gameObject;
            Label(r.bossBar.transform, "BossNameText", "", new Vector2(0, 14), new Vector2(580, 42), 21, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            r.bossFill = Bar(r.bossBar.transform, "BossHealth", new Vector2(0, -15), new Vector2(570, 12), ArtFactory.Hex("DD8462"));
            Label(r.hud.transform, "RetreatWarningText", "", new Vector2(0, 195), new Vector2(700, 35), 21, ArtFactory.Hex("B85343"), TextAnchor.MiddleCenter);

            r.skills = Screen(root.transform, "Skills", "万象构筑", "所有路线可以叠加 · 获取新能力或继续深入强化", "SkillsClose");
            Label(r.skills.transform, "SkillPointsText", "定向点 0", new Vector2(460, 285), new Vector2(270, 40), 23, Mint, TextAnchor.MiddleRight, FontStyle.Bold);
            string[] filters = { "全部", "元素", "召唤", "弹道", "爆破", "剑修", "符阵", "通用", "联动" };
            for (int i = 0; i < filters.Length; i++) Button(r.skills.transform, "Filter" + filters[i], filters[i], new Vector2(-544 + i * 136, 217), new Vector2(122, 40), Sand, Ink, 18);
            r.skillList = Scroll(r.skills.transform, "SkillCards", new Vector2(0, -57), new Vector2(1240, 470), 3, new Vector2(394, 176));
            Label(r.skills.transform, "SkillsFooter", "滚轮浏览更多 ↓    购买后全部路线仍可叠加    ·    Tab / Esc 返回战斗", new Vector2(0, -320), new Vector2(1200, 32), 17, Muted);

            r.upgrade = Screen(root.transform, "Upgrade", "灵感涌现", "从四种可能中选择一种。其他路线仍可在之后获得。", null);
            Label(r.upgrade.transform, "UpgradeHeading", "选择新的力量", new Vector2(0, 235), new Vector2(1200, 60), 31, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            r.upgradeList = Grid(r.upgrade.transform, "UpgradeCards", new Vector2(0, -5), new Vector2(1220, 345), 4, new Vector2(290, 335));
            Button(r.upgrade.transform, "RerollButton", "重新抽取", new Vector2(-170, -275), new Vector2(280, 54), Sand, Ink, 20, "RerollLabel");
            Button(r.upgrade.transform, "SkipUpgrade", "改领 2 个定向点", new Vector2(170, -275), new Vector2(280, 54), Mint, Color.white, 20);

            r.progress = Screen(root.transform, "Progress", "旅途手册", "每次探索都留下收获。任务会自动记录并发放解锁奖励。", "ProgressClose");
            Label(r.progress.transform, "ProgressCrystalText", "", new Vector2(430, 285), new Vector2(350, 40), 23, Mint, TextAnchor.MiddleRight, FontStyle.Bold);
            Label(r.progress.transform, "MetaHeading", "永久成长", new Vector2(-400, 220), new Vector2(400, 40), 25, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(r.progress.transform, "TaskHeading", "探索与解锁", new Vector2(255, 220), new Vector2(670, 40), 25, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            r.metaList = Scroll(r.progress.transform, "MetaList", new Vector2(-400, -70), new Vector2(400, 520), 1, new Vector2(380, 145));
            r.taskList = Scroll(r.progress.transform, "TaskList", new Vector2(255, -70), new Vector2(700, 520), 1, new Vector2(680, 145));

            r.map = Screen(root.transform, "Map", "探索地图", "白色为你的位置，金色为事件，珊瑚色为 Boss 地标。", "MapClose");
            var mapFrame = Box(r.map.transform, "MapFrame", new Vector2(-165, -35), new Vector2(590, 590), Sand);
            r.largeMap = Map(mapFrame, "WorldMap", Vector2.zero, new Vector2(560, 560), 4);
            Label(r.map.transform, "MapLegend", "世界仍在远方延伸\n\n白色方块 · 你的位置\n金色方块 · 补给宝箱\n金色三角 · 守卫营地\n金色菱形 · 恢复祭坛\n珊瑚菱形 · Boss 挑战\n\n已领取的地标不会重复奖励。\n准备不足时可以自由脱战。", new Vector2(395, 100), new Vector2(350, 300), 19, Ink);
            Label(r.map.transform, "MapCoordinates", "", new Vector2(350, -95), new Vector2(440, 90), 17, Ink);
            Button(r.map.transform, "MapNorth", "↑", new Vector2(370, -190), new Vector2(70, 45), Sand, Ink);
            Button(r.map.transform, "MapWest", "←", new Vector2(285, -245), new Vector2(70, 45), Sand, Ink);
            Button(r.map.transform, "MapCenter", "定位", new Vector2(370, -245), new Vector2(80, 45), Mint, Color.white, 18);
            Button(r.map.transform, "MapEast", "→", new Vector2(455, -245), new Vector2(70, 45), Sand, Ink);
            Button(r.map.transform, "MapSouth", "↓", new Vector2(370, -300), new Vector2(70, 45), Sand, Ink);

            r.pause = Dialog(root.transform, "Pause", "旅途稍歇", "暂停时世界停止。你可以随时携带已有收获撤离。");
            Button(r.pause.transform, "ResumeButton", "继续探索", new Vector2(0, 25), new Vector2(400, 60), Mint, Color.white, 24);
            Button(r.pause.transform, "PauseSettings", "设置", new Vector2(0, -60), new Vector2(400, 52), Sand, Ink);
            Button(r.pause.transform, "RetreatButton", "主动撤离并结算", new Vector2(0, -140), new Vector2(400, 52), Sand, Ink);
            r.settings = Dialog(root.transform, "Settings", "旅途设置", "调节声音与画面，让每次冒险都更舒适。");
            Label(r.settings.transform, "MusicCaption", "音乐", new Vector2(-165, 95), new Vector2(150, 35), 21, Ink);
            r.musicSlider = Slider(r.settings.transform, "MusicSlider", new Vector2(80, 95));
            Label(r.settings.transform, "SoundCaption", "音效", new Vector2(-165, 25), new Vector2(150, 35), 21, Ink);
            r.soundSlider = Slider(r.settings.transform, "SoundSlider", new Vector2(80, 25));
            Button(r.settings.transform, "QualityButton", "画面质量", new Vector2(0, -60), new Vector2(460, 50), Sand, Ink, 20, "QualityLabel");
            Button(r.settings.transform, "FullscreenButton", "显示模式", new Vector2(0, -125), new Vector2(460, 50), Sand, Ink, 20, "FullscreenLabel");
            Button(r.settings.transform, "SettingsClose", "保存并返回", new Vector2(0, -210), new Vector2(460, 54), Mint, Color.white);
            r.victory = Dialog(root.transform, "Victory", "挑战完成", "");
            Label(r.victory.transform, "VictoryName", "", new Vector2(0, 100), new Vector2(620, 54), 27, Mint, TextAnchor.MiddleCenter, FontStyle.Bold);
            Label(r.victory.transform, "VictoryBody", "", new Vector2(0, 0), new Vector2(620, 110), 21, Ink, TextAnchor.MiddleCenter);
            Button(r.victory.transform, "VictoryContinue", "继续无限探索", new Vector2(0, -110), new Vector2(450, 58), Mint, Color.white, 23);
            Button(r.victory.transform, "VictoryRetreat", "携带收获撤离", new Vector2(0, -190), new Vector2(450, 52), Sand, Ink);
            r.results = Dialog(root.transform, "Results", "远征记录", "");
            Label(r.results.transform, "ResultsHeading", "满载而归", new Vector2(0, 130), new Vector2(620, 60), 35, Mint, TextAnchor.MiddleCenter, FontStyle.Bold);
            Label(r.results.transform, "ResultsBody", "", new Vector2(0, -20), new Vector2(550, 230), 21, Ink, TextAnchor.MiddleCenter);
            Button(r.results.transform, "ResultsContinue", "返回营地", new Vector2(0, -215), new Vector2(450, 58), Mint, Color.white, 23);
            r.loading = Panel(root.transform, "Loading", Paper);
            Label(r.loading.transform, "LoadingBrand", "万象远征", new Vector2(0, 110), new Vector2(650, 90), 56, Mint, TextAnchor.MiddleCenter, FontStyle.Bold);
            Label(r.loading.transform, "LoadingText", "正在展开新的世界…", new Vector2(0, 0), new Vector2(700, 55), 26, Ink, TextAnchor.MiddleCenter);
            Label(r.loading.transform, "LoadingProgress", "0%", new Vector2(0, -70), new Vector2(300, 45), 22, Muted, TextAnchor.MiddleCenter);

            var templates = Rect(root.transform, "Templates", Vector2.zero, new Vector2(100, 100));
            r.cardTemplate = WidgetTemplate(templates, "CardTemplate"); r.rowTemplate = WidgetTemplate(templates, "RowTemplate"); templates.gameObject.SetActive(false);
            r.abilityTemplate = AbilityTemplate(templates);
            r.cardTemplate.body.rectTransform.sizeDelta = new Vector2(-40, 59);
            r.rowTemplate.body.rectTransform.anchoredPosition = new Vector2(0, -64); r.rowTemplate.body.rectTransform.sizeDelta = new Vector2(-40, 40);
            r.rowTemplate.body.fontSize=14;
            r.rowTemplate.meta.rectTransform.sizeDelta = new Vector2(-40, 25);
            var toast = Box(root.transform, "Toast", new Vector2(-465, -245), new Vector2(565, 64), new Color(.10f, .20f, .22f, .96f));
            Label(toast, "ToastText", "", Vector2.zero, new Vector2(535, 60), 18, Paper, TextAnchor.MiddleCenter); toast.gameObject.SetActive(false);
            var flash = Panel(root.transform, "DamageFlash", Color.clear); r.damageFlash = flash.GetComponent<Image>(); r.damageFlash.raycastTarget = false;
            r.damageFlash.sprite=DamageVignette();r.damageFlash.type=Image.Type.Simple;
            foreach (var panel in new[] { r.selection, r.hud, r.skills, r.upgrade, r.map, r.progress, r.pause, r.settings, r.victory, r.results, r.loading }) panel.SetActive(false);
            return ArtFactory.SavePrefab(root, "UI", "UIRoot");
        }

        static Sprite DamageVignette()
        {
            const string path="Assets/Art/UI/Generated/DamageVignette.asset";
            foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(path))if(asset is Sprite sprite)return sprite;
            var texture=new Texture2D(128,128,TextureFormat.RGBA32,false){name="DamageVignette",wrapMode=TextureWrapMode.Clamp};
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                float edge=Mathf.Min(Mathf.Min(x,127-x),Mathf.Min(y,127-y))/63.5f;
                float alpha=1-Mathf.SmoothStep(0,1,edge/.18f);texture.SetPixel(x,y,new Color(1,1,1,alpha*alpha));
            }
            texture.Apply();AssetDatabase.CreateAsset(texture,path);
            var result=UnityEngine.Sprite.Create(texture,new Rect(0,0,128,128),Vector2.one*.5f,100);result.name="DamageVignette";AssetDatabase.AddObjectToAsset(result,texture);return result;
        }
        static Sprite Sprite(string name, float radius)
        {
            Directory.CreateDirectory("Assets/Art/UI/Generated"); string path = "Assets/Art/UI/Generated/" + name + ".asset";
            var existing = AssetDatabase.LoadAllAssetsAtPath(path); foreach (var asset in existing) if (asset is Sprite s) return s;
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false); texture.name = name;
            var pixels = new Color[64 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            { float dx = Mathf.Max(Mathf.Abs(x - 31.5f) - (32 - radius), 0), dy = Mathf.Max(Mathf.Abs(y - 31.5f) - (32 - radius), 0); pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy))); }
            texture.SetPixels(pixels); texture.Apply(); texture.wrapMode = TextureWrapMode.Clamp; AssetDatabase.CreateAsset(texture, path);
            var sprite = UnityEngine.Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius)); sprite.name = name;
            AssetDatabase.AddObjectToAsset(sprite, texture); return sprite;
        }
        static RectTransform Rect(Transform parent, string name, Vector2 p, Vector2 size)
        { var go = new GameObject(name, typeof(RectTransform)); var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false); rt.sizeDelta = size; rt.anchoredPosition = p; return rt; }
        static Transform Box(Transform parent, string name, Vector2 p, Vector2 size, Color color)
        { var rt = Rect(parent, name, p, size); var image = rt.gameObject.AddComponent<Image>(); image.sprite = rounded; image.type = Image.Type.Sliced; image.color = color; return rt; }
        static GameObject Panel(Transform parent, string name, Color color)
        {
            var rt = Rect(parent, name, Vector2.zero, Vector2.zero); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            var image = rt.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = color.a > .01f; return rt.gameObject;
        }
        static Text Label(Transform parent, string name, string text, Vector2 p, Vector2 size, int fontSize, Color color, TextAnchor align = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var rt = Rect(parent, name, p, size); var label = rt.gameObject.AddComponent<Text>();
            label.font = style==FontStyle.Bold&&boldFont!=null?boldFont:font; label.text = text; label.fontSize = fontSize; label.fontStyle = FontStyle.Normal; label.color = color; label.alignment = align; label.supportRichText = false; label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate; return label;
        }
        static Button Button(Transform parent, string name, string text, Vector2 p, Vector2 size, Color bg, Color fg, int fontSize = 21, string labelName = null)
        {
            var t = Box(parent, name, p, size, bg); var button = t.gameObject.AddComponent<Button>(); button.targetGraphic = t.GetComponent<Image>();
            var colors = button.colors; colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f); colors.pressedColor = new Color(.85f, .92f, .88f); colors.disabledColor = new Color(.7f, .72f, .7f, .65f); button.colors = colors;
            Label(t, labelName ?? name + "Label", text, Vector2.zero, size - new Vector2(20, 8), fontSize, fg, TextAnchor.MiddleCenter); return button;
        }
        static GameObject Screen(Transform parent, string name, string title, string subtitle, string close)
        {
            var panel = Panel(parent, name, new Color(.13f, .23f, .25f, .48f));
            Box(panel.transform, "Sheet", Vector2.zero, new Vector2(1360, 760), Paper);
            Label(panel.transform, name + "Title", title, new Vector2(-280, 321), new Vector2(660, 62), 36, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(panel.transform, name + "Subtitle", subtitle, new Vector2(-150, 275), new Vector2(920, 35), 17, Muted);
            if (close != null) Button(panel.transform, close, "返回", new Vector2(575, 318), new Vector2(105, 42), Sand, Ink, 19);
            return panel;
        }
        static GameObject Dialog(Transform parent, string name, string title, string subtitle)
        {
            var panel = Panel(parent, name, new Color(.13f, .23f, .25f, .55f));
            Box(panel.transform, "Sheet", Vector2.zero, new Vector2(760, 640), Paper);
            Label(panel.transform, name + "Title", title, new Vector2(0, 238), new Vector2(650, 66), 39, Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            Label(panel.transform, name + "Subtitle", subtitle, new Vector2(0, 170), new Vector2(650, 55), 17, Muted, TextAnchor.MiddleCenter);
            return panel;
        }
        static Image Bar(Transform parent, string name, Vector2 p, Vector2 size, Color color)
        {
            var back = Box(parent, name, p, size, Sand);
            var fill = Box(back, name + "Fill", Vector2.zero, Vector2.zero, color).GetComponent<Image>();
            fill.rectTransform.anchorMin=Vector2.zero;fill.rectTransform.anchorMax=Vector2.one;fill.rectTransform.offsetMin=Vector2.zero;fill.rectTransform.offsetMax=Vector2.zero;
            fill.fillAmount = 1; fill.raycastTarget = false; return fill;
        }
        static Transform Grid(Transform parent, string name, Vector2 p, Vector2 size, int columns, Vector2 cell)
        {
            var rt = Rect(parent, name, p, size); var grid = rt.gameObject.AddComponent<GridLayoutGroup>(); grid.cellSize = cell; grid.spacing = new Vector2(16, 16); grid.childAlignment = TextAnchor.UpperLeft; grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = columns; return rt;
        }
        static Transform Scroll(Transform parent, string name, Vector2 p, Vector2 size, int columns, Vector2 cell)
        {
            var rt = Rect(parent, name, p, size); var scroll = rt.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 28;
            var viewport = Rect(rt, "Viewport", Vector2.zero, size); viewport.gameObject.AddComponent<RectMask2D>();
            var content = Grid(viewport, "Content", Vector2.zero, size, columns, cell) as RectTransform;
            content.anchorMin = new Vector2(.5f, 1); content.anchorMax = new Vector2(.5f, 1); content.pivot = new Vector2(.5f, 1); content.anchoredPosition = Vector2.zero;
            var fit = content.gameObject.AddComponent<ContentSizeFitter>(); fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var rail=Box(rt,"ScrollRail",new Vector2(size.x*.5f-3,0),new Vector2(5,size.y-8),Sand) as RectTransform;
            var thumb=Box(rail,"Thumb",Vector2.zero,new Vector2(5,60),new Color(Mint.r,Mint.g,Mint.b,.65f)) as RectTransform;
            var scrollbar=rail.gameObject.AddComponent<Scrollbar>();scrollbar.targetGraphic=thumb.GetComponent<Image>();scrollbar.handleRect=thumb;scrollbar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=scrollbar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
            scroll.viewport = viewport; scroll.content = content; return content;
        }
        static UIWidget WidgetTemplate(Transform parent, string name)
        {
            var t = Box(parent, name, Vector2.zero, new Vector2(330, 176), Color.white); var w = t.gameObject.AddComponent<UIWidget>(); w.background = t.GetComponent<Image>();
            w.selectionOutline=t.gameObject.AddComponent<Outline>();w.selectionOutline.effectDistance=new Vector2(2,-2);w.selectionOutline.enabled=false;
            w.button = t.gameObject.AddComponent<Button>(); w.button.targetGraphic = w.background;
            var accent = Box(t, "Accent", new Vector2(-125, 48), new Vector2(49, 49), Mint); w.accent = accent.GetComponent<Image>();
            var art = accent as RectTransform; art.anchorMin = art.anchorMax = new Vector2(0, 1); art.anchoredPosition = new Vector2(38, -36);
            w.icon = Label(accent, "Icon", "星", Vector2.zero, new Vector2(45, 45), 25, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            w.symbol=Rect(accent,"Symbol",Vector2.zero,new Vector2(34,34)).gameObject.AddComponent<AbilityIcon>();w.symbol.color=Color.white;w.symbol.raycastTarget=false;
            w.portrait=Rect(accent,"Portrait",Vector2.zero,new Vector2(58,58)).gameObject.AddComponent<RawImage>();w.portrait.raycastTarget=false;w.portrait.gameObject.SetActive(false);
            w.preview=Rect(t,"LandscapePreview",Vector2.zero,new Vector2(225,66)).gameObject.AddComponent<RawImage>();w.preview.raycastTarget=false;var preview=w.preview.rectTransform;preview.anchorMin=preview.anchorMax=new Vector2(.5f,1);preview.anchoredPosition=new Vector2(0,-100);w.preview.gameObject.SetActive(false);
            w.lockIcon=Rect(t,"Locked",Vector2.zero,new Vector2(18,18)).gameObject.AddComponent<AbilityIcon>();w.lockIcon.color=Muted;w.lockIcon.raycastTarget=false;w.lockIcon.Set("锁");w.lockIcon.rectTransform.anchorMin=w.lockIcon.rectTransform.anchorMax=Vector2.one;w.lockIcon.rectTransform.anchoredPosition=new Vector2(-17,-20);w.lockIcon.gameObject.SetActive(false);
            w.title = Label(t, "WidgetTitle", "能力名称", new Vector2(0, 0), new Vector2(230, 48), 22, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
            var title = w.title.rectTransform; title.anchorMin = new Vector2(0, 1); title.anchorMax = new Vector2(1, 1); title.pivot = new Vector2(.5f, 1); title.anchoredPosition = new Vector2(29, -13); title.sizeDelta = new Vector2(-96, 48);
            w.body = Label(t, "WidgetBody", "", Vector2.zero, new Vector2(285, 70), 16, Muted);
            var body = w.body.rectTransform; body.anchorMin = new Vector2(0, 1); body.anchorMax = new Vector2(1, 1); body.pivot = new Vector2(.5f, 1); body.anchoredPosition = new Vector2(0, -72); body.sizeDelta = new Vector2(-40, 65);
            w.meta = Label(t, "WidgetMeta", "", Vector2.zero, new Vector2(285, 34), 17, Ink, TextAnchor.MiddleLeft,FontStyle.Bold);
            var meta = w.meta.rectTransform; meta.anchorMin = new Vector2(0, 0); meta.anchorMax = new Vector2(1, 0); meta.pivot = new Vector2(.5f, 0); meta.anchoredPosition = new Vector2(0, 10); meta.sizeDelta = new Vector2(-40, 31);
            w.gameObject.SetActive(false); return w;
        }
        static UIWidget AbilityTemplate(Transform parent)
        {
            var t=Rect(parent,"AbilityTemplate",Vector2.zero,new Vector2(110,58));var w=t.gameObject.AddComponent<UIWidget>();
            var badge=Box(t,"Badge",new Vector2(-34,0),new Vector2(36,36),Mint);w.accent=badge.GetComponent<Image>();
            w.symbol=Rect(badge,"Pictogram",Vector2.zero,new Vector2(29,29)).gameObject.AddComponent<AbilityIcon>();w.symbol.color=Color.white;w.symbol.raycastTarget=false;
            w.title=Label(t,"AbilityName","",new Vector2(22,0),new Vector2(68,55),14,Ink,TextAnchor.MiddleCenter);
            w.gameObject.SetActive(false);return w;
        }
        static MapGraphic Map(Transform parent, string name, Vector2 p, Vector2 size, int radius)
        { var rt = Rect(parent, name, p, size); var map = rt.gameObject.AddComponent<MapGraphic>(); map.radius = radius; map.raycastTarget = false; return map; }
        static Slider Slider(Transform parent, string name, Vector2 p)
        {
            var rt = Rect(parent, name, p, new Vector2(300, 35)); var slider = rt.gameObject.AddComponent<Slider>();
            Box(rt, "Track", Vector2.zero, new Vector2(300, 10), Sand);
            var fill = Box(rt, "Fill", Vector2.zero, new Vector2(300, 10), Mint) as RectTransform;
            fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one; fill.offsetMin = new Vector2(0, 12); fill.offsetMax = new Vector2(0, -12);
            var handle = Box(rt, "Handle", Vector2.zero, new Vector2(24, 24), Mint) as RectTransform; handle.GetComponent<Image>().sprite = circle;
            slider.fillRect = fill; slider.handleRect = handle; slider.targetGraphic = handle.GetComponent<Image>(); slider.minValue = 0; slider.maxValue = 1; slider.value = .5f; return slider;
        }
    }
}
