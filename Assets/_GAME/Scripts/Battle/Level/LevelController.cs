using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Level;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LevelController : MonoBehaviour, IRuntimeSetup
{
    [Header("Debug")]
    [SerializeField] private bool _useOverrideLevelData = true;
    [SerializeField] private LevelData _overrideLevelData;
    [Header("General")]
    [SerializeField] private List<LevelStage> _stages;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private UnitsController _unitsController;
    private Player _player;
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
        Core.Registry(this, typeof(PoolProvider));
    }
    
    public void RuntimeSetup()
    {
        Setup(FindObjectOfType<Player>());
    }

    public async void Setup(Player player)
    {
        _player = player;
        _player.Setup(_cameraController);
        
        _unitsController.Setup(_player.transform);
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
                    _stages[i].Setup(_stages[i-1], _unitsController, _overrideLevelData.GetStageData(_stageDataIndex));
                    _stageDataIndex++;
                    break;
            }
        }

        _realStages = _stages.Where(stage => stage.PlayerPosition != null).ToList();
        _stagesCount = _realStages.Count;
        _playerPositions = _realStages.Select(stage => stage.PlayerPosition).ToList();

        _pathfinder = new LevelPathFinder();
        await UniTask.DelayFrame(1);
        _pathfinder.Rebuild();

        PlayCurrentStage();
    }

    public void ToNextStage()
    {
        var previewPosition = _playerPositions[_currentStageIndex];
        _realStages[_currentStageIndex].End();
        _player.BattleStop();
        
        _currentStageIndex++;
        if (_currentStageIndex >= _stagesCount)
        {
            EndLevel();
            return;
        }
        
        var nextPosition = _playerPositions[_currentStageIndex];
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
        
        await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_PLAY_PLAYER_DELAY));
        _player.BattleReady();
        await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_PLAY_ENEMY_DELAY));
        _realStages[_currentStageIndex].Play(OnSpawnWarningHandler);
    }
    
    private async void PlayFirstStage()
    {
        _currentStageIndex = 0;
        _player.SetPosition(_playerPositions[_currentStageIndex]);
        _realStages[_currentStageIndex].Play(OnSpawnWarningHandler);
        await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.STAGE_FIRST_DURATION));
        ToNextStage();
    }
    
    private void OnSpawnWarningHandler(Vector3 position)
    {
        OnSpawnWarning?.Invoke(position);
    }

    public void EndLevel()
    {
        _player.Victory();
        OnSpawnWarning = null;
    }
    
    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                ToNextStage();
            }
        }
    }
}
