# Project instructions

## Design and workflow

- Read `Docs/GameDesign.md` for confirmed gameplay, `Docs/Architecture.md` for engineering constraints, and the relevant milestone in `Docs/DevelopmentWorkflow.md` before implementing changes.
- Later explicit user instructions override these documents. Keep affected design and implementation notes synchronized when requirements change.
- Current runtime baseline: Unity 2022.3.62f2, built-in render pipeline, UGUI/TMP, legacy input. Addressables is planned but was not installed when these instructions were created. Inspect the current project before assuming implementation exists.

## Required architecture

- AA means Unity Addressables. Runtime content loading must go through a single AssetService adapter with explicit ownership and matching releases. Avoid Resources.Load, runtime AssetDatabase, manual AssetBundle loading, and blocking waits as alternative loading paths.
- All authored `.prefab` files, including UI, characters, enemies, bosses, weapons, projectiles, VFX, world pieces and system prefabs, belong under `Assets/Art/Prefabs/` in appropriate subdirectories. Imported model files belong in `Assets/Art/Models/`; runtime entry points use wrapper prefabs.
- GameManager is the unique owner of overall game flow and state transitions. Delegate combat, generation, progression and persistence implementation to focused modules.
- UIManager is the unique owner of UI opening/closing, layers, modal input and UI resource lifetimes. UI views submit requests; gameplay modules validate purchases, rewards and unlocks.
- Boot is the minimal built-in startup scene; gameplay scenes and normal runtime UI/content use AA. Preserve explicit lifetimes for persistent roots and scene resources.
- For pooled ordinary clones of an AA-loaded prefab, keep the prefab load handle until all clones are destroyed and the pool is disposed. Do not mix this with ownership of InstantiateAsync/ReleaseInstance instances.
- Handle failed loads, obsolete async results, UI closed before loading completes, duplicate transitions, duplicate settlement and cleanup on return to menu.
- Preserve `.meta` files and GUIDs. Keep runtime and Editor code separate. Use the existing Assets/Config, Assets/Scenes and Assets/Scripts roots.

## Gameplay invariants

- No weapon or passive slot limits; branches of the same weapon can coexist. Visual optimization must not silently remove acquired abilities or damage.
- Default auto-targeting with optional right-click priority marking; Shift dodge has a brief invulnerability window. Camera follows with fixed orientation and wheel zoom.
- Bosses are discovered and activated voluntarily. Players may disengage; the boss resets to full health after the exit buffer. Bosses do not appear automatically at a fixed time.
- Map progression: Forest → Desert → Snow → Volcano; first defeat of each boss unlocks the next biome. First defeat of all four unlocks the mixed-biome map.
- Voluntary extraction or death retains earned permanent resources and completed unlocks. Cumulative task progress persists. Settlement and rewards must be idempotent.
- Bright cartoon 3D with the selected chibi A character proportions. Reference image is under Docs/ArtReferences; it is concept art, not an implemented model.

## Delivery

- Do not report planned work, compilation, playtests, performance checks or builds as completed without evidence.
- Validate according to actual change risk. Include meaningful coverage for resource lifecycle, combination rules, deterministic world state, save migration and reward idempotency when implementing those systems.
- Every milestone should have a reproducible runnable result, relevant validation and recorded limitations. A combat prototype is not the finished game.
