using System;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    [Serializable]
    public class EnemySpawnData
    {
        public EnemyType Type;
        public Vector2Int SpawnRangeAmount;
        public float SpawnDelayBetwenUnits = 0f;
    }
}