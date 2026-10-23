using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class PoolService
    {
        readonly Dictionary<string, GameObject> templates = new Dictionary<string, GameObject>();
        readonly Dictionary<string, Stack<GameObject>> free = new Dictionary<string, Stack<GameObject>>();
        readonly HashSet<GameObject> all = new HashSet<GameObject>();
        readonly Dictionary<GameObject, string> ids = new Dictionary<GameObject, string>();
        readonly Transform root;
        public int Count => all.Count;
        public PoolService(Transform root) { this.root = root; }
        public void Register(string id, GameObject prefab) { if (prefab != null) templates[id] = prefab; }
        public GameObject Rent(string id, Vector3 position, Quaternion rotation)
        {
            if (!templates.TryGetValue(id, out var template) || template == null) return null;
            if (!free.TryGetValue(id, out var stack)) free[id] = stack = new Stack<GameObject>();
            GameObject go = null;
            while (stack.Count > 0 && go == null) go = stack.Pop();
            if (go == null) { go = Object.Instantiate(template, root); all.Add(go); ids[go] = id; }
            go.transform.SetPositionAndRotation(position, rotation); go.transform.localScale = template.transform.localScale; go.SetActive(true);
            return go;
        }
        public void Return(GameObject go)
        {
            if (go == null || !go.activeSelf || !ids.TryGetValue(go, out var id)) return;
            go.SetActive(false); go.transform.SetParent(root, false);
            if (!free.TryGetValue(id, out var stack)) free[id] = stack = new Stack<GameObject>();
            if (stack.Count < 256) stack.Push(go);
            else { all.Remove(go); ids.Remove(go); Object.Destroy(go); }
        }
        public void Clear()
        {
            // Disable callbacks may return owned secondary effects; invalidate the registry before destroying objects.
            var releasing = new List<GameObject>(all);
            all.Clear(); ids.Clear(); free.Clear(); templates.Clear();
            foreach (var go in releasing) if (go != null) Object.Destroy(go);
        }
    }
}
