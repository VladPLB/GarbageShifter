using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GAME;
using _GAME.Scripts;
using _GAME.Scripts.Audio;
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
    [Header("Debug")]
    [SerializeField] private bool _useOverrideLevelData = true;
    [SerializeField] private LevelData _overrideLevelData;
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
        Core.Registry(this, typeof(PoolProvider), typeof(UIManager), typeof(DropManager), typeof(InventoryManager));
    }
    
    public void RuntimeSetup()
    {
        _uiManager = Core.Get<UIManager>();
        _mapManager = Core.Get<MapManager>();
        _saveManager = Core.Get<SaveManager>();
        _dropManager = Core.Get<DropManager>();
        _progressData = _saveManager.GetData<ProgressData>();
        
        
        BuildLevel();
        Setup(FindObjectOfType<Player>());
    }

    public async void Setup(Player player)
    {
        _player = player;
        _player.Setup(_cameraController);
        _unitsController.Setup(_player.transform);

        _realStages = _stages.Where(stage => stage.PlayerPosition != null).ToList();
        _stagesCount = _realStages.Count;
        _playerPositions = _realStages.Select(stage => stage.PlayerPosition).ToList();

        _pathfinder = new LevelPathFinder();
        await UniTask.DelayFrame(1);
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

    [ContextMenu("Build Level")]
    private void BuildLevelDebug()
    {
        var levelData = _overrideLevelData;
        _stages = _levelStageBuilder.GetStages(levelData);
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
        var previewPosition = _playerPositions[_currentStageIndex];
        _realStages[_currentStageIndex].End();
        _player.BattleStop();
        await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_COMPLETE_PLAYER_DELAY));
        _dropManager.StartAttraction();
        _currentStageIndex++;
        if (_currentStageIndex >= _stagesCount)
        {
            EndLevel();
            return;
        }
        
        var nextPosition = _playerPositions[_currentStageIndex];
        _realStages[_currentStageIndex].PreStart();
        await UniTask.WaitWhile(() => !_realStages[_currentStageIndex].IsPrestart);
        _player.MoveToPosition(previewPosition, nextPosition);
        _player.OnDestinationTargetPosition += PlayCurrentStage;
    }

    private async void PlayCurrentStage()
    {
        if(_currentStageIndex == 0)
        {
            PlayFirstStage();
            return;
        }
        EventBus.Push(new KeyEvent("StageStart"), EventBus.EventRegion.GAMEPLAY);
        await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_PLAY_PLAYER_DELAY));
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
        await UniTask.WaitWhile(() => !stage.IsPlay);
        await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_PLAY_ENEMY_DELAY));
        stage.Play(OnSpawnWarningHandler);
        await UniTask.WaitWhile(() => !stage.IsCompleted);
        await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_COMPLETE_DELAY));
        ToNextStage();
    }
    
    private async void PlayFirstStage()
    {
        _currentStageIndex = 0;
        _player.SetPosition(_playerPositions[_currentStageIndex]);
        _player.BattleStop();
        var stage = _realStages[_currentStageIndex];
        stage.Play(null);
        await UniTask.WaitWhile(() => !stage.IsCompleted);
        ToNextStage();
    }
    
    private void OnSpawnWarningHandler(Vector3 position)
    {
        OnSpawnWarning?.Invoke(position);
    }

    public void EndLevel()
    {
        EventBus.Push(new KeyEvent("ToLobby"), EventBus.EventRegion.GLOBAL);
        _player.Victory();
        OnSpawnWarning = null;
    }

    private void OnDestroy()
    {
        EventBus.Push(new AmbientStopEvent(AmbientType.All), EventBus.EventRegion.GLOBAL);
        _uiManager.CloseAllWindows();
        Core.Unregistry(this);
    }
}
