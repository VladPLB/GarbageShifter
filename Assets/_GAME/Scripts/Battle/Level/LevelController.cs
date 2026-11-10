using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using _GAME;
using _GAME.Scripts;
using _GAME.Scripts.Ads;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Items;
using _GAME.Scripts.Battle.Level;
using _GAME.Scripts.Battle.Level.Progression;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Common;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Events;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Map;
using _GAME.Scripts.Save;
using _GAME.Scripts.Tutorial;
using _GAME.Scripts.UI.Screens.Battle;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelController : MonoBehaviour, IRuntimeSetup, IReparentIgnored
{
    [Header("Lights")]
    [SerializeField] private Light _light;
    [Header("Debug")]
    [SerializeField] private bool _useOverrideLevelData = true;
    [SerializeField] private LevelData _overrideLevelData;
    [SerializeField] private BattleSettings _battleSettings;
    [Header("General")]
    [SerializeField] private LevelDatasController _levelDatasController;
    [SerializeField] private LevelStageBuilder _levelStageBuilder;
    
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private UnitsController _unitsController;
    [SerializeField] private List<TutorialController> _tutorialControllers;
    
    private UIManager _uiManager;
    private MapManager _mapManager;
    private DropManager _dropManager;
    private SaveManager _saveManager;
    private Settings _settings;
    private ProgressData _progressData;
    private EnemyProgression _enemyProgression;
    // Stage flow (stage play / switching); cancelled while the player is dead
    private CancellationTokenSource _flowCts;
    private int _deathCount = 0;
    private MapManager.LevelType _levelType;
    private float _levelStartTime;
    
    
    private Player _player;
    private List<LevelStage> _stages;
    private List<LevelStage> _realStages;
    private int _stagesCount = 0;
    private List<PlayerPosition> _playerPositions;
    private LevelPathFinder _pathfinder;
    private int _currentStageIndex = 0;

    public Player Player => _player;
    public UnitsController UnitsController => _unitsController;
    public EnemyProgression EnemyProgression => _enemyProgression;
    // Revive is offered only for the first death in the level
    public bool CanRevive => _deathCount <= 1;

    public event Action<Vector3> OnSpawnWarning;

    private void Awake()
    {
        Core.Registry(this, typeof(PoolProvider), typeof(UIManager), typeof(DropManager), typeof(InventoryManager), typeof(Settings));
    }
    
    public void RuntimeSetup()
    {
        _uiManager = Core.Get<UIManager>();
        _mapManager = Core.Get<MapManager>();
        _saveManager = Core.Get<SaveManager>();
        _dropManager = Core.Get<DropManager>();
        _settings = Core.Get<Settings>();
        
        _progressData = _saveManager.GetData<ProgressData>();
        _enemyProgression = new EnemyProgression(_settings.Battle.EnemyProgression, _progressData,
            _mapManager.GetLocationNumber(_progressData.Level), _mapManager.IsLastLevelInLocation(_progressData.Level));
        EventBus.Subscribe<PlayerDeadEvent>(OnPlayerDead, EventBus.EventRegion.GAMEPLAY);
        
        BuildLevel();
        Setup(FindObjectOfType<Player>());
    }

    public async void Setup(Player player)
    {
        var ct = this.GetCancellationTokenOnDestroy();
        try
        {
            _player = player;
            _player.Setup(_cameraController);
            _unitsController.Setup(_player.transform, _enemyProgression.Scaling);

            _realStages = _stages.Where(stage => stage.PlayerPosition != null).ToList();
            _stagesCount = _realStages.Count;
            _playerPositions = _realStages.Select(stage => stage.PlayerPosition).ToList();

            _pathfinder = new LevelPathFinder();
            await UniTask.DelayFrame(1, cancellationToken: ct);
            _pathfinder.Rebuild();

            foreach (var tutorialController in _tutorialControllers)
            {
                tutorialController.Initialize();
            }
            _uiManager.OpenWindow<GameplayScreen>();
            ResetFlow();
            PlayCurrentStage();
            _levelStartTime = Time.time;
            ReportLevel("level_start");
            EventBus.Push(new KeyEvent("SceneLoaded"), EventBus.EventRegion.GLOBAL);
            EventBus.Push(new MusicPlayEvent(MusicTrack.Battle), EventBus.EventRegion.GLOBAL);
        }
        catch (OperationCanceledException)
        {
            // component destroyed mid-await, expected
        }
    }

    [ContextMenu("Build Level")]
    private void BuildLevelDebug()
    {
        var levelData = _overrideLevelData;
        _stages = _levelStageBuilder.GetStages(levelData);
        
        var lightSettings = _battleSettings.GetLightSettings(levelData.LocationType);
        _light.color = lightSettings.Color;
        _light.intensity = lightSettings.Intensity;
        for (int i = 0; i < _stages.Count; i++)
        {
            switch (_stages[i].StageType)
            {
                case LevelStageType.Start:
                    _stages[i].Setup();
                    break;
                default:
                    _stages[i].Setup(_stages[i - 1]);
                    break;
            }
        }
    }
    private void BuildLevel()
    {
        var (zoneIndex, locationIndex, levelIndex) = _mapManager.GetInfo(_progressData.Level);
        LevelZoneData zoneData = _mapManager.GetZone(zoneIndex);
        zoneData.Setup();
        var locationType = zoneData.GetLocationType(locationIndex);
        var levelType = zoneData.GetLevelType(locationIndex, levelIndex);
        _levelType = levelType;

        var levels = _levelDatasController.GetLevelDataList(_progressData.Level, locationType, levelType);
        var levelData = _useOverrideLevelData? _overrideLevelData: levels.GetRandomItem();
        _stages = _levelStageBuilder.GetStages(levelData);
        
        var lightSettings = _settings.Battle.GetLightSettings(levelData.LocationType);
        _light.color = lightSettings.Color;
        _light.intensity = lightSettings.Intensity;
        
        var _stageDataIndex = 0;
        for (int i = 0; i < _stages.Count; i++)
        {
            switch (_stages[i].StageType)
            {
                case LevelStageType.Start:
                    _stages[i].Setup();
                    break;
                case LevelStageType.Rotate:
                case LevelStageType.End:
                    _stages[i].Setup(_stages[i - 1]);
                    break;
                default:
                    _stages[i].Setup(_stages[i-1], _unitsController, levelData.GetStageData(_stageDataIndex));
                    _stageDataIndex++;
                    break;
            }
        }
    }

    public async void ToNextStage()
    {
        var ct = _flowCts.Token;
        try
        {
            var previewPosition = _playerPositions[_currentStageIndex];
            _realStages[_currentStageIndex].End();
            _player.BattleStop();
            _player.RestoreEnergyShield();
            await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_COMPLETE_PLAYER_DELAY), cancellationToken: ct);
            _dropManager.StartAttraction();
            _currentStageIndex++;
            if (_currentStageIndex >= _stagesCount)
            {
                ShowCompletedScreen();
                return;
            }

            var nextPosition = _playerPositions[_currentStageIndex];
            _realStages[_currentStageIndex].PreStart();
            await UniTask.WaitWhile(() => !_realStages[_currentStageIndex].IsPrestart, cancellationToken: ct);
            _player.MoveToPosition(previewPosition, nextPosition);
            _player.OnDestinationTargetPosition += PlayCurrentStage;
        }
        catch (OperationCanceledException)
        {
            // component destroyed mid-await, expected
        }
    }

    private void PlayCurrentStage()
    {
        PlayCurrentStage(false);
    }

    private async void PlayCurrentStage(bool isRestart)
    {
        if(_currentStageIndex == 0)
        {
            PlayFirstStage();
            return;
        }
        var ct = _flowCts.Token;
        try
        {
            if (!isRestart)
            {
                EventBus.Push(new KeyEvent("StageStart"), EventBus.EventRegion.GAMEPLAY);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_PLAY_PLAYER_DELAY), cancellationToken: ct);
            var stage = _realStages[_currentStageIndex];
            if (stage.BigWeapon != null)
            {
                _player.BattleReady(stage.BigWeapon);
            }
            else
            {
                _player.BattleReady();
            }
            stage.CheckPlay();
            await UniTask.WaitWhile(() => !stage.IsPlay, cancellationToken: ct);
            await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_PLAY_ENEMY_DELAY), cancellationToken: ct);
            stage.Play(OnSpawnWarningHandler);
            await UniTask.WaitWhile(() => !stage.IsCompleted, cancellationToken: ct);
            await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_COMPLETE_DELAY), cancellationToken: ct);
            ToNextStage();
        }
        catch (OperationCanceledException)
        {
            // component destroyed mid-await, expected
        }
    }

    private async void PlayFirstStage()
    {
        var ct = _flowCts.Token;
        try
        {
            _currentStageIndex = 0;
            _player.SetPosition(_playerPositions[_currentStageIndex]);
            _player.BattleStop();
            var stage = _realStages[_currentStageIndex];
            stage.Play(null);
            await UniTask.WaitWhile(() => !stage.IsCompleted, cancellationToken: ct);
            ToNextStage();
        }
        catch (OperationCanceledException)
        {
            // component destroyed mid-await, expected
        }
    }

    private void OnSpawnWarningHandler(Vector3 position)
    {
        OnSpawnWarning?.Invoke(position);
    }

    private async void ShowCompletedScreen()
    {
        var ct = this.GetCancellationTokenOnDestroy();
        try
        {
            _player.Victory();
            _enemyProgression.OnLevelCompleted();
            _dropManager.OnLevelCompleted(_enemyProgression.Location, _enemyProgression.IsLastLevelInLocation);
            ReportLevel("level_complete", ("is_location_complete", _enemyProgression.IsLastLevelInLocation));
            if (_enemyProgression.IsLastLevelInLocation)
            {
                ReportLevel("location_complete");
            }

            OnSpawnWarning = null;
            await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.LEVEL_COMPLETE_PLAYER_DELAY), cancellationToken: ct);
            _uiManager.OpenWindow<GameplayCompetedScreen>();
        }
        catch (OperationCanceledException)
        {
            // component destroyed mid-await, expected
        }
    }

    private void ResetFlow()
    {
        _flowCts?.Cancel();
        _flowCts?.Dispose();
        _flowCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
    }

    // Player died: stage flow and enemies stop, revive window on the first death, defeat otherwise
    private void OnPlayerDead(PlayerDeadEvent deadEvent)
    {
        // level is already completed
        if (_currentStageIndex >= _stagesCount)
            return;

        _deathCount++;
        _flowCts?.Cancel();
        _player.OnDestinationTargetPosition -= PlayCurrentStage;
        _realStages[_currentStageIndex].Pause();
        _unitsController.FreezeAll();
        ReportLevel("player_death", ("can_revive", CanRevive));

        ShowDeathScreenDelayed();
    }

    // let the death animation and camera play before the revive / defeat window
    private async void ShowDeathScreenDelayed()
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.PLAYER_DEATH_SCREEN_DELAY), cancellationToken: this.GetCancellationTokenOnDestroy());
            // without the revive window prefab registered in UIManager go straight to defeat
            if (!CanRevive || _uiManager.OpenWindow<GameplayReviveScreen>() == null)
            {
                Defeat();
            }
        }
        catch (OperationCanceledException)
        {
            // component destroyed mid-await, expected
        }
    }

    // Restarts the current stage: spawned enemies are removed and the stage is played again
    public void Revive()
    {
        if (!_player.IsDead)
            return;

        ResetFlow();
        _realStages[_currentStageIndex].ResetForRestart();
        _player.Revive();
        ReportLevel("level_revive");
        PlayCurrentStage(true);
    }

    public void Defeat()
    {
        _enemyProgression?.OnDefeat();
        ReportLevel("level_fail");
        if (_uiManager.OpenWindow<GameplayDefeatScreen>() == null)
        {
            // no defeat window yet: loot is lost, leave the level
            _dropManager.LocalInventory.Clear();
            EndLevel("level_defeat");
        }
    }

    // interstitialPlacement: interstitial is queued and shown in the lobby; null - none (e.g. right after a rewarded ad)
    public void EndLevel(string interstitialPlacement = null)
    {
        _progressData.AddLevelPlayed();
        if (!string.IsNullOrEmpty(interstitialPlacement))
        {
            Core.Get<AdsManager>()?.QueueInterstitial(interstitialPlacement);
        }
        EventBus.Push(new KeyEvent("ToLobby"), EventBus.EventRegion.GLOBAL);
    }

    private void ReportLevel(string eventName, params (string Key, object Value)[] parameters)
    {
        var all = new List<(string, object)>
        {
            ("level_type", _levelType.ToString()),
            ("stage", _currentStageIndex),
            ("stages", _stagesCount),
            ("deaths", _deathCount),
            ("is_buffed", _enemyProgression?.IsBuffed ?? false),
            ("duration_sec", Mathf.RoundToInt(Time.time - _levelStartTime)),
        };
        all.AddRange(parameters);
        AnalyticsEvent.Push(eventName, all.ToArray());
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<PlayerDeadEvent>(OnPlayerDead, EventBus.EventRegion.GAMEPLAY);
        _flowCts?.Dispose();
        EventBus.Push(new AmbientStopEvent(AmbientType.All), EventBus.EventRegion.GLOBAL);
        _uiManager.CloseAllWindows();
        Core.Unregistry(this);
    }
}
