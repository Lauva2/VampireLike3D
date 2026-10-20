using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VampireLike
{
    public sealed class MenuStage : MonoBehaviour
    {
        public static MenuStage Active { get; private set; }
        AssetScope scope;
        GameManager game;
        PoolService pool;
        GameObject character;
        readonly List<GameObject> sceneObjects = new List<GameObject>();
        int selectionVersion;
        IEnumerator Start()
        {
            Active = this; game = GameManager.Instance;
            if (game == null) yield break;
            scope = new AssetScope("menu-stage"); pool = new PoolService(transform);
            foreach (string id in new[] { "terrain/forest", "decoration/forest", "character/adventurer", "character/elementalist", "character/swordsman" })
                yield return game.Assets.LoadPrefab(id, scope, prefab => pool.Register(id, prefab));
            if (scope.Disposed) yield break;
            for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z++)
            {
                var ground = pool.Rent("terrain/forest", new Vector3(x * 32, 0, z * 32), Quaternion.identity);
                if (ground != null) sceneObjects.Add(ground);
            }
            var random = new System.Random(71);
            for (int i = 0; i < 16; i++)
            {
                float a = (float)random.NextDouble() * Mathf.PI * 2; float r = 6 + (float)random.NextDouble() * 8;
                var go = pool.Rent("decoration/forest", new Vector3(Mathf.Sin(a) * r + 4, 0, Mathf.Cos(a) * r + 3), Quaternion.Euler(0, random.Next(360), 0));
                if (go != null) { go.GetComponent<DecorationVariant>()?.Select(go.transform.position.z < 4 && go.transform.position.x > 1 && go.transform.position.x < 12 ? 2 : i); sceneObjects.Add(go); }
            }
            SelectCharacter("adventurer");
        }
        public void SelectCharacter(string id)
        {
            if (pool == null || scope == null || scope.Disposed) return;
            selectionVersion++;
            if (character != null) pool.Return(character);
            character = pool.Rent(game.Catalog.Character(id).prefabId, new Vector3(5.5f, 0, -.5f), Quaternion.Euler(0, 156, 0));
            if (character != null)
            {
                var controller = character.GetComponent<CharacterController>(); if (controller != null) controller.enabled = false;
                character.transform.localScale = Vector3.one * 3.1f;
            }
        }
        void Update()
        {
            if (character == null) return;
            character.GetComponent<CharacterAnimator>()?.Animate(.08f, false, false);
            character.transform.rotation = Quaternion.Euler(0, 156 + Mathf.Sin(Time.unscaledTime * .45f) * 7, 0);
        }
        void OnDestroy()
        {
            if (Active == this) Active = null;
            pool?.Clear();
            if (game != null && scope != null) game.StartCoroutine(ReleaseLater(game.Assets, scope));
        }
        static IEnumerator ReleaseLater(AssetService assets, AssetScope owner) { yield return null; yield return null; assets.Release(owner); }
    }
}
