using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class ProgressionService
    {
        readonly GameManager game;
        float lastTasks;
        int pendingLevels;
        List<UpgradeDefinition> offered;
        readonly System.Random random;
        public ProgressionService(GameManager game) { this.game = game; random = new System.Random(game.Run.seed ^ 0x24bf); }
        public bool Eligible(UpgradeDefinition upgrade)
        {
            if (!game.Save.TaskUnlocked(upgrade.taskId)) return false;
            var run = game.Run;
            if (upgrade.effect == UpgradeEffect.NewAbility) return !run.abilities.Contains(upgrade.target);
            if (upgrade.target != AbilityKind.All && !run.abilities.Contains(upgrade.target)) return false;
            if (upgrade.unique && run.Level(upgrade.id) > 0) return false;
            if (upgrade.effect == UpgradeEffect.ElectricAmmo && !run.abilities.Contains(AbilityKind.Gun)) return false;
            if (upgrade.effect == UpgradeEffect.FireSword && !run.abilities.Contains(AbilityKind.Sword)) return false;
            return true;
        }
        public int Cost(UpgradeDefinition u) => u.cost + game.Run.Level(u.id) * Mathf.Max(1, u.cost / 2);
        public List<UpgradeDefinition> Available()
        {
            var list = new List<UpgradeDefinition>();
            foreach (var u in game.Catalog.upgrades) if (Eligible(u)) list.Add(u);
            return list;
        }
        public List<UpgradeDefinition> Choices()
        {
            if(pendingLevels<=0)return new List<UpgradeDefinition>();
            if(offered!=null)return new List<UpgradeDefinition>(offered);
            var options = Available();
            for (int i = options.Count - 1; i > 0; i--) { int j = random.Next(i + 1); var temp = options[j]; options[j] = options[i]; options[i] = temp; }
            if (options.Count > 4) options.RemoveRange(4, options.Count - 4);
            offered=options;return new List<UpgradeDefinition>(offered);
        }
        public bool Apply(string id, bool purchase)
        {
            if(!purchase&&(pendingLevels<=0||offered==null||!offered.Exists(u=>u.id==id)))return false;
            return ApplyCore(id,purchase,false);
        }
        public bool GrantForDiagnostic(string id)=>game.DiagnosticMode&&ApplyCore(id,false,true);
        bool ApplyCore(string id,bool purchase,bool diagnostic)
        {
            var u = game.Catalog.Upgrade(id);
            if (u == null || !Eligible(u)) return false;
            if (purchase)
            {
                int cost = Cost(u);
                if (game.Run.skillPoints < cost) { game.UI.Toast("定向点不足，探索营地或继续升级可获得。"); return false; }
                game.Run.skillPoints -= cost;
            }
            game.Run.upgrades[id] = game.Run.Level(id) + 1;
            if (u.effect == UpgradeEffect.NewAbility) game.Run.abilities.Add(u.target);
            game.Player.RefreshStats(true); game.Abilities.Refresh();
            game.Audio.Play(SoundCue.Upgrade);
            if (!purchase&&!diagnostic) { pendingLevels--;offered=null;game.UI.CloseUpgrade(); }
            game.UI.Toast("获得「" + u.name + "」");
            return true;
        }
        public void Experience(int amount)
        {
            var run = game.Run; run.experience += amount;
            while (run.experience >= run.nextExperience)
            {
                run.experience -= run.nextExperience; run.level++; run.skillPoints += 2;
                run.nextExperience = Mathf.RoundToInt(12 + run.level * 5 + Mathf.Pow(run.level, 1.28f));
                pendingLevels++;
            }
        }
        public void SkipLevel() { if(pendingLevels<=0||offered==null)return;pendingLevels--;offered=null;game.Run.skillPoints += 2; game.UI.CloseUpgrade(); }
        public void Reroll(){if(pendingLevels<=0||offered==null||game.Run.rerolls<=0)return;game.Run.rerolls--;offered=null;game.UI.ShowUpgrade(Choices());}
        public void Tick()
        {
            if (pendingLevels > 0 && !game.IsPaused) game.UI.ShowUpgrade(Choices());
            if (Time.unscaledTime - lastTasks < 1) return;
            lastTasks = Time.unscaledTime; CheckTasks();
        }
        public void CheckTasks()
        {
            var profile = game.Save.Profile;
            foreach (var task in game.Catalog.tasks)
            {
                if (profile.completedTasks.Contains(task.id) || profile.Metric(task.metric) < task.target) continue;
                profile.completedTasks.Add(task.id); profile.crystals += task.crystals;
                game.Run.earnedCrystals += task.crystals;
                game.Save.MarkDirty(); game.Save.Flush();
                game.UI.Toast("任务完成 · " + task.name + "\n新内容已解锁，晶石 +" + task.crystals, 6);
                game.Audio.Play(SoundCue.Unlock);
            }
        }
        public void Reward(int points, int crystals)
        {
            game.Run.skillPoints += points; game.Run.earnedCrystals += crystals;
            game.Save.Profile.crystals += crystals; game.Save.MarkDirty();
        }
    }
}
