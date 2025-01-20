using System;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Common;
using UnityEngine;
using UnityEngine.Events;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelStage: MonoBehaviour
    {
        [SerializeField] private LevelStageType _stageType;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private Transform _stageInConnector;
        [SerializeField] private Transform _stageOutConnector;
        [SerializeField] private PlayerPosition _playerPosition;
        [SerializeField] private UnityEvent _startLevelEvent;
        

        public LevelStageType StageType => _stageType;
        public Transform OutConnector => _stageOutConnector;
        public PlayerPosition PlayerPosition => _playerPosition;

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
            _enemySpawner?.Setup(unitsController, enemyStagePreset);
        }
        
        private void SetupPosition(LevelStage previewStage)
        {
            var connector = previewStage? previewStage.OutConnector: null;
            Vector3 offset = _stageInConnector.localPosition * -1f;
            transform.forward = connector? connector.forward: Vector3.forward;
            transform.position =(connector? connector.position: Vector3.zero) + offset;
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

        public void Play(Action<Vector3> onSpawnWarning)
        {
            _onSpawnWarning = onSpawnWarning;
            _enemySpawner?.Play(OnSpawnWarningHandler);
            _startLevelEvent?.Invoke();
        }
        
        public void End()
        {
            _onSpawnWarning = null;
            _enemySpawner?.Stop();
        }

        private void OnSpawnWarningHandler(Vector3 position)
        {
            _onSpawnWarning?.Invoke(position);
        }
    }
}