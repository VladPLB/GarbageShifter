# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

Unity project (URP, `com.unity.render-pipelines.universal`). Gameplay code lives under `Assets/_GAME/Scripts/`. There is no CI/build script, no test assembly, and no `.asmdef` split — all game scripts compile into the default `Assembly-CSharp`. There is nothing to "run" from the CLI; changes are verified by opening the project in the Unity Editor and pressing Play (Gameplay/Lobby scenes under `Assets/_GAME/Scenes/`).

Key third-party dependencies (see `Packages/manifest.json` and `Assets/Plugins`):
- **Cysharp UniTask** (`com.cysharp.unitask`) — used everywhere instead of coroutines/`Task` for async flow (`async UniTaskVoid`/`async void` + `await UniTask.Delay(...)`, `UniTask.WaitWhile(...)`, `UniTask.DelayFrame(...)`).
- **DOTween** (`DG.Tweening`) — tweening.
- **Cinemachine**, **Animation Rigging**, **AI Navigation** — used by camera and enemy/ragdoll systems.
- Third-party asset packs (Grabbit, FIMSpace procedural animation, PolygonSciFiSpace) live outside `_GAME` and are asset/animation content, not code you should need to touch.

## Architecture

### Service locator ("Core"), not a DI framework

There is no Zenject/VContainer. `Assets/_GAME/Scripts/Cores/Core.cs` is a hand-rolled singleton service registry:

- `Core.Registry(this, params Type[] conditions)` — registers a MonoBehaviour instance under its own type. If `conditions` are given, registration is deferred until every listed type is already registered (a simple dependency-wait queue), letting managers declare "I need X, Y, Z booted first" without an explicit init order.
- `Core.Get<T>()` — static lookup by type, used everywhere instead of scene references or injected constructors.
- `Core.Unregistry(this)` — called from `OnDestroy` to remove a module.
- Registered MonoBehaviours are auto-reparented under the `Core` GameObject unless they implement the empty marker interface `IReparentIgnored`.
- If a registered object implements `IRuntimeSetup`, `RuntimeSetup()` is invoked immediately once its dependency conditions are satisfied — this is the de facto "constructor"/init hook for manager classes (see `LevelController.Awake/RuntimeSetup`, `SaveManager.Awake/RuntimeSetup`).

When adding a new manager-style class, follow this pattern: register in `Awake()` with `Core.Registry(this, typeof(DepA), typeof(DepB), ...)`, implement `IRuntimeSetup.RuntimeSetup()` to pull dependencies via `Core.Get<T>()` and do real initialization there, and unregister in `OnDestroy()`.

`Settings` (`Cores/Settings.cs`) is the root config MonoBehaviour, exposing ScriptableObject settings (e.g. `BattleSettings`, `DefaultSaveDataConfig`) via `Core.Get<Settings>()`.

### Event bus

`Assets/_GAME/Scripts/Cores/EventsBus/EventBus.cs` is a static pub/sub bus keyed by event **type** and an `EventRegion` enum (`GLOBAL`, `GAMEPLAY`, `LOBBY`; `DEFAULT` broadcasts to/subscribes across all regions). Events are plain classes implementing the empty `IEvent` marker interface, one file per event under `Cores/EventsBus/Events/`. Use `EventBus.Subscribe<T>/Unsubscribe<T>/Push<T>(data, region)`; always unsubscribe in `OnDestroy`/`OnDisable` since the bus holds raw delegate references. Prefer this over direct cross-system references or `UnityEvent`s for decoupled notifications (UI opening, audio cues, scene/stage transitions, tutorial triggers).

### Save data

`Cores/Save/SaveManager.cs` is a `Core`-registered module implementing `IRuntimeSetup`. Save "slices" implement `ISaveData` (with an `OnDataChanged` event); each is serialized independently via `JsonUtility` into a single `PlayerPrefs` blob. Get/create a slice with `SaveManager.GetData<T>()` — don't instantiate save data types directly. Default data objects come from a `DefaultSaveDataConfig` ScriptableObject.

