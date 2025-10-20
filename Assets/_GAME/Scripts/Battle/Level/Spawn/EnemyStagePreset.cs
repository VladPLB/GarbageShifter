using System.Collections.Generic;
using _GAME.Scripts.Map;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    [CreateAssetMenu(fileName = "EnemyStageData", menuName = "Scriptable/Level/EnemyStagePreset", order = 0)]
    public class EnemyStagePreset : ScriptableObject
    {
        [SerializeField] private List<LevelStage> _stages = new();
        [SerializeField] private List<EnemySpawnDataList> _enemies;

        public LevelStage GetRandomStage()
        {
            return _stages.GetRandomItem();
        }
        
        public (List<EnemySpawnData>, List<float>) GetEnemiesSpawnData()
        {
            List<EnemySpawnData> outList = new();
            List<float> spawnDelay = new();
            for (int i = 0; i < _enemies.Count; i++)
            {
                outList.Add(_enemies[i].Get());
                spawnDelay.Add(_enemies[i].SpawnDelayGroup);
            }

            return (outList, spawnDelay);
        }
    }
}