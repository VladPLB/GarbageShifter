using System;
using System.Collections.Generic;
using System.Linq;
using _GAME.Scripts.Common;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    [Serializable]
    public class EnemySpawnPointsGroup
    {
        [SerializeField] private List<EnemySpawnPoint> _spawnPoints;

        public bool ContainsSpawnerByTypes(List<EnemyClassType> _types)
        {
            for (int i = 0; i < _types.Count; i++)
            {
                if (!_spawnPoints.Any(p => p.IsTypeContains(_types[i])))
                    return false;
            }

            return true;
        }

        public EnemySpawnPoint GetSpawnPoint(EnemyClassType classType)
        {
            List<EnemySpawnPoint> targetSpawnPoints = new();
            foreach (var spawnPoint in _spawnPoints)
            {
                if(spawnPoint.IsTypeContains(classType))
                    targetSpawnPoints.Add(spawnPoint);
            }
            
            return targetSpawnPoints.GetRandomItem();
        }
    }
}