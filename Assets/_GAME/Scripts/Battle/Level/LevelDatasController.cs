using System.Collections.Generic;
using _GAME.Scripts.Map;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelDatasController: MonoBehaviour
    {
        [SerializeField]
        private List<LevelData> _prepareLevelDatas = new List<LevelData>();
        [SerializeField]
        private List<LevelData> _levelDataListForBuilder = new List<LevelData>();

        public List<LevelData> GetLevelDataList(int level, MapManager.LocationType locationType, MapManager.LevelType levelType)
        {
            var list = new List<LevelData>();
            if(level < _prepareLevelDatas.Count)
            {
                list.Add(_prepareLevelDatas[level]);
                return list;
            }
            
            foreach (LevelData levelData in _levelDataListForBuilder)
            {
                if(levelData.IsCorrect(locationType, levelType))
                    list.Add(levelData);
            }
            return list;
        }
    }
}