using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelStageBuilder:MonoBehaviour
    {
        [SerializeField] private List<LevelStage> _startStages = new();
        [SerializeField] private List<LevelStage> _rotateStages = new();
        [SerializeField] private List<LevelStage> _endStages = new();

        public List<LevelStage> GetStages(LevelData levelData)
        {
            List<LevelStage> stageList = new List<LevelStage>();
            if (levelData.Start != null)
            {
                stageList.Add(levelData.Start);
            }
            else
            {
                var startStages = new List<LevelStage>();
                foreach (LevelStage stage in _startStages)
                {
                    if(stage.LocationType == levelData.LocationType)
                        startStages.Add(stage);
                }
                
                stageList.Add(_startStages.GetRandomItem());
            }
            
            var rotators = new List<LevelStage>();
            if (levelData.UseRotators)
            {
                foreach (LevelStage stage in _rotateStages)
                {
                    if(stage.LocationType == levelData.LocationType)
                        rotators.Add(stage);
                }
            }
            
            for (int i = 0; i < levelData.StageCount; i++)
            {
                stageList.Add(levelData.GetStageData(i).GetRandomStage());
                if (i < levelData.StageCount - 1 && rotators.Count > 0)
                {
                    if (Random.Range(0, 1000) < 500)
                    {
                        stageList.Add(rotators.GetRandomItem());
                    }
                }
            }
            
            if (levelData.End != null)
            {
                stageList.Add(levelData.End);
            }
            else
            {
                var endStages = new List<LevelStage>();
                foreach (LevelStage stage in _endStages)
                {
                    if(stage.LocationType == levelData.LocationType)
                        endStages.Add(stage);
                }
                
                stageList.Add(endStages.GetRandomItem());
            }

            var outList = new List<LevelStage>();
            for(int i = 0; i < stageList.Count; i++)
            {
                outList.Add(Instantiate(stageList[i], transform).GetComponent<LevelStage>());
            }
            
            return outList;
        }
    }
}