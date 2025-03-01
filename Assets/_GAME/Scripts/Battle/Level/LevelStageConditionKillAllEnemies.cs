using System;
using _GAME.Scripts.Battle.Enemy;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelStageConditionKillAllEnemies : levelStageConditionBase
    {
        protected UnitsController _unitsController = null;
        public override bool IsNext => _isSkipStage ||  _unitsController == null || _unitsController.EnemyCount <= 0;

        public override void Setup(LevelStage stage)
        {
            _unitsController = stage.UnitsController;
        }
    }
}