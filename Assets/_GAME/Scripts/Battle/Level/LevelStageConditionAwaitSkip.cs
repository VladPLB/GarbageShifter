using System;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelStageConditionAwaitSkip : levelStageConditionBase
    {
        public override bool IsNext => _isSkipStage;
    }
}