using System;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Battle.Player;
using _GAME.Scripts.Common;
using _GAME.Scripts.Events;
using _GAME.Scripts.Map;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelStage: MonoBehaviour
    {
        [SerializeField] private MapManager.LocationType _locationType;
        [SerializeField] private LevelStageType _stageType;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private Transform _stageInConnector;
        [SerializeField] private Transform _stageOutConnector;
        [SerializeField] private PlayerPosition _playerPosition;
        [SerializeField] private BigWeapon _bigWeapon;
        [SerializeField] private levelStageConditionBase _prestartCondition;
        [SerializeField] private levelStageConditionBase _playCondition;
        [SerializeField] private levelStageConditionBase _completeCondition;
        [SerializeField] private UnityEvent _prestartLevelEvent;
        [SerializeField] private UnityEvent _startLevelEvent;
        [SerializeField] private UnityEvent _endLevelEvent;
        
        private UnitsController _unitsController = null;
        
        public MapManager.LocationType LocationType => _locationType;
        public LevelStageType StageType => _stageType;
        public Transform OutConnector => _stageOutConnector;
        public PlayerPosition PlayerPosition => _playerPosition;
        public UnitsController UnitsController => _unitsController;

        public BigWeapon BigWeapon => _bigWeapon;
        
        public bool IsPrestart => _prestartCondition == null || _prestartCondition.IsNext;
        public bool IsPlay => _playCondition == null || _playCondition.IsNext;
        public bool IsCompleted => _completeCondition ==null || _completeCondition.IsNext;

        private Action<Vector3> _onSpawnWarning;

        public void Setup()
        {
            Setup(null);
        }
        
        public void Setup(LevelStage previewStage)
        {
            SetupPosition(previewStage);
            SetupPlayerPosition();
        }
        public void Setup(LevelStage previewStage, UnitsController unitsController, EnemyStagePreset enemyStagePreset)
        {
            Setup(previewStage);
            _unitsController = unitsController;
            _enemySpawner?.Setup(unitsController, enemyStagePreset);
        }
        
        private void SetupPosition(LevelStage previewStage)
        {
            var connector = previewStage? previewStage.OutConnector: null;
            transform.forward = connector? connector.forward: Vector3.forward;
            Vector3 offset = _stageInConnector.position - transform.position;
            transform.position = (connector? connector.position: Vector3.zero) - offset;

        }
        
        private void SetupPlayerPosition()
        {
            var playerPositionType = _stageType switch
            {
                LevelStageType.Start => PlayerPositionType.Start,
                LevelStageType.End => PlayerPositionType.End,
                LevelStageType.Normal => PlayerPositionType.Default,
                _ => PlayerPositionType.Default
            };
            PlayerPosition?.Setup(playerPositionType);
        }

        public void PreStart()
        {
            _prestartCondition?.Setup(this);
            _prestartLevelEvent?.Invoke();
        }

        public void CheckPlay()
        {
            _playCondition?.Setup(this);
        }

        public void Play(Action<Vector3> onSpawnWarning)
        {
            _onSpawnWarning = onSpawnWarning;
            _enemySpawner?.Play(OnSpawnWarningHandler);
            _completeCondition?.Setup(this);
            _startLevelEvent?.Invoke();
            EventBus.Push(new KeyEvent("StagePlay"), EventBus.EventRegion.GAMEPLAY);
        }
        
        public void End()
        {
            _onSpawnWarning = null;
            _enemySpawner?.Stop();
            _endLevelEvent?.Invoke();
            EventBus.Push(new KeyEvent("StageComplete"), EventBus.EventRegion.GAMEPLAY);
        }

        private void OnSpawnWarningHandler(Vector3 position)
        {
            _onSpawnWarning?.Invoke(position);
        }
    }
}