### Strategy-pattern-heavy gameplay systems

Most gameplay variation is expressed as small interchangeable strategies rather than branching logic, using either abstract MonoBehaviour bases or abstract ScriptableObject bases:

- Enemy behavior: `BaseEnemyFireConditionBehaviour` (Battle/Enemy/FireConditionBehaviours/*), `BaseRagdollBehaviour` (Battle/Enemy/RagdollBehaviours/*), `DeadHandler` (Battle/Enemy/DeadHandlers/*), `BaseAllyTargetFinder`.
- Inventory item rules as ScriptableObjects: `ItemConditionShowSO`, `ItemConditionUseSO`, `ItemConditionCraftSO` (each with `...ByLevel`/`...ByPlace`/`...Always`/`...Never` concrete variants) and `ItemUsePipelineSO` for use-effects. New item behavior = add a new SO subclass, not new `if` branches.
- Level structure: `LevelStage` + `levelStageConditionBase` subclasses (`...ConditionKillAllEnemies`, `...ConditionDelay`, `...ConditionEvent`, `...ConditionPermanent`, `...ConditionNewer`) compose how a stage decides it's complete; `LevelStageBuilder` turns a `LevelData` asset into a `List<LevelStage>`.

When extending these systems, add a new subclass of the relevant base rather than adding special-case logic to the base or its callers.

### Level/stage flow

`LevelController` (Battle/Level/LevelController.cs) drives the gameplay loop with `async void`/UniTask: build stages from `LevelData` → `PlayFirstStage` → per-stage `PlayCurrentStage`/`ToNextStage` awaiting stage state (`stage.IsPlay`, `stage.IsCompleted`) and fixed `GameConstants` delays → `ShowCompletedScreen`. It pulls its dependencies from `Core.Get<T>()` inside `RuntimeSetup()` and pushes lifecycle events (`KeyEvent`, `MusicPlayEvent`, `AmbientStopEvent`) through `EventBus`.

### Pooling

`Cores/Pools/{Pool,LocalPool,PoolProvider}.cs` + `IPoolableItem` — used for bullets, effects, enemies/drops instead of `Instantiate`/`Destroy` in hot paths. Register poolable prefabs through `PoolProvider` (itself a `Core` module).

### UI

`UI/UIWindow.cs` is the abstract base for screens (`OnOpen`/`OnClose`, closes itself via a `UIManager` reference set with `SetManager`). Screens live under `UI/Screens/**` (Battle, Lobby, Communications, etc.) and are opened via `UIManager.OpenWindow<T>()` (obtained through `Core.Get<UIManager>()`), not by toggling GameObjects directly.

## Code style conventions

- Namespaces mirror folder paths under the `_GAME.Scripts` root, e.g. `Assets/_GAME/Scripts/Battle/Enemy/` → `namespace _GAME.Scripts.Battle.Enemy`. A few older top-level scripts (`LevelController`, `Core`, `Settings`) sit directly in `_GAME.Scripts` or global namespace — new code should follow the folder-mirrors-namespace convention.
- Private fields: `_camelCase`, usually `[SerializeField] private`; grouped under `[Header("...")]` blocks in inspector-heavy MonoBehaviours.
- Public API exposed via `=>` expression-bodied properties over private backing fields (e.g. `public Player Player => _player;`) rather than public fields or full property bodies.
- Async work uses UniTask (`async void`/`async UniTaskVoid` + `await UniTask.Delay/DelayFrame/WaitWhile`), not coroutines, for new gameplay sequencing code.
- ScriptableObjects for designer-facing data/config/rules use `[CreateAssetMenu(fileName = "...", menuName = "Scriptable/<Category>/...")]`.
- 4-space indentation, Allman-style braces (opening brace on its own line).
