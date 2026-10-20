using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace VampireLike
{
    [Serializable] public sealed class ChunkRecord { public bool visited; public List<string> completed = new List<string>(); }
    public sealed class WorldJournal
    {
        readonly string root;
        readonly Dictionary<Vector2Int, ChunkRecord> active = new Dictionary<Vector2Int, ChunkRecord>();
        public WorldJournal(string runId)
        {
            string parent=Path.GetFullPath(Path.Combine(Application.temporaryCachePath,"WorldSessions"));
            root = Path.GetFullPath(Path.Combine(parent, runId));
            if(!root.StartsWith(parent+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)||runId.IndexOfAny(Path.GetInvalidFileNameChars())>=0)throw new ArgumentException("Invalid run journal id");
            Directory.CreateDirectory(root);
        }
        string FilePath(Vector2Int key) => Path.Combine(root, key.x + "_" + key.y + ".json");
        public bool Visited(Vector2Int key)
        {
            if(active.TryGetValue(key,out var record))return record.visited;
            try{return File.Exists(FilePath(key))&&JsonUtility.FromJson<ChunkRecord>(File.ReadAllText(FilePath(key))).visited;}catch{return false;}
        }
        public void Visit(Vector2Int key){var record=Load(key);if(record.visited)return;record.visited=true;Write(key,record);}
        public ChunkRecord Load(Vector2Int key)
        {
            if (active.TryGetValue(key, out var record)) return record;
            try { record = File.Exists(FilePath(key)) ? JsonUtility.FromJson<ChunkRecord>(File.ReadAllText(FilePath(key))) : null; }
            catch (Exception ex) { Debug.LogWarning("World record: " + ex.Message); }
            if (record == null) record = new ChunkRecord();
            if (record.completed == null) record.completed = new List<string>();
            active[key] = record; return record;
        }
        public void Complete(Vector2Int key, string id)
        {
            var record = Load(key); if (!record.completed.Contains(id)) record.completed.Add(id);
            Write(key, record);
        }
        void Write(Vector2Int key, ChunkRecord record)
        {
            try { File.WriteAllText(FilePath(key), JsonUtility.ToJson(record)); }
            catch (Exception ex) { Debug.LogError("World record write: " + ex.Message); }
        }
        public void Unload(Vector2Int key) { active.Remove(key); }
        public void Dispose() { active.Clear();try{if(Directory.Exists(root))Directory.Delete(root,true);}catch(Exception ex){Debug.LogWarning("Journal cleanup: "+ex.Message);} }
    }
}
