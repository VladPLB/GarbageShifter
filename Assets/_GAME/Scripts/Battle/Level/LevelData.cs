using System.Collections.Generic;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable/Level/LevelData", order = 0)]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private List<EnemyStagePreset> _stages;

        public EnemyStagePreset GetStageData(int index)
        {
            index = Mathf.Clamp(index, 0, _stages.Count - 1);
            return _stages[index];
        }
    }
}