using System.Collections.Generic;
using _GAME.Scripts.Map;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable/Level/LevelData", order = 0)]
    public class LevelData : ScriptableObject
    {
        [SerializeField] private LevelStage _start = null;
        [SerializeField] private LevelStage _end = null;
        [SerializeField] private MapManager.LocationType _locationType;
        [SerializeField] private MapManager.LevelType _levelTypes;
        [SerializeField] private List<EnemyStagePreset> _stages;
        [SerializeField] private bool _useRotators = true;

        public LevelStage Start => _start;
        public LevelStage End => _end;
        public int StageCount => _stages.Count;
        
        public MapManager.LocationType LocationType => _locationType;
        public MapManager.LevelType LevelType => _levelTypes;
        public bool UseRotators => _useRotators;
        
        public bool IsCorrect(MapManager.LocationType locationType, MapManager.LevelType levelType)
        {
            return _locationType == locationType && _levelTypes ==  levelType;
        }

        public EnemyStagePreset GetStageData(int index)
        {
            index = Mathf.Clamp(index, 0, _stages.Count - 1);
            return _stages[index];
        }
    }
}