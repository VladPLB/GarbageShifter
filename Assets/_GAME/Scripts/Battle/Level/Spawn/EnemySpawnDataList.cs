using System;
using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    [Serializable]
    public class EnemySpawnDataList
    {
        [SerializeField] private List<EnemySpawnData> _datas;
        public float SpawnDelayGroup = 0f;
        public EnemySpawnData Get() => _datas.GetRandomItem();
    }
}