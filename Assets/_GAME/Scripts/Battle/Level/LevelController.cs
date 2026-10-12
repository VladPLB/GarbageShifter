using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GAME;
using _GAME.Scripts;
using _GAME.Scripts.Audio;
using _GAME.Scripts.Battle.Context;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Items;
using _GAME.Scripts.Battle.Level;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Common;
using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Events;
using _GAME.Scripts.Inventory;
using _GAME.Scripts.Map;
using _GAME.Scripts.Save;
using _GAME.Scripts.Tutorial;
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
    
    
    private Player _player;
    private List<LevelStage> _stages;
    private List<LevelStage> _realStages;
    private int _stagesCount = 0;
    private List<PlayerPosition> _playerPositions;
    private LevelPathFinder _pathfinder;
    private int _currentStageIndex = 0;

    public Player Player => _player;
    public UnitsController UnitsController => _unitsController;

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
            _unitsController.Setup(_player.transform);

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
            PlayCurrentStage();
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
        var ct = this.GetCancellationTokenOnDestroy();
        try
        {
            var previewPosition = _playerPositions[_currentStageIndex];
            _realStages[_currentStageIndex].End();
            _player.BattleStop();
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

    private async void PlayCurrentStage()
    {
        if(_currentStageIndex == 0)
        {
            PlayFirstStage();
            return;
        }
        var ct = this.GetCancellationTokenOnDestroy();
        try
        {
            EventBus.Push(new KeyEvent("StageStart"), EventBus.EventRegion.GAMEPLAY);
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
        var ct = this.GetCancellationTokenOnDestroy();
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

            OnSpawnWarning = null;
            await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.LEVEL_COMPLETE_PLAYER_DELAY), cancellationToken: ct);
            _uiManager.OpenWindow<GameplayCompetedScreen>();
        }
        catch (OperationCanceledException)
        {
            // component destroyed mid-await, expected
        }
    }

    public void EndLevel()
    {
        EventBus.Push(new KeyEvent("ToLobby"), EventBus.EventRegion.GLOBAL);
    }

    private void OnDestroy()
    {
        EventBus.Push(new AmbientStopEvent(AmbientType.All), EventBus.EventRegion.GLOBAL);
        _uiManager.CloseAllWindows();
        Core.Unregistry(this);
    }
}
