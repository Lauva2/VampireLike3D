using System;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    public sealed class SaveService
    {
        public PlayerProfile Profile { get; private set; }
        public string Path { get; }
        public string RecoveryMessage { get; private set; }
        bool dirty;
        bool invalidPrimary;
        float lastWrite;
        public SaveService(string path = null)
        {
            Path = path ?? System.IO.Path.Combine(Application.persistentDataPath, "profile-v1.json");
            var primary = Read(Path); invalidPrimary = File.Exists(Path) && primary == null;
            Profile = primary ?? Read(Path + ".bak") ?? new PlayerProfile();
            if (invalidPrimary) RecoveryMessage = "存档异常，已恢复备份或创建新档。";
            Normalize(Profile);
        }
        static PlayerProfile Read(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var p = JsonUtility.FromJson<PlayerProfile>(File.ReadAllText(path));
                if (p != null && p.version == 0) p.version = 1;
                return p != null && p.version == 1 ? p : null;
            }
            catch (Exception ex) { Debug.LogWarning("Save read: " + ex.Message); return null; }
        }
        static void Normalize(PlayerProfile p)
        {
            if (p.bossWins == null || p.bossWins.Length != 4) p.bossWins = new bool[4];
            if (p.completedTasks == null) p.completedTasks = new System.Collections.Generic.List<string>();
            if (p.settledRuns == null) p.settledRuns = new System.Collections.Generic.List<string>();
            p.crystals = Mathf.Max(0, p.crystals);
        }
        public void MarkDirty() { dirty = true; }
        public void Tick()
        {
            if (dirty && Time.realtimeSinceStartup - lastWrite > 4) Flush();
        }
        public void Flush()
        {
            if (!dirty && File.Exists(Path)) return;
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            string temp = Path + ".tmp";
            try
            {
                File.WriteAllText(temp, JsonUtility.ToJson(Profile, true));
                if (invalidPrimary && File.Exists(Path)) { File.Move(Path, Path + ".corrupt-" + DateTime.UtcNow.Ticks); invalidPrimary = false; }
                if (File.Exists(Path)) File.Replace(temp, Path, Path + ".bak");
                else File.Move(temp, Path);
                dirty = false; lastWrite = Time.realtimeSinceStartup;
            }
            catch (Exception ex) { Debug.LogError("Save write failed: " + ex); }
        }
        public bool Settle(string runId, float seconds)
        {
            if (Profile.settledRuns.Contains(runId)) return false;
            Profile.settledRuns.Add(runId);
            if (Profile.settledRuns.Count > 64) Profile.settledRuns.RemoveAt(0);
            Profile.bestSeconds = Mathf.Max(Profile.bestSeconds, seconds);
            dirty = true; Flush(); return true;
        }
        public bool TaskUnlocked(string taskId) => string.IsNullOrEmpty(taskId) || Profile.completedTasks.Contains(taskId);
        public bool MapUnlocked(BiomeId biome)
        {
            if (biome == BiomeId.Forest) return true;
            if (biome != BiomeId.Mixed) return Profile.bossWins[(int)biome - 1];
            foreach (bool win in Profile.bossWins) if (!win) return false;
            return true;
        }
        public int MetaCost(int level) => 20 + level * 15;
        public bool BuyMeta(int index)
        {
            int[] levels = { Profile.damageLevel, Profile.healthLevel, Profile.speedLevel, Profile.pickupLevel };
            if (index < 0 || index > 3) return false;
            int cost = MetaCost(levels[index]);
            if (Profile.crystals < cost) return false;
            Profile.crystals -= cost;
            if (index == 0) Profile.damageLevel++;
            if (index == 1) Profile.healthLevel++;
            if (index == 2) Profile.speedLevel++;
            if (index == 3) Profile.pickupLevel++;
            dirty = true; Flush(); return true;
        }
    }
}
