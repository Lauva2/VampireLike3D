using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VampireLike
{
    public sealed class UIManager : MonoBehaviour
    {
        GameManager game;
        UIRefs view;
        readonly Dictionary<string, Text> texts = new Dictionary<string, Text>();
        readonly Dictionary<string, Button> buttons = new Dictionary<string, Button>();
        string modal, selectedCharacter = "adventurer", filter = "全部", settingsReturn;
        BiomeId selectedBiome;
        float toastUntil, nextHUD;
        List<UpgradeDefinition> currentChoices;
        readonly Dictionary<string, Texture2D> portraits = new Dictionary<string, Texture2D>();
        readonly Dictionary<AbilityKind, UIWidget> abilityWidgets = new Dictionary<AbilityKind, UIWidget>();
        readonly Dictionary<int,Texture2D> landscapes=new Dictionary<int,Texture2D>();
        public bool Ready => view != null;
        public bool BlocksGameplay => !string.IsNullOrEmpty(modal);
        public UIRefs View => view;
        public string ActiveModal=>modal;
        public IEnumerator Initialize(GameManager manager, AssetScope scope)
        {
            game = manager;
            yield return game.Assets.Instantiate("ui/root", scope, null, root =>
            {
                if (root == null) return;
                DontDestroyOnLoad(root); view = root.GetComponent<UIRefs>();
            });
            if (view == null) yield break;
            foreach (var text in view.GetComponentsInChildren<Text>(true)) if (!texts.ContainsKey(text.name)) texts.Add(text.name, text);
            foreach (var button in view.GetComponentsInChildren<Button>(true)) if (!buttons.ContainsKey(button.name)) buttons.Add(button.name, button);
            Bind(); ShowLoading("正在整理远征行囊…");
            foreach (var c in game.Catalog.characters)
            {
                GameObject prefab = null;
                yield return game.Assets.LoadPrefab(c.prefabId, scope, p => prefab = p);
                if (prefab != null) portraits[c.id] = PortraitRenderer.Create(prefab);
            }
            var previews=new AssetScope("selection-previews");
            GameObject pathPrefab=null;yield return game.Assets.LoadPrefab("path/network",previews,p=>pathPrefab=p);
            for(int i=0;i<4;i++)
            {
                var biome=game.Catalog.biomes[i];GameObject terrain=null,decoration=null,architecture=null;
                yield return game.Assets.LoadPrefab(biome.terrainId,previews,p=>terrain=p);
                yield return game.Assets.LoadPrefab(biome.decorationId,previews,p=>decoration=p);
                yield return game.Assets.LoadPrefab("architecture/"+new[]{"forest","desert","snow","volcano"}[i],previews,p=>architecture=p);
                if(terrain!=null&&decoration!=null)landscapes[i]=PortraitRenderer.Landscape(terrain,decoration,architecture,pathPrefab,(BiomeId)i);
                yield return null;
            }
            if(landscapes.Count==4)
            {
                var mixed=new Texture2D(384,160,TextureFormat.RGB24,false);
                for(int i=0;i<4;i++)mixed.SetPixels(i*96,0,96,160,landscapes[i].GetPixels(144,0,96,160));mixed.Apply();landscapes[4]=mixed;
            }
            yield return null;game.Assets.Release(previews);
        }
        void Bind()
        {
            On("PlayButton", ShowSelection); On("ProgressButton", ShowProgress); On("SettingsButton", ShowSettings); On("QuitButton", () => { game.Save.Flush(); Application.Quit(); });
            On("StartButton", () => game.BeginRun(selectedCharacter, selectedBiome));
            foreach (var name in new[] { "SelectionClose", "SkillsClose", "MapClose", "ProgressClose", "SettingsClose", "ResumeButton", "VictoryContinue" }) On(name, CloseModal);
            On("RetreatButton", () => { CloseModal(); game.FinishRun(true); });
            On("VictoryRetreat", () => { CloseModal(); game.FinishRun(true); });
            On("ResultsContinue", game.ReturnToMenu);
            On("HUDSkills", ToggleSkills); On("HUDMap", ToggleMap); On("HUDPause", TogglePause);
            On("PauseSettings", ShowSettings);
            On("RerollButton", () =>
            {
                game.Progression.Reroll();
            });
            On("SkipUpgrade", () => game.Progression.SkipLevel());
            foreach (var branch in new[] { "全部", "元素", "召唤", "弹道", "爆破", "剑修", "符阵", "通用", "联动" })
            { string value = branch; On("Filter" + branch, () => { filter = value; PopulateSkills(); }); }
            On("MapNorth", () => view.largeMap.offset += Vector2Int.up * 4);
            On("MapSouth", () => view.largeMap.offset += Vector2Int.down * 4);
            On("MapWest", () => view.largeMap.offset += Vector2Int.left * 4);
            On("MapEast", () => view.largeMap.offset += Vector2Int.right * 4);
            On("MapCenter", () => view.largeMap.offset = Vector2Int.zero);
            On("QualityButton", () => { game.Save.Profile.quality = (game.Save.Profile.quality + 1) % 3; ApplySettings(); });
            On("FullscreenButton", () => { Screen.fullScreen = !Screen.fullScreen; ApplySettings(); });
            view.musicSlider.onValueChanged.AddListener(v => { game.Save.Profile.musicVolume = v; game.Audio.RefreshVolumes(); game.Save.MarkDirty(); });
            view.soundSlider.onValueChanged.AddListener(v => { game.Save.Profile.soundVolume = v; game.Audio.RefreshVolumes(); game.Save.MarkDirty(); });
        }
        void On(string name, Action action)
        {
            if (buttons.TryGetValue(name, out var button)) button.onClick.AddListener(() => { game.Audio.Play(SoundCue.Click); action(); });
            else Debug.LogError("Missing UI button: " + name);
        }
        void Set(string name, string value) { if (texts.TryGetValue(name, out var text)) text.text = value; }
        Coroutine panelTransition;
        void ActiveOnly(GameObject selected, string reason)
        {
            bool opening=selected!=null&&!selected.activeSelf;
            if(panelTransition!=null){StopCoroutine(panelTransition);panelTransition=null;}
            foreach(var panel in new[]{view.menu,view.selection,view.upgrade,view.skills,view.pause,view.map,view.progress,view.settings,view.results,view.victory,view.loading}){var sheet=panel.transform.Find("Sheet");if(sheet!=null)sheet.localScale=Vector3.one;}
            foreach(var group in view.GetComponentsInChildren<CanvasGroup>(true))group.alpha=1;
            ClearToast();
            foreach (var panel in new[] { view.menu, view.selection, view.upgrade, view.skills, view.pause, view.map, view.progress, view.settings, view.results, view.victory, view.loading }) panel.SetActive(panel == selected);
            view.hud.SetActive(game.Flow == GameFlow.Playing && (selected == null || selected == view.upgrade));
            modal = reason;
            game.Pause("ui", game.Ready && !string.IsNullOrEmpty(reason));
            if(opening)panelTransition=StartCoroutine(Reveal(selected));
        }
        IEnumerator Reveal(GameObject panel)
        {
            var group=panel.GetComponent<CanvasGroup>();if(group==null)group=panel.AddComponent<CanvasGroup>();
            var sheet=panel.transform.Find("Sheet");
            float start=Time.unscaledTime;group.alpha=0;
            while(Time.unscaledTime-start<.16f){float t=Mathf.SmoothStep(0,1,(Time.unscaledTime-start)/.16f);group.alpha=t;if(sheet!=null)sheet.localScale=Vector3.one*Mathf.Lerp(.975f,1,t);yield return null;}
            group.alpha=1;if(sheet!=null)sheet.localScale=Vector3.one;panelTransition=null;
        }
        public void ShowMenu() { ActiveOnly(view.menu, "menu"); Set("MenuCrystalText", "旅途晶石  " + game.Save.Profile.crystals); }
        public void ShowHUD() { ActiveOnly(null, null); nextHUD = 0; }
        public void ShowLoading(string message) { ActiveOnly(view.loading, "loading"); Set("LoadingText", message); LoadingProgress(0); }
        public void LoadingProgress(float progress) => Set("LoadingProgress", Mathf.RoundToInt(progress * 100) + "%");
        public void CloseModal()
        {
            if (modal == "results" || modal == "loading") return;
            game.Save.MarkDirty(); game.Save.Flush();
            if(modal=="settings"&&settingsReturn=="pause"){ActiveOnly(view.pause,"pause");return;}
            if (game.Flow == GameFlow.Menu) ShowMenu(); else ShowHUD();
        }
        bool CanToggle() => game.Ready && modal != "upgrade" && modal != "victory";
        public void TogglePause() { if (!CanToggle()) return; if (modal != null) CloseModal(); else ActiveOnly(view.pause, "pause"); }
        public void ToggleSkills() { if (!CanToggle()) return; if (modal == "skills") CloseModal(); else { ActiveOnly(view.skills, "skills"); PopulateSkills(); } }
        public void ToggleMap() { if (!CanToggle()) return; if (modal == "map") CloseModal(); else { ActiveOnly(view.map, "map"); view.largeMap.offset = Vector2Int.zero; } }
        public void ShowSelection()
        {
            ActiveOnly(view.selection, "selection");
            Clear(view.characterList); Clear(view.biomeList);
            foreach (var character in game.Catalog.characters)
            {
                var c = character; bool unlocked = game.Save.TaskUnlocked(c.taskId);
                var widget = Widget(view.characterList, false);
                Fill(widget, c.name, c.subtitle + " · " + c.description, unlocked ? c.id == selectedCharacter ? "已选择" : "点击选择" : TaskHint(c.taskId), "旅", c.color);
                if (portraits.TryGetValue(c.id, out var portrait)) { widget.portrait.texture = portrait; widget.portrait.gameObject.SetActive(true); widget.symbol.gameObject.SetActive(false); }
                widget.button.interactable = unlocked;
                widget.lockIcon.gameObject.SetActive(!unlocked);Highlight(widget,unlocked&&c.id==selectedCharacter,c.color);
                widget.button.onClick.AddListener(() => { selectedCharacter = c.id; MenuStage.Active?.SelectCharacter(c.id); ShowSelection(); });
            }
            for (int i = 0; i < 5; i++)
            {
                BiomeId biome = (BiomeId)i; var config = game.Catalog.Biome(biome); bool unlocked = game.Save.MapUnlocked(biome);
                var widget = Widget(view.biomeList, true);
                string title = i == 4 ? "万象交界" : config.name;
                string description = i == 4 ? "所有群系交汇，无限探索与自由组合。" : config.subtitle;
                string requirement = i == 0 ? "初始开放" : i == 4 ? "首次击败全部四个群系 Boss" : "首次击败" + game.Catalog.biomes[i - 1].bossName;
                Fill(widget, title, description, unlocked ? selectedBiome == biome ? "已选择 · 无限地图" : "点击选择" : requirement, new[] { "林", "沙", "雪", "焰", "象" }[i], i == 4 ? new Color(.55f, .4f, .85f) : config.accent);
                if(landscapes.TryGetValue(i,out var landscape))
                {
                    widget.preview.gameObject.SetActive(true);widget.preview.texture=landscape;
                    widget.body.rectTransform.anchoredPosition=new Vector2(0,-136);widget.body.rectTransform.sizeDelta=new Vector2(-36,47);widget.body.fontSize=16;
                    widget.meta.fontSize=15;widget.meta.rectTransform.sizeDelta=new Vector2(-28,26);widget.meta.rectTransform.anchoredPosition=new Vector2(0,8);
                }
                widget.button.interactable = unlocked;
                widget.lockIcon.gameObject.SetActive(!unlocked);Highlight(widget,unlocked&&selectedBiome==biome,config.accent);
                widget.button.onClick.AddListener(() => { selectedBiome = biome; ShowSelection(); });
            }
            var selected = game.Catalog.Character(selectedCharacter);
            Set("SelectionDescription", selected.name + " · " + selected.description);
            if (buttons.TryGetValue("StartButton", out var start)) start.interactable = game.Save.TaskUnlocked(selected.taskId) && game.Save.MapUnlocked(selectedBiome);
            Set("SelectionCrystalText", "晶石  " + game.Save.Profile.crystals);
        }
        public void ShowUpgrade(List<UpgradeDefinition> choices)
        {
            currentChoices = choices; ActiveOnly(view.upgrade, "upgrade"); Clear(view.upgradeList);
            Set("UpgradeHeading", "等级 " + game.Run.level + " · 选择新的力量");
            foreach (var u in choices)
            {
                var upgrade = u; var widget = Widget(view.upgradeList, true);
                Fill(widget, u.name, u.description, u.unique ? "新机制 · 可与其他路线叠加" : "当前等级 " + game.Run.Level(u.id) + " → " + (game.Run.Level(u.id) + 1), Icon(u), u.color);
                var accent=(RectTransform)widget.accent.transform;accent.anchorMin=accent.anchorMax=new Vector2(.5f,1);accent.anchoredPosition=new Vector2(0,-65);accent.sizeDelta=new Vector2(86,86);
                widget.symbol.rectTransform.sizeDelta=new Vector2(60,60);
                widget.title.rectTransform.anchoredPosition=new Vector2(0,-122);widget.title.rectTransform.sizeDelta=new Vector2(-36,48);widget.title.alignment=TextAnchor.MiddleCenter;
                widget.body.rectTransform.anchoredPosition=new Vector2(0,-180);widget.body.rectTransform.sizeDelta=new Vector2(-42,80);widget.body.alignment=TextAnchor.UpperLeft;
                widget.meta.rectTransform.sizeDelta=new Vector2(-40,48);widget.meta.alignment=TextAnchor.MiddleCenter;
                widget.button.onClick.AddListener(() => game.Progression.Apply(upgrade.id, false));
            }
            Set("RerollLabel", "重新抽取  " + game.Run.rerolls);
            if (buttons.TryGetValue("RerollButton", out var reroll)) reroll.interactable = game.Run.rerolls > 0;
        }
        public void CloseUpgrade() { ShowHUD(); }
        void PopulateSkills()
        {
            Clear(view.skillList); Set("SkillPointsText", "定向点  " + game.Run.skillPoints);
            foreach (var u in game.Catalog.upgrades)
            {
                if (filter != "全部" && u.branch != filter) continue;
                var upgrade = u; bool eligible = game.Progression.Eligible(u); var widget = Widget(view.skillList, true);
                string status = !game.Save.TaskUnlocked(u.taskId) ? TaskHint(u.taskId) : u.unique && game.Run.Level(u.id) > 0 ? "已掌握" : u.effect == UpgradeEffect.NewAbility && game.Run.abilities.Contains(u.target) ? "已获得" : !eligible ? "需要先获得对应能力" : "定向点 " + game.Progression.Cost(u) + "  ·  等级 " + game.Run.Level(u.id);
                Fill(widget, u.name, u.description, status, Icon(u), u.color);
                widget.lockIcon.gameObject.SetActive(!game.Save.TaskUnlocked(u.taskId));
                if(eligible&&game.Run.skillPoints<game.Progression.Cost(u))widget.meta.text+=" · 不足";
                widget.button.interactable = eligible && game.Run.skillPoints >= game.Progression.Cost(u);
                widget.button.onClick.AddListener(() => { if (game.Progression.Apply(upgrade.id, true)) PopulateSkills(); });
            }
        }
        public void ShowProgress()
        {
            ActiveOnly(view.progress, "progress"); Clear(view.metaList); Clear(view.taskList);
            var p = game.Save.Profile;
            Set("ProgressCrystalText", "旅途晶石  " + p.crystals);
            string[] names = { "锋芒", "活力", "轻盈", "引力" };
            string[] descriptions = { "永久伤害 +6% / 级", "永久最大生命 +10 / 级", "永久移动速度 +2.5% / 级", "永久拾取范围 +0.25 / 级" };
            int[] levels = { p.damageLevel, p.healthLevel, p.speedLevel, p.pickupLevel };
            for (int i = 0; i < 4; i++)
            {
                int index = i; var widget = Widget(view.metaList, false); int cost = game.Save.MetaCost(levels[i]);
                Fill(widget, names[i] + "  Lv." + levels[i], descriptions[i], cost + " 晶石 · 点击提升", new[] { "攻", "生", "速", "引" }[i], new Color(.2f, .62f, .52f));
                widget.button.interactable = p.crystals >= cost;
                widget.button.onClick.AddListener(() => { if (game.Save.BuyMeta(index)) { game.Audio.Play(SoundCue.Upgrade); ShowProgress(); } });
            }
            foreach (var task in game.Catalog.tasks)
            {
                bool done = p.completedTasks.Contains(task.id); var widget = Widget(view.taskList, false);
                Fill(widget, (done ? "已完成 · " : "") + task.name, task.description, (done ? task.target : Mathf.Min(task.target, p.Metric(task.metric))) + " / " + task.target + "  ·  晶石 +" + task.crystals, done ? "成" : "途", done ? new Color(.2f, .65f, .46f) : new Color(.84f, .58f, .24f));
                widget.button.interactable = false;
            }
        }
        string TaskHint(string taskId)
        {
            var task = Array.Find(game.Catalog.tasks, t => t.id == taskId);
            return task == null ? "未解锁" : "解锁任务 · " + task.name;
        }
        public void ShowSettings()
        {
            if(modal!="settings")settingsReturn=modal;
            ActiveOnly(view.settings, "settings");
            view.musicSlider.SetValueWithoutNotify(game.Save.Profile.musicVolume); view.soundSlider.SetValueWithoutNotify(game.Save.Profile.soundVolume);
            ApplySettings();
        }
        void ApplySettings()
        {
            int quality = game.Save.Profile.quality;
            QualitySettings.shadows = quality == 0 ? ShadowQuality.Disable : ShadowQuality.All;
            QualitySettings.shadowDistance = quality == 2 ? 75 : 40;
            QualitySettings.antiAliasing = quality == 2 ? 4 : 0;
            Set("QualityLabel", "画面质量 · " + new[] { "流畅", "均衡", "精致" }[quality]);
            Set("FullscreenLabel", Screen.fullScreen ? "显示模式 · 全屏" : "显示模式 · 窗口");
            game.Save.MarkDirty(); game.Audio.RefreshVolumes();
        }
        public void RestoreSettings(){ApplySettings();}
        public void ShowBossVictory(BiomeId biome)
        {
            ActiveOnly(view.victory, "victory");
            Set("VictoryName", "击败「" + game.Catalog.Biome(biome).bossName + "」");
            Set("VictoryBody", biome == BiomeId.Volcano && game.Save.MapUnlocked(BiomeId.Mixed) ? "四方试炼完成 · 万象交界已开放！\n首胜与任务成果已经保存。" : "首胜与任务成果已经保存。\n你可以继续探索，也可以携带收获撤离。");
        }
        public void ShowResults(bool extraction)
        {
            ActiveOnly(view.results, "results"); var run = game.Run;
            Set("ResultsHeading", extraction ? "满载而归" : "旅途未完");
            Set("ResultsBody", "生存时间   " + Clock(run.seconds) + "\n击败敌人   " + run.kills + "\n成长等级   " + run.level + "\n本局晶石   " + run.earnedCrystals + "\n已掌握能力   " + run.abilities.Count + "\n\n永久资源、任务进度与解锁均已保存。");
        }
        public void Toast(string message, float duration = 3)
        {
            Set("ToastText", message); toastUntil = Time.unscaledTime + duration;
            if (texts.TryGetValue("ToastText", out var text)) text.transform.parent.gameObject.SetActive(true);
        }
        void ClearToast()
        {
            toastUntil = 0;
            if (texts.TryGetValue("ToastText", out var toast)) toast.transform.parent.gameObject.SetActive(false);
        }
        public void DamageFlash() { if (view != null) view.damageFlash.color = new Color(1, .24f, .18f, .18f); }
        static void SetBar(Image fill,float value)
        {
            value=Mathf.Clamp01(value);fill.fillAmount=value;fill.enabled=value>.001f;
            fill.rectTransform.anchorMax=new Vector2(value,1);
            fill.rectTransform.offsetMin=Vector2.zero;fill.rectTransform.offsetMax=Vector2.zero;
        }
        void Update()
        {
            if (view == null) return;
            var flash = view.damageFlash.color; flash.a = Mathf.MoveTowards(flash.a, 0, Time.unscaledDeltaTime); view.damageFlash.color = flash;
            if (Time.unscaledTime > toastUntil && texts.TryGetValue("ToastText", out var toast)) toast.transform.parent.gameObject.SetActive(false);
            if (!game.Ready || Time.unscaledTime < nextHUD) return;
            nextHUD = Time.unscaledTime + .1f;
            var run = game.Run; var player = game.Player;
            SetBar(view.healthFill,player.Health/player.MaxHealth);SetBar(view.experienceFill,run.experience/(float)run.nextExperience);
            SetBar(view.dodgeFill,1-player.DodgeCooldown/2.2f);
            Set("HealthText", Mathf.CeilToInt(player.Health) + " / " + Mathf.CeilToInt(player.MaxHealth));
            Set("LevelText", "Lv. " + run.level); Set("TimerText", Clock(run.seconds));
            Set("BiomeText", game.Catalog.Biome(game.World.CurrentBiome).name + "  ·  无限探索");
            Set("RunCurrencyText", "定向点 " + run.skillPoints + "     晶石 " + game.Save.Profile.crystals);
            Set("KillsText", "击败 " + run.kills); Set("InteractionText", game.World.InteractionText ?? "");
            foreach (var pair in abilityWidgets) pair.Value.gameObject.SetActive(run.abilities.Contains(pair.Key));
            foreach (var kind in run.abilities)
            {
                if (!abilityWidgets.TryGetValue(kind, out var badge))
                {
                    badge = Instantiate(view.abilityTemplate, view.abilityList, false); abilityWidgets.Add(kind, badge);
                    badge.title.text = game.Catalog.Ability(kind).name; badge.symbol.Set(Icon(kind));
                    badge.accent.color = game.Catalog.Ability(kind).color;
                }
                badge.gameObject.SetActive(true);
            }
            var boss = game.Combat.Boss;
            if(texts.TryGetValue("BiomeText",out var biomeText))biomeText.gameObject.SetActive(boss==null);
            view.bossBar.SetActive(boss != null);
            if (boss != null) { SetBar(view.bossFill,boss.Health/boss.MaxHealth); Set("BossNameText", game.Catalog.Biome(boss.Biome).bossName + (boss.Health < boss.MaxHealth * .5f ? "  ·  狂怒阶段" : "  ·  守卫试炼")); }
            Set("RetreatWarningText", game.Combat.RetreatProgress > 0 ? "即将脱战 · " + Mathf.Max(0, 2 - game.Combat.RetreatProgress).ToString("0.0") + " 秒后 Boss 重置" : "");
            var mark = game.Combat.Marked;
            Set("MarkText", mark == null ? "右键敌人 · 标记集火" : "集火目标  " + (mark.Type == 4 ? game.Catalog.Biome(mark.Biome).bossName : mark.Elite ? "精英" : new[] { "追击怪", "迅捷怪", "冲锋怪", "投射怪" }[mark.Type]) + "  " + Mathf.CeilToInt(mark.Health));
            Set("MapCoordinates", "世界坐标  " + game.World.GlobalPosition.x.ToString("0") + " / " + game.World.GlobalPosition.y.ToString("0") + "    种子 " + run.seed);
        }
        static string Clock(float seconds) => ((int)seconds / 60).ToString("00") + ":" + ((int)seconds % 60).ToString("00");
        public static string Icon(AbilityKind kind)
        {
            switch (kind) { case AbilityKind.Gun: return "弹"; case AbilityKind.Fireball: return "焰"; case AbilityKind.Sword: return "剑"; case AbilityKind.Spirit: return "灵"; case AbilityKind.Cannon: return "炮"; case AbilityKind.Rune: return "符"; default: return "星"; }
        }
        static string Icon(UpgradeDefinition upgrade)
        {
            if(upgrade.target!=AbilityKind.All)return Icon(upgrade.target);
            switch(upgrade.effect){case UpgradeEffect.Damage:return "攻";case UpgradeEffect.AttackSpeed:return "速";case UpgradeEffect.Health:case UpgradeEffect.Regeneration:return "生";case UpgradeEffect.MoveSpeed:return "足";case UpgradeEffect.Pickup:return "引";case UpgradeEffect.Armor:return "盾";case UpgradeEffect.Range:return "距";case UpgradeEffect.Count:return "叠";default:return "星";}
        }
        UIWidget Widget(Transform parent, bool card)
        {
            var widget = Instantiate(card ? view.cardTemplate : view.rowTemplate, parent, false);
            widget.gameObject.SetActive(true); widget.button.onClick.RemoveAllListeners(); return widget;
        }
        static void Clear(Transform parent)
        {
            foreach (Transform child in parent) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        }
        static void Fill(UIWidget widget, string title, string body, string meta, string icon, Color color)
        {
            widget.title.text = title; widget.body.text = body; widget.meta.text = meta; widget.icon.text = icon;
            widget.accent.color = color;
            if (widget.symbol != null) { widget.symbol.gameObject.SetActive(true); widget.symbol.Set(icon); widget.icon.gameObject.SetActive(false); }
            if (widget.portrait != null) widget.portrait.gameObject.SetActive(false);
            if(widget.preview!=null)widget.preview.gameObject.SetActive(false);
        }
        static void Highlight(UIWidget widget,bool selected,Color color)
        {
            widget.selectionOutline.enabled=selected;widget.selectionOutline.effectColor=color;
            widget.background.color=selected?Color.Lerp(Color.white,color,.10f):Color.white;
        }
        void OnDestroy() { foreach (var texture in portraits.Values) if (texture != null) Destroy(texture);foreach(var texture in landscapes.Values)if(texture!=null)Destroy(texture); }
    }
}
