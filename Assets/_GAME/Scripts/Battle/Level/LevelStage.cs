using System;
using _GAME.Scripts.Battle.Enemy;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelStage: MonoBehaviour
    {
        [SerializeField] private LevelStageType _stageType;
        [SerializeField] private EnemySpawner _enemySpawner;
        [SerializeField] private Transform _stageInConnector;
        [SerializeField] private Transform _stageOutConnector;
        [SerializeField] private PlayerPosition _playerPosition;

        public LevelStageType StageType => _stageType;
        public Transform OutConnector => _stageOutConnector;
        public PlayerPosition PlayerPosition => _playerPosition;

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

        public void Play()
        {
            _enemySpawner?.Play();
        }
        
        public void End()
        {
            _enemySpawner?.Stop();
        }
    }
}