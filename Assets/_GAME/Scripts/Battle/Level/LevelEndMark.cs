using _GAME.Scripts.Cores.Save.SavesConfigs;
using _GAME.Scripts.Save;
using UnityEngine;

namespace _GAME.Scripts.Battle.Level
{
    public class LevelEndMark:MonoBehaviour
    {
        private SaveManager _saveManager;
        private ProgressData _progressData;
        
        public void EndLevel()
        {
            _saveManager = Core.Get<SaveManager>();
            _progressData = _saveManager.GetData<ProgressData>();
            
            _progressData.NextLevel();
        }
    }
}