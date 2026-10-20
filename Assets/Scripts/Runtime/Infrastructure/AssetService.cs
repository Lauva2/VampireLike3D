using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace VampireLike
{
    public sealed class AssetScope
    {
        internal readonly HashSet<string> keys = new HashSet<string>();
        internal readonly List<GameObject> instances = new List<GameObject>();
        public bool Disposed { get; internal set; }
        public readonly string Name;
        public AssetScope(string name) { Name = name; }
    }

    public sealed class AssetService : IDisposable
    {
        sealed class Slot
        {
            public AsyncOperationHandle<GameObject> handle;
            public int owners;
        }
        readonly Dictionary<string, Slot> prefabs = new Dictionary<string, Slot>();
        readonly Dictionary<string, AsyncOperationHandle<AudioClip>> audio = new Dictionary<string, AsyncOperationHandle<AudioClip>>();
        AsyncOperationHandle<GameCatalog> catalogHandle;
        AsyncOperationHandle initialization;
        public GameCatalog Catalog { get; private set; }
        public int LoadedPrefabCount => prefabs.Count;
        readonly HashSet<string> diagnosticFailures=new HashSet<string>();
        public bool DiagnosticFailNextPrefab(string id){if(GameManager.Instance==null||!GameManager.Instance.DiagnosticMode)return false;diagnosticFailures.Add(id);return true;}
        public string LastError { get; private set; }
        public IEnumerator Initialize()
        {
            initialization = Addressables.InitializeAsync(false);
            yield return initialization;
            if (initialization.Status != AsyncOperationStatus.Succeeded) { LastError = "资源系统初始化失败"; yield break; }
            catalogHandle = Addressables.LoadAssetAsync<GameCatalog>("config/catalog");
            yield return catalogHandle;
            if (catalogHandle.Status != AsyncOperationStatus.Succeeded) { LastError = "内容目录加载失败"; yield break; }
            Catalog = catalogHandle.Result;
        }
        public IEnumerator LoadPrefab(string id, AssetScope scope, Action<GameObject> ready)
        {
            if (scope.Disposed) yield break;
            if(diagnosticFailures.Remove(id)){LastError="受控加载失败: "+id;ready?.Invoke(null);yield break;}
            bool created = false;
            if (!prefabs.TryGetValue(id, out Slot slot))
            {
                AssetReferenceGameObject reference = null;
                try { reference = Catalog.Prefab(id); }
                catch (Exception ex) { LastError = ex.Message; }
                if (reference == null) { ready?.Invoke(null); yield break; }
                slot = new Slot { handle = Addressables.LoadAssetAsync<GameObject>(reference.RuntimeKey) };
                prefabs.Add(id, slot);
                created = true;
            }
            if (scope.keys.Add(id)) slot.owners++;
            if (created) slot.handle.Completed += _ => ReleaseUnused(id, slot);
            yield return slot.handle;
            if (scope.Disposed) { ReleaseUnused(id, slot); yield break; }
            if (slot.handle.Status != AsyncOperationStatus.Succeeded)
            {
                LastError = "加载失败: " + id; ready?.Invoke(null); yield break;
            }
            ready?.Invoke(slot.handle.Result);
        }
        public IEnumerator Instantiate(string id, AssetScope scope, Transform parent, Action<GameObject> ready)
        {
            if (scope.Disposed) yield break;
            AssetReferenceGameObject reference = null;
            try { reference = Catalog.Prefab(id); } catch (Exception ex) { LastError = ex.Message; }
            if (reference == null) { ready?.Invoke(null); yield break; }
            var operation = Addressables.InstantiateAsync(reference.RuntimeKey, parent);
            bool finished=false;GameObject instance=null;
            operation.Completed+=completed=>
            {
                if(completed.Status==AsyncOperationStatus.Succeeded)
                {
                    if(scope.Disposed)Addressables.ReleaseInstance(completed.Result);
                    else {instance=completed.Result;scope.instances.Add(instance);}
                }
                else if(completed.IsValid())Addressables.Release(completed);
                finished=true;
            };
            while(!finished)yield return null;
            if(scope.Disposed)yield break;
            if (instance==null)
            {
                LastError = "实例化失败: " + id;
                ready?.Invoke(null);
                yield break;
            }
            ready?.Invoke(instance);
        }
        public IEnumerator LoadScene(string address, Action<AsyncOperationHandle<SceneInstance>> ready)
        {
            var operation = Addressables.LoadSceneAsync(address, LoadSceneMode.Additive);
            yield return operation;
            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                LastError = "场景加载失败: " + address;
                if (operation.IsValid()) Addressables.Release(operation);
                ready?.Invoke(default); yield break;
            }
            ready?.Invoke(operation);
        }
        public IEnumerator LoadAudio(string id, Action<AudioClip> ready)
        {
            if (!audio.TryGetValue(id, out var operation))
            {
                var entry = Array.Find(Catalog.audio, a => a.id == id);
                if (entry == null) { ready?.Invoke(null); yield break; }
                operation = Addressables.LoadAssetAsync<AudioClip>(entry.reference.RuntimeKey); audio[id] = operation;
            }
            yield return operation;
            ready?.Invoke(operation.Status == AsyncOperationStatus.Succeeded ? operation.Result : null);
        }
        public IEnumerator UnloadScene(AsyncOperationHandle<SceneInstance> scene)
        {
            if (!scene.IsValid()) yield break;
            var unload = Addressables.UnloadSceneAsync(scene, true);
            yield return unload;
        }
        public void Release(AssetScope scope)
        {
            if (scope == null || scope.Disposed) return;
            scope.Disposed = true;
            foreach (var instance in scope.instances) if (instance != null) Addressables.ReleaseInstance(instance);
            scope.instances.Clear();
            foreach (var key in scope.keys)
                if (prefabs.TryGetValue(key, out Slot slot)) { slot.owners--; ReleaseUnused(key, slot); }
            scope.keys.Clear();
        }
        void ReleaseUnused(string id, Slot slot)
        {
            if (slot.owners > 0 || !slot.handle.IsValid()||!slot.handle.IsDone || !prefabs.TryGetValue(id, out Slot current) || current != slot) return;
            if (slot.handle.IsValid()) Addressables.Release(slot.handle);
            prefabs.Remove(id);
        }
        public void Dispose()
        {
            foreach (var slot in prefabs.Values) if (slot.handle.IsValid()) Addressables.Release(slot.handle);
            prefabs.Clear();
            foreach (var operation in audio.Values) if (operation.IsValid()) Addressables.Release(operation);
            audio.Clear();
            if (catalogHandle.IsValid()) Addressables.Release(catalogHandle);
            if (initialization.IsValid()) Addressables.Release(initialization);
            Catalog = null;
        }
    }
}
